// -----------------------------------------------------------------------
// <copyright file = "MoneyOutEventArgs.cs" company = "IGT">
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
    /// Event notifying that money has left the EGM.
    /// </summary>
    [Serializable]
    public sealed class MoneyOutEventArgs : MoneyChangedEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the amount of money that has left the EGM, in base units.
        /// </summary>
        public long MoneyOutAmount { get; private set; }

        /// <summary>
        /// Gets the source that received the outgoing money.
        /// </summary>
        public MoneyOutSource MoneyOutSource { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="MoneyOutEventArgs"/>.
        /// </summary>
        /// <param name="moneyOutAmount">
        /// The amount of money that has left the EGM, in base units.
        /// </param>
        /// <param name="moneyOutSource">
        /// The source that received the outgoing money.
        /// </param>
        /// <param name="gamingMeters">
        /// All the gaming meters with the post-change values.
        /// </param>
        public MoneyOutEventArgs(long moneyOutAmount,
                                 MoneyOutSource moneyOutSource,
                                 GamingMeters gamingMeters)
            : base(MoneyChangedEventType.MoneyOut, gamingMeters)
        {
            MoneyOutAmount = moneyOutAmount;
            MoneyOutSource = moneyOutSource;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(base.ToString());
            builder.AppendLine("\t MoneyOutAmount: " + MoneyOutAmount);
            builder.AppendLine("\t MoneyOutSource: " + MoneyOutSource);

            return builder.ToString();
        }

        #endregion
    }
}
