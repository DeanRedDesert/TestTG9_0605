// -----------------------------------------------------------------------
// <copyright file = "ClearSymbolHighlightsCommandPayload.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.SymbolHighlights
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using IgtUsbDevice;
    using StreamingLights;

    /// <summary>
    /// Command for clearing symbol highlights. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class ClearSymbolHighlightsCommandPayload : UsbCommandPayload
    {
        private readonly ushort numberOfHighlightsToClear;
        private readonly ushort clearDataCrc;

        /// <summary>
        /// Constructor for <see cref="ClearSymbolHighlightsCommandPayload"/> that specifies which reel to clear.
        /// </summary>
        /// <param name="targetDevice">Targeted device.</param>
        /// <param name="numberOfHighlightsToClear">Number of highlights to clear.</param>
        /// <param name="clearDataCrc">CRC value of associated clear data.</param>
        public ClearSymbolHighlightsCommandPayload(byte targetDevice, ushort numberOfHighlightsToClear, ushort clearDataCrc)
            : base((byte)StreamingLightCommandCode.ClearSymbolHighlights, targetDevice)
        {
            this.numberOfHighlightsToClear = numberOfHighlightsToClear;
            this.clearDataCrc = clearDataCrc;
        }

        #region IPackable Overrides

        /// <inheritdoc />
        public override void Pack(Stream stream)
        {
            base.Pack(stream);

            stream.Write(BitConverter.GetBytes(numberOfHighlightsToClear), 0, 2);
            stream.Write(BitConverter.GetBytes(clearDataCrc), 0, 2);
        }

        #endregion
    }
}