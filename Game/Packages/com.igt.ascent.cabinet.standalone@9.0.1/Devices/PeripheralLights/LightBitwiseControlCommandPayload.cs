//-----------------------------------------------------------------------
// <copyright file = "LightBitwiseControlCommandPayload.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using IgtUsbDevice;

    /// <summary>
    /// Payload to Light Bitwise Control command.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class LightBitwiseControlCommandPayload : UsbCommandPayload
    {
        private readonly ushort startingLight;
        private readonly byte bitsPerLight;

        /// <summary>
        /// Initialize an instance of <see cref="LightBitwiseControlCommandPayload"/>.
        /// </summary>
        /// <param name="targetDevice">The light group to control.</param>
        /// <param name="startingLight">The first light to control.</param>
        /// <param name="bitsPerLight">The number of bits of control per light.</param>
        public LightBitwiseControlCommandPayload(byte targetDevice, ushort startingLight, byte bitsPerLight)
            : base((byte)LightCommandCode.BitwiseControl, targetDevice)
        {
            this.startingLight = startingLight;
            this.bitsPerLight = bitsPerLight;
        }

        #region IPackable Overrides

        /// <inheritdoc/>
        public override void Pack(Stream stream)
        {
            base.Pack(stream);
            stream.Write(BitConverter.GetBytes(startingLight), 0, sizeof(ushort));
            stream.WriteByte(bitsPerLight);
        }

        #endregion
    }
}
