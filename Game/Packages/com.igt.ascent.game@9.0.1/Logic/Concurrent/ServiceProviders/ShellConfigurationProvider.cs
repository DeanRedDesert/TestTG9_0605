// -----------------------------------------------------------------------
// <copyright file = "ShellConfigurationProvider.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Logic.Services;
    using Game.Core.Money;

    /// <summary>
    /// A provider that provides the config data of a Shell.
    /// </summary>
    /// <remarks>
    /// As config data never change during an activate context, services provided here are all Synchronous Game Services.
    /// </remarks>
    /// <devdoc>
    /// NO Asynchronous Game Service should be provided here.
    /// DO NOT subscribe to any Shell Lib events in this provider!
    /// </devdoc>
    public class ShellConfigurationProvider : NonObserverProviderBase
    {
        #region Constants

        private const string DefaultName = nameof(ShellConfigurationProvider);

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the credit formatter.
        /// </summary>
        [GameService]
        public CreditFormatter CreditFormatter { get; }

        /// <summary>
        /// Gets the time of day format.
        /// </summary>
        [GameService]
        public TimeOfDayFormat TimeOfDayFormat { get; }

        /// <summary>
        /// Gets the information on which language icon to use for which language.
        /// </summary>
        /// <remarks>
        /// The keys are culture codes as specified in <see cref="System.Globalization.CultureInfo"/>.
        /// The values are country codes as defined by the ISO 3166-2 standard.
        /// </remarks>
        [GameService]
        public IReadOnlyDictionary<string, string> LanguageIconInformation { get; }

        /// <summary>
        /// Gets the cultures available for the player to pick.
        /// </summary>
        /// <devdoc>
        /// We have to use List here because Fuel LocalizationController.EnabledLanguages
        /// is expecting the List type.  Future clean up should start with modifying Fuel
        /// to consume IReadOnlyList rather than List, followed by modifying this provider
        /// and other providers such as LanguageConfigurationProvider.
        /// </devdoc>
        [GameService]
        public List<string> AvailableCultures { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellConfigurationProvider"/>.
        /// </summary>
        /// <param name="shellLib">
        /// The shell lib that provides config data for the GameServices in this provider.
        /// </param>
        /// <param name="name">
        /// The name of the service provider.
        /// This parameter is optional.  If not specified, the provider name will be set to <see cref="DefaultName"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="shellLib"/> is null.
        /// </exception>
        public ShellConfigurationProvider(IShellLib shellLib, string name = DefaultName) : base(name)
        {
            if(shellLib == null)
            {
                throw new ArgumentNullException(nameof(shellLib));
            }

            CreditFormatter = shellLib.Localization.CreditFormatter;
            TimeOfDayFormat = shellLib.Localization.TimeOfDayFormat;
            LanguageIconInformation = shellLib.Localization.LanguageIconInformation;
            AvailableCultures = shellLib.GameCulture.AvailableCultures?.ToList();
        }

        #endregion
    }
}