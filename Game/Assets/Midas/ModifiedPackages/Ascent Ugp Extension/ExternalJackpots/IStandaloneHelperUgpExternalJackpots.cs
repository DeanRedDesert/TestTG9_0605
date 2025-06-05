//-----------------------------------------------------------------------
// <copyright file = "IStandaloneHelperUgpExternalJackpots.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System.Collections.Generic;

    /// <summary>
    /// Standalone helper interface for UGP external jackpots.
    /// </summary>
    public interface IStandaloneHelperUgpExternalJackpots
    {
        /// <summary>
        /// Standalone helper function to set external jackpots.
        /// </summary>
        /// <param name="isVisible">The flag indicating if the jackpots should be shown.</param>
        /// <param name="iconId">The icon id to use.</param>
        /// <param name="jackpots">The jackpots to show.</param>
        void SetExternalJackpots(bool isVisible, int iconId, List<ExternalJackpot> jackpots);
    }
}
