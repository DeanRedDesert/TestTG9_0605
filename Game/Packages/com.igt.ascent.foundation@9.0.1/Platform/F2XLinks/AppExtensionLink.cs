// -----------------------------------------------------------------------
// <copyright file = "AppExtensionLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExtensionBinLib.Interfaces;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;

    /// <summary>
    /// Implementation of <see cref="IAppExtensionLink"/>.
    /// </summary>
    internal sealed class AppExtensionLink : InnerLinkBase, IAppExtensionLink, IAppApiCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The list of App message categories this link will subscribe.
        /// </summary>
        private readonly List<CategorySubscription> appCategorySubscriptions =
            new List<CategorySubscription>
                {
                    new CategorySubscription(MessageCategory.AppActivation, true),
                    new CategorySubscription(MessageCategory.DisplayControl, true),
                    new CategorySubscription(MessageCategory.ChooserServices, true),
                };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AppExtensionLink"/>.
        /// </summary>
        internal AppExtensionLink()
        {
            ApiManager = new AppApiManager(appCategorySubscriptions, this);
        }

        #endregion

        #region IAppExtensionLink Implementation

        /// <inheritdoc />
        public IExtensionIdentity ActiveAppExtension { get; private set; }

        /// <inheritdoc />
        public IChooserServicesCategory ChooserServicesCategory { get; private set; }

        #endregion

        #region IAppApiCallbacks Implementation

        /// <inheritdoc />
        public void ProcessAppApiStart(IEnumerable<Extension> extensions)
        {
            if(extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            IsConnected = false;

            // Make sure that the number of activated app extensions is one.
            var extensionList = extensions.ToList();
            if(extensionList.Count != 1)
            {
                throw new InvalidOperationException(
                    $"The number of activated app extensions must be one, it is actually: {extensionList.Count}.");
            }

            ActiveAppExtension = extensionList.First().ToPublic();
        }

        /// <inheritdoc />
        public void ProcessAppApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            ChooserServicesCategory = Retrieve<IChooserServicesCategory>(installedHandlers, MessageCategory.ChooserServices);

            IsConnected = true;
        }

        #endregion
    }
}