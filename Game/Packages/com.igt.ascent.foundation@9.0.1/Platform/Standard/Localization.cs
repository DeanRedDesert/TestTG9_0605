// -----------------------------------------------------------------------
// <copyright file = "Localization.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// Implementation of the <see cref="ILocalization"/> interface that is backed by the F2X.
    /// </summary>
    /// <typeparam name="TContext">Type of the context.</typeparam>
    internal sealed class Localization<TContext> : ILocalization, IContextCache<TContext> where TContext : class
    {
        #region Private Fields

        private readonly CategoryInitializer<ILocalizationCategory> localizationCategory =
            new CategoryInitializer<ILocalizationCategory>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The interface of localization category.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(ILocalizationCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            localizationCategory.Initialize(category);
        }

        #endregion

        #region ILocalization Implementation

        /// <inheritdoc/>
        public CreditFormatter CreditFormatter { get; private set; }

        /// <inheritdoc/>
        public TimeOfDayFormat TimeOfDayFormat { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> LanguageIconInformation { get; private set; }

        #endregion

        #region IContextCache<in TContext> Implementation

        /// <inheritdoc />
        public void NewContext(TContext newContext)
        {
            CreditFormatter = GetConfigDataCreditFormatter();

            TimeOfDayFormat = GetConfigDataTimeOfDayFormat();

            LanguageIconInformation = GetConfigDataLanguageIconInformation();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the config data of credit formatter from the Foundation.
        /// </summary>
        /// <returns>
        /// The credit formatter config data.
        /// </returns>
        private CreditFormatter GetConfigDataCreditFormatter()
        {
            var reply = localizationCategory.Instance.GetCreditFormatterInfo();

            var numericDecimalSeparator = RestoreSeparator(reply.NumericDecimalSeparator);

            var numericGroupSeparator = RestoreSeparator(reply.NumericGroupSeparator);

            // Setting the currency symbol to "None" will cause the deserialized XML message to be null.
            // If the deserialized message data is null set the whole unit currency symbol to an empty string.
            var wholeUnitSymbol = reply.WholeUnitCurrencySymbol.Value ?? string.Empty;
            var wholeUnitSymbolPosition = reply.WholeUnitCurrencySymbol.CurrencySymbolPosition.ToPublic();

            string centSymbol;
            CurrencySymbolPosition centSymbolPosition;

            // This node is optional.  We need to check if it is available.
            if(reply.BaseUnitCurrencySymbol != null)
            {
                centSymbol = reply.BaseUnitCurrencySymbol.Value;
                centSymbolPosition = reply.BaseUnitCurrencySymbol.CurrencySymbolPosition.ToPublic();
            }
            else
            {
                centSymbol = null;
                centSymbolPosition = CurrencySymbolPosition.Right;
            }

            return new CreditFormatter(numericDecimalSeparator,
                                       numericGroupSeparator,
                                       wholeUnitSymbol,
                                       centSymbol,
                                       wholeUnitSymbolPosition,
                                       centSymbolPosition,
                                       reply.NegativeAmountDisplayFormat.ToNegativeSignPosition(),
                                       reply.UseGroupSeparatorForNonMonetaryNumbers);
        }

        /// <summary>
        /// Gets the config data of time-of-day format from the Foundation.
        /// </summary>
        /// <returns>
        /// The time-of-day format config data.
        /// </returns>
        private TimeOfDayFormat GetConfigDataTimeOfDayFormat()
        {
            return (TimeOfDayFormat)localizationCategory.Instance.GetTimeOfDayFormat();
        }

        /// <summary>
        /// Gets the config data of language icon information from the Foundation.
        /// </summary>
        /// <returns>
        /// The language icon information config data.
        /// </returns>
        private Dictionary<string, string> GetConfigDataLanguageIconInformation()
        {
            var reply = localizationCategory.Instance.QueryLanguageIcons();

            // If language icons are available return it, otherwise return null
            // so that the presentation assets default to the old behavior.
            return reply?.ToDictionary(entry => entry.CultureString, entry => entry.LanguageIconId);
        }

        /// <summary>
        /// Restores the numeric separator represented by a string received in F2X message.
        /// </summary>
        /// <remarks>
        /// If the separator is selected to "SPACE" in configuration page, the process of deserializing 
        /// the XML message will drop the space, and the related field will get an empty string. 
        /// We have to correct it to " ".
        /// </remarks>
        /// <param name="f2XString">
        /// The original string received in F2X message.
        /// </param>
        /// <returns>
        /// The separator string after restoration.
        /// </returns>
        private static string RestoreSeparator(string f2XString)
        {
            return f2XString == string.Empty ? " " : f2XString ?? string.Empty;
        }

        #endregion
    }
}