// -----------------------------------------------------------------------
// <copyright file = "KeyFrameEntry.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// The information about a key frame.
    /// </summary>
    public class KeyFrameEntry : IEquatable<KeyFrameEntry>, ICompactSerializable
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public KeyFrameEntry()
        {
            
        }

        /// <summary>
        /// Constructs a new instance given the frame index and frame time.
        /// </summary>
        /// <param name="frameIndex">The index of the frame in the sequence.</param>
        /// <param name="frameTime">The time in milliseconds of the frame in the sequence.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="frameIndex"/> is less than zero.
        /// </exception>
        public KeyFrameEntry(int frameIndex, ulong frameTime) : this()
        {
            if(frameIndex < 0)
            {
                throw new ArgumentException("The frame index cannot be less than zero.", nameof(frameIndex));
            }

            FrameIndex = frameIndex;
            FrameTime = frameTime;
        }

        /// <summary>
        /// Gets the index of the key frame.
        /// </summary>
        public int FrameIndex { get; private set; } 

        /// <summary>
        /// Gets the time of the frame in the sequence relative to the start in milliseconds.
        /// </summary>
        public ulong FrameTime { get; private set; }

        /// <inheritdoc />
        public bool Equals(KeyFrameEntry other)
        {
            if(other != null)
            {
                if(ReferenceEquals(this, other))
                {
                    return true;
                }

                return FrameIndex == other.FrameIndex && FrameTime == other.FrameTime;
            }

            return false;
        }

        #region Overrides of Object

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as KeyFrameEntry);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return FrameIndex.GetHashCode() ^ FrameTime.GetHashCode();
        }

        #endregion

        #region ICompactSerializable

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            FrameIndex = CompactSerializer.ReadInt(stream);
            FrameTime = CompactSerializer.ReadUlong(stream);
        }

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, FrameIndex);
            CompactSerializer.Write(stream, FrameTime);
        }

        #endregion
    }
}