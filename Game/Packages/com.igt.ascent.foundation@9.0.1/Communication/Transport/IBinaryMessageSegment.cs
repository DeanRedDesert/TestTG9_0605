//-----------------------------------------------------------------------
// <copyright file = "IBinaryMessageSegment.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;

    /// <summary>
    /// A binary message segment represents part of a binary message. It is used to marshal data from an object into
    /// a buffer, and vice versa.
    /// </summary>
    public interface IBinaryMessageSegment
    {
        /// <summary>
        /// Gets the size (in bytes) of this message segment;
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Writes the contents of this message segment into the buffer, starting at the given index.
        /// </summary>
        /// <param name="buffer">The <see cref="byte"/> array to write the data into.</param>
        /// <param name="startPosition">The position in the byte array to start writing the data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="buffer"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="BinaryMessageOverflowException">
        /// Thrown if <paramref name="buffer"/> does not contain enough space for this segment, from 
        /// <paramref name="startPosition"/> until the end of the buffer.
        /// </exception>
        void Write(byte[] buffer, int startPosition);

        /// <summary>
        /// Reads the contents of this message segment from the buffer, starting at the given index.
        /// </summary>
        /// <param name="buffer">The <see cref="byte"/> array to read the data from.</param>
        /// <param name="startPosition">The position in the byte array to start reading the data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="buffer"/> is <b>null</b>.
        /// </exception>
        /// <exception cref="BinaryMessageUnderflowException">
        /// Thrown if <paramref name="buffer"/> does not contain enough bytes, from <paramref name="startPosition"/>
        /// until the end of the buffer, to fill this segment.
        /// </exception>
        void Read(byte[] buffer, int startPosition);
    }
}
