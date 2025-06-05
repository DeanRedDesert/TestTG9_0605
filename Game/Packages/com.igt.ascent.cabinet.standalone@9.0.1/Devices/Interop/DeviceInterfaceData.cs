//-----------------------------------------------------------------------
// <copyright file = "DeviceInterfaceData.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// SP_DEVICE_INTERFACE_DATA
    /// </summary>
    [StructLayout(LayoutKind.Sequential), Serializable]
    internal struct DeviceInterfaceData
    {
        public uint Size;
        public Guid ClassGuid;
        public uint Flags;
        public IntPtr Reserved;

        /// <summary>
        /// Create an instance of <see cref="DeviceInterfaceData"/>,
        /// setting <see cref="Size"/> to correct value.
        /// </summary>
        public static DeviceInterfaceData New()
        {
            return new DeviceInterfaceData
            {
                Size = (uint)Marshal.SizeOf(typeof(DeviceInterfaceData)),
                ClassGuid = new Guid(),
                Flags = 0,
                Reserved = IntPtr.Zero
            };
        }
    }
}
