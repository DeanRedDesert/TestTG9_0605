using System;
using System.IO;
using System.Linq;
using System.Text;
using Midas.Core.Configuration;
using Midas.Logging;
using Midas.Presentation.Data;
using UnityEngine;
using IgtUnityEngine;
using Midas.Core.StateMachine;

namespace Midas
{
	public sealed partial class GamingSystem
	{
		private const string StatusDatabaseDumpFileName = "StatusDatabase.SystemError.Dump.txt";
		private const string StateMachineDumpFileName = "StateMachine.SystemError.Dump.txt";
		private const string ScreenShotFileNamePrefix = "Screenshot.SystemError.{0}.png";

		private static void OnUnityLogMessage(string logString, string stackTrace, LogType type)
		{
			if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
				Log.Instance.Fatal(BuildLogEntry());

			string BuildLogEntry()
			{
				var logEntryBuilder = new StringBuilder(512);

				logEntryBuilder.AppendLine(string.Concat("== ", type, " ", new string('=', 76 - type.ToString().Length)));

				logEntryBuilder.AppendLine(logString);

				if (!string.IsNullOrEmpty(stackTrace))
				{
					logEntryBuilder.AppendLine("StackTrace:");
					logEntryBuilder.AppendLine(stackTrace);
				}

				return logEntryBuilder.ToString();
			}
		}

		private static void OnError(string errorMessage)
		{
			// Capture the contents of the status database.

			var subFolder = CreateSubFolder();

			try
			{
				using var tf = File.Create(Path.Combine(subFolder, StatusDatabaseDumpFileName), 1, FileOptions.WriteThrough);
				{
					using var sw = new StreamWriter(tf);
					sw.Write(StatusDatabase.GetJsonDump(true));
				}
			}
			catch (Exception e)
			{
				Log.Instance.Warn("Status database capture failed", e);
			}

			// Capture current state of all state machines

			try
			{
				using var tf = File.Create(Path.Combine(subFolder, StateMachineDumpFileName), 1, FileOptions.WriteThrough);
				{
					using var sw = new StreamWriter(tf);
					sw.Write(StateMachines.GetJsonDump(true));
				}
			}
			catch (Exception e)
			{
				Log.Instance.Warn("State machine capture failed", e);
			}

			// Capture screenshots

			try
			{
				if (!Application.isEditor)
					CreateSystemErrorScreenshots(subFolder);
			}
			catch (Exception e)
			{
				Log.Instance.Warn("Error log screen capture failed", e);
			}

			string CreateSubFolder()
			{
				var folder = DateTime.Now.ToString("yyyyMMdd-HHmmss");

				var s = Path.Combine(Factory.LogDirectory, folder);
				var index = 0;
				while (Directory.Exists(s))
				{
					s = Path.Combine(Factory.LogDirectory, folder, index.ToString());
					index++;
				}

				Directory.CreateDirectory(s);
				return s;
			}
		}

		private static void CreateSystemErrorScreenshots(string folder)
		{
			// Try to make screenshots on the machine and in standalone. Not in EditorMode, as the screenshots are not correct
			foreach (var display in GetEnabledDisplays())
			{
				var texture = ScreenCapture.CaptureScreenshotAsTexture((uint)display);
				if (texture != null)
				{
					var bytes = texture.EncodeToPNG();
					using var tf = File.Create(Path.Combine(folder, string.Format(ScreenShotFileNamePrefix, display)), 1, FileOptions.WriteThrough);
					tf.Write(bytes, 0, bytes.Length);
				}
			}
		}

		private static int[] GetEnabledDisplays()
		{
			// Make sure, the ConfiguredMonitors is available, else use the active Display.displays.
			if (StatusDatabase.ConfigurationStatus != null && StatusDatabase.ConfigurationStatus.ConfiguredMonitors.Count > 0)
			{
				return StatusDatabase.ConfigurationStatus.ConfiguredMonitors
					.Select(GetUnityMonitorIndex)
					.ToArray();
			}

			return Display.displays.Select((display, index) => (display, index))
				.Where(v => v.display.active)
				.Select(v => v.index)
				.ToArray();
		}

		private static int GetUnityMonitorIndex(MonitorType monitorType)
		{
			switch (monitorType)
			{
				case MonitorType.MainPortrait:
				case MonitorType.MainStandard:
					return (int)MonitorRole.Main;
				case MonitorType.TopPortrait:
				case MonitorType.TopStandard:
					return (int)MonitorRole.Top;
				case MonitorType.ButtonPanelPortrait:
				case MonitorType.ButtonPanelStandard:
					return (int)MonitorRole.ButtonPanel;
				case MonitorType.TopperPortrait:
				case MonitorType.TopperStandard:
					return (int)MonitorRole.Topper;
				default:
					// Throw an exception when MonitorType defined a new monitor that is unknown to UnityMonitorRole.
					throw new ApplicationException($"Cannot map {monitorType} to a UnityMonitorRole.");
			}
		}
	}
}