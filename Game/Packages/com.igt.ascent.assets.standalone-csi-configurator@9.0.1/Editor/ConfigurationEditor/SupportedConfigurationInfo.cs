//-----------------------------------------------------------------------
// <copyright file = "SupportedConfigurationInfo.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.ConfigurationEditor.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// This class contains all the information about the supported configuration at runtime.
    /// </summary>
    internal class SupportedConfigurationInfo
    {
        /// <summary>
        /// The enum list of the methods exported in the supported configuration editor class.
        /// </summary>
        private enum EditorMethods
        {
            ImportConfig,
            ExportConfig,
            UpdateConfig,
        };

        /// <summary>
        /// The cached method infos for the supported configuration editor.
        /// </summary>
        private readonly Dictionary<EditorMethods, MethodInfo> editorMethodInfos =
            new Dictionary<EditorMethods, MethodInfo>();

        /// <summary>
        /// The cached type of the config type defined to contain the configuration settings.
        /// </summary>
        private readonly Type configType;

        /// <summary>
        /// The cached instance of the supported configuration editor.
        /// </summary>
        private readonly object configEditor;

        /// <summary>
        /// The name of the supported configuration.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="SupportedConfigurationInfo"/>.
        /// </summary>
        /// <param name="name">The name of the supported configuration.</param>
        /// <param name="configTypeAssembly">The assembly name that defines the configuration type.</param>
        /// <param name="configTypeName">The type name defined for the supported configuration.</param>
        /// <param name="editorTypeName">The type name defined for the supported configuration editor.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when failed to find the configuration type, or to find the configuration editor, or to
        /// initialize an instance of the configuration editor, or to find the methods for the configuration
        /// editor through the reflection.
        /// </exception>
        public SupportedConfigurationInfo(string name, string configTypeAssembly,
            string configTypeName, string editorTypeName)
        {
            Name = name;

            // Reflect the config type.
            configType = string.IsNullOrEmpty(configTypeAssembly)
                ? AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(type => type.Name == configTypeName)
                : Assembly.Load(configTypeAssembly)
                    .GetExportedTypes()
                    .FirstOrDefault(type => type.Name == configTypeName);
            if(configType == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Failed to find the configuration type [{0}].", configTypeName));
            }

            // Reflect the editor type.
            var editorType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.Name == editorTypeName);
            if(editorType == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Failed to find the configuration editor [{0}].", editorTypeName));
            }

            // Create an instance of the editor.
            configEditor = Activator.CreateInstance(editorType);
            if(configEditor == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Failed to initialize an instance of the configuration editor class [{0}].", editorTypeName));
            }

            // Reflect the method infos of the editor.
            foreach(EditorMethods id in Enum.GetValues(typeof(EditorMethods)))
            {
                var method = editorType.GetMethod(id.ToString());
                if(method == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "Failed to find the method [{0}] in the configuration editor class [{1}].",
                        id, editorTypeName));
                }
                editorMethodInfos.Add(id, method);
            }
        }

        /// <summary>
        /// Invokes the UpdateConfig method in the corresponding configuration editor so that the UI
        /// can be updated.
        /// </summary>
        public void InvokeEditorUpdateConfig(string configId)
        {
            var methodinfo = editorMethodInfos[EditorMethods.UpdateConfig];
            if(methodinfo == null) return;

            methodinfo.Invoke(configEditor, new object[]{configId});
        }

        /// <summary>
        /// Exports the configuration settings from the corresponding configuration editor and serializes
        /// to XML element.
        /// </summary>
        /// <returns>The serialized configuration settings in format of XML element.</returns>
        public XElement SerializeConfig()
        {
            var methodinfo = editorMethodInfos[EditorMethods.ExportConfig];
            var config = (methodinfo == null) ? null : methodinfo.Invoke(configEditor, null);
            if(config == null) return null;

            using(var memoryStream = new MemoryStream())
            using(var streamWriter = new StreamWriter(memoryStream))
            {
                var serializer = new XmlSerializer(configType, new XmlRootAttribute(Name));

                // Don't want any namespace info coming with the generated XML.
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                serializer.Serialize(streamWriter, config, ns);

                // Return the serialized XElement piece.
                return XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
            }
        }

        /// <summary>
        /// Deserializes the configuration settings from XML element and imports into the corresponding
        /// configuration editor.
        /// </summary>
        /// <param name="element">The XML element to deserialize from.</param>
        public void DeserializeConfig(XElement element)
        {
            if(element == null) return;

            var serializer = new XmlSerializer(configType, new XmlRootAttribute(element.Name.ToString()));
            var config = serializer.Deserialize(element.CreateReader());

            var methodinfo = editorMethodInfos[EditorMethods.ImportConfig];
            if(methodinfo == null) return;

            methodinfo.Invoke(configEditor, new[] {config});
        }
    }
}
