// -----------------------------------------------------------------------
// <copyright file = "BaseShellIdlePresentationAction.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// Presentation actions supported by <see cref="BaseShellIdleState"/>.
    /// </summary>
    public enum BaseShellIdlePresentationAction
    {
        /// <summary>
        /// Starts a theme in a new coplayer.
        /// </summary>
        StartNewTheme,

        /// <summary>
        /// Switches a coplayer to load a new theme.
        /// </summary>
        SwitchCoplayerTheme,

        /// <summary>
        /// Shuts down a running coplayer.
        /// </summary>
        ShutDownCoplayer,

        /// <summary>
        /// Requests to display the chooser.
        /// </summary>
        RequestChooser,

        /// <summary>
        /// Request to cashout by player.
        /// </summary>
        RequestCashout,

        /// <summary>
        /// Request to add credits. Only valid in development/show mode.
        /// </summary>
        AddCredits,
    }
}