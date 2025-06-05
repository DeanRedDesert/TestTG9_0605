//-----------------------------------------------------------------------
// <copyright file = "ICustomReportSection.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// Provides a collection of <see cref="CustomReportItem"/>s that
    /// are logically grouped as a report section.
    /// </summary>
    public interface ICustomReportSection
    {
        /// <summary>
        /// Gets a list of <see cref="CustomReportItem"/>s for the
        /// report section.
        /// </summary>
        IList<CustomReportItem> CustomReportItems { get; }
    }
}
