//-----------------------------------------------------------------------
// <copyright file = "Point.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Object used to represent a point ((x, y) pixel coordinate) on a monitor.
    /// </summary>
    [Serializable]
    public struct Point : IEquatable<Point>
    {
        #region Private Fields

        #endregion

        /// <summary>
        /// The x pixel coordinate on the monitor.
        /// </summary>
        public ushort X { get; }

        /// <summary>
        /// The y pixel coordinate on the monitor.
        /// </summary>
        public ushort Y { get; }

        /// <summary>
        /// Instantiates a <see cref="Point"/> object using the specified (x, y) coordinates.
        /// </summary>
        /// <param name="x">The x pixel coordinate of the point.</param>
        /// <param name="y">The y pixel coordinate of the point.</param>
        public Point(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string that specifically describes the object.</returns>
        public override string ToString()
        {
            return $"Point: X({X}), Y({Y})";
        }

        #region Equality

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   obj.GetType() == GetType() &&
                   Equals((Point)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return X.GetHashCode() + Y.GetHashCode();
            }
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden not-equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not equal. False otherwise.</returns>
        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        #endregion

        #region IEquatable<Point>

        /// <inheritdoc />
        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        #endregion  
    }
}