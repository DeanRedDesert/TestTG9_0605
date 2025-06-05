using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugWindowsEnabler : DebugWindow
	{
		private static readonly string[] vSyncStrings =
		{
			"vSync Off",
			"vSync 1",
			"vSync 2",
			"vSync 3",
			"vSync 4"
		};

		private List<DebugWindow> windows = new List<DebugWindow>();
		private Vector2 scrollPosition;
		private IDictionary<int, KeyValuePair<string, Func<string>>> customButtons = new Dictionary<int, KeyValuePair<string, Func<string>>>();
		private int nextCustomButtonId;

		[SerializeField]
		private UnityEvent fpsEvent = null;

		[SerializeField]
		private UnityEvent fpsModeEvent = null;

		public void ToggleEnabled()
		{
			enabled = !enabled;
			if (enabled)
				Cursor.visible = true;
		}

		public void DisableAllWindows()
		{
			windows.ForEach(w => w.enabled = false);
		}

		public int AddCustomButtonFunction(string buttonName, Func<string> callback)
		{
			customButtons[nextCustomButtonId] = new KeyValuePair<string, Func<string>>(buttonName, callback);
			return nextCustomButtonId++;
		}

		public bool RemoveCustomButtonFunction(int customButtonId)
		{
			return customButtons.Remove(customButtonId);
		}

		public bool AddDebugWindow(DebugWindow window)
		{
			if (window == this || windows.Contains(window))
			{
				return false;
			}

			windows.Add(window);
			windows.Sort((a, b) => string.CompareOrdinal(a.WindowName, b.WindowName));
			return true;
		}

		public bool RemoveDebugWindow(DebugWindow window)
		{
			return windows.Remove(window);
		}

		protected override void Reset()
		{
			base.Reset();

			WindowName = "Windows";
			WindowRect = new Rect(0, 115, 380, 1040);
			BackgroundColor = Color.yellow;
		}

		protected override void RenderWindowContent()
		{
			using (var scrollView = new GUILayout.ScrollViewScope(scrollPosition, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
			{
				if (GUILayout.Button("FPS", ButtonStyle))
				{
					fpsEvent?.Invoke();
				}

				if (GUILayout.Button("FPS Mode", ButtonStyle))
				{
					fpsModeEvent?.Invoke();
				}

				AddVSyncButton();

				if (GUILayout.Button($"Cursor={Cursor.visible}", ButtonStyle))
				{
					Cursor.visible = !Cursor.visible;
				}

				HorizontalLine(Color.gray);
				AddWindowButtons();
				HorizontalLine(Color.gray);

				AddCustomButtons();

				// if (GUILayout.Button($"TesterFriendly={StatusDatabase.ConfigurationStatus.IsTesterFriendly}", ButtonStyle))
				// {
				//     StatusDatabase.ConfigurationStatus.IsTesterFriendly = !StatusDatabase.ConfigurationStatus.IsTesterFriendly;
				// }

				HorizontalLine(Color.gray);
				if (GUILayout.Button("Close", ButtonStyle))
				{
					enabled = false;
				}

				scrollPosition = scrollView.scrollPosition;
			}
		}

		private void AddWindowButtons()
		{
			foreach (var window in windows)
			{
				AddWindowButton(window);
			}
		}

		private void AddWindowButton(DebugWindow window)
		{
			var c = GUI.backgroundColor;
			if (window.enabled)
			{
				GUI.backgroundColor = window.BackgroundColor;
			}

			if (GUILayout.Button(window.WindowName, ButtonStyle))
			{
				window.enabled = !window.enabled;
			}

			GUI.backgroundColor = c;
		}

		private void AddVSyncButton()
		{
			int vSync = QualitySettings.vSyncCount;
			if (GUILayout.Button(vSyncStrings[vSync], ButtonStyle))
			{
				if (vSync < 4)
				{
					vSync++;
				}
				else
				{
					vSync = 0;
				}

				QualitySettings.vSyncCount = vSync;
			}
		}

		private void AddCustomButtons()
		{
			var changedButtonNames = new Dictionary<int, string>();
			foreach (var customButtonIdAndNameWithFunction in customButtons)
			{
				if (GUILayout.Button(customButtonIdAndNameWithFunction.Value.Key, ButtonStyle))
				{
					changedButtonNames[customButtonIdAndNameWithFunction.Key] = customButtonIdAndNameWithFunction.Value.Value();
				}
			}

			foreach (var changedButtonName in changedButtonNames)
			{
				customButtons[changedButtonName.Key] = new KeyValuePair<string, Func<string>>(changedButtonName.Value, customButtons[changedButtonName.Key].Value);
			}
		}
	}
}