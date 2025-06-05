//-----------------------------------------------------------------------
// <copyright file = "UsbSingleByteCommandPayload.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// A USB command payload that has one single <see cref="byte"/> data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class UsbSingleByteCommandPayload : UsbCommandPayload
    {
        private readonly byte data;

        /// <summary>
        /// Initialize an instance of <see cref="UsbSingleByteCommandPayload"/>.
        /// </summary>
        /// <param name="functionCode">The function code of command.</param>
        /// <param name="targetDevice">The id of the device to control.</param>
        /// <param name="data">The payload data.</param>
        public UsbSingleByteCommandPayload(byte functionCode, byte targetDevice, byte data)
            : base(functionCode, targetDevice)
        {
            this.data = data;
        }

        #region IPackable Overrides

        /// <inheritdoc/>
        public override void Pack(Stream stream)
        {
            base.Pack(stream);
            stream.WriteByte(data);
        }

        #endregion
    }
}
