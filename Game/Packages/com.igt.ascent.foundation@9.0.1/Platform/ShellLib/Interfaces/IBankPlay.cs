//-----------------------------------------------------------------------
// <copyright file = "IBankPlay.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs for a game to talk to the Foundation in terms of Bank Play, such as
    /// getting the player meters, Bank Play status, money events and machine wide bet constraints etc.
    /// </summary>
    public interface IBankPlay
    {
        /// <summary>
        /// Event occurs when any of player money changed event has happened.
        /// </summary>
        event EventHandler<MoneyChangedEventArgs> MoneyChangedEvent;

        /// <summary>
        /// Event occurs when a bet has been placed without having been committed that results in
        /// a reduction of the player-bettable meter.  This can happen for mid-game bets.
        /// Normal starting bets will result in a <see cref="MoneyCommittedChangedEvent"/> instead.
        /// </summary>
        event EventHandler<MoneyBetEventArgs> MoneyBetEvent;

        /// <summary>
        /// Event occurs when a money transfer to or from the player bettable balance has occurred.
        /// </summary>
        event EventHandler<MoneyBettableTransferEventArgs> MoneyBettableTransferEvent;

        /// <summary>
        /// Event occurs when the total amount committed (for placing starting bets) has been changed.
        /// The current "total committed" amount is provided in the message.
        /// </summary>
        event EventHandler<MoneyCommittedChangedEventArgs> MoneyCommittedChangedEvent;

        /// <summary>
        /// Event occurs when money has been received.
        /// </summary>
        event EventHandler<MoneyInEventArgs> MoneyInEvent;

        /// <summary>
        /// Event occurs when the money has left the EGM.
        /// </summary>
        event EventHandler<MoneyOutEventArgs> MoneyOutEvent;

        /// <summary>
        /// Event occurs when the Foundation forcibly sets one or more of the player meters to a new value.
        /// </summary>
        event EventHandler<MoneySetEventArgs> MoneySetEvent;

        /// <summary>
        /// Event occurs when player has won the money.
        /// </summary>
        event EventHandler<MoneyWonEventArgs> MoneyWonEvent;

        /// <summary>
        /// Event occurs when one or more of Bank Play properties have changed.
        /// </summary>
        event EventHandler<BankPlayPropertiesUpdateEventArgs> BankPlayPropertiesUpdateEvent;

        /// <summary>
        /// Gets the machine wide bet constraints.
        /// </summary>
        MachineWideBetConstraints MachineWideBetConstraints { get; }

        /// <summary>
        /// Gets the flag that indicates that support for player initiated transfers between "player bettable"
        /// and "player transferable" meters are required in order to manage the "player bettable" balance
        /// (e.g. UK "banked-credits" requirements).
        /// </summary>
        bool SupportPlayerTransfersForBettable { get; }

        /// <summary>
        /// Gets all Bank Play related properties.
        /// </summary>
        BankPlayProperties BankPlayProperties { get; }

        /// <summary>
        /// Requests the current player meters.
        /// </summary>
        GamingMeters GamingMeters { get; }
        
        /// <summary>
        /// Requests that available metered funds are returned to the player.
        /// </summary>
        void RequestCashout();
    }
}
