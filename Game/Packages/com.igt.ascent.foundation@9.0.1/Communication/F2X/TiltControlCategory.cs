//-----------------------------------------------------------------------
// <copyright file = "TiltControlCategory.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------
// This file requires manual changes when merging.
// All changes are marked with "MANUAL EDIT: "
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2XTransport;
    using Schemas.Internal.TiltControl;
    using Version = Schemas.Internal.Types.Version;

    /// <summary>
    /// Implementation of the F2X <see cref="TiltControl"/> category.
    /// TiltControl category of messages.
    /// Category: 126; Major Version: 1
    /// </summary>
    public class TiltControlCategory : F2XTransactionalCategoryBase<TiltControl>, ITiltControlCategory, IMultiVersionSupport
    {
        #region Fields

        /// <summary>
        /// Object which implements the TiltControlCategory callbacks.
        /// </summary>
        private readonly ITiltControlCategoryCallbacks callbackHandler;

        /// <summary>
        /// All versions supported by this category class.
        /// </summary>
        private readonly List<Version> supportedVersions = new List<Version>
        {
            new Version(1, 0),
            new Version(1, 1),
            new Version(1, 2),
            new Version(1, 3)
        };

        /// <summary>
        /// The version to use for communications by this category.
        /// Initialized to 0.0. Will be set by <see cref="SetVersion"/>.
        /// </summary>
        private Version effectiveVersion = new Version(0, 0);

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="TiltControlCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// TiltControlCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public TiltControlCategory(IF2XTransport transport, ITiltControlCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<TiltClearedByAttendantSend>(HandleTiltClearedByAttendant);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.TiltControl;

        /// <inheritdoc/>
        public override uint MajorVersion => effectiveVersion.MajorVersion;

        /// <inheritdoc/>
        public override uint MinorVersion => effectiveVersion.MinorVersion;

        #endregion

        #region IMultiVersionSupport Members

        /// <inheritdoc/>
        public void SetVersion(uint major, uint minor)
        {
            var version = new Version(major, minor);

            if(!supportedVersions.Contains(version))
            {
                throw new ArgumentException(
                    $"{version} is not supported by TiltControlCategory class.");
            }

            effectiveVersion = version;
        }

        #endregion

        #region ITiltControlCategory Members

        /// <inheritdoc/>
        public bool RequestClearTilt(string tiltName)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<RequestClearTiltSend>();
            var content = (RequestClearTiltSend)request.Message.Item;
            content.TiltName = tiltName;

            var reply = SendMessageAndGetReply<RequestClearTiltReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.RequestSuccess;
        }

        /// <inheritdoc/>
        public bool RequestTilt(string tiltName, IEnumerable<TiltLocalization> tiltLocalizations, IEnumerable<TiltAttribute> tiltAttributes)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<RequestTiltSend>();
            var content = (RequestTiltSend)request.Message.Item;
            content.TiltName = tiltName;
            content.TiltLocalizations = tiltLocalizations == null ? null : tiltLocalizations.ToList();
            content.TiltAttributes = tiltAttributes == null ? null : tiltAttributes.ToList();
            // MANUAL EDIT: Throw the exception when the TiltAttribute DelayPreventGamePlayUntilNoGameInProgress is not
            //              supported by current category version.
            if(content.TiltAttributes?.Contains(TiltAttribute.DelayPreventGamePlayUntilNoGameInProgress) == true
               && effectiveVersion < new Version(1, 3))
            {
                throw new InvalidOperationException(
                    "TiltAttribute DelayPreventGamePlayUntilNoGameInProgress is NOT supported on the TiltControl category version 1.2 or older.");
            }

            var reply = SendMessageAndGetReply<RequestTiltReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.RequestSuccess;
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the TiltClearedByAttendantSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleTiltClearedByAttendant(TiltClearedByAttendantSend message)
        {
            var errorMessage = callbackHandler.ProcessTiltClearedByAttendant(message.TiltName);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<TiltClearedByAttendantReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        #endregion

    }

}

