// -----------------------------------------------------------------------
// <copyright file = "SymbolTrackingRow.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.SymbolHighlights
{
    /// <summary>
    /// Designates the row(s) a tracked symbol will display when in the visible reel window.
    /// </summary>
    public enum SymbolTrackingRow
    {
        /// <summary>
        /// Track the top row of the symbol stop.
        /// </summary>
        Top = 0x00,

        /// <summary>
        /// Track the bottom row of the symbol stop.
        /// </summary>
        Bottom = 0x01,

        /// <summary>
        /// Track the bottom and top row of the symbol stop.
        /// </summary>
        All = 0xFF
    }
}