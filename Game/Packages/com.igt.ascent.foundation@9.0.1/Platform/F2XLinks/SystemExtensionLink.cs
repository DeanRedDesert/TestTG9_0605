// -----------------------------------------------------------------------
// <copyright file = "SystemExtensionLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExtensionBinLib.Interfaces;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;
    using F2XInternalTypes = Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;

    /// <summary>
    /// Implementation of <see cref="ISystemExtensionLink"/>.
    /// </summary>
    internal sealed class SystemExtensionLink : InnerLinkBase, ISystemExtensionLink, ISystemApiCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The list of System message categories this link will subscribe.
        /// </summary>
        private readonly List<CategorySubscription> systemCategorySubscriptions =
            new List<CategorySubscription>
                {
                    new CategorySubscription(MessageCategory.SystemActivation, false),
                };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SystemExtensionLink"/>.
        /// </summary>
        internal SystemExtensionLink()
        {
            ApiManager = new SystemApiManager(systemCategorySubscriptions, this);
        }

        #endregion

        #region ISystemExtensionLink Implementation

        /// <inheritdoc />
        public IReadOnlyList<IExtensionIdentity> SupportedExtensions { get; private set; }

        #endregion

        #region ISystemApiCallbacks Implementation

        /// <inheritdoc />
        public void ProcessSystemApiStart(IEnumerable<F2XInternalTypes.Extension> extensions)
        {
            if(extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            IsConnected = false;

            SupportedExtensions = extensions.Select(extension => extension.ToPublic())
                                             .ToList();
        }

        /// <inheritdoc />
        public void ProcessSystemApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            IsConnected = true;
        }

        #endregion
    }
}