//-----------------------------------------------------------------------
// <copyright file = "UsbGeneralDescriptorRequest.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Runtime.InteropServices;

#pragma warning disable 1591

    /// <summary>
    /// Request for all descriptor types other than String Descriptor type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class UsbGeneralDescriptorRequest
    {
        public UsbDescriptorType DescriptorType;
        public byte ConfigurationIndex;
        public byte InterfaceIndex;
        public byte AltSetting;
        public byte EndPointIndex;
    }

    #pragma warning restore 1591
}
