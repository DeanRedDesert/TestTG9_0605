// -----------------------------------------------------------------------
// <copyright file = "BankPropertiesUpdateEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using System.Text;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// Event notifying that one or more of bank properties have changed.
    /// </summary>
    [Serializable]
    public class BankPropertiesUpdateEventArgs : NonTransactionalEventArgs
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
        /// Gets the amount of money that is available for player betting, in base units.
        /// If null, the value has not changed since last time it was queried/updated.
        /// </summary>
        public long? PlayerBettableMeter { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BankPropertiesUpdateEventArgs"/>.
        /// </summary>
        /// <param name="canBet">
        /// The nullable flag indicating whether the player can bet.
        /// </param>
        /// <param name="canCommitGameCycle">
        /// The nullable flag indicating whether the player can commit a game-cycle.
        /// </param>
        /// <param name="playerBettableMeter">
        /// The nullable amount of money that is available for player betting, in base units.
        /// </param>
        /// <remarks>
        /// An argument being null means that the value has not been changed since last time it was queried/updated.
        /// </remarks>
        public BankPropertiesUpdateEventArgs(bool? canBet, bool? canCommitGameCycle, long? playerBettableMeter)
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

            builder.AppendLine("BankPropertiesUpdateEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t CanBet: " + CanBet);
            builder.AppendLine("\t CanCommitGameCycle: " + CanCommitGameCycle);
            builder.AppendLine("\t PlayerBettableMeter: " + PlayerBettableMeter);

            return builder.ToString();
        }

        #endregion
    }
}
