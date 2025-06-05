//-----------------------------------------------------------------------
// <copyright file = "StandaloneDeviceType.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration
{
    /// <summary>
    /// Enumeration which indicates the type of implementation
    /// that supports a device interface and to be used by
    /// a standalone game.
    /// </summary>
    public enum StandaloneDeviceType
    {
        /// <summary>
        /// The device is not used by the standalone game.
        /// </summary>
        NotUsed,

        /// <summary>
        /// A virtual implementation that simulates device operations
        /// without physical devices being present.
        /// </summary>
        VirtualImplementation,

        /// <summary>
        /// A hardware implementation that controls the physical devices.
        /// </summary>
        HardwareImplementation
    }
}
