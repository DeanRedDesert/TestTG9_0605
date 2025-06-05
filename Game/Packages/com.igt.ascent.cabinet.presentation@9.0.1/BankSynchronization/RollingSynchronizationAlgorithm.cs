//-----------------------------------------------------------------------
// <copyright file = "RollingSynchronizationAlgorithm.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    using System;

    /// <summary>
    /// A bank synchronization algorithm that plays through the entries in the playlist
    /// on a loop.
    /// </summary>
    public class RollingSynchronizationAlgorithm : ISynchronizationAlgorithm
    {
        #region ISynchronizationAlgorithm Members

        /// <inheritdoc />
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="playlist"/> is null.
        /// </exception>
        public int GetPlaylistEntryIndex(Playlist playlist, long currentTime, uint bankPositionNumber, uint totalMachinesInBank)
        {
            if(playlist == null)
            {
                throw new ArgumentNullException(nameof(playlist));
            }

            var entryIndex = -1;

            var offset = currentTime % playlist.TotalDisplayTime;
            long timeCounter = 0;
            for(var index = 0; index < playlist.Items.Length; index++)
            {
                timeCounter += playlist.Items[index].DisplayTime;
                if(offset < timeCounter)
                {
                    entryIndex = index;
                    break;
                }
            }

            return entryIndex;
        }

        /// <inheritdoc />
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="index"/> is negative or larger than the number of playlist entries.
        /// </exception>
        public NextEntryInfo GetStartTimeForNextPlaylistEntry(Playlist playlist, int index, long currentTime, uint bankPositionNumber, uint totalMachinesInBank)
        {
            if(playlist == null)
            {
                throw new ArgumentNullException(nameof(playlist));
            }

            if(index < 0 || index >= playlist.Items.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"The index of {index} is out of range. Number of elements is {playlist.Items.Length}.");
            }

            var nextIndex = (index + 1) % playlist.Items.Length;
            var entryInfo = new NextEntryInfo
            {
                NextEntryIndex = nextIndex
            };

            var offset = currentTime % playlist.TotalDisplayTime;
            var timeCounter = CalculateTimePositionWithinPlaylist(playlist, nextIndex);

            if(timeCounter < offset)
            {
                // The specified index is past this current cycle so it needs to wait until the next one.
                entryInfo.StartTime = currentTime + (playlist.TotalDisplayTime - offset) + timeCounter;
            }
            else
            {
                entryInfo.StartTime = currentTime - offset + timeCounter;
            }

            return entryInfo;
        }

        /// <inheritdoc />
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="playlist"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="index"/> is negative or larger than the number of playlist entries.
        /// </exception>
        public NextEntryInfo GetStartTimeForCurrentPlaylistEntry(Playlist playlist, int index, long currentTime, uint bankPositionNumber, uint totalMachinesInBank)
        {
            if(playlist == null)
            {
                throw new ArgumentNullException(nameof(playlist));
            }

            if(index < 0 || index >= playlist.Items.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"The index of {index} is out of range. Number of elements is {playlist.Items.Length}.");
            }

            var entryInfo = new NextEntryInfo
            {
                NextEntryIndex = index
            };

            var offset = currentTime % playlist.TotalDisplayTime;
            var timeCounter = CalculateTimePositionWithinPlaylist(playlist, index);

            entryInfo.StartTime =currentTime - offset + timeCounter;

            return entryInfo;
        }

        #endregion

        /// <summary>
        /// Calculates the time at which a given playlist entry is supposed to start assuming that
        /// the playlist starts at time 0.
        /// </summary>
        /// <param name="playlist">The playlist to use.</param>
        /// <param name="index">The index of the playlist entry to calculate the start time for.</param>
        /// <returns>The start time of the playlist entry.</returns>
        private static long CalculateTimePositionWithinPlaylist(Playlist playlist, int index)
        {
            long timeCounter = 0;

            for(var entryIndex = 0; entryIndex < index; entryIndex++)
            {
                timeCounter += playlist.Items[entryIndex].DisplayTime;
            }

            return timeCounter;
        }
    }
}
