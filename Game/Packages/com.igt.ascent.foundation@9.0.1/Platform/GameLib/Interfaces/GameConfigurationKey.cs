// -----------------------------------------------------------------------
// <copyright file = "GameConfigurationKey.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// This data type represents a key used to query the configuration item by the game client.
    /// </summary>
    /// <devdoc>
    /// This data type is different from <see cref="ConfigurationItemKey" /> as it does not allow the user to specify
    /// the scope identifier for a theme or payvar configuration key, as the game client is only allowed to query the
    /// custom configuration items for the active theme or payvar.
    /// </devdoc>
    public readonly struct GameConfigurationKey : IEquatable<GameConfigurationKey>
    {
        #region Underlying Data

        /// <summary>
        /// Gets the underlying key data instance.
        /// </summary>
        internal ConfigurationKeyData KeyData { get; }

        #endregion

        #region Public Properties

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

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the key instance with the given information.
        /// </summary>
        /// <param name="keyData">The underlying data of the key.</param>
        private GameConfigurationKey(ConfigurationKeyData keyData)
        {
            KeyData = keyData;
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Convert a <see cref="ConfigurationKeyData"/> to a <see cref="GameConfigurationKey"/> instance.
        /// </summary>
        /// <param name="keyData">The <see cref="ConfigurationKeyData"/> instance.</param>
        /// <returns>A <see cref="GameConfigurationKey"/> instance equivalent to <paramref name="keyData"/>.</returns>
        internal static GameConfigurationKey From(ConfigurationKeyData keyData)
        {
            return new GameConfigurationKey(keyData);
        }

        /// <summary>
        /// Create a new configuration key that can be used to inquiry the configuration items defined for
        /// current active theme.
        /// </summary>
        /// <param name="configName">The configuration name to search.</param>
        /// <returns>The configuration key.</returns>
        /// <exception cref = "ArgumentException">
        /// Thrown when <paramref name="configName" /> is null or empty.
        /// </exception>
        public static GameConfigurationKey NewThemeKey(string configName)
        {
            if(string.IsNullOrEmpty(configName))
            {
                throw new ArgumentException("Configuration item name can not be null or empty.", nameof(configName));
            }

            var keyData = new ConfigurationKeyData(ConfigurationScope.Theme, string.Empty, configName);
            return new GameConfigurationKey(keyData);
        }

        /// <summary>
        /// Create a new configuration key that can be used to inquiry the configuration items defined for
        /// current active payvar.
        /// </summary>
        /// <param name="configName">The configuration name to search.</param>
        /// <returns>The configuration key.</returns>
        /// <exception cref = "ArgumentException">
        /// Thrown when <paramref name="configName" /> is null or empty.
        /// </exception>
        public static GameConfigurationKey NewPayvarKey(string configName)
        {
            if(string.IsNullOrEmpty(configName))
            {
                throw new ArgumentException("Configuration item name can not be null or empty.", nameof(configName));
            }

            var keyData = new ConfigurationKeyData(ConfigurationScope.Payvar, string.Empty, configName);
            return new GameConfigurationKey(keyData);
        }

        /// <summary>
        /// Create a new configuration key that can be used to inquiry the configuration items defined for
        /// the specified extension package.
        /// </summary>
        /// <param name="extensionIdentifier">The identifier of the target extension package.</param>
        /// <param name="configName">The configuration name to search.</param>
        /// <returns>The configuration key.</returns>
        /// <exception cref = "ArgumentException">
        /// Thrown when either <paramref name="extensionIdentifier" /> or <paramref name="configName" />
        /// is null or empty.
        /// </exception>
        public static GameConfigurationKey NewExtensionKey(string extensionIdentifier, string configName)
        {
            if(string.IsNullOrEmpty(extensionIdentifier))
            {
                throw new ArgumentException(
                    "The extension identifier cannot be null or empty.", nameof(extensionIdentifier));
            }

            if(string.IsNullOrEmpty(configName))
            {
                throw new ArgumentException("Configuration item name can not be null or empty.", nameof(configName));
            }

            var keyData = new ConfigurationKeyData(ConfigurationScope.Extension, extensionIdentifier, configName);
            return new GameConfigurationKey(keyData);
        }

        #endregion

        #region IEquatable<GameConfigurationKey> Members

        /// <inheritdoc/>
        public bool Equals(GameConfigurationKey rightHand)
        {
            return KeyData.Equals(rightHand.KeyData);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;
            if(obj is GameConfigurationKey key)
            {
                result = Equals(key);
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
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Scope({ConfigScope}:{ScopeIdentifier})/ConfigName({ConfigName})";
        }

        #endregion

        #region Overloaded Operators

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(GameConfigurationKey left, GameConfigurationKey right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(GameConfigurationKey left, GameConfigurationKey right)
        {
            return !(left == right);
        }

        #endregion
    }
}