//-----------------------------------------------------------------------
// <copyright file = "UsbInterfaceDescriptor.cs" company = "IGT">
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
    public class UsbInterfaceDescriptor
    {
        public byte Length;
        public UsbDescriptorType DescriptorType;
        public byte InterfaceNumber;
        public byte AlternateSetting;
        public byte NumberEndPoints;
        public byte InterfaceClass;
        public byte InterfaceSubClass;
        public byte InterfaceProtocol;
        public byte InterfaceIndex;

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("UsbInterfaceDescriptor -");
            builder.AppendLine("\t Length = " + Length);
            builder.AppendLine("\t Descriptor Type = " + DescriptorType);
            builder.AppendLine("\t Interface Number = " + InterfaceNumber);
            builder.AppendLine("\t Alternate Setting = " + AlternateSetting);
            builder.AppendLine("\t # of End Points = " + NumberEndPoints);
            builder.AppendLine("\t Interface Class = " + InterfaceClass);
            builder.AppendLine("\t Interface Sub Class = " + InterfaceSubClass);
            builder.AppendLine("\t Interface Protocol = " + InterfaceProtocol);
            builder.AppendLine("\t Interface Index = " + InterfaceIndex);

            return builder.ToString();
        }
    }

    #pragma warning restore 1591
}
