//-----------------------------------------------------------------------
// <copyright file = "NextEntryInfo.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    /// <summary>
    /// The information on a playlist entry used to know when it can be started.
    /// </summary>
    public struct NextEntryInfo
    {
        /// <summary>
        /// The time in milliseconds for when the specified index should start playing.
        /// </summary>
        public long StartTime;

        /// <summary>
        /// The next playlist entry index to play.
        /// </summary>
        public int NextEntryIndex;
    }
}
