// -----------------------------------------------------------------------
// <copyright file = "ExtensionCriticalDataAccessValidation.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Standard
{
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation;
    using Platform.Interfaces;

    /// <summary>
    /// Implementation of <see cref="ICriticalDataAccessValidation"/> to be used by Extension clients.
    /// </summary>
    internal sealed class ExtensionCriticalDataAccessValidation : ICriticalDataAccessValidation
    {
        #region Implementation of ICriticalDataAccessValidation

        /// <inheritdoc />
        public void ValidateCriticalDataAccess(IList<CriticalDataSelector> criticalDataSelectors, DataAccessing dataAccessing)
        {
            var disallowedSelectors = criticalDataSelectors
                                      .Where(selector => !SupportedCriticalDataScopeTable.IsScopeAllowed(CriticalDataScopeClientType.Extension,
                                                                                                         selector.Scope))
                                      .ToArray();

            if(disallowedSelectors.Any())
            {
                var disallowedScopes = string.Join(",", disallowedSelectors.Distinct().Select(selector => selector.Scope.ToString()).ToArray());
                throw new CriticalDataAccessDeniedException($"The critical data scope [{disallowedScopes}] are not accessible to extensions.");
            }
        }

        #endregion
    }
}