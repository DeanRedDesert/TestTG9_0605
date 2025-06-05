//-----------------------------------------------------------------------
// <copyright file = "IStandaloneIdentificationManager.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions
{
    /// <summary>
    /// Defines an interface to retrieve machine identification data when running in standalone mode.
    /// </summary>
    internal interface IStandaloneIdentificationManager
    {
        /// <summary>
        /// Retrieve the serial number for the machine.
        /// </summary>
        /// <returns>The serial number for the machine.</returns>
        /// <remarks>
        /// When running standalone the serial number will be constant with critical data. Clearing critical data will
        /// result in a new serial number.
        /// </remarks>
        string GetMachineSerialNumber();

        /// <summary>
        /// Retrieve the G2S EGM Identifier for the machine.
        /// </summary>
        /// <returns>The G2S EGM Identifier for the machine.</returns>
        /// <remarks>
        /// When running standalone the G2S EGM Identifier will be constant with critical data. Clearing critical data will
        /// result in a new G2S EGM Identifier.
        /// </remarks>
        string GetG2SEgmIdentifier();

        /// <summary>
        /// Retrieve the machine asset number for the machine.
        /// </summary>
        /// <returns>The machine asset number for the machine.</returns>
        /// <remarks>
        /// When running standalone the machine asset number will be constant with critical data. Clearing critical data will
        /// result in a new machine asset number.
        /// </remarks>
        uint GetMachineAssetNumber();

        /// <summary>
        /// Retrieve the floor location for the machine
        /// </summary>
        /// <returns>The floor locationn for the machine.</returns>
        string GetMachineFloorLocation();
    }
}
