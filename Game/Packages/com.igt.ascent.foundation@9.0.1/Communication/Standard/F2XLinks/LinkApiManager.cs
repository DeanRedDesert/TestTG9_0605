// -----------------------------------------------------------------------
// <copyright file = "LinkApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2X.Schemas.Internal.LinkControl;
    using F2XCallbacks;
    using F2XTransport;

    /// <summary>
    /// This class manages the negotiation and installation of categories on <see cref="CategoryNegotiationLevel.Link"/>.
    /// </summary>
    internal sealed class LinkApiManager : ApiManager, ILinkControlCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interfaces for handling API negotiation results.
        /// </summary>
        private ILinkApiCallbacks linkApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="LinkApiManager"/>.
        /// </summary>
        /// <param name="subscriptions">
        /// The categories to subscribe.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        public LinkApiManager(IList<CategorySubscription> subscriptions)
            : base(CategoryNegotiationLevel.Link, subscriptions)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This is for Link Controller to monitor the link level negotiations.
        /// </summary>
        /// <param name="callbacks">The callbacks interface to use.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="callbacks"/> is null.
        /// </exception>
        public void SetLinkApiCallbacks(ILinkApiCallbacks callbacks)
        {
            if(callbacks == null)
            {
                throw new ArgumentNullException("callbacks");
            }

            linkApiCallbacks = callbacks;
        }

        #endregion

        #region ILinkControlCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessGetApiVersions(string jurisdiction, string connectToken,
                                            GetApiVersionsSendDiscoveryContexts discoveryContexts,
                                            GetApiVersionsSendExtensionImports extensionImports,
                                            out GetApiVersionsReplyContentCategoryVersions callbackResult)
        {
            linkApiCallbacks.ProcessLinkApiStart(jurisdiction, connectToken,
                                                 discoveryContexts.DiscoveryContext,
                                                 extensionImports == null
                                                     ? new List<ExtensionImport>()
                                                     : extensionImports.ExtensionImport);

            callbackResult = new GetApiVersionsReplyContentCategoryVersions
                                 {
                                     CategoryVersion = GetRequestedCategories()
                                         .Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                                         .ToList()
                                 };

            return null;
        }

        /// <inheritdoc />
        public string ProcessSetApiVersions(SetApiVersionsSendCategoryVersions categoryVersions, out bool callbackResult)
        {
            List<CategoryVersionInformation> selectedCategories;

            // LinkControl schema allows null nodes here.
            if(categoryVersions == null || categoryVersions.CategoryVersion == null)
            {
                selectedCategories = new List<CategoryVersionInformation>();
            }
            else
            {
                selectedCategories = categoryVersions.CategoryVersion
                                                     .Select(version => version.ToPublic())
                                                     .ToList();
            }

            IDictionary<MessageCategory, IApiCategory> installedHandlers;

            callbackResult = TryInstallApiCategories(selectedCategories, out installedHandlers);

            if(callbackResult && linkApiCallbacks != null)
            {
                linkApiCallbacks.ProcessLinkApiNegotiation(installedHandlers);
            }

            // Callback result will report the success/failure.
            // There is no need to populate the error code and message.
            return null;
        }

        /// <inheritdoc />
        public string ProcessPark()
        {
            linkApiCallbacks.ProcessLinkPark();

            return null;
        }

        #endregion
    }
}