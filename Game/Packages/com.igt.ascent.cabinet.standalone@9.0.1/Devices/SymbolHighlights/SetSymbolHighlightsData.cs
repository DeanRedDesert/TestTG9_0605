// -----------------------------------------------------------------------
// <copyright file = "SetSymbolHighlightsData.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.SymbolHighlights
{
    using System;
    using System.Collections.Generic;
    using Cabinet.SymbolHighlights;
    using Presentation.PeripheralLights.Streaming;

    /// <summary>
    /// Class used to format symbol highlight data.
    /// </summary>
    internal static class SetSymbolHighlightsData
    {
        /// <summary>
        /// The byte length of symbol tracking data.
        /// </summary>
        private const byte SymbolTrackingDataLength = 0x08;

        /// <summary>
        /// The byte length of hot position data.
        /// </summary>
        private const byte HotPositionDataLength = 0x08;

        /// <summary>
        /// Code used to determine the symbol tracking highlight type.
        /// </summary>
        private const byte SymbolTrackingTypeCode = 0x00;

        /// <summary>
        /// Code used to determine the hot position highlight type.
        /// </summary>
        private const byte HotPositionTypeCode = 0x01;

        /// <summary>
        /// Generate byte data for the set symbol highlight command.
        /// </summary>
        /// <param name="symbolTrackingData">Tracking data to send.</param>
        /// <param name="hotPositionData">Hot position data to send.</param>
        /// <param name="crcValue">Output CRC16 value of data.</param>
        /// <returns>Byte array of data.</returns>
        public static byte[] GenerateByteData(IEnumerable<SymbolTrackingData> symbolTrackingData, 
            IEnumerable<SymbolHotPositionData> hotPositionData, out ushort crcValue)
        {
            var returnData = new List<byte>();

            foreach(var tracking in symbolTrackingData)
            {
                // Length of highlight.
                returnData.AddRange(BitConverter.GetBytes(SymbolTrackingDataLength));

                // Highlight type.
                returnData.Add(SymbolTrackingTypeCode);

                // Reel Index.
                returnData.Add((byte)tracking.ReelIndex);

                // Reel Stop.
                returnData.Add((byte)tracking.ReelStop);

                //Tracking row.
                returnData.Add((byte)tracking.RowToTrack);

                // Tracking color.
                returnData.AddRange(BitConverter.GetBytes(tracking.Color.PackedColor));
            }

            foreach(var hotPosition in hotPositionData)
            {
                // Length of highlight.
                returnData.AddRange(BitConverter.GetBytes(HotPositionDataLength));

                // Highlight type.
                returnData.Add(HotPositionTypeCode);

                // Reel Index.
                returnData.Add((byte)hotPosition.ReelIndex);

                // Reel Stop.
                returnData.Add((byte)hotPosition.ReelStop);

                // Hot position color.
                returnData.AddRange(BitConverter.GetBytes(hotPosition.Color.PackedColor));

                // Window stop index.
                returnData.Add((byte)hotPosition.WindowStopIndex);
            }

            // Format header bytes as it would be in the command. 
            var headerBytes = new List<byte>
            {
                (byte)(returnData.Count & 0xFF),
                (byte)((returnData.Count & 0xFF00) >> 8)
            };

            // Get CRC16 value.
            crcValue = Crc16Calculation.Hash(headerBytes, returnData);

            return returnData.ToArray();
        }
    }
}