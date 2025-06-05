//-----------------------------------------------------------------------
// <copyright file = "CustomReportItem.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Represents a game-specific custom report item localized to
    /// the active culture.
    /// </summary>
    public class CustomReportItem : ReportItem<string>
    {
        /// <summary>
        /// Instantiates a <see cref="CustomReportItem"/> with
        /// a localized label and value.
        /// </summary>
        /// <param name="label">Localized label of the report item.</param>
        /// <param name="value">Localized value of the report item.</param>
        public CustomReportItem(string label, string value)
            : base(label, value)
        {
        }
    }
}
