//-----------------------------------------------------------------------
// <copyright file = "BinaryMessage.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A binary message is a sequence of <see cref="IBinaryMessageSegment"/> objects, and is used to write its
    /// contents to a byte array.
    /// </summary>
    public class BinaryMessage : IBinaryMessage
    {
        private readonly LinkedList<IBinaryMessageSegment> segments = new LinkedList<IBinaryMessageSegment>();

        /// <inheritdoc/>
        public int Size { get; private set; }

        /// <summary>
        /// Initializes a new message builder with the given message segments.
        /// </summary>
        /// <param name="segments">An array of binary message segments.</param>
        public BinaryMessage(params IBinaryMessageSegment[] segments)
        {
            foreach(var segment in segments)
            {
                Append(segment);
            }
        }

        /// <summary>
        /// Appends a binary message segment to the segments in this builder.
        /// </summary>
        /// <param name="segment">The segment to append.</param>
        public void Append(IBinaryMessageSegment segment)
        {
            if(segment != null)
            {
                segments.AddLast(segment);
                Size += segment.Size;
            }
        }

        /// <inheritdoc/>
        public void Prepend(IBinaryMessageSegment segment)
        {
            if(segment != null)
            {
                segments.AddFirst(segment);
                Size += segment.Size;
            }
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
                    "The buffer does not contain enough bytes to fill a message for this builder.",
                    Size,
                    buffer.Length - startPosition,
                    segments.Select(segment => segment.GetType()).ToList());
            }
            var position = startPosition;
            foreach(var segment in segments)
            {
                segment.Write(buffer, position);
                position += segment.Size;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var segment in segments)
            {
                sb.Append(segment);
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        /// <inheritdoc/>
        public IEnumerator<IBinaryMessageSegment> GetEnumerator()
        {
            return segments.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}