// -----------------------------------------------------------------------
// <copyright file = "ConfigurableBetAndLinesProvider.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;

    /// <summary>
    /// Provides configurable bet and lines information from the foundation.
    /// </summary>
    /// <remarks>
    /// The configurable bet and lines configuration is provided by the configuration extension ButtonConfiguration.
    /// </remarks>
    public class ConfigurableBetAndLinesProvider
    {
        #region Private Members

        /// <summary>
        /// The id of the configurable bet and lines configuration extension.
        /// </summary>
        private const string ConfigurationExtensionId = "185ED5CE-B38A-4F37-936B-4188D6181C51";

        /// <summary>
        /// The name of the "ButtonLayoutConfiguration" element of the configurable bet and lines configuration extension.
        /// </summary>
        private const string ButtonLayoutConfigName = "ButtonsLayout";

        /// <summary>
        /// Lines dominant bet style string returned by the Foundation used to 
        /// translate to the appropriate <see cref="BetButtonLayout"/>.
        /// </summary>
        private const string DefaultBetSelection = "LINES top row, BET MULTIPLIER bottom row";

        /// <summary>
        /// The <see cref="IGameConfigurationRead"/> to read the buttonLayoutConfiguration configuration from.
        /// </summary>
        private readonly IGameConfigurationRead configuration;

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new configurable bet and lines provider.
        /// </summary>
        /// <param name="configuration">The configuration from which to get the reel strip configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown if configuration is null.</exception>
        public ConfigurableBetAndLinesProvider(IGameConfigurationRead configuration)
        {
            if(configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.configuration = configuration;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Read the configurable bet and lines provider configuration from the foundation.
        /// </summary>
        public void ReadButtonLayoutConfiguration()
        {
            var buttonLayoutConfigurationKey = GameConfigurationKey.NewExtensionKey(ConfigurationExtensionId,
                ButtonLayoutConfigName);
            bool betPanelStyle = configuration.IsConfigurationDefined(buttonLayoutConfigurationKey);

            if(betPanelStyle)
            {
                var betStyleSelection = configuration.GetConfiguration<string>(buttonLayoutConfigurationKey,
                    ConfigurationItemType.Item);

                var selectedStyle = betStyleSelection == DefaultBetSelection
                    ? BetButtonLayout.LinesDominant
                    : BetButtonLayout.MultiplierDominant;

                var configButtonLayout = new ConfigurableBetAndLinesConfiguration(selectedStyle);

                if(!configButtonLayout.Equals(ConfigurableButtonPanelLayoutConfiguration))
                {
                    ConfigurableButtonPanelLayoutConfiguration = configButtonLayout;
                }
            }
        }

        #endregion

        #region Game Services

        /// <summary>
        /// Configuration used to influence the layout of the button panel.
        /// </summary>
        [GameService]
        public ConfigurableBetAndLinesConfiguration ConfigurableButtonPanelLayoutConfiguration { get; private set; }

        #endregion
    }
}