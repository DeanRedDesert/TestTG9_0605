//-----------------------------------------------------------------------
// <copyright file = "RleDecompressionError.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;

    /// <summary>
    /// Represents the RLE decompression error status message.
    /// </summary>
    public class RleDecompressionError
    {
        /// <summary>
        /// The different decompression error codes.
        /// </summary>
        [Flags]
        public enum ErrorCode
        {
            /// <summary>
            /// The RLE data in the frame defines for more data than there are lights on the device.
            /// </summary>
            MoreDataThanLightsOnDevice = 1,

            /// <summary>
            /// The RLE data is not formatted correctly. For example the repeat count might extend past the amount
            /// of data provided.
            /// </summary>
            RleFormatError = 2
        }

        /// <summary>
        /// Construct a new instance given the status data.
        /// </summary>
        /// <param name="data">The status data to decode.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="data"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="data"/> contains less than 2 bytes.
        /// </exception>
        public RleDecompressionError(byte[] data)
        {
            const int requiredDataLength = 2;

            if(data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if(data.Length < requiredDataLength)
            {
                throw new ArgumentException(
                    $"Data contains {data.Length} bytes when at least {requiredDataLength} bytes were expected.",
                                            nameof(data));
            }

            CommandId = data[0];
            Error = (ErrorCode)data[1];
        }

        /// <summary>
        /// Gets the command ID associated with the frame(s) that generated the error.
        /// </summary>
        public byte CommandId
        {
            get;
        }

        /// <summary>
        /// Gets the decompression error.
        /// </summary>
        public ErrorCode Error
        {
            get;
        }
    }
}
