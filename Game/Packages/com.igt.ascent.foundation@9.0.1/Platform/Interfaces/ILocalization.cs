// -----------------------------------------------------------------------
// <copyright file = "ILocalization.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System.Collections.Generic;
    using Game.Core.Money;

    /// <summary>
    /// This interface defines APIs for an application to query localization related config data.
    /// </summary>
    public interface ILocalization
    {
        /// <summary>
        /// Gets the credit formatter.
        /// </summary>
        CreditFormatter CreditFormatter { get; }

        /// <summary>
        /// Gets the time of day format.
        /// </summary>
        TimeOfDayFormat TimeOfDayFormat { get; }

        /// <summary>
        /// Gets the language icon information, which is a dictionary that maps
        /// culture codes to language icon ids or <code>null</code> if the Foundation
        /// contains no language icon information.
        /// </summary>
        /// <remarks>
        /// In case no language icon information is provided by the Foundation, the application may
        /// choose to select an appropriate language icon or default to a language agnostic icon.
        /// </remarks>
        IReadOnlyDictionary<string, string> LanguageIconInformation { get; }
    }
}