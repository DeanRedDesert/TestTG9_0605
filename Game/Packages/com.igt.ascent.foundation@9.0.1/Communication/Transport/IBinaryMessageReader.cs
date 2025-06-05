//-----------------------------------------------------------------------
// <copyright file = "IBinaryMessageReader.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;

    /// <summary>
    /// An object that reads the contents of a message from a buffer.
    /// </summary>
    public interface IBinaryMessageReader
    {
        /// <summary>
        /// Gets the length of the underlying buffer.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the position within the buffer that this reader has read up to.
        /// </summary>
        int Position { get; set; }

        /// <summary>
        /// Gets the underlying byte array.
        /// </summary>
        /// <returns>A reference to the underlying byte array.</returns>
        byte[] GetBytes();

        /// <summary>
        /// Reads a segment of the given type from the current position.
        /// </summary>
        /// <typeparam name="TSegment">The segment type to retrieve.</typeparam>
        /// <returns>The segment with the given type.</returns>
        TSegment Read<TSegment>() where TSegment : IBinaryMessageSegment, new();

        /// <summary>
        /// Reads from the current position using the provided segment.
        /// </summary>
        /// <param name="segment">The segment to read with.</param>
        /// <returns>The provided segment, with the message data that was read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="segment"/> is <b>null</b>.
        /// </exception>
        TSegment ReadWith<TSegment>(TSegment segment) where TSegment : IBinaryMessageSegment;
    }
}
