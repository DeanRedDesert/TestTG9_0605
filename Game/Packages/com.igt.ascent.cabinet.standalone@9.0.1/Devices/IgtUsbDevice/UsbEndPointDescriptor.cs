//-----------------------------------------------------------------------
// <copyright file = "UsbEndPointDescriptor.cs" company = "IGT">
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
    public class UsbEndPointDescriptor
    {
        public byte  Length;
        public UsbDescriptorType DescriptorType;
        public byte  EndPointAddress;
        public byte  Attributes;
        public ushort MaxPacketSize;
        public byte  PollingInterval;

        /// <summary>
        /// Check whether the end point is for interrupt in communication.
        /// </summary>
        /// <devdoc>
        /// This function should have been part of an EndPointAttributeDecoder
        /// and an EndPointAddressDecoder.  But since we don't need the fully
        /// decoded attributes and address for now, we implement this simple
        /// method to get the single piece of information we care.
        /// </devdoc>
        /// <returns>
        /// True if the end point is for interrupt in, false otherwise.
        /// </returns>
        public bool IsInterruptIn()
        {
            const byte maskInterrupt = 0x03;
            const byte maskDirectionIn = 0x80;

            var isInterrupt = (Attributes & maskInterrupt) == maskInterrupt;
            var isDirectionIn = (EndPointAddress & maskDirectionIn) == maskDirectionIn;

            return isInterrupt && isDirectionIn;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("UsbEndPointDescriptor -");
            builder.AppendLine("\t Length = " + Length);
            builder.AppendLine("\t Descriptor Type = " + DescriptorType);
            builder.AppendLine("\t End Point Address = " + EndPointAddress);
            builder.AppendLine("\t Attributes = " + Attributes);
            builder.AppendLine("\t Max Packet Size = " + MaxPacketSize);
            builder.AppendLine("\t Polling Interval = " + PollingInterval);

            return builder.ToString();
        }
    }

    #pragma warning restore 1591
}
