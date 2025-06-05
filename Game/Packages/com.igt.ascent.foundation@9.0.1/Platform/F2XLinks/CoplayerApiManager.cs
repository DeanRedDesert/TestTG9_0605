// -----------------------------------------------------------------------
// <copyright file = "CoplayerApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.CoplayerApiControl;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;

    /// <summary>
    /// This class manages the negotiation and installation of categories on <see cref="CategoryNegotiationLevel.Coplayer"/>.
    /// </summary>
    internal sealed class CoplayerApiManager : ApiManager, ICoplayerApiControlCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interface for handling API negotiation results.
        /// </summary>
        private readonly ICoplayerApiCallbacks coplayerApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerApiManager"/>.
        /// </summary>
        /// <param name="subscriptions">
        /// The categories to subscribe.
        /// </param>
        /// <param name="coplayerApiCallbacks">
        /// The interface for handling API negotiation results.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        public CoplayerApiManager(IList<CategorySubscription> subscriptions, ICoplayerApiCallbacks coplayerApiCallbacks = null)
            : base(CategoryNegotiationLevel.Coplayer, subscriptions)
        {
            this.coplayerApiCallbacks = coplayerApiCallbacks;
        }

        #endregion

        #region ICoplayerApiControlCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessGetCoplayerApiVersions(int coplayer,
                                                    ThemeIdentifier theme,
                                                    string themeTag,
                                                    string themeTagDataFile,
                                                    string g2SThemeIdentifier,
                                                    out GetCoplayerApiVersionsReplyContentCategoryVersions callbackResult)
        {
            coplayerApiCallbacks?.ProcessCoplayerApiStart(coplayer,
                                                          theme.Value,
                                                          g2SThemeIdentifier,
                                                          themeTag,
                                                          themeTagDataFile);

            callbackResult = new GetCoplayerApiVersionsReplyContentCategoryVersions
                                 {
                                     CategoryVersion = GetRequestedCategories()
                                                       .Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                                                       .ToList()
                                 };

            return null;
        }

        /// <inheritdoc/>
        public string ProcessSetCoplayerApiVersions(SetCoplayerApiVersionsSendCategoryVersions categoryVersions,
                                                    out bool callbackResult)
        {
            var selectedCategories = categoryVersions.CategoryVersion
                                                     .Select(version => version.ToPublic())
                                                     .ToList();

            callbackResult = TryInstallApiCategories(selectedCategories, out var installedHandlers);

            if(callbackResult)
            {
                coplayerApiCallbacks?.ProcessCoplayerApiNegotiation(installedHandlers);
            }

            // Callback result will report the success/failure.
            // There is no need to populate the error code and message.
            return null;
        }

        #endregion
    }
}