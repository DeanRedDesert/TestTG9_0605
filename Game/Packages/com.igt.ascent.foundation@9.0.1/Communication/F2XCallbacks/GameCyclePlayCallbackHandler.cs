// -----------------------------------------------------------------------
// <copyright file = "GameCyclePlayCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.CoplayerLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// This class implements callback methods supported by the
    /// F2X GameCyclePlay category.
    /// </summary>
    internal sealed class GameCyclePlayCallbackHandler : IGameCyclePlayCategoryCallbacks
    {
        #region Private Fields

        private readonly INonTransactionalEventCallbacks nonTransactionalEventCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="GameCyclePlayCallbackHandler"/>.
        /// </summary>
        /// <param name="nonTransactionalEventCallbacks">
        /// The callback interface for handling non-transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="nonTransactionalEventCallbacks"/> is null.
        /// </exception>
        public GameCyclePlayCallbackHandler(INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            this.nonTransactionalEventCallbacks = nonTransactionalEventCallbacks ?? throw new ArgumentNullException(nameof(nonTransactionalEventCallbacks));
        }

        #endregion

        #region IGameCyclePlayCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessAbortComplete()
        {
            nonTransactionalEventCallbacks.PostEvent(new AbortCompleteEventArgs());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessAwardResultsPosted(Amount amountWon)
        {
            // TODO ETG: Use case?
            return null;
        }

        /// <inheritdoc/>
        public string ProcessEnrollResponse(bool success)
        {
            nonTransactionalEventCallbacks.PostEvent(new EnrollResponseReadyEventArgs(success));
            return null;
        }

        /// <inheritdoc/>
        public string ProcessEvalOutcomeResponse(bool isLastOutcome)
        {
            nonTransactionalEventCallbacks.PostEvent(new OutcomeResponseReadyEventArgs(isLastOutcome));
            return null;
        }

        /// <inheritdoc/>
        public string ProcessFinalizeAwardResponse()
        {
            nonTransactionalEventCallbacks.PostEvent(new FinalizeOutcomeEventArgs());
            return null;
        }

        #endregion
    }
}