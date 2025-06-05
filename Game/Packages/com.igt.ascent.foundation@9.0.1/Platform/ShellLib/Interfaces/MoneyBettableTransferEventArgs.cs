// -----------------------------------------------------------------------
// <copyright file = "MoneyBettableTransferEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Event notifying that money has been transferred to or from the player bettable balance.
    /// </summary>
    [Serializable]
    public sealed class MoneyBettableTransferEventArgs : MoneyChangedEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the moved amount, in base units.
        /// </summary>
        public long MoneyMovedAmount { get; private set; }

        /// <summary>
        /// Gets the optional flag that, if present, the MoneyBettableTransfer event is in response to
        /// a game initiated transfer request.
        /// If true the game initiated transfer request was successful. If false, the transfer request
        /// failed, and the player meters were NOT changed as a result of the game initiated request.
        /// </summary>
        public bool? GameTransferRequestSuccessful { get; private set; }

        /// <summary>
        /// Gets the direction of the transfer.
        /// </summary>
        public BettableTransferDirection BettableTransferDirection { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="MoneyBettableTransferEventArgs"/>.
        /// </summary>
        /// <param name="moneyMovedAmount">
        /// The moved amount, in base units.
        /// </param>
        /// <param name="gameTransferRequestSuccessful">
        /// The optional flag to indicates if the game initiated transfer request was successful.
        /// </param>
        /// <param name="bettableTransferDirection">
        /// The direction of the transfer.
        /// </param>
        /// <param name="gamingMeters">
        /// All the gaming meters with the post-change values.
        /// </param>
        public MoneyBettableTransferEventArgs(long moneyMovedAmount,
                                              bool? gameTransferRequestSuccessful,
                                              BettableTransferDirection bettableTransferDirection,
                                              GamingMeters gamingMeters)
            : base(MoneyChangedEventType.MoneyBettableTransfer, gamingMeters)
        {
            MoneyMovedAmount = moneyMovedAmount;
            GameTransferRequestSuccessful = gameTransferRequestSuccessful;
            BettableTransferDirection = bettableTransferDirection;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(base.ToString());
            builder.AppendLine("\t MoneyMovedAmount: " + MoneyMovedAmount);
            builder.AppendLine("\t GameTransferRequestSuccessful: " + GameTransferRequestSuccessful);
            builder.AppendLine("\t BettableTransferDirection: " + BettableTransferDirection);

            return builder.ToString();
        }

        #endregion
    }
}
