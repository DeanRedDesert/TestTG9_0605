//-----------------------------------------------------------------------
// <copyright file = "ProgressiveSettings.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// The settings for a progressive level.
    /// </summary>
    /// <remarks>
    /// This is a simple data structure for display and storage purpose.
    /// It is not intended for comparison or being used as collection keys.
    /// </remarks>
    public class ProgressiveSettings : IProgressiveSettings
    {
        /// <summary>
        /// Gets or sets the start amount for the progressive.
        /// </summary>
        public long StartAmount { get; set; }

        /// <summary>
        /// Gets or sets the maximum amount that the progressive can award.
        /// </summary>
        public long MaxAmount { get; set; }

        /// <summary>
        /// Gets or sets the contribution rate for the progressive as a percentage
        /// in the range 0 - 100.
        /// </summary>
        public decimal ContributionPercentage { get; set; }
    }
}
