//-----------------------------------------------------------------------
// <copyright file = "AttractStatus.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    /// <summary>
    /// The status information for the current attract sequence.
    /// </summary>
    public struct AttractStatus
    {
        /// <summary>
        /// The current attract mode.
        /// </summary>
        public AttractMode AttractMode;

        /// <summary>
        /// The time to wait for in milliseconds until the next attract sequence.
        /// </summary>
        public long WaitTime;

        /// <summary>
        /// When set, indicates the playlist entry index that is currently playing.
        /// </summary>
        public int? EntryIndexPlaying;
    }
}
