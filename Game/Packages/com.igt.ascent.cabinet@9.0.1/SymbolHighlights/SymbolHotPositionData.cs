// -----------------------------------------------------------------------
// <copyright file = "SymbolHotPositionData.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Communication.Cabinet.SymbolHighlights
{
    /// <summary>
    /// Data class for symbol hot positions. Hot positions will flash the given color 
    /// across the entire reel when the symbol intersects its window stop index.
    /// </summary>
    public class SymbolHotPositionData
    {
        /// <summary>
        /// Gets the reel index of the hot position. Zero being the leftmost reel.
        /// </summary>
        public int ReelIndex { get; }

        /// <summary>
        /// Gets the reel stop of the hot position. Zero being the first reel stop.
        /// </summary>
        public int ReelStop { get; }

        /// <summary>
        /// Gets the color of the hot position.
        /// </summary>
        public Rgb15 Color { get; }

        /// <summary>
        /// Gets the window stop when the hot position will be activated. Zero being the top window stop, 
        /// four being the bottom window stop.
        /// </summary>
        public int WindowStopIndex { get; }

        /// <summary>
        /// Create an instance of <see cref="SymbolHotPositionData"/>.
        /// </summary>
        /// <param name="reelIndex">Reel index to track.</param>
        /// <param name="reelStop">Reel stop to track.</param>
        /// <param name="color">Color of the tracked symbol.</param>
        /// <param name="windowStopIndex">Window index that triggers the hot position lights.</param>
        public SymbolHotPositionData(int reelIndex, int reelStop, Rgb15 color, int windowStopIndex)
        {
            ReelIndex = reelIndex;
            ReelStop = reelStop;
            Color = color;
            WindowStopIndex = windowStopIndex;
        }
    }
}