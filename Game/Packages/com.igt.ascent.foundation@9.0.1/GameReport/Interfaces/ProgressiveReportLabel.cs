//-----------------------------------------------------------------------
// <copyright file = "ProgressiveReportLabel.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Represents a Foundation-defined progressive level report item.
    /// </summary>
    public enum ProgressiveReportLabel
    {
        /// <summary>
        /// The localized description of the progressive level in terms of the game.
        /// </summary>
        GameLevelDescription,

        /// <summary>
        /// The contribution rate for the progressive level.
        /// </summary>
        ContributionPercentage,

        /// <summary>
        /// The total Return to Player (RTP) of the progressive level.
        /// </summary>
        LevelTotalRtp,

        /// <summary>
        /// A localized description of the type of progressive level.
        /// </summary>
        LinkStatus,

        /// <summary>
        /// The maximum amount in base units that the progressive level can award.
        /// </summary>
        MaxAmount,

        /// <summary>
        /// The starting or reset amount in base units for the progressive level.
        /// </summary>
        StartAmount,

        /// <summary>
        /// The Return to Player (RTP) percent from the start amount.
        /// </summary>
        StartPercent
    }
}
