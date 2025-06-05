//-----------------------------------------------------------------------
// <copyright file = "DiskStoreSection.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    /// <summary>
    /// The DiskStoreSection enumeration is used to represent
    /// the different data sections in a Disk Store.
    /// 
    /// This is only used by Standalone GameLib.
    /// </summary>
    public enum DiskStoreSection
    {
        /// <summary>
        /// The section where the game owned critical data is safe stored.
        /// </summary>
        CriticalData,

        /// <summary>
        /// The section where the meters are safe stored.
        /// </summary>
        Meters,

        /// <summary>
        /// The section where the foundation owned critical data is safe stored.
        /// </summary>
        FoundationData,

        ///<summary>
        /// The section where the history of completed game cycles are safe stored.
        /// </summary>
        History,

        /// <summary>
        /// The section where the critical data per theme is safe stored.
        /// </summary>
        ThemeCriticalData,

        /// <summary>
        /// The section where the anlytics per theme are safe stored.
        /// </summary>
        ThemeAnalyticsCriticalData,

        /// <summary>
        /// The section where the persistent per theme are safe stored.
        /// </summary>
        ThemePersistentCriticalData,

        /// <summary>
        /// The section where the configurations per theme are safe stored.
        /// </summary>
        ThemeConfigurations,

        /// <summary>
        /// The section where the configuration files per theme are safe stored.
        /// </summary>
        ThemeConfigurationProfiles,

        /// <summary>
        /// The section where the critical data per payvar is safe stored.
        /// </summary>
        PayvarCriticalData,

        /// <summary>
        /// The section where the analytics per payvar are safe stored.
        /// </summary>
        PayvarAnalyticsCriticalData,

        /// <summary>
        /// The section where the persistent per payvar are safe stored.
        /// </summary>
        PayvarPersistentCriticalData,

        /// <summary>
        /// The section where the configurations per payvar are safe stored.
        /// </summary>
        PayvarConfigurations,

        /// <summary>
        /// The section where the configuration files per payvar are safe stored.
        /// </summary>
        PayvarConfigurationProfiles,

        /// <summary>
        /// The section where the configuration per extension are safe stored.
        /// </summary>
        ExtensionConfigurations,

        /// <summary>
        /// The section where the configuration files per extension are safe stored.
        /// </summary>
        ExtensionConfigurationProfiles
    }
}
