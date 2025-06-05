//-----------------------------------------------------------------------
// <copyright file = "LightStartSequenceCommandPayload.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Payload to Light Start Sequence command that is sent WITHOUT additional data.
    /// </summary>
    /// <remarks>
    /// The "start Sequence without additional data" command is essentially the
    /// "start sequence with additional data" command with a fixed size (2 bytes)
    /// of data.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class LightStartSequenceCommandPayload : LightStartSequenceWithDataCommandPayload
    {
        /// <summary>
        /// The device driver reads 32 bits for getting the sequence number,
        /// so a fixed size of empty data (2 bytes) must be placed in the
        /// upper two bytes of that 32 bits value.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        private readonly byte[] placeholder = new byte[2];

        /// <summary>
        /// Initialize an instance of <see cref="LightStartSequenceCommandPayload"/>.
        /// </summary>
        /// <param name="targetDevice">The light group to control.</param>
        /// <param name="transitionMode">How light is transitioned.</param>
        /// <param name="sequenceNumber">The pre-determined sequence id.</param>
        public LightStartSequenceCommandPayload(byte targetDevice, TransitionMode transitionMode, ushort sequenceNumber)
            : base(targetDevice, transitionMode, sequenceNumber)
        {
        }

        #region IPackable Overrides

        /// <inheritdoc/>
        public override void Pack(Stream stream)
        {
            base.Pack(stream);
            stream.Write(placeholder, 0, placeholder.Length);
        }

        #endregion
    }
}
