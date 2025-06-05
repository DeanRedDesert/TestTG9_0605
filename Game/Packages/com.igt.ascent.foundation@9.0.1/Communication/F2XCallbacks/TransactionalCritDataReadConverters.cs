// -----------------------------------------------------------------------
// <copyright file = "TransactionalCritDataReadConverters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X.Schemas.Internal.TransactionalCritDataRead;
    using F2X.Schemas.Internal.Types;

    /// <summary>
    /// Collection of extension methods helping convert between interface type
    /// CriticalDataScopeType and F2X schema type TransactionalCritDataRead.ItemChoiceType.
    /// </summary>
    public static class TransactionalCritDataReadConverters
    {
        /// <summary>
        /// Converts a <see cref="CriticalDataSelector"/> to a <see cref="CriticalDataItemSelector"/> for
        /// F2X transactional critical data read.
        /// </summary>
        /// <param name="criticalDataSelector">The critical data selector to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="criticalDataSelector"/> is null.
        /// </exception>
        public static CriticalDataItemSelector ToTransactionalCriticalDataReadItemSelector(
            this CriticalDataSelector criticalDataSelector)
        {
            if(criticalDataSelector == null)
            {
                throw new ArgumentNullException(nameof(criticalDataSelector));
            }

            var criticalDataItemSelector = new CriticalDataItemSelector
            {
                Name = criticalDataSelector.Path,
                ItemElementName = criticalDataSelector.Scope.ToTransReadScopeType()
            };

            switch(criticalDataSelector.Scope)
            {
                case CriticalDataScope.Payvar:
                case CriticalDataScope.PayvarPersistent:
                case CriticalDataScope.PayvarAnalytics:
                    criticalDataItemSelector.Item = criticalDataSelector.ScopeIdentifier.ToPayvarIdentifier();
                    break;
                case CriticalDataScope.Theme:
                case CriticalDataScope.ThemePersistent:
                case CriticalDataScope.ThemeAnalytics:
                    criticalDataItemSelector.Item = criticalDataSelector.ScopeIdentifier.ToThemeIdentifier();
                    break;
                case CriticalDataScope.Extension:
                case CriticalDataScope.ExtensionPersistent:
                case CriticalDataScope.ExtensionAnalytics:
                    criticalDataItemSelector.Item = criticalDataSelector.ScopeIdentifier.ToExtensionIdentifier();
                    break;
            }

            return criticalDataItemSelector;
        }

        /// <summary>
        /// Converts a <see cref="CriticalDataItemSelector"/> for F2X transactional critical data read
        /// to a <see cref="CriticalDataSelector"/>.
        /// </summary>
        /// <param name="criticalDataItemSelector">The critical data item selector to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataItemSelector"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when scope identifier of <paramref name="criticalDataItemSelector"/> is invalid.
        /// </exception>
        public static CriticalDataSelector ToCriticalDataSelector(this CriticalDataItemSelector criticalDataItemSelector)
        {
            if(criticalDataItemSelector == null)
            {
                throw new ArgumentNullException(nameof(criticalDataItemSelector));
            }

            var path = criticalDataItemSelector.Name;
            var scopeType = criticalDataItemSelector.ItemElementName.ToPublic();

            string identifier;
            switch(criticalDataItemSelector.ItemElementName)
            {
                case ItemChoiceType.Payvar:
                case ItemChoiceType.PayvarPersistent:
                case ItemChoiceType.PayvarAnalytics:
                    identifier = ((PayvarIdentifier)criticalDataItemSelector.Item).Value;
                    break;
                case ItemChoiceType.Theme:
                case ItemChoiceType.ThemePersistent:
                case ItemChoiceType.ThemeAnalytics:
                    identifier = ((ThemeIdentifier)criticalDataItemSelector.Item).Value;
                    break;
                case ItemChoiceType.Extension:
                case ItemChoiceType.ExtensionPersistent:
                case ItemChoiceType.ExtensionAnalytics:
                    identifier = ((ExtensionIdentifier)criticalDataItemSelector.Item).Value;
                    break;
                default:
                    throw new ArgumentException(
                        $"The scope {criticalDataItemSelector.ItemElementName} is not supported to accessing critical data transactionally.", nameof(criticalDataItemSelector));
            }

            return new CriticalDataSelector(scopeType, identifier, path);
        }
    }
}
