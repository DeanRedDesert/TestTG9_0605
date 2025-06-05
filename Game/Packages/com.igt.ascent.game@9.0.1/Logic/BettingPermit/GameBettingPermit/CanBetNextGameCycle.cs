// -----------------------------------------------------------------------
// <copyright file = "CanBetNextGameCycle.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.GameBettingPermit
{
    using System;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Concurrent.Interfaces;
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// A game side can-bet operand that checks bet amounts for next game cycle.
    /// </summary>
    public sealed class CanBetNextGameCycle : ICanBetOperand
    {
        #region Private Fields

        /// <summary>
        /// Limits for starting bet amounts.
        /// </summary>
        private readonly BetLimits startingBetLimits;

        /// <summary>
        /// The interface used to query game cycle state.
        /// </summary>
        private readonly IGameCyclePlay gameCyclePlay;

        /// <summary>
        /// The interface used to query current player bettable meter balance.
        /// </summary>
        private readonly IShellExposition shellExposition;

        /// <summary>
        /// The interface used to query the pending win of current game cycle.
        /// </summary>
        private readonly IPendingWinQuery pendingWinQuery;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CanBetNextGameCycle"/>.
        /// </summary>
        /// <param name="startingBetLimits">
        /// Limits for starting bet amounts.
        /// </param>
        /// <param name="gameCyclePlay">
        /// The interface used to query game cycle state.
        /// </param>
        /// <param name="shellExposition">
        /// The interface used to query current player bettable meter balance.
        /// </param>
        /// <param name="pendingWinQuery">
        /// The interface used to query the pending win of current game cycle.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the argument is null.
        /// </exception>
        public CanBetNextGameCycle(BetLimits startingBetLimits,
                                   IGameCyclePlay gameCyclePlay,
                                   IShellExposition shellExposition,
                                   IPendingWinQuery pendingWinQuery)
        {
            this.startingBetLimits = startingBetLimits ?? throw new ArgumentNullException(nameof(startingBetLimits));
            this.gameCyclePlay = gameCyclePlay ?? throw new ArgumentNullException(nameof(gameCyclePlay));
            this.shellExposition = shellExposition ?? throw new ArgumentNullException(nameof(shellExposition));
            this.pendingWinQuery = pendingWinQuery ?? throw new ArgumentNullException(nameof(pendingWinQuery));
        }

        #endregion

        #region ILogicalOperand<Amount> Implementation

        /// <inheritdoc/>
        public bool Execute(Amount data)
        {
            var result = false;

            switch(gameCyclePlay.GameCycleState)
            {
                case GameCycleState.MainPlayComplete:
                {
                    result = startingBetLimits.Allows(data.BaseValue) &&
                             data.BaseValue <= shellExposition.BankProperties.PlayerBettableMeter + pendingWinQuery.GetPendingWin();
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}