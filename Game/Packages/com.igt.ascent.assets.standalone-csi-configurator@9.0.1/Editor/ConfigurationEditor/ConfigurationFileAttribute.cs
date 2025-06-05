//-----------------------------------------------------------------------
// <copyright file = "ConfigurationFileAttribute.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.ConfigurationEditor.Editor
{
    using System;

    /// <summary>
    /// Defines the attribute to specify the configuration file name.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ConfigurationFileAttribute : Attribute
    {
        /// <summary>
        /// The cached configuration file name.
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Initializes an instance of <see cref="ConfigurationFileAttribute"/>.
        /// </summary>
        /// <param name="filename">The configuration file name.</param>
        public ConfigurationFileAttribute(string filename)
        {
            FileName = filename;
        }
    }
}
