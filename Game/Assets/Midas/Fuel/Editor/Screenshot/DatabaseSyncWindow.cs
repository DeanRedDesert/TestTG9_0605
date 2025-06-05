using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Midas.Fuel.Editor.Screenshot
{
	/// <summary>
	/// Used to show a window indicating that the database is in the process of syncing
	/// </summary>
	public sealed class DatabaseSyncWindow : EditorWindow
	{
		public struct ButtonDef
		{
			/// <summary>
			/// Action to call when button is pressed.
			/// </summary>
			public Action PressAction;

			/// <summary>
			/// Function returning whether or not to show button.
			/// </summary>
			public Func<bool> Show;

			/// <summary>
			/// Text to display on the button.
			/// </summary>
			public string Text;
		}

		/// <summary>
		/// Data about the status of the sync.
		/// </summary>
		private static SyncData syncStatus;

		/// <summary>
		/// Handle to the window
		/// </summary>
		public static EditorWindow WindowHandle;

		/// <summary>
		/// Get or set the text that is displayed at the top of the progress window.
		/// </summary>
		public static string HeadingText { get; set; }

		/// <summary>
		/// Action to call in order to cancel the sync process.
		/// </summary>
		private static readonly List<ButtonDef> buttonDefs = new List<ButtonDef>();

		/// <summary>
		/// Show the window.
		/// </summary>
		/// <param name="syncData">Data to show with the window.</param>
		public static void ShowWindow(SyncData syncData)
		{
			buttonDefs.Clear();
			WindowHandle = GetWindow(typeof(DatabaseSyncWindow), true, "In Progress");
			syncStatus = syncData;
		}

		/// <summary>
		/// Show the window.
		/// </summary>
		/// <param name="syncData">Data to show with the window.</param>
		/// <param name="buttonDef"><see cref="ButtonDef"/> of button to show in Window.</param>
		public static void ShowWindow(SyncData syncData, ButtonDef buttonDef)
		{
			ShowWindow(syncData);

			buttonDefs.Add(buttonDef);
		}

		/// <summary>
		/// Hide the window
		/// </summary>
		public static void HideWindow()
		{
			HeadingText = string.Empty;

			if (WindowHandle != null)
			{
				WindowHandle.Close();
			}
		}

		public void OnGUI()
		{
			if (syncStatus != null)
			{
				var progressPercentage = syncStatus.Progress == 0 ? "~" : syncStatus.Progress.ToString();

				GUILayout.Label(HeadingText, EditorStyles.boldLabel);
				GUILayout.Label(syncStatus.Status, EditorStyles.boldLabel);
				GUILayout.Label($"Progress: {progressPercentage}%", EditorStyles.boldLabel);

				GUILayout.FlexibleSpace();

				foreach (var buttonDef in buttonDefs)
				{
					if (buttonDef.Show())
					{
						if (GUILayout.Button(buttonDef.Text))
						{
							buttonDef.PressAction();
						}
					}
				}
			}
		}

		public void Update()
		{
			// Make sure the sync data is being drawn anytime the window is updated.
			// If this is not done explicitly, the display seems to skip a lot of updates during syncing.
			Repaint();
		}

		public void OnInspectorUpdate()
		{
			Repaint();
		}

		public void OnDestroy()
		{
			syncStatus.RunningScreenshots = false;
		}
	}
}