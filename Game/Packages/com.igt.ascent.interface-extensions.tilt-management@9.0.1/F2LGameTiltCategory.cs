//-----------------------------------------------------------------------
// <copyright file = "F2LGameTiltCategory.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.TiltManagement
{
    using System;
    using System.Collections.Generic;
    using F2L;
    using F2L.Schemas;
    using F2L.Schemas.Internal;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2L game tilt API category.
    /// </summary>
    public class F2LGameTiltCategory : F2LTransactionalCategoryBase<GameTilt>, IGameTiltCategory
    {
        #region Constructor and Initialization

        /// <summary>
        /// Create an instance of the game tilt category.
        /// </summary>
        /// <param name="transport">Transport the category handler will be installed in.</param>
        public F2LGameTiltCategory(IF2XTransport transport)
            : base(transport)
        {
        }

        #endregion

        #region IApiCategory Overrides

        /// <inheritdoc />
        public override uint MajorVersion
        {
            get { return 1; }
        }

        /// <inheritdoc />
        public override uint MinorVersion
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public override MessageCategory Category
        {
            get { return MessageCategory.GameTilt; }
        }

        #endregion

        #region IGameTiltCategory Implementation

        /// <inheritdoc/>
        public bool PostTilt(string tiltName, bool hardTilt, bool notifyProtocols, bool progressiveLinkDown,
                             ICollection<TiltLocalization> tiltLocalizations)
        {
            if(string.IsNullOrEmpty(tiltName))
            {
                throw new ArgumentException("A tilt name must be provided", "tiltName");
            }

            if(tiltLocalizations == null || tiltLocalizations.Count == 0)
            {
                throw new ArgumentException("At least one localization must be provided", "tiltLocalizations");
            }

            var request = CreateTransactionalRequest<GameTiltRequestTiltSend>();
            var content = (GameTiltRequestTiltSend)request.Message.Item;
            content.TiltName = tiltName;

            //Convert attributes.
            var attributesList = new List<string>();
            if(hardTilt)
            {
                attributesList.Add(GameTiltAttribute.PreventGamePlay.ToString());
            }
            if(notifyProtocols)
            {
                attributesList.Add(GameTiltAttribute.NotifyProtocols.ToString());
            }
            if(progressiveLinkDown)
            {
                attributesList.Add(GameTiltAttribute.ProgressiveLinkDown.ToString());
            }
            var attributesString = String.Join(" ", attributesList.ToArray());
            content.TiltAttributes = attributesString;

            content.TiltLocalizations.AddRange(tiltLocalizations);

            var reply = SendMessageAndGetReply<GameTiltRequestTiltReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.RequestTiltSuccess;
        }

        /// <inheritdoc/>
        public bool ClearTilt(string tiltName)
        {
            if(string.IsNullOrEmpty(tiltName))
            {
                throw new ArgumentException("A tilt name must be provided", "tiltName");
            }

            var request = CreateTransactionalRequest<GameTiltRequestClearTiltSend>();
            var content = (GameTiltRequestClearTiltSend)request.Message.Item;
            content.TiltName = tiltName;
            var reply = SendMessageAndGetReply<GameTiltRequestClearTiltReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.RequestClearTiltSuccess;
        }

        #endregion
    }
}