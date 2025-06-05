//-----------------------------------------------------------------------
// <copyright file = "RealTimeCommandStatus.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;

    /// <summary>
    /// Represents the real time command status message.
    /// </summary>
    internal class RealTimeCommandStatus
    {
        /// <summary>
        /// The different real time command status codes.
        /// </summary>
        public enum StatusCode : byte
        {
            /// <summary>
            /// The status code is invalid.
            /// </summary>
            Invalid = 0xFF,

            /// <summary>
            /// The real time command was accepted.
            /// </summary>
            CommandAccepted = 0x00,

            /// <summary>
            /// The group is busy.
            /// </summary>
            GroupIsBusy = 0x01,

            /// <summary>
            /// The number of frames sent is larger than the device queue.
            /// </summary>
            TooManyFrames = 0x02,

            /// <summary>
            /// The number of frames sent will not fit in the free space
            /// in the device queue.
            /// </summary>
            WillNotFit = 0x04,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            Reserved = 0x08,

            /// <summary>
            /// The frame data is invalid.
            /// </summary>
            FrameDataInvalid = 0x10,

            /// <summary>
            /// The override field is invalid.
            /// </summary>
            OverrideFieldInvalid = 0x20
        }

        /// <summary>
        /// Construct a new instance using the status data.
        /// </summary>
        /// <param name="statusData">The status payload information.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="statusData"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="statusData"/> is less than 8 bytes.
        /// </exception>
        public RealTimeCommandStatus(byte[] statusData)
        {
            const int requiredDataLength = 8;

            if(statusData == null)
            {
                throw new ArgumentNullException(nameof(statusData));
            }

            if(statusData.Length < requiredDataLength)
            {
                throw new ArgumentException(
                    $"statusData contained {statusData.Length} bytes when at least {requiredDataLength} were expected.",
                                            nameof(statusData));
            }

            if(Enum.IsDefined(typeof(StatusCode), statusData[0]))
            {
                Status = (StatusCode)statusData[0];
            }
            else
            {
                Status = StatusCode.Invalid;
            }

            NumberOfFrames = BitConverter.ToUInt16(statusData, 1);
            QueueTotalDuration = BitConverter.ToUInt32(statusData, 3);
            CommandId = statusData[7];
        }

        /// <summary>
        /// The status code of the command.
        /// </summary>
        public StatusCode Status
        {
            get;
        }

        /// <summary>
        /// The number of frames that can fit into the device queue.
        /// If <see cref="Status"/> is set to FrameDataInvalid this property
        /// contains the which frame is invalid.
        /// </summary>
        public ushort NumberOfFrames
        {
            get;
        }

        /// <summary>
        /// The total duration of frames left in the queue expressed in seconds.
        /// </summary>
        public uint QueueTotalDuration
        {
            get;
        }

        /// <summary>
        /// Gets the command ID associated with the real time command.
        /// </summary>
        public byte CommandId
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"Status: {Status} Number of Frames: {NumberOfFrames} Queue Duration: {QueueTotalDuration} Command ID: {CommandId}";
        }
    }
}
