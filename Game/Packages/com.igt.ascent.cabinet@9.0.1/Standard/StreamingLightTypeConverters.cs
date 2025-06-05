// -----------------------------------------------------------------------
// <copyright file = "StreamingLightTypeConverters.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// Internal components made visible to IGT.Game.Core.Communication.Cabinet.Standard.Tests for unit tests.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("IGT.Game.Core.Communication.Cabinet.Standard.Tests")]

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using SymbolHighlights;
    using Schema = CSI.Schemas.Internal;

    /// <summary>
    /// Static class for converting between CSI schema highlight types and Cabinet highlight types.
    /// </summary>
    internal static class StreamingLightTypeConverters
    {
        /// <summary>
        /// Convert Cabinet symbol tracking data to CSI Schema.
        /// </summary>
        /// <param name="source">Cabinet symbol tracking data.</param>
        /// <returns>CSI Schema symbol tracking data.</returns>
        public static Schema.SymbolTrackingData ToInternal(this SymbolTrackingData source)
        {
            return
                new Schema.SymbolTrackingData
                {
                    ReelIndex = Convert.ToByte(source.ReelIndex),
                    ReelStop = Convert.ToByte(source.ReelStop),
                    Color = source.Color.PackedColor,
                    RowIndex = Convert.ToByte((int)source.RowToTrack)
                };
        }

        /// <summary>
        /// Convert CSI Schema symbol tracking data to Cabinet.
        /// </summary>
        /// <param name="source">CSI Schema symbol tracking data.</param>
        /// <returns>Cabinet symbol tracking data.</returns>
        public static SymbolTrackingData ToPublic(this Schema.SymbolTrackingData source)
        {
            return new SymbolTrackingData(source.ReelIndex,
                                          source.ReelStop,
                                          new Rgb15(source.Color),
                                          (SymbolTrackingRow)source.RowIndex);
        }

        /// <summary>
        /// Convert Cabinet symbol hot position data to CSI Schema.
        /// </summary>
        /// <param name="source">Cabinet symbol hot position data.</param>
        /// <returns>CSI Schema symbol hot position data.</returns>
        public static Schema.SymbolHotPositionData ToInternal(this SymbolHotPositionData source)
        {
            return
                new Schema.SymbolHotPositionData
                {
                    ReelIndex = Convert.ToByte(source.ReelIndex),
                    ReelStop = Convert.ToByte(source.ReelStop),
                    WindowStopIndex = Convert.ToByte(source.WindowStopIndex),
                    Color = source.Color.PackedColor
                };
        }

        /// <summary>
        /// Convert CSI Schema symbol hot position data to Cabinet.
        /// </summary>
        /// <param name="source">CSI Schema symbol hot position data.</param>
        /// <returns>Cabinet symbol hot position data.</returns>
        public static SymbolHotPositionData ToPublic(this Schema.SymbolHotPositionData source)
        {
            return
                new SymbolHotPositionData(source.ReelIndex,
                                          source.ReelStop,
                                          new Rgb15(source.Color),
                                          source.WindowStopIndex);
        }

        /// <summary>
        /// Convert Cabinet symbol highlight feature to CSI Schema.
        /// </summary>
        /// <param name="source">Cabinet symbol highlight feature.</param>
        /// <returns>CSI Schema symbol highlight feature.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is not a valid enumeration.</exception>
        public static Schema.SymbolHighlightFeatureTypes ToInternal(this SymbolHighlightFeature source)
        {
            switch(source)
            {
                case SymbolHighlightFeature.SymbolTracking:
                    return Schema.SymbolHighlightFeatureTypes.SymbolTracking;

                case SymbolHighlightFeature.SymbolHotPosition:
                    return Schema.SymbolHighlightFeatureTypes.HotPosition;

                default:
                    throw new ArgumentException("source cannot be converted because it is not a valid enumeration.", nameof(source));
            }
        }

        /// <summary>
        /// Convert CSI Schema symbol highlight feature to Cabinet.
        /// </summary>
        /// <param name="source">CSI Schema symbol highlight feature.</param>
        /// <returns>Cabinet symbol highlight feature.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is not a valid enumeration.</exception>
        public static SymbolHighlightFeature ToPublic(this Schema.SymbolHighlightFeatureTypes source)
        {
            switch(source)
            {
                case Schema.SymbolHighlightFeatureTypes.SymbolTracking:
                    return SymbolHighlightFeature.SymbolTracking;

                case Schema.SymbolHighlightFeatureTypes.HotPosition:
                    return SymbolHighlightFeature.SymbolHotPosition;

                default:
                    throw new ArgumentException("source cannot be converted because it is not a valid enumeration.", nameof(source));
            }
        }
    }
}
