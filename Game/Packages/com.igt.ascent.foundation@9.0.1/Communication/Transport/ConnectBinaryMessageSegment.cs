//-----------------------------------------------------------------------
// <copyright file = "ConnectBinaryMessageSegment.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;

    /// <summary>
    /// Represents the connect segment of a binary message.
    /// </summary>
    internal sealed class ConnectBinaryMessageSegment : IBinaryMessageSegment
    {
        /// <summary>
        /// Gets the size of this segment in bytes.
        /// </summary>
        public static readonly int SegmentSize = sizeof(byte) + sizeof(byte);

        /// <summary>
        /// Major version of the F2L transport supported.
        /// </summary>
        public byte VersionMajor { get; set; }

        /// <summary>
        /// Minor version of the F2L transport supported.
        /// </summary>
        public byte VersionMinor { get; set; }

        #region IBinaryMessageSegment Implementation

        /// <inheritdoc/>
        public int Size => SegmentSize;

        /// <inheritdoc/>
        public void Read(byte[] buffer, int startPosition)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if(buffer.Length - startPosition < Size)
            {
                throw new BinaryMessageUnderflowException(
                    "Byte array does not have sufficient bytes to fill this segment.", Size, buffer.Length - startPosition,
                    GetType());
            }
            VersionMajor = buffer[startPosition];
            VersionMinor = buffer[startPosition + 1];
        }

        /// <inheritdoc/>
        public void Write(byte[] buffer, int startPosition)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if(buffer.Length - startPosition < Size)
            {
                throw new BinaryMessageOverflowException(
                    "Byte array does not have sufficient bytes to contain this segment.", Size, buffer.Length - startPosition,
                    GetType());
            }
            buffer[startPosition] = VersionMajor;
            buffer[startPosition + 1] = VersionMinor;
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return "VersionMajor: " + VersionMajor + " VersionMinor: " + VersionMinor;
        }
    }
}