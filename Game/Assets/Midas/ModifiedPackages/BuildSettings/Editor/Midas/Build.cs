using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IGT.Game.SDKAssets.AscentBuildSettings;
using IGT.Game.SDKAssets.AscentBuildSettings.Editor;
using IgtUnityEditor;
using UnityEditor;
using UnityEngine;

namespace IGT.Game.SDKAssets.SDKBuild.Editor
{
	public static class Build
	{
		#region Fields

		private static SettingsToRestore settingsToRestore;
		private static readonly string[] debugScriptDefines = { "DEBUG", "DEBUG_TRACING" };
		private static readonly string[] releaseScriptDefines = { "RELEASE" };

		#endregion

		public static Action OnPreBuild;

		#region Public Methods

		[MenuItem("Midas/Build/Standalone")]
		public static void StandaloneRelease() => BuildStandalone(false, true);

		[MenuItem("Midas/Build/Standalone Development")]
		public static void BuildStandaloneDevelopment() => BuildStandalone(false, false, BuildOptions.Development);

		[MenuItem("Midas/Build/Standalone Dev+Debug")]
		public static void BuildStandaloneDevDebug() => BuildStandalone(false, false, BuildOptions.Development | BuildOptions.AllowDebugging);

		[MenuItem("Midas/Build/Standalone Development with Deep Profiling")]
		public static void BuildStandaloneDevelopmentWithProfiling() => BuildStandalone(false, false, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.EnableDeepProfilingSupport);

		[MenuItem("Midas/Build/EGM")]
		public static void BuildPlatformRelease() => BuildPlatform(false, true);

		[MenuItem("Midas/Build/EGM Development")]
		public static void BuildPlatformDevelopment() => BuildPlatform(false, false, BuildOptions.Development);

		[MenuItem("Midas/Build/EGM Dev+Debug")]
		public static void BuildPlatformDevDebug() => BuildPlatform(false, false, BuildOptions.Development | BuildOptions.AllowDebugging);

		[MenuItem("Midas/Build/EGM Development with Deep Profiling")]
		public static void BuildPlatformDevelopmentWithProfiling() => BuildPlatform(false, false, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.EnableDeepProfilingSupport);

		public static void BuildPlatform(bool fromCommandLine, bool isRelease, BuildOptions userDefinedBuildOptions = BuildOptions.None)
		{
			try
			{
				var targetFolder = fromCommandLine ? "Release" : Path.Combine(isRelease ? "Egm" : "EgmDevelopment", PlayerSettings.productName);
				var buildFolder = Path.Combine("Builds", targetFolder) + Path.AltDirectorySeparatorChar;

				SaveCurrentSettings();

				OnPreBuild?.Invoke();

				UpdatePlayerSettings(FullScreenMode.FullScreenWindow, false);
				UpdateScriptDefines(GetScriptDefinesToAdd(isRelease), GetScriptDefinesToRemove(isRelease));

				IGTEGMSettings.IgtEgmBuildSettings = CreateBuildSettings(isRelease, IgtGameParameters.CreateReleaseParameters(IGTEGMSettings.IgtEgmBuildSettings.GameParameters.TargetedFoundation));

				IGTEGMSettings.BuildPlayer(buildFolder, userDefinedBuildOptions | BuildOptions.StrictMode);

				if (!fromCommandLine)
					Process.Start(new ProcessStartInfo("explorer.exe", $"\"{Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, buildFolder).Replace(Path.AltDirectorySeparatorChar.ToString(), "")}\""));
			}
			finally
			{
				RestoreSettings();
			}
		}

		public static void BuildStandalone(bool fromCommandLine, bool isRelease, BuildOptions userDefinedBuildOptions = BuildOptions.None)
		{
			try
			{
				var targetFolder = fromCommandLine ? "Release" : isRelease ? "Standalone" : "StandaloneDevelopment";
				var buildFolder = Path.Combine("Builds", targetFolder) + Path.AltDirectorySeparatorChar;

				SaveCurrentSettings();

				OnPreBuild?.Invoke();

				UpdatePlayerSettings(FullScreenMode.Windowed, true);
				UpdateScriptDefines(GetScriptDefinesToAdd(isRelease), GetScriptDefinesToRemove(isRelease));

				IGTEGMSettings.IgtEgmBuildSettings = CreateBuildSettings(isRelease, CreateStandaloneGameParameters());

				IGTEGMSettings.BuildPlayer(buildFolder, userDefinedBuildOptions | BuildOptions.StrictMode);

				if (!fromCommandLine)
					Process.Start(new ProcessStartInfo("explorer.exe", $"\"{Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, buildFolder).Replace(Path.AltDirectorySeparatorChar.ToString(), "")}\""));
			}
			finally
			{
				RestoreSettings();
			}
		}

