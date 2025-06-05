//-----------------------------------------------------------------------
// <copyright file = "IGameInformation.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines the methods to request game information from the foundation.
    /// </summary>
    public interface IGameInformation
    {
        #region Information on Theme(s)

        /// <summary>
        /// Gets a collection of tag information of all paytables for a list of themes.
        /// </summary>
        /// <param name="themeIdentifiers">
        /// The identifiers of the themes used to query information from Foundation.
        /// </param>
        /// <returns>
        /// The collection of <see cref="PaytableTag"/> lists, keyed by the theme identifiers passed in.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="themeIdentifiers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="themeIdentifiers"/> is empty.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IDictionary<string, IList<PaytableTag>> GetAllPaytableTagsForThemes(IEnumerable<string> themeIdentifiers);

        /// <summary>
        /// Gets a collection of tag information of enabled paytables for a list of themes.
        /// </summary>
        /// <param name="themeIdentifiers">
        /// The identifiers of the themes used to query information from Foundation.
        /// </param>
        /// <returns>
        /// The collection of <see cref="PaytableTag"/> lists, keyed by the theme identifiers passed in.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="themeIdentifiers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="themeIdentifiers"/> is empty.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IDictionary<string, IList<PaytableTag>> GetEnabledPaytableTagsForThemes(IEnumerable<string> themeIdentifiers);

        /// <summary>
        /// Gets a collection of enabled paytables with enabled denominations for a list of themes.
        /// </summary>
        /// <param name="themeIdentifiers">
        /// The identifiers of the themes used to query information from Foundation.
        /// </param>
        /// <returns>
        /// The collection of <see cref="PaytableDenominationInfo"/> lists, keyed by the theme identifiers passed in.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="themeIdentifiers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="themeIdentifiers"/> is empty.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IDictionary<string, IList<PaytableDenominationInfo>> GetEnabledPaytableDenominationInfos(IEnumerable<string> themeIdentifiers);

        /// <summary>
        /// Gets a collection of enabled paytables with enabled denomination for a theme
        /// </summary>
        /// <param name="themeIdentifier">
        /// The identifier of the theme used to query information from Foundation.
        /// </param>
        /// <returns>
        /// The list of <see cref="PaytableDenominationInfo"/>.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IEnumerable<PaytableDenominationInfo> GetEnabledPaytableDenominationInfo(string themeIdentifier);

        /// <summary>
        /// Gets the theme tag information for a given theme identifier.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme used to query the theme tag information from Foundation.</param>
        /// <returns>An instance of type <see cref="ThemeTag"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="themeIdentifier"/> is empty or null.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if tag information for <paramref name="themeIdentifier"/> could not be found.
        /// </exception>
        ThemeTag GetThemeTag(string themeIdentifier);

        /// <summary>
        /// Gets the status of the theme along with the theme tag information.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme.</param>
        /// <returns>
        /// A key value pair of the theme tag information combined with the status of the theme.
        /// The status is "true" if at least one of the theme's payvars is enabled with a denomination.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="themeIdentifier"/> is empty or null.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        KeyValuePair<ThemeTag, bool> GetThemeStatus(string themeIdentifier);

        #endregion

        #region Information on Paytable(s)

        /// <summary>
        /// Gets the tag information on paytable identified by the given identifier.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme.</param>
        /// <param name="paytableIdentifier">The identifier of the paytable.</param>
        /// <returns>
        /// The tag information for the paytable.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        PaytableTag GetPaytableTag(string themeIdentifier, string paytableIdentifier);

        /// <summary>
        /// Gets the payback information for a list of paytable identifiers.
        /// </summary>
        /// <param name="paytableIdentifiers">The list of paytable identifiers.</param>
        /// <returns>
        /// The list of payback information for the given paytables.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="paytableIdentifiers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="paytableIdentifiers"/> is empty.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IEnumerable<PaytablePaybackInfo> GetPaybackInfos(IEnumerable<string> paytableIdentifiers);

        /// <summary>
        /// Gets the payback information for the paytable identified by the given identifier.
        /// </summary>
        /// <param name="paytableIdentifier">The identifier of the paytable.</param>
        /// <returns>
        /// The payback information for the paytable.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        PaytablePaybackInfo GetPaybackInfo(string paytableIdentifier);

        /// <summary>
        /// Gets the enabled denominations for a list of paytable identifiers.
        /// </summary>
        /// <param name="paytableIdentifiers">The list of paytable identifiers.</param>
        /// <returns>
        /// The list of enabled denominations for the given paytables.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="paytableIdentifiers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="paytableIdentifiers"/> is empty.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IList<PaytableDenominationInfo> GetEnabledDenominationInfos(IEnumerable<string> paytableIdentifiers);

        /// <summary>
        /// Gets the enabled denominations for the paytable identified by the given identifier.
        /// </summary>
        /// <param name="paytableIdentifier">The identifier of the paytable.</param>
        /// <returns>
        /// The list of enabled denominations for the given paytable.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IEnumerable<long> GetEnabledDenominations(string paytableIdentifier);

        /// <summary>
        /// Gets the supported denominations for the paytable identified by the given identifier.
        /// </summary>
        /// <param name="paytableIdentifier">The identifier of the paytable.</param>
        /// <returns>
        /// The list of supported denominations for the given paytable.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        IEnumerable<long> GetSupportedDenominations(string paytableIdentifier);

        /// <summary>
        /// Gets the max bet, in credits, for the given paytable and denomination.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme.</param>
        /// <param name="paytableIdentifier">The identifier of the paytable.</param>
        /// <param name="denomination">The denomination.</param>
        /// <returns>
        /// The max bet, in credits, for the given paytable and denomination.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        long GetMaxBet(string themeIdentifier, string paytableIdentifier, long denomination);

        /// <summary>
        /// Gets the button panel min bet, in credits, for the given paytable and denomination.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme.</param>
        /// <param name="paytableIdentifier">The identifier of the paytable.</param>
        /// <param name="denomination">The denomination.</param>
        /// <returns>
        /// The button panel min bet, in credits, for the given paytable and denomination.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        long GetButtonPanelMinBet(string themeIdentifier, string paytableIdentifier, long denomination);

        #endregion
    }
}
