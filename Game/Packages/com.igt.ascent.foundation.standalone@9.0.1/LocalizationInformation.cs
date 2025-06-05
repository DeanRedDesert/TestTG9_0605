//-----------------------------------------------------------------------
// <copyright file = "LocalizationInformation.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Ascent.Communication.Platform.Interfaces;
    using Money;

    /// <summary>
    /// Implementation of <see cref="ILocalizationInformation"/> interface that provides
    /// localization information.
    /// </summary>
    internal class LocalizationInformation : ILocalizationInformation
    {
        private CultureInfo cultureInfo;
        private CreditFormatter creditFormatter;

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizationInformation"/>.
        /// </summary>
        public LocalizationInformation()
        {
            cultureInfo = new CultureInfo("en-US");
            var format = cultureInfo.NumberFormat;

            creditFormatter = new CreditFormatter(format.CurrencyDecimalSeparator,
                                                  format.CurrencyGroupSeparator,
                                                  format.CurrencySymbol,
                                                  string.Empty,
                                                  (CurrencySymbolPosition)format.CurrencyPositivePattern,
                                                  CurrencySymbolPosition.Right,
                                                  NegativeNumberFormat.InParentheses,
                                                  true);
        }

        /// <summary>
        /// Sets the current culture which is used to create a
        /// <see cref="CreditFormatter"/>.
        /// </summary>
        /// <param name="cultureName">Culture to use.</param>
        public void SetCulture(string cultureName)
        {
            if(cultureInfo.Name != cultureName)
            {
                cultureInfo = new CultureInfo(cultureName);
                var format = cultureInfo.NumberFormat;

                creditFormatter = new CreditFormatter(format.CurrencyDecimalSeparator,
                                                      format.CurrencyGroupSeparator,
                                                      format.CurrencySymbol,
                                                      string.Empty,
                                                      (CurrencySymbolPosition)format.CurrencyPositivePattern,
                                                      CurrencySymbolPosition.Right,
                                                      NegativeNumberFormat.InParentheses,
                                                      true);
            }
        }

        /// <summary>
        /// Set credit formatter by caller,
        /// </summary>
        /// <param name="newCreditFormatter">
        /// The new credit formatter value to set by caller.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="newCreditFormatter"/> is null.
        /// </exception>
        public void SetCreditFormatter(CreditFormatter newCreditFormatter)
        {
            creditFormatter = newCreditFormatter ?? throw new ArgumentNullException(nameof(newCreditFormatter));
        }

        #region ILocalizationInformation Members

        /// <inheritdoc/>
        public CreditFormatter GetCreditFormatter()
        {
            return creditFormatter;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Return <see cref="TimeOfDayFormat.Invalid"/> so that the game is free to use whatever format it likes.
        /// </remarks>
        public TimeOfDayFormat GetTimeOfDayFormat()
        {
            return TimeOfDayFormat.Invalid;
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetLanguageIconInformation()
        {
            return new Dictionary<string, string>
            {
                { "cs-CZ", "CZ" },//Czech
                { "bg-BG", "BG" },//Bulgarian
                { "en-US", "US" },//US English
                { "en-AU", "AU" },//Australia English
                { "es-ES", "ES" },//Spanish
                { "fr-FR", "FR" },//France French 
                { "fr-CA", "CA-QC" },//Canada French
                { "is-IS", "IS" },//Icelandic
                { "pt-BR", "BR" },//Brazil Portuguese
                { "pt-PT", "PT" },//Portugal Portuguese
                { "ro-RO", "RO" },//Romanian
                { "ru-RU", "RU" },//Russian
                { "sr-Latn-RS", "RS" },//Serbian
                { "uk-UA", "UA" },//Ukrainian
                { "zh-CN", "CN" } //Chinese
            };
        }

        #endregion
    }
}