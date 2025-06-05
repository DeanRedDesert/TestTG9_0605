// -----------------------------------------------------------------------
// <copyright file = "AppApiManager.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2X.Schemas.Internal.AppApiControl;
    using F2X.Schemas.Internal.Types;
    using F2XCallbacks;
    using F2XTransport;

    /// <summary>
    /// This class manages the negotiation and installation of categories on <see cref="CategoryNegotiationLevel.App"/>.
    /// </summary>
    internal sealed class AppApiManager : ApiManager, IAppApiControlCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interface for handling API negotiation results.
        /// </summary>
        private readonly IAppApiCallbacks appApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AppApiManager"/>.
        /// </summary>
        /// <param name="subscriptions">
        /// The categories to subscribe.
        /// </param>
        /// <param name="appApiCallbacks">
        /// The interface for handling API negotiation results.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        public AppApiManager(IList<CategorySubscription> subscriptions, IAppApiCallbacks appApiCallbacks = null)
            : base(CategoryNegotiationLevel.App, subscriptions)
        {
            this.appApiCallbacks = appApiCallbacks;
        }

        #endregion

        #region IAppApiControlCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessGetAppApiVersions(IEnumerable<Extension> extensions,
                                                  out GetAppApiVersionsReplyContentCategoryVersions callbackResult)
        {
            appApiCallbacks?.ProcessAppApiStart(extensions);

            // TODO: Requested categories may depend on linked Extension Versions and could be determined from calling ProcesAppApiStart.
            callbackResult = new GetAppApiVersionsReplyContentCategoryVersions
                                    {
                                        CategoryVersion = GetRequestedCategories()
                                            .Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                                            .ToList()
                                    };

            return null;
        }

        /// <inheritdoc/>
        public string ProcessSetAppApiVersions(SetAppApiVersionsSendCategoryVersions categoryVersions,
                                                  out bool callbackResult)
        {
            var selectedCategories = categoryVersions.CategoryVersion
                                                     .Select(version => version.ToPublic())
                                                     .ToList();

            callbackResult = TryInstallApiCategories(selectedCategories, out var installedHandlers);

            if(callbackResult)
            {
                appApiCallbacks?.ProcessAppApiNegotiation(installedHandlers);
            }

            // Callback result will report the success/failure.
            // There is no need to populate the error code and message.
            return null;
        }

        #endregion
    }
}
