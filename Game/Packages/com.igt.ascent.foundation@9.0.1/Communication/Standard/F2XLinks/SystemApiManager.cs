// -----------------------------------------------------------------------
// <copyright file = "SystemApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2X.Schemas.Internal.SystemApiControl;
    using F2X.Schemas.Internal.Types;
    using F2XCallbacks;
    using F2XTransport;

    /// <summary>
    /// This class manages the negotiation and installation of categories on <see cref="CategoryNegotiationLevel.System"/>.
    /// </summary>
    internal sealed class SystemApiManager : ApiManager, ISystemApiControlCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interface for handling API negotiation results.
        /// </summary>
        private readonly ISystemApiCallbacks systemApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SystemApiManager"/>.
        /// </summary>
        /// <param name="subscriptions">
        /// The categories to subscribe.
        /// </param>
        /// <param name="systemApiCallbacks">
        /// The interface for handling API negotiation results.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        public SystemApiManager(IList<CategorySubscription> subscriptions, ISystemApiCallbacks systemApiCallbacks = null)
            : base(CategoryNegotiationLevel.System, subscriptions)
        {
            this.systemApiCallbacks = systemApiCallbacks;
        }

        #endregion

        #region ISystemApiControlCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessGetSystemApiVersions(IEnumerable<Extension> extensions,
                                                  out GetSystemApiVersionsReplyContentCategoryVersions callbackResult)
        {
            if(systemApiCallbacks != null)
            {
                systemApiCallbacks.ProcessSystemApiStart(extensions);
            }

            // TODO: Requested categories may depend on linked Extension Versions and could be determined from calling ProcesSystemApiStart.
            callbackResult = new GetSystemApiVersionsReplyContentCategoryVersions
                                 {
                                     CategoryVersion = GetRequestedCategories()
                                         .Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                                         .ToList()
                                 };

            return null;
        }

        /// <inheritdoc/>
        public string ProcessSetSystemApiVersions(SetSystemApiVersionsSendCategoryVersions categoryVersions,
                                                  out bool callbackResult)
        {
            var selectedCategories = categoryVersions.CategoryVersion
                                                     .Select(version => version.ToPublic())
                                                     .ToList();

            IDictionary<MessageCategory, IApiCategory> installedHandlers;

            callbackResult = TryInstallApiCategories(selectedCategories, out installedHandlers);

            if(callbackResult && systemApiCallbacks != null)
            {
                systemApiCallbacks.ProcessSystemApiNegotiation(installedHandlers);
            }

            // Callback result will report the success/failure.
            // There is no need to populate the error code and message.
            return null;
        }

        #endregion
    }
}