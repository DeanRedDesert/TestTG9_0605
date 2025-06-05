//-----------------------------------------------------------------------
// <copyright file = "SystemConfigFileHelper.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class that stores/retrieves the values to/from the SystemConfig.xml file.
    /// </summary>
    internal class SystemConfigFileHelper
    {
        private readonly SetPaytableList paytableList;
        private readonly SetFoundationOwnedSettings foundationOwnedSettings;
        private readonly SetSystemControlledProgressives systemControlledProgressives;
        private readonly SetProgressiveSimulatorSetup progressiveSimulator;
        private readonly SetGameSubMode setGameSubMode;
        private readonly SetTournamentSessionConfiguration setTournamentSessionConfiguration;
        private readonly SetGameLinkConfiguration setGameLinkConfiguration;
        private string displayErrorMessage = string.Empty;
        private string writeFileErrorMessage = null;
        private bool isPaytableUpdated;
        private readonly string systemConfigEditorFile;
        private const string SystemConfigFile = "SystemConfig.xml";
        private const string SystemConfigurationDirectory = @"Temp\SystemConfiguration";

        /// <summary>
        /// Stores all the values from individual classes to a common SystemConfigurations.
        /// </summary>
        /// <param name="setPaytableList">Paytable List</param>
        /// <param name="setFoundationOwnedSettings">Foundation Owned Settings</param>
        /// <param name="setSystemControlledProgressives">System Controlled Progressives</param>
        /// <param name="setProgressiveSimulatorSetup">Progressive Simulator setup.</param>
        /// <param name="setGameSubMode">The game sub mode for the current game.</param>
        /// <param name="setTournamentSessionConfiguration">The tournament session configuration for the tournament game sub-mode.</param>
        /// <param name="setGameLinkConfiguration">The GameLink configuration for the STOMP broker.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="setPaytableList"/> or <paramref name="setFoundationOwnedSettings"/> or
        /// <paramref name="setGameSubMode"/> or <paramref name="setTournamentSessionConfiguration"/> is null.
        /// </exception>
        public SystemConfigFileHelper(SetPaytableList setPaytableList,
            SetFoundationOwnedSettings setFoundationOwnedSettings,
            SetSystemControlledProgressives setSystemControlledProgressives,
            SetProgressiveSimulatorSetup setProgressiveSimulatorSetup,
            SetGameSubMode setGameSubMode,
            SetTournamentSessionConfiguration setTournamentSessionConfiguration,
            SetGameLinkConfiguration setGameLinkConfiguration)
        {
            if(setPaytableList == null)
            {
                throw new ArgumentNullException("setPaytableList");
            }
            if(setFoundationOwnedSettings == null)
            {
                throw new ArgumentNullException("setFoundationOwnedSettings");
            }
            if(setSystemControlledProgressives == null)
            {
                throw new ArgumentNullException("setSystemControlledProgressives");
            }
            if(setGameSubMode == null)
            {
                throw new ArgumentNullException("setGameSubMode");
            }
            if(setTournamentSessionConfiguration == null)
            {
                throw new ArgumentNullException("setTournamentSessionConfiguration");
            }
            if(setGameLinkConfiguration == null)
            {
                throw new ArgumentNullException("setGameLinkConfiguration");
            }
            if (setProgressiveSimulatorSetup == null)
            {
                throw new ArgumentNullException("setProgressiveSimulatorSetup");
            }
            paytableList = setPaytableList;
            systemControlledProgressives = setSystemControlledProgressives;
            foundationOwnedSettings = setFoundationOwnedSettings;
            progressiveSimulator = setProgressiveSimulatorSetup;
            this.setGameSubMode = setGameSubMode;
            this.setTournamentSessionConfiguration = setTournamentSessionConfiguration;
            this.setGameLinkConfiguration = setGameLinkConfiguration;
            var currentDirectory = Directory.GetCurrentDirectory();
            var systemConfigDirectory = Path.Combine(currentDirectory, SystemConfigurationDirectory);

            if(!Directory.Exists(systemConfigDirectory))
            {
                Directory.CreateDirectory(systemConfigDirectory);
            }

            systemConfigEditorFile = Path.Combine(systemConfigDirectory, SystemConfigFile);

            // Copy SystemConfig.xml from main game folder to SystemConfiguration folder the first time the editor is loaded.
            // This will ensure that the values are retained when the project is opened for the first time.
            if(!File.Exists(systemConfigEditorFile) && File.Exists(SystemConfigFile))
            {
                File.Copy(SystemConfigFile, systemConfigEditorFile);
            }
        }

        /// <summary>
        /// Displays editor messages related to saving the configuration file.
        /// </summary>
        public void Display()
        {
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(SystemConfigEditor.BoxSize));

            CheckErrorMessages();
            if(displayErrorMessage != string.Empty)
            {
                EditorGUILayout.HelpBox(displayErrorMessage, MessageType.Error);
            }

            CheckErrorMessages();
            if(displayErrorMessage != string.Empty)
            {
                EditorGUILayout.HelpBox(displayErrorMessage, MessageType.Error);
            }

            if(GUILayout.Button(new GUIContent("Save Configuration", "Save contents to XML file"),
               GUILayout.MaxWidth(SystemConfigEditor.BoxSize)))
            {
                writeFileErrorMessage = string.Empty;
                try
                {
                    if(!WriteToFile())
                    {
                        writeFileErrorMessage = string.Format("Unable to write {0} file.", SystemConfigFile);
                    }
                }
                catch(Exception exception)
                {
                    writeFileErrorMessage =
                        string.Format(
                            "There was an unhandled exception of type {1} when trying to save the system configuration. Error: {0}",
                            exception.Message, exception.GetType().Name);
                }
            }

            if(writeFileErrorMessage == string.Empty)
            {
                EditorGUILayout.HelpBox("Write Complete without errors", MessageType.Info);
            }
            else if(writeFileErrorMessage != null)
            {
                EditorGUILayout.HelpBox(writeFileErrorMessage, MessageType.Error);
            }

            if(GUILayout.Button(new GUIContent("Clear Editor Cache", "Clears editor system config cache and reloads from the root system config file."),
                    GUILayout.MaxWidth(SystemConfigEditor.BoxSize)))
            {
                if(File.Exists(SystemConfigFile))
                {
                    File.Copy(SystemConfigFile, systemConfigEditorFile, true);
                    LoadSetsFromFile();
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Saves the Xml settings file.
        /// </summary>
        /// <returns>True if the file was saved successfully.</returns>
        private bool WriteToFile()
        {
            var systemConfigData = new SystemConfigurations
            {
                PaytableList = paytableList.Data,
                FoundationOwnedSettings = foundationOwnedSettings.Data,
                SystemControlledProgressives = systemControlledProgressives.Data,
                GameSubModeType = setGameSubMode.Data,
                TournamentSessionConfiguration =
                    setGameSubMode.Data.Type == GameSubModeString.Tournament
                        ? setTournamentSessionConfiguration.Data
                        : null,
                StompBrokerConfiguration = setGameLinkConfiguration.Data,
                SystemProgressiveSimulator = progressiveSimulator.Data
            };

            if(!WriteToFile(SystemConfigFile, systemConfigData))
            {
                return false;
            }

            var systemConfigEditorData = new SystemConfigurations
            {
                PaytableList = paytableList.Data,
                FoundationOwnedSettings = foundationOwnedSettings.EditorData,
                SystemControlledProgressives = systemControlledProgressives.EditorData,
                GameSubModeType = setGameSubMode.Data,
                TournamentSessionConfiguration = setTournamentSessionConfiguration.EditorData,
                StompBrokerConfiguration = setGameLinkConfiguration.EditorData,
                SystemProgressiveSimulator = progressiveSimulator.EditorData
            };

            if(!WriteToFile(systemConfigEditorFile, systemConfigEditorData))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stores values into the XML file.
        /// </summary>
        /// <param name="fileName">
        /// The xml file name where the values are to be written.
        /// </param>
        /// <param name="systemConfigurationData">
        /// The <see cref="SystemConfigurations"/> value to be written in the file.
        /// </param>
        /// <returns>True if the file was written</returns>
        private static bool WriteToFile(string fileName, SystemConfigurations systemConfigurationData)
        {
            if(File.Exists(fileName))
            {
                var attributes = File.GetAttributes(fileName);
                if((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    var result = EditorUtility.DisplayDialog("Read Only File",
                        string.Format("The file {0} is currently marked as read only. Do you want to save anyways?",
                            fileName), "Yes", "No");
                    if(result)
                    {
                        File.SetAttributes(fileName, attributes & ~FileAttributes.ReadOnly);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            using(var streamWriter = new StreamWriter(fileName, false))
            {
                var serializer = new XmlSerializer(typeof(SystemConfigurations));
                serializer.Serialize(streamWriter, systemConfigurationData);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return true;
        }

        /// <summary>
        /// Loads values into SystemConfigurations from the XML settings file.
        /// </summary>
        public void LoadSetsFromFile()
        {
            var serializer = new XmlSerializer(typeof(SystemConfigurations));
            if(File.Exists(SystemConfigFile))
            {
                using(var reader = new FileStream(SystemConfigFile, FileMode.Open, FileAccess.Read))
                {
                    SystemConfigurations systemConfigurations;
                    try
                    {
                        systemConfigurations = serializer.Deserialize(reader) as SystemConfigurations;
                    }
                    catch
                    {
                        File.Delete(SystemConfigFile);
                        throw new InvalidDataException(string.Format("Could not read {0} - file has been deleted.", SystemConfigFile));
                    }

                    if(systemConfigurations == null)
                    {
                        throw new NullReferenceException(string.Format("Could not load {0}", SystemConfigFile));
                    }

                    //Checking for contents in the "SystemConfig.xml" is important to set the "Supported"
                    //flags in the editor.
                    if(systemConfigurations.FoundationOwnedSettings != null)
                    {
                        foundationOwnedSettings.SetSupportedFlag();
                    }
                    if(systemConfigurations.SystemControlledProgressives != null)
                    {
                        systemControlledProgressives.SetSupportedFlag();
                    }
                    if(systemConfigurations.StompBrokerConfiguration != null)
                    {
                        setGameLinkConfiguration.SetSupportedFlag();
                    }
                    if (systemConfigurations.SystemProgressiveSimulator != null)
                    {
                        progressiveSimulator.SetSupportedFlag();
                    }
                    reader.Close();
                }
            }

            if(File.Exists(systemConfigEditorFile))
            {
                using(var reader = new FileStream(systemConfigEditorFile, FileMode.Open, FileAccess.Read))
                {
                    SystemConfigurations systemConfigurations;
                    try
                    {
                        systemConfigurations = serializer.Deserialize(reader) as SystemConfigurations;
                    }
                    catch
                    {
                        File.Delete(systemConfigEditorFile);
                        throw new InvalidDataException(string.Format("Could not read {0} - file has been deleted.", systemConfigEditorFile));
                    }

                    if(systemConfigurations == null)
                    {
                        throw new NullReferenceException(string.Format("Could not load {0}", systemConfigEditorFile));
                    }
                    if(systemConfigurations.PaytableList != null)
                    {
                        paytableList.UpdatePaytableList(systemConfigurations.PaytableList);
                    }
                    if(systemConfigurations.FoundationOwnedSettings != null)
                    {
                        foundationOwnedSettings.UpdateFoundation(systemConfigurations.FoundationOwnedSettings);
                    }
                    if(systemConfigurations.SystemControlledProgressives != null)
                    {
                        systemControlledProgressives.UpdateSystemControlledProgressives(
                            systemConfigurations.SystemControlledProgressives);
                    }
                    if(systemConfigurations.GameSubModeType != null)
                    {
                        setGameSubMode.UpdateGameSubModeWithEditorData(systemConfigurations.GameSubModeType);
                    }
                    if(systemConfigurations.TournamentSessionConfiguration != null)
                    {
                        setTournamentSessionConfiguration.UpdateTournamentSessionConfigurationEditorData(
                            systemConfigurations.TournamentSessionConfiguration);
                    }
                    if(systemConfigurations.StompBrokerConfiguration != null)
                    {
                        setGameLinkConfiguration.UpdateGameLinkConfiguration(
                            systemConfigurations.StompBrokerConfiguration);
                    }
                    if(systemConfigurations.SystemProgressiveSimulator != null)
                    {
                        progressiveSimulator.UpdateProgressiveSimulatorSettings(systemConfigurations.SystemProgressiveSimulator);
                    }
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// Checks for error messages.
        /// </summary>
        private void CheckErrorMessages()
        {
            displayErrorMessage = string.Empty;
            var errorMessage = string.Empty;
            errorMessage += paytableList.CheckErrors();
            errorMessage += systemControlledProgressives.CheckErrors();

            if(errorMessage != string.Empty)
            {
                displayErrorMessage = "Errors: " + errorMessage;
            }
        }
    }
}
