//-----------------------------------------------------------------------
// <copyright file = "GameInspectionReport.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// Represents a game inspection report consisting of a standard report part
    /// and a collection of progressive level report parts. 
    /// </summary>
    public class GameInspectionReport : IGameInspectionReport
    {
        /// <summary>
        /// Instantiates a <see cref="GameInspectionReport"/> with standard and
        /// progressive level parts.
        /// </summary>
        /// <param name="standardReportPart">The <see cref="StandardReportPart"/>.</param>
        /// <param name="progressiveLevelParts">A collection of <see cref="ProgressiveLevelReportPart"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if either parameter is null.</exception> 
        public GameInspectionReport(StandardReportPart standardReportPart,
                                    IEnumerable<ProgressiveLevelReportPart> progressiveLevelParts)
        {
            StandardReportPart = standardReportPart ?? throw new ArgumentNullException(nameof(standardReportPart), "Argument may not be null.");
            ProgressiveLevelParts = progressiveLevelParts ?? throw new ArgumentNullException(nameof(progressiveLevelParts), "Argument may not be null.");
        }

        /// <inheritdoc/>
        public StandardReportPart StandardReportPart { get; }

        /// <inheritdoc/>
        public IEnumerable<ProgressiveLevelReportPart> ProgressiveLevelParts { get; }
    }
}