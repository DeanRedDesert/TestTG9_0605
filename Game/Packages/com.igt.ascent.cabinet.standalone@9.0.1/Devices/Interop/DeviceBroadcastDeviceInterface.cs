//-----------------------------------------------------------------------
// <copyright file = "DeviceBroadcastDeviceInterface.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;
    using System.Runtime.InteropServices;

#pragma warning disable 1591

    /// <summary>
    /// DEV_BROADCAST_DEVICEINTERFACE
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), Serializable]
    internal struct DeviceBroadcastDeviceInterface
    {
        public int Size;
        public int Devicetype;
        public int Reserved;
        public Guid ClassGuid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string Name;
    }

    #pragma warning restore 1591
}
