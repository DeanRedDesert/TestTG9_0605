// -----------------------------------------------------------------------
// <copyright file = "CriticalDataSelector.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This class is used to identify a specific critical data item.
    /// </summary>
    public class CriticalDataSelector : IEquatable<CriticalDataSelector>
    {
        /// <summary>
        /// The scope identifier object.
        /// </summary>
        private readonly CriticalDataScopeKey criticalDataScopeKey;

        /// <summary>
        /// Gets the scope of critical data.
        /// </summary>
        public CriticalDataScope Scope => criticalDataScopeKey.Scope;

        /// <summary>
        /// Gets the identifier of the scope where critical data resides in.
        /// </summary>
        public string ScopeIdentifier => criticalDataScopeKey.IdentifierString;

        /// <summary>
        /// Gets the path of critical data.
        /// </summary>
        public CriticalDataName Path { get; }

        /// <summary>
        /// Construct the instance with the scope identifier guid.
        /// </summary>
        /// <param name="scope">
        /// The scope for reading critical data.
        /// </param>
        /// <param name="scopeIdentifierGuid">The scope identifier guid.</param>
        /// <param name="path">The path of the critical data.</param>
        public CriticalDataSelector(CriticalDataScope scope, Guid scopeIdentifierGuid, CriticalDataName path)
            : this(CriticalDataScopeKey.New(scope, scopeIdentifierGuid), path)
        {
        }

        /// <summary>
        /// Construct the instance with the scope identifier string.
        /// </summary>
        /// <param name="scope">
        /// The scope for reading critical data.
        /// </param>
        /// <param name="scopeIdentifierString">The scope identifier string. Accepts non-Guid string.</param>
        /// <param name="path">The path of the critical data.</param>
        public CriticalDataSelector(CriticalDataScope scope, string scopeIdentifierString, CriticalDataName path)
            : this(CriticalDataScopeKey.New(scope, scopeIdentifierString), path)
        {
        }

        /// <summary>
        /// Construct the instance with  the scope identifier object.
        /// </summary>
        /// <param name="key">
        /// The identifier object indicates both the scope and identifier of the target critical data.
        /// </param>
        /// <param name="path">The path of the critical data.</param>
        private CriticalDataSelector(CriticalDataScopeKey key, CriticalDataName path)
        {
            criticalDataScopeKey = key;
            Path = path;
        }

        #region IEquatable<CriticalDataSelector> Members

        /// <inheritdoc />
        public bool Equals(CriticalDataSelector other)
        {
            if(ReferenceEquals(other, null))
            {
                return false;
            }

            if(ReferenceEquals(other, this))
            {
                return true;
            }

            return criticalDataScopeKey.Equals(other.criticalDataScopeKey) &&
                   Path == other.Path;
        }

        #endregion

        /// <summary>
        /// Override default implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check</param>
        /// <returns>
        /// True if the right hand object equals to this object.
        /// False otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as CriticalDataSelector);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;

            hash = hash * 37 + criticalDataScopeKey.GetHashCode();
            hash = hash * 37 + Path.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>
        /// True if two operands are considered equal.
        /// False otherwise.
        /// </returns>
        public static bool operator ==(CriticalDataSelector left, CriticalDataSelector right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>
        /// True if two operands are not considered equal.
        /// False otherwise.
        /// </returns>
        public static bool operator !=(CriticalDataSelector left, CriticalDataSelector right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Convert a <see cref="CriticalDataSelector"/> to a string.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Scope({Scope})/Scope Identifier({ScopeIdentifier})/Path({Path})";
        }
    }
}