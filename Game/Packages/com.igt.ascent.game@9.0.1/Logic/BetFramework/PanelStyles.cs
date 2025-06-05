//-----------------------------------------------------------------------
// <copyright file = "PanelStyles.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework
{
    /// <summary>
    /// An enumeration of standard bet panel styles.
    /// </summary>
    public enum PanelStyles
    {
        /// <summary>
        /// Wagerable item (line) selection on top row, wager-per-item selection on bottom row.
        /// </summary>
        LinesAndCoins,

        /// <summary>
        /// Only bottom five buttons are used, each selecting wagerable items and wager-per-item.
        /// </summary>
        Commit5Button,

        /// <summary>
        /// All ten buttons are used, each selecting wagerable items and wager-per-item.
        /// </summary>
        Commit10Button,

        /// <summary>
        /// Custom button panel. No predefined layout.
        /// </summary>
        Custom,

        /// <summary>
        /// Five lines buttons, and more than six wager-per-item buttons.
        /// </summary>
        AdvancedLinesAndCoins,

        /// <summary>
        /// Six or more buttons, each selecting wagerable items and wager-per-item.
        /// </summary>
        AdvancedCommit
    }
}
