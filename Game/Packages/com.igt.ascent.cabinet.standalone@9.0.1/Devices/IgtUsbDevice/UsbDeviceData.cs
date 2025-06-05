//-----------------------------------------------------------------------
// <copyright file = "UsbDeviceData.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Text;
    using Interop;

#pragma warning disable 1591

    [Serializable]
    public class UsbDeviceData
    {
        public string DriverName { get; set; }
        public byte PortAddress { get; set; }

        public string VendorName { get; set; }
        public string ProductName { get; set; }
        public string InterfaceName { get; set; }
        public string SerialName { get; set; }
        public string FileName { get; set; }

        public UsbDeviceDescriptor DeviceDescriptor { get; set; }

        public UsbConfigurationDescriptor ConfigurationDescriptor { get; private set; }
        public UsbInterfaceDescriptor InterfaceDescriptor { get; private set; }
        public UsbFunctionalDescriptor FunctionalDescriptor { get; private set; }
        public UsbEndPointDescriptor EndPointDescriptor { get; private set; }

        public byte[] FeatureDescriptorData { get; set; }

        /// <summary>
        /// Get the flag indicating whether the device data is for a Feature Zero.
        /// </summary>
        public bool IsFeatureZero => FunctionalDescriptor.FeatureNumber == DeviceFeatureNumber.FeatureZero;

        /// <summary>
        /// Initialize an instance of <see cref="UsbDeviceData"/>
        /// with default field values.
        /// </summary>
        public UsbDeviceData()
        {
            DriverName = string.Empty;
            VendorName = string.Empty;
            ProductName = string.Empty;
            InterfaceName = string.Empty;
            SerialName = string.Empty;
            FileName = string.Empty;

            DeviceDescriptor = new UsbDeviceDescriptor();
            ConfigurationDescriptor = new UsbConfigurationDescriptor();
            InterfaceDescriptor = new UsbInterfaceDescriptor();
            FunctionalDescriptor = new UsbFunctionalDescriptor();
            EndPointDescriptor = new UsbEndPointDescriptor();
        }

        /// <summary>
        /// Map the data from a byte array to various descriptors.
        /// </summary>
        /// <param name="buffer">The serialized data for the descriptors.</param>
        public void SetConfigurationDescriptors(byte[] buffer)
        {
            var offset = 0;

            while(offset < buffer.Length)
            {
                var descriptorLength = buffer[offset];
                var descriptorType = (UsbDescriptorType)buffer[offset + 1];

                switch(descriptorType)
                {
                    case UsbDescriptorType.Configuration:
                        ConfigurationDescriptor = Win32Methods.Unpack<UsbConfigurationDescriptor>(buffer, offset);
                        break;

                    case UsbDescriptorType.Interface:
                        InterfaceDescriptor = Win32Methods.Unpack<UsbInterfaceDescriptor>(buffer, offset);
                        break;

                    case UsbDescriptorType.Functional:
                        FunctionalDescriptor = Win32Methods.Unpack<UsbFunctionalDescriptor>(buffer, offset);
                        break;

                    case UsbDescriptorType.EndPoint:
                        EndPointDescriptor = Win32Methods.Unpack<UsbEndPointDescriptor>(buffer, offset);
                        break;
                }

                offset += descriptorLength;
            }
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("UsbDeviceData -");
            builder.AppendLine("\t Driver Name = " + DriverName);
            builder.AppendLine("\t Driver Address = " + PortAddress);
            builder.AppendLine("\t Vendor Name = " + VendorName);
            builder.AppendLine("\t Product Name = " + ProductName);
            builder.AppendLine("\t Interface Name = " + InterfaceName);
            builder.AppendLine("\t Serial Name = " + SerialName);
            builder.AppendLine("\t File Name = " + FileName);

            builder.AppendLine();
            builder.AppendLine(DeviceDescriptor.ToString());

            builder.AppendLine();
            builder.AppendLine(ConfigurationDescriptor.ToString());

            builder.AppendLine();
            builder.AppendLine(InterfaceDescriptor.ToString());

            builder.AppendLine();
            builder.AppendLine(FunctionalDescriptor.ToString());

            return builder.ToString();
        }
    }

    #pragma warning restore 1591
}
