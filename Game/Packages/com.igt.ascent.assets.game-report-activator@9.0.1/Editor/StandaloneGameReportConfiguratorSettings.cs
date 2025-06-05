//-----------------------------------------------------------------------
// <copyright file = "StandaloneGameReportConfiguratorSettings.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.SDK.GameReport.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using UnityEngine;
    using F2XRegistryVerTip = Game.Core.Registries.Internal.F2X.F2XRegistryVer1;
    using F2XReportRegistryVerTip = Game.Core.Registries.Internal.F2X.F2XReportRegistryVer2;

    /// <summary>
    /// The standalone game report configuration settings required for a standalone game report generation.
    /// </summary>
    public class StandaloneGameReportConfiguratorSettings
    {
        /// <summary>
        /// The assembly file extension.
        /// </summary>
        public const string AssemblyFileExtension = ".dll";

        private static readonly XmlSerializer SettingsXmlSerializer =
            new XmlSerializer(typeof(StandaloneGameReportConfiguratorSettings));

        /// <summary>
        /// The file name to save the settings to.
        /// </summary>
        private const string ConfigurationFileName = "StandaloneGameReportConfigurationSettings.xml";

        /// <summary>
        /// Relative path to the Registries directory.
        /// </summary>
        private const string RegistriesPath = "Registries";

        /// <summary>
        /// Report registry file search pattern.
        /// </summary>
        private const string ReportRegistryFilePattern = "*.xreportreg";

        private const string AssemblyNameArgument = "AssemblyName";
        private const string TypeNameArgument = "TypeName";

        private string assemblyName;
        private string objectName;

        /// <summary>
        /// Gets whether any configuration setting needs to be saved.
        /// </summary>
        [XmlIgnore]
        public bool ConfigurationChanged { get; private set; }

        /// <summary>
        /// Gets whether the report registry is being used and
        /// <see cref="AssemblyName"/> and <see cref="ObjectName"/>
        /// should be read-only.
        /// </summary>
        [XmlIgnore]
        public bool UsingReportRegistry { get; private set; }

        /// <summary>
        /// Gets or sets the game report assembly name.
        /// </summary>
        public string AssemblyName
        {
            get { return assemblyName; }
            set
            {
                if(!String.IsNullOrEmpty(value))
                {
                    // Add assembly extension
                    if(!value.EndsWith(AssemblyFileExtension))
                    {
                        value += AssemblyFileExtension;
                    }

                    if(assemblyName != value)
                    {
                        assemblyName = value;
                        ConfigurationChanged = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the game report object name.
        /// </summary>
        public string ObjectName
        {
            get { return objectName; }
            set
            {
                if(!String.IsNullOrEmpty(value) && objectName != value)
                {
                    objectName = value;
                    ConfigurationChanged = true;
                }
            }
        }

        /// <summary>
        /// Saves the configuration settings to an XML file in the
        /// game's root directory.
        /// </summary>
        public void Save()
        {
            // Ensure settings file is not read only
            if(File.Exists(ConfigurationFileName))
            {
                var info = new FileInfo(ConfigurationFileName);
                info.IsReadOnly = false;
            }

            using(var writer = new FileStream(ConfigurationFileName, FileMode.Create))
            {
                SettingsXmlSerializer.Serialize(writer, this);
                ConfigurationChanged = false;
            }
        }

        /// <summary>
        /// Loads the configuration settings from the XML file if it exists.
        /// Also loads the report registry file to attempt to get the assembly and object names.
        /// </summary>
        /// <returns>The loaded report settings.</returns>
        public static StandaloneGameReportConfiguratorSettings Load()
        {
            var settings = new StandaloneGameReportConfiguratorSettings();

            if(File.Exists(ConfigurationFileName))
            {
                using(var fileStream = new FileStream(ConfigurationFileName, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        settings =
                            (StandaloneGameReportConfiguratorSettings)SettingsXmlSerializer.Deserialize(fileStream);
                    }
                    catch(XmlException)
                    {
                        Debug.Log("Game report settings have not been written.");
                    }
                }
            }

            settings.LoadReportRegistry();

            // Default to saved state
            settings.ConfigurationChanged = false;
            return settings;
        }

        /// <summary>
        /// Attempts to load the report registry and read the
        /// assembly and object names.
        /// </summary>
        /// <remarks>
        /// If a report registry file exists, any errors will be logged.
        /// If a report registry file does not exist, no action is taken.
        /// </remarks>
        private void LoadReportRegistry()
        {
            try
            {
                var reportRegistryFileName = Directory.GetFiles(RegistriesPath, ReportRegistryFilePattern)
                    .FirstOrDefault();

                if(!string.IsNullOrEmpty(reportRegistryFileName))
                {
                    var reportName = Path.GetFileName(reportRegistryFileName);

                    var reportRegistry =
                        F2XRegistryVerTip.Registry.Load<F2XReportRegistryVerTip.ReportRegistry>(reportRegistryFileName);

                    var assemblyArg = reportRegistry.CommandLineArguments
                        .FirstOrDefault(arg => arg.name == AssemblyNameArgument);
                    var objectArg = reportRegistry.CommandLineArguments
                        .FirstOrDefault(arg => arg.name == TypeNameArgument);

                    if(assemblyArg == null)
                    {
                        throw new InvalidOperationException(
                            string.Format("Cannot find '{0}' in report registry '{1}'.",
                                AssemblyNameArgument,
                                reportName));
                    }

                    if(objectArg == null)
                    {
                        throw new InvalidOperationException(
                            string.Format("Cannot find '{0}' in report registry '{1}'.",
                                TypeNameArgument,
                                reportName));
                    }


                    AssemblyName = assemblyArg.Value;
                    ObjectName = objectArg.Value;

                    UsingReportRegistry = true;
                }
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.ToString());
                UsingReportRegistry = false;
            }
        }
    }
}