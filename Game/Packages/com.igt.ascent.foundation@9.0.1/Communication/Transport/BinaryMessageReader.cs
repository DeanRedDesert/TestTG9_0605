//-----------------------------------------------------------------------
// <copyright file = "BinaryMessageReader.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;

    /// <summary>
    /// An implementation of <see cref="IBinaryMessageReader"/>.
    /// </summary>
    public class BinaryMessageReader : IBinaryMessageReader
    {
        private readonly byte[] buffer;

        /// <summary>
        /// Initializes a new instance of <see cref="BinaryMessageReader"/>.
        /// </summary>
        /// <param name="buffer">
        /// The byte array containing a serialized binary message.
        /// </param>
        /// <param name="startPosition">
        /// The position within the buffer to start the reader at.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="buffer"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="startPosition"/> is less than zero or greater than or equal to 
        /// the length of the buffer.
        /// </exception>
        public BinaryMessageReader(byte[] buffer, int startPosition = 0)
        {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            if((uint)startPosition >= Length)
            {
                var message = $"Argument must be greater than or equal to 0 and less than {Length}.";
                throw new ArgumentOutOfRangeException(nameof(startPosition), startPosition, message);
            }
            Position = startPosition;
        }

        #region IBinaryMessageReader implementation

        /// <inheritdoc/>
        public int Length => buffer.Length;

        /// <inheritdoc/>
        public int Position { get; set; }

        /// <inheritdoc/>
        public byte[] GetBytes()
        {
            return buffer;
        }

        /// <inheritdoc/>
        public TSegment Read<TSegment>() where TSegment : IBinaryMessageSegment, new()
        {
            var segment = new TSegment();
            segment.Read(buffer, Position);
            Position += segment.Size;
            return segment;
        }

        /// <inheritdoc />
        public TSegment ReadWith<TSegment>(TSegment segment) where TSegment : IBinaryMessageSegment
        {
            if(segment == null)
            {
                throw new ArgumentNullException();
            }
            segment.Read(buffer, Position);
            Position += segment.Size;
            return segment;
        }

        #endregion
    }
}