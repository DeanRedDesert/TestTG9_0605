using System;
using System.IO;
using Midas.Logging;
using UnityEngine;

namespace Midas.Tools
{
	public static partial class Logging
	{
		private static string RootFolder => Application.isEditor ? Directory.GetCurrentDirectory() : Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) ?? "";
		private static string ApplicationName => Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);

		public static void Init()
		{
			if (!Factory.IsInitialized)
			{
				Debug.Log($"Logging uses ErrorLogDirectory='{ErrorLogDirectory}'");
				Debug.Log($"Logging uses LogDirectory='{LogDirectory}'");
			}

			Factory.InitLogging(Config, LogDirectory, ErrorLogDirectory);
		}

		public static void DeInit()
		{
			Factory.DeInitLogging();
		}

		private static string GetCommandlineValue(string parameter)
		{
			foreach (var argument in Environment.GetCommandLineArgs())
			{
				if (argument.StartsWith(parameter, StringComparison.Ordinal))
					return argument.Substring(parameter.Length);
			}

			return null;
		}

		private static string ErrorLogDirectory
		{
			get
			{
				if (Application.isEditor)
					return Path.Combine(RootFolder, Application.isPlaying ? "Logs" : "EditorLogs");

				var logRoot = GetCommandlineValue("-c");
				return !string.IsNullOrEmpty(logRoot) ? logRoot : Path.Combine(RootFolder, "Logs");
			}
		}

		private static string LogDirectory
		{
			get
			{
				// If it runs from Editor, do not use sub-directory, since we
				// won't be able to get a meaningful executable name.

				if (Application.isEditor)
					return Path.Combine(RootFolder, Application.isPlaying ? "Logs" : "EditorLogs");

				var logRoot = GetCommandlineValue("-d");
				if (!string.IsNullOrEmpty(logRoot))
				{
					var logSubDirectory = ApplicationName + "-DevelopmentLogs";
					return Path.Combine(logRoot, logSubDirectory);
				}

				return ErrorLogDirectory;
			}
		}
	}
}