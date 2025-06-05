//-----------------------------------------------------------------------
// <copyright file = "IBankStatus.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using IGT.Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs for bank status events.
    /// </summary>
    public interface IBankStatus
    {
        /// <summary>
        /// The event that is triggered when the bank status has changed.
        /// </summary>
        event EventHandler<BankMeterEventArgs> BankMeterEvent;

        /// <summary>
        /// Gets the player meters.
        /// </summary>
        /// <returns>The current player meters.</returns>
        IPlayerMeters GetPlayerMeters();

        /// <summary>
        /// Registers for the type of bank status events a client needs to be notified of.
        /// The registration request overwrites the list of previous registrations.
        /// To clear the current registration, call <see cref="ClearBankMeterEventRegistration"/>.
        /// </summary>
        /// <param name="eventTypes">
        /// List of event types to be registered for.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="eventTypes"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="eventTypes"/> is empty.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown if a transaction is not open when this method is called.
        /// </exception>
        void RegisterBankMeterEvents(IList<BankMeterEventType> eventTypes);

        /// <summary>
        /// Clears any bank meter event registration.
        /// </summary>
        /// <exception cref="InvalidTransactionException">
        /// Thrown if a transaction is not open when this method is called.
        /// </exception>
        void ClearBankMeterEventRegistration();

    }
}