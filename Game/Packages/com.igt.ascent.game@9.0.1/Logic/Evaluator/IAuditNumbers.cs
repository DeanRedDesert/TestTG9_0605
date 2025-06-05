//-----------------------------------------------------------------------
// <copyright file = "IAuditNumbers.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Interface to use for Auditing random number sources.
    /// </summary>
    public interface IAuditNumbers
    {
        /// <summary>
        /// Get a list of numbers which have been drawn since AuditNumbers was last called.
        /// </summary>
        /// <returns>List of numbers drawn from the object which implements this interface.</returns>
        /// <remarks>The list of drawn numbers is cleared on each call to this function.</remarks>
        ReadOnlyCollection<int> AuditNumbers();
    }
}