// -----------------------------------------------------------------------
// <copyright file = "TsmApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2X.Schemas.Internal.TsmApiControl;
    using F2X.Schemas.Internal.Types;
    using F2XCallbacks;
    using F2XTransport;

    /// <summary>
    /// This class manages the negotiation and installation of categories on <see cref="CategoryNegotiationLevel.Tsm"/>.
    /// </summary>
    internal sealed class TsmApiManager : ApiManager, ITsmApiControlCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interface for handling API negotiation results.
        /// </summary>
        private readonly ITsmApiCallbacks tsmApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="TsmApiManager"/>.
        /// </summary>
        /// <param name="subscriptions">
        /// The categories to subscribe.
        /// </param>
        /// <param name="tsmApiCallbacks">
        /// The interface for handling API negotiation results.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        public TsmApiManager(IList<CategorySubscription> subscriptions, ITsmApiCallbacks tsmApiCallbacks = null)
            : base(CategoryNegotiationLevel.Tsm, subscriptions)
        {
            this.tsmApiCallbacks = tsmApiCallbacks;
        }

        #endregion

        #region ITsmApiControlCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessGetTsmApiVersions(TsmIdentifier tsm,
                                               IEnumerable<Extension> extensions,
                                               out GetTsmApiVersionsReplyContentCategoryVersions callbackResult)
        {
            if(tsmApiCallbacks != null)
            {
                tsmApiCallbacks.ProcessTsmApiStart(tsm.Value, extensions);
            }

            // TODO: Requested categories may depend on TSM ID or Extension Version and could be determined from calling ProcesTsmApiStart.
            callbackResult = new GetTsmApiVersionsReplyContentCategoryVersions
                                 {
                                     CategoryVersion = GetRequestedCategories().Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                                                                               .ToList(),
                                 };

            return null;
        }

        /// <inheritdoc/>
        public string ProcessSetTsmApiVersions(SetTsmApiVersionsSendCategoryVersions categoryVersions,
                                               out bool callbackResult)
        {
            var selectedCategories = categoryVersions.CategoryVersion
                                                     .Select(version => version.ToPublic())
                                                     .ToList();

            IDictionary<MessageCategory, IApiCategory> installedHandlers;

            callbackResult = TryInstallApiCategories(selectedCategories, out installedHandlers);

            if(callbackResult && tsmApiCallbacks != null)
            {
                tsmApiCallbacks.ProcessTsmApiNegotiation(installedHandlers);
            }

            // Callback result will report the success/failure.
            // There is no need to populate the error code and message.
            return null;
        }

        #endregion
    }
}