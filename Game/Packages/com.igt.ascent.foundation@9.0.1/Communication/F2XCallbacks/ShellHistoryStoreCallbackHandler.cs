// -----------------------------------------------------------------------
// <copyright file = "ShellHistoryStoreCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ShellLib.Interfaces;
    using F2X;
    using F2XTransport;
    using F2XGamingMeters = F2X.Schemas.Internal.ShellHistoryStore.GamingMeters;

    /// <summary>
    /// This class is responsible for handling callbacks from the <see cref="ShellHistoryStoreCategory"/>.
    /// </summary>
    internal sealed class ShellHistoryStoreCallbackHandler : IShellHistoryStoreCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellHistoryStoreCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">
        /// The callback interface for handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public ShellHistoryStoreCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #endregion

        #region IShellHistoryStoreCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessLogEndGameCycle(int coplayer, uint numberOfSteps, F2XGamingMeters gamingMeters)
        {
            var eventArgs = new LogEndGameCycleEventArgs(coplayer, Convert.ToInt32(numberOfSteps), ToPublic(gamingMeters));

            eventCallbacks.PostEvent(eventArgs);

            return null;
        }

        /// <inheritdoc/>
        public string ProcessShellHistoryWritePermitted(bool writePermitted, int coplayer)
        {
            eventCallbacks.PostEvent(new ShellHistoryStoreWritePermittedChangedEventArgs(coplayer, writePermitted));

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts an F2X <see cref="F2XGamingMeters"/> to a public <see cref="GamingMeters"/>.
        /// </summary>
        /// <param name="gamingMeters">The F2X gaming meters to convert.</param>
        /// <returns>The conversion result.</returns>
        private static GamingMeters ToPublic(F2XGamingMeters gamingMeters)
        {
            if(gamingMeters == null)
            {
                throw new ArgumentNullException(nameof(gamingMeters));
            }

            return new GamingMeters(gamingMeters.PlayerTransferable.Value,
                                    gamingMeters.PlayerBettable.Value,
                                    gamingMeters.PaidMeter.Value);
        }

        #endregion
    }
}