//-----------------------------------------------------------------------
// <copyright file = "ConfigurationMaster.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.ConfigurationEditor.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// This class provides a set of manipulation functionalities for a configuration editor.
    /// </summary>
    public class ConfigurationMaster
    {
        /// <summary>
        /// The file name of the configuration XML file.
        /// </summary>
        private readonly string configFilename;

        /// <summary>
        /// The root name of the configuration XML file.
        /// </summary>
        private readonly string configRootName;

        /// <summary>
        /// The cached mapping of sub-configurations supported by current configuration editor.
        /// </summary>
        private readonly Dictionary<string, SupportedConfigurationInfo> supportedConfigurations;

        /// <summary>
        /// Initializes an instance of <see cref="ConfigurationMaster"/>.
        /// </summary>
        /// <param name="typeOfConfigEditor">The type of the configuration editor.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="typeOfConfigEditor"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no <see cref="ConfigurationFileAttribute"/> or no <see cref="ConfigurationRootAttribute"/>
        /// or no <see cref="SupportedConfigurationAttribute"/> is defined for the configuration editor.
        /// </exception>
        public ConfigurationMaster(Type typeOfConfigEditor)
        {
            if(typeOfConfigEditor == null)
            {
                throw new ArgumentNullException("typeOfConfigEditor");
            }

            var fileAttr = Attribute.GetCustomAttribute(
                typeOfConfigEditor,
                typeof(ConfigurationFileAttribute)
                ) as ConfigurationFileAttribute;
            if(fileAttr == null)
            {
                throw new InvalidOperationException(string.Format(
                    "No ConfigurationFileAttribute is defined for the editor [{0}].", typeOfConfigEditor.Name));
            }
            configFilename = fileAttr.FileName;

            var rootAttr = Attribute.GetCustomAttribute(
                typeOfConfigEditor,
                typeof(ConfigurationRootAttribute)
                ) as ConfigurationRootAttribute;
            if(rootAttr == null)
            {
                throw new InvalidOperationException(string.Format(
                    "No ConfigurationRootAttribute is defined for the editor [{0}].", typeOfConfigEditor.Name));
            }
            configRootName = rootAttr.ConfigRoot;

            var configAttrs =
                Attribute.GetCustomAttributes(typeOfConfigEditor, typeof(SupportedConfigurationAttribute))
                    .OfType<SupportedConfigurationAttribute>().ToList();
            if(!configAttrs.Any())
            {
                throw new InvalidOperationException(string.Format(
                    "No SupportedConfigurationAttribute is defined for the editor [{0}].", typeOfConfigEditor.Name));
            }
            supportedConfigurations = new Dictionary<string, SupportedConfigurationInfo>();
            configAttrs.ForEach(attr => supportedConfigurations.Add(
                attr.ConfigId,
                new SupportedConfigurationInfo(
                    attr.Name,
                    attr.ConfigTypeAssembly,
                    attr.ConfigTypeName,
                    attr.EditorTypeName)));
        }

        /// <summary>
        /// Open the configuration file in XML for the editor to work on.
        /// </summary>
        public void OpenConfigurations()
        {
            if(!File.Exists(configFilename)) return;

            using(var configStream = (new StreamReader(configFilename)).BaseStream)
            using(var reader = XmlReader.Create(configStream))
            {
                var configElement = configStream == null ? new XElement(configRootName) : XElement.Load(reader);
                foreach(var configuration in supportedConfigurations)
                {
                    var element = configElement.Element(configuration.Value.Name);
                    if(element == null) continue;
                    configuration.Value.DeserializeConfig(element);
                }
            }
        }

        /// <summary>
        /// Save all the configurations in the editor to the configuration file in XML.
        /// </summary>
        public void SaveConfigurations()
        {
            var systemConfigRoot = new XElement(configRootName);
            foreach(
                var element in supportedConfigurations.Select(configuration => configuration.Value.SerializeConfig())
                    .Where(element => element != null))
            {
                systemConfigRoot.Add(element);
            }

            if(File.Exists(configFilename))
            {
                var attributes = File.GetAttributes(configFilename);
                if((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(configFilename, attributes & ~FileAttributes.ReadOnly);
                }
            }

            systemConfigRoot.Save(configFilename);
        }

        /// <summary>
        /// Update all the supported configurations in the editor.
        /// </summary>
        public void EditConfigurations()
        {
            foreach(var configuration in supportedConfigurations)
            {
                configuration.Value.InvokeEditorUpdateConfig(configuration.Key);
            }
        }
    }
}
