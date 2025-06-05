//-----------------------------------------------------------------------
// <copyright file = "MoneyLocation.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// The MoneyLocation enumeration is used to represent the places where
    /// money can be held.  It could be used to indicate the origination and
    /// destination of a money movement.
    /// </summary>
    [Serializable]
    public enum MoneyLocation
    {
        /// <summary>
        /// No location is specified, either invalid, or don't care.
        /// </summary>
        Unknown,

        /// <summary>
        /// The Player Wagerable Meter.
        /// </summary>
        PlayerWagerableMeter,

        /// <summary>
        /// The Player Bank Meter.
        /// </summary>
        PlayerBankMeter,
    }
}
