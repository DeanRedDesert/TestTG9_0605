// -----------------------------------------------------------------------
//  <copyright file = "CustomBuildPaths.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SDKBuild
{
    using System.Collections.Generic;

    /// <summary>
    /// Stores a collection of directories and files that should be copied from the 
    /// project location to the build location when a build is started.
    /// </summary>
    public class CustomBuildPaths
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CustomBuildPaths"/>.
        /// </summary>
        public CustomBuildPaths()
        {
            CustomDirectories = new List<string>();
            CustomFiles = new List<string>();
        }

        /// <summary>
        /// Gets or sets a list of custom directories to copy during a build.
        /// </summary>
        public List<string> CustomDirectories { get; set; }

        /// <summary>
        /// Gets or sets a list of custom files to copy during a build.
        /// </summary>
        public List<string> CustomFiles { get; set; }
    }
}