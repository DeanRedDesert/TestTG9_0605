//-----------------------------------------------------------------------
// <copyright file = "ShowEnvironment.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    /// <summary>
    /// Enumerator for the Show Environment.
    /// </summary>
    public enum ShowEnvironment
    {
        /// <summary>
        /// Used when the foundation cannot determine if the environment is Development or Show.
        /// </summary>
        Invalid,
        /// <summary>
        /// Development is for internal/development use.
        /// </summary>
        Development,
        /// <summary>
        /// Show is for demonstration of the theme closer to a finished product.
        /// </summary>
        Show,
    }
}
