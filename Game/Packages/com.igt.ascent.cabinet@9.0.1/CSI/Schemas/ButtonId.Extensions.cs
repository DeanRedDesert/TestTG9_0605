////-----------------------------------------------------------------------
//// <copyright file = "ButtonId.Extensions.cs" company = "IGT">
////     Copyright (c) 2021 IGT.  All rights reserved.
//// </copyright>
////-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CSI.Schemas.Internal
{
    using System;

    /// <summary>
    /// Extension of ButtonId to implement IEquatable.
    /// </summary>
    public partial class ButtonId : IEquatable<ButtonId>
    {
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(obj is null)
            {
                return false;
            }

            if(ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((ButtonId)obj);
        }
        /// <inheritdoc />
        public bool Equals(ButtonId other)
        {
            if(other is null)
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(deviceIdField, other.deviceIdField, StringComparison.Ordinal) && valueField == other.valueField;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((DeviceId != null ? DeviceId.GetHashCode() : 0) * 397) ^ Value.GetHashCode();
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="left">First object for comparison.</param>
        /// <param name="right">Second object for comparison.</param>
        /// <returns>True if the objects are equal.</returns>
        public static bool operator ==(ButtonId left, ButtonId right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloaded to match equals.
        /// </summary>
        /// <param name="left">First object for comparison.</param>
        /// <param name="right">Second object for comparison.</param>
        /// <returns>True if the objects are not equal.</returns>
        public static bool operator !=(ButtonId left, ButtonId right)
        {
            return !Equals(left, right);
        }
    }
}
