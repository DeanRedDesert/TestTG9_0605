//-----------------------------------------------------------------------
// <copyright file = "StreamingStatusCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    /// <summary>
    /// The streaming lights status codes.
    /// </summary>
    internal enum StreamingStatusCode : ushort
    {
        /// <summary>
        /// The status of the last received real time light frame command.
        /// </summary>
        RealTimeCommandStatus = 0x0000,

        /// <summary>
        /// Requested number of frames available. This status is sent
        /// if the last real time light frame command sent to the group
        /// was invalid because there was not enough space in the frame queue.
        /// This status means there is now enough room.
        /// </summary>
        RequestedNumberOfFramesAvailable = 0x0100,

        /// <summary>
        /// If the last command sent to the group was invalid because the
        /// group was busy, this status means the group can now accept commands.
        /// </summary>
        GroupReady = 0x0200,

        /// <summary>
        /// The frame threshold in the specified group's queue has now been reached.
        /// </summary>
        RequestedThresholdReached = 0x0300,

        /// <summary>
        /// An error was encountered when decompressing the RLE frame data.
        /// </summary>
        RleDecompressionError = 0x0400,

        /// <summary>
        /// Hardware error.
        /// </summary>
        HardwareError = 0x8000
    }
}
