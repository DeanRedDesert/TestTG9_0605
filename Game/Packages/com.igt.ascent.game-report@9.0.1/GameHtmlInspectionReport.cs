//-----------------------------------------------------------------------
// <copyright file = "GameHtmlInspectionReport.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using Interfaces;

    /// <summary>
    /// Represents a game defined Html inspection report.
    /// </summary>
    public class GameHtmlInspectionReport : IGameHtmlInspectionReport
    {
        /// <summary>
        /// Instantiates a <see cref="GameHtmlInspectionReport"/>.
        /// </summary>
        /// <param name="htmlInspectionReport">The Html report in single-string format.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the passed parameter <paramref name="htmlInspectionReport"/> is null or empty.
        /// </exception>
        public GameHtmlInspectionReport(string htmlInspectionReport)
        {
            if(string.IsNullOrEmpty(htmlInspectionReport))
            {
                throw new ArgumentNullException(nameof(htmlInspectionReport), "Argument may not be null or empty.");
            }

            HtmlReport = htmlInspectionReport;
        }

        /// <inheritdoc/>
        public string HtmlReport
        {
            get;
        }
    }
}