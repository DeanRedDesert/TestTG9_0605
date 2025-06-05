// -----------------------------------------------------------------------
// <copyright file = "MoneyWonEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying that player has won the money.
    /// </summary>
    [Serializable]
    public sealed class MoneyWonEventArgs : MoneyChangedEventArgs
    {
        #region Properties

        /// <summary>
        /// The amount of money won in base units.
        /// </summary>
        public long MoneyWonAmount { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="MoneyWonEventArgs"/>.
        /// </summary>
        /// <param name="moneyWonAmount">
        /// The amount of money won, in base units.
        /// </param>
        /// <param name="gamingMeters">
        /// All the gaming meters with the post-change values.
        /// </param>
        public MoneyWonEventArgs(long moneyWonAmount, GamingMeters gamingMeters)
            : base(MoneyChangedEventType.MoneyWon, gamingMeters)
        {
            MoneyWonAmount = moneyWonAmount;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(base.ToString());
            builder.AppendLine("\t MoneyWonAmount: " + MoneyWonAmount);

            return builder.ToString();
        }

        #endregion
    }
}
