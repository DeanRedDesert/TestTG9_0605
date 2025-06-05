// -----------------------------------------------------------------------
// <copyright file = "MoneyInEventArgs.cs" company = "IGT">
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
    /// Event notifying that money has been received.
    /// </summary>
    [Serializable]
    public sealed class MoneyInEventArgs : MoneyChangedEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the amount of received money, in base units.
        /// </summary>
        public long MoneyInAmount { get; private set; }

        /// <summary>
        /// Gets the source of the received money.
        /// </summary>
        public MoneyInSource MoneyInSource { get; private set; }

        /// <summary>
        /// Gets the optional enumeration value that is only present to indicate the funds transfer account
        /// source of the received money when the MoneyInSource is <see cref="Platform.Interfaces.MoneyInSource.FundsTransfer"/>.
        /// </summary>
        public FundsTransferAccountSource? FundsTransferAccountSource { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="MoneyInEventArgs"/>.
        /// </summary>
        /// <param name="moneyInAmount">
        /// The amount of received money, in base units.
        /// </param>
        /// <param name="moneyInSource">
        /// The source of the received money.
        /// </param>
        /// <param name="fundsTransferAccountSource">
        /// The funds transfer account source of the received money.
        /// </param>
        /// <param name="gamingMeters">
        /// All the gaming meters with the post-change values.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="fundsTransferAccountSource"/> is set but <paramref name="moneyInSource"/>
        /// is not <see cref="Platform.Interfaces.MoneyInSource.FundsTransfer"/>.
        /// </exception>
        public MoneyInEventArgs(long moneyInAmount,
                                MoneyInSource moneyInSource,
                                FundsTransferAccountSource? fundsTransferAccountSource,
                                GamingMeters gamingMeters)
            : base(MoneyChangedEventType.MoneyIn, gamingMeters)
        {
            if(fundsTransferAccountSource != null && moneyInSource != MoneyInSource.FundsTransfer)
            {
                throw new ArgumentException($"FundsTransferAccountSource can only be set when MoneyInSource (current {moneyInAmount}) is FundsTransfer.");
            }

            MoneyInAmount = moneyInAmount;
            MoneyInSource = moneyInSource;
            FundsTransferAccountSource = fundsTransferAccountSource;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(base.ToString());
            builder.AppendLine("\t MoneyInAmount: " + MoneyInAmount);
            builder.AppendLine("\t MoneyInSource: " + MoneyInSource);
            builder.AppendLine("\t FundsTransferAccountSource: " + FundsTransferAccountSource);

            return builder.ToString();
        }

        #endregion
    }
}
