//-----------------------------------------------------------------------
// <copyright file = "TypeConverters.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2X.Schemas.Internal.CustomConfigurationRead;
    using F2X.Schemas.Internal.GameInformation;
    using F2X.Schemas.Internal.ParcelComm;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;
    using Money;
    using ConfigurationItemType = Ascent.Communication.Platform.Interfaces.ConfigurationItemType;
    using CurrencySymbolPosition = Money.CurrencySymbolPosition;
    using F2XConfigurationItemType = F2X.Schemas.Internal.CustomConfigurationRead.ConfigurationItemType;
    using F2XCurrencySymbolPosition = F2X.Schemas.Internal.Localization.CurrencySymbolPosition;
    using F2XParcelCommEntity = F2X.Schemas.Internal.ParcelComm.ParcelCommEntity;
    using F2XProgressiveSettings = F2X.Schemas.Internal.ProgressiveData.ProgressiveSettings;
    using NegativeAmountDisplayFormat = F2X.Schemas.Internal.Localization.CreditFormatterInfoNegativeAmountDisplayFormat;
    using ProgressiveSettings = Ascent.Communication.Platform.Interfaces.ProgressiveSettings;
    using Version = F2X.Schemas.Internal.Types.Version;

    /// <summary>
    /// Collection of extension methods helping convert between
    /// interface types and F2X schema types.
    /// </summary>
    public static class TypeConverters
    {
        #region Fields

        /// <summary>
        /// Mappings from F2X <see cref="F2XCurrencySymbolPosition"/> to <see cref="CreditFormatter"/>.
        /// </summary>
        private static readonly Dictionary<F2XCurrencySymbolPosition, CurrencySymbolPosition> MapToPublicCurrencySymbolPosition =
            new Dictionary<F2XCurrencySymbolPosition, CurrencySymbolPosition>
                {
                    { F2XCurrencySymbolPosition.Left, CurrencySymbolPosition.Left },
                    { F2XCurrencySymbolPosition.Right, CurrencySymbolPosition.Right },
                    { F2XCurrencySymbolPosition.LeftWithSpace, CurrencySymbolPosition.LeftWithSpace },
                    { F2XCurrencySymbolPosition.RightWithSpace, CurrencySymbolPosition.RightWithSpace },
                };

        /// <summary>
        /// Mappings from F2X <see cref="F2XCurrencySymbolPosition"/> to <see cref="NegativeNumberFormat"/>.
        /// </summary>
        private static readonly Dictionary<NegativeAmountDisplayFormat, NegativeNumberFormat> MapToNegativeSignPosition =
            new Dictionary<NegativeAmountDisplayFormat, NegativeNumberFormat>
                {
                    { NegativeAmountDisplayFormat.NegativeSignIsFirstSymbol, NegativeNumberFormat.IsFirstSymbol },
                    { NegativeAmountDisplayFormat.NegativeSignBeforeNumerics, NegativeNumberFormat.BeforeNumerics },
                    { NegativeAmountDisplayFormat.NegativeSignAfterNumerics, NegativeNumberFormat.AfterNumerics },
                    { NegativeAmountDisplayFormat.NegativesShownInParentheses, NegativeNumberFormat.InParentheses },
                };

        /// <summary>
        /// Mappings from <see cref="ConfigurationItemType"/> to F2X <see cref="F2XConfigurationItemType"/>.
        /// </summary>
        private static readonly Dictionary<ConfigurationItemType, F2XConfigurationItemType> MapToInternalConfigurationItemType =
            new Dictionary<ConfigurationItemType, F2XConfigurationItemType>
                {
                    { ConfigurationItemType.Invalid, F2XConfigurationItemType.Invalid },
                    { ConfigurationItemType.Amount, F2XConfigurationItemType.Amount },
                    { ConfigurationItemType.Boolean, F2XConfigurationItemType.Boolean },
                    { ConfigurationItemType.EnumerationList, F2XConfigurationItemType.EnumerationList },
                    { ConfigurationItemType.FlagList, F2XConfigurationItemType.FlagList },
                    { ConfigurationItemType.Float, F2XConfigurationItemType.Float },
                    { ConfigurationItemType.Int64, F2XConfigurationItemType.Int64 },
                    { ConfigurationItemType.Item, F2XConfigurationItemType.Item },
                    { ConfigurationItemType.String, F2XConfigurationItemType.String },
                };

        /// <summary>
        /// Mappings from F2X <see cref="F2XConfigurationItemType"/> to <see cref="CurrencySymbolPosition"/>.
        /// </summary>
        private static Dictionary<F2XConfigurationItemType, ConfigurationItemType> mapToPublicConfigurationItemType;

        #endregion

        #region Public Methods

        /// <summary>
        /// Extension method to convert an F2X <see cref="CategoryVersion"/>
        /// to a <see cref="CategoryVersionInformation"/>.
        /// </summary>
        /// <param name="version">The category version to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CategoryVersionInformation ToPublic(this CategoryVersion version)
        {
            return new CategoryVersionInformation(version.Category,
                                                  version.Version.MajorVersion,
                                                  version.Version.MinorVersion);
        }

        /// <summary>
        /// Extension method to convert a <see cref="CategoryVersionInformation"/>
        /// to an F2X <see cref="CategoryVersion"/>.
        /// </summary>
        /// <param name="version">The category version to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CategoryVersion ToInternal(this CategoryVersionInformation version)
        {
            return new CategoryVersion
                       {
                           Category = version.Category,
                           Version = new Version
                                         {
                                             MajorVersion = version.MajorVersion,
                                             MinorVersion = version.MinorVersion
                                         }
                       };
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="F2XCurrencySymbolPosition"/> to
        /// a <see cref="CurrencySymbolPosition"/>.
        /// </summary>
        /// <param name="symbolPosition">The currency symbol position to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CurrencySymbolPosition ToPublic(this F2XCurrencySymbolPosition symbolPosition)
        {
            return MapToPublicCurrencySymbolPosition[symbolPosition];
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="NegativeAmountDisplayFormat"/> to
        /// a <see cref="NegativeNumberFormat"/>.
        /// </summary>
        /// <param name="displayFormat">The negative amount display format to convert.</param>
        /// <returns>The conversion result.</returns>
        public static NegativeNumberFormat ToNegativeSignPosition(this NegativeAmountDisplayFormat displayFormat)
        {
            return MapToNegativeSignPosition[displayFormat];
        }

        /// <summary>
        /// Extension method to convert a <see cref="ConfigurationItemType"/> to
        /// a F2X <see cref="F2XConfigurationItemType"/>.
        /// </summary>
        /// <param name="configType">The config type to convert.</param>
        /// <returns>The conversion result.</returns>
        public static F2XConfigurationItemType ToInternal(this ConfigurationItemType configType)
        {
            return MapToInternalConfigurationItemType[configType];
        }

        /// <summary>
        /// Extension method to convert a a F2X <see cref="F2XConfigurationItemType"/> to
        /// a <see cref="ConfigurationItemType"/>.
        /// </summary>
        /// <param name="configType">The config type to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ConfigurationItemType ToPublic(this F2XConfigurationItemType configType)
        {
            if(mapToPublicConfigurationItemType == null)
            {
                mapToPublicConfigurationItemType = MapToInternalConfigurationItemType.ToDictionary(pair => pair.Value,
                                                                                                   pair => pair.Key);
            }

            return mapToPublicConfigurationItemType[configType];
        }

        /// <summary>
        /// Extension method to convert a <see cref="ThemeIdentifier"/> to a
        /// <see cref="CustomConfigurationItemScopeSelector"/>.
        /// </summary>
        /// <param name="identifier">The identifier to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CustomConfigurationItemScopeSelector ToConfigurationItemScopeSelector(
            this ThemeIdentifier identifier)
        {
            return new CustomConfigurationItemScopeSelector { Item = identifier };
        }

        /// <summary>
        /// Extension method to convert a <see cref="PayvarIdentifier"/> to a
        /// <see cref="CustomConfigurationItemScopeSelector"/>.
        /// </summary>
        /// <param name="identifier">The identifier to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CustomConfigurationItemScopeSelector ToConfigurationItemScopeSelector(
            this PayvarIdentifier identifier)
        {
            return new CustomConfigurationItemScopeSelector { Item = identifier };
        }

        /// <summary>
        /// Extension method to convert an <see cref="ExtensionIdentifier"/> to a
        /// <see cref="CustomConfigurationItemScopeSelector"/>.
        /// </summary>
        /// <param name="identifier">The identifier to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CustomConfigurationItemScopeSelector ToConfigurationItemScopeSelector(
            this ExtensionIdentifier identifier)
        {
            return new CustomConfigurationItemScopeSelector { Item = identifier };
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="F2XProgressiveSettings"/> to
        /// a <see cref="ProgressiveSettings"/>.
        /// </summary>
        /// <param name="settings">The progressive settings to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ProgressiveSettings ToPublic(this F2XProgressiveSettings settings)
        {
            return settings == null
                       ? null
                       : new ProgressiveSettings
                             {
                                 StartAmount = settings.StartAmount != null ? settings.StartAmount.Value : 0,
                                 MaxAmount = settings.MaximumAmount != null ? settings.MaximumAmount.Value : 0,
                                 ContributionPercentage = ConvertToDecimal(settings.ContributionPercentage)
                             };
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="CustomConfigurationItemSelector"/> to
        /// a <see cref="ConfigurationItemKey"/>.
        /// </summary>
        /// <param name="selector">The configuration key to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ConfigurationItemKey ToConfigurationItemKey(this CustomConfigurationItemSelector selector)
        {
            if(selector == null)
            {
                return new ConfigurationItemKey();
            }

            ConfigurationScope configScope;
            string scopeIdentifier;

            GetScopeAndIdentifier(selector, out configScope, out scopeIdentifier);

            return new ConfigurationItemKey(configScope, scopeIdentifier, selector.Name);
        }

        /// <summary>
        /// Extension method to convert a <see cref="ConfigurationItemKey"/> to
        /// a F2X <see cref="CustomConfigurationItemSelector"/>.
        /// </summary>
        /// <param name="configKey">The configuration key to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CustomConfigurationItemSelector ToConfigurationItemSelector(this ConfigurationItemKey configKey)
        {
            return new CustomConfigurationItemSelector
                       {
                           Item = configKey.GetTypedScopeIdentifier(),
                           Name = configKey.ConfigName
                       };
        }

        /// <summary>
        /// Extension method to retrieve a typed scope identifier from
        /// a <see cref="ConfigurationItemKey"/>.
        /// </summary>
        /// <param name="configKey">The configuration key to convert.</param>
        /// <returns>
        /// The typed scope identifier, either a <see cref="PayvarIdentifier"/>
        /// or a <see cref="ThemeIdentifier"/>.
        /// </returns>
        public static object GetTypedScopeIdentifier(this ConfigurationItemKey configKey)
        {
            object typedIdentifier;

            switch(configKey.ConfigScope)
            {
                case ConfigurationScope.Payvar:
                    typedIdentifier = configKey.ScopeIdentifier.ToPayvarIdentifier();
                    break;

                case ConfigurationScope.Theme:
                    typedIdentifier = configKey.ScopeIdentifier.ToThemeIdentifier();
                    break;

                case ConfigurationScope.Extension:
                    typedIdentifier = configKey.ScopeIdentifier.ToExtensionIdentifier();
                    break;

                default:

                    // This should never happen.
                    throw new ArgumentException(
                        "Cannot get typed scope identifier from the configuration item key.");
            }

            return typedIdentifier;

        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="PayvarPaybackPercentageData"/> to an <see cref="PaytablePaybackInfo"/>.
        /// </summary>
        /// <param name="percentageData">The percentage data to convert.</param>
        /// <returns>
        /// The conversion result, or null if <paramref name="percentageData"/> is null.
        /// </returns>
        public static PaytablePaybackInfo ToPaytablePaybackInfo(this PayvarPaybackPercentageData percentageData)
        {
            return percentageData == null
                       ? null
                       : new PaytablePaybackInfo(percentageData.Payvar.Value,
                                                 ConvertToDecimal(percentageData.PaybackPercentage),
                                                 ConvertToDecimal(percentageData.MinimumPaybackPercentage),
                                                 ConvertToDecimal(percentageData.MinimumPaybackPercentageWithoutProgressives));
        }

        /// <summary>
        /// Extension method to convert a <see cref="ParcelCommEndpoint"/> to
        /// a F2X <see cref="F2XParcelCommEntity"/>.
        /// </summary>
        /// <param name="entity">The parcel comm entity to convert.</param>
        /// <returns>The conversion result.</returns>
        public static F2XParcelCommEntity ToInternal(this ParcelCommEndpoint entity)
        {
            F2XParcelCommEntity result = null;
            if(entity != null)
            {
                result = new F2XParcelCommEntity();
                switch(entity.EntityType)
                {
                    case EndpointType.Theme:
                    {
                        result.Item = new ThemeIdentifier { Value = entity.EntityIdentifier };
                        break;
                    }

                    case EndpointType.Shell:
                    {
                        result.Item = new ShellIdentifier { Value = entity.EntityIdentifier };
                        break;
                    }

                    case EndpointType.Extension:
                    {
                        result.Item = new ExtensionIdentifier { Value = entity.EntityIdentifier };
                        break;
                    }

                    case EndpointType.Ptc:
                    {
                        result.Item = new PtcEndpointIdentifier { Value = entity.EntityIdentifier };
                        break;
                    }

                    case EndpointType.CommonThemeControl:
                    {
                        result.Item = new CommonThemeControlSelector();
                        break;
                    }

                    case EndpointType.Chooser:
                    {
                        result.Item = new ChooserSelector();
                        break;
                    }

                    default:

                        // This should never happen.
                        throw new ArgumentException("Invalid entity type used. Endpoint identifier can't be created.");
                }
            }

            return result;
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="F2XParcelCommEntity"/> to
        /// a <see cref="ParcelCommEndpoint"/>.
        /// </summary>
        /// <param name="f2XEntity">The F2X parcel comm entity to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ParcelCommEndpoint ToPublic(this F2XParcelCommEntity f2XEntity)
        {
            ParcelCommEndpoint result = null;
            if(f2XEntity != null)
            {
                var themeIdentifier = f2XEntity.Item as ThemeIdentifier;
                if(themeIdentifier != null)
                {
                    result = new ParcelCommEndpoint(EndpointType.Theme, themeIdentifier.Value);
                }
                else
                {
                    var shellIdentifier = f2XEntity.Item as ShellIdentifier;
                    if(shellIdentifier != null)
                    {
                        result = new ParcelCommEndpoint(EndpointType.Shell, shellIdentifier.Value);
                    }
                    else
                    {
                        var extensionIdentifier = f2XEntity.Item as ExtensionIdentifier;
                        if(extensionIdentifier != null)
                        {
                            result = new ParcelCommEndpoint(EndpointType.Extension, extensionIdentifier.Value);
                        }
                        else
                        {
                            var ptcIdentifier = f2XEntity.Item as PtcEndpointIdentifier;
                            if(ptcIdentifier != null)
                            {
                                result = new ParcelCommEndpoint(EndpointType.Ptc, ptcIdentifier.Value);
                            }
                            else
                            {
                                var commonThemeControlSelector = f2XEntity.Item as CommonThemeControlSelector;
                                if(commonThemeControlSelector != null)
                                {
                                    result = new ParcelCommEndpoint(EndpointType.CommonThemeControl, null);
                                }
                                else
                                {
                                    var chooserSelector = f2XEntity.Item as ChooserSelector;
                                    if(chooserSelector != null)
                                    {
                                        result = new ParcelCommEndpoint(EndpointType.Chooser, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Converts an object of type <see cref="ThemeInformation"/>
        /// to <see cref="ThemeTag"/>.
        /// </summary>
        /// <param name="themeInformation">
        /// Reference to the <see cref="ThemeInformation"/> object.
        /// </param>
        /// <returns>An object of type <see cref="ThemeTag"/>.</returns>
        public static ThemeTag ToPublic(this ThemeInformation themeInformation)
        {
            ThemeTag themeTag = null;
            if(themeInformation != null)
            {
                themeTag = new ThemeTag(themeInformation.Theme.Value, themeInformation.G2SThemeIdentifier,
                                        themeInformation.Tag, themeInformation.TagDataFile);
            }

            return themeTag;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts a <see cref="CustomConfigurationItemScopeSelector"/> to
        /// a <see cref="ConfigurationScope"/> and a scope identifier.
        /// </summary>
        /// <param name="scopeSelector">The key to convert.</param>
        /// <param name="configScope">Returns the configuration scope.</param>
        /// <param name="scopeIdentifier">Returns the scope identifier.</param>
        /// <exception cref="InvalidCastException">
        /// Thrown when the conversion fails.
        /// </exception>
        private static void GetScopeAndIdentifier(CustomConfigurationItemScopeSelector scopeSelector,
                                                  out ConfigurationScope configScope, out string scopeIdentifier)
        {
            var payvarIdentifier = scopeSelector.Item as PayvarIdentifier;
            if(payvarIdentifier != null)
            {
                configScope = ConfigurationScope.Payvar;
                scopeIdentifier = payvarIdentifier.Value;
            }
            else
            {
                var themeIdentifier = scopeSelector.Item as ThemeIdentifier;
                if(themeIdentifier != null)
                {
                    configScope = ConfigurationScope.Theme;
                    scopeIdentifier = themeIdentifier.Value;
                }
                else
                {
                    var extensionIdentifier = scopeSelector.Item as ExtensionIdentifier;
                    if(extensionIdentifier != null)
                    {
                        configScope = ConfigurationScope.Extension;
                        scopeIdentifier = extensionIdentifier.Value;
                    }
                    else
                    {
                        // This should never happen.
                        throw new InvalidCastException(
                            "Cannot get configuration scope and identifier from the scope selector.");
                    }
                }
            }
        }

        /// <summary>
        /// Convert a string to a decimal type of percentageData.
        /// </summary>
        /// <param name="numberString">The string to convert.</param>
        /// <returns>
        /// The conversion result.  0 if <paramref name="numberString"/> is null, empty or invalid number.
        /// </returns>
        private static decimal ConvertToDecimal(string numberString)
        {
            decimal result;

            result = decimal.TryParse(numberString, out result)
                         ? result
                         : 0;

            return result;
        }
    }

    #endregion
}
