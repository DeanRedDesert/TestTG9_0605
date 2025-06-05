//-----------------------------------------------------------------------
// <copyright file = "ISolenoid.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Interface for Solenoid related functionality of the cabinet.
    /// </summary>
    public interface ISolenoid
    {
        /// <summary>
        /// Sets the handle solenoid to locked.
        /// </summary>
        void SetStateToLocked();

        /// <summary>
        /// Sets the handle solenoid to unlocked.
        /// </summary>
        void SetStateToUnlocked();

        /// <summary>
        /// Clicks the handle solenoid.
        /// </summary>
        void ClickSolenoid();
    }
}
