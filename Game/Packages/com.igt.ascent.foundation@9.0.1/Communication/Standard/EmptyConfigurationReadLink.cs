// -----------------------------------------------------------------------
// <copyright file = "EmptyConfigurationReadLink.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class represents a configuration read link that links to no registries. Thus no custom configuration item
    /// is available from this link.
    /// </summary>
    internal class EmptyConfigurationReadLink : IConfigurationReadLink
    {
        #region Implementation of IConfigurationReadLink

        /// <inheritdoc />
        public ConfigurationItemType GetConfigurationType(ConfigurationItemKey configKey)
        {
            return ConfigurationItemType.Invalid;
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ConfigurationItemType> GetConfigurationTypes(
            IList<ConfigurationItemKey> configKeys)
        {
            if(configKeys == null)
            {
                throw new ArgumentNullException("configKeys");
            }

            return configKeys.ToDictionary(
                // ReSharper disable once ConvertClosureToMethodGroup
                configKey => configKey,
                configKey => ConfigurationItemType.Invalid);
        }

        /// <inheritdoc />
        public IDictionary<string, ConfigurationItemType> QueryConfigurations(ConfigurationScopeKey scopeKey)
        {
            return new Dictionary<string, ConfigurationItemType>();
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            throw new ConfigurationNotDefinedException(configKey);
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, object> GetConfigurations(
            IDictionary<ConfigurationItemKey, ConfigurationItemType> configKeysAndTypes)
        {
            if(configKeysAndTypes == null)
            {
                throw new ArgumentNullException("configKeysAndTypes");
            }

            return configKeysAndTypes.ToDictionary(
                configPair => configPair.Key,
                configPair => (object)null);
        }

        /// <inheritdoc />
        public ICollection<string> QueryReferencedEnumDeclaration(ConfigurationItemKey configKey)
        {
            return null;
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IList<ConfigurationItemKey> configKeys)
        {
            if(configKeys == null)
            {
                throw new ArgumentNullException("configKeys");
            }

            return configKeys.ToDictionary(
                // ReSharper disable once ConvertClosureToMethodGroup
                configKey => configKey,
                configKey => (ICollection<string>)null);
        }

        #endregion
    }
}