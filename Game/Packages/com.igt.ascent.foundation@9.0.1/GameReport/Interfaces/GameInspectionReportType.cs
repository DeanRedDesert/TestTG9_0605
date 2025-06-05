// -----------------------------------------------------------------------
// <copyright file = "GameInspectionReportType.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    using System;

    /// <summary>
    /// Enum defining supported game inspection report types.
    /// </summary>
    [Flags]
    public enum GameInspectionReportType
    {
        /// <summary>
        /// No game report type specified.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// A data-structure based report that the foundation uses to generate the report presentation.
        /// </summary>
        Itemized = 0x01,
    
        /// <summary>
        /// A game defined Html formatted report. The foundation will display the report as is.
        /// </summary>
        Html = 0x02,

        /// <summary>
        /// All report types have been set by the game.
        /// </summary>
        All = Itemized | Html,
    }
}