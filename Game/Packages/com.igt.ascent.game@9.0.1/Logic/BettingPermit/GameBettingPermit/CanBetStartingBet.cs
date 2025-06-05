// -----------------------------------------------------------------------
// <copyright file = "CanBetStartingBet.cs" company = "IGT">
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
    /// A game side can-bet operand that checks bet amounts against starting bet rules.
    /// </summary>
    public sealed class CanBetStartingBet : ICanBetOperand
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
        /// The interface used to query game cycle betting information.
        /// </summary>
        private readonly IGameCycleBetting gameCycleBetting;

        /// <summary>
        /// The interface used to query current player bettable meter balance.
        /// </summary>
        private readonly IShellExposition shellExposition;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CanBetStartingBet"/>.
        /// </summary>
        /// <param name="startingBetLimits">
        /// Limits for starting bet amounts.
        /// </param>
        /// <param name="gameCyclePlay">
        /// The interface used to query game cycle state.
        /// </param>
        /// <param name="gameCycleBetting">
        /// The interface used to query game cycle betting information.
        /// </param>
        /// <param name="shellExposition">
        /// The interface used to query current player bettable meter balance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the argument is null.
        /// </exception>
        public CanBetStartingBet(BetLimits startingBetLimits,
                                 IGameCyclePlay gameCyclePlay,
                                 IGameCycleBetting gameCycleBetting,
                                 IShellExposition shellExposition)
        {
            this.startingBetLimits = startingBetLimits ?? throw new ArgumentNullException(nameof(startingBetLimits));
            this.gameCyclePlay = gameCyclePlay ?? throw new ArgumentNullException(nameof(gameCyclePlay));
            this.gameCycleBetting = gameCycleBetting ?? throw new ArgumentNullException(nameof(gameCycleBetting));
            this.shellExposition = shellExposition ?? throw new ArgumentNullException(nameof(shellExposition));
        }

        #endregion

        #region ILogicalOperand<Amount> Implementation

        /// <inheritdoc/>
        public bool Execute(Amount data)
        {
            var result = false;

            switch(gameCyclePlay.GameCycleState)
            {
                case GameCycleState.Idle:
                case GameCycleState.Committed:
                {
                    result = startingBetLimits.Allows(data.BaseValue) &&
                             data.BaseValue <= shellExposition.BankProperties.PlayerBettableMeter +
                                               (gameCycleBetting.GetCommittedBet() ?? 0);
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}