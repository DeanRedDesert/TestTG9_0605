//-----------------------------------------------------------------------
// <copyright file = "IShowCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// Interface for games running in show mode. Currently only adding money is supported.
    /// </summary>
    public interface IShowCategory
    {
        /// <summary>
        /// Add money to a game running in Show mode.
        /// </summary>
        /// <param name="value">Amount in credits.</param>
        /// <param name="denomination">AVP - style denom ( 1 == 1 cent.) </param>
        /// <returns>True if message reply was ok ( no errors ), False otherwise.</returns>
        bool AddMoney(long value, long denomination);

        /// <summary>
        /// Returns the Show Environment.
        /// </summary>
        /// <returns>The current show environment of the Foundation. Either development, show or invalid.  
        /// Development is for internal/development use; 
        /// Show is for demonstration of the theme closer to a finished product; 
        /// Invalid is used when the foundation can't determine if the environment is Development or Show.
        /// </returns>
        ShowEnvironment GetShowEnvironment();
    }
}
