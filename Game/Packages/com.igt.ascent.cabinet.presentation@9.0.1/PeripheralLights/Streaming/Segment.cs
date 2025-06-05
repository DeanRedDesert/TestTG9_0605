//-----------------------------------------------------------------------
// <copyright file = "Segment.cs" company = "IGT">
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
    /// Represents a light sequence segment.
    /// </summary>
    public class Segment : IEnumerable<Frame>, IEquatable<Segment>
    {
        private readonly List<Frame> frames;
        private ulong? displayTime;
        private ushort loop;
        private KeyFrameTable keyFrameTable;

        /// <summary>
        /// Creates a new empty segment.
        /// </summary>
        public Segment()
        {
            frames = new List<Frame>();
            SequenceVersion = LightSequence.CurrentSequenceVersion;
        }

        #region Properties

        /// <summary>
        /// Gets and sets the loop count for the segment.
        /// A value of 0 means play the segment once.
        /// </summary>
        public ushort Loop
        {
            get => loop;
            set
            {
                if(loop != value)
                {
                    displayTime = null;
                }
                loop = value;
            }
        }

        /// <summary>
        /// The frames to display in this segment.
        /// </summary>
        public IList<Frame> Frames => frames.AsReadOnly();

        /// <summary>
        /// Gets the display time in milliseconds for the segment. This
        /// accounts for any loops specified.
        /// </summary>
        public ulong DisplayTime
        {
            get
            {
                if(!displayTime.HasValue)
                {
                    var frameDisplayTime = (ulong)Frames.Sum(frame => frame.TotalDisplayTime);
                    displayTime = frameDisplayTime * (uint)(Loop + 1);
                }

                return displayTime.Value;
            }
        }

        /// <summary>
        /// Gets or sets the version number of the light sequence this segment belongs to.
        /// </summary>
        internal ushort SequenceVersion { get; set; }

        /// <summary>
        /// Gets the number of key frames in the segment.
        /// </summary>
        internal int KeyFrameCount => keyFrameTable?.Count ?? 0;

        #endregion

        /// <inheritdoc />
        public bool Equals(Segment other)
        {
            var equal = false;
            if(other != null)
            {
                if(ReferenceEquals(this, other))
                {
                    equal = true;
                }
                else
                {
                    equal = Loop == other.Loop && Frames.SequenceEqual(other.Frames) &&
                        ((keyFrameTable == null && other.keyFrameTable == null) ||
                            keyFrameTable?.Equals(other.keyFrameTable) == true);
                }
            }
            return equal;
        }

        #region Serialization Implementation

        /// <summary>
        /// Allows segment date to be written to different file versions.
        /// </summary>
        /// <param name="stream">Place to stream to.</param>
        /// <param name="version">The file version to serialize as.</param>
        internal void SerializeByVersion(System.IO.Stream stream, ushort version)
        {
            CompactSerializer.Write(stream, Loop);

            var segmentsNull = frames == null;
            CompactSerializer.Write(stream, segmentsNull);
            if(!segmentsNull)
            {
                CompactSerializer.Write(stream, frames.Count);
                foreach(var frame in frames)
                {
                    var frameNull = frame == null;
                    CompactSerializer.Write(stream, frameNull);
                    if(!frameNull)
                    {
                        frame.SerializeByVersion(stream, version);
                    }
                    if(version <= 2)
                    {
                        CompactSerializer.Write(stream, (ushort)0xA5A5);
                    }
                }
            }
            if(version >= 3)
            {
                CompactSerializer.Write(stream, keyFrameTable);
            }
        }

        /// <summary>
        /// Allows segment date to be read from different file versions.
        /// </summary>
        /// <param name="stream">Place to stream from.</param>
        /// <param name="version">The file version to deserialize as.</param>
        internal void DeserializeByVersion(System.IO.Stream stream, ushort version)
        {
            SequenceVersion = version;
            Loop = CompactSerializer.ReadUshort(stream);

            if(!CompactSerializer.ReadBool(stream))
            {
                var frameCount = CompactSerializer.ReadInt(stream);
                frames.Clear();
                while(frameCount > 0)
                {
                    var framNull = CompactSerializer.ReadBool(stream);
                    if(!framNull)
                    {
                        var frame = new Frame();
                        frame.DeserializeByVersion(stream, version);
                        frames.Add(frame);
                    }
                    --frameCount;
                    if(version <= 2)
                    {
                        CompactSerializer.ReadUshort(stream);
                    }
                }
            }

            if(version >= 3)
            {
                keyFrameTable = CompactSerializer.ReadSerializable<KeyFrameTable>(stream);
            }
        }

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as Segment);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Convert.ToInt32(Loop)
                   ^Convert.ToInt32(DisplayTime);
        }

        #region IEnumerable Implementation

        /// <inheritdoc />
        public IEnumerator<Frame> GetEnumerator()
        {
            return frames.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return frames.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Add a frame to the segment at a specific index.
        /// </summary>
        /// <param name="index">The index to add the frame at.</param>
        /// <param name="newFrame">The frame to add.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="newFrame"/> is null.
        /// </exception>
        public void AddFrame(int index, Frame newFrame)
        {
            if(newFrame == null)
            {
                throw new ArgumentNullException(nameof(newFrame));
            }

            frames.Insert(index, newFrame);
            displayTime = null;
            PostChangedEvent();
        }

        /// <summary>
        /// Add a frame to the segment.
        /// </summary>
        /// <param name="newFrame">The frame to add.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="newFrame"/> is null.
        /// </exception>
        public void AddFrame(Frame newFrame)
        {
            if(newFrame == null)
            {
                throw new ArgumentNullException(nameof(newFrame));
            }

            frames.Add(newFrame);
            displayTime = null;
            PostChangedEvent();
        }

        /// <summary>
        /// Replaces a frame at a certain index with a new one.
        /// </summary>
        /// <param name="index">The index to replace the frame at.</param>
        /// <param name="newFrame">The new frame to replace with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="newFrame"/> is null.
        /// </exception>
        public void ReplaceFrame(int index, Frame newFrame)
        {
            frames[index] = newFrame ?? throw new ArgumentNullException(nameof(newFrame));
            displayTime = null;
            PostChangedEvent();
        }

        /// <summary>
        /// Remove a frame at a certain index.
        /// </summary>
        /// <param name="index">The frame index to remove.</param>
        public void RemoveFrame(int index)
        {
            frames.RemoveAt(index);
            displayTime = null;
            keyFrameTable?.Remove(index);
            PostChangedEvent();
        }

        /// <summary>
        /// Marks the specified frame as being a key frame.
        /// </summary>
        /// <param name="index">The frame index within the segment that is a key frame.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the number of frames
        /// in the segment.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the segment is part of a sequence that is too old to support key frames.
        /// </exception>
        internal void MarkFrameAsKeyFrame(int index)
        {
            if(index < 0 || index >= frames.Count)
            {
                throw new ArgumentException($"Frame index {index} is not a valid frame index in the segment.", nameof(index));
            }

            if(SequenceVersion < 3)
            {
                throw new InvalidOperationException(
                    $"The sequence version {SequenceVersion} does not support key frames.");
            }

            var newEntry = new KeyFrameEntry(index, CalculateDisplayTimeToFrame(index));
            if(keyFrameTable == null)
            {
                keyFrameTable = new KeyFrameTable();
            }

            keyFrameTable.Add(newEntry);
        }

        /// <summary>
        /// Clears the key frame table of all key frames.
        /// </summary>
        internal void ClearKeyFrameTable()
        {
            keyFrameTable?.Clear();
        }

        /// <summary>
        /// Post the frames removed event.
        /// </summary>
        private void PostChangedEvent()
        {
            var handle = FramesChangedEvent;
            handle?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event to be fired off when the frames change.
        /// </summary>
        internal event EventHandler<EventArgs> FramesChangedEvent;

        /// <summary>
        /// Calculates the display time up to the specified frame index.
        /// </summary>
        /// <param name="targetIndex">The target frame index.</param>
        /// <returns>The time in milliseconds to the target frame.</returns>
        private ulong CalculateDisplayTimeToFrame(int targetIndex)
        {
            ulong time = 0;

            for(var index = 0; index < targetIndex; index++)
            {
                time += frames[index].TotalDisplayTime;
            }

            return time;
        }
    }
}
