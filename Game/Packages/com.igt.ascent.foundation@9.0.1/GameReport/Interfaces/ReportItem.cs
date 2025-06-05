//-----------------------------------------------------------------------
// <copyright file = "ReportItem.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    /// <summary>
    /// Represents a localized report item with a generic label.
    /// </summary>
    /// <typeparam name="TLabel">The type of the label component.</typeparam>
    public class ReportItem<TLabel>
    {
        /// <summary>
        /// Instantiates a <see cref="ReportItem{TLabel}"/> with
        /// a localized label and value.
        /// </summary>
        /// <param name="label">Label of the report item.</param>
        /// <param name="value">Localized value of the report item.</param>
        protected ReportItem(TLabel label, string value)
        {
            Label = label;
            Value = value;
        }

        /// <summary>
        /// Gets the label of the report item.
        /// </summary>
        public TLabel Label { get; }

        /// <summary>
        /// Gets the localized value of the report item.
        /// </summary>
        public string Value { get; }
    }
}
