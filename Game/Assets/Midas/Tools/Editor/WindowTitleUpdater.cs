using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Midas.Tools.Editor
{
	public static class WindowTitleUpdater
	{
		/// <summary>
		///	Called to add an event handler to update the window title.
		/// </summary>
		/// <remarks>
		/// Reflection version of the following since Unity 2020 doesn't have these as public members.
		/// See https://docs.unity3d.com/2023.2/Documentation/ScriptReference/ApplicationTitleDescriptor.html
		/// 
		/// private static void CustomTitleBar(ApplicationTitleDescriptor desc)
		/// {
		/// 	desc.title = $"My Editor Custom Title version: {Random.value}";
		/// }
		///
		/// [MenuItem("Test/Setup custom title bar")]
		/// static void Setup()
		/// {
		/// 	EditorApplication.updateMainWindowTitle -= CustomTitleBar;
		/// 	// This callback will be triggered when a new scene is loaded or when Unity starts.
		/// 	EditorApplication.updateMainWindowTitle += CustomTitleBar;
		/// 	EditorApplication.UpdateMainWindowTitle();
		/// }
		/// </remarks>
		/// <remarks>
		///	The below method is covered by the MIT software license.
		/// </remarks>
		[InitializeOnLoadMethod]
		private static void OnLoad()
		{
			// Get the ApplicationTitleDescriptor Type.
			var tEditorApplication = typeof(EditorApplication);
			var tApplicationTitleDescriptor = tEditorApplication.Assembly.GetTypes().First(x => x.FullName == "UnityEditor.ApplicationTitleDescriptor");

			// Get the event and method type to update the title of the main window.
			var eiUpdateMainWindowTitle = tEditorApplication.GetEvent("updateMainWindowTitle", BindingFlags.Static | BindingFlags.NonPublic);
			var miUpdateMainWindowTitle = tEditorApplication.GetMethod("UpdateMainWindowTitle", BindingFlags.Static | BindingFlags.NonPublic);

			// Convert Action<ApplicationTitleDescriptor> to Action<object>.
			var delegateType = typeof(Action<>).MakeGenericType(tApplicationTitleDescriptor);
			var methodInfo = ((Action<object>)UpdateMainWindowTitle).Method;
			var del = Delegate.CreateDelegate(delegateType, null, methodInfo);

			// Register the event handler and ensure it isn't double added.
			eiUpdateMainWindowTitle?.GetRemoveMethod(true).Invoke(null, new object[] { del });
			eiUpdateMainWindowTitle?.GetAddMethod(true).Invoke(null, new object[] { del });

			// Refresh the title.
			miUpdateMainWindowTitle?.Invoke(null, Array.Empty<object>());
		}

		private static void UpdateMainWindowTitle(object desc)
		{
			try
			{
				var gameFolder = Directory.GetParent(Application.dataPath);
				var rootFolder = gameFolder?.Parent;
				if (gameFolder == null || rootFolder == null)
					return;

				var name = rootFolder.Name;

				var files = gameFolder.GetFiles("*.tgversion");
				var tgVersion = "TG " + files.Length switch
				{
					1 => Path.GetFileNameWithoutExtension(files[0].Name),
					_ => "Unknown"
				};

				var lines = File.ReadAllText(@$"{rootFolder}\GameInfo.json");
				var gameName = ExtractValue(lines, "GameName");
				var gameId = ExtractValue(lines, "GameId");

				// UnityEditor.ApplicationTitleDescriptor.title = "Custom Title";
				typeof(EditorApplication).Assembly.GetTypes()
					.First(x => x.FullName == "UnityEditor.ApplicationTitleDescriptor")
					.GetField("title", BindingFlags.Instance | BindingFlags.Public)
					?.SetValue(desc, $"{name} - {gameName} - {gameId} - {tgVersion} - IGT Unity {Application.unityIGTVersion}");
			}
			catch (Exception e)
			{
				Console.WriteLine("Unable to update window title.");
				Console.WriteLine(e);
			}
		}

		private static string ExtractValue(string data, string key)
		{
			var pattern = $"\"{key}\": \"";
			var startIndex = data.IndexOf(pattern, StringComparison.Ordinal) + pattern.Length;
			var endIndex = data.IndexOf("\"", startIndex, StringComparison.Ordinal);
			return data.Substring(startIndex, endIndex - startIndex);
		}
	}
}