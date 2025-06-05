//-----------------------------------------------------------------------
// <copyright file = "AscribedGameType.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;

    /// <summary>
    /// The enumeration defines the types of the ascribed game.
    /// </summary>
    [Serializable]
    public enum AscribedGameType
    {
        /// <summary>
        /// Indicates that the ascribed game is a Theme.
        /// </summary>
        Theme,

        /// <summary>
        /// Indicates that the ascribed game is a Shell.
        /// </summary>
        Shell,
    }
}
