//-----------------------------------------------------------------------
// <copyright file = "UsbConfigurationDescriptor.cs" company = "IGT">
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
    public class UsbConfigurationDescriptor
    {
        public byte Length;
        public UsbDescriptorType DescriptorType;
        public ushort TotalLength;
        public byte NumberInterfaces;
        public byte ConfigurationValue;
        public byte ConfigurationIndex;
        public byte Attributes;
        public byte MaxPower;

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("UsbConfigurationDescriptor -");
            builder.AppendLine("\t Length = " + Length);
            builder.AppendLine("\t Descriptor Type = " + DescriptorType);
            builder.AppendLine("\t Total Length = " + TotalLength);
            builder.AppendLine("\t # of Interfaces = " + NumberInterfaces);
            builder.AppendLine("\t Configuration Value = " + ConfigurationValue);
            builder.AppendLine("\t Configuration Index = " + ConfigurationIndex);
            builder.AppendLine("\t Attributes = " + Attributes);
            builder.AppendLine("\t Max Power = " + MaxPower);

            return builder.ToString();
        }
    }

    #pragma warning restore 1591
}
