using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IGT.Ascent.Assets.StandaloneSafeStorage;
using IGT.Ascent.Assets.StandaloneSafeStorage.Editor;
using Midas.Tools.Editor.ToolbarExt;
using UnityEditor;
using UnityEngine;

namespace Midas.Ascent.Editor
{
	[InitializeOnLoad]
	public class SafeStorageToolbar : IToolbarExtension
	{
		private static readonly Action<SaveSlot> saveToSlot;
		private static readonly Action<SaveSlot> loadFromSlot;
		private static readonly Func<SaveSlot, bool> canLoadFromSlot;

		static SafeStorageToolbar()
		{
			ToolbarExtender.LeftToolbarGUI.Add(new SafeStorageToolbar());

			// Calling this to refresh internal state.
			SafeStorageMenu.ValidateLoadSlot1();

			var t = typeof(SafeStorageMenu);
			var saveMethod = t.GetMethod("SaveToSlot", BindingFlags.Static | BindingFlags.NonPublic);
			saveToSlot = saveSlot => saveMethod?.Invoke(null, new object[] { saveSlot });
			var loadMethod = t.GetMethod("LoadFromSlot", BindingFlags.Static | BindingFlags.NonPublic);
			loadFromSlot = saveSlot => loadMethod?.Invoke(null, new object[] { saveSlot });
			var savedSlotsField = t.GetField("savedSlots", BindingFlags.Static | BindingFlags.NonPublic);
			canLoadFromSlot = saveSlot => ((IReadOnlyList<SaveSlot>)savedSlotsField?.GetValue(null))?.Contains(saveSlot) == true;
		}

		public bool IsDirty => false;

		public void OnGui()
		{
			GUILayout.FlexibleSpace();

			var wasEnabled = GUI.enabled;

			for (var i = 0; i < 5; i++)
			{
				var saveSlot = SaveSlot.Slot1 + i;
				var slotHasData = canLoadFromSlot(saveSlot);

				if (EditorApplication.isPlaying)
				{
					if (GUILayout.Button(new GUIContent((i + 1).ToString(), $"Save Slot {i + 1}"), slotHasData ? ToolbarStyles.CommandHighlightedButtonStyle : ToolbarStyles.CommandButtonStyle, GUILayout.Width(30)))
						saveToSlot(saveSlot);
				}
				else
				{
					GUI.enabled = slotHasData;
					if (GUILayout.Button(new GUIContent((i + 1).ToString(), $"Load Slot {i + 1} and Play"), slotHasData ? ToolbarStyles.CommandHighlightedButtonStyle : ToolbarStyles.CommandButtonStyle, GUILayout.Width(30)))
					{
						loadFromSlot(saveSlot);
						EditorApplication.EnterPlaymode();
					}

					GUI.enabled = wasEnabled;
				}
			}

			GUI.enabled = !EditorApplication.isPlaying;

			var tex = EditorGUIUtility.IconContent("CacheServerDisconnected@2x").image;
			if (GUILayout.Button(new GUIContent(null, tex, "Clear Safe Storage and Play"), "Command"))
			{
				SafeStorageMenu.Clear();
				EditorApplication.EnterPlaymode();
			}

			GUI.enabled = wasEnabled;
		}
	}
}