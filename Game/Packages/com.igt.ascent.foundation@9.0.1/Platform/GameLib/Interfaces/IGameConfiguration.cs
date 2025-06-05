//-----------------------------------------------------------------------
// <copyright file = "IGameConfiguration.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for providing static game configuration information.
    /// </summary>
    public interface IGameConfiguration
    {
        /// <summary>
        /// Get the languages supported by the game.
        /// </summary>
        ICollection<string> AvailableLanguages { get; }
    }
}