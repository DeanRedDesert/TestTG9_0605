//-----------------------------------------------------------------------
// <copyright file = "SwitchThemeContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Event indicating a switch theme context and all pertinent information.
    /// </summary>
    [Serializable]
    public class SwitchThemeContextEventArgs : EventArgs
    {
        /// <summary>
        /// The tag defined for this switch theme context.
        /// </summary>
        public string ThemeTag { get; private set; }

        /// <summary>
        /// The tag data file defined for this switch theme context.
        /// </summary>
        public string ThemeTagDataFile { get; private set; }

        /// <summary>
        /// The collection of resource paths (tagfield, pathfield) for this switch 
        /// theme context.
        /// </summary>
        public Dictionary<string, string> ResourcePaths { get; private set; }

        /// <summary>
        /// The denom for this switch theme context.
        /// </summary>
        public uint Denom { get; private set; }

        /// <summary>
        /// The payvar tag for this switch theme context.
        /// </summary>
        public string PayvarTag { get; private set; }

        /// <summary>
        /// The payvar tag data file for this switch theme context.
        /// </summary>
        public string PayvarTagDataFile { get; private set; }

        /// <summary>
        /// Instantiates an instance of <see cref="SwitchThemeContextEventArgs"/>.
        /// </summary>
        /// <param name="themeTag">The theme tag.</param>
        /// <param name="themeTagDataFile">The theme tag's data file.</param>
        /// <param name="resourcePaths">The collection of resource paths.</param>
        /// <param name="denom">The denom value.</param>
        /// <param name="payvarTag">The payvar tag.</param>
        /// <param name="payvarTagDataFile">The payvar tag's data file.</param>
        public SwitchThemeContextEventArgs(string themeTag, string themeTagDataFile, 
                                           Dictionary<string, string> resourcePaths, uint denom,
                                           string payvarTag, string payvarTagDataFile)
        {
            ThemeTag = themeTag;
            ThemeTagDataFile = themeTagDataFile;
            ResourcePaths = resourcePaths;
            Denom = denom;
            PayvarTag = payvarTag;
            PayvarTagDataFile = payvarTagDataFile;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var info = new StringBuilder();

            info.AppendLine("SwitchThemeContextEventArgs -");
            info.AppendLine($"\t ThemeTag({ThemeTag})");
            info.AppendLine($"\t ThemeTagDataFile({ThemeTagDataFile})");

            foreach(var keyValuePair in ResourcePaths)
            {
                info.AppendLine($"\t Tag({keyValuePair.Key}), Path({keyValuePair.Value})");
            }

            info.AppendLine($"\t Denom({Denom})");
            info.AppendLine($"\t PayvarTag({PayvarTag})");
            info.AppendLine($"\t PayvarTagDataFile({PayvarTagDataFile})");

            return info.ToString();
        }
    }
}