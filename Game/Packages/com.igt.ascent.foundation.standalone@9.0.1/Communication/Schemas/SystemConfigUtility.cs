//-----------------------------------------------------------------------
// <copyright file = "SystemConfigUtility.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Standalone.Schemas
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using Ascent.Build.EmbeddedResources;

    /// <summary>
    /// Utility Class processing the system configuration file.
    /// </summary>
    public static class SystemConfigUtility
    {
        /// <summary>
        /// Load the system configurations from a stream.
        /// </summary>
        /// <param name="systemConfigStream">
        /// A stream from which the system configurations to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <returns>The root xml element in <paramref name="systemConfigStream"/>.</returns>
        public static XElement LoadSystemConfigFile(Stream systemConfigStream)
        {
            XElement rootElement = null;

            if (systemConfigStream != null)
            {
                Stream schemaStream = null;
                XmlReader xsdReader = null;
                XmlReader reader = null;

                try
                {
                    // Create an XML reader to read the system configuration file.
                    schemaStream =
                        ResourcesAssembly.GetCurrent().GetManifestResourceStream(typeof(SystemConfigUtility),
                                                                                    "SystemConfigFileSchemas.xsd");
                    xsdReader = XmlReader.Create(schemaStream ?? throw new InvalidOperationException("Unable to load \"SystemConfigFileSchemas.xsd\"."));

                    var settings = new XmlReaderSettings
                    {
                        IgnoreComments = true,
                        IgnoreWhitespace = true,
                        ValidationType = ValidationType.Schema
                    };
                    settings.Schemas.Add(null, xsdReader);

                    reader = XmlReader.Create(systemConfigStream, settings);

                    // Read the root element.
                    rootElement = XElement.Load(reader);
                }
                finally
                {
                    schemaStream?.Dispose();

                    ((IDisposable)xsdReader)?.Dispose();

                    ((IDisposable)reader)?.Dispose();
                }
            }

            return rootElement;
        }
    }
}
