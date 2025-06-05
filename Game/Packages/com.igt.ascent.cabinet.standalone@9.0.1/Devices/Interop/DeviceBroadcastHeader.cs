//-----------------------------------------------------------------------
// <copyright file = "DeviceBroadcastHeader.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;
    using System.Runtime.InteropServices;

#pragma warning disable 1591

    /// <summary>
    /// DEV_BROADCAST_HDR
    /// </summary>
    [StructLayout(LayoutKind.Sequential), Serializable]
    internal struct DeviceBroadcastHeader
    {
        public uint Size;
        public uint DeviceType;
        public uint Reserved;
    }

    #pragma warning restore 1591
}
