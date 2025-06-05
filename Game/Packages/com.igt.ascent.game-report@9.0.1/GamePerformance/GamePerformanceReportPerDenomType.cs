// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceReportPerDenomType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    using System.Collections.Generic;

    /// <summary>
    /// This class contains the all the meter information for the game performance report per denom.
    /// </summary>
    public class GamePerformanceReportPerDenomType
    {
        /// <summary>
        /// The denomination of this report.
        /// </summary>
        public long Denom { get; set; }

        /// <summary>
        /// The list of meter groups within the report.
        /// </summary>
        public List<GamePerformanceMeterGroupType> MeterGroupList { get; set; } = new List<GamePerformanceMeterGroupType>();
    }
}
