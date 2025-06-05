//-----------------------------------------------------------------------
// <copyright file = "LanguageIconInformationProvider.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Services;

    /// <summary>
    /// Provides information about Language Icons which should be used when representing languages.
    /// </summary>
    /// <remarks>
    /// Language icon information is provided by the Foundation starting with M-Series.
    /// </remarks>
    public class LanguageIconInformationProvider
    {
        /// <summary>
        /// The game library.
        /// </summary>
        private readonly IGameLib gameLib;

        /// <summary>
        /// Provides information on which language icon to use for which language.
        /// </summary>
        /// <remarks>
        /// The keys are culture codes as specified in <see cref="System.Globalization.CultureInfo"/>. The values are country codes
        /// as defined by the ISO 3166-2 standard.
        /// </remarks>
        [GameService]
        public IDictionary<string, string> LanguageIconInformation
        {
            get
            {
                return gameLib.LocalizationInformation.GetLanguageIconInformation();
            }
        }

        /// <summary>
        /// Creates a new LanguageIconProvider.
        /// </summary>
        /// <param name="gameLib">The game library.</param>
        public LanguageIconInformationProvider(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException("gameLib");
            }

            this.gameLib = gameLib;
        }
    }
}
