//-----------------------------------------------------------------------
// <copyright file = "DeviceInterfaceDetailData.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;
    using System.Runtime.InteropServices;

#pragma warning disable 1591

    /// <summary>
    /// SP_DEVICE_INTERFACE_DETAIL_DATA
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), Serializable]
    internal struct DeviceInterfaceDetailData
    {
        public int Size;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string DevicePath;
        
        /// <summary>
        /// Create an instance of <see cref="DeviceInterfaceDetailData"/>
        /// based on the type of the operating system.
        /// </summary>
        public static DeviceInterfaceDetailData New()
        {
            return new DeviceInterfaceDetailData
            {
                Size = IntPtr.Size == 8 
                       // For 64 bit operating systems.
                       ? 8
                       // For 32 bit operating systems.
                       : 4 + Marshal.SystemDefaultCharSize,
                DevicePath = string.Empty
            };
        }
    }

    #pragma warning restore 1591
}
