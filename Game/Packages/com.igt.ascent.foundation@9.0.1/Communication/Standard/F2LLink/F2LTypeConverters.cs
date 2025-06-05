//-----------------------------------------------------------------------
// <copyright file = "F2LTypeConverters.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2LLink
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using CustomConfigItemScope = F2L.Schemas.Internal.CustomConfigItemScope;
    using CustomConfigItemType = F2L.Schemas.Internal.CustomConfigItemType;
    using F2LCriticalDataScope = F2L.Schemas.Internal.CriticalDataScope;
    using F2LGameCycleState = F2L.Schemas.Internal.GameCycleState;
    using F2LPlayerMeters = F2L.Schemas.Internal.PlayerMeters;

    /// <summary>
    /// Collection of extension methods helping convert between
    /// interface types and F2L schema types.
    /// </summary>
    internal static class F2LTypeConverters
    {
        /// <summary>
        /// Mappings from F2L <see cref="F2LGameCycleState"/> to <see cref="GameCycleState"/>.
        /// </summary>
        /// <remarks>
        /// We cannot do direct cast as the enum values are not one-to-one mapping.
        /// </remarks>
        private static readonly Dictionary<F2LGameCycleState, GameCycleState> MapToPublicGameCycleState =
            new Dictionary<F2LGameCycleState, GameCycleState>
                {
                    { F2LGameCycleState.Invalid, GameCycleState.Invalid },
                    { F2LGameCycleState.Idle, GameCycleState.Idle },
                    { F2LGameCycleState.Committed, GameCycleState.Committed },
                    { F2LGameCycleState.EnrollPending, GameCycleState.EnrollPending },
                    { F2LGameCycleState.EnrollComplete, GameCycleState.EnrollComplete },
                    { F2LGameCycleState.Playing, GameCycleState.Playing },
                    { F2LGameCycleState.EvaluatePending, GameCycleState.EvaluatePending },
                    { F2LGameCycleState.MainPlayComplete, GameCycleState.MainPlayComplete },
                    { F2LGameCycleState.AncillaryPlaying, GameCycleState.AncillaryPlaying },
                    { F2LGameCycleState.AncillaryEvaluatePending, GameCycleState.AncillaryEvaluatePending },
                    { F2LGameCycleState.AncillaryPlayComplete, GameCycleState.AncillaryPlayComplete },
                    { F2LGameCycleState.BonusPlaying, GameCycleState.BonusPlaying },
                    { F2LGameCycleState.BonusEvaluatePending, GameCycleState.BonusEvaluatePending },
                    { F2LGameCycleState.BonusPlayComplete, GameCycleState.BonusPlayComplete },
                    { F2LGameCycleState.FinalizeAwardPending, GameCycleState.FinalizeAwardPending },
                    { F2LGameCycleState.Finalized, GameCycleState.Finalized },
                };

        /// <summary>
        /// Mappings from <see cref="ConfigurationScope"/> to F2L <see cref="CustomConfigItemScope"/>.
        /// </summary>
        /// <remarks>
        /// We cannot do direct cast as the enum values are not one-to-one mapping.
        /// </remarks>
        private static readonly Dictionary<ConfigurationScope, CustomConfigItemScope> MapToCustomConfigItemScope =
            new Dictionary<ConfigurationScope, CustomConfigItemScope>
                {
                    { ConfigurationScope.Payvar, CustomConfigItemScope.Payvar },
                    { ConfigurationScope.Theme, CustomConfigItemScope.Theme },
                };

        /// <summary>
        /// Mappings from F2L <see cref="CustomConfigItemType"/> to <see cref="ConfigurationItemType"/>.
        /// </summary>
        /// <remarks>
        /// We cannot do direct cast as the enum values are not one-to-one mapping.
        /// </remarks>
        private static readonly Dictionary<CustomConfigItemType, ConfigurationItemType> MapToConfigurationItemType =
            new Dictionary<CustomConfigItemType, ConfigurationItemType>
                {
                    { CustomConfigItemType.Invalid, ConfigurationItemType.Invalid },
                    { CustomConfigItemType.Amount, ConfigurationItemType.Amount },
                    { CustomConfigItemType.Boolean, ConfigurationItemType.Boolean },
                    { CustomConfigItemType.Enumeration, ConfigurationItemType.EnumerationList },
                    { CustomConfigItemType.FlagList, ConfigurationItemType.FlagList },
                    { CustomConfigItemType.Float, ConfigurationItemType.Float },
                    { CustomConfigItemType.Int64, ConfigurationItemType.Int64 },
                    { CustomConfigItemType.Item, ConfigurationItemType.Item },
                    { CustomConfigItemType.String, ConfigurationItemType.String },
                };

        /// <summary>
        /// Mappings from <see cref="CriticalDataScope"/> to F2L <see cref="F2LCriticalDataScope"/>.
        /// </summary>
        /// <remarks>
        /// We cannot do direct cast as the enum values are not one-to-one mapping.
        /// </remarks>
        private static readonly Dictionary<CriticalDataScope, F2LCriticalDataScope> MapToF2LCriticalDataScope =
            new Dictionary<CriticalDataScope, F2LCriticalDataScope>
                {
                    { CriticalDataScope.Feature, F2LCriticalDataScope.Feature },
                    { CriticalDataScope.GameCycle, F2LCriticalDataScope.GameCycle },
                    { CriticalDataScope.History, F2LCriticalDataScope.History },
                    { CriticalDataScope.Payvar, F2LCriticalDataScope.Payvar },
                    { CriticalDataScope.PayvarAnalytics, F2LCriticalDataScope.PayvarAnalytics },
                    { CriticalDataScope.PayvarPersistent, F2LCriticalDataScope.PayvarPersistent },
                    { CriticalDataScope.Theme, F2LCriticalDataScope.Theme },
                    { CriticalDataScope.ThemeAnalytics, F2LCriticalDataScope.ThemeAnalytics },
                    { CriticalDataScope.ThemePersistent, F2LCriticalDataScope.ThemePersistent }
                };

        /// <summary>
        /// Extension method to convert a F2L <see cref="F2LGameCycleState"/> to
        /// a <see cref="GameCycleState"/>.
        /// </summary>
        /// <param name="f2LGameCycleState">The F2L game cycle state to convert.</param>
        /// <returns>The conversion result.</returns>
        public static GameCycleState ToPublic(this F2LGameCycleState f2LGameCycleState)
        {
            if(!MapToPublicGameCycleState.ContainsKey(f2LGameCycleState))
            {
                throw new ArgumentException(
                    string.Format("F2L GameCycleState'{0}' cannot be converted to the public GameCycleState type.", f2LGameCycleState),
                    "f2LGameCycleState");
            }

            return MapToPublicGameCycleState[f2LGameCycleState];
        }

        /// <summary>
        /// Extension method to convert a <see cref="ConfigurationScope"/> to
        /// a F2L <see cref="CustomConfigItemScope"/>.
        /// </summary>
        /// <param name="configScope">The config type to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentException">
        /// Throw when the configuration scope cannot be converted to the target type.
        /// </exception>
        public static CustomConfigItemScope ToCustomConfigItemScope(this ConfigurationScope configScope)
        {
            if(!MapToCustomConfigItemScope.ContainsKey(configScope))
            {
                throw new ArgumentException(
                    string.Format("ConfigurationScope '{0}' cannot be converted to F2L CustomConfigItemScope type.", configScope),
                    "configScope");
            }

            return MapToCustomConfigItemScope[configScope];
        }

        /// <summary>
        /// Extension method to convert a F2L <see cref="CustomConfigItemType"/> to
        /// a <see cref="ConfigurationItemType"/>.
        /// </summary>
        /// <param name="configType">The config type to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ConfigurationItemType ToConfigurationItemType(this CustomConfigItemType configType)
        {
            if(!MapToConfigurationItemType.ContainsKey(configType))
            {
                throw new ArgumentException(
                    string.Format("F2L CustomConfigItemType '{0}' cannot be converted to the public ConfigurationItemType type.", configType),
                    "configType");
            }

            return MapToConfigurationItemType[configType];
        }

        /// <summary>
        /// Extension method to convert a <see cref="CriticalDataScope"/> to a F2L <see cref="F2LCriticalDataScope"/>.
        /// </summary>
        /// <param name="criticalDataScope">The critical data scope to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="criticalDataScope"/> cannot be converted to F2L critical data scope.
        /// </exception>>
        public static F2LCriticalDataScope ToF2LCriticalDataScope(this CriticalDataScope criticalDataScope)
        {
            if(!MapToF2LCriticalDataScope.ContainsKey(criticalDataScope))
            {
                throw new ArgumentException(
                    string.Format("CriticalDataScope '{0}' cannot be converted to the F2L CriticalDataScope type.", criticalDataScope),
                    "criticalDataScope");
            }

            return MapToF2LCriticalDataScope[criticalDataScope];
        }

        /// <summary>
        /// Extension method to convert a F2L <see cref="F2LPlayerMeters"/> to
        /// a <see cref="PlayerMeters"/>.
        /// </summary>
        /// <param name="f2LPlayerMeters">The F2L player meters to convert.</param>
        /// <returns>The conversion result.</returns>
        public static PlayerMeters ToPublic(this F2LPlayerMeters f2LPlayerMeters)
        {
            return f2LPlayerMeters == null
                       ? new PlayerMeters(0, 0, 0)
                       : new PlayerMeters(f2LPlayerMeters.PlayerWagerableMeter,
                                          f2LPlayerMeters.PlayerBankMeter,
                                          f2LPlayerMeters.PlayerPaidMeter);
        }
    }
}