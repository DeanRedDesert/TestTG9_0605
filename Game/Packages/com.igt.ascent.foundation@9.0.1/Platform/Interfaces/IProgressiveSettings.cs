// -----------------------------------------------------------------------
// <copyright file = "IProgressiveSettings.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// Readonly settings interface for a progressive level.
    /// </summary>
    public interface IProgressiveSettings
    {
        /// <summary>
        /// Gets the start amount for the progressive.
        /// </summary>
        long StartAmount { get; }

        /// <summary>
        /// Gets the maximum amount that the progressive can award.
        /// </summary>
        long MaxAmount { get; }

        /// <summary>
        /// Gets the contribution rate for the progressive as a percentage
        /// in the range 0 - 100.
        /// </summary>
        decimal ContributionPercentage { get; }
    }
}