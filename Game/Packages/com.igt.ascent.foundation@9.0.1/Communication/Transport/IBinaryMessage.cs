//-----------------------------------------------------------------------
// <copyright file = "IBinaryMessage.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System.Collections.Generic;

    /// <summary>
    /// A binary message is a sequence of <see cref="IBinaryMessageSegment"/>s that can be written into a byte array.
    /// </summary>
    public interface IBinaryMessage : IEnumerable<IBinaryMessageSegment>
    {
        /// <summary>
        /// Gets the size, in bytes, of the message.
        /// </summary>
        /// <remarks>Use this do determine how large a message buffer needs to be.</remarks>
        int Size { get; }

        /// <summary>
        /// Prepends the given segment to this message.
        /// </summary>
        /// <param name="segment">The segment to prepend.</param>
        void Prepend(IBinaryMessageSegment segment);

        /// <summary>
        /// Writes the binary message into the given buffer.
        /// </summary>
        /// <param name="buffer">The byte array to write the buffer to. Must be large enough to accept the message.</param>
        /// <param name="startPosition">The position within the buffer to begin writing.</param>
        /// <exception cref="BinaryMessageOverflowException">
        /// Thrown if <paramref name="buffer"/> does not contain at least <see cref="Size"/> bytes from the
        /// <paramref name="startPosition"/>.
        /// </exception>
        void Write(byte[] buffer, int startPosition);
    }
}