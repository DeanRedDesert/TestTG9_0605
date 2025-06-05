//-----------------------------------------------------------------------
// <copyright file = "Rgb6.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Object used to represent 6 bit RGB values.
    /// </summary>
    [Serializable]
    public struct Rgb6 : IEquatable<Rgb6>
    {
        /// <summary>
        /// Two bit red value.
        /// </summary>
        public byte Red { get; private set; }

        /// <summary>
        /// Two bit green value.
        /// </summary>
        public byte Green { get; private set; }

        /// <summary>
        /// Two bit blue value.
        /// </summary>
        public byte Blue { get; private set; }

        /// <summary>
        /// Get the packed color value.
        /// </summary>
        public byte PackedColor =>
            (byte)(
                ((Red & 0x03) << 4) |
                ((Green & 0x03) << 2) |
                (Blue & 0x03)
            );

        /// <summary>
        /// Construct a six bit color with the given values.
        /// </summary>
        /// <param name="red">The two bit red value of the color.</param>
        /// <param name="green">The two bit green value of the color.</param>
        /// <param name="blue">The two bit blue value of the color.</param>
        /// <remarks>For a given bit range, the least significant bits are used in the byte.</remarks>
        public Rgb6(byte red, byte green, byte blue) : this()
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Implement IEquatable(T) for enhanced performance.
        /// </summary>
        /// <param name="rightHand">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public bool Equals(Rgb6 rightHand)
        {
            return PackedColor == rightHand.PackedColor;
        }


        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if(obj is Rgb6 rgb6)
            {
                result = Equals(rgb6);
            }

            return result;
        }

        /// <summary>
        /// Overridden hash code to match equals.
        /// </summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode()
        {
            return PackedColor;
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(Rgb6 first, Rgb6 second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(Rgb6 first, Rgb6 second)
        {
            return !first.Equals(second);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"R {Red}/G {Green}/B {Blue}";
        }
    }
}
