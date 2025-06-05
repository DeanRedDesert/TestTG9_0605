//-----------------------------------------------------------------------
// <copyright file = "GenerateInspectionReportEventArgs.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using Game.Core.GameReport.Interfaces;

    /// <summary>
    /// The arguments and result for the request of generating an inspection report.
    /// </summary>
    public class GenerateInspectionReportEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the culture/language to use when formatting report text.
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// Gets the identifier of the theme to use for generating the report.
        /// This is the identifier maintained by the Foundation, used in communication
        /// with the Foundation to identifier a theme.
        /// </summary>
        public string ThemeIdentifier { get; internal set; }

        /// <summary>
        /// Gets the identifier of the paytable to use for generating the report.
        /// This is the identifier maintained by the Foundation, used in communication
        /// with the Foundation to identify an individual "pay variation".
        /// </summary>
        public string PaytableIdentifier { get; }

        /// <summary>
        /// Gets the denomination to use for generating the report.
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        /// Gets or sets the generated report.
        /// </summary>
        public IGameInspectionReport GeneratedReport { get; set; }

        /// <summary>
        /// Gets or sets the error message if a report could not be generated.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="GenerateInspectionReportEventArgs"/>.
        /// </summary>
        /// <param name="culture">The culture/language to use when formatting report text.</param>
        /// <param name="themeIdentifier">The identifier of the theme to use for generating the report.</param>
        /// <param name="paytableIdentifier">The identifier of the paytable to use for generating the report.</param>
        /// <param name="denomination">The denomination to use for generating the report.</param>
        public GenerateInspectionReportEventArgs(string culture, string themeIdentifier, string paytableIdentifier,
                                                 long denomination)
        {
            Culture = culture;
            ThemeIdentifier = themeIdentifier;
            PaytableIdentifier = paytableIdentifier;
            Denomination = denomination;
        }
    }
}
