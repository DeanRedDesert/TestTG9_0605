//-----------------------------------------------------------------------
// <copyright file = "InterfaceExtensionDataScope.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    /// <summary>
    /// Used to represent the different available safe storage scopes for data maintained by interface extensions.
    /// </summary>
    public enum InterfaceExtensionDataScope
    {
        /// <summary>
        /// Used by the extension to store information about the current game cycle.
        /// </summary>
        GameCycle,

        /// <summary>
        /// Used for items local to a particular paytable variant.
        /// </summary>
        Payvar,

        /// <summary>
        /// Used for items local to a particular theme.
        /// </summary>
        Theme
    }
}
