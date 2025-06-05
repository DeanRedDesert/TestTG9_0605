// -----------------------------------------------------------------------
// <copyright file = "IShellHistoryControl.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs for accessing shell's history context and critical data during History Mode.
    /// </summary>
    public interface IShellHistoryControl
    {
        /// <summary>
        /// A request to bind a session to the shell history context.
        /// </summary>
        /// <param name="sessionId">
        /// The session to bind with the history context.  Must be greater than 0,
        /// i.e. it cannot be the shell dedicated session.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sessionId"/> is not greater than 0.
        /// </exception>
        void BindSessionToHistory(int sessionId);

        /// <summary>
        /// Request to get the information on the game cycle being displayed in history.
        /// </summary>
        /// <returns>
        /// The content of the response. This element should be omitted when an Exception is present.
        /// </returns>
        HistoryThemeInfo GetHistoryThemeInformation();

        /// <summary>
        /// Message requesting to read critical data with the given name.
        /// </summary>
        /// <param name="nameList">
        /// Identifies a list of the names of critical data to read.
        /// </param>
        /// <returns>
        /// A critical data block which contains all the critical data having been read.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="nameList"/> is null or empty.
        /// </exception>
        ICriticalDataBlock ReadCriticalData(IList<string> nameList);
    }
}