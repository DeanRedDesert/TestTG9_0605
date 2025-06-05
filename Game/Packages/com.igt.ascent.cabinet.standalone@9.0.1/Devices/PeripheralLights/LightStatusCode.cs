//-----------------------------------------------------------------------
// <copyright file = "LightStatusCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;

    /// <summary>
    /// The status code in the UsbStatusMessage received from
    /// the light device driver.
    /// </summary>
    [Serializable]
    internal enum LightStatusCode : ushort
    {
        #region Statuses

        /// <summary>
        /// Sequence completed successfully.
        /// </summary>
        SequenceComplete = 0x0000,

        /// <summary>
        /// Invalid group specific data in a command to the light group.
        /// Reported if the device could not parse the command quickly
        /// enough to reply with "message rejected".
        /// </summary>
        InvalidGroupSpecificData = 0x0100,

        #endregion

        #region Tilts

        /// <summary>
        /// Reserved.  Do not use.
        /// </summary>
        Reserved = 0x8000,

        /// <summary>
        /// Hardware error on the light group.
        /// </summary>
        HardwareError = 0x8100,

        /// <summary>
        /// The light group was tracking another device and
        /// that device tilted.
        /// </summary>
        TrackedDeviceTilted = 0x8200

        #endregion
    }
}
