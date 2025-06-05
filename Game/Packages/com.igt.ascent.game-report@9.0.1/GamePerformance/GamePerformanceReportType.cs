// -----------------------------------------------------------------------
// <copyright file = "GamePerformanceReportType.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.GamePerformance
{
    using System.Collections.Generic;

    /// <summary>
    /// This class contains the information of the game performance report data for specific payvar.
    /// </summary>
    public class GamePerformanceReportType
    {
        /// <summary>
        /// The list of sub-reports for game performance report per denominations.
        /// </summary>
        public List<GamePerformanceReportPerDenomType> GamePerformancePerDenomList { get; set; } = new List<GamePerformanceReportPerDenomType>();
    }
}
