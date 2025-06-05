// -----------------------------------------------------------------------
// <copyright file = "BankProperties.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// This class contains bank related properties that are sent to Shell by Foundation.
    /// The values of properties could be updated during an active coplayer context.
    /// </summary>
    [Serializable]
    public class BankProperties
    {
        #region Properties

        /// <summary>
        /// Gets the flag that indicates if the player can bet.
        /// </summary>
        public bool CanBet { get; private set; }

        /// <summary>
        /// Gets the flag that indicates if the player can commit a game-cycle.
        /// </summary>
        public bool CanCommitGameCycle { get; private set; }

        /// <summary>
        /// Gets the amount of money available for player betting that is suitable for display to the player, in base units.
        /// </summary>
        public long PlayerBettableMeter { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="BankProperties"/>.
        /// </summary>
        /// <param name="canBet">
        /// The flag indicates if the player can bet.
        /// </param>
        /// <param name="canCommitGameCycle">
        /// The flag indicates if the player can commit a game-cycle.
        /// </param>
        /// <param name="playerBettableMeter">
        /// The value of player bettable meter, in base units.
        /// </param>
        public BankProperties(bool canBet, bool canCommitGameCycle, long playerBettableMeter)
        {
            CanBet = canBet;
            CanCommitGameCycle = canCommitGameCycle;
            PlayerBettableMeter = playerBettableMeter;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("BankProperties -");
            builder.AppendLine("\t CanBet: " + CanBet);
            builder.AppendLine("\t CanCommitGameCycle: " + CanCommitGameCycle);
            builder.AppendLine("\t PlayerBettableMeter: " + PlayerBettableMeter);

            return builder.ToString();
        }

        #endregion
    }
}