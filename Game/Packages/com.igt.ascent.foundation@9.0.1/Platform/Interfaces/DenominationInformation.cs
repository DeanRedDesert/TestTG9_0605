// -----------------------------------------------------------------------
// <copyright file = "DenominationInformation.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// App-Denomination level information.
    /// </summary>
    public class DenominationInformation
    {
        /// <summary>
        /// List of attributes describing this denomination.
        /// </summary>
        public IReadOnlyList<DenominationAttribute> Attributes { get; }

        /// <summary>
        /// The max bet for this denomination.
        /// </summary>
        public ulong MaxBet { get; }

        /// <summary>
        /// The denomination associated with this information.
        /// </summary>
        public uint Denomination { get; }

        /// <summary>
        /// Initializes an instance of <see cref="DenominationInformation"/>.
        /// </summary>
        /// <param name="attributes">A list of <see cref="DenominationAttribute"/>s about this denomination.</param>
        /// <param name="maxBet">The maximum bet for this denomination.</param>
        /// <param name="denom">The denomination associated with this information.</param>
        public DenominationInformation(IReadOnlyList<DenominationAttribute> attributes, ulong maxBet, uint denom)
        {
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            MaxBet = maxBet;
            Denomination = denom;
        }
    }
}
