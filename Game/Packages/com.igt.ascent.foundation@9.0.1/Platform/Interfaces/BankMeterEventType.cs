//-----------------------------------------------------------------------
// <copyright file = "BankMeterEventType.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// An enumeration of different major Bank Events a client might be interested in.
    /// </summary>
    /// 
    public enum BankMeterEventType
    {
        /// <summary>
        /// Any change of the PlayerMeters happened which is not related to cashout (i.e., CashoutStart, CashoutEnd).
        /// The player meters reported along with this event type represent the meters after movement.
        /// </summary>
        CreditMovement,

        /// <summary>
        /// Cashout start. Major Bank Events of all type of cashouts.
        /// The PlayerMeters reported along with this event type represent the meters before cashout started.
        /// </summary>
        CashoutStart,

        /// <summary>
        /// Cashout end. Major Bank Events of all type of cashouts.
        /// The PlayerMeters reported along with this event type represent the meters after cashout.
        /// </summary>
        CashoutEnd
    }
}