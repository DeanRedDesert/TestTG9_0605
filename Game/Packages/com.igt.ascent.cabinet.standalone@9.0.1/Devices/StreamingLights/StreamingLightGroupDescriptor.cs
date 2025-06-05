//-----------------------------------------------------------------------
// <copyright file = "StreamingLightGroupDescriptor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;
    using IgtUsbDevice;

    /// <summary>
    /// The descriptor for a streaming light group.
    /// </summary>
    internal class StreamingLightGroupDescriptor : IUnpackable
    {
        #region Constants

        /// <summary>
        /// The size of the light descriptor record that is unpacked using <see cref="IUnpackable"/>.
        /// </summary>
        private const byte RecordLength = 7;

        /// <summary>
        /// The bit mask for the real time control supported flag.
        /// This flag is controlled by the first bit of the attributes field.
        /// </summary>
        private const ushort RealTimeControlSupportedMask = 0x0001;

        /// <summary>
        /// The bit mask for the set overall intensity control supported flag.
        /// This flag is controlled by the second bit of the attributes field.
        /// </summary>
        private const ushort IntensityControlSupportedMask = 0x0002;

        /// <summary>
        /// The bit mask for the set overall intensity control supported flag.
        /// This flag is controlled by the third bit of the attributes field.
        /// </summary>
        private const ushort SymbolHighlightSupportedMask = 0x0004;

        #endregion

        #region Properties

        /// <summary>
        /// The number of lights in the group.
        /// </summary>
        public ushort NumberOfLights
        {
            get;
            private set;
        }

        /// <summary>
        /// The size of the frame queue on the device.
        /// </summary>
        public ushort NumberOfFramesSupported
        {
            get;
            private set;
        }

        /// <summary>
        /// The attributes of the group.
        /// </summary>
        public ushort Attributes
        {
            get;
            private set;
        }

        /// <summary>
        /// If the group supports real time control.
        /// </summary>
        public bool RealTimeControlSupported
        {
            get;
            private set;
        }

        /// <summary>
        /// If the group supports intensity control.
        /// </summary>
        public bool IntensityControlSupported
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets if the group supports symbol highlights.
        /// </summary>
        public bool SymbolHighlightsSupported
        {
            get;
            private set;
        }

        #endregion

        #region IUnpackable Members

        /// <inheritdoc />
        public int DataSize => RecordLength;

        /// <inheritdoc />
        public void Unpack(byte[] buffer, int offset)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if(offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if(buffer[offset] != RecordLength)
            {
                throw new InvalidUsbDeviceDataException("The record length for the light group descriptor is invalid.");
            }

            offset++;
            NumberOfLights = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);
            Attributes = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);
            NumberOfFramesSupported = BitConverter.ToUInt16(buffer, offset);

            RealTimeControlSupported = (Attributes & RealTimeControlSupportedMask) == RealTimeControlSupportedMask;
            IntensityControlSupported = (Attributes & IntensityControlSupportedMask) == IntensityControlSupportedMask;
            SymbolHighlightsSupported = (Attributes & SymbolHighlightSupportedMask) == SymbolHighlightSupportedMask;
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"Number of Lights: {NumberOfLights} / Number of Frames: {NumberOfFramesSupported} / Attributes 0x{Attributes:X4}";
        }
    }
}
