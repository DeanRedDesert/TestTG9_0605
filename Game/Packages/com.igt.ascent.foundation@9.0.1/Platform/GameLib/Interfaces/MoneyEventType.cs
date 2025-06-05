// -----------------------------------------------------------------------
// <copyright file = "MoneyEventType.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// The MoneyEventType enumeration is used to represent the type of a money event.
    /// </summary>
    [Serializable]
    public enum MoneyEventType
    {
        /// <summary>
        /// Notification of money being accepted by the EGM.
        /// </summary>
        MoneyIn,

        /// <summary>
        /// Notification of money leaving the EGM.
        /// </summary>
        MoneyOut,

        /// <summary>
        /// Notification of the player's credit balance being reduced through a bet which was placed without having
        /// been committed. This can happen for mid-game bets. Normal starting bets will result in a
        /// <see cref="MoneyCommittedChanged"/> event instead.
        /// </summary>
        MoneyBet,

        /// <summary>
        /// Notification of the outcome list wins being reflected in the
        /// player meters.
        /// </summary>
        MoneyWon,

        /// <summary>
        /// Notification of the player meters being forcibly set by the Foundation.
        /// </summary>
        MoneySet,

        /// <summary>
        /// Notification of the money transfer between the player wagerable
        /// meter and the player bank meter.
        /// </summary>
        MoneyWagerable,

        /// <summary>
        /// Notification of a change in the committed bet amount.
        /// </summary>
        MoneyCommittedChanged,
    }
}