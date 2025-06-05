//-----------------------------------------------------------------------
// <copyright file = "IStandaloneHelperUgpReserve.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    /// <summary>
    /// Standalone helper interface for UGP Reserve.
    /// </summary>
    public interface IStandaloneHelperUgpReserve
    {
        /// <summary>
        /// Sets the reserve configuration.
        /// </summary>
        /// <param name="isReserveAllowedWithCredits">
        /// The flag indicating if reserve is allowed with creidts.
        /// </param>
        /// <param name="isReserveAllowedWithoutCredits">
        /// The flag indicating if reserve is allowed without creidts.
        /// </param>
        /// <param name="reserveTimeWithCreditsMilliseconds">
        /// The reserve time with credits, in milliseconds.
        /// </param>
        /// <param name="reserveTimeWithoutCreditsMilliseconds">
        /// The reserve time without credits, in milliseconds.
        /// </param>
        void SetReserveConfiguration(
                                bool isReserveAllowedWithCredits, bool isReserveAllowedWithoutCredits,
                                long reserveTimeWithCreditsMilliseconds, long reserveTimeWithoutCreditsMilliseconds);
    }
}
