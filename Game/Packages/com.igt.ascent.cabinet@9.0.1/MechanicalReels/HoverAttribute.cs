//-----------------------------------------------------------------------
// <copyright file = "HoverAttribute.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;

    /// <summary>
    /// Reel attribute for hovering between two different stops.
    /// </summary>
    public struct HoverAttribute : IEquatable<HoverAttribute>
    {
        /// <summary>
        /// No hover.
        /// </summary>
        public static readonly HoverAttribute Off = new HoverAttribute(HoverLevel.Off);

        /// <summary>
        /// How the reel should hover.
        /// </summary>
        public HoverLevel Level { get; }

        /// <summary>
        /// Hover limits for <see cref="HoverLevel.Custom"/>.
        /// </summary>
        public HoverLimits Limits { get; }

        /// <summary>
        /// Level-only constructor. Sets <see cref="Limits"/> to null.
        /// </summary>
        /// <param name="level">How the reel should hover.</param>
        /// <exception cref="InvalidHoverAttributeException">
        /// Thrown if <see cref="HoverLevel.Custom"/> is used.
        /// </exception>
        public HoverAttribute(HoverLevel level) : this()
        {
            if(level == HoverLevel.Custom)
            {
                throw new InvalidHoverAttributeException();
            }
            Level = level;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="level">How the reel should hover.</param>
        /// <param name="limits">Hover limits for <see cref="HoverLevel.Custom"/>.</param>
        /// <exception cref="InvalidHoverAttributeException">
        /// Thrown if <see cref="HoverLevel.Custom"/> is set with null <paramref name="limits"/>,
        /// or if any other value is set without null <paramref name="limits"/>.
        /// </exception>
        public HoverAttribute(HoverLevel level, HoverLimits limits) : this()
        {
            if(level != HoverLevel.Custom)
            {
                throw new InvalidHoverAttributeException();
            }
            Level = level;
            Limits = limits;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Level == HoverLevel.Custom 
                ? $"Custom: Lower {Limits.LowerLimit} Upper {Limits.UpperLimit}"
                : Level.ToString();
        }

        /// <inheritdoc/>
        public bool Equals(HoverAttribute other)
        {
            return other.Level == Level && Limits.Equals(other.Limits);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is HoverAttribute hoverAttr && Equals(hoverAttr);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // HoverLimits hash code is only 16 bits (two bytes).
            return Limits.GetHashCode() << 8 | (int)Level;
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(HoverAttribute first, HoverAttribute second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(HoverAttribute first, HoverAttribute second)
        {
            return !first.Equals(second);
        }
    }
}
