// -----------------------------------------------------------------------
// <copyright file = "ConnectApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2X.Schemas.Internal.Connect;
    using F2X.Schemas.Internal.Types;
    using F2XCallbacks;
    using F2XTransport;
    using InterfaceExtensions.Interfaces;

    /// <summary>
    /// This class manages the negotiation and installation of categories on connect category level.
    /// </summary>
    internal sealed class ConnectApiManager : ApiManager, IConnectCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interfaces for handling API negotiation results.
        /// </summary>
        private readonly IConnectApiCallbacks connectApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ConnectApiManager"/>.
        /// </summary>
        /// <param name="connectApiCallbacks">
        /// The interface for handling API negotiation results.
        /// </param>
        /// <devdoc>
        /// There is no negotiation level of Connect, so we just use Link here.
        /// </devdoc>
        public ConnectApiManager(IConnectApiCallbacks connectApiCallbacks)
            : base(CategoryNegotiationLevel.Link, new List<CategorySubscription>
                                                      {
                                                          new CategorySubscription(MessageCategory.F2XLinkControl, true)
                                                      })
        {
            this.connectApiCallbacks = connectApiCallbacks ?? throw new ArgumentNullException(nameof(connectApiCallbacks));
        }

        #endregion

        #region ApiManager Overrides

        /// <inheritdoc/>
        public override void Initialize(FoundationTarget target,
                                        ICategoryNegotiationDependencies categoryDependencies,
                                        IInterfaceExtensionDependencies baseExtensionDependencies = null,
                                        IEnumerable<IInterfaceExtensionConfiguration> interfaceExtensionConfigurations = null)
        {
            // Connect level does not have interface extensions.
            DoInitialize(target, categoryDependencies, null, null, true);
        }

        #endregion

        #region IConnectCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessGetLinkControlApiVersions(out GetLinkControlApiVersionsReplyContentCategoryVersions callbackResult)
        {
            callbackResult = new GetLinkControlApiVersionsReplyContentCategoryVersions
                                 {
                                     CategoryVersion = GetRequestedCategories()
                                         .Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                                         .ToList()
                                 };

            return null;
        }

        /// <inheritdoc />
        public string ProcessSetLinkControlApiVersion(CategoryVersion categoryVersion)
        {
            var selectedCategories = new List<CategoryVersionInformation> { categoryVersion.ToPublic() };

            string errorMessage = null;

            // Connect category does not have a "category accepted" field.
            // So we use the error message to report the failure.
            if(!TryInstallApiCategories(selectedCategories, out _))
            {
                errorMessage = "Failed to install API categories.";
            }

            return errorMessage;
        }

        /// <inheritdoc />
        public void ProcessShutdown()
        {
            connectApiCallbacks.ProcessConnectShutDown();
        }

        #endregion
    }
}