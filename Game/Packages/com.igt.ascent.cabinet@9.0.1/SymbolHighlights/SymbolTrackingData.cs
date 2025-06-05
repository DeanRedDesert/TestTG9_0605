// -----------------------------------------------------------------------
// <copyright file = "SymbolTrackingData.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.SymbolHighlights
{
    /// <summary>
    /// Data class used for sending symbol tracking commands. Symbol tracking is when a reel's back light LEDs 
    /// follow a symbol as it spins by the visible reel window.
    /// </summary>
    public class SymbolTrackingData
    {
        /// <summary>
        /// Gets the reel index of the tracked symbol. Zero being the leftmost reel.
        /// </summary>
        public int ReelIndex { get; }

        /// <summary>
        /// Gets the reel stop of the tracked symbol. Zero being the first reel stop.
        /// </summary>
        public int ReelStop { get; }

        /// <summary>
        /// Gets the color of the tracked symbol.
        /// </summary>
        public Rgb15 Color { get; }

        /// <summary>
        /// Gets the row(s) of each symbol stop that is performing symbol tracking.
        /// </summary>
        public SymbolTrackingRow RowToTrack { get; }

        /// <summary>
        /// Create and instance of <see cref="SymbolTrackingData"/>.
        /// </summary>
        /// <param name="reelIndex">Reel index to track.</param>
        /// <param name="reelStop">Reel stop to track.</param>
        /// <param name="color">Color of the tracked symbol.</param>
        /// <param name="rowToTrack">Row(s) that the tracked symbol will display when in the visible reel window.</param>
        public SymbolTrackingData(int reelIndex, int reelStop, Rgb15 color, SymbolTrackingRow rowToTrack)
        {
            ReelIndex = reelIndex;
            ReelStop = reelStop;
            Color = color;
            RowToTrack = rowToTrack;
        }
    }
}