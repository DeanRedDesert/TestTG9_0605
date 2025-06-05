// -----------------------------------------------------------------------
// <copyright file = "CanBetAllPerGameMode.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.GameBettingPermit
{
    using Communication.Platform.Interfaces;
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// A game side can-bet operand that allows all bet amounts per game mode.
    /// This operand does not care about the individual bet amount.
    /// </summary>
    public sealed class CanBetAllPerGameMode : ICanBetOperand
    {
        #region Private Fields

        /// <summary>
        /// The game mode of current game context.
        /// </summary>
        private readonly GameMode gameMode;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CanBetAllPerGameMode"/>.
        /// </summary>
        /// <param name="gameMode">
        /// The game mode of current game context.
        /// </param>
        public CanBetAllPerGameMode(GameMode gameMode)
        {
            this.gameMode = gameMode;
        }

        #endregion

        #region ILogicalOperand<Amount> Implementation

        /// <inheritdoc/>
        public bool Execute(Amount data)
        {
            return gameMode == GameMode.Utility;
        }

        #endregion
    }
}