// -----------------------------------------------------------------------
// <copyright file = "F2XRegistryTypesVer1.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Registries.Internal.F2X.F2XRegistryTypesVer1
{
    /// <summary>
    /// This partial class defines custom methods of <see cref="ExtensionVersion"/> type.
    /// </summary>
    public partial class ExtensionVersion
    {
        /// <summary>
        /// Convert to a <see cref="System.Version"/> instance.
        /// </summary>
        /// <returns>A <see cref="System.Version"/> instance.</returns>
        public System.Version ToVersion()
        {
            return new System.Version((int)MajorVersion, (int)MinorVersion, (int)PatchVersion);
        }
    }
}