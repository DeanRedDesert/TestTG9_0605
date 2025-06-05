//-----------------------------------------------------------------------
// <copyright file = "CriticalDataScopeConverters.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using NonTransReadItemChoiceType = F2X.Schemas.Internal.NonTransactionalCritDataRead.ItemChoiceType;
    using TransReadItemChoiceType = F2X.Schemas.Internal.TransactionalCritDataRead.ItemChoiceType;
    using TransWriteItemChoiceType = F2X.Schemas.Internal.TransactionalCritDataWrite.ItemChoiceType;

    /// <summary>
    /// Collection of extension methods help converting between interface type of
    /// <see cref="CriticalDataScope"/> and F2X schema types.
    /// </summary>
    internal static class CriticalDataScopeConverters
    {
        #region Private Members

        /// <summary>
        /// Mappings from F2X <see cref="NonTransReadItemChoiceType"/> to <see cref="CriticalDataScope"/>.
        /// </summary>
        private static readonly Dictionary<NonTransReadItemChoiceType, CriticalDataScope>
            MapFromNonTransReadScopeType =
                new Dictionary<NonTransReadItemChoiceType, CriticalDataScope>
                    {
                        { NonTransReadItemChoiceType.Payvar, CriticalDataScope.Payvar },
                        { NonTransReadItemChoiceType.PayvarPersistent, CriticalDataScope.PayvarPersistent },
                        { NonTransReadItemChoiceType.PayvarAnalytics, CriticalDataScope.PayvarAnalytics },
                        { NonTransReadItemChoiceType.Theme, CriticalDataScope.Theme },
                        { NonTransReadItemChoiceType.ThemePersistent, CriticalDataScope.ThemePersistent },
                        { NonTransReadItemChoiceType.ThemeAnalytics, CriticalDataScope.ThemeAnalytics },
                        { NonTransReadItemChoiceType.Extension, CriticalDataScope.Extension },
                        { NonTransReadItemChoiceType.ExtensionPersistent, CriticalDataScope.ExtensionPersistent },
                        { NonTransReadItemChoiceType.ExtensionAnalytics, CriticalDataScope.ExtensionAnalytics }
                    };

        /// <summary>
        /// Mappings from F2X <see cref="TransReadItemChoiceType"/> to <see cref="CriticalDataScope"/>.
        /// </summary>
        private static readonly Dictionary<TransReadItemChoiceType, CriticalDataScope>
            MapFromTransReadScopeType =
                new Dictionary<TransReadItemChoiceType, CriticalDataScope>
                    {
                        { TransReadItemChoiceType.Payvar, CriticalDataScope.Payvar },
                        { TransReadItemChoiceType.PayvarPersistent, CriticalDataScope.PayvarPersistent },
                        { TransReadItemChoiceType.PayvarAnalytics, CriticalDataScope.PayvarAnalytics },
                        { TransReadItemChoiceType.Theme, CriticalDataScope.Theme },
                        { TransReadItemChoiceType.ThemePersistent, CriticalDataScope.ThemePersistent },
                        { TransReadItemChoiceType.ThemeAnalytics, CriticalDataScope.ThemeAnalytics },
                        { TransReadItemChoiceType.Extension, CriticalDataScope.Extension },
                        { TransReadItemChoiceType.ExtensionPersistent, CriticalDataScope.ExtensionPersistent },
                        { TransReadItemChoiceType.ExtensionAnalytics, CriticalDataScope.ExtensionAnalytics }
                    };

        /// <summary>
        /// Mappings from F2X <see cref="TransWriteItemChoiceType"/> to <see cref="CriticalDataScope"/>.
        /// </summary>
        private static readonly Dictionary<TransWriteItemChoiceType, CriticalDataScope>
            MapFromTransWriteScopeType =
                new Dictionary<TransWriteItemChoiceType, CriticalDataScope>
                    {
                        { TransWriteItemChoiceType.Extension, CriticalDataScope.Extension },
                        { TransWriteItemChoiceType.ExtensionPersistent, CriticalDataScope.ExtensionPersistent },
                        { TransWriteItemChoiceType.ExtensionAnalytics, CriticalDataScope.ExtensionAnalytics }
                    };

        /// <summary>
        /// Mappings from <see cref="CriticalDataScope"/> to F2X <see cref="NonTransReadItemChoiceType"/>.
        /// </summary>
        private static Dictionary<CriticalDataScope, NonTransReadItemChoiceType> mapToNonTransReadScopeType;

        /// <summary>
        /// Mappings from <see cref="CriticalDataScope"/> to F2X <see cref="TransReadItemChoiceType"/>.
        /// </summary>
        private static Dictionary<CriticalDataScope, TransReadItemChoiceType> mapToTransReadScopeType;

        /// <summary>
        /// Mappings from <see cref="CriticalDataScope"/> to F2X <see cref="TransWriteItemChoiceType"/>.
        /// </summary>
        private static Dictionary<CriticalDataScope, TransWriteItemChoiceType> mapToTransWriteScopeType;

        /// <summary>
        /// Gets the mappings from <see cref="CriticalDataScope"/> to F2X <see cref="NonTransReadItemChoiceType"/>.
        /// </summary>
        private static Dictionary<CriticalDataScope, NonTransReadItemChoiceType> MapToNonTransReadScopeType
        {
            get
            {
                return mapToNonTransReadScopeType ??
                       (mapToNonTransReadScopeType =
                           MapFromNonTransReadScopeType.ToDictionary(pair => pair.Value, pair => pair.Key));
            }
        }

        /// <summary>
        /// Gets the mappings from <see cref="CriticalDataScope"/> to F2X <see cref="TransReadItemChoiceType"/>.
        /// </summary>
        private static Dictionary<CriticalDataScope, TransReadItemChoiceType> MapToTransReadScopeType
        {
            get
            {
                return mapToTransReadScopeType ??
                       (mapToTransReadScopeType =
                           MapFromTransReadScopeType.ToDictionary(pair => pair.Value, pair => pair.Key));
            }
        }

        /// <summary>
        /// Gets the mappings from <see cref="CriticalDataScope"/> to F2X <see cref="TransWriteItemChoiceType"/>.
        /// </summary>
        private static Dictionary<CriticalDataScope, TransWriteItemChoiceType> MapToTransWriteScopeType
        {
            get
            {
                return mapToTransWriteScopeType ??
                       (mapToTransWriteScopeType =
                           MapFromTransWriteScopeType.ToDictionary(pair => pair.Value, pair => pair.Key));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Extension method to convert a F2X <see cref="NonTransReadItemChoiceType"/> to
        /// a <see cref="CriticalDataScope"/>.
        /// </summary>
        /// <param name="itemChoiceType">The item choice type to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="itemChoiceType"/> cannot be converted to critical data scope.
        /// </exception>
        public static CriticalDataScope ToPublic(this NonTransReadItemChoiceType itemChoiceType)
        {
            if(!MapFromNonTransReadScopeType.ContainsKey(itemChoiceType))
            {
                throw new ArgumentException("Cannot convert to critical data scope from given " + itemChoiceType);
            }

            return MapFromNonTransReadScopeType[itemChoiceType];
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="TransReadItemChoiceType"/> to
        /// a <see cref="CriticalDataScope"/>.
        /// </summary>
        /// <param name="itemChoiceType">The item choice type to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="itemChoiceType"/> cannot be converted to critical data scope.
        /// </exception>
        public static CriticalDataScope ToPublic(this TransReadItemChoiceType itemChoiceType)
        {
            if(!MapFromTransReadScopeType.ContainsKey(itemChoiceType))
            {
                throw new ArgumentException("Cannot convert to critical data scope from given " + itemChoiceType);
            }

            return MapFromTransReadScopeType[itemChoiceType];
        }

        /// <summary>
        /// Extension method to convert a F2X <see cref="TransWriteItemChoiceType"/> to
        /// a <see cref="CriticalDataScope"/>.
        /// </summary>
        /// <param name="itemChoiceType">The item choice type to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="itemChoiceType"/> cannot be converted to critical data scope.
        /// </exception>
        public static CriticalDataScope ToPublic(this TransWriteItemChoiceType itemChoiceType)
        {
            if(!MapFromTransWriteScopeType.ContainsKey(itemChoiceType))
            {
                throw new ArgumentException("Cannot convert to critical data scope from given " + itemChoiceType);
            }

            return MapFromTransWriteScopeType[itemChoiceType];
        }

        /// <summary>
        /// Extension method to convert a <see cref="CriticalDataScope"/> to
        /// a F2X <see cref="NonTransReadItemChoiceType"/>.
        /// </summary>
        /// <param name="criticalDataScope">The critical data scope to convert.</param>
        /// <returns> The conversion result. </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="criticalDataScope"/> cannot be converted to item choice type.
        /// </exception>
        public static NonTransReadItemChoiceType ToNonTransReadScopeType(this CriticalDataScope criticalDataScope)
        {
            if(!MapToNonTransReadScopeType.ContainsKey(criticalDataScope))
            {
                throw new ArgumentException("Cannot convert to item choice type from given " + criticalDataScope);
            }

            return MapToNonTransReadScopeType[criticalDataScope];
        }

        /// <summary>
        /// Extension method to convert a <see cref="CriticalDataScope"/> to
        /// a F2X <see cref="TransReadItemChoiceType"/>.
        /// </summary>
        /// <param name="criticalDataScope">The critical data scope to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="criticalDataScope"/> cannot be converted to item choice type.
        /// </exception>
        public static TransReadItemChoiceType ToTransReadScopeType(this CriticalDataScope criticalDataScope)
        {
            if(!MapToTransReadScopeType.ContainsKey(criticalDataScope))
            {
                throw new ArgumentException("Cannot convert to item choice type from given " + criticalDataScope);
            }

            return MapToTransReadScopeType[criticalDataScope];
        }

        /// <summary>
        /// Extension method to convert a <see cref="CriticalDataScope"/> to
        /// a F2X <see cref="TransWriteItemChoiceType"/>.
        /// </summary>
        /// <param name="criticalDataScope">The critical data scope to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="criticalDataScope"/> cannot be converted to item choice type.
        /// </exception>
        public static TransWriteItemChoiceType ToTransWriteScopeType(this CriticalDataScope criticalDataScope)
        {
            if(!MapToTransWriteScopeType.ContainsKey(criticalDataScope))
            {
                throw new ArgumentException("Cannot convert to item choice type from given " + criticalDataScope);
            }

            return MapToTransWriteScopeType[criticalDataScope];
        }

        #endregion
    }
}
