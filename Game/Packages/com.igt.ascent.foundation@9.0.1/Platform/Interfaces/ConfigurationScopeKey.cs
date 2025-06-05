// -----------------------------------------------------------------------
// <copyright file = "ConfigurationScopeKey.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This data type represents a key used to query the configurations of a specific scope
    ///  from the report or extension client.
    /// </summary>
    public struct ConfigurationScopeKey : IEquatable<ConfigurationScopeKey>
    {
        /// <summary>
        /// The underlying key data instance.
        /// </summary>
        private readonly ConfigurationKeyData keyData;

        #region Public Properties

        /// <summary>
        /// Gets the configuration scope.
        /// </summary>
        public ConfigurationScope ConfigScope => keyData.ConfigScope;

        /// <summary>
        /// Gets the identifier of the configuration scope.
        /// </summary>
        public string ScopeIdentifier => keyData.ScopeIdentifier;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the key instance with the given information.
        /// </summary>
        /// <param name="keyData">The underlying key data object.</param>
        private ConfigurationScopeKey(ConfigurationKeyData keyData)
        {
            this.keyData = keyData;
        }

        #endregion

        #region Static Factory Members

        /// <summary>
        /// Convert a <see cref="ConfigurationKeyData"/> to a <see cref="ConfigurationScopeKey"/> instance.
        /// </summary>
        /// <param name="keyData">The <see cref="ConfigurationKeyData"/> instance.</param>
        /// <returns>A <see cref="ConfigurationScopeKey"/> instance equivalent to <paramref name="keyData"/>.</returns>
        internal static ConfigurationScopeKey From(ConfigurationKeyData keyData)
        {
            return new ConfigurationScopeKey(keyData);
        }

        /// <summary>
        /// Create a new configuration key with the given information.
        /// </summary>
        /// <param name="scope">The configuration scope where the configuration items are located.</param>
        /// <param name="identifier">The identifier of the target scope.</param>
        /// <exception cref = "ArgumentException">
        /// Thrown when <paramref name="identifier" /> is null or empty.
        /// </exception>
        /// <returns>The new configuration key.</returns>
        /// <remarks>
        /// This factory method is exposed to user code and validates the input <paramref name="identifier"/>.
        /// </remarks>
        public static ConfigurationScopeKey NewKey(ConfigurationScope scope, string identifier)
        {
            if(string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return new ConfigurationScopeKey(new ConfigurationKeyData(scope, identifier));
        }

        #endregion

        #region Implementation of IEquatable<ConfigurationScopeKey>

        /// <inheritdoc />
        public bool Equals(ConfigurationScopeKey other)
        {
            return keyData.Equals(other.keyData);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ConfigurationScopeKey && Equals((ConfigurationScopeKey)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return keyData.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Scope({ConfigScope}:{ScopeIdentifier})";
        }

        #endregion

        #region Overload Operators

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(ConfigurationScopeKey left, ConfigurationScopeKey right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal. False otherwise.</returns>
        public static bool operator !=(ConfigurationScopeKey left, ConfigurationScopeKey right)
        {
            return !(left == right);
        }

        #endregion
    }
}