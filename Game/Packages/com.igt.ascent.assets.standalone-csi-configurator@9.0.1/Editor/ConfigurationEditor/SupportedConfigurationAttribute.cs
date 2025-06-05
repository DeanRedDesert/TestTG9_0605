//-----------------------------------------------------------------------
// <copyright file = "SupportedConfigurationAttribute.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.ConfigurationEditor.Editor
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines the attribute to specify the supported configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class SupportedConfigurationAttribute : Attribute
    {
        /// <summary>
        /// The cached configuration ID.
        /// </summary>
        public readonly string ConfigId;

        /// <summary>
        /// The cached name for the supported configuration.
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the name of the supported configuration.
        /// </summary>
        /// <remarks>
        /// All space characters in between the configuration name will be removed automatically.
        /// </remarks>
        public string Name
        {
            get { return name ?? Regex.Replace(ConfigId, @"\s+", ""); }
            set { name = Regex.Replace(value, @"\s+", ""); }
        }

        /// <summary>
        /// Gets or sets the assembly name where the config type is defined.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public string ConfigTypeAssembly { get; set; }

        /// <summary>
        /// The cached config type name for the supported configuration.
        /// </summary>
        private string configTypeName;

        /// <summary>
        /// Gets or sets the config type name for the supported configuration.
        /// </summary>
        /// <remarks>
        /// All space characters in between the config type name will be removed automatically.
        /// If not defined explicitly, the config type name will be the configuration name suffixed by "Config"
        /// by default.
        /// For example, if the configuration name is Xxxxx, the default editor type name will be XxxxxConfig.
        /// </remarks>
        public string ConfigTypeName
        {
            get { return configTypeName ?? Name + "Config"; }
            set { configTypeName = Regex.Replace(value, @"\s+", ""); }
        }

        /// <summary>
        /// The cached editor type name for the supported configuration.
        /// </summary>
        private string editorTypeName;

        /// <summary>
        /// Gets or sets the editor type name for the supported configuration.
        /// </summary>
        /// <remarks>
        /// All space characters in between the editor type name will be removed automatically.
        /// If not defined explicitly, the editor type name will be the configuration name suffixed by "Editor"
        /// by default.
        /// For example, if the configuration name is Xxxxx, the default editor type name will be XxxxxEditor.
        /// </remarks>
        public string EditorTypeName
        {
            get { return editorTypeName ?? Name + "Editor"; }
            set { editorTypeName = Regex.Replace(value, @"\s+", ""); }
        }

        /// <summary>
        /// Initializes an instance of <see cref="SupportedConfigurationAttribute"/>.
        /// </summary>
        /// <param name="configId">The configuration ID.</param>
        public SupportedConfigurationAttribute(string configId)
        {
            ConfigId = configId;
        }
    }
}
