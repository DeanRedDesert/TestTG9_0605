// -----------------------------------------------------------------------
// <copyright file = "CriticalDataScopeKey.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// The abstraction of the scope identifier normalizes the critical data scope identifiers with guids and regular strings.
    /// </summary>
    internal abstract class CriticalDataScopeKey
    {
        /// <summary>
        /// The scope of the critical data.
        /// </summary>
        public CriticalDataScope Scope { get; }

        /// <summary>
        /// Gets the identifier string.
        /// </summary>
        public abstract string IdentifierString { get; }

        /// <summary>
        /// Creates a new scope identifier object based on the scope and the identifier string.
        /// </summary>
        /// <param name="scope">The scope of the critical data.</param>
        /// <param name="scopeIdentifierString">The identifier string which indicates the location of the critical data.</param>
        /// <returns>The scope identifier object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="scopeIdentifierString"/> is null.
        /// </exception>
        public static CriticalDataScopeKey New(CriticalDataScope scope, string scopeIdentifierString)
        {
            if(string.IsNullOrEmpty(scopeIdentifierString))
            {
                throw new ArgumentNullException(nameof(scopeIdentifierString));
            }

            switch(scope)
            {
                case CriticalDataScope.Extension:
                case CriticalDataScope.ExtensionAnalytics:
                case CriticalDataScope.ExtensionPersistent:
                    return new ConcreteCriticalDataScopeKey<Guid>(scope, new Guid(scopeIdentifierString));
                default:
                    return new ConcreteCriticalDataScopeKey<string>(scope, scopeIdentifierString);
            }
        }

        /// <summary>
        /// Create a new scope identifier object based on the scope and the identifier guid.
        /// </summary>
        /// <param name="scope">The scope of the critical data.</param>
        /// <param name="scopeIdentifierGuid">
        /// The identifier guid which indicates the location of the critical data.
        /// </param>
        /// <returns>The scope identifier object.</returns>
        /// <exception cref="ArgumentException">
        /// When the scope of <paramref name="scope"/> does not accept guid as the identifier,
        /// or when <paramref name="scopeIdentifierGuid"/> is empty.
        /// </exception>
        public static CriticalDataScopeKey New(CriticalDataScope scope, Guid scopeIdentifierGuid)
        {
            if(scopeIdentifierGuid == Guid.Empty)
            {
                throw new ArgumentException("Scope identifier guid cannot be empty.", nameof(scopeIdentifierGuid));
            }

            switch(scope)
            {
                case CriticalDataScope.Extension:
                case CriticalDataScope.ExtensionAnalytics:
                case CriticalDataScope.ExtensionPersistent:
                    return new ConcreteCriticalDataScopeKey<Guid>(scope, scopeIdentifierGuid);
                default:
                    throw new ArgumentException(
                        $"Cannot take guid as the scope identifier of '{scope}'.", nameof(scope));
            }
        }

        /// <summary>
        /// Construct the identifier with the critical data scope.
        /// </summary>
        /// <param name="scope">The critical data scope</param>
        protected CriticalDataScopeKey(CriticalDataScope scope)
        {
            Scope = scope;
        }

        #region Implementation Types

        /// <summary>
        /// The scope identifier implementation which takes <typeparamref name="T"/> as the identifier type.
        /// </summary>
        private sealed class ConcreteCriticalDataScopeKey<T> : CriticalDataScopeKey
        {
            /// <summary>
            /// The identifier of the scope where the critical data is located.
            /// </summary>
            private readonly T identifier;

            /// <inheritdoc />
            public override string IdentifierString => identifier.ToString();

            /// <summary>
            /// Construct the instance with the scope and identifier value.
            /// </summary>
            /// <param name="scope">The scope of the critical data.</param>
            /// <param name="identifier">The identifier value which is used to locate the critical data.</param>
            public ConcreteCriticalDataScopeKey(CriticalDataScope scope, T identifier)
                : base(scope)
            {
                this.identifier = identifier;
            }

            /// <inheritdoc />
            public override bool Equals(object other)
            {
                if(!(other is ConcreteCriticalDataScopeKey<T> otherIdentifier))
                {
                    return false;
                }

                if(ReferenceEquals(this, otherIdentifier))
                {
                    return true;
                }

                return identifier.Equals(otherIdentifier.identifier) &&
                       Scope == otherIdentifier.Scope;
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                var hash = 23;

                hash = hash * 37 + Scope.GetHashCode();
                hash = hash * 37 + identifier.GetHashCode();
                return hash;
            }
        }

        #endregion
    }
}