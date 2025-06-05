//-----------------------------------------------------------------------
// <copyright file = "StandaloneGameReportConfigurator.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.SDK.GameReport.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// An editor window that is used to configure the settings required to run a Standalone game report.
    /// </summary>
    public class StandaloneGameReportConfigurator : EditorWindow
    {
        #region Constants

        /// <summary>
        /// The minimum window width.
        /// </summary>
        private const float MinWindowWidth = 700f;

        /// <summary>
        /// The minimum window height.
        /// </summary>
        private const float MinWindowHeight = 450f;

        /// <summary>
        /// The width of the text fields.
        /// </summary>
        private const float TextFieldWidth = 550f;

        /// <summary>
        /// The height of the text fields.
        /// </summary>
        private const float TextFieldHeight = 20f;

        /// <summary>
        /// The width of the "generate report" and the "save settings" buttons.
        /// </summary>
        private const float ButtonWidth = 300f;

        /// <summary>
        /// The height of the "generate report" and the "save settings" buttons.
        /// </summary>
        private const float ButtonHeight = 25f;

        /// <summary>
        /// The assembly file extension.
        /// </summary>
        private const string AssemblyFileExtension = StandaloneGameReportConfiguratorSettings.AssemblyFileExtension;

        /// <summary>
        /// The game report file search pattern.
        /// </summary>
        private const string GameReportPattern = "GameReport*.txt";

        /// <summary>
        /// The game html report file search pattern.
        /// </summary>
        private const string GameHtmlReportPattern = "GameReport*.html";

        /// <summary>
        /// Name for the min playable credit balance report file.
        /// </summary>
        private const string MinPlayableCreditBalanceReportFileName = "MinPlayableCreditBalanceReport.txt";

        /// <summary>
        /// Wild card search pattern for the game level award report files.
        /// </summary>
        private const string GameLevelAwardReportFileNameSearchPattern = "GameLevelAwardReport*.txt";

        /// <summary>
        /// Name for the setup validation report file.
        /// </summary>
        private const string SetupValidationReportFileNameFormat = "SetupValidationReport.txt";

        /// <summary>
        /// The game performance data file name.
        /// </summary>
        private const string GamePerformanceReportFileName = "GamePerformanceReport.txt";

        /// <summary>
        /// Suffix to the name of the report log file.
        /// </summary>
        /// <remarks>
        /// Error and build status are written to a log file named "ReportAssemblyName-GameReportLog.txt".
        /// </remarks>
        /// <devdoc>
        /// If changed, this must also be updated in 
        /// GameReportActivator/GameReportActivator/Program.cs,
        /// and Core/GameReport/GameReportLog.cs.
        /// </devdoc>
        private const string LogFileNameSuffix = "-GameReportLog";

        #endregion

        #region Fields

        /// <summary>
        /// The standalone game report configuration settings.
        /// </summary>
        private static StandaloneGameReportConfiguratorSettings settings;

        /// <summary>
        /// The generate report message string.
        /// </summary>
        private string messageString = string.Empty;

        /// <summary>
        /// The scroll position of the message text. The scroll bar is only shown if the message expands over
        /// the fixed window.
        /// </summary>
        private Vector2 scrollPos = Vector2.zero;

        #endregion

        #region Properties

        /// <summary>
        /// The search pattern for report log files.  Example: TiGameReport-GameReportLog1.txt
        /// </summary>
        private string LogFileNamePattern => Path.GetFileNameWithoutExtension(settings.AssemblyName) + LogFileNameSuffix + "*.txt";

        #endregion

        /// <summary>
        /// Add a standalone game report option to the Window menu.
        /// </summary>
        [MenuItem("Tools/Standalone Game Reporting")]
        private static void Initialize()
        {
            // Get existing open window or make a new one if none.
            var configuratorWindow =
                (StandaloneGameReportConfigurator)GetWindow(typeof(StandaloneGameReportConfigurator));

            // Fix the window size.
            configuratorWindow.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            configuratorWindow.maxSize = configuratorWindow.minSize;

            ReadGameReportConfigurationFromFile();
        }

        /// <summary>
        /// Display the standalone game report editor window.
        /// </summary>
        /// <remarks>
        /// The custom editor window can float free or be docked as a tab.
        /// </remarks>
        private void OnGUI()
        {
            // Sometimes this gets cleared when switching windows while Unity is importing new assets.
            if(settings == null)
            {
                ReadGameReportConfigurationFromFile();
            }

            GUILayout.Label("Standalone Game Report Configurator", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Space(5);
            DisplayAssemblyName();

            GUILayout.Space(10);
            DisplayObjectName();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(150);
            DisplayGenerateReportsButton();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(150);
            DisplaySaveSettingsButton();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(150);
            DisplayDeleteLogsButton();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            DisplayMessageBox();
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Display the game report assembly name.
        /// </summary>
        private void DisplayAssemblyName()
        {
            const string toolTip =
                "The game report assembly name with the " + AssemblyFileExtension + " file extension.";

            EditorGUILayout.LabelField(new GUIContent("Game Report Assembly Name", toolTip));

            string assemblyName = settings.AssemblyName;

            if(settings.UsingReportRegistry)
            {
                EditorGUILayout.LabelField(new GUIContent(assemblyName, "Loaded from report registry"));
            }
            else
            {
                assemblyName = EditorGUILayout.TextField(assemblyName,
                                                         GUILayout.Width(TextFieldWidth),
                                                         GUILayout.Height(TextFieldHeight));
            }

            settings.AssemblyName = assemblyName;
        }

        /// <summary>
        /// Display the game report object name.
        /// </summary>
        /// <remarks>
        /// The game report object name should include the game report object namespace.
        /// (i.e. SiberianStormGameReport.SiberianStormGameReport)
        /// </remarks>
        private void DisplayObjectName()
        {
            const string toolTip = "The namespace of the game object name and the game object name " +
                                   "(ex. SiberianStormGameReportNamespace.SiberianStormGameReport).";

            EditorGUILayout.LabelField(new GUIContent("Game Report Object Name (with namespace)", toolTip));

            if(settings.UsingReportRegistry)
            {
                EditorGUILayout.LabelField(new GUIContent(settings.ObjectName, "Loaded from report registry"));
            }
            else
            {
                var objectName = EditorGUILayout.TextField(settings.ObjectName,
                                                           GUILayout.Width(TextFieldWidth),
                                                           GUILayout.Height(TextFieldHeight));

                settings.ObjectName = objectName;
            }
        }

        /// <summary>
        /// Display a generate report button which rebuilds the game report object and generates a Standalone game report.
        /// </summary>
        private void DisplayGenerateReportsButton()
        {
            const string buttonToolTip =
                "Generate all supported reports in Standalone mode using the current configuration settings " +
                "specified in this editor window.";

            // Build the game report object when the button has been clicked.
            // Create a button using a fixed layout. It is a rectangle positioned at
            // Rect(left, top, width, height).
            if(GUILayout.Button(new GUIContent("GENERATE REPORTS", buttonToolTip),
                                GUILayout.Width(ButtonWidth),
                                GUILayout.Height(ButtonHeight)))
            {
                messageString = string.Empty;

                try
                {
                    GenerateGameReport();
                }
                catch(Exception exception)
                {
                    messageString = exception.Message;
                }
            }
        }

        /// <summary>
        /// Display a save button which saves the current settings to a file.
        /// </summary>
        /// <remarks>
        /// The file is saved to the root directory of the game project.
        /// </remarks>
        private void DisplaySaveSettingsButton()
        {
            const string buttonToolTip = "Save the current configuration settings " +
                                         "to StandaloneGameReportConfigurationSettings.xml under the " +
                                         "root project directory of the game.";

            // Only enable the button if any setting has been changed.
            GUI.enabled = settings.ConfigurationChanged;

            // Save the settings to an XML file
            if(GUILayout.Button(new GUIContent("SAVE SETTINGS", buttonToolTip),
                                GUILayout.Width(ButtonWidth),
                                GUILayout.Height(ButtonHeight)))
            {
                settings.Save();
            }

            GUI.enabled = true;
        }

        /// <summary>
        /// Display a delete logs button which deletes all existing log files.
        /// </summary>
        private void DisplayDeleteLogsButton()
        {
            const string buttonToolTip = "Delete all existing game report log files.  " +
                                         "Log files are appended each time reports are generated  " +
                                         "Delete existing log files provides a clean slate for easier debugging";

            var currentDirectory = Directory.GetCurrentDirectory();

            GUI.enabled = Directory.GetFiles(currentDirectory, LogFileNamePattern).Any();

            if(GUILayout.Button(new GUIContent("DELETE LOG FILES", buttonToolTip),
                                GUILayout.Width(ButtonWidth),
                                GUILayout.Height(ButtonHeight)))
            {
                var logFileNames = Directory.GetFiles(currentDirectory, LogFileNamePattern);
                foreach(var fileName in logFileNames)
                {
                    File.Delete(fileName);
                }
            }

            GUI.enabled = true;
        }

        /// <summary>
        /// Display a message on the generate report results.
        /// </summary>
        /// <remarks>
        /// This can be a success or a fail message.
        /// </remarks>
        private void DisplayMessageBox()
        {
            const string toolTip = "The message being displayed when the game report gets generated.";

            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
                                                        GUILayout.Width(MinWindowWidth),
                                                        GUILayout.Height(200));

            EditorGUILayout.LabelField(new GUIContent(messageString, toolTip),
                                       EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Read the settings from a file.
        /// </summary>
        private static void ReadGameReportConfigurationFromFile()
        {
            settings = StandaloneGameReportConfiguratorSettings.Load();
        }

        /// <summary>
        /// Generate a game report.
        /// </summary>
        private void GenerateGameReport()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var assemblyName = Path.GetFileNameWithoutExtension(settings.AssemblyName);
            var assetAssembliesPath = Path.Combine(currentDirectory, @"Assets\Assemblies");

            // Quote the string to preserve the paths with spaces.
            var libraryAssembliesPath = Path.Combine(currentDirectory, @"Library\ScriptAssemblies");
            var referencePath = $"\"{assetAssembliesPath};{libraryAssembliesPath}\"";

            // Start the GameReportActivator process
            var processInfo = new ProcessStartInfo(GameReportActivatorBuilder.GetBuild().ActivatorExecutablePath)
            {
                // Note: "-d." uses the current directory as the development capture directory
                Arguments = $"-g0{assemblyName} -g1{settings.ObjectName} -g2Standalone -g3{referencePath} -d."
            };

            var process = Process.Start(processInfo);

            if(process == null)
            {
                messageString = "Failed to start the game report activator.";
                return;
            }

            process.WaitForExit();

            if(process.ExitCode != 0)
            {
                ShowReportLog("An error has occurred when generating the report.");
                return;
            }

            // Get generated report file names
            var searchDirectory = Directory.GetCurrentDirectory();
            var reportFileNames = Directory.GetFiles(searchDirectory, GameReportPattern)
                                           .Concat(Directory.GetFiles(searchDirectory, GameHtmlReportPattern))
                                           .Concat(Directory.GetFiles(searchDirectory, MinPlayableCreditBalanceReportFileName))
                                           .Concat(Directory.GetFiles(searchDirectory, GameLevelAwardReportFileNameSearchPattern))
                                           .Concat(Directory.GetFiles(searchDirectory, SetupValidationReportFileNameFormat))
                                           .Concat(Directory.GetFiles(searchDirectory, GamePerformanceReportFileName))
                                           .ToList();

            if(!reportFileNames.Any())
            {
                ShowReportLog("Game report file(s) have not been created.");
                return;
            }

            messageString = "Game report file(s) have been generated successfully. " +
                            "The game report file(s) have been written to:";

            // Show each report
            foreach(var reportFileName in reportFileNames)
            {
                // Open the report file using notepad
                Process.Start("notepad.exe", reportFileName);
                messageString += Environment.NewLine + reportFileName;
            }
        }

        /// <summary>
        /// Verifies that the report log file exists and if it does,
        /// shows it to the user using the default application.
        /// </summary>
        /// <param name="errorMessage">Error message to display.</param>
        private void ShowReportLog(string errorMessage)
        {
            var builder = new StringBuilder(errorMessage);

            var reportLogFileName = GetReportLogFileName(Directory.GetCurrentDirectory(), LogFileNamePattern);

            if(!string.IsNullOrEmpty(reportLogFileName))
            {
                var reportLogFile = Path.Combine(Directory.GetCurrentDirectory(), reportLogFileName);

                if(File.Exists(reportLogFile))
                {
                    // Open the log file using notepad
                    Process.Start("notepad.exe", reportLogFileName);
                    builder.AppendLine(" For more details please refer to:")
                           .AppendLine(reportLogFileName);
                }
            }

            messageString = builder.ToString();
        }

        /// <summary>
        /// Gets name of report log file that logs will be written into.
        /// </summary>
        /// <param name="directory">The directory the log files located.</param>
        /// <param name="filePattern">The pattern of the file names to search.</param>
        /// <returns>
        /// The latest log file name.
        /// </returns>
        private string GetReportLogFileName(string directory, string filePattern)
        {
            var directoryInfo = new DirectoryInfo(directory);
            var fileInfoList = new List<FileInfo>(directoryInfo.GetFiles(filePattern));

            var fileNames = from fileInfo in fileInfoList
                            orderby fileInfo.LastWriteTimeUtc descending
                            select fileInfo.Name;

            return fileNames.FirstOrDefault();
        }
    }
}
