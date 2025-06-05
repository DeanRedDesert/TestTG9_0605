//-----------------------------------------------------------------------
// <copyright file = "IUgpExternalJackpotsCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    /// <summary>
    /// Interface to accept callbacks from the UGP ExternalJackpots category.
    /// </summary>
    public interface IUgpExternalJackpotsCategoryCallbacks
    {
        /// <summary>
        /// Method called when UgpExternalJackpotsCategory UpdateJackpots message is received from the foundation.
        /// </summary>
        /// <param name="externalJackpots">
        /// The external jackpots received from the foundation.
        /// </param>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessUpdateJackpots(ExternalJackpots externalJackpots);
    }
}
