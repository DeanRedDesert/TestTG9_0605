//-----------------------------------------------------------------------
// <copyright file = "LocalizationInformation.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2XCallbacks;
    using Money;

    /// <summary>
    /// Implementation of <see cref="ILocalizationInformation"/> interface that requests
    /// localization information from the Foundation.
    /// </summary>
    internal class LocalizationInformation : ILocalizationInformation, IContextCache
    {
        #region Private Fields

        /// <summary>
        /// Transaction verification. Interface for verifying that a transaction is open. 
        /// </summary>
        private readonly ITransactionVerification transactionVerification;

        /// <summary>
        /// Localization category interface. The category interface used to request localization information.
        /// </summary>
        private ILocalizationCategory localizationCategory;

        /// <summary>
        /// Cached CreditFormatter. Will be null if the value has not been cached.
        /// </summary>
        private CreditFormatter cachedCreditFormatter;

        /// <summary>
        /// Cached flag information. Will be null if the value has not been cached.
        /// </summary>
        private Dictionary<string, string> cachedLanguageIconInformation;

        /// <summary>
        /// Cached TimeOfDayFormat. Will be null if the value has not been cached.
        /// </summary>
        /// <remarks>The type is nullable, because the type being cached is an enum.</remarks>
        private TimeOfDayFormat? cachedTimeOfDayFormat;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizationInformation"/>.
        /// </summary>
        /// <param name="transactionVerification">
        /// The interface for verifying that a transaction is open.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transactionVerification"/> is null.
        /// </exception>
        public LocalizationInformation(ITransactionVerification transactionVerification)
        {
            if(transactionVerification == null)
            {
                throw new ArgumentNullException("transactionVerification");
            }

            this.transactionVerification = transactionVerification;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="localizationHandler">
        /// The interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="localizationHandler"/> is null.
        /// </exception>
        public void Initialize(ILocalizationCategory localizationHandler)
        {
            if(localizationHandler == null)
            {
                throw new ArgumentNullException("localizationHandler");
            }

            localizationCategory = localizationHandler;
        }

        #endregion

        #region ILocalizationInformation Members

        /// <inheritdoc />
        public CreditFormatter GetCreditFormatter()
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            if(cachedCreditFormatter == null)
            {
                var reply = localizationCategory.GetCreditFormatterInfo();

                // This node is optional.  We need to check if it is available.
                var centSymbolSetting = reply.BaseUnitCurrencySymbol;

                // If the separator is selected to "SPACE" in configuration page, the process of deserializing 
                // the XML message will drop the space, and the related field will get an empty string. 
                // We have to correct it to " ". 
                var numericDecimalSeparator = reply.NumericDecimalSeparator == string.Empty ? " " : reply.NumericDecimalSeparator ?? string.Empty;

                var numericGroupSeparator = reply.NumericGroupSeparator == string.Empty ? " " : reply.NumericGroupSeparator ?? string.Empty;

                // Setting the currency symbol to "None" will cause the deserialized XML message to be null.
                // If the deserialized message data is null set the whole unit currency symbol to an empty string.
                var wholeUnitCurrencySymbol = reply.WholeUnitCurrencySymbol.Value ?? string.Empty;

                cachedCreditFormatter = new CreditFormatter(numericDecimalSeparator,
                                                            numericGroupSeparator,
                                                            wholeUnitCurrencySymbol,
                                                            centSymbolSetting == null
                                                                ? null
                                                                : centSymbolSetting.Value,
                                                            reply.WholeUnitCurrencySymbol.CurrencySymbolPosition.ToPublic(),
                                                            centSymbolSetting == null
                                                                ? CurrencySymbolPosition.Right
                                                                : centSymbolSetting.CurrencySymbolPosition.ToPublic(),
                                                            reply.NegativeAmountDisplayFormat.ToNegativeSignPosition(),
                                                            reply.UseGroupSeparatorForNonMonetaryNumbers);
            }

            return cachedCreditFormatter;
        }

        /// <inheritdoc />
        public TimeOfDayFormat GetTimeOfDayFormat()
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            if(cachedTimeOfDayFormat == null)
            {
                cachedTimeOfDayFormat = (TimeOfDayFormat)localizationCategory.GetTimeOfDayFormat();
            }

            return cachedTimeOfDayFormat.Value;
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetLanguageIconInformation()
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            IDictionary<string, string> result = null;

            if(cachedLanguageIconInformation != null)
            {
                // Return new dictionary, so that modifications done by the game do not destroy the information coming from the
                // foundation. Can be replaced with a ReadOnlyDictionary wrapper when we switch to .NET 4.5 or higher.
                // Change this to return the cachedLanguageIconInformation in case of performance issues.
                result = new Dictionary<string, string>(cachedLanguageIconInformation);
            }
            else
            {
                var languageIcons = localizationCategory.QueryLanguageIcons();
                // If language icons are available return it, otherwise return null so that the presentation assets default to the
                // old behavior.
                if(languageIcons != null)
                {
                    cachedLanguageIconInformation = languageIcons.ToDictionary(entry => entry.CultureString,
                                                                               entry => entry.LanguageIconId);
                    result = new Dictionary<string, string>(cachedLanguageIconInformation);
                }
            }

            return result;
        }

        #endregion

        #region IContextCache Members

        /// <inheritdoc />
        public void NewContext()
        {
            cachedCreditFormatter = null;
            cachedTimeOfDayFormat = null;
            cachedLanguageIconInformation = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if this object has been initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when any API is called before Initialize is called.
        /// </exception>
        private void CheckInitialization()
        {
            if(localizationCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "LocalizationInformation cannot be used without calling its Initialize method first.");
            }
        }

        #endregion
    }
}
