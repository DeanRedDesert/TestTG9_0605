//-----------------------------------------------------------------------
// <copyright file = "SizeRect.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Object used to represent the size of a rectangle (width and height) on a monitor.
    /// </summary>
    [Serializable]
    public struct SizeRect : IEquatable<SizeRect>
    {
        #region Private Fields

        #endregion

        /// <summary>
        /// The width (in pixels) of the rectangle.
        /// </summary>
        public ushort Width { get; }

        /// <summary>
        /// The height (in pixels) of the rectangle.
        /// </summary>
        public ushort Height { get; }

        /// <summary>
        /// Instantiates a <see cref="SizeRect"/> object using the specified width and height.
        /// </summary>
        /// <param name="width">The width of the rectangle..</param>
        /// <param name="height">The height of the rectangle.</param>
        public SizeRect(ushort width, ushort height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string that specifically describes the object.</returns>
        public override string ToString()
        {
            return $"SizeRect: Height({Height}), Width({Width})";
        }

        #region Equality

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   obj.GetType() == GetType() &&
                   Equals((SizeRect)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return Height.GetHashCode() + Width.GetHashCode();
            }
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(SizeRect left, SizeRect right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden not-equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not equal. False otherwise.</returns>
        public static bool operator !=(SizeRect left, SizeRect right)
        {
            return !(left == right);
        }

        #endregion

        #region IEquatable<SizeRect> 

        /// <inheritdoc />
        public bool Equals(SizeRect other)
        {
            return Width == other.Width && Height == other.Height;
        }

        #endregion
    }
}