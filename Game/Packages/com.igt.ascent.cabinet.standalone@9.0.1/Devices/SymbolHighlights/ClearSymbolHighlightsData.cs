// -----------------------------------------------------------------------
// <copyright file = "ClearSymbolHighlightsData.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.SymbolHighlights
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cabinet.SymbolHighlights;
    using Presentation.PeripheralLights.Streaming;

    /// <summary>
    /// Static class that formats byte data for clear symbol highlight commands.
    /// </summary>
    internal static class ClearSymbolHighlightsData
    {        
        /// <summary>
        /// The byte length of symbol tracking data.
        /// </summary>
        private const byte SymbolTrackingDataLength = 0x06;

        /// <summary>
        /// The byte length of hot position data.
        /// </summary>
        private const byte HotPositionDataLength = 0x06;

        /// <summary>
        /// Generate byte data for a clear symbol highlight command.
        /// </summary>
        /// <param name="featuresToClear">Features to clear.</param>
        /// <param name="reelIndex">Reel to clear, 0xFF being all reels.</param>
        /// <param name="crcValue">CRC value of the given data.</param>
        /// <returns>Byte data of the clear symbol highlights command.</returns>
        public static byte[] GenerateByteData(IList<SymbolHighlightFeature> featuresToClear, byte reelIndex, out ushort crcValue)
        {
            var clearData = new List<byte>();
            var featuresList = featuresToClear.ToList();

            if(featuresList.Contains(SymbolHighlightFeature.SymbolTracking))
            {
                // Length of data element.
                clearData.AddRange(BitConverter.GetBytes(SymbolTrackingDataLength));

                // Highlight type (zero is symbol tracking).
                clearData.Add(0);

                // Reel index.
                clearData.Add(reelIndex);

                // Reel stop index (clear all stops).
                clearData.Add(0xFF);

                // Rows of the stop (clear all rows).
                clearData.Add(0xFF);
            }

            if(featuresList.Contains(SymbolHighlightFeature.SymbolHotPosition))
            {
                // Length of data element.
                clearData.AddRange(BitConverter.GetBytes(HotPositionDataLength));

                // Highlight type (one is hot position).
                clearData.Add(1);

                // Reel index.
                clearData.Add(reelIndex);

                // Reel stop index (clear all stops).
                clearData.Add(0xFF);

                // Window position (clear all positions).
                clearData.Add(0xFF);
            }

            // Format header bytes as it would be in the command. 
            var headerBytes = new List<byte>
            {
                (byte)(featuresList.Count & 0xFF),
                (byte)((featuresList.Count & 0xFF00) >> 8)
            };

            // Get CRC16 value.
            crcValue = Crc16Calculation.Hash(headerBytes, clearData);

            return clearData.ToArray();
        }
    }
}