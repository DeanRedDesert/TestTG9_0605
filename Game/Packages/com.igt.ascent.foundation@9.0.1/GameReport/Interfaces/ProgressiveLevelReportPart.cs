//-----------------------------------------------------------------------
// <copyright file = "ProgressiveLevelReportPart.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Represents a report part for a progressive level that contains Foundation-defined 
    /// and custom report items.
    /// </summary>
    public class ProgressiveLevelReportPart : ReportPart<ProgressiveReportItem, ProgressiveReportLabel>
    {
        /// <summary>
        /// Instantiates a new <see cref="ProgressiveLevelReportPart"/>.
        /// </summary>
        /// <param name="progressiveLevel">The progressive game level.</param>
        /// <param name="levelTotalRtp">The total Return to Player (RTP) for the
        /// progressive level.</param>
        public ProgressiveLevelReportPart(int progressiveLevel, decimal levelTotalRtp)
        {
            ProgressiveLevel = progressiveLevel;
            LevelTotalRtp = levelTotalRtp;
        }

        /// <summary>
        /// Gets the progressive game level.
        /// </summary>
        public int ProgressiveLevel { get; }

        /// <summary>
        /// Gets the total Return to Player (RTP) for the progressive level.
        /// </summary>
        public decimal LevelTotalRtp { get; }
    }
}