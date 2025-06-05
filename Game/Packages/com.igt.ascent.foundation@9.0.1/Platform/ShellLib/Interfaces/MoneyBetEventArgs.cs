// -----------------------------------------------------------------------
// <copyright file = "MoneyBetEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying that a bet has been placed without having been committed.
    /// This usually happens for mid-game bets.
    /// </summary>
    [Serializable]
    public sealed class MoneyBetEventArgs : MoneyChangedEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the placed bet amount, in base units.
        /// </summary>
        public long BetAmount { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="MoneyBetEventArgs"/>.
        /// </summary>
        /// <param name="betAmount">
        /// The placed bet amount, in base units.
        /// </param>
        /// <param name="gamingMeters">
        /// All the gaming meters with the post-change values.
        /// </param>
        public MoneyBetEventArgs(long betAmount, GamingMeters gamingMeters)
            : base(MoneyChangedEventType.MoneyBet, gamingMeters)
        {
            BetAmount = betAmount;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(base.ToString());
            builder.AppendLine("\t BetAmount: " + BetAmount);

            return builder.ToString();
        }

        #endregion
    }
}
