// -----------------------------------------------------------------------
// <copyright file = "CriticalDataScope.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// The enumeration defines the scopes for accessing to critical data.
    /// </summary>
    [Serializable]
    public enum CriticalDataScope
    {
        /// <summary>
        /// Indicating the critical data item is in feature scope.
        /// </summary>
        Feature,

        /// <summary>
        /// Indicating the critical data item is in game-cycle scope.
        /// </summary>
        GameCycle,

        /// <summary>
        /// Indicating the critical data item is in history scope.
        /// </summary>
        History,

        /// <summary>
        /// Indicating the critical data item is in payvar scope.
        /// </summary>
        Payvar,

        /// <summary>
        /// Indicating the critical data item is in payvar-persistent scope.
        /// </summary>
        /// <remarks>
        /// Items at the payvar-persistent scope are distinct/separate from payvar scope items of the same name.
        /// 
        /// CAUTION: Persistent scopes will survive both E2 and RAM clear.  Do not use this scope
        /// to store information that might depend on any configurations stored in E2 or RAM,
        /// such as currency, credit formatter etc.
        /// </remarks>
        PayvarPersistent,

        /// <summary>
        /// Indicating the critical data item is in payvar-analytics scope.
        /// </summary>
        /// <remarks>
        /// Items at the payvar-analytics scope are distinct from both payvar and payvar-persistent scope.
        /// </remarks>
        PayvarAnalytics,

        /// <summary>
        /// Indicating the critical data item is in theme scope.
        /// </summary>
        Theme,

        /// <summary>
        /// Indicating the critical data item is in the theme-persistent scope.
        /// </summary>
        /// <remarks>
        /// Items at the theme-persistent scope are distinct/separate from theme scope items of the same name.
        /// 
        /// CAUTION: Persistent scopes will survive both E2 and RAM clear.  Do not use this scope
        /// to store information that might depend on any configurations stored in E2 or RAM,
        /// such as currency, credit formatter etc.
        /// </remarks>
        ThemePersistent,

        /// <summary>
        /// Indicating the critical data item is in the theme-analytics scope.
        /// </summary>
        /// <remarks>
        /// Items at the theme-analytics scope are distinct from both theme and theme-persistent scope.
        /// </remarks>
        ThemeAnalytics,

        /// <summary>
        /// Indicating the critical data item is in extension scope.
        /// </summary>
        Extension,

        /// <summary>
        /// Indicating the critical data item is in extension-persistent scope.
        /// </summary>
        /// <remarks>
        /// Items at the extension-persistent scope are distinct/separate from extension scope items of the same name.
        /// 
        /// CAUTION: Persistent scopes will survive both E2 and RAM clear.  Do not use this scope
        /// to store information that might depend on any configurations stored in E2 or RAM,
        /// such as currency, credit formatter etc.
        /// </remarks>
        ExtensionPersistent,

        /// <summary>
        /// Indicating the critical data item is in extension-analytics scope.
        /// </summary>
        /// <remarks>
        /// Items at the extension-analytics scope are distinct from both extension and extension-persistent scope.
        /// </remarks>
        ExtensionAnalytics
    }
}