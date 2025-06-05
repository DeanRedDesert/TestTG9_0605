using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Editor.General;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Data
{
	public static class PropertyPathHelper
	{
		public static int DrawItemSelector(IReadOnlyList<(string Name, Type PropertyType)> statusItemProperties, Rect position, string itemPath)
		{
			if (Application.isPlaying)
			{
				var wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUI.TextField(position, itemPath);
				GUI.enabled = wasEnabled;
				return GetPropertyIndex(statusItemProperties, itemPath);
			}

			var searchString = string.IsNullOrEmpty(itemPath) ? "Unassigned" : itemPath;
			var choiceIndex = GetPropertyIndex(statusItemProperties, itemPath);

			var popRect = new Rect(position.x, position.y, position.width, position.height);
			using (new ChangeGuiBackgroundColorScope(choiceIndex == 0 ? Color.cyan : GUI.backgroundColor))
			{
				using (new EditorGUI.ChangeCheckScope())
				{
					if (statusItemProperties.Count < 15)
					{
						var nameList = statusItemProperties.Select(v => v.Name).ToArray();
						choiceIndex = GuiUtil.CheckedPopup(popRect, searchString, nameList);
					}
					else
					{
						var nameList = statusItemProperties.Select(v => v.Name.Replace('.', '/')).ToArray();
						choiceIndex = GuiUtil.CheckedPopup(popRect, searchString.Replace('.', '/'), nameList);
					}
				}
			}

			return choiceIndex;
		}

		private static int GetPropertyIndex(IReadOnlyList<(string Name, Type PropertyType)> statusItemProperties, string propertyPath)
		{
			for (var i = 0; i < statusItemProperties.Count; i++)
			{
				var item = statusItemProperties[i];
				if (item.Name == propertyPath)
					return i;
			}

			return -1;
		}
	}
}