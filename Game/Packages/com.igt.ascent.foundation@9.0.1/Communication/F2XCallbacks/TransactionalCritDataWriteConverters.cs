// -----------------------------------------------------------------------
// <copyright file = "TransactionalCritDataWriteConverters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X.Schemas.Internal.TransactionalCritDataWrite;

    /// <summary>
    /// Collection of extension methods helping convert between interface type
    /// CriticalDataScopeType and F2X schema type TransactionalCritDataWrite.ItemChoiceType.
    /// </summary>
    public static class TransactionalCritDataWriteConverters
    {
        /// <summary>
        /// Converts a <see cref="CriticalDataSelector"/> to a <see cref="CriticalDataItemSelector"/>
        /// for F2X transactional critical data write.
        /// </summary>
        /// <param name="criticalDataSelector">The critical data selector to convert.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="criticalDataSelector"/> is null.
        /// </exception>
        public static CriticalDataItemSelector ToTransactionalCriticalDataWriteItemSelector(
            this CriticalDataSelector criticalDataSelector)
        {
            if(criticalDataSelector == null)
            {
                throw new ArgumentNullException(nameof(criticalDataSelector));
            }

            var criticalDataItemSelector = new CriticalDataItemSelector
            {
                Name = criticalDataSelector.Path,
                ItemElementName = criticalDataSelector.Scope.ToTransWriteScopeType()
            };

            switch(criticalDataSelector.Scope)
            {
                case CriticalDataScope.Extension:
                case CriticalDataScope.ExtensionPersistent:
                case CriticalDataScope.ExtensionAnalytics:
                    criticalDataItemSelector.Item = criticalDataSelector.ScopeIdentifier.ToExtensionIdentifier();
                    break;
            }

            return criticalDataItemSelector;
        }

        /// <summary>
        /// Converts a <see cref="CriticalDataItemSelector"/> for F2X transactional critical data write
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
                case ItemChoiceType.Extension:
                case ItemChoiceType.ExtensionPersistent:
                case ItemChoiceType.ExtensionAnalytics:
                    identifier = criticalDataItemSelector.Item.Value;
                    break;
                default:
                    throw new ArgumentException(
                        $"The scope {criticalDataItemSelector.ItemElementName} is not supported for writing critical data transactionally.", nameof(criticalDataItemSelector));
            }

            return new CriticalDataSelector(scopeType, identifier, path);
        }
    }
}
