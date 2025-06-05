//-----------------------------------------------------------------------
// <copyright file = "IUgpProgressiveAwardCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    /// <summary>
    /// Interface to accept callbacks from the UGP ProgressiveAward category.
    /// </summary>
    public interface IUgpProgressiveAwardCategoryCallbacks
    {
        /// <summary>
        /// Method called when UgpProgressiveAwardCategory ProgressiveVerfifed message is received from the foundation.
        /// </summary>
        /// <param name="progressiveAwardIndex">The index of progressive award.</param>
        /// <param name="progressiveLevelId">The ID of progressive level.</param>
        /// <param name="verifiedAmount">The verified amount of progressive award.</param>
        /// <param name="payType">The pay type of progressive award.</param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessProgressiveVerified(int progressiveAwardIndex, string progressiveLevelId,
                                          long verifiedAmount, ProgressiveAwardPayTypeEnum payType);

        /// <summary>
        /// Method called when UgpProgressiveAwardCategory ProgressivePaid message is received from the foundation.
        /// </summary>
        /// <param name="progressiveAwardIndex">The index of progressive award.</param>
        /// <param name="progressiveLevelId">The ID of progressive level.</param>
        /// <param name="paidAmount">The paid amount of progressive award.</param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessProgressivePaid(int progressiveAwardIndex, string progressiveLevelId, long paidAmount);
    }
}
