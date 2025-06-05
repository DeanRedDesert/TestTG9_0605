// -----------------------------------------------------------------------
// <copyright file = "SolenoidService.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Cabinet
{
    using CabinetServices;

    /// <summary>
    /// The cabinet service that provides access to solenoid functionalities.
    /// </summary>
    public sealed class SolenoidService : CabinetServiceBase, ISolenoidService
    {
        #region Implementation of ISolenoidService

        /// <inheritdoc />
        public void LockSolenoid()
        {
            VerifyCabinetIsConnected();

            CabinetLib.LockSolenoid();
        }

        /// <inheritdoc />
        public void UnlockSolenoid()
        {
            VerifyCabinetIsConnected();

            CabinetLib.UnlockSolenoid();
        }

        /// <inheritdoc />
        public void ClickSolenoid()
        {
            VerifyCabinetIsConnected();

            CabinetLib.ClickSolenoid();
        }

        #endregion
    }
}