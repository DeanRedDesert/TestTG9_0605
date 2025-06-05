//-----------------------------------------------------------------------
// <copyright file = "ProgressiveReportItem.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Represents a localized report item for a progressive level
    /// with a <see cref="ProgressiveReportLabel"/>.
    /// </summary>
    public class ProgressiveReportItem : ReportItem<ProgressiveReportLabel>
    {
        /// <summary>
        /// Instantiates a new <see cref="ProgressiveReportItem"/> with a
        /// <see cref="ProgressiveReportLabel"/> and a localized value.
        /// </summary>
        /// <param name="label"><see cref="ProgressiveReportLabel"/> of the Foundation-defined report item.</param>
        /// <param name="value">Localized value of the report item.</param>
        public ProgressiveReportItem(ProgressiveReportLabel label, string value) 
            : base(label, value)
        {
        }
    }
}
