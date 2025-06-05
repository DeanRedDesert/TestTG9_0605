//-----------------------------------------------------------------------
// <copyright file = "CRC16Calculation.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Internals are exposed to Standalone.Devices because CRC16 calculations are needed in the assembly,
// and this class should not be exposed as public.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("IGT.Game.Core.Communication.Cabinet.Standalone.Devices")]
namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Calculates the CRC for light frame data.
    /// </summary>
    /// <remarks>
    /// The CRC specifications come from the IGT Feature 121 specification.
    /// </remarks>
    internal static class Crc16Calculation
    {
        /// <summary>
        /// The polynomial to be used as the divisor.
        /// </summary>
        private const ushort Polynomial = 0x8408;

        /// <summary>
        /// The starting value of the crc.
        /// </summary>
        private const ushort InitialValue = 0xFFFF;

        /// <summary>
        /// The value used to XOR with the data crc to
        /// get the final result. 
        /// </summary>
        private const ushort XorOutValue = 0xFFFF;

        /// <summary>
        /// A table of all possible CRC values. 
        /// </summary>
        private static readonly ushort[] TableCrc16 = InitializeCrc16Table();

        /// <summary>
        /// Creates the table of all possible CRC values. 
        /// </summary>
        /// <returns>
        /// The generated CRC value table.
        /// </returns>
        private static ushort[] InitializeCrc16Table()
        {
            var table = new ushort[256];
            for(ushort arrayIndex = 0; arrayIndex < table.Length; ++arrayIndex)
            {
                ushort crc = 0;
                var bitCheckValue = arrayIndex;
                for(var loopCounter = 0; loopCounter < 8; ++loopCounter)
                {
                    // Check if the bit above the MSB is a 1.
                    if(((crc ^ bitCheckValue) & 0x0001) > 0)
                    {
                        // Shift the CRC right by 1 bit and XOR it 
                        // with the polynomial. 
                        crc = (ushort)((crc >> 1) ^ Polynomial);
                    }
                    else
                    {
                        // Shift the CRC right 1 bit. 
                        crc = (ushort)(crc >> 1);
                    }
                    // Shift right 1 bit. 
                    bitCheckValue = (ushort)(bitCheckValue >> 1);
                }
                table[arrayIndex] = crc;
            }

            return table;
        }

        /// <summary>
        /// Calculates the CRC value given a list of header and data
        /// bytes.
        /// </summary>
        /// <param name="header">The list of header bytes.</param>
        /// <param name="data">The list of data bytes.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when header list or
        /// data list is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when header list 
        /// or data list is empty.</exception>
        /// <returns>The final CRC result.</returns>
        public static ushort Hash(List<byte> header, List<byte> data)
        {   
            if(header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }
            if(data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if(header.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(header), "Header count must be greater than 0");
            }
            if(data.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Data count must be greater than 0");
            }  

            var crc = InitialValue;

            // Calculate the CRC for the header data using 0xffff as 
            // initial value.
            for(var loopCounter = 0; loopCounter < header.Count; ++loopCounter)
            {
                crc = (ushort)((crc >> 8) ^ TableCrc16[header[loopCounter] ^ (crc & 0xFF)]);
            }

            // Calculate the final CRC using the CRC from the header data
            // as the initial value. 
            for(var loopCounter = 0; loopCounter < data.Count; ++loopCounter)
            {
                crc = (ushort)((crc >> 8) ^ TableCrc16[data[loopCounter] ^ (crc & 0xFF)]);
            }

            // Use 0xffff as an xor out.
            return (ushort)(crc ^ XorOutValue);
        }
    }
}

