//-----------------------------------------------------------------------
// <copyright file = "PlayerBankMeterChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Standalone
{
    using System;

    /// <summary>
    /// Event indicating the Player Bank Amount has has changed.
    /// </summary>
    public class PlayerBankMeterChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for PlayerBankMeterChangedEventArgs.
        /// </summary>
        /// <param name="playerBankAmt">Players Bank Amount.</param>
        public PlayerBankMeterChangedEventArgs(long playerBankAmt)
        {
            PlayerBankAmt = playerBankAmt;
        }

        /// <summary>
        /// Players Bank Amount.
        /// </summary>
        public long PlayerBankAmt { get; }
    }
}
