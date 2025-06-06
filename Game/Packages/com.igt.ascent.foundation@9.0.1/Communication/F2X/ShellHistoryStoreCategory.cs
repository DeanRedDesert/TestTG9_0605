//-----------------------------------------------------------------------
// <copyright file = "ShellHistoryStoreCategory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2XTransport;
    using Schemas.Internal.ShellHistoryStore;

    /// <summary>
    /// Implementation of the F2X <see cref="ShellHistoryStore"/> category.
    /// F2X Shell History Store category of messages, which provides the APIs for the concurrent game shell to access
    /// its critical data related to game cycle history. This category is only valid in Play mode.
    /// Category: 1027; Major Version: 1
    /// </summary>
    public class ShellHistoryStoreCategory : F2XTransactionalCategoryBase<ShellHistoryStore>, IShellHistoryStoreCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the ShellHistoryStoreCategory callbacks.
        /// </summary>
        private readonly IShellHistoryStoreCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="ShellHistoryStoreCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// ShellHistoryStoreCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public ShellHistoryStoreCategory(IF2XTransport transport, IShellHistoryStoreCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<LogEndGameCycleSend>(HandleLogEndGameCycle);
            AddMessagehandler<ShellHistoryWritePermittedSend>(HandleShellHistoryWritePermitted);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.ShellHistoryStore;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IShellHistoryStoreCategory Members

        /// <inheritdoc/>
        public IEnumerable<int> GetShellHistoryWritePermitted()
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<GetShellHistoryWritePermittedSend>();

            var reply = SendMessageAndGetReply<GetShellHistoryWritePermittedReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.Coplayers;
        }

        /// <inheritdoc/>
        public IEnumerable<ReadCritDataReplyContentItem> ReadCritData(int coplayer, IEnumerable<string> readKeys)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<ReadCritDataSend>();
            var content = (ReadCritDataSend)request.Message.Item;
            content.Coplayer = coplayer;
            content.ReadKeys = readKeys.ToList();

            var reply = SendMessageAndGetReply<ReadCritDataReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content;
        }

        /// <inheritdoc/>
        public void RemoveCritData(int coplayer, IEnumerable<string> criticalDataItems)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<RemoveCritDataSend>();
            var content = (RemoveCritDataSend)request.Message.Item;
            content.Coplayer = coplayer;
            content.CriticalDataItems = criticalDataItems.ToList();

            var reply = SendMessageAndGetReply<RemoveCritDataReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
        }

        /// <inheritdoc/>
        public void WriteCritData(int coplayer, IEnumerable<CriticalDataItemListItem> criticalDataItems)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<WriteCritDataSend>();
            var content = (WriteCritDataSend)request.Message.Item;
            content.Coplayer = coplayer;
            content.CriticalDataItems = criticalDataItems.ToList();

            var reply = SendMessageAndGetReply<WriteCritDataReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the LogEndGameCycleSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleLogEndGameCycle(LogEndGameCycleSend message)
        {
            var errorMessage = callbackHandler.ProcessLogEndGameCycle(message.Coplayer, message.NumberOfSteps, message.GamingMeters);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<LogEndGameCycleReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        /// <summary>
        /// Handler for the ShellHistoryWritePermittedSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleShellHistoryWritePermitted(ShellHistoryWritePermittedSend message)
        {
            var errorMessage = callbackHandler.ProcessShellHistoryWritePermitted(message.WritePermitted, message.Coplayer);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<ShellHistoryWritePermittedReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        #endregion

    }

}

