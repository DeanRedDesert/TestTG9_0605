// -----------------------------------------------------------------------
// <copyright file = "PlatformSettings.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    /// <summary>
    /// Global settings accessed by all SDK internal components.
    /// </summary>
    internal static class PlatformSettings
    {
        /// <summary>
        /// The flag indicating whether the safe storage should hold human readable strings that represent the data.
        /// </summary>
        internal static bool SafeStorageRepStringsEnabled { get; set; }
    }
}