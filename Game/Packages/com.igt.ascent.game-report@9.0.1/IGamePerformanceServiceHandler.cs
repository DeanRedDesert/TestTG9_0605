// -----------------------------------------------------------------------
// <copyright file = "IGamePerformanceServiceHandler.cs" company = "IGT">
//     Copyright (c) 2016 IGT. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using GamePerformance;

    /// <summary>
    /// This interface defines the service handler for Game Performance Data reporting service.
    /// </summary>
    public interface IGamePerformanceServiceHandler : IReportResourceCleanUp
    {
        /// <summary>
        /// Generates the game performance data which contains all played denominations with specified
        /// theme and paytable.
        /// </summary>
        /// <param name="gamePerformanceReportContext">
        /// The context used for generating the game performance report.
        /// </param>
        /// <returns>
        /// The generated report of game performance data.
        /// </returns>
        GamePerformanceReportType GenerateGamePerformanceReport(GamePerformanceReportContext gamePerformanceReportContext);
    }
}
