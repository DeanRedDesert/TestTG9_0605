//-----------------------------------------------------------------------
// <copyright file = "MoneyChangedEventType.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration defines the type of money event.
    /// </summary>
    [Serializable]
    public enum MoneyChangedEventType
    {
        /// <summary>
        /// Money has been received.
        /// </summary>
        MoneyIn,

        /// <summary>
        /// Money has left the EGM.
        /// </summary>
        MoneyOut,

        /// <summary>
        /// A bet has been placed without having been committed.
        /// This usually happens for mid-game bets.
        /// </summary>
        MoneyBet,

        /// <summary>
        /// Player has won the money.
        /// </summary>
        MoneyWon,

        /// <summary>
        /// Foundation has forcibly set one or more of the gaming meters to a new value
        /// </summary>
        MoneySet,

        /// <summary>
        /// Money has been transfered to or from the player bettable balance.
        /// </summary>
        MoneyBettableTransfer,

        /// <summary>
        /// The total amount committed (for placing starting bets) has been changed.
        /// </summary>
        MoneyCommittedChanged
    }
}
