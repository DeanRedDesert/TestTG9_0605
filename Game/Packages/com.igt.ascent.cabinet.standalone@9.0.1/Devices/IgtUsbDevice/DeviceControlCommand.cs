//-----------------------------------------------------------------------
// <copyright file = "DeviceControlCommand.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This data structure represents a command sent to the device.
    /// </summary>
    internal class DeviceControlCommand
    {
        /// <summary>
        /// Command header.
        /// </summary>
        private readonly IgtClassStandardHeader header;

        /// <summary>
        /// Command payload.
        /// </summary>
        private readonly UsbCommandPayload payload;

        /// <summary>
        /// Additional data for the command.
        /// </summary>
        private readonly byte[] data;

        /// <summary>
        /// Initialize an instance of <see cref="DeviceControlCommand"/> with
        /// a given payload and a byte array of additional data.
        /// </summary>
        /// <param name="payload">The command payload.</param>
        /// <param name="data">Additional data for the command.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="payload"/> is null.
        /// </exception>
        public DeviceControlCommand(UsbCommandPayload payload, byte[] data)
        {
            if(payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var totalSize = Marshal.SizeOf(payload) + (data?.Length ?? 0);

            header = new IgtClassStandardHeader((ushort)totalSize);
            this.payload = payload;
            this.data = data;
        }

        /// <summary>
        /// Get a byte array where all the fields of the data structure
        /// are packed consecutively.
        /// </summary>
        /// <returns>Byte array after packing the data structure.</returns>
        public byte[] GetBytes()
        {
            using(var stream = new MemoryStream())
            {
                header.Pack(stream);
                payload.Pack(stream);

                if(data != null)
                {
                    stream.Write(data, 0, data.Length);
                }

                return stream.ToArray();
            }
        }
    }
}
