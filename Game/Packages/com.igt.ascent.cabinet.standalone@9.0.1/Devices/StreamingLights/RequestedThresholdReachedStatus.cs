//-----------------------------------------------------------------------
// <copyright file = "RequestedThresholdReachedStatus.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;
    using IgtUsbDevice;

    /// <summary>
    /// The requested threshold reached status message.
    /// </summary>
    internal class RequestedThresholdReachedStatus
    {
        /// <summary>
        /// Construct a new instance using the status data.
        /// </summary>
        /// <param name="status">The status data to decode.</param>
        /// <exception cref="InsufficientDataBufferException">
        /// Thrown if <paramref name="status"/> is less than three bytes long.
        /// </exception>
        public RequestedThresholdReachedStatus(byte[] status)
        {
            if(status.Length < 3)
            {
                throw new InsufficientDataBufferException("Invalid status buffer length.");
            }

            UniqueId = status[0];
            NumberOfFrames = BitConverter.ToUInt16(status, 1);
        }

        /// <summary>
        /// The unique ID from the real time light command.
        /// </summary>
        public byte UniqueId
        {
            get;
        }

        /// <summary>
        /// The number of frames that can fit into the device queue.
        /// </summary>
        public ushort NumberOfFrames
        {
            get;
        }
    }
}
