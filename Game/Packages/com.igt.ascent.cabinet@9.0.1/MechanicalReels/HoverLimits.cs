//-----------------------------------------------------------------------
// <copyright file = "HoverLimits.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;

    /// <summary>
    /// Amount the motor should hover around the final stop after a spin.
    /// </summary>
    public struct HoverLimits : IEquatable<HoverLimits>
    {
        /// <summary>
        /// First stop to hover between.
        /// </summary>
        public byte LowerLimit { get; }

        /// <summary>
        /// Second stop to hover between.
        /// </summary>
        public byte UpperLimit { get; }

        /// <summary>Constructor.</summary>
        /// <param name="lowerLimit">First stop to hover between.</param>
        /// <param name="upperLimit">Second stop to hover between.</param>
        /// <remarks>Firmware does not support hover with matching lower and upper limit values.</remarks>
        public HoverLimits(byte lowerLimit, byte upperLimit) : this()
        {
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LowerLimit ({LowerLimit}) / UpperLimit ({UpperLimit})";
        }

        /// <inheritdoc/>
        public bool Equals(HoverLimits other)
        {
            return other.LowerLimit == LowerLimit && other.UpperLimit == UpperLimit;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is HoverLimits hoverLimits && Equals(hoverLimits);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return UpperLimit << 8 | LowerLimit;
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(HoverLimits first, HoverLimits second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(HoverLimits first, HoverLimits second)
        {
            return !first.Equals(second);
        }
    }
}
