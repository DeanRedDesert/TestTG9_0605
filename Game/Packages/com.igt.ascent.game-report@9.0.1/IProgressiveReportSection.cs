//-----------------------------------------------------------------------
// <copyright file = "IProgressiveReportSection.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the progressive section of the game report which consists of
    /// an instance of <see cref="IProgressiveLevelData"/> for each progressive level
    /// in the paytable.
    /// </summary>
    public interface IProgressiveReportSection
    {
        /// <summary>
        /// Gets a list of <see cref="IProgressiveLevelData"/> objects.
        /// </summary>
        IList<IProgressiveLevelData> ProgressiveLevels { get; }
    }
}
