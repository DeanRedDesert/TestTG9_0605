//-----------------------------------------------------------------------
// <copyright file = "LegacyLocalizationInformation.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2LLink
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;
    using F2L;
    using F2XLinks;
    using Money;

    /// <summary>
    /// Legacy implementation of <see cref="ILocalizationInformation"/> interface that requests
    /// localization information from the Foundation.
    /// </summary>
    internal class LegacyLocalizationInformation : ILocalizationInformation, IContextCache
    {
        #region Private Fields

        /// <summary>
        /// Game control category interface. The category interface used to request game control information.
        /// </summary>
        private IGameControlCategory gameControlCategory;

        /// <summary>
        /// Transaction verification. Interface for verifying that a transaction is open. 
        /// </summary>
        private readonly ITransactionVerification transactionVerification;

        /// <summary>
        /// Cached CreditFormatter. Will be null if the value has not been cached.
        /// </summary>
        private CreditFormatter cachedCreditFormatter;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="LegacyLocalizationInformation"/>.
        /// </summary>
        /// <param name="transactionVerification">
        /// The interface for verifying that a transaction is open.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transactionVerification"/> is null.
        /// </exception>
        public LegacyLocalizationInformation(ITransactionVerification transactionVerification)
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
        /// <param name="gameControlHandler">
        /// The interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="gameControlHandler"/> is null.
        /// </exception>
        public void Initialize(IGameControlCategory gameControlHandler)
        {
            if(gameControlHandler == null)
            {
                throw new ArgumentNullException("gameControlHandler");
            }

            gameControlCategory = gameControlHandler;
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
                var legacyCreditFormatting = gameControlCategory.GetCreditFormatting();

                // If the separator is selected to "SPACE" in configuration page, the process of deserializing 
                // the XML message will drop the space, and the related field will get an empty string. 
                // We have to correct it to " ". 
                var numericDecimalSeparator = legacyCreditFormatting.NumericDecimalSeparator == string.Empty
                    ? " "
                    : legacyCreditFormatting.NumericDecimalSeparator ?? string.Empty;

                var numericGroupSeparator = legacyCreditFormatting.NumericGroupSeparator == string.Empty
                    ? " "
                    : legacyCreditFormatting.NumericGroupSeparator ?? string.Empty;

                var baseUnitCurrencySymbol = legacyCreditFormatting.BaseUnitCurrencySymbol ?? "";
                var useCreditSeparator = legacyCreditFormatting.UseGroupSeparatorForNonMonetaryNumbersSpecified &&
                                            legacyCreditFormatting.UseGroupSeparatorForNonMonetaryNumbers;

                cachedCreditFormatter = new CreditFormatter(numericDecimalSeparator,
                                                        numericGroupSeparator,
                                                        legacyCreditFormatting.WholeUnitCurrencySymbol, baseUnitCurrencySymbol,
                                                        useCreditSeparator);
            }

            return cachedCreditFormatter;
        }

        /// <inheritdoc />
        public TimeOfDayFormat GetTimeOfDayFormat()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();
            return TimeOfDayFormat.Invalid;
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetLanguageIconInformation()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            return null;
        }

        #endregion

        #region IContextCache Members

        /// <inheritdoc />
        public void NewContext()
        {
            cachedCreditFormatter = null;
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
            if(gameControlCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "LegacyLocalizationInformation cannot be used without calling its Initialize method first.");
            }
        }

        #endregion
    }

}
