//-----------------------------------------------------------------------
// <copyright file = "ConfigurationExtensionLookUp.cs" company = "IGT">
//     Copyright (c) 2017 IGT. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides information on signed registries stored internally in the core.
    /// </summary>
    public static class ConfigurationExtensionLookUp
    {
        private static readonly IDictionary<Guid, string> ConfigurationExtensionLookUpInformation = new Dictionary<Guid, string>
        {
            {new Guid("306CB4A4-DA06-40B4-BE1D-4A35823E97DB"), "Poker Configuration"},
            {new Guid("1f38810d-53ed-4b18-9125-6ffc92de79c8"), "RSDConfigProviderV1"}
        };

        /// <summary>
        /// Returns the name of the configuration extension based on the corresponding extensionId.
        /// </summary>
        /// <param name="extensionIdentifier">The identifier of the configuration extension.</param>
        /// <returns>The name of the extension or null if the identifier does not correspond to a signed extension registry.</returns>
        public static string GetExtensionName(Guid extensionIdentifier)
        {
            if(ConfigurationExtensionLookUpInformation.ContainsKey(extensionIdentifier))
            {
                return ConfigurationExtensionLookUpInformation[extensionIdentifier];
            }

            return null;
        }
    }
}