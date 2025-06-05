//-----------------------------------------------------------------------
// <copyright file = "IPackable.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.IO;

    /// <summary>
    /// This interface defines a method to pack a data structure into a byte stream.
    /// </summary>
    /// <remarks>
    /// All data types that could serve as an input to a device control command
    /// should implement this interface.
    /// </remarks>
    public interface IPackable
    {
        /// <summary>
        /// Pack the data structure into a byte stream.
        /// </summary>
        /// <param name="stream">The stream to which the bytes are written.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        void Pack(Stream stream);
    }
}
