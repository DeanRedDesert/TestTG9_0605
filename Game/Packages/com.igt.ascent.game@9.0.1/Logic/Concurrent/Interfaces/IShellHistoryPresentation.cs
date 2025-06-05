//-----------------------------------------------------------------------
// <copyright file = "IShellHistoryPresentation.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using IGT.Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// An interface for sending history data to the shell presentation.
    /// </summary>
    public interface IShellHistoryPresentation
    {
        /// <summary>
        /// Sets the shell history data.
        /// </summary>
        /// <param name="stateName">The history state name.</param>
        /// <param name="data">The history data.</param>
        void StartHistoryState(string stateName, DataItems data);
    }
}
