//-----------------------------------------------------------------------
// <copyright file = "IUgpProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;

    /// <summary>
    /// Defines an interface that allows a package to retrieve information about awarding a UGP progressive.
    /// </summary>
    public interface IUgpProgressiveAward
    {
        /// <summary>
        /// Event raised when the progressive award has been verified.
        /// </summary>
        event EventHandler<ProgressiveAwardVerifiedEventArgs> ProgressiveAwardVerified;

        /// <summary>
        /// Event raised when the progressive award has been paid.
        /// </summary>
        event EventHandler<ProgressiveAwardPaidEventArgs> ProgressiveAwardPaid;

        /// <summary>
        /// Informs the foundation that the progressive award is started.
        /// </summary>
        /// <param name="progressiveAwardIndex">
        /// The index of the progressive award.
        /// </param>
        /// <param name="progressiveLevelId">
        /// The level ID of the progressive award.
        /// </param>
        /// <param name="defaultVerifiedAmount">
        /// The default verified amount of the progressive award, in units of base denom.
        /// </param>
        void StartingProgressiveAward(int progressiveAwardIndex, string progressiveLevelId, long defaultVerifiedAmount);

        /// <summary>
        /// Informs the foundation that the award display has finished. Payment can then be performed.
        /// </summary>
        /// <param name="progressiveAwardIndex">
        /// The index of the progressive award.
        /// </param>
        /// <param name="progressiveLevelId">
        /// The level ID of the progressive award.
        /// </param>
        /// <param name="defaultPaidAmount">
        /// The default paid amount of the progressive award, in units of base denom.
        /// </param>
        void FinishedDisplay(int progressiveAwardIndex, string progressiveLevelId, long defaultPaidAmount);
    }
}
