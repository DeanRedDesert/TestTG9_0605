//-----------------------------------------------------------------------
// <copyright file = "ThemeIdentification.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// This class encapsulates the theme's identification string and provides comparison
    /// functionality so that themes may be compared without exposing the identifier.
    /// </summary>
    public class ThemeIdentification : IEquatable<ThemeIdentification>
    {
        /// <summary>
        /// The theme's identification string.
        /// </summary>
        internal readonly string Identification;

        /// <summary>
        /// Constructor for the <see cref="ThemeIdentification"/> object.
        /// </summary>
        /// <param name="identification">The input theme's identification string.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="identification"/> is null./</exception>
        internal ThemeIdentification(string identification)
        {
            if(identification == null)
            {
                throw new ArgumentNullException("identification");
            }

            Identification = identification;
        }

        #region Equality

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ThemeIdentification)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Identification.GetHashCode();
        }

        /// <summary>
        /// Override base implementation to behave consistently with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if operands are considered equal. False otherwise.</returns>
        public static bool operator ==(ThemeIdentification left, ThemeIdentification right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }
            if(ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to behave consistently with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if operands are considered unequal. False otherwise.</returns>
        public static bool operator !=(ThemeIdentification left, ThemeIdentification right)
        {
            return !(left == right);
        }

        #endregion

        #region IEquatable<ThemeIdentification>

        /// <inheritdoc />
        public bool Equals(ThemeIdentification other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return Identification == other.Identification;
        }

        #endregion
    }
}