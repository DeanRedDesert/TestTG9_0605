//-----------------------------------------------------------------------
// <copyright file = "GetInspectionReportTypeEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using Game.Core.GameReport.Interfaces;

    /// <summary>
    /// A class encapsulating information needed to get the inspection report type
    /// supported by the game report object.
    /// </summary>
    public class GetInspectionReportTypeEventArgs : EventArgs
    {
        /// <summary>
        /// Gets/sets the <see cref="GameInspectionReportType"/>.
        /// </summary>
        public GameInspectionReportType GameInspectionReportType { get; set; }
    }
}