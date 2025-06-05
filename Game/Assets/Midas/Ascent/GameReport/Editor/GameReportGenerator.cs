using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using IGT.Ascent.SDK.GameReport.Editor;
using IGT.Game.Core.Communication;
using UnityEditor;
using UnityEngine;

namespace Midas.Ascent.GameReport.Editor
{
	/// <summary>
	/// An editor window that is used to configure the settings required to run a Standalone game report.
	/// </summary>
	public sealed class GameReportConfigurator : EditorWindow
	{
		#region Constants

		private const float MinWindowWidth = 700f;
		private const float MinWindowHeight = 450f;
		private const float TextFieldWidth = 550f;
		private const float TextFieldHeight = 20f;
		private const float ButtonWidth = 300f;
		private const float ButtonHeight = 25f;
		private const string AssemblyFileExtension = StandaloneGameReportConfiguratorSettings.AssemblyFileExtension;
		private const string GameReportPattern = "GameReport*.txt";
		private const string GameHtmlReportPattern = "GameReport*.html";
		private const string GameLevelAwardReportFileNameSearchPattern = "GameLevelAwardReport*.txt";
		private const string SetupValidationReportFileNameFormat = "SetupValidationReport.txt";
		private const string GamePerformanceReportFileName = "GamePerformanceReport.txt";
		private const string LogFileNameSuffix = "-GameReportLog";

		#endregion

		#region Fields

		private static StandaloneGameReportConfiguratorSettings settings;
		private static readonly string targetedFoundationPattern = $"\"TargetedFoundation\"\\s*:\\s*\"(?<{FoundationGroupName}>\\w+)\"";
		private string messageString = string.Empty;
		private Vector2 scrollPos = Vector2.zero;

		#endregion

		#region Public

		/// <summary>
		/// Add a standalone game report option to the Window menu.
		/// </summary>
		[MenuItem("Midas/Configuration/Game Report")]
		public static void Initialize()
		{
			var window = GetWindow(typeof(GameReportConfigurator), false, "Game Report");
			window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
			ReadGameReportConfigurationFromFile();
		}

		#endregion

		#region Private

		private const string FoundationGroupName = "Foundation";
		private const string StandaloneGameParametersConfigPath = @"..\..\GameParameters.config";

		private void OnGUI()
		{
			// Sometimes this gets cleared when switching windows while Unity is importing new assets.
			if (settings == null)
				ReadGameReportConfigurationFromFile();

			GUILayout.Label("Standalone Game Report", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginVertical(GUI.skin.box);

			GUILayout.Space(5);
			DisplayAssemblyName();

			GUILayout.Space(25);
			DisplayObjectName();

			GUILayout.Space(25);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(150);
			DisplayGenerateReportButton();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(25);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(150);
			DisplaySaveButton();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(35);
			EditorGUILayout.EndVertical();

			DisplayMessageBox();
			GUILayout.Space(10);

			EditorGUILayout.EndVertical();
		}

		private static void DisplayAssemblyName()
		{
			const string toolTip = "The game report assembly name with the " + AssemblyFileExtension + " file extension.";

			EditorGUILayout.LabelField(new GUIContent("Game Report Assembly Name", toolTip));

			var assemblyName = settings.AssemblyName;

			if (settings.UsingReportRegistry)
				EditorGUILayout.LabelField(new GUIContent(assemblyName, "Loaded from report registry"));
			else
				assemblyName = EditorGUILayout.TextField(assemblyName, GUILayout.Width(TextFieldWidth), GUILayout.Height(TextFieldHeight));

			settings.AssemblyName = assemblyName;
		}

		private static void DisplayObjectName()
		{
			const string toolTip = "The namespace of the game object name and the game object name (ex. Midas.GameReport.GameReport).";

			EditorGUILayout.LabelField(new GUIContent("Game Report Object Name (with namespace)", toolTip));

			if (settings.UsingReportRegistry)
				EditorGUILayout.LabelField(new GUIContent(settings.ObjectName, "Loaded from report registry"));
			else
				settings.ObjectName = EditorGUILayout.TextField(settings.ObjectName, GUILayout.Width(TextFieldWidth), GUILayout.Height(TextFieldHeight));
		}

		private void DisplayGenerateReportButton()
		{
			const string buttonToolTip = "Generate a game report in Standalone mode using the current configuration settings specified in this editor window.";

			// Build the game report object when the button has been clicked.
			// Create a button using a fixed layout. It is a rectangle positioned at
			// Rect(left, top, width, height).
			if (GUILayout.Button(new GUIContent("GENERATE REPORT", buttonToolTip), GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)))
			{
				messageString = string.Empty;

				try
				{
					GenerateGameReport();
				}
				catch (Exception exception)
				{
					messageString = exception.ToString();
				}
			}
		}

		private static void DisplaySaveButton()
		{
			const string buttonToolTip = "Save the current configuration settings to StandaloneGameReportConfigurationSettings.xml under the " +
				"root project directory of the game.";

			// Only enable the button if any setting has been changed.
			GUI.enabled = settings.ConfigurationChanged;

			// Save the settings to an XML file
			if (GUILayout.Button(new GUIContent("SAVE SETTINGS", buttonToolTip), GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)))
				settings.Save();

			GUI.enabled = true;
		}