		#endregion

		#region Private Methods

		private static IgtGameParameters CreateStandaloneGameParameters()
		{
			// Do not use an initializer list here, the serialization and constructor will override.
			var parameters = new IgtGameParameters();
			parameters.Type = IgtGameParameters.GameType.StandaloneFileBackedSafeStorage;
			parameters.ShowMouseCursor = true;
			parameters.FitToScreen = true;
			parameters.ToolConnections = IgtGameParameters.ConnectionType.AnyIp;
			return parameters;
		}

		private static IGTEGMBuild CreateBuildSettings(bool isRelease, IgtGameParameters parameters)
		{
			var buildSettings = ScriptableObject.CreateInstance<IGTEGMBuild>();
			buildSettings.BuildType = BuildType.Game;
			buildSettings.GameParameters = parameters;
			buildSettings.Release = isRelease;
			buildSettings.MonoAoTCompile = true;
			return buildSettings;
		}

		private static void UpdatePlayerSettings(FullScreenMode fullScreenMode, bool isResizableWindow)
		{
			PlayerSettings.fullScreenMode = fullScreenMode;
			PlayerSettings.resizableWindow = isResizableWindow;
			PlayerSettings.runInBackground = true;
		}

		private static void UpdateScriptDefines(IReadOnlyList<string> scriptDefinesToAdd, IReadOnlyList<string> scriptDefinesToRemove)
		{
			PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out settingsToRestore.ScriptingDefineSymbols);
			var scriptDefines = settingsToRestore.ScriptingDefineSymbols
				.Where(s => !scriptDefinesToRemove.Contains(s))
				.Concat(scriptDefinesToAdd)
				.Distinct()
				.ToArray();
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, scriptDefines);
		}

		private static void SaveCurrentSettings()
		{
			PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out settingsToRestore.ScriptingDefineSymbols);
			settingsToRestore.IgtPlayerSettingsMachineTargetBuild = IgtPlayerSettings.MachineTargetBuild;
			settingsToRestore.IgtEgmBuildSettings = IGTEGMSettings.IgtEgmBuildSettings;
			settingsToRestore.PlayerSettingsFullScreenMode = PlayerSettings.fullScreenMode;
			settingsToRestore.PlayerSettingsResizableWindow = PlayerSettings.resizableWindow;
			settingsToRestore.PlayerSettingsRunInBackground = PlayerSettings.runInBackground;
		}

		private static void RestoreSettings()
		{
			IgtPlayerSettings.MachineTargetBuild = settingsToRestore.IgtPlayerSettingsMachineTargetBuild;
			PlayerSettings.fullScreenMode = settingsToRestore.PlayerSettingsFullScreenMode;
			PlayerSettings.resizableWindow = settingsToRestore.PlayerSettingsResizableWindow;
			PlayerSettings.runInBackground = settingsToRestore.PlayerSettingsRunInBackground;
			IGTEGMSettings.IgtEgmBuildSettings = settingsToRestore.IgtEgmBuildSettings;
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, settingsToRestore.ScriptingDefineSymbols);
		}

		private static string[] GetScriptDefinesToRemove(bool isRelease) => isRelease ? debugScriptDefines : releaseScriptDefines;

		private static string[] GetScriptDefinesToAdd(bool isRelease) => isRelease ? releaseScriptDefines : debugScriptDefines;

		#endregion

		private struct SettingsToRestore
		{
			public bool IgtPlayerSettingsMachineTargetBuild;
			public IGTEGMBuild IgtEgmBuildSettings;
			public FullScreenMode PlayerSettingsFullScreenMode;
			public bool PlayerSettingsResizableWindow;
			public bool PlayerSettingsRunInBackground;
			public string[] ScriptingDefineSymbols;
		}
	}
}