//-----------------------------------------------------------------------
// <copyright file = "ConnectCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using F2XTransport;
    using Schemas;
    using Schemas.Internal;

    /// <summary>
    /// F2L connect category implementation. Handles negotiation of the link control API version.
    /// </summary>
    public class ConnectCategory : F2LCategoryBase<Connect>
    {
        #region Fields

        /// <summary>
        /// Category handler which provides support for the link category.
        /// </summary>
        private readonly IApiCategory linkCategory;

        /// <summary>
        /// Callback handler for connect category messages.
        /// </summary>
        private readonly IConnectCallbacks connectCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Create an instance of the connect category.
        /// </summary>
        /// <param name="transport">The transport object this category is installed in.</param>
        /// <param name="connectCallbacks">Callback for connect category methods.</param>
        /// <param name="linkCategory">Category handler which provides support for the link category.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="connectCallbacks"/> or <paramref name="linkCategory"/> is null.
        /// </exception>
        public ConnectCategory(IF2XTransport transport, IConnectCallbacks connectCallbacks, IApiCategory linkCategory)
            : base(transport)
        {
            if(connectCallbacks == null)
            {
                throw new ArgumentNullException("connectCallbacks", "Argument may not be null.");
            }

            if(linkCategory == null)
            {
                throw new ArgumentNullException("linkCategory", "Argument may not be null.");
            }

            this.linkCategory = linkCategory;
            this.connectCallbacks = connectCallbacks;

            AddMessagehandler<ConnectGetLinkControlApiVersionsSend>(HandleGetLinkControlApiVersionsSend);
            AddMessagehandler<ConnectSetLinkControlApiVersionSend>(HandleSetLinkControlApiVersionsSend);
            AddMessagehandler<ConnectShutdownSend>(HandleShutDownSend);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handle the request for the link control versions. Get the link control versions supported and return them
        /// to the foundation.
        /// </summary>
        /// <param name="getLinkMessage">Requesting message.</param>
        private void HandleGetLinkControlApiVersionsSend(ConnectGetLinkControlApiVersionsSend getLinkMessage)
        {
            SendFoundationChannelResponse(GetReplyGetLinkControlApiVersion());
        }

        /// <summary>
        /// Handle the request to set the link control versions. Check the version being set by the foundation and
        /// reply if it is acceptable or not.
        /// </summary>
        /// <param name="setLinkMessage">Requesting message.</param>
        private void HandleSetLinkControlApiVersionsSend(ConnectSetLinkControlApiVersionSend setLinkMessage)
        {
            SendFoundationChannelResponse(GetReplySetLinkControlApiVersion(setLinkMessage));
        }

        /// <summary>
        /// Handle the request to shutdown. Inform the callback handler to process the shutdown message. This message
        /// is unique because it has no reply. The game is to disconnect and close.
        /// </summary>
        /// <param name="shutdownMessage">Requesting message.</param>
        private void HandleShutDownSend(ConnectShutdownSend shutdownMessage)
        {
            connectCallbacks.ProcessShutDown();
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc />
        public override MessageCategory Category
        {
            get { return MessageCategory.F2LConnect; }
        }

        /// <inheritdoc />
        public override uint MajorVersion
        {
            get { return 2; }
        }

        /// <inheritdoc />
        public override uint MinorVersion
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public override void HandleReply(object message, Channel channel)
        {
            throw new UnhandledReplyTypeException("Connect category has no supported message replies.",
                                                  message.GetType());
        }

        #endregion

        #region Reply Builders

        /// <summary>
        /// Construct a GetReplyLinkControlApiVersionsReply message.
        /// </summary>
        /// <returns>A populated GetReplyLinkControlApiVersionsReply message.</returns>
        private Connect GetReplyGetLinkControlApiVersion()
        {
            var reply = CreateReply<ConnectGetLinkControlApiVersionsReply>(0, null);

            var replyContent = (ConnectGetLinkControlApiVersionsReply)reply.Message.Item;

            replyContent.CategoryVersions = new ConnectGetLinkControlApiVersionsReplyCategoryVersions
                                                {
                                                    CategoryVersion =
                                                        new List<CategoryVersionType>
                                                            {
                                                                new CategoryVersionType
                                                                    {
                                                                        Category = (int)linkCategory.Category,
                                                                        Version =
                                                                            new VersionType(linkCategory.MajorVersion,
                                                                                            linkCategory.MinorVersion)
                                                                    }
                                                            }
                                                };

            return reply;
        }

        /// <summary>
        /// Construct a GetReplyLinkControlApiVersion message.
        /// </summary>
        /// <param name="setLink">Message from the foundation attempting to set the link control API version.</param>
        /// <returns>A populated GetReplyLinkControlApiVersion message.</returns>
        private Connect GetReplySetLinkControlApiVersion(ConnectSetLinkControlApiVersionSend setLink)
        {
            //Major versions must match.
            var invalidVersion = setLink.CategoryVersion.Version.MajorVersion != linkCategory.MajorVersion;

            //The minor version of the system must be greater than or equal to the minor version we support.
            invalidVersion |= setLink.CategoryVersion.Version.MinorVersion < linkCategory.MinorVersion;

            var invalidCategory = setLink.CategoryVersion.Category != (int)linkCategory.Category;

            string errorDescription = null;

            if(invalidVersion)
            {
                errorDescription = string.Format(CultureInfo.InvariantCulture,
                                                 "Unsupported Link Version: Expected: {0}.{1}, Actual: {2}.{3}",
                                                 linkCategory.MajorVersion,
                                                 linkCategory.MinorVersion,
                                                 setLink.CategoryVersion.Version.MajorVersion,
                                                 setLink.CategoryVersion.Version.MinorVersion);
            }
            else if(invalidCategory)
            {
                errorDescription = string.Format(CultureInfo.InvariantCulture,
                                                 "Invalid Link Category: Expected: {0}, Actual: {1}",
                                                 linkCategory.Category, setLink.CategoryVersion.Category);
            }


            var reply = CreateReply<ConnectSetLinkControlApiVersionReply>((invalidVersion || invalidCategory) ? 1 : 0,
                                                                          errorDescription);

            return reply;
        }

        #endregion
    }
}