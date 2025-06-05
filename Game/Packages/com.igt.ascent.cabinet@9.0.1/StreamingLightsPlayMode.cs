//-----------------------------------------------------------------------
// <copyright file = "StreamingLightsPlayMode.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// The different play modes for a streaming light sequence.
    /// </summary>
    public enum StreamingLightsPlayMode
    {
        /// <summary>
        /// Start the sequence from the beginning.
        /// </summary>
        Restart,

        /// <summary>
        /// Replace the sequence in memory with a new sequence and play from the current position.
        /// The new sequence has to have the exact same number of segments and frames.
        /// </summary>
        Continue,

        /// <summary>
        /// Queues the sequence in memory to play after the currently playing sequence has finished.
        /// </summary>
        Queue
    }
}
