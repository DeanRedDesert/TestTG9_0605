//-----------------------------------------------------------------------
// <copyright file = "ReportContext.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Globalization;
    using Ascent.Communication.Platform.Interfaces;
    using Logic.PaytableLoader.Interfaces;
    using Money;

    /// <summary>
    /// Represents the context for when a report is requested from the Foundation.
    /// </summary>
    public class ReportContext
    {
        /// <summary>
        /// Instantiates a new <see cref="ReportContext"/>. 
        /// </summary>
        /// <param name="culture">Predefined culture string for the report.</param>
        /// <param name="denomination">Game denomination for the report.</param>
        /// <param name="themeIdentifier">The theme identifier the report is being generated for.</param>
        /// <param name="paytableTag"><see cref="PaytableTag"/> that represents the report paytable.</param>
        /// <param name="maxBet">The max bet, in credits, for the specified denomination and paytable.</param>
        /// <param name="creditFormatter">The formatter used for formatting amount values as currency.</param>
        /// <param name="paytableData">The <see cref="IGenericPaytableData"/> object containing paytable data common to all paytable formats.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="culture"/> is null or empty, or if <paramref name="paytableTag"/>
        /// is empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="creditFormatter"/> is null.
        /// </exception>
        internal ReportContext(string culture, long denomination, string themeIdentifier, PaytableTag paytableTag,
                               long maxBet, CreditFormatter creditFormatter, IGenericPaytableData paytableData)
        {
            if(string.IsNullOrEmpty(culture))
            {
                throw new ArgumentException("culture must not be null or empty", nameof(culture));
            }

            if(paytableTag == new PaytableTag())
            {
                throw new ArgumentException("paytableTag must not be empty", nameof(paytableTag));
            }

            if(creditFormatter == null)
            {
                throw new ArgumentNullException(nameof(creditFormatter), "creditFormatter must not be null");
            }

            Culture = culture;
            CultureInfo = new CultureInfo(culture);
            Denomination = denomination;
            ThemeIdentifier = themeIdentifier;
            PaytableTag = paytableTag;
            MaxBet = maxBet;
            CreditFormatter = creditFormatter;
            PaytableData = paytableData;
        }

        /// <summary>
        /// Gets the predefined culture string.
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> used for formatting.
        /// </summary>
        public CultureInfo CultureInfo { get; }

        /// <summary>
        /// Gets the game denomination.
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        /// Gets the theme identifier the report is being generated for.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// Gets the <see cref="PaytableTag"/> that represents the
        /// active paytable.
        /// </summary>
        public PaytableTag PaytableTag { get; }

        /// <summary>
        /// Gets the maximum bet, in credits, for the current denomination and paytable.
        /// </summary>
        public long MaxBet { get; }

        /// <summary>
        /// Gets the <see cref="IGenericPaytableData"/> object.
        /// </summary>
        public IGenericPaytableData PaytableData
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="CreditFormatter"/> used
        /// for formatting amount values as currency.
        /// </summary>
        public CreditFormatter CreditFormatter { get; }

    }
}
