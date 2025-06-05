//-----------------------------------------------------------------------
// <copyright file = "UsbFunctionalDescriptor.cs" company = "IGT">
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
    public class UsbFunctionalDescriptor
    {
        public byte Length;
        public UsbDescriptorType DescriptorType;
        public ushort Version;
        public DeviceFeatureNumber FeatureNumber;
        public ushort SubFeatureNumber;
        public byte NumberAdditionalDescriptors;

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("UsbFunctionalDescriptor -");
            builder.AppendLine("\t Length = " + Length);
            builder.AppendLine("\t Descriptor Type = " + DescriptorType);
            builder.AppendLine("\t Version = " + Version);
            builder.AppendLine("\t Feature Number = " + FeatureNumber);
            builder.AppendLine("\t Sub Feature Number = " + SubFeatureNumber);
            builder.AppendLine("\t # of Additional Descriptors = " + NumberAdditionalDescriptors);

            return builder.ToString();
        }
    }

    #pragma warning restore 1591
}
