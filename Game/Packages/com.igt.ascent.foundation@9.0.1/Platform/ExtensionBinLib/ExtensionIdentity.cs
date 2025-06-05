// -----------------------------------------------------------------------
// <copyright file = "ExtensionIdentity.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib
{
    using System;
    using Interfaces;

    /// <summary>
    /// Represents the identity of an extension, including its identifier and version.
    /// </summary>
    internal sealed class ExtensionIdentity : IExtensionIdentity, IEquatable<ExtensionIdentity>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionIdentity"/>
        /// using a Guid and a version.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="version"/> is null.
        /// </exception>
        internal ExtensionIdentity(Guid guid, Version version)
        {
            ExtensionIdentifier = guid;
            ExtensionVersion = version ?? throw new ArgumentNullException(nameof(version));
        }

        #endregion

        #region IExtensionIdentity Implementation

        /// <inheritdoc />
        public Guid ExtensionIdentifier { get; }

        /// <inheritdoc />
        public Version ExtensionVersion { get; }

        #endregion

        #region Equality members

        /// <inheritdoc />
        public bool Equals(ExtensionIdentity other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }

            if(ReferenceEquals(this, other))
            {
                return true;
            }

            return ExtensionIdentifier.Equals(other.ExtensionIdentifier) &&
                   ExtensionVersion.Equals(other.ExtensionVersion);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ExtensionIdentity other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (ExtensionIdentifier.GetHashCode() * 397) ^ ExtensionVersion.GetHashCode();
            }
        }

        public static bool operator ==(ExtensionIdentity left, ExtensionIdentity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExtensionIdentity left, ExtensionIdentity right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region Object Overrides

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Guid({ExtensionIdentifier})/Ver({ExtensionVersion})";
        }

        #endregion
    }
}