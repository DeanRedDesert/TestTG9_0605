//-----------------------------------------------------------------------
// <copyright file = "ILocalizationInformation.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System.Collections.Generic;
    using Game.Core.Money;

    /// <summary>
    /// This interface defines the methods to request
    /// localization information from the Foundation.
    /// </summary>
    public interface ILocalizationInformation
    {
        /// <summary>
        /// Requests the credit formatter information.
        /// </summary>
        /// <returns>The credit formatter information.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        CreditFormatter GetCreditFormatter();

        /// <summary>
        /// Requests the time of day format information.
        /// </summary>
        /// <returns>
        /// The format to display the time of day values.
        /// </returns>
        TimeOfDayFormat GetTimeOfDayFormat();

        /// <summary>
        /// Requests the language icon information.
        /// </summary>
        /// <returns>
        /// A dictionary mapping culture codes to language icon ids or <code>null</code> if the Foundation contains no language
        /// icon information.
        /// </returns>
        /// <remarks>
        /// In case no language icon information is provided by the foundation, the game may choose to select an appropriate
        /// language icon or default to a language agnostic icon.
        /// </remarks>
        IDictionary<string, string> GetLanguageIconInformation();
    }
}
