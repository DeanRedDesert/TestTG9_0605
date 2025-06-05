//-----------------------------------------------------------------------
// <copyright file = "Rgb16.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Object used to store 16 bit RGB values.
    /// </summary>
    [Serializable]
    public struct Rgb16 : IEquatable<Rgb16>
    {
        /// <summary>
        /// Five bit red value.
        /// </summary>
        public byte Red { get; private set; }

        /// <summary>
        /// Six bit green value.
        /// </summary>
        public byte Green { get; private set; }

        /// <summary>
        /// Five bit blue value.
        /// </summary>
        public byte Blue { get; private set; }

        /// <summary>
        /// Get the packed color value.
        /// </summary>
        public ushort PackedColor
        {
            get
            {
                var packedColor = (ushort)((Red & 0x1F) << 11);
                packedColor |= (ushort)((Green & 0x3F) << 5);
                packedColor |= (ushort)(Blue & 0x1F);
                return packedColor;
            }
        }

        /// <summary>
        /// Construct an instance with the given color values.
        /// </summary>
        /// <param name="red">The 5-bit red value of the color.</param>
        /// <param name="green">The 6-bit green value of the color.</param>
        /// <param name="blue">The 5-bit blue value of the color.</param>
        /// <remarks>For a given bit range, the least significant bits are used in the byte.</remarks>
        public Rgb16(byte red, byte green, byte blue) : this()
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Construct an instance with the given packed color value.
        /// </summary>
        /// <param name="packedColor">The packed value of R/G/B colors.</param>
        public Rgb16(ushort packedColor) : this()
        {
            Red = (byte)((packedColor >> 11) & 0x1F);
            Green = (byte)((packedColor >> 5) & 0x3F);
            Blue = (byte)(packedColor & 0x1F);
        }

        /// <summary>
        /// Implement IEquatable(T) for enhanced performance.
        /// </summary>
        /// <param name="other">The other object for the equality check.</param>
        /// <returns>True if the other object equals to this object.  False otherwise.</returns>
        public bool Equals(Rgb16 other)
        {
            return PackedColor == other.PackedColor;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if(obj is Rgb16)
            {
                result = Equals((Rgb16)obj);
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
        public static bool operator ==(Rgb16 first, Rgb16 second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(Rgb16 first, Rgb16 second)
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
