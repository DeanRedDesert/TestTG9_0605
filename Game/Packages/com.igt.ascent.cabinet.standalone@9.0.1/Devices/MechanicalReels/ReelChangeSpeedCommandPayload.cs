//-----------------------------------------------------------------------
// <copyright file = "ReelChangeSpeedCommandPayload.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Cabinet.MechanicalReels;
    using IgtUsbDevice;

    /// <summary>
    /// Payload for the Change Speed command.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class ReelChangeSpeedCommandPayload : UsbCommandPayload
    {
        /// <summary>
        ///  The speed index is only in the first 6 bits. Mask out any others just to be safe.
        /// </summary>
        private const byte SpeedIndexBitMask = 0x3F;

        /// <summary>
        /// The passed in speed index.
        /// </summary>
        private readonly byte speedIndex;

        /// <summary>
        /// The requested time in MS that the reel should take to change to the target speed.
        /// </summary>
        private readonly ushort period;

        /// <summary>
        /// Initialize an instance of <see cref="ReelChangeSpeedCommandPayload"/>.
        /// </summary>
        /// <param name="targetDevice">The reel to apply the profile info. to.</param>
        /// <param name="changeSpeedProfile">The profile info. of type <see cref="ChangeSpeedProfile"/>.</param>
        public ReelChangeSpeedCommandPayload(byte targetDevice, ChangeSpeedProfile changeSpeedProfile)
            : base((byte)ReelCommandCode.ChangeSpeed, targetDevice)
        {
            // Period is a ushort, 2 bytes.
            period = changeSpeedProfile.Period;

            // Bit pack the speed index byte with the upper two bits according to direction and immediate values.
            speedIndex=(byte)(changeSpeedProfile.SpeedIndex & SpeedIndexBitMask);
            speedIndex=(byte)(speedIndex | (changeSpeedProfile.Direction == ReelDirection.Descending ? 1 << 6 : 0 << 6));
            speedIndex=(byte)(speedIndex | (changeSpeedProfile.Immediate ? 1 << 7 : 0 << 7));
        }

        #region IPackable Overrides

        /// <inheritdoc/>
        public override void Pack(Stream stream)
        {
            base.Pack(stream);
            stream.WriteByte(speedIndex);
            stream.WriteByte(Convert.ToByte(period & 0x00FF));
            stream.WriteByte(Convert.ToByte((period & 0xFF00) >> 8));
        }

        #endregion
    }
}