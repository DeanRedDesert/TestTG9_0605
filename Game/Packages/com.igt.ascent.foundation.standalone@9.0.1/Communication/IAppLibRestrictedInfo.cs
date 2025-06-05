//-----------------------------------------------------------------------
// <copyright file = "IAppLibRestrictedInfo.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Standalone
{
    using System;

    /// <summary>
    /// AppLib Interface which mimics communication between the Game Manager
    /// and CSI Manager.
    /// </summary>
    public interface IAppLibRestrictedInfo
    {
        /// <summary>
        /// Event which is triggered when the Foundation State changes to Idle or is changed from Idle.
        /// </summary>
        event EventHandler<FoundationStateChangedEventArgs> FoundationStateChangedEvent;

        /// <summary>
        /// Event which is triggered whenever the Player Bank value changes.
        /// </summary>
        event EventHandler<PlayerBankMeterChangedEventArgs> PlayerBankMeterChangedEvent;
    }
}
