// -----------------------------------------------------------------------
// <copyright file = "SessionTrackingOption.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    /// <summary>
    /// Defines the Session tracking options.
    /// </summary>
    public enum SessionTrackingOption
    {
        /// <summary>
        /// Session tracking is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Session tracking is viewable.
        /// </summary>
        Viewable = 1,

        /// <summary>
        /// Session tracking is controlled by player.
        /// </summary>
        PlayerControlled = 2,
    }
}
