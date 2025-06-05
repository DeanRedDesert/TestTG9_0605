//-----------------------------------------------------------------------
// <copyright file = "ICoplayerHistoryControl.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using IGT.Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// An interface used to control what history information is being shown by the presentation.
    /// </summary>
    public interface IHistoryPresentationControl
    {
        /// <summary>
        /// Sets the coplayer history data.
        /// </summary>
        /// <param name="stateName">The coplayer's history state name.</param>
        /// <param name="data">The history data.</param>
        void SetCoplayerHistoryData(string stateName, DataItems data);

        /// <summary>
        /// Sets the shell history data.
        /// </summary>
        /// <param name="coplayerPresentationKey">The cotheme presentation key of the coplayer currently in history display.</param>
        /// <param name="stepNumber">The current history step number.</param>
        /// <param name="stateName">The shell's history state name.</param>
        /// <param name="data">The history data.</param>
        void SetShellHistoryData(CothemePresentationKey coplayerPresentationKey, int stepNumber, string stateName, DataItems data);
    }
}
