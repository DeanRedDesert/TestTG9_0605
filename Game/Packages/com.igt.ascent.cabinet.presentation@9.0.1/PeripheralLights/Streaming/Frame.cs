//-----------------------------------------------------------------------
// <copyright file = "Frame.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// Represents a light frame.
    /// </summary>
    public class Frame : IEquatable<Frame>, ICloneable
    {
        private const ushort FrameHeaderSize = 9;

        private List<byte> frameData;
        private ushort? crcValue;
        private List<Color> colorData;
        private ushort displayTime;
        private TransitionType transitionType;
        private ushort transitionTime;

        #region Constructors

        /// <summary>
        /// Creates a new empty frame.
        /// </summary>
        public Frame()
        {
            frameData = new List<byte>();
            DisplayTime = 33; // Set the default to 33ms which is about 30 FPS.
            TransitionType = TransitionType.Instant;
            colorData = null;
        }

        /// <summary>
        /// Creates a new frame from a list of colors.
        /// </summary>
        /// <param name="lights">The list of light colors for this frame.</param>
        public Frame(IList<Color> lights)
            : this()
        {
            frameData = RunLengthEncoding.Encode(lights);
            // Make a copy of the list passed in.
            colorData = new List<Color>(lights);
        }

        /// <summary>
        /// Creates a new frame from run length encoded data.
        /// </summary>
        /// <param name="lights">The RLE light data.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="lights"/> is null.
        /// </exception>
        public Frame(List<byte> lights)
            : this()
        {
            frameData = lights ?? throw new ArgumentNullException(nameof(lights));
        }

        #endregion

        #region Properties

        /// <summary>
        /// The time to display the frame in milliseconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the value is 0.
        /// </exception>
        public ushort DisplayTime
        {
            get => displayTime;
            set
            {
                if(value == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The display time cannot be 0.");
                }
                if(displayTime != value)
                {
                    crcValue = null;
                }
                displayTime = value;
            }
        }

        /// <summary>
        /// The type of transition to use when transitioning to this frame.
        /// </summary>
        public TransitionType TransitionType
        {
            get => transitionType;
            set
            {
                if(transitionType != value)
                {
                    crcValue = null;
                }
                transitionType = value;
            }
        }

        /// <summary>
        /// The time in milliseconds for the transition to take.
        /// </summary>
        public ushort TransitionTime
        {
            get => transitionTime;
            set
            {
                if(transitionTime != value)
                {
                    crcValue = null;
                }
                transitionTime = value;
            }
        }

        /// <summary>
        /// The run length encoded frame color data.
        /// </summary>
        public IList<byte> FrameData => frameData;

        /// <summary>
        /// The non-encoded frame color data.
        /// </summary>
        public List<Color> ColorFrameData => colorData ?? (colorData = RunLengthEncoding.Decode(frameData));

        /// <summary>
        /// Gets the CRC16 value of this frame.
        /// </summary>
        public ushort Crc16
        {
            get
            {
                if(!crcValue.HasValue)
                {
                    crcValue = CalculateCrc16();
                }
                return crcValue.Value;
            }
        }

        /// <summary>
        /// Gets the total display time in milliseconds for the frame including any
        /// transition time.
        /// </summary>
        public uint TotalDisplayTime => Convert.ToUInt32(DisplayTime + (TransitionType != TransitionType.Instant ? TransitionTime : 0));

        /// <summary>
        /// The length of the frame data.
        /// </summary>
        protected ushort Length => Convert.ToUInt16(FrameHeaderSize + frameData.Count);

        #endregion

        /// <inheritdoc />
        public bool Equals(Frame other)
        {
            var equals = false;
            if(other != null)
            {
                equals = ReferenceEquals(this, other) ||
                    (DisplayTime == other.DisplayTime &&
                    TransitionType == other.TransitionType &&
                    TransitionTime == other.TransitionTime &&
                    FrameData.SequenceEqual(other.FrameData));
            }
            return equals;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Frame);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Convert.ToInt32(Length)
                   ^Convert.ToInt32(DisplayTime)
                   ^Convert.ToInt32(TransitionTime)
                   ^Convert.ToInt32(TransitionType)
                   ^Convert.ToInt32(TotalDisplayTime);
        }

        #region Serialization Implementation

        /// <summary>
        /// Allows frame data to be written to different file versions.
        /// </summary>
        /// <param name="stream">Place to stream to.</param>
        /// <param name="version">The file version to serialize as.</param>
        // TODO: version is unused
        public void SerializeByVersion(System.IO.Stream stream, ushort version)
        {
            if(frameData.Count == 0)
            {
                throw new InvalidFrameException("Frames must have at least one light specified.");
            }

            CompactSerializer.Write(stream, Length);
            // Subtract 1 from the display time when writing it out because
            // that is how it is designed in the USB specification.
            CompactSerializer.Write(stream, (ushort)(DisplayTime - 1));
            CompactSerializer.Write(stream, (byte)TransitionType);
            CompactSerializer.Write(stream, TransitionTime);
            CompactSerializer.Write(stream, Crc16);
            // The list compact serializer function is not used because it adds
            // extra bytes that the hardware will reject.
            foreach(var frameByte in FrameData)
            {
                CompactSerializer.Write(stream, frameByte);
            }
        }

        /// <summary>
        /// Allows frame data to be read from different file versions.
        /// </summary>
        /// <param name="stream">Place to stream from.</param>
        /// <param name="version">The file version to deserialize as.</param>
        // TODO: version is unused
        public void DeserializeByVersion(System.IO.Stream stream, ushort version)
        {
            var length = CompactSerializer.ReadUshort(stream);
            DisplayTime = (ushort)(CompactSerializer.ReadUshort(stream) + 1);
            TransitionType = (TransitionType)CompactSerializer.ReadByte(stream);
            TransitionTime = CompactSerializer.ReadUshort(stream);
            crcValue = CompactSerializer.ReadUshort(stream);

            // The list compact serializer function is not used because it adds
            // extra bytes that the hardware will reject.
            var numberOfFrameBytes = length - FrameHeaderSize;
            frameData = new List<byte>();
            for(var count = 0; count < numberOfFrameBytes; count++)
            {
                frameData.Add(CompactSerializer.ReadByte(stream));
            }
        }

        #endregion

        #region ICloneable Implementation

        /// <inheritdoc />
        public virtual object Clone()
        {
            var newFrame = new Frame(frameData)
            {
                TransitionTime = TransitionTime,
                TransitionType = TransitionType,
                DisplayTime = DisplayTime,
                crcValue = crcValue,
                colorData = colorData?.ToList(),
            };

            return newFrame;
        }

        #endregion

        /// <summary>
        /// Update the list of colors in the frame.
        /// </summary>
        /// <param name="colors"></param>
        public void UpdateColors(IList<Color> colors)
        {
            colorData = new List<Color>(colors);
            frameData = RunLengthEncoding.Encode(colors);
            // The CRC value must be cleared here otherwise the frame is going to be invalid due to
            // the changed frame data.
            crcValue = null;
        }

        /// <summary>
        /// Calculate the CRC16 value of the frame.
        /// </summary>
        /// <returns>The calculated CRC value.</returns>
        protected virtual ushort CalculateCrc16()
        {
            var headerBytes = new List<byte>();

            void AddBytesToHeader(ushort data)
            {
                headerBytes.Add((byte)(data & 0xFF));
                headerBytes.Add((byte)((data & 0xFF00) >> 8));
            }

            AddBytesToHeader(Length);
            AddBytesToHeader((ushort)(DisplayTime - 1));
            headerBytes.Add((byte)TransitionType);
            AddBytesToHeader(TransitionTime);

            return Crc16Calculation.Hash(headerBytes, frameData);
        }
    }
}
