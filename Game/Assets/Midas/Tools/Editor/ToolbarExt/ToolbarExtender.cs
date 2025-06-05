// ReSharper disable All - This file is borrowed from https://github.com/marijnz/unity-toolbar-extender

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Midas.Tools.Editor.ToolbarExt
{
	[InitializeOnLoad]
	public static class ToolbarExtender
	{
		private static int toolCount;
		private static GUIStyle commandStyle = null;

		public static readonly List<IToolbarExtension> LeftToolbarGUI = new List<IToolbarExtension>();
		public static readonly List<IToolbarExtension> RightToolbarGUI = new List<IToolbarExtension>();

		static ToolbarExtender()
		{
			Type toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
			FieldInfo toolIcons = toolbarType.GetField("k_ToolCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			toolCount = toolIcons != null ? ((int)toolIcons.GetValue(null)) : 8;

			ToolbarCallback.CheckDirty = () => RightToolbarGUI.Any(t => t.IsDirty) || LeftToolbarGUI.Any(t => t.IsDirty);
			ToolbarCallback.OnToolbarGUI = OnGUI;
			ToolbarCallback.OnToolbarGUILeft = GUILeft;
			ToolbarCallback.OnToolbarGUIRight = GUIRight;
		}

		public const float space = 8;
		public const float largeSpace = 20;
		public const float buttonWidth = 32;
		public const float dropdownWidth = 80;
		public const float playPauseStopWidth = 140;

		static void OnGUI()
		{
			// Create two containers, left and right
			// Screen is whole toolbar

			if (commandStyle == null)
			{
				commandStyle = new GUIStyle("CommandLeft");
			}

			var screenWidth = EditorGUIUtility.currentViewWidth;

			// Following calculations match code reflected from Toolbar.OldOnGUI()
			float playButtonsPosition = Mathf.RoundToInt((screenWidth - playPauseStopWidth) / 2);

			Rect leftRect = new Rect(0, 0, screenWidth, Screen.height);
			leftRect.xMin += space; // Spacing left
			leftRect.xMin += buttonWidth * toolCount; // Tool buttons
			leftRect.xMin += space; // Spacing between tools and pivot
			leftRect.xMin += 64 * 2; // Pivot buttons
			leftRect.xMax = playButtonsPosition;

			Rect rightRect = new Rect(0, 0, screenWidth, Screen.height);
			rightRect.xMin = playButtonsPosition;
			rightRect.xMin += commandStyle.fixedWidth * 3; // Play buttons
			rightRect.xMax = screenWidth;
			rightRect.xMax -= space; // Spacing right
			rightRect.xMax -= dropdownWidth; // Layout
			rightRect.xMax -= space; // Spacing between layout and layers
			rightRect.xMax -= dropdownWidth; // Layers
			rightRect.xMax -= space; // Spacing between layers and account
			rightRect.xMax -= dropdownWidth; // Account
			rightRect.xMax -= space; // Spacing between account and cloud
			rightRect.xMax -= buttonWidth; // Cloud
			rightRect.xMax -= space; // Spacing between cloud and collab
			rightRect.xMax -= 78; // Colab

			// Add spacing around existing controls
			leftRect.xMin += space;
			leftRect.xMax -= space;
			rightRect.xMin += space;
			rightRect.xMax -= space;

			// Add top and bottom margins
			leftRect.y = 4;
			leftRect.height = 22;
			rightRect.y = 4;
			rightRect.height = 22;
 
			if (leftRect.width > 0)
			{
				GUILayout.BeginArea(leftRect);
				GUILayout.BeginHorizontal();
				foreach (var handler in LeftToolbarGUI)
				{
					handler.OnGui();
				}

				GUILayout.EndHorizontal();
				GUILayout.EndArea();
			}

			if (rightRect.width > 0)
			{
				GUILayout.BeginArea(rightRect);
				GUILayout.BeginHorizontal();
				foreach (var handler in RightToolbarGUI)
				{
					handler.OnGui();
				}

				GUILayout.EndHorizontal();
				GUILayout.EndArea();
			}
		}

		public static void GUILeft()
		{
			GUILayout.BeginHorizontal();
			foreach (var handler in LeftToolbarGUI)
			{
				handler.OnGui();
			}

			GUILayout.EndHorizontal();
		}

		public static void GUIRight()
		{
			GUILayout.BeginHorizontal();
			foreach (var handler in RightToolbarGUI)
			{
				handler.OnGui();
			}

			GUILayout.EndHorizontal();
		}
	}
}