		private void DisplayMessageBox()
		{
			const string toolTip = "The message being displayed when the game report gets generated.";

			EditorGUILayout.BeginVertical();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			EditorGUILayout.LabelField(new GUIContent(messageString, toolTip), EditorStyles.wordWrappedLabel);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private static void ReadGameReportConfigurationFromFile() => settings = StandaloneGameReportConfiguratorSettings.Load();

		private static string ParseGameParametersFile(string gameParametersConfigPath)
		{
			string parametersFoundationTarget = null;

			// Load GameParameters file relative to the GameReportActivator.exe
			var reportActivatorPath = Path.GetDirectoryName(typeof(GameReportConfigurator).Assembly.Location);

			if (string.IsNullOrEmpty(reportActivatorPath) || !Directory.Exists(reportActivatorPath))
				throw new DirectoryNotFoundException($"Cannot find path to GameReportActivator: '{reportActivatorPath}'.");

			var gameParametersFileName = Path.Combine(reportActivatorPath, gameParametersConfigPath);

			if (!File.Exists(gameParametersFileName))
				throw new FileNotFoundException($"Cannot find the file: '{gameParametersFileName}'.");

			using (var reader = new StreamReader(gameParametersFileName))
			{
				while (!reader.EndOfStream)
				{
					var input = reader.ReadLine();
					if (!string.IsNullOrEmpty(input))
					{
						var match = Regex.Match(input, targetedFoundationPattern);
						if (match.Success)
						{
							parametersFoundationTarget = match.Groups[FoundationGroupName].Value;
							break;
						}
					}
				}
			}

			if (string.IsNullOrEmpty(parametersFoundationTarget))
				throw new InvalidOperationException("Cannot locate Foundation Target in Game Parameters file.");

			return parametersFoundationTarget;
		}

		private static bool RunGameReport(
			string assemblyName, //g0
			string reportTypeName, //g1
			string developmentDirectory, //d
			bool isStandalone,
			string captureDirectory) //c
		{
			var isDevelopment = !string.IsNullOrEmpty(developmentDirectory);

			// Development mode only.
			if (isDevelopment && !Directory.Exists(developmentDirectory))
				// Create the directory if needed.
				// Files won't be created if the directory does not exist.
				Directory.CreateDirectory(developmentDirectory);

			var handle = Activator.CreateInstance(assemblyName, reportTypeName);
			var gameReportObject = handle.Unwrap();
			var gameReportType = gameReportObject.GetType();
			var methodInfo = gameReportType.GetMethod("Run");
			if (methodInfo == null)
				throw new ApplicationException($"Run method is not available in the type {reportTypeName} in the assembly {assemblyName}.");

			var foundationTarget = ParseGameParametersFile(StandaloneGameParametersConfigPath);

			var env = CommandLineArguments.Environment;
			var fieldInfo = env.GetType().GetField("flags", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo != null)
			{
				var flags = (Dictionary<string, string>)fieldInfo.GetValue(env);
				flags["d"] = developmentDirectory;
				flags["c"] = captureDirectory;
				fieldInfo.SetValue(env, flags);
			}

			var backingField = env.GetType().GetField("<CommandLineArgs>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
			if (backingField != null)
			{
				var propArgs = (string[])backingField.GetValue(env);
				propArgs = propArgs.Append($"-g0{assemblyName}").ToArray();
				backingField.SetValue(env, propArgs);
			}

			// Run game report object.
			methodInfo.Invoke(gameReportObject, new object[] { isStandalone, foundationTarget });
			return true;
		}

		private void GenerateGameReport()
		{
			var assemblyName = Path.GetFileNameWithoutExtension(settings.AssemblyName);
			// Quote the string to preserve the paths with spaces.
			var success = RunGameReport(assemblyName, settings.ObjectName, ".", true, Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
			if (!success)
			{
				ShowReportLog("An error has occurred when generating the report.");
				return;
			}

			// Get generated report file names
			var searchDirectory = Directory.GetCurrentDirectory();
			var reportFileNames = Directory.GetFiles(searchDirectory, GameReportPattern)
				.Concat(Directory.GetFiles(searchDirectory, GameHtmlReportPattern))
				.Concat(Directory.GetFiles(searchDirectory, GameLevelAwardReportFileNameSearchPattern))
				.Concat(Directory.GetFiles(searchDirectory, SetupValidationReportFileNameFormat))
				.Concat(Directory.GetFiles(searchDirectory, GamePerformanceReportFileName))
				.ToList();

			if (!reportFileNames.Any())
			{
				ShowReportLog("Game report file(s) have not been created.");
				return;
			}

			messageString = "Game report file(s) have been generated successfully. The game report file(s) have been written to:";

			foreach (var reportFileName in reportFileNames)
			{
				Process.Start("notepad.exe", reportFileName);
				messageString += Environment.NewLine + reportFileName;
			}
		}

		private void ShowReportLog(string errorMessage)
		{
			var builder = new StringBuilder(errorMessage);
			var reportLogFileName = GetReportLogFileName(Directory.GetCurrentDirectory(), settings.AssemblyName + LogFileNameSuffix + "*.txt");

			if (!string.IsNullOrEmpty(reportLogFileName))
			{
				var reportLogFile = Path.Combine(Directory.GetCurrentDirectory(), reportLogFileName);

				if (File.Exists(reportLogFile))
				{
					Process.Start("notepad.exe", reportLogFileName);
					builder.AppendLine(" For more details please refer to:").AppendLine(reportLogFileName);
				}
			}

			messageString = builder.ToString();
		}

		private static string GetReportLogFileName(string directory, string filePattern)
		{
			var directoryInfo = new DirectoryInfo(directory);
			var fileInfoList = new List<FileInfo>(directoryInfo.GetFiles(filePattern));
			var fileNames = fileInfoList.OrderByDescending(fileInfo => fileInfo.LastWriteTimeUtc).Select(fileInfo => fileInfo.Name);
			return fileNames.FirstOrDefault();
		}

		#endregion
	}
}