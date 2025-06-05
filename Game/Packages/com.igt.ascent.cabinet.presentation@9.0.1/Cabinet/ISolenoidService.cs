// -----------------------------------------------------------------------
// <copyright file = "ISolenoidService.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Cabinet
{
    using CabinetServices;

    /// <summary>
    /// The cabinet service that provides access to solenoid functionalities.
    /// </summary>
    public interface ISolenoidService : ICabinetService
    {
        /// <summary>
        /// Locks the cabinet solenoid. No status results or resulting solenoid states
        /// are available from this call.
        /// </summary>
        void LockSolenoid();

        /// <summary>
        /// Unlocks the cabinet solenoid.  No status results or solenoid states
        /// are available from this call.
        /// </summary>
        void UnlockSolenoid();

        /// <summary>
        /// Clicks the cabinet solenoid.  No status results or solenoid states
        /// are available from this call.
        /// </summary>
        void ClickSolenoid();
    }
}