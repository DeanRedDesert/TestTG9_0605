//-----------------------------------------------------------------------
// <copyright file = "ConfigurationNotDefinedException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Globalization;

    /// <summary>
    /// This exception indicates that a configuration item being accessed
    /// is not defined.
    /// </summary>
    [Serializable]
    public class ConfigurationNotDefinedException : Exception
    {
        /// <summary>ConfigurationNotDefinedException
        /// The scope of the configuration being accessed when the exception is thrown.
        /// </summary>
        public string ScopeName { get; private set; }

        /// <summary>
        /// The identifier of the configuration scope.
        /// </summary>
        public string ScopeIdentifier { get; private set; }

        /// <summary>
        /// The name of the configuration being accessed when the exception is thrown.
        /// </summary>
        public string ConfigName { get; private set; }

        /// <summary>
        /// The template of the error message.
        /// </summary>
        private const string MessageFormat = "Configuration item {0} in the scope {1} is not defined";

        /// <summary>
        /// The template of the error message related with the scope identifier.
        /// </summary>
        private const string MessageFormatWithIdentifier
            = "Configuration item {0} in the scope {1} with identifier {2} is not defined";

        /// <summary>
        /// The template of error message with configuration keys.
        /// </summary>
        private const string MessageFormatWithKey = "Configuration item {0} is not defined";

        /// <summary>
        /// Construct a <see cref="ConfigurationNotDefinedException"/> with a general message.
        /// </summary>
        /// <param name="configScope">The scope of the configuration item.</param>
        /// <param name="configName">The name of the configuration item.</param>
        public ConfigurationNotDefinedException(ConfigurationScope configScope, string configName)
            : this(string.Format(CultureInfo.InvariantCulture, MessageFormat, configName, configScope),
                   configScope, configName)
        {
        }

        /// <summary>
        /// Construct a <see cref="ConfigurationNotDefinedException"/>  with a general message and an inner exception.
        /// </summary>
        /// <param name="configScope">The scope of the configuration item.</param>
        /// <param name="configName">The name of the configuration item.</param>
        /// <param name="ex">The inner exception.</param>
        public ConfigurationNotDefinedException(ConfigurationScope configScope, string configName, Exception ex)
            : this(string.Format(CultureInfo.InvariantCulture, MessageFormat, configName, configScope),
                   configScope, configName, ex)
        {
        }

        /// <summary>
        /// Construct a <see cref="ConfigurationNotDefinedException"/>  with a specified message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="configScope">The scope of the configuration item.</param>
        /// <param name="configName">The name of the configuration item.</param>
        public ConfigurationNotDefinedException(string message, ConfigurationScope configScope, string configName)
            : base(message)
        {
            ScopeName = configScope.ToString();
            ConfigName = configName;
        }

        /// <summary>
        /// Construct a <see cref="ConfigurationNotDefinedException"/> with a specified message and an inner exception.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="configScope">The scope of the configuration item.</param>
        /// <param name="configName">The name of the configuration item.</param>
        /// <param name="ex">The inner exception.</param>
        public ConfigurationNotDefinedException(string message, ConfigurationScope configScope, string configName,
                                                Exception ex)
            : base(message, ex)
        {
            ScopeName = configScope.ToString();
            ConfigName = configName;
        }

        /// <summary>
        /// Construct a <see cref="ConfigurationNotDefinedException"/> with a general message.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        public ConfigurationNotDefinedException(ConfigurationItemKey configKey)
            : this(string.Format(CultureInfo.InvariantCulture, MessageFormatWithKey, configKey),
                   configKey)
        {
        }

        /// <summary>
        /// Construct a <see cref="ConfigurationNotDefinedException"/> with a specified message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="configKey">The key identifying the configuration item.</param>
        public ConfigurationNotDefinedException(string message, ConfigurationItemKey configKey)
            : this(message, configKey.ConfigScope, configKey.ScopeIdentifier, configKey.ConfigName)
        {
        }

        /// <summary>
        /// Construct a <see cref="ConfigurationNotDefinedException"/> with a specified message.
        /// </summary>
        /// <param name="configScope">The scope of the configuration item.</param>
        /// <param name="scopeIdentifier">The identifier of the configuration scope.</param>
        /// <param name="configName">The name of the configuration item.</param>
        public ConfigurationNotDefinedException(ConfigurationScope configScope, string scopeIdentifier,
            string configName)
            : this(string.Format(CultureInfo.InvariantCulture, MessageFormatWithIdentifier,
                configName, configScope, scopeIdentifier), configScope, scopeIdentifier, configName)
        {
        }

        /// <summary>
        /// Construct a <see cref="ConfigurationNotDefinedException"/> with a specified message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="configScope">The scope of the configuration item.</param>
        /// <param name="scopeIdentifier">The identifier of the configuration scope.</param>
        /// <param name="configName">The name of the configuration item.</param>
        public ConfigurationNotDefinedException(string message, ConfigurationScope configScope, string scopeIdentifier,
                                                string configName)
            : base(message)
        {
            ScopeName = configScope.ToString();
            ScopeIdentifier = scopeIdentifier;
            ConfigName = configName;
        }
    }
}
