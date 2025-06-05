// -----------------------------------------------------------------------
// <copyright file = "ConfigurationKeyData.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This class contains the essential information to query configuration data from Foundation.
    /// </summary>
    internal struct ConfigurationKeyData : IEquatable<ConfigurationKeyData>
    {
        #region Public Properties

        /// <summary>
        /// Gets the configuration scope.
        /// </summary>
        public ConfigurationScope ConfigScope { get; }

        /// <summary>
        /// Gets the identifier of the configuration scope.
        /// </summary>
        public string ScopeIdentifier { get; }

        /// <summary>
        /// Gets the configuration item name.
        /// </summary>
        public string ConfigName { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new configuration key with the given information.
        /// </summary>
        /// <param name="scope">The configuration scope where the configuration items are located.</param>
        /// <param name="identifier">The identifier of the target scope.</param>
        /// <param name="configName">The configuration item name.</param>
        /// <returns>The new configuration key.</returns>
        /// <exception cref="FormatException">
        /// Thrown when <paramref name="scope"/> is <see cref="ConfigurationScope.Extension"/>
        /// while <paramref name="identifier"/> is not a valid GUID string.
        /// </exception>
        /// <remarks>
        /// This constructor is called by internal code and does not validate the input <paramref name="identifier"/>.
        /// </remarks>
        public ConfigurationKeyData(ConfigurationScope scope, string identifier, string configName=null)
            : this()
        {
            ConfigScope = scope;
            ConfigName = configName;
            ScopeIdentifier = scope == ConfigurationScope.Extension ? new Guid(identifier).ToString() : identifier;
        }

        #endregion

        #region Equality Overrides

        /// <inheritdoc />
        public bool Equals(ConfigurationKeyData other)
        {
            return ConfigScope == other.ConfigScope &&
                   ScopeIdentifier == other.ScopeIdentifier &&
                   ConfigName == other.ConfigName;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ConfigurationKeyData && Equals((ConfigurationKeyData)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)ConfigScope ^ 397;
                hashCode = (hashCode * 397) ^ (ScopeIdentifier != null ? ScopeIdentifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConfigName != null ? ConfigName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}