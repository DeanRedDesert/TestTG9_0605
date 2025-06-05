//-----------------------------------------------------------------------
// <copyright file = "StandardReportItem.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Represents a localized standard report item
    /// with a <see cref="StandardReportLabel"/>.
    /// </summary>
    public class StandardReportItem : ReportItem<StandardReportLabel>
    {
        /// <summary>
        /// Instantiates a new <see cref="StandardReportItem"/> with a
        /// <see cref="StandardReportLabel"/> and a localized value.
        /// </summary>
        /// <param name="label"><see cref="StandardReportLabel"/> of the Foundation-defined
        /// report item.</param>
        /// <param name="value">Localized value of the report item.</param>
        public StandardReportItem(StandardReportLabel label, string value) 
            : base(label, value)
        {
        }
    }
}
