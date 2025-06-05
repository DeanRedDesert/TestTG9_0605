//-----------------------------------------------------------------------
// <copyright file = "RawBinaryMessageSegment.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;

    /// <summary>
    /// An implementation of <see cref="IBinaryMessageSegment"/> for a raw byte array.
    /// </summary>
    public class RawBinaryMessageSegment : IBinaryMessageSegment
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RawBinaryMessageSegment"/> with a new, empty byte
        /// array of the specified size.
        /// </summary>
        /// <param name="size">The size of the byte array to create.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="size"/> is less than 0.
        /// </exception>
        public RawBinaryMessageSegment(int size)
        {
            if(size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than or equal to 0.");
            }
            Bytes = new byte[size];
        }
        
        /// <summary>
        /// Initializes a new instance of <see cref="RawBinaryMessageSegment"/> with a byte array.
        /// </summary>
        /// <param name="bytes">The byte array that represents this segment.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="bytes"/> is null.
        /// </exception>
        public RawBinaryMessageSegment(byte[] bytes)
        {
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        /// <summary>
        /// Gets the byte array for this segment.
        /// </summary>
        public byte[] Bytes { get; }

        #region IBinaryMessageSegment implementation

        /// <inheritdoc/>
        public int Size => Bytes.Length;

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
            Array.Copy(buffer, startPosition, Bytes, 0, Size);
        }

        /// <inheritdoc/>
        public void Write(byte[] buffer, int startPosition)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), "The buffer cannot be null.");
            }
            if(buffer.Length - startPosition < Size)
            {
                throw new BinaryMessageOverflowException(
                    "Byte array does not have sufficient bytes to contain this segment.", Size, buffer.Length - startPosition,
                    GetType());
            }
            Array.Copy(Bytes, 0, buffer, startPosition, Size);
        }

        #endregion
    }
}