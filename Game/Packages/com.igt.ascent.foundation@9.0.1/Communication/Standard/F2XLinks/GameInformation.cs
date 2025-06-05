//-----------------------------------------------------------------------
// <copyright file = "GameInformation.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.GameInformation;
    using F2X.Schemas.Internal.Types;
    using F2XCallbacks;
    using GroupPayvarInformation = F2X.Schemas.Internal.GameGroupInformation.PayvarInformation;
    using PayvarInformation = F2X.Schemas.Internal.GameInformation.PayvarInformation;

    /// <summary>
    /// Standard implementation of <see cref="IGameInformation"/> that communicates with the Foundation
    /// for game information.
    /// </summary>
    internal class GameInformation : IGameInformation
    {       
        #region Private Fields

        /// <summary>
        /// The interface for querying game information.
        /// </summary>
        private IGameInformationCategory cachedGameInformationCategory;

        /// <summary>
        /// The interface for querying game group information.
        /// </summary>
        private IGameGroupInformationCategory cachedGameGroupInformationCategory;

        /// <summary>
        /// The interface for verifying transactions.
        /// </summary>
        private readonly ITransactionVerification transactionVerification;

        /// <summary>
        /// The mount point of the package.
        /// </summary>
        private string cachedMountPoint;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new instance of <see cref="GameInformation"/>.
        /// </summary>
        /// <param name="transactionVerification">
        /// The interface for verifying that a transaction is open.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="transactionVerification"/> is null.
        /// </exception>
        public GameInformation(ITransactionVerification transactionVerification)
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
        /// <param name="gameInformationCategory">
        /// The interface for querying game information.
        /// </param>
        /// <param name="gameGroupInformationCategory">
        /// The interface for querying game group information.
        /// </param>
        /// <param name="mountPoint">
        /// The mount point of the package.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="gameInformationCategory"/> is null.
        /// </exception>
        public void Initialize(IGameInformationCategory gameInformationCategory,
                               IGameGroupInformationCategory gameGroupInformationCategory,
                               string mountPoint)
        {
            if(gameInformationCategory == null)
            {
                throw new ArgumentNullException("gameInformationCategory");
            }

            cachedGameInformationCategory = gameInformationCategory;
            cachedGameGroupInformationCategory = gameGroupInformationCategory;

            cachedMountPoint = mountPoint;
        }

        #endregion

        #region IGameInformation Implementation

        #region Information on Theme(s)

        /// <inheritdoc/>
        public IDictionary<string, IList<PaytableTag>> GetAllPaytableTagsForThemes(IEnumerable<string> themeIdentifiers)
        {
            var themeIdentifierList = CheckAndConvertThemeIdentifiers(themeIdentifiers);

            return GetPaytableTags(themeIdentifierList, false);
        }

        /// <inheritdoc/>
        public IDictionary<string, IList<PaytableTag>> GetEnabledPaytableTagsForThemes(IEnumerable<string> themeIdentifiers)
        {
            var themeIdentifierList = CheckAndConvertThemeIdentifiers(themeIdentifiers);

            return GetPaytableTags(themeIdentifierList, true);
        }

        /// <inheritdoc/>
        public IDictionary<string, IList<PaytableDenominationInfo>> GetEnabledPaytableDenominationInfos(IEnumerable<string> themeIdentifiers)
        {
            var themeIdentifierList = CheckAndConvertThemeIdentifiers(themeIdentifiers);

            var themePaytableTags = GetPaytableTags(themeIdentifierList, true);

            var result = new Dictionary<string, IList<PaytableDenominationInfo>>();

            foreach(var themePaytableTag in themePaytableTags)
            {
                var payvarIdentifiers = themePaytableTag.Value.Select(tag => tag.PaytableIdentifier);

                result[themePaytableTag.Key] = GetEnabledDenominationInfos(payvarIdentifiers);
            }

            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<PaytableDenominationInfo> GetEnabledPaytableDenominationInfo(string themeIdentifier)
        {
            var infos = GetEnabledPaytableDenominationInfos(new List<string> { themeIdentifier });

            return infos[themeIdentifier];
        }

        /// <inheritdoc/>
        public ThemeTag GetThemeTag(string themeIdentifier)
        {
            return GetThemeStatus(themeIdentifier).Key;
        }

        /// <inheritdoc/>
        public KeyValuePair<ThemeTag, bool> GetThemeStatus(string themeIdentifier)
        {
            if(string.IsNullOrEmpty(themeIdentifier))
            {
                throw new ArgumentNullException("themeIdentifier");
            }

            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            var themes = new List<ThemeIdentifier> { new ThemeIdentifier { Value = themeIdentifier } };
            var themeInformationList = cachedGameInformationCategory.GetThemeInformation(themes);
            var themeInfo = themeInformationList.FirstOrDefault();
            return new KeyValuePair<ThemeTag, bool>(themeInfo.ToPublic(), themeInfo != null && themeInfo.Enabled);
        }

        #endregion

        #region Information on Paytable(s)

        /// <inheritdoc/>
        public PaytableTag GetPaytableTag(string themeIdentifier, string paytableIdentifier)
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            // Check up if this paytable belongs to a paytable group.
            var isInGroup = GetGroupPaytableInformation(themeIdentifier, paytableIdentifier) != null;

            var replyEntries = cachedGameInformationCategory.GetPayvarInformation(
                new List<PayvarIdentifier> { paytableIdentifier.ToPayvarIdentifier() });

            // When building the result, we don't care if the payvar is enabled or not.
            var paytableInfo = replyEntries.First(entry => entry.Payvar.Value == paytableIdentifier);

            return CreatePaytableTag(paytableInfo, isInGroup);
        }

        /// <inheritdoc/>
        public IEnumerable<PaytablePaybackInfo> GetPaybackInfos(IEnumerable<string> paytableIdentifiers)
        {
            var payvarIdentifierList = CheckAndConvertPayvarIdentifiers(paytableIdentifiers);

            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            var result = cachedGameInformationCategory.GetPayvarPaybackPercentageData(payvarIdentifierList)
                                                      .Select(percentageData => percentageData.ToPaytablePaybackInfo());

            return result;
        }

        /// <inheritdoc/>
        public PaytablePaybackInfo GetPaybackInfo(string paytableIdentifier)
        {
            return GetPaybackInfos(new List<string> { paytableIdentifier })
                        .First(info => info.PaytableIdentifier == paytableIdentifier);
        }

        /// <inheritdoc/>
        public IList<PaytableDenominationInfo> GetEnabledDenominationInfos(IEnumerable<string> paytableIdentifiers)
        {
            var payvarIdentifierList = CheckAndConvertPayvarIdentifiers(paytableIdentifiers);

            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            var replies = cachedGameInformationCategory.GetPayvarEnabledDenominations(payvarIdentifierList);

            return replies.SelectMany(entry => entry.Denomination != null
                                                   ? entry.Denomination.Select(denom => new PaytableDenominationInfo(
                                                                                            entry.Payvar.Value,
                                                                                            denom))
                                                   : new List<PaytableDenominationInfo>())

                          .ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<long> GetEnabledDenominations(string paytableIdentifier)
        {
            return GetEnabledDenominationInfos(new List<string> { paytableIdentifier })
                        .Where(info => info.PaytableIdentifier == paytableIdentifier)
                        .Select(info => info.Denomination);
        }

        /// <inheritdoc/>
        public IEnumerable<long> GetSupportedDenominations(string paytableIdentifier)
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            var reply = cachedGameInformationCategory.GetPayvarSupportedDenominations(
                                    new List<PayvarIdentifier>{ paytableIdentifier.ToPayvarIdentifier() });

            return reply.First(entry => entry.Payvar.Value == paytableIdentifier)
                        .Denomination.ConvertAll(denom => (long)denom);
        }

        /// <inheritdoc/>
        public long GetMaxBet(string themeIdentifier, string paytableIdentifier, long denomination)
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            ulong result = 0;

            // Check up if this paytable belongs to a paytable group.
            var paytableInfo = GetGroupPaytableInformation(themeIdentifier, paytableIdentifier);
            if(paytableInfo != null)
            {
                result = paytableInfo.RedefinedMaxbetCredits;
            }
            else
            {
                // So far, the given paytable doesn't belong to a paytable group.
                var replyEntries = cachedGameInformationCategory.GetBetResolution(
                    new List<ThemeIdentifier> { themeIdentifier.ToThemeIdentifier() });

                var betResolution = replyEntries.First(entry => entry.Theme.Value == themeIdentifier)
                    .BetResolution;

                switch(betResolution)
                {
                    case ThemeBetResolutionBetResolution.PerTheme:
                    {
                        var replyList = cachedGameInformationCategory.GetThemeMaxBetCredits(
                            new List<ThemeIdentifier> { themeIdentifier.ToThemeIdentifier() });

                        result = replyList.First(entry => entry.Theme.Value == themeIdentifier)
                            .MaxBetCredits;
                        break;
                    }

                    case ThemeBetResolutionBetResolution.PerPayvar:
                    {
                        var replyList = cachedGameInformationCategory.GetPayvarMaxBetCredits(
                            new List<PayvarIdentifier> { paytableIdentifier.ToPayvarIdentifier() });

                        result = replyList.First(entry => entry.Payvar.Value == paytableIdentifier)
                            .MaxBetCredits;
                        break;
                    }

                    case ThemeBetResolutionBetResolution.PerPayvarDenomination:
                    {
                        var replyList = cachedGameInformationCategory.GetDenominationMaxBetCredits(
                            new List<PayvarDenomination>
                            {
                                new PayvarDenomination
                                {
                                    Payvar = paytableIdentifier.ToPayvarIdentifier(),
                                    Denomination = (uint)denomination
                                }
                            });

                        result = replyList.First(entry => entry.PayvarDenomination.Payvar.Value == paytableIdentifier &&
                                                          entry.PayvarDenomination.Denomination == (uint)denomination)
                            .MaxBetCredits;
                        break;
                    }
                }
            }

            return (long)result;
        }

        /// <inheritdoc/>
        public long GetButtonPanelMinBet(string themeIdentifier, string paytableIdentifier, long denomination)
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            var replyEntries = cachedGameInformationCategory.GetBetResolution(
                                        new List<ThemeIdentifier> { themeIdentifier.ToThemeIdentifier() });

            ulong result = 0;

            var betResolution = replyEntries.First(entry => entry.Theme.Value == themeIdentifier)
                                            .BetResolution;

            switch(betResolution)
            {
                case ThemeBetResolutionBetResolution.PerTheme:
                    {
                        var replyList = cachedGameInformationCategory.GetThemeButtonPanelMinBet(
                                            new List<ThemeIdentifier> { themeIdentifier.ToThemeIdentifier() });

                        result = replyList.First(entry => entry.Theme.Value == themeIdentifier)
                                          .ButtonPanelMinBetCredits;
                        break;
                    }

                case ThemeBetResolutionBetResolution.PerPayvar:
                    {
                        var replyList = cachedGameInformationCategory.GetPayvarButtonPanelMinBet(
                                            new List<PayvarIdentifier> { paytableIdentifier.ToPayvarIdentifier() });

                        result = replyList.First(entry => entry.Payvar.Value == paytableIdentifier)
                                          .ButtonPanelMinBetCredits;
                        break;
                    }

                case ThemeBetResolutionBetResolution.PerPayvarDenomination:
                    {
                        var replyList = cachedGameInformationCategory.GetDenominationButtonPanelMinBet(
                                            new List<PayvarDenomination>
                                            {
                                                new PayvarDenomination
                                                    {
                                                        Payvar = paytableIdentifier.ToPayvarIdentifier(),
                                                        Denomination = (uint)denomination
                                                    }
                                            });

                        result = replyList.First(entry => entry.PayvarDenomination.Payvar.Value == paytableIdentifier &&
                                                          entry.PayvarDenomination.Denomination == (uint)denomination)
                                          .ButtonPanelMinBetCredits;
                        break;
                    }
            }

            return (long)result;
        }

        #endregion

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
            if(cachedGameInformationCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "GameInformation cannot be used without calling its Initialize method first.");
            }
        }

        /// <summary>
        /// Validates the passed in list is neither null nor empty,
        /// then converts it to a list of Theme Identifier type of data.
        /// </summary>
        /// <param name="themeIdentifiers">A list of identifier strings.</param>
        /// <returns>The validated and converted list of Theme Identifier type of data.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="themeIdentifiers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="themeIdentifiers"/> is empty.
        /// </exception>
        private static IList<ThemeIdentifier> CheckAndConvertThemeIdentifiers(IEnumerable<string> themeIdentifiers)
        {
            if(themeIdentifiers == null)
            {
                throw new ArgumentNullException("themeIdentifiers");
            }

            var result = themeIdentifiers.Select(identifier => identifier.ToThemeIdentifier()).ToList();
            if(result.Count == 0)
            {
                throw new ArgumentException("There must be at least one theme identifier specified.", "themeIdentifiers");
            }

            return result;
        }

        /// <summary>
        /// Validates the passed in list is neither null nor empty,
        /// then converts it to a list of Payvar Identifier type of data.
        /// </summary>
        /// <param name="paytableIdentifiers">A list of identifier strings.</param>
        /// <returns>The validated and converted list of Payvar Identifier type of data.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="paytableIdentifiers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="paytableIdentifiers"/> is empty.
        /// </exception>
        private static IList<PayvarIdentifier> CheckAndConvertPayvarIdentifiers(IEnumerable<string> paytableIdentifiers)
        {
            if(paytableIdentifiers == null)
            {
                throw new ArgumentNullException("paytableIdentifiers");
            }

            var result = paytableIdentifiers.Select(identifier => identifier.ToPayvarIdentifier()).ToList();
            if(result.Count == 0)
            {
                throw new ArgumentException("There must be at least one paytable identifier specified.", "paytableIdentifiers");
            }

            return result;
        }

        /// <summary>
        /// Gets the group paytable information by the given paytable.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme.</param>
        /// <param name="paytableIdentifier">The identifier of the paytable.</param>
        /// <returns>
        /// The group paytable information for the given paytable. Null if the paytable is not in a group.
        /// </returns>
        private GroupPayvarInformation GetGroupPaytableInformation(string themeIdentifier, string paytableIdentifier)
        {
            if(cachedGameGroupInformationCategory != null)
            {
                var paytableGroups =
                    cachedGameGroupInformationCategory.GetPayvarGroupsForTheme(
                        new List<ThemeIdentifier>
                        {
                            themeIdentifier.ToThemeIdentifier()
                        }).FirstOrDefault(theme => theme.Theme.Value == themeIdentifier);

                if(paytableGroups != null)
                {
                    var paytableInfo =
                        paytableGroups.PayvarGroups.SelectMany(paytableGroup => paytableGroup.PayvarInformations)
                            .FirstOrDefault(info => info.Payvar.Value == paytableIdentifier);

                    return paytableInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a <see cref="PaytableTag"/> data from a <see cref="PayvarInformation"/> data,
        /// based on whether the paytable is in a game group or not.
        /// </summary>
        /// <param name="payvarInfo">The payvar information used to create the paytable tag.</param>
        /// <param name="isInGroup">The flag indicating whether the paytable is in a game group.</param>
        /// <returns>The paytable tag created.</returns>
        private PaytableTag CreatePaytableTag(PayvarInformation payvarInfo, bool isInGroup)
        {
            return isInGroup
                       ? new PaytableTag(
                             payvarInfo.Payvar.Value,
                             Path.Combine(cachedMountPoint, payvarInfo.TagDataFile),
                             payvarInfo.Tag,
                             true,
                             Path.Combine(cachedMountPoint, payvarInfo.GroupTagDataFile),
                             payvarInfo.GroupTag)
                       : new PaytableTag(
                             payvarInfo.Payvar.Value,
                             Path.Combine(cachedMountPoint, payvarInfo.TagDataFile),
                             payvarInfo.Tag);
        }

        /// <summary>
        /// Gets the paytable tags for a list of themes.
        /// </summary>
        /// <param name="themeIdentifierList">The list of themes to query.</param>
        /// <param name="enabledOnly">The flag indicating if to query for all paytables, or just the enabled ones.</param>
        /// <returns>The collection of paytable tag lists, keyed by the theme identifiers passed in.</returns>
        private IDictionary<string, IList<PaytableTag>> GetPaytableTags(IList<ThemeIdentifier> themeIdentifierList,
                                                                        bool enabledOnly)
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            var themePayvarsList = cachedGameInformationCategory.GetPayvarsForTheme(themeIdentifierList);
            var themeGameGroupsList = cachedGameGroupInformationCategory == null
                                          ? null
                                          : cachedGameGroupInformationCategory.GetPayvarGroupsForTheme(themeIdentifierList).ToList();

            var result = new Dictionary<string, IList<PaytableTag>>();

            foreach(var themePayvars in themePayvarsList)
            {
                var theme = themePayvars.Theme.Value;
                var paytableTags = new List<PaytableTag>();

                var themeGameGroups = themeGameGroupsList == null
                                          ? null
                                          : themeGameGroupsList.FirstOrDefault(entry => entry.Theme.Value == theme);

                var payvarInfos = cachedGameInformationCategory.GetPayvarInformation(themePayvars.Payvar);
                if(enabledOnly)
                {
                    payvarInfos = payvarInfos.Where(info => info.Enabled);
                }

                foreach(var payvarInfo in payvarInfos)
                {
                    var isInGroup = themeGameGroups != null &&
                                    themeGameGroups.PayvarGroups.SelectMany(paytableGroup => paytableGroup.PayvarInformations)
                                                                .FirstOrDefault(info => info.Payvar.Value == payvarInfo.Payvar.Value) != null;

                    paytableTags.Add(CreatePaytableTag(payvarInfo, isInGroup));
                }

                result[theme] = paytableTags;
            }

            return result;
        }

        #endregion
    }
}
