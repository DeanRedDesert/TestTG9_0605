//-----------------------------------------------------------------------
// <copyright file = "IUgpProgressiveAwardCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    /// <summary>
    /// Defines an interface that allows a package to retrieve information about awarding a UGP progressive.
    /// </summary>
    public interface IUgpProgressiveAwardCategory
    {
        /// <summary>
        /// This informs the platform that the progressive award
        /// </summary>
        /// <param name="progressiveAwardIndex">
        /// The index of the progressive award.
        /// </param>
        /// <param name="progressiveLevelId">
        /// The id of the awarding progressive level.
        /// </param>
        /// <param name="defaultVerifiedAmount">
        /// The default verified amount of the progressive award, in units of base denom.
        /// </param>
        void StartingProgressiveAward(int progressiveAwardIndex, string progressiveLevelId, long defaultVerifiedAmount);

        /// <summary>
        /// The informs the platform that the award display has finished. Payment can then be performed.
        /// </summary>
        /// <param name="progressiveAwardIndex">
        /// The index of the progressive award.
        /// </param>
        /// <param name="progressiveLevelId">
        /// The id of the awarding progressive level.
        /// </param>
        /// <param name="defaultPaidAmount">
        /// The default paid amount of the progressive award, in units of base denom.
        /// </param>
        void FinishedDisplay(int progressiveAwardIndex, string progressiveLevelId, long defaultPaidAmount);
    }
}
