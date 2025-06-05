// -----------------------------------------------------------------------
//  <copyright file = "ReelStripDistortionConfigurationProvider.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;

    /// <summary>
    /// Provides the reel strip distortion configuration from the foundation.
    /// </summary>
    /// <remarks>
    /// The reel strip distortion configuration is provided by the configuration extension RSDConfigInterfaceV1.
    /// </remarks>
    public class ReelStripDistortionConfigurationProvider
    {
        /// <summary>
        /// The id of the reel strip distortion configuration extension.
        /// </summary>
        private const string ConfigurationExtensionId = "6caaed91-36ac-476f-a8c2-f8b50fa3048e";

        /// <summary>
        /// The name of the "Enabled" element of the reel strip distortion configuration extension.
        /// </summary>
        private const string EnabledConfigName = "Enabled";

        /// <summary>
        /// The name of the "Limit" element of the reel strip distortion configuration extension.
        /// </summary>
        private const string LimitConfigName = "Limit";

        /// <summary>
        /// The <see cref="IGameConfigurationRead"/> to read the reel strip distortion configuration from.
        /// </summary>
        private readonly IGameConfigurationRead configuration;

        /// <summary>
        /// Create a new reel strip configuration provider.
        /// </summary>
        /// <param name="configuration">The configuration from which to get the reel strip configuration.</param>
        public ReelStripDistortionConfigurationProvider(IGameConfigurationRead configuration)
        {
            if(configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.configuration = configuration;
        }

        /// <summary>
        /// Read the reel strip distortion configuration from the foundation.
        /// </summary>
        public void ReadReelStripDistortionConfiguration()
        {
            // Load the reel strip distortion related config items.
            var enabledConfigurationKey = GameConfigurationKey.NewExtensionKey(ConfigurationExtensionId, EnabledConfigName);
            var limitConfigurationKey = GameConfigurationKey.NewExtensionKey(ConfigurationExtensionId, LimitConfigName);
            var configurationKeys = new Dictionary<GameConfigurationKey, ConfigurationItemType>
            {
                {enabledConfigurationKey, ConfigurationItemType.Boolean},
                {limitConfigurationKey, ConfigurationItemType.Float}
            };

            var queryResult = configuration.GetConfigurations(configurationKeys);

            var enabled = queryResult[enabledConfigurationKey];
            var limit = queryResult[limitConfigurationKey];
            if(enabled != null && limit != null)
            {
                var newConfiguration = new ReelStripDistortionConfiguration((bool)enabled, (float)limit);

                if(!newConfiguration.Equals(ReelStripDistortionConfiguration))
                {
                    ReelStripDistortionConfiguration = newConfiguration;
                }
            }
        }

        #region Game Services

        /// <summary>
        /// Configuration used to influence the behavior of the DistortedSymbolFactory.
        /// </summary>
        [GameService]
        public ReelStripDistortionConfiguration ReelStripDistortionConfiguration { get; private set; }

        #endregion
    }
}
