// -----------------------------------------------------------------------
//  <copyright file = "PackageVersion.cs" company = "IGT">
//      Copyright (c) 2023 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the package version information.
    /// </summary>
    public class PackageVersion
    {
        /// <summary>
        /// The package name.
        /// </summary>
        [JsonProperty("package")]
        public string Package { get; set; }
        
        /// <summary>
        /// The package version.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}