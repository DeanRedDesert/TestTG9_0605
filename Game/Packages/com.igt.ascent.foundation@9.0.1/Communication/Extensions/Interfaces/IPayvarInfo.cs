// -----------------------------------------------------------------------
// <copyright file = "IPayvarInfo.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// The information on a payvar that is needed by the interface extensions.
    /// </summary>
    public interface IPayvarInfo
    {
        /// <summary>
        /// Gets a paytable identifier that is unique to each theme-paytable combination.
        /// </summary>
        string PaytableIdentifier { get; }

        /// <summary>
        /// Gets the group tag for the payvar.
        /// </summary>
        string GroupPayvarTag { get; }

        /// <summary>
        /// Gets the data for the <see cref="GroupPayvarTag"/>.
        /// </summary>
        string GroupPayvarTagData { get; }

        /// <summary>
        /// Gets the redefinition of a max bet value, for the payvar metering.
        /// </summary>
        /// <remarks>
        /// The key represents the max bet configured at the group level,
        /// and the value represents the corresponding max bet at the payvar level.
        /// </remarks>
        IDictionary<long, long> MaxBetRedefinition { get; }
    }
}