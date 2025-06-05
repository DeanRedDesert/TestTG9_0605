// -----------------------------------------------------------------------
// <copyright file = "GameBettingPermit.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.GameBettingPermit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Concurrent.Interfaces;
    using Game.Core.Money;
    using Interfaces;
    using LogicalOperations;

    /// <summary>
    /// An implementation of <see cref="IBettingPermit"/> that manages all can-bet logic on the game side.
    /// </summary>
    public class GameBettingPermit : IBettingPermit
    {
        #region Properties

        /// <summary>
        /// The operator used to check if all bet amounts are allowed.
        /// </summary>
        protected LogicalAnd<Amount> PermitAllOperator { get; set; }

        /// <summary>
        /// The operator used to check if all bet amounts are disallowed.
        /// </summary>
        protected LogicalOr<Amount> PermitNoneOperator { get; set; }

        /// <summary>
        /// The operator used to check if an individual bet amount is allowed.
        /// </summary>
        protected LogicalAnd<Amount> PermitAmountOperator { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Parameter-less constructor for easy initialization by derived classes.
        /// </summary>
        protected GameBettingPermit()
        {
            GameDenomination = 1;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GameBettingPermit"/>.
        /// </summary>
        /// <param name="coplayerLib">
        /// The interface used to communicate with the Foundation.
        /// </param>
        /// <param name="shellExposition">
        /// The interface used to query properties managed by the Shell.
        /// </param>
        /// <param name="pendingWinQuery">
        /// The interface used to query the pending win of current game cycle.
        /// </param>
        /// <param name="allowBettingNextGameCycle">
        /// The optional flag indicating if betting for next game cycle is allowed (usually used by Double Up).
        /// </param>
        /// <param name="allowMidGameBetting">
        /// The optional flag indicating if mid game betting is allowed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the nullable arguments is null.
        /// </exception>
        public GameBettingPermit(ICoplayerLib coplayerLib, IShellExposition shellExposition, IPendingWinQuery pendingWinQuery,
                                 bool allowBettingNextGameCycle = false, bool allowMidGameBetting = false)
        {
            if(coplayerLib == null)
            {
                throw new ArgumentNullException(nameof(coplayerLib));
            }

            if(shellExposition == null)
            {
                throw new ArgumentNullException(nameof(shellExposition));
            }

            if(pendingWinQuery == null)
            {
                throw new ArgumentNullException(nameof(pendingWinQuery));
            }

            PermitAllOperator = new LogicalAnd<Amount>(new CanBetAllPerGameMode(coplayerLib.Context.GameMode));

            PermitNoneOperator = new LogicalOr<Amount>(new CanBetNonePerGameMode(coplayerLib.Context.GameMode),
                                                       new CanBetNonePerBankPlayStatus(shellExposition));

            var gameCycleOrOperator = new LogicalOr<Amount>();
            gameCycleOrOperator.AddOperand(new CanBetStartingBet(shellExposition.MachineWideBetConstraints.StartingBetLimits,
                                                                 coplayerLib.GameCyclePlay,
                                                                 coplayerLib.GameCycleBetting,
                                                                 shellExposition));

            if(allowBettingNextGameCycle)
            {
                gameCycleOrOperator.AddOperand(new CanBetNextGameCycle(shellExposition.MachineWideBetConstraints.StartingBetLimits,
                                                                       coplayerLib.GameCyclePlay,
                                                                       shellExposition,
                                                                       pendingWinQuery));
            }

            if(allowMidGameBetting)
            {
                gameCycleOrOperator.AddOperand(new CanBetMidGameBet(shellExposition.MachineWideBetConstraints.MidGameBetLimits,
                                                                    shellExposition.MachineWideBetConstraints.MidGameMaxBetBehavior,
                                                                    coplayerLib.GameCyclePlay,
                                                                    coplayerLib.GameCycleBetting,
                                                                    shellExposition));
            }

            PermitAmountOperator = new LogicalAnd<Amount>(
                new List<ILogicalOperand<Amount>>
                    {
                        new CanBetPerTokenizedAmount(shellExposition.MachineWideBetConstraints.TokenizedAmount),
                        new CanBetPerPayvarLimits(coplayerLib.GameCycleBetting.ConfigData),
                        gameCycleOrOperator,
                    });

            GameDenomination = coplayerLib.Context.Denomination;
        }

        #endregion

        #region IBettingPermit Implementation

        /// <inheritdoc/>
        public long GameDenomination { get; }

        /// <inheritdoc/>
        public bool CanBet(long betInCredits)
        {
            bool result;

            if(PermitAllOperator?.Execute(null) == true)
            {
                result = true;
            }
            else if(PermitNoneOperator?.Execute(null) == true)
            {
                result = false;
            }
            else if(PermitAmountOperator != null)
            {
                result = PermitAmountOperator.Execute(Amount.FromGameCredits(betInCredits, GameDenomination));
            }
            else
            {
                // If PermitAmountOperator is null, we allow all bets.
                result = true;
            }

            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<bool> CanBet(IEnumerable<long> betsInCredits)
        {
            IEnumerable<bool> result;

            if(PermitAllOperator?.Execute(null) == true)
            {
                result = Enumerable.Repeat(true, betsInCredits.Count());
            }
            else if(PermitNoneOperator?.Execute(null) == true)
            {
                result = Enumerable.Repeat(false, betsInCredits.Count());
            }
            else if(PermitAmountOperator != null)
            {
                result = betsInCredits.Select(bet => PermitAmountOperator.Execute(Amount.FromGameCredits(bet, GameDenomination)));
            }
            else
            {
                // If PermitAmountOperator is null, we allow all bets.
                result = Enumerable.Repeat(true, betsInCredits.Count());
            }

            return result;
        }

        #endregion
    }
}