//-----------------------------------------------------------------------
// <copyright file = "UgpMessageStripCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System;
    using System.Collections.Generic;
	using F2X;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2X UgpMessageStrip category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpMessageStripCategory : F2XTransactionalCategoryBase<UgpMessageStripCategoryInternal>,
                                             IUgpMessageStripCategory
    {
        #region Private Fields

        /// <summary>
        /// The message list.
        /// </summary>
        private readonly List<string> messages = new List<string>();

        /// <summary>
        /// The lock to access the message list.
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// Object which implements the UgpMessageStripCategory callbacks.
        /// </summary>
        private readonly IUgpMessageStripCategoryCallbacks callbackHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="UgpMessageStripCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// UgpMessageStripCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public UgpMessageStripCategory(IF2XTransport transport, IUgpMessageStripCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));

            AddMessagehandler<UgpMessageStripCategoryAddMessageSend>(HandleAddMessageSend);
            AddMessagehandler<UgpMessageStripCategoryRemoveMessageSend>(HandleRemoveMessageSend);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpMessageStrip;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 1;

        #endregion

        #region IUgpMessageStripCategory Implementation

        /// <inheritdoc/>
        public IEnumerable<string> GetMessages()
        {
			var ugpMessages = SendGetMessagesRequestAndGetReply();

			lock(lockObject)
			{
				messages.Clear();

				for (var i = 0; i < ugpMessages.Count; i++)
					AddMessageText(messages, ugpMessages[i]);

				return messages.ToArray();
			}
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the UgpMessageStripCategoryAddMessageSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleAddMessageSend(UgpMessageStripCategoryAddMessageSend message)
        {
            lock(lockObject)
            {
				AddMessageText(messages, message);
            }

            callbackHandler.ProcessAddMessage(messages);

            var reply = CreateReply<UgpMessageStripCategoryAddMessageReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the UgpMessageStripCategoryRemoveMessageSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleRemoveMessageSend(UgpMessageStripCategoryRemoveMessageSend message)
        {
            lock(lockObject)
            {
                messages.RemoveAll(x => x == message.MessageText);
            }

            callbackHandler.ProcessRemoveMessage(messages);

            var reply = CreateReply<UgpMessageStripCategoryRemoveMessageReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

		#region Private Methods

		private List<UgpMessageStripMessage> SendGetMessagesRequestAndGetReply()
		{
			var request = CreateTransactionalRequest<UgpMessageStripCategoryGetMessagesSend>();
			var reply = SendMessageAndGetReply<UgpMessageStripCategoryGetMessagesReply>(Channel.Foundation, request);

			CheckReply(new ReplyException
			{
				ErrorCode = reply.Reply.ReplyCode,
				ErrorDescription = reply.Reply.ErrorDescription
			});

			return reply.Messages;
		}

		private static void AddMessageText(IList<string> textMessages, UgpMessageStripCategoryAddMessageSend ugpMessage)
		{
			AddMessageText(textMessages, ugpMessage.MessageText, ugpMessage.IsPriority);
		}

		private static void AddMessageText(IList<string> textMessages, UgpMessageStripMessage ugpMessage)
		{
			AddMessageText(textMessages, ugpMessage.MessageText, ugpMessage.IsPriority);
		}

		private static void AddMessageText(IList<string> textMessages, string messageText, bool isPriority)
		{
			if (isPriority)
				textMessages.Insert(0, messageText);
			else
				textMessages.Add(messageText);
		}

		#endregion
    }
}
