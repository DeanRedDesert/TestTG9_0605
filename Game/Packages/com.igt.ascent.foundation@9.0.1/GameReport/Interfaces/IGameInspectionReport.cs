//-----------------------------------------------------------------------
// <copyright file = "IGameInspectionReport.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the content of a game inspection report.
    /// </summary>
    public interface IGameInspectionReport
    {
        /// <summary>
        /// Gets the standard report part that contains Foundation-defined and custom report items.
        /// </summary>
        StandardReportPart StandardReportPart { get; }

        /// <summary>
        /// Gets a collection of progressive level report parts where each
        /// progressive report part contains Foundation-defined and custom report items
        /// for a progressive level.
        /// </summary>
        IEnumerable<ProgressiveLevelReportPart> ProgressiveLevelParts { get; }
    }
}