// -----------------------------------------------------------------------
// <copyright file = "ThemeApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2X.Schemas.Internal.ThemeApiControl;
    using F2X.Schemas.Internal.Types;
    using F2XCallbacks;
    using F2XTransport;

    /// <summary>
    /// This class manages the negotiation and installation of categories on <see cref="CategoryNegotiationLevel.Theme"/>.
    /// </summary>
    internal sealed class ThemeApiManager : ApiManager, IThemeApiControlCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interface for handling API negotiation results.
        /// </summary>
        private readonly IThemeApiCallbacks themeApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ThemeApiManager"/>.
        /// </summary>
        /// <param name="subscriptions">
        /// The categories to subscribe.
        /// </param>
        /// <param name="themeApiCallbacks">
        /// The interface for handling API negotiation results.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        public ThemeApiManager(IList<CategorySubscription> subscriptions, IThemeApiCallbacks themeApiCallbacks = null)
            : base(CategoryNegotiationLevel.Theme, subscriptions)
        {
            this.themeApiCallbacks = themeApiCallbacks;
        }

        #endregion

        #region IThemeApiControlCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessGetThemeApiVersions(ThemeIdentifier tsm,
                                                 IEnumerable<Extension> extensions,
                                                 out GetThemeApiVersionsReplyContentCategoryVersions callbackResult)
        {
            if(themeApiCallbacks != null)
            {
                themeApiCallbacks.ProcessThemeApiStart(tsm.Value, extensions);
            }

            // TODO: Requested categories may depend on Theme ID or Extension Version and could be determined from calling ProcesThemeApiStart.
            callbackResult = new GetThemeApiVersionsReplyContentCategoryVersions
                                 {
                                     CategoryVersion = GetRequestedCategories()
                                         .Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                                         .ToList()
                                 };

            return null;
        }

        /// <inheritdoc/>
        public string ProcessSetThemeApiVersions(SetThemeApiVersionsSendCategoryVersions categoryVersions,
                                                 out bool callbackResult)
        {
            var selectedCategories = categoryVersions.CategoryVersion
                                                     .Select(version => version.ToPublic())
                                                     .ToList();

            IDictionary<MessageCategory, IApiCategory> installedHandlers;

            callbackResult = TryInstallApiCategories(selectedCategories, out installedHandlers);

            if(callbackResult && themeApiCallbacks != null)
            {
                themeApiCallbacks.ProcessThemeApiNegotiation(installedHandlers);
            }

            // Callback result will report the success/failure.
            // There is no need to populate the error code and message.
            return null;
        }

        #endregion
    }
}