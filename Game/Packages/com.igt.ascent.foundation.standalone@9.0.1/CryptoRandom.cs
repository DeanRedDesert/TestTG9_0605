//-----------------------------------------------------------------------
// <copyright file = "CryptoRandom.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides an adapter from <see cref="RNGCryptoServiceProvider" /> to the <see cref="Random" /> interface.
    /// This class is not thread-safe.
    /// To implement thread-safety simply add locks around modifications to <see cref="bufferPosition" />.
    /// </summary>
    internal class CryptoRandom : Random
    {
        /// <summary>
        /// The cryptographically secure random number generator.
        /// </summary>
        private readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        /// <summary>
        /// The default size for the buffer pool.
        /// </summary>
        public const uint DefaultBufferPoolSize = 512;

        /// <summary>
        /// The buffer pool for storing the random bytes.
        /// Improves performance over calling GetBytes() from <see cref="RNGCryptoServiceProvider"/> every time.
        /// </summary>
        private readonly byte[] rngBuffer;

        /// <summary>
        /// The current read position in the buffer.
        /// </summary>
        private int bufferPosition;

        /// <summary>
        /// Gets a value indicating the length of the buffer pool.
        /// </summary>
        public int BufferPoolSize => rngBuffer.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandom" /> class with the <see cref="DefaultBufferPoolSize"/>.
        /// This overload uses <see cref="DefaultBufferPoolSize" />.
        /// </summary>
        public CryptoRandom() : this(DefaultBufferPoolSize) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandom" /> class with a given buffer length.
        /// </summary>
        /// <param name="bufferLength">A larger value (divisible by 4) will result in increased performance.</param>
        public CryptoRandom(uint bufferLength)
        {
            rngBuffer = new byte[bufferLength];
            ResetBuffer();
        }

        /// <summary>
        /// Fills the buffer with random bytes and resets <see cref="bufferPosition"/>.
        /// </summary>
        private void ResetBuffer()
        {
            rng.GetBytes(rngBuffer);
            bufferPosition = 0;
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero and less than <see cref="Int32.MaxValue" />.
        /// </returns>
        public override int Next()
        {
            // Mask away the sign bit so that we always return nonnegative integers
            return (int)GetRandomUInt32() & 0x7FFFFFFF;
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to zero.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals zero, <paramref name="maxValue"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="maxValue"/> is less than zero.
        /// </exception>
        public override int Next(int maxValue)
        {
            if(maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue));
            }

            return Next(0, maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="minValue"/> is greater than <paramref name="maxValue"/>.
        /// </exception>
        public override int Next(int minValue, int maxValue)
        {
            if(minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue));
            }

            if(minValue == maxValue)
            {
                return minValue;
            }

            // "redundant" typecasts required for when the diff would be greater than int.MaxValue
            // since the diff will always be positive we can directly cast rather than use Math.ABS()
            var diff = (ulong)(maxValue - (long)minValue);
            const ulong range32Bits = 0x100000000L;
            var minAcceptableRemainder = range32Bits % diff;
            ulong product;

            // ensure that the number is not skewed towards the lower end of the range.
            do
            {
                var rand = GetRandomUInt32();
                product = rand * diff;
            } while(product % range32Bits < minAcceptableRemainder);

            return minValue + (int)(product >> 32);
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number in the range [0.0, 1.0).
        /// </returns>
        public override double NextDouble()
        {
            // TODO improve precision by using UInt64 which also changes the range to [0.0, 1.0]?
            return GetRandomUInt32() / (1.0 + uint.MaxValue);
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="buffer"/> is null.
        /// </exception>
        public override void NextBytes(byte[] buffer)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            // Can we fit the requested number of bytes in the buffer?
            if(rngBuffer.Length >= buffer.Length)
            {
                var count = buffer.Length;

                EnsureRandomBuffer(count);

                Buffer.BlockCopy(rngBuffer, bufferPosition, buffer, 0, count);

                bufferPosition += count;
            }
            else
            {
                // Draw bytes directly from the RNGCryptoProvider
                rng.GetBytes(buffer);
            }
        }

        /// <summary>
        /// Gets one random unsigned 32bit integer in a thread safe manner.
        /// </summary>
        private uint GetRandomUInt32()
        {
            EnsureRandomBuffer(4);

            var rand = BitConverter.ToUInt32(rngBuffer, bufferPosition);

            bufferPosition += 4;

            return rand;
        }

        /// <summary>
        /// Ensures that we have enough bytes in the random buffer.
        /// </summary>
        /// <param name="requiredBytes">The number of required bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="requiredBytes"/> is greater than <see cref="BufferPoolSize"/>.
        /// </exception>
        private void EnsureRandomBuffer(int requiredBytes)
        {
            if(requiredBytes > rngBuffer.Length)
                throw new ArgumentOutOfRangeException(nameof(requiredBytes), "cannot be greater than random buffer");

            if(rngBuffer.Length - bufferPosition < requiredBytes)
                ResetBuffer();
        }
    }
}
