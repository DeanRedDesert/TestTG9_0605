//-----------------------------------------------------------------------
// <copyright file = "CacheCrcException.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;

    /// <summary>
    /// Exception thrown when there is a CRC issue with the critical data cache.
    /// </summary>
    [Serializable]
    public class CacheCrcException : Exception
    {
        /// <summary>
        /// Gets the expected CRC.
        /// </summary>
        public uint ExpectedCrc { get; }

        /// <summary>
        /// Gets the calculated CRC.
        /// </summary>
        public uint CalculatedCrc { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="CacheCrcException"/>.
        /// </summary>
        /// <param name="expectedCrc">The expected CRC of the data.</param>
        /// <param name="calculatedCrc">The calculated CRC of the data.</param>
        public CacheCrcException(uint expectedCrc, uint calculatedCrc)
            : base($"The expected CRC ({expectedCrc}) did not match the calculated CRC ({calculatedCrc}).")
        {
            ExpectedCrc = expectedCrc;
            CalculatedCrc = calculatedCrc;
        }
    }
}
