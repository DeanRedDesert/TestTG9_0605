//-----------------------------------------------------------------------
// <copyright file = "LightUtility.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections;

    /// <summary>
    /// Static class holding a collection of utility functions for light control.
    /// </summary>
    public static class LightUtility
    {
        /// <summary>
        /// Create a packed byte array from the passed light data.
        /// Each byte of light data should only have set bits in the number of
        /// least significant bits specified in <paramref name="bitsPerLight"/>.
        /// </summary>
        /// <param name="bitsPerLight">The number of bits to use from each lightData byte.</param>
        /// <param name="lightData">Light data to pack.</param>
        /// <returns>Packed light data.</returns>
        public static byte[] PackBits(byte bitsPerLight, byte[] lightData)
        {
            var packedLength = (int)Math.Ceiling(lightData.Length * bitsPerLight / 8f);
            var packedBytes = new byte[packedLength];
            var totalBits = packedLength * 8;

            var currentBit = totalBits - 1;
            var bitArray = new BitArray(totalBits);

            foreach(var light in lightData)
            {
                //Start at the most significant bit of the data.
                for(var bit = bitsPerLight - 1; bit >= 0; bit--)
                {
                    var sourceBit = (1 << bit) & light;
                    bitArray.Set(currentBit, sourceBit != 0);
                    currentBit--;
                }
            }

            bitArray.CopyTo(packedBytes, 0);
            Array.Reverse(packedBytes);
            return packedBytes;
        }
    }
}
