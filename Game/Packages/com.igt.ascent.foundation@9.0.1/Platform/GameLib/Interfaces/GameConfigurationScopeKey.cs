// -----------------------------------------------------------------------
// <copyright file = "GameConfigurationScopeKey.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// This data type represents a key used to query the configurations of a specific scope
    ///  from the game client
    /// </summary>
    /// <devdoc>
    /// This data type is different from <see cref="ConfigurationScopeKey"/> as it does not allow the user to specify
    /// the identifiers for theme or payvar configuration scope , as the game client is only allowed to query the
    /// custom configuration items for the active theme or payvar.
    /// </devdoc>
    public readonly struct GameConfigurationScopeKey : IEquatable<GameConfigurationScopeKey>
    {
        #region Underlying Data

        /// <summary>
        /// Gets the underlying key data instance.
        /// </summary>
        internal ConfigurationKeyData KeyData { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the configuration scope.
        /// </summary>
        public ConfigurationScope ConfigScope => KeyData.ConfigScope;

        /// <summary>
        /// Gets the identifier of the configuration scope.
        /// </summary>
        public string ScopeIdentifier => KeyData.ScopeIdentifier;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the key instance with the given information.
        /// </summary>
        /// <param name="keyData">The underlying key data object.</param>
        private GameConfigurationScopeKey(ConfigurationKeyData keyData)
        {
            KeyData = keyData;
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Create a new configuration key that can be used to inquiry the configuration items defined for
        /// current active theme.
        /// </summary>
        /// <returns>The configuration key.</returns>
        public static GameConfigurationScopeKey NewThemeScopeKey()
        {
            return new GameConfigurationScopeKey(new ConfigurationKeyData(ConfigurationScope.Theme, string.Empty));
        }

        /// <summary>
        /// Create a new configuration key that can be used to inquiry the configuration items defined for
        /// current active pavyar.
        /// </summary>
        /// <returns>The configuration key.</returns>
        public static GameConfigurationScopeKey NewPayvarScopeKey()
        {
            return new GameConfigurationScopeKey(new ConfigurationKeyData(ConfigurationScope.Payvar, string.Empty));
        }

        /// <summary>
        /// Create a new configuration key that can be used to inquiry the configuration items defined for
        /// the specified extension package.
        /// </summary>
        /// <param name="extensionIdentifier">The identifier of the target extension package.</param>
        /// <returns>The configuration key.</returns>
        /// <exception cref = "ArgumentException">
        /// Thrown when either <paramref name="extensionIdentifier" /> is null or empty.
        /// </exception>
        public static GameConfigurationScopeKey NewExtensionScopeKey(string extensionIdentifier)
        {
            if(string.IsNullOrEmpty(extensionIdentifier))
            {
                throw new ArgumentException(
                    "The extension identifier cannot be null or empty.", nameof(extensionIdentifier));
            }

            var keyData =
                new ConfigurationKeyData(ConfigurationScope.Extension, extensionIdentifier);
            return new GameConfigurationScopeKey(keyData);
        }

        #endregion

        #region Implementation of IEquatable<GameConfigurationScopeKey>

        /// <inheritdoc />
        public bool Equals(GameConfigurationScopeKey other)
        {
            return KeyData.Equals(other.KeyData);
        }

        #endregion

        #region Overrides of ValueType

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is GameConfigurationScopeKey key && Equals(key);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return KeyData.GetHashCode();
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
        public static bool operator ==(GameConfigurationScopeKey left, GameConfigurationScopeKey right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal. False otherwise.</returns>
        public static bool operator !=(GameConfigurationScopeKey left, GameConfigurationScopeKey right)
        {
            return !(left == right);
        }

        #endregion
    }
}