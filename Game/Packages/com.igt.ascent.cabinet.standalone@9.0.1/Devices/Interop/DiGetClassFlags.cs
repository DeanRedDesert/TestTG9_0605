//-----------------------------------------------------------------------
// <copyright file = "DiGetClassFlags.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;

#pragma warning disable 1591

    /// <summary>
    /// Constant mappings for DIGCF_ flags.
    /// Based on code pulled from http://www.pinvoke.net
    /// </summary>
    [Flags, Serializable]
    internal enum DiGetClassFlags : uint
    {
        Default         = 0x00000001,  // only valid with DeviceInterface
        Present         = 0x00000002,
        AllClasses      = 0x00000004,
        Profile         = 0x00000008,
        DeviceInterface = 0x00000010
    }

    #pragma warning restore 1591
}
