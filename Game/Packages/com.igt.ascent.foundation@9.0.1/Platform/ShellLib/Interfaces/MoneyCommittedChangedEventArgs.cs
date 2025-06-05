// -----------------------------------------------------------------------
// <copyright file = "MoneyCommittedChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying that the total amount committed (for placing starting bets) has been changed.
    /// The current "total committed" amount is provided.
    /// </summary>
    [Serializable]
    public sealed class MoneyCommittedChangedEventArgs : MoneyChangedEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the current total amount committed to bet, in base units.
        /// </summary>
        public long CommittedAmount { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="MoneyCommittedChangedEventArgs"/>.
        /// </summary>
        /// <param name="committedAmount">
        /// The current amount total committed to bet, in base units.
        /// </param>
        /// <param name="gamingMeters">
        /// All the gaming meters with the post-change values.
        /// </param>
        public MoneyCommittedChangedEventArgs(long committedAmount, GamingMeters gamingMeters)
            : base(MoneyChangedEventType.MoneyCommittedChanged, gamingMeters)
        {
            CommittedAmount = committedAmount;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(base.ToString());
            builder.AppendLine("\t CommittedAmount: " + CommittedAmount);

            return builder.ToString();
        }

        #endregion
    }
}
