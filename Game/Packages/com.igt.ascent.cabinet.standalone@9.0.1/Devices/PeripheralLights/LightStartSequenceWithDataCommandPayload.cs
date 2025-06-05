//-----------------------------------------------------------------------
// <copyright file = "LightStartSequenceWithDataCommandPayload.cs" company = "IGT">
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
    /// Payload to Light Start Sequence command that is sent WITH additional data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class LightStartSequenceWithDataCommandPayload : UsbCommandPayload
    {
        private readonly TransitionMode transitionMode;
        private readonly ushort sequenceNumber;

        /// <summary>
        /// Initialize an instance of <see cref="LightStartSequenceWithDataCommandPayload"/>.
        /// </summary>
        /// <param name="targetDevice">The light group to control.</param>
        /// <param name="transitionMode">How light is transitioned.</param>
        /// <param name="sequenceNumber">The pre-determined sequence id.</param>
        public LightStartSequenceWithDataCommandPayload(byte targetDevice, TransitionMode transitionMode, ushort sequenceNumber)
            : base((byte)LightCommandCode.StartSequence, targetDevice)
        {
            this.transitionMode = transitionMode;
            this.sequenceNumber = sequenceNumber;
        }

        #region IPackable Overrides

        /// <inheritdoc/>
        public override void Pack(Stream stream)
        {
            base.Pack(stream);
            stream.WriteByte((byte)transitionMode);
            stream.Write(BitConverter.GetBytes(sequenceNumber), 0, sizeof(ushort));
        }

        #endregion
    }
}
