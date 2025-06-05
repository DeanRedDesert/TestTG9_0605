//-----------------------------------------------------------------------
// <copyright file = "UsbStringDescriptorRequest.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Runtime.InteropServices;

#pragma warning disable 1591

    /// <summary>
    /// Request for String Descriptor type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class UsbStringDescriptorRequest
    {
        public UsbDescriptorType DescriptorType;
        public ushort LanaguageId;
        public byte ConfigurationIndex;
        public byte Reserved;
    }

    #pragma warning restore 1591
}
