//-----------------------------------------------------------------------
// <copyright file = "IOutcome.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Base type for all outcomes and awards.
    /// </summary>
    public interface IOutcome
    {
        /// <summary>
        /// Gets the origin of the outcome.
        /// </summary>
        OutcomeOrigin Origin { get; }

        /// <summary>
        /// Gets the string identifying tag for this outcome.
        /// </summary>
        string Tag { get; }

        /// <summary>
        /// Gets a source string field for creator's use.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Gets a general purpose field based on the value of source attribute.
        /// </summary>
        string SourceDetail { get; }
    }
}