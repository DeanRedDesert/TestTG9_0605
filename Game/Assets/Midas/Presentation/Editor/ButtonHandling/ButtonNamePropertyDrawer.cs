using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.ButtonHandling
{
	[CustomPropertyDrawer(typeof(ButtonName))]
	public sealed class ButtonNamePropertyDrawer : PropertyDrawer
	{
		private List<string> selectableButtonNames;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UpdateSelectableButtonNames();

			using (new PropertyScope(ref position, label, property))
			{
				var idNameProperty = property.FindPropertyRelative("idName");
				var options = selectableButtonNames.Select(b => b).ToArray();
				var index = GetSelectionOfValue(idNameProperty.stringValue);

				using (new EditorGUI.ChangeCheckScope())
				{
					index = EditorGUI.Popup(position, index, options);
					if (GUI.changed && index != -1)
					{
						idNameProperty.stringValue = GetValueOfSelection(index);
						idNameProperty.serializedObject.ApplyModifiedProperties();
					}
				}
			}
		}

		private int GetSelectionOfValue(string value)
		{
			return selectableButtonNames.FindIndex(s => s == value);
		}

		private string GetValueOfSelection(int selection)
		{
			return selectableButtonNames[selection];
		}

		private void UpdateSelectableButtonNames()
		{
			if (selectableButtonNames != null)
			{
				return;
			}

			selectableButtonNames = new List<string>();
			AddButtonFunctionProperties(typeof(PhysicalButtons));
		}

		private void AddButtonFunctionProperties(Type type)
		{
			selectableButtonNames.Add("Undefined");
			selectableButtonNames.AddRange(
				type.GetProperties()
					.Where(p => p.PropertyType == typeof(PhysicalButton))
					.Select(p => p.Name));
		}
	}
}