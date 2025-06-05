//-----------------------------------------------------------------------
// <copyright file = "ConfigurationItemKey.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// Struct that identifies a custom configuration item.
    /// </summary>
    [Serializable]
    public struct ConfigurationItemKey : IEquatable<ConfigurationItemKey>
    {
        /// <summary>
        /// Gets the underlying key data instance.
        /// </summary>
        internal ConfigurationKeyData KeyData { get; }

        /// <summary>
        /// Gets the configuration item scope.
        /// </summary>
        public ConfigurationScope ConfigScope => KeyData.ConfigScope;

        /// <summary>
        /// Gets the identifier of the configuration scope.
        /// </summary>
        public string ScopeIdentifier => KeyData.ScopeIdentifier;

        /// <summary>
        /// Gets the configuration item name.
        /// </summary>
        public string ConfigName => KeyData.ConfigName;

        /// <summary>
        /// Gets the status of <see cref="ScopeIdentifier"/>.
        /// True if it has not been set to a valid value.
        /// </summary>
        public bool ScopeIdentifierMissing => string.IsNullOrEmpty(ScopeIdentifier);

        /// <summary>
        /// Initialize a new instance of <see cref="ConfigurationItemKey"/>,
        /// using the underlying key data object.
        /// </summary>
        /// <param name="keyData">The underlying data of the key.</param>
        private ConfigurationItemKey(ConfigurationKeyData keyData)
        {
            KeyData = keyData;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="ConfigurationItemKey"/>,
        /// using an empty scope identifier.
        /// </summary>
        /// <param name="configScope">The configuration item scope.</param>
        /// <param name="configName">The configuration item name.</param>
        public ConfigurationItemKey(ConfigurationScope configScope, string configName)
            : this(configScope, string.Empty, configName)
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="ConfigurationItemKey"/>.
        /// </summary>
        /// <param name="configScope">The configuration item scope.</param>
        /// <param name="scopeIdentifier">The identifier of the configuration scope.</param>
        /// <param name="configName">The configuration item name.</param>
        /// <exception cref = "ArgumentException">
        /// Thrown if <paramref name="configName"/> is null or an empty string.
        /// </exception>
        /// <exception cref = "ArgumentNullException">
        /// Thrown if <paramref name="scopeIdentifier"/> is null.
        /// </exception>
        public ConfigurationItemKey(ConfigurationScope configScope, string scopeIdentifier, string configName)
            : this()
        {
            if(scopeIdentifier == null)
            {
                throw new ArgumentNullException(nameof(scopeIdentifier));
            }

            if(string.IsNullOrEmpty(configName))
            {
                throw new ArgumentException("Configuration item name can not be null or empty.", nameof(configName));
            }

            KeyData = new ConfigurationKeyData(configScope, scopeIdentifier, configName);
        }

        #region Static Factory Methods

        /// <summary>
        /// Convert a <see cref="ConfigurationKeyData"/> to a <see cref="ConfigurationItemKey"/> instance.
        /// </summary>
        /// <param name="keyData">The <see cref="ConfigurationKeyData"/> instance.</param>
        /// <returns>A <see cref="ConfigurationItemKey"/> instance equivalent to <paramref name="keyData"/>.</returns>
        internal static ConfigurationItemKey From(ConfigurationKeyData keyData)
        {
            return new ConfigurationItemKey(keyData);
        }

        #endregion

        #region IEquatable<ConfigurationItemKey> Members

        /// <inheritdoc/>
        public bool Equals(ConfigurationItemKey rightHand)
        {
            return  KeyData.Equals(rightHand.KeyData);
        }

        #endregion

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if(obj is ConfigurationItemKey)
            {
                result = Equals((ConfigurationItemKey)obj);
            }

            return result;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <returns>The hash code generated.</returns>
        public override int GetHashCode()
        {
            return KeyData.GetHashCode();
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(ConfigurationItemKey left, ConfigurationItemKey right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(ConfigurationItemKey left, ConfigurationItemKey right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Scope({ConfigScope}:{ScopeIdentifier})/ConfigName({ConfigName})";
        }
    }
}
