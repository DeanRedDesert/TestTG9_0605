//-----------------------------------------------------------------------
// <copyright file = "IGameHtmlInspectionReport.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Defines the contents of an Html game inspection report.
    /// </summary>
    public interface IGameHtmlInspectionReport
    {
        /// <summary>
        /// Gets a custom game-defined Html report as a single string.
        /// </summary>
        string HtmlReport { get; }
    }
}