// -----------------------------------------------------------------------
// <copyright file = "LightSequencePosition.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System;

    /// <summary>
    /// Represents a position inside a light sequence.
    /// </summary>
    public class LightSequencePosition : IEquatable<LightSequencePosition>, ICloneable
    {
        /// <summary>
        /// The position at the beginning of the sequence.
        /// </summary>
        public static readonly LightSequencePosition Zero = new LightSequencePosition();

        /// <summary>
        /// Gets the current frame number within the segment.
        /// </summary>
        public int Frame { get; internal set; }

        /// <summary>
        /// Gets the current segment number within the sequence.
        /// </summary>
        public int Segment { get; internal set; }

        /// <summary>
        /// Gets the current segment loop count.
        /// </summary>
        public int SegmentLoop { get; internal set; }

        /// <summary>
        /// Equals operator that compares two LightSequencePosition instances.
        /// </summary>
        /// <param name="left">The instance on the left side of the operator.</param>
        /// <param name="right">The instance on the right side of the operator.</param>
        /// <returns>True if the two instances are equal.</returns>
        public static bool operator ==(LightSequencePosition left, LightSequencePosition right)
        {
            if(!ReferenceEquals(left, null) && !ReferenceEquals(right, null))
            {
                return left.Equals(right);
            }

            // If both sides are null they are equal; otherwise not equal if one is null and the other isn't.
            return ReferenceEquals(left, null) && ReferenceEquals(right, null);
        }

        /// <summary>
        /// Not equal operator that compares two LightSequencePosition instances.
        /// </summary>
        /// <param name="left">The instance on the left side of the operator.</param>
        /// <param name="right">The instance on the right side of the operator.</param>
        /// <returns>True if the two instances are not equal.</returns>
        public static bool operator !=(LightSequencePosition left, LightSequencePosition right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Resets the position back to zero.
        /// </summary>
        internal void Reset()
        {
            Frame = 0;
            Segment = 0;
            SegmentLoop = 0;
        }

        #region Overrides of Object

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as LightSequencePosition);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Frame.GetHashCode() ^ Segment.GetHashCode() ^ SegmentLoop.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Segment: {Segment}, Segment Loop: {SegmentLoop}, Frame: {Frame}";
        }

        #endregion

        #region IEquatable<LightSequencePosition> Members

        /// <inheritdoc />
        public bool Equals(LightSequencePosition other)
        {
            if(other == null)
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return Frame == other.Frame && Segment == other.Segment && SegmentLoop == other.SegmentLoop;
        }

        #endregion

        #region ICloneable Members

        /// <inheritdoc />
        public object Clone()
        {
            return new LightSequencePosition { Frame = Frame, Segment = Segment, SegmentLoop = SegmentLoop };
        }

        #endregion
    }
}