// -----------------------------------------------------------------------
// <copyright file = "CallSite.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing
{
    /// <summary>
    /// This enumeration identifies the place where a tracing event is emitted.
    /// </summary>
    /// <remarks>
    /// The enumeration value is in the order of priority, from low to high.
    /// Events emitted from a lower priority site could be overwritten by
    /// the ones from a higher priority site.
    /// </remarks>
    public enum CallSite
    {
        /// <summary>
        /// The tracing event is emitted by a SDK code.
        /// </summary>
        SDK,

        /// <summary>
        /// The tracing event is emitted by a game specific code.
        /// </summary>
        Game
    }
}