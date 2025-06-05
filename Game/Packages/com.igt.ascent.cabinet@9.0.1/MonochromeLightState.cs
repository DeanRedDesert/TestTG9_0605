//-----------------------------------------------------------------------
// <copyright file = "MonochromeLightState.cs" company = "IGT">
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
    /// Struct representing the state of a single monochrome light.
    /// </summary>
    [Serializable]
    public struct MonochromeLightState : IEquatable<MonochromeLightState>, ICompactSerializable
    {
        /// <summary>
        /// Index of the light.
        /// </summary>
        public ushort LightNumber { get; private set; }

        /// <summary>
        /// Brightness of the light.
        /// </summary>
        public byte Brightness { get; private set; }

        /// <summary>
        /// Construct an instance of the light state with the given parameters.
        /// </summary>
        /// <param name="lightNumber">The light number of the state.</param>
        /// <param name="brightness">The brightness of the light.</param>
        public MonochromeLightState(ushort lightNumber, byte brightness) : this()
        {
            LightNumber = lightNumber;
            Brightness = brightness;
        }

        /// <summary>
        /// Implement IEquatable(T) for enhanced performance.
        /// </summary>
        /// <param name="other">The other object for the equality check.</param>
        /// <returns>True if the other object equals to this object.  False otherwise.</returns>
        public bool Equals(MonochromeLightState other)
        {
            return LightNumber == other.LightNumber &&
                   Brightness == other.Brightness;
        }

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if(obj is MonochromeLightState state)
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
            return (LightNumber << 8) | Brightness;
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(MonochromeLightState first, MonochromeLightState second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="first">First object for comparison.</param>
        /// <param name="second">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(MonochromeLightState first, MonochromeLightState second)
        {
            return !first.Equals(second);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Light {LightNumber} at Brightness {Brightness}";
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, LightNumber);
            CompactSerializer.Write(stream, Brightness);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            LightNumber = CompactSerializer.ReadUshort(stream);
            Brightness = CompactSerializer.ReadByte(stream);
        }

        #endregion
    }
}
