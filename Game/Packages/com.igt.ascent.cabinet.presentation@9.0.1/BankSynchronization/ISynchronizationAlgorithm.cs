//-----------------------------------------------------------------------
// <copyright file = "ISynchronizationAlgorithm.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    /// <summary>
    /// The interface for a bank synchronization algorithm.
    /// </summary>
    public interface ISynchronizationAlgorithm
    {
        /// <summary>
        /// Gets the playlist entry index for the given time and bank position.
        /// </summary>
        /// <param name="playlist">The playlist file to use.</param>
        /// <param name="currentTime">The current time in milliseconds.</param>
        /// <param name="bankPositionNumber">The position of the machine in the bank.</param>
        /// <param name="totalMachinesInBank">The total number of machines in the bank.</param>
        /// <returns>The playlist entry for the current time.</returns>
        // ReSharper disable UnusedParameter
        int GetPlaylistEntryIndex(Playlist playlist, long currentTime, uint bankPositionNumber, uint totalMachinesInBank);

        /// <summary>
        /// Gets the start time for the specified playlist entry index after the index specified by <paramref name="index"/>.
        /// </summary>
        /// <param name="playlist">The playlist file to use.</param>
        /// <param name="index">The index of the currently playing playlist entry.</param>
        /// <param name="currentTime">The current time in milliseconds.</param>
        /// <param name="bankPositionNumber">The position of the machine in the bank.</param>
        /// <param name="totalMachinesInBank">The total number of machines in the bank.</param>
        /// <returns>The information on the next playlist entry.</returns>
        NextEntryInfo GetStartTimeForNextPlaylistEntry(Playlist playlist, int index, long currentTime, uint bankPositionNumber, uint totalMachinesInBank);

        /// <summary>
        /// Gets the start time for the specified playlist entry index. If the current time is in the middle of
        /// the current playlist entry, the time when it was supposed to start is returned.
        /// </summary>
        /// <param name="playlist">The playlist file to use.</param>
        /// <param name="index">The index of the currently playing playlist entry.</param>
        /// <param name="currentTime">The current time in milliseconds.</param>
        /// <param name="bankPositionNumber">The position of the machine in the bank.</param>
        /// <param name="totalMachinesInBank">The total number of machines in the bank.</param>
        /// <returns>The information on the playlist entry that should be playing given the current time.</returns>
        NextEntryInfo GetStartTimeForCurrentPlaylistEntry(Playlist playlist, int index, long currentTime, uint bankPositionNumber, uint totalMachinesInBank);
        // ReSharper restore UnusedParameter
    }
}
