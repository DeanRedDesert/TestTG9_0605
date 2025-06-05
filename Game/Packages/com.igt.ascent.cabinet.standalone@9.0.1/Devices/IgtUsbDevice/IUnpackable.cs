//-----------------------------------------------------------------------
// <copyright file = "IUnpackable.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// This interface provides a data structure with the ability of
    /// being initialized with data contained in a byte array.
    /// </summary>
    /// <remarks>
    /// All data types that could serve as an output of a device control command
    /// should implement this interface.
    /// </remarks>
    public interface IUnpackable
    {
        /// <summary>
        /// Get the total size, in number of bytes, of all un-packable
        /// fields in the data structure.
        /// </summary>
        int DataSize { get; }

        /// <summary>
        /// Unpack a byte array into the data structure.
        /// </summary>
        /// <param name="buffer">
        /// The buffer containing the data to unpack.
        /// </param>
        /// <param name="offset">
        /// The offset into <paramref name="buffer"/> where unpacking
        /// is to start at.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buffer"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="offset"/> is out of range.
        /// </exception>
        /// <exception cref="InsufficientDataBufferException">
        /// Thrown when an error occurred when <paramref name="buffer"/>
        /// is not big enough for unpacking.
        /// </exception>
        void Unpack(byte[] buffer, int offset);
    }
}
