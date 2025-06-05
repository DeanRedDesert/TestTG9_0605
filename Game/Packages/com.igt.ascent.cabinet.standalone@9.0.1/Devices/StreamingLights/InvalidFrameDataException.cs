//-----------------------------------------------------------------------
// <copyright file = "InvalidFrameDataException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;

    /// <summary>
    /// Exception for when the frame data sent to the device is rejected as invalid.
    /// </summary>
    [Serializable]
    public class InvalidFrameDataException : Exception
    {
        private const string FrameNumberMessage = "Command ID {0} had an invalid frame at position {1}.";
        private const string RleErrorMessage = "Command ID {0} had an RLE decompression error of type {1}.";

        /// <summary>
        /// Construct a new instance given the command ID and frame number.
        /// </summary>
        /// <param name="commandId">The command ID that is associated with the frame error.</param>
        /// <param name="frameNumber">The frame number in the hardware device queue that is invalid.</param>
        public InvalidFrameDataException(byte commandId, ushort frameNumber)
            : base(string.Format(FrameNumberMessage, commandId, frameNumber))
        {
            CommandId = commandId;
            FrameNumber = frameNumber;
        }

        /// <summary>
        /// Construct a new instance given the command ID and the RLE error.
        /// </summary>
        /// <param name="commandId">The command ID that is associated with the frame error.</param>
        /// <param name="rleError">The RLE error encountered.</param>
        public InvalidFrameDataException(byte commandId, RleDecompressionError.ErrorCode rleError)
            : base(string.Format(RleErrorMessage, commandId, rleError))
        {
            CommandId = commandId;
            RleError = rleError;
        }

        /// <summary>
        /// Gets the command ID associated with the exception.
        /// </summary>
        public byte CommandId { get; private set; }

        /// <summary>
        /// Gets the frame number in the device buffer that generated the error. (If available)
        /// </summary>
        public ushort? FrameNumber { get; private set; }

        /// <summary>
        /// Gets the RLE error associated with the frame. (If available)
        /// </summary>
        public RleDecompressionError.ErrorCode? RleError { get; private set; }
    }
}