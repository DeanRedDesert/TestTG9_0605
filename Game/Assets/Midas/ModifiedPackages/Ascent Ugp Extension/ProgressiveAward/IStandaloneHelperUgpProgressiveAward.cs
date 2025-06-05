//-----------------------------------------------------------------------
// <copyright file = "IStandaloneHelperUgpProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    /// <summary>
    /// Standalone helper interface for UGP Progressive Award.
    /// </summary>
    public interface IStandaloneHelperUgpProgressiveAward
    {
        /// <summary>
        /// Raises the progressive award verified event.
        /// </summary>
        /// <param name="progressiveIndex">
        /// The index of progressive award.
        /// </param>
        /// <param name="verifiedAmount">
        /// The verified amount of progressive award.
        /// </param>
        void SendVerified(int progressiveIndex, long verifiedAmount);

        /// <summary>
        /// Raises the progressive award paid event.
        /// </summary>
        /// <param name="progressiveIndex">
        /// The index of progressive award.
        /// </param>
        /// <param name="paidAmount">
        /// The paid amount of progressive award.
        /// </param>
        void SendPaid(int progressiveIndex, long paidAmount);

        /// <summary>
        /// Sets the flag indicating if standalone progressive award is manually controlled.
        /// </summary>
        /// <param name="isControlledManually">
        /// The flag to set indicating if standalone progressive award is manually controlled.
        /// </param>
        void SetManualControl(bool isControlledManually);
    }
}
