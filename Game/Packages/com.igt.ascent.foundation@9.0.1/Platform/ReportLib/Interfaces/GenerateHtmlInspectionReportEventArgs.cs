//-----------------------------------------------------------------------
// <copyright file = "GenerateHtmlInspectionReportEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Game.Core.GameReport.Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// A class encapsulating information needed to generate an Html inspection report.
    /// </summary>
    public class GenerateHtmlInspectionReportEventArgs : EventArgs
    {
        /// <summary>
        /// Gets/sets the specific culture that this report is created for.
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// Gets/sets a dictionary of theme IDs vs a collection of supported <see cref="PaytableDenominationInfo"/> objects.
        /// </summary>
        public IDictionary<string, IList<PaytableDenominationInfo>> GameThemeAndPayvarDictionary { get; }

        /// <summary>
        /// Gets/sets the generated Html report.
        /// </summary>
        public IGameHtmlInspectionReport GeneratedHtmlReport { get; set; }

        /// <summary>
        /// Gets/sets the error message if a report could not be generated.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="GenerateHtmlInspectionReportEventArgs"/>.
        /// </summary>
        /// <param name="culture">The specific culture that this report is created for.</param>
        /// <param name="gameThemeAndPayvarDictionary">The Dictionary of theme IDs vs a collection of supported <see cref="PaytableDenominationInfo"/> objects.</param>
        public GenerateHtmlInspectionReportEventArgs(string culture, IDictionary<string, IList<PaytableDenominationInfo>> gameThemeAndPayvarDictionary)
        {
            GameThemeAndPayvarDictionary = gameThemeAndPayvarDictionary;
            Culture = culture;
        }
    }
}