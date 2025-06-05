// -----------------------------------------------------------------------
// <copyright file = "FoundationBettingPermit.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.FoundationBettingPermit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Platform.GameLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Game.Core.Money;
    using Interfaces;
    using LogicalOperations;

    /// <summary>
    /// An implementation if <see cref="IBettingPermit"/> that queries Foundation
    /// for can-bet logic.
    /// </summary>
    public class FoundationBettingPermit : IBettingPermit
    {
        #region Private Fields

        /// <summary>
        /// The reference to game lib for communicating with Foundation.
        /// </summary>
        private readonly IGameLib gameLib;

        /// <summary>
        /// The can-bet operator that manages some game side check.
        /// </summary>
        protected ILogicalOperator<Amount> PermitOperator { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="FoundationBettingPermit"/>.
        /// </summary>
        /// <param name="gameLib">
        /// The reference to game lib for communicating with Foundation.
        /// </param>
        public FoundationBettingPermit(IGameLib gameLib)
        {
            this.gameLib = gameLib ?? throw new ArgumentNullException(nameof(gameLib));

            GameDenomination = gameLib.GameDenomination;

            // Ideally CanBetPerState could be another operand.  However, since we want to consolidate
            // the F2L CanCommitBet calls into one, we cannot make it an operand, but implement it as
            // a private method instead.

            PermitOperator = new LogicalAnd<Amount>(new List<ILogicalOperand<Amount>>
                                                        {
                                                            new CanBetPerLimits(gameLib.GameMaxBet, gameLib.GameMinBet),
                                                        });
        }

        #endregion

        #region IBettingPermit Implementation

        /// <inheritdoc/>
        public long GameDenomination { get; }

        /// <inheritdoc/>
        public bool CanBet(long betInCredits)
        {
            return CanBetPerState(betInCredits, GameDenomination);
        }

        /// <inheritdoc/>
        public IEnumerable<bool> CanBet(IEnumerable<long> betsInCredits)
        {
            var betCreditList = betsInCredits.ToList();

            return CanBetPerState(betCreditList, GameDenomination);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if a bet amount can be made in current Game Mode and Game Cycle State,
        /// query the Foundation as needed.
        /// </summary>
        /// <param name="betCredit">
        /// The bet amount in units of <paramref name="denomination"/>.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the bet credit.
        /// </param>
        /// <returns>
        /// True if the bet amount can be made; False otherwise.
        /// </returns>
        private bool CanBetPerState(long betCredit, long denomination)
        {
            var result = false;

            switch(gameLib.GameContextMode)
            {
                case GameMode.Utility:
                    {
                        // Anything can be committed in utility mode.
                        result = true;
                        break;
                    }
                case GameMode.Play:
                    {
                        var gameCycleState = gameLib.QueryGameCycleState();

                        switch(gameCycleState)
                        {
                            case GameCycleState.Idle:
                            case GameCycleState.Committed:
                                {
                                    result = OkayByPermitOperator(betCredit, denomination) &&
                                             gameLib.CanCommitBet(betCredit, denomination);
                                    break;
                                }
                            case GameCycleState.MainPlayComplete:
                                {
                                    result = OkayByPermitOperator(betCredit, denomination) && 
                                             gameLib.CanBetNextGameCycle(betCredit, denomination);
                                    break;
                                }
                        }
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// Checks if a bet amount can be made in current Game Mode and Game Cycle State,
        /// query the Foundation as needed.
        /// </summary>
        /// <param name="betCredits">
        /// The list of bet amounts, in units of <paramref name="denomination"/>.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the bet credit.
        /// </param>
        /// <returns>
        /// A list of Boolean flags in which each Boolean value correlates to a bet passed in.
        /// </returns>
        private IEnumerable<bool> CanBetPerState(IList<long> betCredits, long denomination)
        {
            IEnumerable<bool> result;

            switch(gameLib.GameContextMode)
            {
                case GameMode.Utility:
                    {
                        // Anything can be committed in utility mode.
                        result = betCredits.Select(bet => true);
                        break;
                    }
                case GameMode.Play:
                    {
                        var gameCycleState = gameLib.QueryGameCycleState();

                        switch(gameCycleState)
                        {
                            case GameCycleState.Idle:
                            case GameCycleState.Committed:
                                {
                                    var operatorResults = betCredits.Select(bet => OkayByPermitOperator(bet, denomination));
                                    var gameLibResults = gameLib.CanCommitBets(betCredits, denomination);
                                    result = operatorResults.Zip(gameLibResults, (a, b) => a && b);
                                    break;
                                }
                            case GameCycleState.MainPlayComplete:
                                {
                                    var operatorResults = betCredits.Select(bet => OkayByPermitOperator(bet, denomination));
                                    var gameLibResults = gameLib.CanBetNextGameCycle(betCredits, denomination);
                                    result = operatorResults.Zip(gameLibResults, (a, b) => a && b);
                                    break;
                                }
                            default:
                                {
                                    result = betCredits.Select(bet => false);
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        result = betCredits.Select(bet => false);
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// Checks if a bet amount can be made per <see cref="PermitOperator"/>.
        /// </summary>
        /// <param name="betCredit">
        /// The bet amount in units of <paramref name="denomination"/>.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the bet credit.
        /// </param>
        /// <returns>
        /// True if <see cref="PermitOperator"/> not specified, or it is specified and allows bet amount;
        /// False otherwise.
        /// </returns>
        private bool OkayByPermitOperator(long betCredit, long denomination)
        {
            return PermitOperator == null ||
                   PermitOperator.Execute(Amount.FromGameCredits(betCredit, denomination));
        }

        #endregion
    }
}