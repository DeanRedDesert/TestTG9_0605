//-----------------------------------------------------------------------
// <copyright file = "CreditMeterDisplayBehaviorMode.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace  IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// An enum describing the type of credit movement that occurred.
    /// </summary>
    [Serializable]
    public enum CreditMovementType
    {
        /// <summary>
        /// The reason for the change in money is unspecified or unknown.
        /// </summary>
        Unspecified,
    
        /// <summary>
        /// The amount committed for placing bets has changed. This amount
        /// is not permanent and may vary until the bet is placed, or may be returned to the Bank or Wagerable meter.
        /// </summary>
        MoneyCommittedChanged,
    
        /// <summary>
        /// A bet has been placed that results in a reduction of the Bank or Wagerable meter.
        /// </summary>
        MoneyBet,
    
        /// <summary>
        /// Money has been received.
        /// </summary>
        MoneyIn,
    
        /// <summary>
        /// Money has left the EGM.
        /// </summary>
        MoneyOut,
    
        /// <summary>
        /// The player has won money.
        /// </summary>
        MoneyWon,
    
        /// <summary>
        /// The Foundation forcibly set one or more of the player meters to a new value.
        /// </summary>
        MoneySet,
    
        /// <summary>
        /// Money was transferred from the Bank to the Wagerable meter.
        /// </summary>
        TransferToWagerable,
    
        /// <summary>
        /// Money was transferred from the Wagerable to the Bank meter.
        /// </summary>
        TransferFromWagerable
    }
}