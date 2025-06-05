// -----------------------------------------------------------------------
// <copyright file = "BankPlayPropertiesUpdateEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying that one or more of bank play properties have changed.
    /// </summary>
    [Serializable]
    public sealed class BankPlayPropertiesUpdateEventArgs : NonTransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the flag indicating whether or not the player can bet.
        /// If null, the value has not changed since last time it was queried/updated.
        /// </summary>
        public bool? CanBet { get; private set; }

        /// <summary>
        /// Gets the flag indicating whether or not the player can commit a game-cycle.
        /// If null, the value has not changed since last time it was queried/updated.
        /// </summary>
        public bool? CanCommitGameCycle { get; private set; }

        /// <summary>
        /// Gets the flag indicating whether or not a player cash-out request is offerable.
        /// If null, the value has not changed since last time it was queried/updated.
        /// </summary>
        public bool? CashoutOfferable { get; private set; }

        /// <summary>
        /// Gets the flag indicating whether or not the player can request money to
        /// be moved to/from "player transferable" to/from "player bettable".
        /// If null, the value has not changed since last time it was queried/updated.
        /// </summary>
        public bool? PlayerBettableTransferOfferable { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="BankPlayPropertiesUpdateEventArgs"/>.
        /// </summary>
        /// <param name="canBet">
        /// The nullable flag indicating whether or not the player can bet.
        /// </param>
        /// <param name="canCommitGameCycle">
        /// The nullable flag indicating whether or not the player can commit a game-cycle.
        /// </param>
        /// <param name="cashoutOfferable">
        /// The nullable flag indicating whether or not a player cash-out request is offerable.
        /// </param>
        /// <param name="playerBettableTransferOfferable">
        /// The nullable flag indicating whether or not the player can request money to
        /// be moved to/from "player transferable" to/from "player bettable".
        /// </param>
        /// <remarks>
        /// An argument being null means that the value has not been changed since last time it was queried/updated.
        /// </remarks>
        public BankPlayPropertiesUpdateEventArgs(bool? canBet,
                                                 bool? canCommitGameCycle,
                                                 bool? cashoutOfferable,
                                                 bool? playerBettableTransferOfferable)
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

            builder.AppendLine("BankPlayPropertiesUpdateEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t CanBet: " + CanBet);
            builder.AppendLine("\t CanCommitGameCycle: " + CanCommitGameCycle);
            builder.AppendLine("\t CashoutOfferable: " + CashoutOfferable);
            builder.AppendLine("\t PlayerBettableTransferOfferable: " + PlayerBettableTransferOfferable);

            return builder.ToString();
        }

        #endregion
    }
}
