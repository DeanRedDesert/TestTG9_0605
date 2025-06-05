//-----------------------------------------------------------------------
// <copyright file = "UsbCommandPayload.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Payload to IGT class USB control out command.
    /// </summary>
    /// <remarks>
    /// Command payload must have a sequential layout, and align at 1 byte boundary.
    /// This is critical for Marshal.Sizeof to return the correct value when
    /// calculating the payload size in DeviceControlCommand's constructor.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    public class UsbCommandPayload : IPackable
    {
        /// <summary>
        /// Get the function code of command.
        /// </summary>
        public byte FunctionCode { get; private set; }

        /// <summary>
        /// Get the id of the device to which the command is sent.
        /// </summary>
        public byte TargetDevice { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="UsbCommandPayload"/>
        /// with the given function code and target device id.
        /// </summary>
        /// <param name="functionCode">The function code of command.</param>
        /// <param name="targetDevice">The id of the device to which the command is sent.</param>
        public UsbCommandPayload(byte functionCode, byte targetDevice)
        {
            FunctionCode = functionCode;
            TargetDevice = targetDevice;
        }

        #region IPackable Members

        /// <inheritdoc/>
        public virtual void Pack(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.WriteByte(FunctionCode);
            stream.WriteByte(TargetDevice);
        }

        #endregion
    }
}
