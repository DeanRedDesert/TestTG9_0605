// -----------------------------------------------------------------------
// <copyright file = "LineSelectionMode.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    /// <summary>
    /// The configurable line selection mode.
    /// </summary>
    public enum LineSelectionMode
    {
        /// <summary>
        /// Game has not specified the line selection mode.
        /// </summary>
        Undefined,

        /// <summary>
        /// Player can not choose the amount of lines to bet on.
        /// </summary>
        Forced,

        /// <summary>
        /// Player can choose the amount of lines to bet on.
        /// </summary>
        PlayerSelectable,
    }
}