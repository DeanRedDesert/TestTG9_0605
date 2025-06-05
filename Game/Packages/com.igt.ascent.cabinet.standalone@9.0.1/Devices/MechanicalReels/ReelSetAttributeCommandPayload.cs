//-----------------------------------------------------------------------
// <copyright file = "ReelSetAttributeCommandPayload.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using IgtUsbDevice;

    /// <summary>
    /// Payload to Reel Set Attribute command.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class ReelSetAttributeCommandPayload : UsbCommandPayload
    {
        // Cannot use bool to store toEnable, since bool takes up 4 bytes
        // in Marshal.Sizeof method when calculating payload size in
        // DeviceControlCommand's constructor.
        private readonly byte toEnable;
        private readonly ReelAttribute attribute;

        /// <summary>
        /// Initialize an instance of <see cref="ReelSetAttributeCommandPayload"/>.
        /// </summary>
        /// <param name="targetDevice">The reel to control.</param>
        /// <param name="toEnable">True to enable the attribute, false disable.</param>
        /// <param name="attribute">The attribute to set/clear.</param>
        public ReelSetAttributeCommandPayload(byte targetDevice, bool toEnable, ReelAttribute attribute)
            : base((byte)ReelCommandCode.SetAttribute, targetDevice)
        {
            this.toEnable = Convert.ToByte(toEnable);
            this.attribute = attribute;
        }

        #region IPackable Overrides

        /// <inheritdoc/>
        public override void Pack(Stream stream)
        {
            base.Pack(stream);

            stream.WriteByte(toEnable);
            stream.WriteByte((byte)attribute);
        }

        #endregion
    }
}
