// -----------------------------------------------------------------------
// <copyright file = "AscribedGameApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2X.Schemas.Internal.AscribedGameApiControl;
    using F2X.Schemas.Internal.Types;
    using F2XCallbacks;
    using F2XTransport;

    /// <summary>
    /// This class manages the negotiation and installation of categories on <see cref="CategoryNegotiationLevel.AscribedGame"/>.
    /// </summary>
    internal sealed class AscribedGameApiManager : ApiManager, IAscribedGameApiControlCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interface for handling API negotiation results.
        /// </summary>
        private readonly IAscribedGameApiCallbacks ascribedGameApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AscribedGameApiManager"/>.
        /// </summary>
        /// <param name="subscriptions">
        /// The categories to subscribe.
        /// </param>
        /// <param name="ascribedGameApiCallbacks">
        /// The interface for handling API negotiation results.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        public AscribedGameApiManager(IList<CategorySubscription> subscriptions, IAscribedGameApiCallbacks ascribedGameApiCallbacks = null)
            : base(CategoryNegotiationLevel.AscribedGame, subscriptions)
        {
            this.ascribedGameApiCallbacks = ascribedGameApiCallbacks;
        }

        #endregion

        #region IAscribedGameApiControlCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessGetAscribedGameApiVersions(AscribedGame game,
                                                        IEnumerable<Extension> extensions,
                                                        out GetAscribedGameApiVersionsReplyContentCategoryVersions callbackResult)
        {
            if(ascribedGameApiCallbacks != null)
            {
                ascribedGameApiCallbacks.ProcessAscribedGameApiStart(game, extensions);
            }

            // TODO: Requested categories may depend on Game ID or Extension Version and could be determined from calling ProcessAscribedGameApiStart.
            callbackResult = new GetAscribedGameApiVersionsReplyContentCategoryVersions
            {
                CategoryVersion = GetRequestedCategories()
                    .Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                    .ToList()
            };

            return null;
        }

        /// <inheritdoc/>
        public string ProcessSetAscribedGameApiVersions(SetAscribedGameApiVersionsSendCategoryVersions categoryVersions,
                                                        out bool callbackResult)
        {
            var selectedCategories = categoryVersions.CategoryVersion
                                                     .Select(version => version.ToPublic())
                                                     .ToList();

            IDictionary<MessageCategory, IApiCategory> installedHandlers;

            callbackResult = TryInstallApiCategories(selectedCategories, out installedHandlers);

            if(callbackResult && ascribedGameApiCallbacks != null)
            {
                ascribedGameApiCallbacks.ProcessAscribedGameApiNegotiation(installedHandlers);
            }

            // Callback result will report the success/failure.
            // There is no need to populate the error code and message.
            return null;
        }

        #endregion
    }
}