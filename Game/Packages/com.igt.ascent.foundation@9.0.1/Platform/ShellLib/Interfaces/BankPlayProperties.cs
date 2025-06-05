// -----------------------------------------------------------------------
// <copyright file = "BankPlayProperties.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// Contains the bank-play related properties.
    /// </summary>
    [Serializable]
    public sealed class BankPlayProperties
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
        /// Gets the flag that indicates if a player cash-out request is offerable.
        /// </summary>
        public bool CashoutOfferable { get; private set; }

        /// <summary>
        /// Gets the flag that indicates if the player may request money to
        /// be moved to/from "player transferable" to/from "player bettable".
        /// </summary>
        public bool PlayerBettableTransferOfferable { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="BankPlayProperties"/>.
        /// </summary>
        /// <param name="canBet">
        /// The flag indicates if the player can bet.
        /// </param>
        /// <param name="canCommitGameCycle">
        /// The flag indicates if the player can commit a game-cycle.
        /// </param>
        /// <param name="cashoutOfferable">
        /// The flag indicates if a player cash-out request is offerable.
        /// </param>
        /// <param name="playerBettableTransferOfferable">
        /// The flag indicates if the player may request money to be moved to/from "player transferable"
        /// to/from "player bettable".
        /// </param>
        public BankPlayProperties(bool canBet,
                                  bool canCommitGameCycle,
                                  bool cashoutOfferable,
                                  bool playerBettableTransferOfferable)
        {
            CanBet = canBet;
            CanCommitGameCycle = canCommitGameCycle;
            CashoutOfferable = cashoutOfferable;
            PlayerBettableTransferOfferable = playerBettableTransferOfferable;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("BankPlayProperties -");
            builder.AppendLine("\t CanBet: " + CanBet);
            builder.AppendLine("\t CanCommitGameCycle: " + CanCommitGameCycle);
            builder.AppendLine("\t CashoutOfferable: " + CashoutOfferable);
            builder.AppendLine("\t PlayerBettableTransferOfferable: " + PlayerBettableTransferOfferable);

            return builder.ToString();
        }

        #endregion
    }
}
