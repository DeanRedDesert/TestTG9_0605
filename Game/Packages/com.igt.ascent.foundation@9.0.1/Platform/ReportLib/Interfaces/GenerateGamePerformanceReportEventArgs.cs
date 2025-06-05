//-----------------------------------------------------------------------
// <copyright file = "GenerateGamePerformanceReportEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;

    /// <summary>
    /// The arguments and result for the request of generating a game performance data report.
    /// </summary>
    public class GenerateGamePerformanceReportEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the theme identifier to use for generating the report.
        /// This is the identifier maintained by the Foundation, used in communication
        /// with the Foundation to identify a game theme.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// Gets the paytable identifier to use for generating the report.
        /// This is the identifier maintained by the Foundation, used in communication
        /// with the Foundation to identify an individual "pay variation".
        /// </summary>
        public string PaytableIdentifier { get; }

        /// <summary>
        /// Gets or sets the generated report.
        /// </summary>
        public string GeneratedReport { get; set; }

        /// <summary>
        /// Gets or sets the error message if a report could not be generated.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="GenerateGamePerformanceReportEventArgs"/>.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier to use for generating the report.</param>
        /// <param name="paytableIdentifier">The paytable identifier to use for generating the report.</param>
        public GenerateGamePerformanceReportEventArgs(string themeIdentifier, string paytableIdentifier)
        {
            ThemeIdentifier = themeIdentifier;
            PaytableIdentifier = paytableIdentifier;
        }
    }
}
