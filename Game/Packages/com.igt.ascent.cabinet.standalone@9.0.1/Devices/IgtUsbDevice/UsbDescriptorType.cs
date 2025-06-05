//-----------------------------------------------------------------------
// <copyright file = "UsbDescriptorType.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

#pragma warning disable 1591

    [Serializable]
    public enum UsbDescriptorType : byte
    {
        Device = 0x01,
        Configuration = 0x02,
        String = 0x03,
        Interface = 0x04,
        EndPoint = 0x05,
        Reserved = 0x06,
        ConfigurationPower = 0x07,
        InterfacePower = 0x08,
        Functional = 0x41,

        /// <summary>
        /// Per IGT USB Specification, 0x42 to 0x5F are for feature descriptors,
        /// but so far for the devices of our interest, i.e. reels, lights and
        /// button panel, only 0x42 is being used.
        /// </summary>
        FeatureSpecific = 0x42
    }

    #pragma warning restore 1591
}
