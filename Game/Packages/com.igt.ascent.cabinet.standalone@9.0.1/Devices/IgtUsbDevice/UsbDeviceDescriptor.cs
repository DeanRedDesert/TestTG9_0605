//-----------------------------------------------------------------------
// <copyright file = "UsbDeviceDescriptor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

#pragma warning disable 1591

    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    public class UsbDeviceDescriptor
    {
        public byte Length;
        public UsbDescriptorType DescriptorType;
        public ushort UsbRelease;
        public byte DeviceClass;
        public byte SubClass;
        public byte DeviceProtocol;
        public byte MaxPacketSize;
        public ushort VendorId;
        public ushort ProductId;
        public ushort DeviceRelease;
        public byte ManufacturerIndex;
        public byte ProductIndex;
        public byte SerialNumberIndex;
        public byte NumberConfigurations;

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("UsbDeviceDescriptor -");
            builder.AppendLine("\t Length = " + Length);
            builder.AppendLine("\t Descriptor Type = " + DescriptorType);
            builder.AppendLine("\t Usb Release = " + UsbRelease);
            builder.AppendLine("\t Device Class = " + DeviceClass);
            builder.AppendLine("\t Sub Class = " + SubClass);
            builder.AppendLine("\t Device Protocol = " + DeviceProtocol);
            builder.AppendLine("\t Max Packet Size = " + MaxPacketSize);
            builder.AppendLine("\t Vendor Id = " + VendorId);
            builder.AppendLine("\t Product Id = " + ProductId);
            builder.AppendLine("\t Device Release = " + DeviceRelease);
            builder.AppendLine("\t Manufacturer Index = " + ManufacturerIndex);
            builder.AppendLine("\t Product Index = " + ProductIndex);
            builder.AppendLine("\t Serial Number Index = " + SerialNumberIndex);
            builder.AppendLine("\t # of Configurations = " + NumberConfigurations);

            return builder.ToString();
        }
    }

    #pragma warning restore 1591
}
