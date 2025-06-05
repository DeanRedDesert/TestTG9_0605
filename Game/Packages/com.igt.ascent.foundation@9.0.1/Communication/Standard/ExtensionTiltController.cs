// -----------------------------------------------------------------------
// <copyright file = "ExtensionTiltController.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces.TiltControl;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.Interfaces.TiltControl;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Tilts;

    /// <summary>
    /// A class that allows for <see cref="IActiveTilt"/> to be posted and cleared by extensions, while maintaining the Foundation
    /// driven tilt requirements.
    /// </summary>
    internal sealed class ExtensionTiltController : TiltController, IExtensionTiltController
    {
        #region Private Fields

        /// <summary>
        /// The maximum length allowed for a tilt key.
        /// </summary>
        /// <remarks>
        /// We restrict the key to 39 as foundation allows 76 characters for a tilt key.
        /// 39 (tilt key characters) + 1 ("_") + 36 (guid length).
        /// </remarks>
        private const int MaxKeyLength = 39;

        /// <summary>
        /// Keeps a track of extensions and the number of tilts each extension has requested and pending.
        /// </summary>
        private readonly Dictionary<Guid, int> extensionTiltCount = new Dictionary<Guid, int>();

        /// <summary>
        /// The delimiter used for merging and splitting tilt keys.
        /// </summary>
        private static readonly char[] TiltKeyDelimiter = { '_' };


        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Constructs an instance of <see cref="ExtensionTiltController"/>.
        /// </summary>
        /// <param name="transactionVerifier">
        /// The interface used to verify a transaction exists for the functions that require one.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// The interface for dispatching incoming events.  
        /// </param>
        public ExtensionTiltController(ITransactionVerification transactionVerifier,
            IEventDispatcher transactionalEventDispatcher) : base(transactionVerifier, transactionalEventDispatcher)
        {
        }

        #endregion Constructors

        #region Event Handlers

        /// <inheritdoc/>
        protected override void HandleTiltClearedByAttendantEvent(object sender,
            EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEvent is TiltClearedByAttendantEventArgs eventArgs)
            {
                // Clear this tilt but do not notify the foundation.
                ClearTilt(eventArgs.TiltName, false);

                var handler = ExtensionTiltClearedByAttendantEvent;
                if(handler != null)
                {
                    // This returns two strings, the guid and the rest of the string.
                    var pair = SplitKey(eventArgs.TiltName);

                    var extensionGuid = new Guid(pair.First());
                    var key = pair.Last();

                    extensionTiltCount[extensionGuid]--;
                    if(extensionTiltCount[extensionGuid] <= 0)
                    {
                        extensionTiltCount.Remove(extensionGuid);
                    }

                    handler(this, new ExtensionTiltClearedByAttendantEventArgs(extensionGuid, key));
                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        #endregion

        #region IExtensionTiltController

        /// <inheritdoc/>
        public bool PostTilt(IActiveTilt tilt, Guid extensionGuid, string key, IEnumerable<object> titleFormat,
            IEnumerable<object> messageFormat)
        {
            if(extensionGuid == Guid.Empty)
            {
                throw new ArgumentException("The extension guid cannot be empty.");
            }

            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("A tilt key must be provided", nameof(key));
            }

            if(key.Length > MaxKeyLength)
            {
                throw new ArgumentException($"The key that will be used to track this tilt cannot exceed {MaxKeyLength} characters.");
            }

            if(PostTilt(tilt, MergeKey(extensionGuid, key), titleFormat, messageFormat))
            {
                // We only want to add this if a tilt successfully posted.
                if(!extensionTiltCount.ContainsKey(extensionGuid))
                {
                    extensionTiltCount.Add(extensionGuid, 0);
                }
                extensionTiltCount[extensionGuid]++;

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool ClearTilt(Guid extensionGuid, string key)
        {
            // If this extension exists then update the dictionary.  
            if(extensionTiltCount.ContainsKey(extensionGuid))
            {
                extensionTiltCount[extensionGuid]--;

                if(extensionTiltCount[extensionGuid] <= 0)
                {
                    extensionTiltCount.Remove(extensionGuid);
                }
            }

            return ClearTilt(MergeKey(extensionGuid, key));
        }

        /// <inheritdoc/>
        public bool TiltPresent(Guid extensionGuid, string key)
        {
            return TiltPresent(MergeKey(extensionGuid, key));
        }

        /// <inheritdoc/>
        public bool IsTilted(Guid extensionGuid)
        {
            return extensionTiltCount.ContainsKey(extensionGuid);
        }

        /// <inheritdoc />
        public event EventHandler<ExtensionTiltClearedByAttendantEventArgs> ExtensionTiltClearedByAttendantEvent;

        #endregion

        #region Private Methods

        /// <summary>
        /// Concatenates the extension's guid with the tilt key name.
        /// </summary>
        /// <param name="extensionGuid">The extension's guid.</param>
        /// <param name="key">The tilt key.</param>
        /// <returns>Concatenated string.</returns>
        private static string MergeKey(Guid extensionGuid, string key)
        {
            return $"{extensionGuid}{TiltKeyDelimiter[0]}{key ?? ""}";
        }

        /// <summary>
        /// Split the passed key into two strings consisting of an extension guid and the tilt name.  
        /// It is the opposite of MergeKey.
        /// </summary>
        /// <param name="key">The tilt key.</param>
        /// <returns>Returns an array of two strings: the guid and the rest of the string.</returns>
        private static string[] SplitKey(string key)
        {
            return key.Split(TiltKeyDelimiter, 2);
        }

        #endregion
    }
}