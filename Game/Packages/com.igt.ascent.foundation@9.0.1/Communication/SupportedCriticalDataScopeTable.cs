//-----------------------------------------------------------------------
// <copyright file = "SupportedCriticalDataScopeTable.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Collection of supported critical data scope table used for validation of accessing
    /// critical data. 
    /// </summary>
    internal static class SupportedCriticalDataScopeTable
    {
        #region Validation Table

        /// <summary>
        /// The critical data types allowed to be accessed in games.
        /// </summary>
        private static readonly List<CriticalDataScope> AllowedCriticalDataScopesForGame
            = new List<CriticalDataScope>
                  {
                      CriticalDataScope.Feature,
                      CriticalDataScope.GameCycle,
                      CriticalDataScope.History,
                      CriticalDataScope.Payvar,
                      CriticalDataScope.PayvarAnalytics,
                      CriticalDataScope.PayvarPersistent,
                      CriticalDataScope.Theme,
                      CriticalDataScope.ThemeAnalytics,
                      CriticalDataScope.ThemePersistent
                  };

        /// <summary>
        /// The critical data types allowed to be accessed in reports.
        /// </summary>
        private static readonly List<CriticalDataScope> AllowedCriticalDataScopesForReport
            = new List<CriticalDataScope>
                  {
                      CriticalDataScope.Payvar,
                      CriticalDataScope.PayvarAnalytics,
                      CriticalDataScope.PayvarPersistent,
                      CriticalDataScope.Theme,
                      CriticalDataScope.ThemeAnalytics,
                      CriticalDataScope.ThemePersistent
                  };

        /// <summary>
        /// The critical data types allowed to be accessed in extensions.
        /// </summary>
        private static readonly List<CriticalDataScope> AllowedCriticalDataScopesForExtension
            = new List<CriticalDataScope>
                  {
                      CriticalDataScope.Extension,
                      CriticalDataScope.ExtensionAnalytics,
                      CriticalDataScope.ExtensionPersistent
                  };

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if given <paramref name="scope"/> is allowed to access critical data
        /// for specified <paramref name="clientType"/>.
        /// </summary>
        /// <param name="clientType">The specified client that will access critical data.</param>
        /// <param name="scope">The given scope to check.</param>
        /// <returns>
        /// True if given <paramref name="scope"/> is allowed to access critical data for
        /// specified <paramref name="clientType"/>, false otherwise.
        /// </returns>
        public static bool IsScopeAllowed(CriticalDataScopeClientType clientType,
                                          CriticalDataScope scope)
        {
            switch(clientType)
            {
                case CriticalDataScopeClientType.Game:
                    return AllowedCriticalDataScopesForGame.Contains(scope);
                case CriticalDataScopeClientType.Report:
                    return AllowedCriticalDataScopesForReport.Contains(scope);
                case CriticalDataScopeClientType.Extension:
                    return AllowedCriticalDataScopesForExtension.Contains(scope);
                default:
                    throw new ArgumentOutOfRangeException("clientType", clientType, "The specified client is not valid.");
            }
        }

        #endregion
    }
}
