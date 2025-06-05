//-----------------------------------------------------------------------
// <copyright file = "CacheCrcException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;

    /// <summary>
    /// Exception thrown when there is a CRC issue with the critical data cache.
    /// </summary>
    [Serializable]
    public class CacheCrcException : Exception
    {
        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="expectedCrc">The expected CRC of the data.</param>
        /// <param name="calculatedCrc">The calculated CRC of the data.</param>
        public CacheCrcException(uint expectedCrc, uint calculatedCrc)
            : base($"The expected CRC ({expectedCrc}) did not match the calculated CRC ({calculatedCrc}).")
        {
        }
    }
}
