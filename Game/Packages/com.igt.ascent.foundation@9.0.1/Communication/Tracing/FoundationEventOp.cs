// -----------------------------------------------------------------------
// <copyright file = "FoundationEventOp.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Tracing
{
    /// <summary>
    /// Defines the operation on a Foundation event.
    /// </summary>
    public enum FoundationEventOp
    {
        /// <summary>
        /// The Foundation event is posted, and waits for process before replying Foundation.
        /// </summary>
        Posted,

        /// <summary>
        /// The Foundation event is processed.
        /// </summary>
        Processed,

        /// <summary>
        /// The Foundation event is queued.  Response to Foundation is sent right away without waiting to be processed.
        /// </summary>
        Queued,
    }
}