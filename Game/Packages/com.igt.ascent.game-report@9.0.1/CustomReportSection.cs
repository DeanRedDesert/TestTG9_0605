//-----------------------------------------------------------------------
// <copyright file = "CustomReportSection.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// A basic implementation of <see cref="ICustomReportSection"/>
    /// that can be instantiated with a collection of 
    /// <see cref="CustomReportItem"/>s.
    /// </summary>
    public class CustomReportSection : ICustomReportSection
    {
        private readonly List<CustomReportItem> customReportItems;

        /// <summary>
        /// Instantiates a new <see cref="CustomReportSection"/> with 
        /// a collection of <see cref="CustomReportItem"/>s.
        /// </summary>
        /// <param name="reportItems">Collection of <see cref="CustomReportItem"/>s
        /// to add to the custom report section.</param>
        public CustomReportSection(IEnumerable<CustomReportItem> reportItems)
        {
            customReportItems = new List<CustomReportItem>(reportItems);
        }

        #region ICustomReportSection Members

        /// <inheritdoc/>
        public IList<CustomReportItem> CustomReportItems => customReportItems;

        #endregion
    }
}
