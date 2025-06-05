// -----------------------------------------------------------------------
// <copyright file = "ProgressiveAwardStatus.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    /// <summary>
    /// Defines the status of a progressive award.
    /// </summary>
    public enum ProgressiveAwardStatus
    {
        /// <summary>
        /// The progressive award is triggered.
        /// </summary>
        Triggered,

        /// <summary>
        /// The progressive award is verified.
        /// </summary>
        Verified,

        /// <summary>
        /// The progressive award is finished on display.
        /// </summary>
        FinishedDisplay,

        /// <summary>
        /// The progressive award is paid.
        /// </summary>
        Paid,

        /// <summary>
        /// The progressive award is cleared.
        /// </summary>
        Cleared
    }
}
