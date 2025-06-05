//-----------------------------------------------------------------------
// <copyright file = "UsbSingleUshortCommandPayload.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A USB command payload that has one single <see cref="ushort"/> data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class UsbSingleUshortCommandPayload : UsbCommandPayload
    {
        private readonly ushort data;

        /// <summary>
        /// Initialize an instance of <see cref="UsbSingleUshortCommandPayload"/>.
        /// </summary>
        /// <param name="functionCode">The function code of command.</param>
        /// <param name="targetDevice">The id of the device to control.</param>
        /// <param name="data">The payload data.</param>
        public UsbSingleUshortCommandPayload(byte functionCode, byte targetDevice, ushort data)
            : base(functionCode, targetDevice)
        {
            this.data = data;
        }

        #region IPackable Overrides

        /// <inheritdoc/>
        public override void Pack(Stream stream)
        {
            base.Pack(stream);
            stream.Write(BitConverter.GetBytes(data), 0, sizeof(ushort));
        }

        #endregion
    }
}
