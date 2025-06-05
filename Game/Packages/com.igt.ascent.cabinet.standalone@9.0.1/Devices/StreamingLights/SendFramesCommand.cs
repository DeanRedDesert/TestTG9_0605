//-----------------------------------------------------------------------
// <copyright file = "SendFramesCommand.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using IgtUsbDevice;

    /// <summary>
    /// Sends frames to the device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class SendFramesCommand : UsbCommandPayload
    {
        private const byte HeaderVersion = 0;

        private readonly ushort numberOfFrames;
        private readonly ushort frameThreshold;
        private readonly byte uniqueId;
        private readonly byte overrideQueue;
        private readonly byte headerVersion;

        /// <summary>
        /// Create a new instance using the command parameters.
        /// </summary>
        /// <param name="targetDevice">The target device/group for the command.</param>
        /// <param name="numberOfFrames">The number of frames being sent.</param>
        /// <param name="frameThreshold">
        /// The frame free threshold expressed in milliseconds
        /// that will trigger a message by the device for more frames.
        /// </param>
        /// <param name="uniqueCommandId">
        /// The unique ID for this command so it can be identified in the frame queue status
        /// messages from the device..
        /// </param>
        /// <param name="append">
        /// If true append the new frames onto the end of the queue, otherwise clear the queue
        /// before loading the frames.
        /// </param>
        public SendFramesCommand(byte targetDevice, ushort numberOfFrames, ushort frameThreshold,
                                 byte uniqueCommandId, bool append)
            : base((byte)StreamingLightCommandCode.RealTimeLightFrameControl, targetDevice)
        {
            this.numberOfFrames = numberOfFrames;
            this.frameThreshold = frameThreshold;
            uniqueId = uniqueCommandId;
            overrideQueue = (byte)(append ? 0 : 1);

            // This is needed otherwise the message will serialize out to the wrong length
            // when sent to the USB device.
            headerVersion = HeaderVersion;
        }

        #region IPackable Members

        /// <inheritdoc />
        public override void Pack(Stream stream)
        {
            base.Pack(stream);
            stream.Write(BitConverter.GetBytes(numberOfFrames), 0, 2);
            stream.Write(BitConverter.GetBytes(frameThreshold), 0, 2);
            stream.WriteByte(uniqueId);
            stream.WriteByte(overrideQueue);
            stream.WriteByte(headerVersion);
        }

        #endregion
    }
}
