//-----------------------------------------------------------------------
// <copyright file = "ConfigurationRootAttribute.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.ConfigurationEditor.Editor
{
    using System;

    /// <summary>
    /// Defines the attribute to specify the root name of the configuration file in XML.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ConfigurationRootAttribute : Attribute
    {
        /// <summary>
        /// The cached configuration root.
        /// </summary>
        public readonly string ConfigRoot;

        /// <summary>
        /// Initializes an instance of <see cref="ConfigurationRootAttribute"/>.
        /// </summary>
        /// <param name="configRoot">The configuration root.</param>
        public ConfigurationRootAttribute(string configRoot)
        {
            ConfigRoot = configRoot;
        }
    }
}
