//-----------------------------------------------------------------------
// <copyright file = "Version.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types
{
    using System;
    using System.Globalization;
    using F2XTransport;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type Version.
    /// </summary>
    public sealed partial class Version : IVersion, IComparable<Version>, IComparable,
        IEquatable<Version>
    {
        #region Constructors

        /// <summary>
        /// Initialize an instance of <see cref="Version"/> class
        /// with the major and minor version values as 0.
        /// </summary>
        /// <remarks>
        /// This parameter-less constructor is needed by the serialization.
        /// </remarks>
        // ReSharper disable MemberCanBePrivate.Global
        public Version()
        {
        }
        // ReSharper restore MemberCanBePrivate.Global

        /// <summary>
        /// Initialize an instance of <see cref="Version"/> class
        /// with the major and minor version values.
        /// </summary>
        /// <param name="majorVersion">Major value of the version.</param>
        /// <param name="minorVersion">Minor value of the version.</param>
        public Version(uint majorVersion, uint minorVersion)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        #endregion

        #region IComparable<Version> Members

        /// <inheritdoc/>
        public int CompareTo(Version other)
        {
            // All instances are greater than null.
            if(ReferenceEquals(other, null))
            {
                return 1;
            }

            if(MajorVersion == other.MajorVersion &&
               MinorVersion == other.MinorVersion)
            {
                return 0;
            }

            if(MajorVersion > other.MajorVersion ||
               (MajorVersion == other.MajorVersion && MinorVersion > other.MinorVersion))
            {
                return 1;
            }

            return -1;
        }

        #endregion

        #region IComparable Members

        /// <inheritdoc/>
        int IComparable.CompareTo(object obj)
        {
            if(!ReferenceEquals(obj, null) && !(obj is Version))
            {
                throw new ArgumentException("Object must be of type Version in order to compare.");
            }

            return CompareTo(obj as Version);
        }

        #endregion

        #region IEquatable<Version> Members

        /// <inheritdoc/>
        public bool Equals(Version other)
        {
            return CompareTo(other) == 0;
        }

        #endregion

        #region Object Overrides

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return CompareTo(obj as Version) == 0;
        }

        /// <inheritdoc/>
        /// <devdoc>
        /// This type is not immutable, so it should not
        /// be used as a hash key.
        /// </devdoc>
        public override int GetHashCode()
        {
            return MajorVersion.GetHashCode() ^ MinorVersion.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Version {0}.{1}", MajorVersion, MinorVersion);
        }

        #endregion

        #region Operator Overrides

        /// <summary>
        /// Override base implementation to go with the Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(Version left, Version right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }

            if(ReferenceEquals(left, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(Version left, Version right)
        {
            if(ReferenceEquals(left, right))
            {
                return false;
            }

            if(ReferenceEquals(left, null))
            {
                return true;
            }

            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the CompareTo method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if left operand is greater than the right operand.  False otherwise.</returns>
        public static bool operator >(Version left, Version right)
        {
            if(ReferenceEquals(left, right))
            {
                return false;
            }

            if(ReferenceEquals(left, null))
            {
                return false;
            }

            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Override base implementation to go with the CompareTo method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if left operand is greater than or equal to the right operand.  False otherwise.</returns>
        public static bool operator >=(Version left, Version right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }

            if(ReferenceEquals(left, null))
            {
                return false;
            }

            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Override base implementation to go with the CompareTo method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if left operand is less than the right operand.  False otherwise.</returns>
        public static bool operator <(Version left, Version right)
        {
            if(ReferenceEquals(left, right))
            {
                return false;
            }

            if(ReferenceEquals(left, null))
            {
                return true;
            }

            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Override base implementation to go with the CompareTo method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if left operand is less than or equal to the right operand.  False otherwise.</returns>
        public static bool operator <=(Version left, Version right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }

            if(ReferenceEquals(left, null))
            {
                return true;
            }

            return left.CompareTo(right) <= 0;
        }

        #endregion
    }
}
