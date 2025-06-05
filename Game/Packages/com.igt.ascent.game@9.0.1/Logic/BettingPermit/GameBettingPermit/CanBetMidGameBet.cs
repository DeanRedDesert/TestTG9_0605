// -----------------------------------------------------------------------
// <copyright file = "CanBetMidGameBet.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.GameBettingPermit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Concurrent.Interfaces;
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// A game side can-bet operand that checks bet amounts against mid game betting rules.
    /// </summary>
    /// <devdoc>
    /// This class is made internal to prevent games from using it for the moment.
    /// It should be made public when the mid-game betting support becomes available in Foundation.
    /// Also remember to remove the ReSharper warning suppression below.
    /// </devdoc>
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    internal sealed class CanBetMidGameBet : ICanBetOperand
    {
        #region Private Fields

        /// <summary>
        /// Limits for mid game bet amounts.
        /// </summary>
        private readonly BetLimits midGameBetLimits;

        /// <summary>
        /// How to apply the mid game max bet limit.
        /// </summary>
        private readonly MidGameMaxBetBehavior midGameMaxBetBehavior;

        /// <summary>
        /// The interface used to query game cycle state.
        /// </summary>
        private readonly IGameCyclePlay gameCyclePlay;

        /// <summary>
        /// The interface used to query the accumulated bet amount of current game cycle.
        /// </summary>
        private readonly IGameCycleBetting gameCycleBetting;

        /// <summary>
        /// The interface used to query current player bettable meter balance.
        /// </summary>
        private readonly IShellExposition shellExposition;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CanBetMidGameBet"/>.
        /// </summary>
        /// <param name="midGameBetLimits">
        /// Limits for mid game bet amounts.
        /// </param>
        /// <param name="midGameMaxBetBehavior">
        /// How to apply the mid game max bet limit.
        /// </param>
        /// <param name="gameCyclePlay">
        /// The interface used to query game cycle state.
        /// </param>
        /// <param name="gameCycleBetting">
        /// The interface used to query the accumulated bet amount of current game cycle.
        /// </param>
        /// <param name="shellExposition">
        /// The interface used to query current player bettable meter balance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the argument is null.
        /// </exception>
        public CanBetMidGameBet(BetLimits midGameBetLimits,
                                MidGameMaxBetBehavior midGameMaxBetBehavior,
                                IGameCyclePlay gameCyclePlay,
                                IGameCycleBetting gameCycleBetting,
                                IShellExposition shellExposition)
        {
            this.midGameBetLimits = midGameBetLimits ?? throw new ArgumentNullException(nameof(midGameBetLimits));
            this.gameCyclePlay = gameCyclePlay ?? throw new ArgumentNullException(nameof(gameCyclePlay));
            this.gameCycleBetting = gameCycleBetting ?? throw new ArgumentNullException(nameof(gameCycleBetting));
            this.shellExposition = shellExposition ?? throw new ArgumentNullException(nameof(shellExposition));
            this.midGameMaxBetBehavior = midGameMaxBetBehavior;
        }

        #endregion

        #region ILogicalOperand<Amount> Implementation

        /// <inheritdoc/>
        public bool Execute(Amount data)
        {
            var result = false;

            // 0 value always returns false.
            if(data.BaseValue != 0)
            {
                switch(gameCyclePlay.GameCycleState)
                {
                    case GameCycleState.Playing:
                    {
                        checked
                        {
                            var maxBetLimitOkay = midGameBetLimits.MaxBet == 0;

                            if(!maxBetLimitOkay)
                            {
                                var accumulatedBet = midGameMaxBetBehavior == MidGameMaxBetBehavior.LimitByTotalOfMidGame
                                                         ? gameCycleBetting.GetAccumulatedMidGameBet()
                                                         : gameCycleBetting.GetAccumulatedMidGameBet() + gameCycleBetting.GetStartingBet();

                                maxBetLimitOkay = accumulatedBet + data.BaseValue <= midGameBetLimits.MaxBet;
                            }

                            result = maxBetLimitOkay &&
                                 midGameBetLimits.MinAllows(data.BaseValue) &&
                                 data.BaseValue <= shellExposition.BankProperties.PlayerBettableMeter;
                        }

                        break;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}