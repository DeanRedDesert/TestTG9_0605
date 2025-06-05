// -----------------------------------------------------------------------
// <copyright file = "SymbolHighlightFeature.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.SymbolHighlights
{
    /// <summary>
    /// Features that can be used by Symbol Highlights.
    /// </summary>
    public enum SymbolHighlightFeature
    {
        /// <summary>
        /// Symbol tracking feature. Symbol tracking is when a reel's back light LEDs 
        /// follow a symbol as it spins by the visible reel window.
        /// </summary>
        SymbolTracking,

        /// <summary>
        /// Hot position feature. Hot positions will flash the given color across 
        /// the entire reel when the symbol intersects its window stop index.
        /// </summary>
        SymbolHotPosition
    }
}