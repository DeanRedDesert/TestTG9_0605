// -----------------------------------------------------------------------
// <copyright file = "GameInformation.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Communication.Standalone.Schemas;
    using Registries;

    /// <summary>
    /// Standalone implementation of <see cref="IGameInformation"/> that imitates the Foundation
    /// for game information.
    /// </summary>
    internal class GameInformation : IGameInformation
    {
        #region Private Fields

        /// <summary>
        /// Dictionary of the game's loaded registries.
        /// </summary>
        private readonly IDictionary<IThemeRegistry, IList<IPayvarRegistry>> registries;

        /// <summary>
        /// Dictionary of PaytableBindings that is keyed by [PaytableIdentifier][Denomination].
        /// </summary>
        private readonly IDictionary<string, IDictionary<long, PaytableBinding>> paytableBindings;

        /// <summary>
        /// The mount point of the report package.
        /// </summary>
        private readonly string mountPoint;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="GameInformation"/>.
        /// </summary>
        /// <param name="registries">
        /// The game's loaded registries.
        /// </param>
        /// <param name="paytableBindings">
        /// Mapping from paytable identifier to dictionary of denomination and paytable info/<see cref="PaytableBinding"/>.
        /// </param>
        /// <param name="mountPoint">
        /// The mount point of the report package.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        public GameInformation(IDictionary<IThemeRegistry, IList<IPayvarRegistry>> registries,
                               IDictionary<string, IDictionary<long, PaytableBinding>> paytableBindings,
                               string mountPoint)
        {
            this.registries = registries ?? throw new ArgumentNullException(nameof(registries));
            this.paytableBindings = paytableBindings ?? throw new ArgumentNullException(nameof(paytableBindings));
            this.mountPoint = mountPoint ?? throw new ArgumentNullException(nameof(mountPoint));
        }

        #endregion

        #region IGameInformation Implementation

        #region Information on Theme(s)

        /// <inheritdoc/>
        public IDictionary<string, IList<PaytableTag>> GetAllPaytableTagsForThemes(IEnumerable<string> themeIdentifiers)
        {
            // In standalone, only enabled paytables are viewed as available.
            return GetEnabledPaytableTagsForThemes(themeIdentifiers);
        }

        /// <inheritdoc/>
        public IDictionary<string, IList<PaytableTag>> GetEnabledPaytableTagsForThemes(
            IEnumerable<string> themeIdentifiers)
        {
            var requestedThemes = CheckAndConvertIdentifiers(themeIdentifiers);

            var result = requestedThemes.ToDictionary(key => key, value => new List<PaytableTag>() as IList<PaytableTag>);

            foreach(var entry in paytableBindings)
            {
                var paytableIdentifier = entry.Key;
                var paytableBinding = entry.Value.First().Value;

                if(requestedThemes.Contains(paytableBinding.ThemeIdentifier))
                {
                    result[paytableBinding.ThemeIdentifier].Add(CreatePaytableTag(paytableIdentifier, paytableBinding));
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public IDictionary<string, IList<PaytableDenominationInfo>> GetEnabledPaytableDenominationInfos(
            IEnumerable<string> themeIdentifiers)
        {
            var requestedThemes = CheckAndConvertIdentifiers(themeIdentifiers);

            var result = requestedThemes.ToDictionary(key => key, value => new List<PaytableDenominationInfo>());

            foreach(var entry in paytableBindings)
            {
                var paytableIdentifier = entry.Key;
                var themeIdentifier = entry.Value.First().Value.ThemeIdentifier;

                var denoms = entry.Value.Keys;

                if(requestedThemes.Contains(themeIdentifier))
                {
                    result[themeIdentifier].AddRange(
                        denoms.Select(denom => new PaytableDenominationInfo(paytableIdentifier, denom)));
                }
            }

            return result.ToDictionary(entry => entry.Key, entry => entry.Value as IList<PaytableDenominationInfo>);
        }

        /// <inheritdoc/>
        public IEnumerable<PaytableDenominationInfo> GetEnabledPaytableDenominationInfo(string themeIdentifier)
        {
            var paytableDenomList = new List<PaytableDenominationInfo>();
            foreach(var item in paytableBindings)
            {
                paytableDenomList.AddRange(from subItem in item.Value
                                           where subItem.Value.ThemeIdentifier == themeIdentifier
                                           select new PaytableDenominationInfo(item.Key, subItem.Key));
            }
            return paytableDenomList;
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
                throw new ArgumentNullException(nameof(themeIdentifier));
            }

            string paytableIdentifier = null;
            foreach(var keyvalue in paytableBindings)
            {
                foreach(var binding in keyvalue.Value)
                {
                    if(binding.Value.ThemeIdentifier == themeIdentifier)
                    {
                        paytableIdentifier = binding.Value.PaytableIdentifier;
                        break;
                    }
                }

                if(!string.IsNullOrEmpty(paytableIdentifier))
                {
                    break;
                }
            }

            IThemeRegistry themeRegistry = null;
            if(!string.IsNullOrEmpty(paytableIdentifier))
            {
                foreach(var themeReg in registries)
                {
                    foreach(var payvarreg in themeReg.Value)
                    {
                        if(payvarreg.PaytableIdentifier == paytableIdentifier)
                        {
                            themeRegistry = themeReg.Key;
                            break;
                        }
                    }

                    if(themeRegistry != null)
                    {
                        break;
                    }
                }
            }

            if(themeRegistry == null)
            {
                throw new ArgumentException("Theme with the provided id does not exist: " + themeIdentifier);
            }

            return new KeyValuePair<ThemeTag, bool>(new ThemeTag(themeIdentifier, themeRegistry.G2SThemeId, "", ""),
                true);
        }

        #endregion

        #region Information on Paytable(s)

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="paytableIdentifier"/> is not found in paytableBingdings.
        /// </exception>
        public PaytableTag GetPaytableTag(string themeIdentifier, string paytableIdentifier)
        {
            ValidatePaytableIdentifier(paytableIdentifier);

            var denomDictionary = paytableBindings[paytableIdentifier];
            var paytableBinding = denomDictionary.First().Value;

            return CreatePaytableTag(paytableIdentifier, paytableBinding);
        }

        /// <inheritdoc/>
        public IEnumerable<PaytablePaybackInfo> GetPaybackInfos(IEnumerable<string> paytableIdentifiers)
        {
            var paytableIdentifierList = CheckAndConvertIdentifiers(paytableIdentifiers);

            return paytableIdentifierList.Select(identifier => GetPaybackInfo(identifier));
        }

        /// <inheritdoc/>
        public PaytablePaybackInfo GetPaybackInfo(string paytableIdentifier)
        {
            ValidatePaytableIdentifier(paytableIdentifier);

            var paytableBinding = paytableBindings[paytableIdentifier].First().Value;

            var groupPayvar = GetGroupPaytableInformation(paytableBinding)
                                    .FirstOrDefault(payvar => payvar.PaytableIdentifier == paytableIdentifier);

            return groupPayvar != null
                       ? groupPayvar.PaybackInfo
                       : GetPayvarRegistry(paytableBinding).PaybackInfo;
        }

        /// <inheritdoc/>
        public IList<PaytableDenominationInfo> GetEnabledDenominationInfos(IEnumerable<string> paytableIdentifiers)
        {
            var paytableIdentifierList = CheckAndConvertIdentifiers(paytableIdentifiers);

            return paytableIdentifierList
                        .SelectMany(identifier => GetEnabledDenominations(identifier)
                                                      .Select(denom => new PaytableDenominationInfo(identifier, denom)))
                        .ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<long> GetEnabledDenominations(string paytableIdentifier)
        {
            ValidatePaytableIdentifier(paytableIdentifier);

            return paytableBindings.ContainsKey(paytableIdentifier)
                       ? paytableBindings[paytableIdentifier].Keys
                       : new List<long>();
        }

        /// <inheritdoc/>
        public IEnumerable<long> GetSupportedDenominations(string paytableIdentifier)
        {
            ValidatePaytableIdentifier(paytableIdentifier);

            var themeIdentifier = paytableBindings[paytableIdentifier].First().Value.ThemeIdentifier;
            foreach(var registry in registries)
            {
                if(registry.Key.G2SThemeId == themeIdentifier)
                {
                    var lpayvar = registry.Value;
                    foreach(var item in lpayvar)
                    {
                        if(item.PaytableIdentifier == paytableIdentifier)
                        {
                            return item.GetSupportedDenominations();
                        }
                    }
                }
            }
            return new List<long>();
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">
        /// Thrown when paytable binding for <paramref name="paytableIdentifier"/> and <paramref name="denomination"/> is not found.
        /// </exception>
        public long GetMaxBet(string themeIdentifier, string paytableIdentifier, long denomination)
        {
            ValidatePaytableIdentifier(paytableIdentifier);

            long maxBet = 0;

            var paytableBinding = paytableBindings[paytableIdentifier][denomination];
            if(paytableBinding == null)
            {
                throw new ArgumentException(
                    $"Paytable {paytableIdentifier} and denomination {denomination} are not found in the paytableBindings.",
                    nameof(paytableIdentifier));
            }
            if(paytableBinding.MaxBetSpecified)
            {
                maxBet = (long)paytableBinding.MaxBet;

                var groupPayvar = GetGroupPaytableInformation(paytableBinding)
                                        .FirstOrDefault(payvar => payvar.PaytableIdentifier == paytableIdentifier);
                if(groupPayvar?.MaxBetRedefinition.ContainsKey(maxBet) == true)
                {
                    maxBet = groupPayvar.MaxBetRedefinition[maxBet];
                }
            }

            return maxBet;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">
        /// Thrown when paytable binding for <paramref name="paytableIdentifier"/> and <paramref name="denomination"/> is not found.
        /// </exception>
        public long GetButtonPanelMinBet(string themeIdentifier, string paytableIdentifier, long denomination)
        {
            ValidatePaytableIdentifier(paytableIdentifier);

            ulong buttonPanelMinBet = 0;

            var paytable = paytableBindings[paytableIdentifier][denomination];
            if(paytable == null)
            {
                throw new ArgumentException(
                    $"Paytable {paytableIdentifier} and denomination {denomination} are not found in the paytableBindings.",
                    nameof(paytableIdentifier));
            }

            if(paytable.ButtonPanelMinBetSpecified)
            {
                buttonPanelMinBet = paytable.ButtonPanelMinBet;
            }

            return (long)buttonPanelMinBet;
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the passed in list is neither null nor empty, then converts it to a list
        /// to avoid possible multiple enumeration of IEnumerable.
        /// </summary>
        /// <param name="identifiers">A list of identifier strings to validate.</param>
        /// <returns>The validated and converted list.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="identifiers"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="identifiers"/> is empty.
        /// </exception>
        private static IList<string> CheckAndConvertIdentifiers(IEnumerable<string> identifiers)
        {
            if(identifiers == null)
            {
                throw new ArgumentNullException(nameof(identifiers));
            }

            var result = identifiers.ToList();
            if(result.Count == 0)
            {
                throw new ArgumentException("There must be at least one identifier specified.", nameof(identifiers));
            }

            return result;
        }

        /// <summary>
        /// Validates if a given paytable identifier can be found.
        /// </summary>
        /// <param name="paytableIdentifier">The paytable identifier to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="paytableIdentifier"/> is not found.
        /// </exception>
        private void ValidatePaytableIdentifier(string paytableIdentifier)
        {
            if(!paytableBindings.ContainsKey(paytableIdentifier))
            {
                throw new ArgumentException(
                    $"Paytable identifier {paytableIdentifier} is not found in the paytableBindings.",
                    nameof(paytableIdentifier));
            }
        }

        /// <summary>
        /// Gets the payvar registry for the specified paytable binding.
        /// </summary>
        /// <param name="paytableBinding">
        /// <see cref="PaytableBinding"/> object specifying a specific paytable instance.
        /// </param>
        /// <returns>
        /// The payvar registry for the paytable binding..
        /// </returns>
        private IPayvarRegistry GetPayvarRegistry(PaytableBinding paytableBinding)
        {
            return registries.First(registrySet => registrySet.Key.G2SThemeId == paytableBinding.ThemeIdentifier)
                             .Value
                             .First(payvar => payvar.PaytableTagName == paytableBinding.PaytableName &&
                                              payvar.PaytableTagFileName == paytableBinding.PaytableFileName);
        }

        /// <summary>
        /// Gets a list of <see cref="Payvar"/> objects for the specified paytable binding if it refers
        /// to a game group.
        /// </summary>
        /// <param name="paytableBinding">
        /// <see cref="PaytableBinding"/> object specifying a specific paytable instance.
        /// </param>
        /// <returns>
        /// A list of <see cref="Payvar"/> objects if the paytable binding refers to
        /// a game group; otherwise, an empty list.
        /// </returns>
        private IList<Payvar> GetGroupPaytableInformation(PaytableBinding paytableBinding)
        {
            var payvarRegistry = GetPayvarRegistry(paytableBinding);

            return payvarRegistry.PayvarGroupRegistry != null
                       ? payvarRegistry.PayvarGroupRegistry.Payvars
                       : new List<Payvar>();
        }

        /// <summary>
        /// Creates a <see cref="PaytableTag"/> data.
        /// </summary>
        private PaytableTag CreatePaytableTag(string paytableIdentifier, PaytableBinding paytableBinding)
        {
            var groupPayvar = GetGroupPaytableInformation(paytableBinding)
                                  .FirstOrDefault(payvar => payvar.PaytableIdentifier == paytableIdentifier);

            return groupPayvar != null
                       ? new PaytableTag(paytableIdentifier,
                                         Path.Combine(mountPoint, paytableBinding.PaytableFileName),
                                         paytableBinding.PaytableName,
                                         true,
                                         Path.Combine(mountPoint, groupPayvar.GroupPayvarTagData),
                                         groupPayvar.GroupPayvarTag)
                       : new PaytableTag(paytableIdentifier,
                                         Path.Combine(mountPoint, paytableBinding.PaytableFileName),
                                         paytableBinding.PaytableName);
        }

        #endregion
    }
}