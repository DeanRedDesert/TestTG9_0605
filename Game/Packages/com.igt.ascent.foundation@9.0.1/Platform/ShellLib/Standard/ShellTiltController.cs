// -----------------------------------------------------------------------
// <copyright file = "ShellTiltController.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Game.Core.Communication.Foundation;
    using Game.Core.Communication.Foundation.Standard;
    using Game.Core.Tilts;
    using IGT.Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;
    using Platform.Interfaces.TiltControl;

    /// <summary>
    /// A class that allows for <see cref="ITilt"/> to be posted and cleared, while maintaining the Foundation
    /// driven tilt requirements.
    /// </summary>
    internal class ShellTiltController : TiltController, IShellTiltController
    {
        #region Private Fields

        /// <summary>
        /// The maximum length allowed for a tilt key.
        /// </summary>
        /// <remarks>
        /// We restrict the key to 39 as foundation allows 76 characters for a tilt key.
        /// 2 (coplayer id length) + 1 ("_") + 73 (tilt key characters).
        /// </remarks>
        private const int MaxKeyLength = 73;

        /// <summary>
        /// The delimiter used for merging and splitting tilt keys.
        /// </summary>
        private static readonly char[] TiltKeyDelimiter = { '_' };

        /// <summary>
        /// Id used for posting tilts on behalf of the shell.
        /// </summary>
        private const int ShellId = -1;

        /// <summary>
        /// Lock used to protect TiltInfoList resource.
        /// </summary>
        private readonly ReaderWriterLockSlim tiltInfoLock = new ReaderWriterLockSlim();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an instance of <see cref="ShellTiltController"/>.
        /// </summary>
        /// <param name="transactionVerifier">
        /// The interface used to verify a transaction exists for the functions that require one.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// The interface for dispatching incoming events.
        /// </param>
        public ShellTiltController(ITransactionVerification transactionVerifier,
            IEventDispatcher transactionalEventDispatcher) : base(transactionVerifier, transactionalEventDispatcher)
        {
        }

        #endregion

        #region IShellTiltController Implementation

        public event EventHandler<ShellTiltClearedByAttendantEventArgs> ShellTiltClearedByAttendantEvent;

        /// <inheritdoc cref="IShellTiltController"/>
        public bool PostTilt(ITilt tilt, string key, IEnumerable<object> titleFormat, 
            IEnumerable<object> messageFormat, int? coplayerId = null)
        {
            if(coplayerId.HasValue && coplayerId.Value < 0)
            {
                throw new ArgumentException("The Coplayer Id cannot be negative.");
            }

            if(string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("A tilt key must be provided", nameof(key));
            }

            if(key.Length > MaxKeyLength)
            {
                throw new ArgumentException($"The key: {key} cannot be used to track this tilt because it exceeds {MaxKeyLength} characters.");
            }

            if(tilt == null)
            {
                throw new ArgumentNullException(nameof(tilt));
            }

            if(tilt.DiscardBehavior == TiltDiscardBehavior.Never)
            {
                throw new ArgumentException("Power hit tolerant tilts are not yet supported, " + 
                                            "please use TiltDiscardBehavior.OnGameTermination");
            }

            var id = coplayerId ?? ShellId;
            tiltInfoLock.EnterWriteLock();
            try
            {
                return base.PostTilt(tilt, MergeKey(id, key), titleFormat, messageFormat);
            }
            finally
            {
                tiltInfoLock.ExitWriteLock();
            }
        }

        /// <inheritdoc cref="IShellTiltController"/>
        public bool ClearTilt(string key, int? coplayerId = null)
        {
            var id = coplayerId ?? ShellId;
            tiltInfoLock.EnterWriteLock();
            try
            { 
                return base.ClearTilt(MergeKey(id, key));
            }
            finally
            {
                tiltInfoLock.ExitWriteLock();
            }
        }

        /// <inheritdoc cref="IShellTiltController"/>
        public bool ClearAllTilts(int? coplayerId = null)
        {
            var tiltsCleared = false;
            var id = coplayerId ?? ShellId;
            tiltInfoLock.EnterWriteLock();
            try
            {
                foreach(var coplayerTilt in 
                    TiltInfoList.Where(tilt => tilt.Key.StartsWith(id.ToString() + TiltKeyDelimiter[0])))
                {
                    tiltsCleared |= base.ClearTilt(coplayerTilt.Key);
                }

                return tiltsCleared;
            }
            finally
            {
                tiltInfoLock.ExitWriteLock();
            }
        }

        /// <inheritdoc cref="IShellTiltController"/>
        public bool TiltPresent(string key, int? coplayerId = null)
        {
            var id = coplayerId ?? ShellId;
            tiltInfoLock.EnterReadLock();
            try
            {
                return base.TiltPresent(MergeKey(id, key));
            }
            finally
            {
                tiltInfoLock.ExitReadLock();
            }
        }

        /// <inheritdoc cref="IShellTiltController"/>
        public bool IsTilted(int? coplayerId = null)
        {
            var id = coplayerId ?? ShellId;
            tiltInfoLock.EnterReadLock();
            try
            {
                return TiltInfoList.Exists(tilt => SplitKey(tilt.Key).CoplayerId == id);
            }
            finally
            {
                tiltInfoLock.ExitReadLock();
            }
        }

        /// <inheritdoc cref="IShellTiltController"/>
        public bool IsAnyTilted()
        {
            tiltInfoLock.EnterReadLock();
            try
            {
                return base.IsTilted();
            }
            finally
            {
                tiltInfoLock.ExitReadLock();
            }
        }

        #endregion

        #region Event Handlers

        /// <inheritdoc/>
        protected override void HandleTiltClearedByAttendantEvent(object sender,
            EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEvent is TiltClearedByAttendantEventArgs eventArgs)
            {
                tiltInfoLock.EnterWriteLock();
                try
                {
                    // Clear this tilt but do not notify the foundation.
                    ClearTilt(eventArgs.TiltName, false);
                }
                finally
                {
                    tiltInfoLock.ExitWriteLock();
                }

                var handler = ShellTiltClearedByAttendantEvent;
                if(handler != null)
                {
                    // This returns two strings, the coplayer id and the rest of the string.
                    var (id, key) = SplitKey(eventArgs.TiltName);

                    handler(this, id == ShellId ? new ShellTiltClearedByAttendantEventArgs(key)
                        : new ShellTiltClearedByAttendantEventArgs(key, id));
                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Concatenates the shell or coplayer identifier with the tilt key name.
        /// </summary>
        /// <param name="coplayerId">The shell or coplayer's identifier.</param>
        /// <param name="key">The tilt key.</param>
        /// <returns>Concatenated string.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown in key is null.
        /// </exception>
        private static string MergeKey(int coplayerId, string key)
        {
            return $"{coplayerId}{TiltKeyDelimiter[0]}{key ?? throw new ArgumentException("Key cannot be null")}";
        }

        /// <summary>
        /// Split the passed key into two strings consisting of an shell or coplayer identifier and the tilt name.
        /// It is the opposite of MergeKey.
        /// </summary>
        /// <param name="mergedKey">The tilt key.</param>
        /// <returns>Returns a tuple containing a coplayer id and the key.</returns>
        private static (int CoplayerId, string Key) SplitKey(string mergedKey)
        {
            var splitKey = mergedKey.Split(TiltKeyDelimiter, 2);
            return (Convert.ToInt32(splitKey[0]), splitKey[1]);
        }

        #endregion
    }
}