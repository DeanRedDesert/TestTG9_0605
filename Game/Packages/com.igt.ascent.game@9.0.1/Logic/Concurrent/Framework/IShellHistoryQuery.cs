// -----------------------------------------------------------------------
// <copyright file = "IShellHistoryQuery.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using IGT.Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// An interface used to query history information from the shell.
    /// </summary>
    internal interface IShellHistoryQuery
    {
        /// <summary>
        /// Gets a history record containing the shell's current history data, or the difference between the shell's
        /// history data and the base data, if provided.
        /// </summary>
        /// <param name="stepNumber">The step number to assign to the record.</param>
        /// <param name="baseData">
        /// The (optional) base data. If provided, only the difference between the base data and the shell's history 
        /// data will be contained in the history record.
        /// </param>
        /// <returns>
        /// A history record containing either all of the shell's current history data or the difference if base data 
        /// was provided.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="stepNumber"/> is not a positive integer.
        /// </exception>
        HistoryRecord GetHistoryRecord(int stepNumber, DataItems baseData = null);
    }
}
