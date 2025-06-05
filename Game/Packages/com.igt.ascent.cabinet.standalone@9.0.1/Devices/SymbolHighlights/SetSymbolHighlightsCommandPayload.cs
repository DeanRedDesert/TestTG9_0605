// -----------------------------------------------------------------------
// <copyright file = "SetSymbolHighlightsCommandPayload.cs" company = "IGT">
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
    /// Send symbol highlights to the device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class SetSymbolHighlightsCommandPayload : UsbCommandPayload
    {
        private readonly ushort numberOfHighlights;

        private readonly ushort crcValue;

        /// <summary>
        /// Constructs for <seealso cref="SetSymbolHighlightsCommandPayload"/>.
        /// </summary>
        /// <param name="targetDevice">Targeted device.</param>
        /// <param name="numberOfHighlights"></param>
        /// <param name="crcValue">CRC16 of associated data.</param>
        public SetSymbolHighlightsCommandPayload(byte targetDevice, ushort numberOfHighlights, ushort crcValue) 
            : base((byte)StreamingLightCommandCode.SetSymbolHighlights, targetDevice)
        {

            this.numberOfHighlights = numberOfHighlights;
            this.crcValue = crcValue;
        }

        #region IPackable Overrides

        /// <inheritdoc />
        public override void Pack(Stream stream)
        {
            base.Pack(stream);

            stream.Write(BitConverter.GetBytes(numberOfHighlights), 0, 2);
            stream.Write(BitConverter.GetBytes(crcValue), 0, 2);
        }

        #endregion
    }
}