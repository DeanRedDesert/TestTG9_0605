//-----------------------------------------------------------------------
// <copyright file = "CustomConfigurationReadCategory.cs" company = "IGT">
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
    using Schemas.Internal.CustomConfigurationRead;
    using Version = Schemas.Internal.Types.Version;

    /// <summary>
    /// Implementation of the F2X <see cref="CustomConfigurationRead"/> category.
    /// Category of messages.  Category: 107   Version: 1. This category is used to request information about custom
    /// configuration items.  None of the messages in this category can modify custom configuration items.
    /// </summary>
    public class CustomConfigurationReadCategory : F2XTransactionalCategoryBase<CustomConfigurationRead>, ICustomConfigurationReadCategory, IMultiVersionSupport
    {
        #region Fields

        /// <summary>
        /// All versions supported by this category class.
        /// </summary>
        private readonly List<Version> supportedVersions = new List<Version>
        {
            new Version(1, 0),
            new Version(1, 1)
        };

        /// <summary>
        /// The version to use for communications by this category.
        /// Initialized to 0.0. Will be set by <see cref="SetVersion"/>.
        /// </summary>
        private Version effectiveVersion = new Version(0, 0);

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="CustomConfigurationReadCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        public CustomConfigurationReadCategory(IF2XTransport transport)
            : base(transport)
        {
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.CustomConfigurationRead;

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
                    $"{version} is not supported by CustomConfigurationReadCategory class.");
            }

            effectiveVersion = version;
        }

        #endregion

        #region ICustomConfigurationReadCategory Members

        /// <inheritdoc/>
        public IEnumerable<CustomConfigItemReferencedEnumeration> GetCustomConfigItemReferencedEnumerations(IEnumerable<CustomConfigurationItemSelector> customConfigItemSelection)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<GetCustomConfigItemReferencedEnumerationsSend>();
            var content = (GetCustomConfigItemReferencedEnumerationsSend)request.Message.Item;
            content.CustomConfigItemSelection = customConfigItemSelection.ToList();

            var reply = SendMessageAndGetReply<GetCustomConfigItemReferencedEnumerationsReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content;
        }

        /// <inheritdoc/>
        public IEnumerable<GetCustomConfigItemTypesReplyResult> GetCustomConfigItemTypes(IEnumerable<CustomConfigurationItemSelector> customConfigItemSelection)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<GetCustomConfigItemTypesSend>();
            var content = (GetCustomConfigItemTypesSend)request.Message.Item;
            content.CustomConfigItemSelection = customConfigItemSelection.ToList();

            var reply = SendMessageAndGetReply<GetCustomConfigItemTypesReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content;
        }

        /// <inheritdoc/>
        public IEnumerable<GetCustomConfigItemValueDataReplyResult> GetCustomConfigItemValueData(IEnumerable<GetCustomConfigItemValueDataSendSelector> customConfigItemSelection)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<GetCustomConfigItemValueDataSend>();
            var content = (GetCustomConfigItemValueDataSend)request.Message.Item;
            content.CustomConfigItemSelection = customConfigItemSelection.ToList();

            var reply = SendMessageAndGetReply<GetCustomConfigItemValueDataReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content;
        }

        /// <inheritdoc/>
        public IEnumerable<CustomConfigItemNameAndType> GetScopedCustomConfigItemNames(CustomConfigurationItemScopeSelector customConfigItemScopeSelection)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<GetScopedCustomConfigItemNamesSend>();
            var content = (GetScopedCustomConfigItemNamesSend)request.Message.Item;
            content.CustomConfigItemScopeSelection = customConfigItemScopeSelection;

            var reply = SendMessageAndGetReply<GetScopedCustomConfigItemNamesReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content;
        }

        #endregion

    }

}

