//-----------------------------------------------------------------------
// <copyright file = "Rgb15.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Object used to store 15 bit RGB value.
    /// </summary>
    [Serializable]
    public struct Rgb15 : IEquatable<Rgb15>
    {
        /// <summary>
        /// Construct a new instance with the given color values.
        /// </summary>
        /// <param name="red">The 5-bit red value of the color.</param>
        /// <param name="green">The 5-bit green value of the color.</param>
        /// <param name="blue">The 5-bit blue value of the color.</param>
        /// <remarks>
        /// For a given bit range, the least significant bits are used in the byte.
        /// </remarks>
        public Rgb15(byte red, byte green, byte blue)
            : this()
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Construct a new instance with the given packed color value.
        /// </summary>
        /// <param name="packedColor">Packed color value.</param>
        public Rgb15(ushort packedColor)
            :this()
        {
            Red = Convert.ToByte((packedColor & 0x7C00) >> 10);
            Green = Convert.ToByte((packedColor & 0x03E0) >> 5);
            Blue = Convert.ToByte(packedColor & 0x001F);
        }

        #region Properties

        /// <summary>
        /// Five bit red value.
        /// </summary>
        public byte Red { get; private set; }

        /// <summary>
        /// Five bit green value.
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
                var packedColor = (ushort)((Red & 0x1F) << 10);
                packedColor |= (ushort)((Green & 0x1F) << 5);
                packedColor |= (ushort)(Blue & 0x1F);
                return packedColor;
            }
        }

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Rgb15 && Equals((Rgb15)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return PackedColor;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"R {Red}/G {Green}/B {Blue}";
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(Rgb15 first, Rgb15 second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(Rgb15 first, Rgb15 second)
        {
            return !first.Equals(second);
        }

        #region IEquatable<Rgb15> Members

        /// <inheritdoc />
        public bool Equals(Rgb15 obj)
        {
            return PackedColor == obj.PackedColor;
        }

        #endregion
    }
}
