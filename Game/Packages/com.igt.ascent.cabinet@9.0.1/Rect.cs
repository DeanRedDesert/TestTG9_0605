//-----------------------------------------------------------------------
// <copyright file = "Rect.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Text;

    /// <summary>
    /// Object used to represent the size of a rectangle (of content) on a monitor.
    /// </summary>
    [Serializable]
    public struct Rect : IEquatable<Rect>
    {
        #region Private Fields

        #endregion

        /// <summary>
        /// The pixel location of the top-left (x, y) coordinate of the content on the monitor.
        /// </summary>
        public Point Location { get; }

        /// <summary>
        /// The size (width and height) of the content on the monitor.
        /// </summary>
        public SizeRect Size { get; }

        /// <summary>
        /// Instantiates a <see cref="Rect"/> object using the specified <see cref="Point"/>
        /// and <see cref="SizeRect"/> parameters.
        /// </summary>
        /// <param name="location">The top-left pixel coordinate (x, y) of the content.</param>
        /// <param name="size">The size (width, height) of the content.</param>
        public Rect(Point location, SizeRect size)
        {
            Location = location;
            Size = size;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string that specifically describes the object.</returns>
        public override string ToString()
        {
            var info = new StringBuilder();
            info.AppendLine("Rect:");
            info.AppendLine($"Location({Location}), Size({Size})");
            return info.ToString();
        }

        #region Equality

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   obj.GetType() == GetType() &&
                   Equals((Rect)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return Location.GetHashCode() + Size.GetHashCode();
            }
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden not-equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not equal. False otherwise.</returns>
        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
        }

        #endregion

        #region IEquatable<Rect>

        /// <inheritdoc />
        public bool Equals(Rect other)
        {
            return Location == other.Location && Size == other.Size;
        }

        #endregion  
    }
}