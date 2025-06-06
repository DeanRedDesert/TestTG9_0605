﻿//-----------------------------------------------------------------------
// <copyright file = "KeyValueStoreCategory.cs" company = "IGT">
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
    using Schemas.Internal.KVSTypes;

    /// <summary>
    /// Implementation of the F2X <see cref="KeyValueStore"/> category.
    /// KeyValueStore supports all KeyValueStoreMethod implementations.
    /// </summary>
    public abstract class KeyValueStoreCategory<TCategory> : F2XTransactionalCategoryBase<TCategory>, IKeyValueStoreCategory where TCategory : class, ICategory, new()
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="KeyValueStoreCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        protected KeyValueStoreCategory(IF2XTransport transport)
            : base(transport)
        {
        }

        #endregion

        #region IKeyValueStoreCategory Members

        /// <inheritdoc/>
        public IEnumerable<ReadReplyContentItem> Read(IEnumerable<string> key)
        {
            var request = CreateTransactionalRequest<ReadSend>();
            var content = (ReadSend)request.Message.Item;
            content.Key = key.ToList();

            var reply = SendMessageAndGetReply<ReadReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content;
        }

        /// <inheritdoc/>
        public void Remove(IEnumerable<string> key)
        {
            var request = CreateTransactionalRequest<RemoveSend>();
            var content = (RemoveSend)request.Message.Item;
            content.Key = key.ToList();

            var reply = SendMessageAndGetReply<RemoveReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
        }

        /// <inheritdoc/>
        public void Write(IEnumerable<WriteSendItem> item)
        {
            var request = CreateTransactionalRequest<WriteSend>();
            var content = (WriteSend)request.Message.Item;
            content.Item = item.ToList();

            var reply = SendMessageAndGetReply<WriteReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
        }

        #endregion

    }

}

