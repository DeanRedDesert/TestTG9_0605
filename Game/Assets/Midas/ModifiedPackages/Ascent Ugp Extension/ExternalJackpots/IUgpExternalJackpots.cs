//-----------------------------------------------------------------------
// <copyright file = "IUgpExternalJackpots.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;

    /// <summary>
    /// Defines an interface for the UGP external jackpots.
    /// </summary>
    public interface IUgpExternalJackpots
    {
        /// <summary>
        /// Gets the data for the external jackpots.
        /// </summary>
		ExternalJackpots GetExternalJackpots();

        /// <summary>
        /// Event rasied when the external jackpot has changed.
        /// </summary>
        event EventHandler<ExternalJackpotChangedEventArgs> ExternalJackpotChanged;
    }
}
