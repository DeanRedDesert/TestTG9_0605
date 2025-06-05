//-----------------------------------------------------------------------
// <copyright file = "RgbLightState.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Struct representing the state of a single RGB light.
    /// </summary>
    [Serializable]
    public struct RgbLightState : IEquatable<RgbLightState>, ICompactSerializable
    {
        /// <summary>
        /// Index of the light.
        /// </summary>
        public ushort LightNumber { get; private set; }

        /// <summary>
        /// Color of the light.
        /// </summary>
        public Rgb16 Color { get; private set; }

        /// <summary>
        /// Construct an instance of the light state with the given parameters.
        /// </summary>
        /// <param name="lightNumber">The light number for the state.</param>
        /// <param name="color">The state of the light.</param>
        public RgbLightState(ushort lightNumber, Rgb16 color) : this()
        {
            LightNumber = lightNumber;
            Color = color;
        }

        /// <summary>
        /// Implement IEquatable(T) for enhanced performance.
        /// </summary>
        /// <param name="other">The other object for the equality check.</param>
        /// <returns>True if the other object equals to this object.  False otherwise.</returns>
        public bool Equals(RgbLightState other)
        {
            return LightNumber == other.LightNumber &&
                   Color == other.Color;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if(obj is RgbLightState state)
            {
                result = Equals(state);
            }

            return result;
        }

        /// <summary>
        /// Overridden hash code to match equals.
        /// </summary>
        /// <returns>A hash code for this object.</returns>
        public override int GetHashCode()
        {
            return (LightNumber << 16) | Color.PackedColor;
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(RgbLightState first, RgbLightState second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(RgbLightState first, RgbLightState second)
        {
            return !first.Equals(second);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Light {LightNumber} of Color {Color}";
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, LightNumber);
            CompactSerializer.Write(stream, Color.PackedColor);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            LightNumber = CompactSerializer.ReadUshort(stream);

            var packedColor = CompactSerializer.ReadUshort(stream);
            Color = new Rgb16(packedColor);
        }

        #endregion
    }
}
