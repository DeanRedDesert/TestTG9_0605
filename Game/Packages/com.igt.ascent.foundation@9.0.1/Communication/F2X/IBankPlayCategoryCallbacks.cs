//-----------------------------------------------------------------------
// <copyright file = "IBankPlayCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using Schemas.Internal.BankPlay;
    using Schemas.Internal.Types;

    /// <summary>
    /// Interface that handles callbacks from the F2X <see cref="BankPlay"/> category.
    /// Bank Play category of messages for the Shell Bin to retrieve the player bet configuration, bank play status,
    /// player meters, and get notification from the Foundation in the event of bank player status or player money
    /// changed.
    /// Category: 1029; Major Version: 1
    /// </summary>
    /// <remarks>
    /// All documentation is generated from the XSD schema files.
    /// </remarks>
    public interface IBankPlayCategoryCallbacks
    {
        /// <summary>
        /// Notifies that a bet has been placed that results in a reduction of the player-bettable meter.
        /// </summary>
        /// <param name="playerMeters">
        /// 
        /// </param>
        /// <param name="amountBet">
        /// The placed bet amount.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessMoneyBet(PlayerMeters playerMeters, Amount amountBet);

        /// <summary>
        /// Notifies that a money transfer that affects the player bettable balance occurred.
        /// </summary>
        /// <param name="playerMeters">
        /// 
        /// </param>
        /// <param name="amountMoved">
        /// The amount is moved.
        /// </param>
        /// <param name="gameTransferRequestSuccessful">
        /// Optional flag. If present, the MoneyBettableTransfer message is in response to a game initiated transfer
        /// request. If true the game initiated transfer request was successful. If false, the transfer request failed,
        /// and the player meters were NOT changed as a result of the game initiated request.
        /// </param>
        /// <param name="gameTransferRequestSuccessfulSpecified">
        /// 
        /// </param>
        /// <param name="bettableTransferDirection">
        /// Direction of the transfer.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessMoneyBettableTransfer(PlayerMeters playerMeters, Amount amountMoved, bool gameTransferRequestSuccessful, bool gameTransferRequestSuccessfulSpecified, BettableTransferDirection bettableTransferDirection);

        /// <summary>
        /// Notifies that the total amount committed (for placing starting bets) has been changed. The current "total
        /// committed" amount is provided in the message.
        /// </summary>
        /// <param name="playerMeters">
        /// 
        /// </param>
        /// <param name="amountCommitted">
        /// The current amount total committed to bet.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessMoneyCommittedChanged(PlayerMeters playerMeters, Amount amountCommitted);

        /// <summary>
        /// Notifies that money has been received.
        /// </summary>
        /// <param name="playerMeters">
        /// 
        /// </param>
        /// <param name="amountIn">
        /// Amount of received money.
        /// </param>
        /// <param name="moneyInSource">
        /// Source of the received money.
        /// </param>
        /// <param name="fundsTransferAccountSource">
        /// Funds transfer account source of the received money. This information is sent when the MoneyInSource is
        /// "FundsTransfer". In all other cases this value is not sent.
        /// </param>
        /// <param name="fundsTransferAccountSourceSpecified">
        /// 
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessMoneyIn(PlayerMeters playerMeters, Amount amountIn, MoneyInSource moneyInSource, FundsTransferAccountSource fundsTransferAccountSource, bool fundsTransferAccountSourceSpecified);

        /// <summary>
        /// Notifies that the money has left the EGM.
        /// </summary>
        /// <param name="playerMeters">
        /// 
        /// </param>
        /// <param name="amountOut">
        /// Amount of money that has left the EGM.
        /// </param>
        /// <param name="moneyOutSource">
        /// Source that received the outgoing money.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessMoneyOut(PlayerMeters playerMeters, Amount amountOut, MoneyOutSource moneyOutSource);

        /// <summary>
        /// Notifies that the Foundation forcibly set one or more of the player meters to a new value.
        /// </summary>
        /// <param name="playerMeters">
        /// 
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessMoneySet(PlayerMeters playerMeters);

        /// <summary>
        /// Notifies that player has won the money.
        /// </summary>
        /// <param name="playerMeters">
        /// 
        /// </param>
        /// <param name="amountWon">
        /// Amount of money won.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessMoneyWon(PlayerMeters playerMeters, Amount amountWon);

        /// <summary>
        /// Notifies that one or more of bank play properties have changed.
        /// </summary>
        /// <param name="bankPlayProperties">
        /// Contains the bank play properties that are changed **ONLY**.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessUpdateBankPlayProperties(BankPlayProperties bankPlayProperties);

    }

}

