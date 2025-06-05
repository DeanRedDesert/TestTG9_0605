// -----------------------------------------------------------------------
// <copyright file = "BankMeterEventArgs.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// Event arguments for the bank status event notification.
    /// </summary>
    [Serializable]
    public class BankMeterEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Initializes the bank status event arguments.
        /// </summary>
        /// <param name="playerMeters">Player meters.</param>
        /// <param name="eventType">Type of the event.</param>
        public BankMeterEventArgs(IPlayerMeters playerMeters, BankMeterEventType eventType)
        {
            PlayerMeters = playerMeters;
            EventType = eventType;
            CreditMovementType = CreditMovementType.Unspecified;
        }

        /// <summary>
        /// Initializes the bank status event arguments with credit movement data.
        /// </summary>
        /// <param name="playerMeters">Player meters.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="creditMovementType">Describes the type of <see cref="CreditMovementType"/>.</param>
        public BankMeterEventArgs(IPlayerMeters playerMeters, BankMeterEventType eventType, CreditMovementType creditMovementType)
        {
            PlayerMeters = playerMeters;
            EventType = eventType;
            CreditMovementType = creditMovementType;
        }

        /// <summary>
        /// Data of the bank status event - the player meters.
        /// </summary>
        public IPlayerMeters PlayerMeters { get; }

        /// <summary>
        /// Type of the event.
        /// </summary>
        public BankMeterEventType EventType { get; }

        /// <summary>
        /// If specified the <see cref="CreditMovementType"/> associated with this event.
        /// </summary>
        public CreditMovementType CreditMovementType { get; }
    }
}