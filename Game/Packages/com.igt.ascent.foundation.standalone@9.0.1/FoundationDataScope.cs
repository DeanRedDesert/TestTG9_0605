//-----------------------------------------------------------------------
// <copyright file = "FoundationDataScope.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    /// <summary>
    /// The FoundationDataScope enumeration is used to represent
    /// the different available safe storage for the data
    /// maintained by the Foundation.
    /// </summary>
    internal enum FoundationDataScope
    {
        /// <summary>
        /// Used by the foundation to store information about the current game cycle.
        /// </summary>
        GameCycle = 0,

        /// <summary>
        /// Used for items local to a particular paytable variant.
        /// </summary>
        PayVar = 1,

        /// <summary>
        /// Used for items local to a particular theme.
        /// </summary>
        Theme = 2
    }
}
