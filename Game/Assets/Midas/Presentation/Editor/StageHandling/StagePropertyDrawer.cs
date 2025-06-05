using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Core;
using Midas.Presentation.Editor.Utilities;
using Midas.Presentation.StageHandling;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.StageHandling
{
	[CustomPropertyDrawer(typeof(Stage))]
	public sealed class StagePropertyDrawer : PropertyDrawer
	{
		#region Public

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UpdateSelectableStages();

			using (new PropertyScope(ref position, label, property))
			{
				var idProperty = property.FindPropertyRelative("id");
				var nameProperty = property.FindPropertyRelative("name");
				var index = GetSelectionOfValue(idProperty.intValue, nameProperty.stringValue);
				var options = _selectableStages.Select(b => b.Name).ToArray();

				using (new EditorGUI.ChangeCheckScope())
				{
					index = EditorGUI.Popup(position, index, options);
					if (index != -1)
					{
						var selectedStage = GetStageOfSelection(index);
						if (GUI.changed ||
							selectedStage.Name != nameProperty.stringValue) // we compare this for old files where the name was not stored
						{
							idProperty.intValue = GetStageOfSelection(index).Id;
							idProperty.serializedObject.ApplyModifiedProperties();
							nameProperty.stringValue = GetStageOfSelection(index).Name;
							nameProperty.serializedObject.ApplyModifiedProperties();
						}
					}
				}
			}
		}

		#endregion

		#region Private

		private static int GetSelectionOfValue(int value, string name)
		{
			var index = _selectableStages.FindIndex(s => s.Stage.Id == value);
			if (index == -1)
			{
				index = _selectableStages.FindIndex(s => s.Stage.Name == name);
			}

			return index;
		}

		private static Stage GetStageOfSelection(int selection)
		{
			return _selectableStages[selection].Stage;
		}

		private static void UpdateSelectableStages()
		{
			if (_selectableStages != null)
			{
				return;
			}

			_selectableStages = new List<(string, Stage)>();
			foreach (var type in ReflectionUtil.GetAllTypes(
						t => !t.Assembly.FullName.Contains("Assembly-CSharp-Editor") && t.GetCustomAttributes(typeof(StagesAttribute), false).FirstOrDefault()
							is StagesAttribute))
			{
				AddStagesProperties(type);
			}
		}

		private static void AddStagesProperties(Type type)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
			var stagesAttribute = (StagesAttribute)type.GetCustomAttribute(typeof(StagesAttribute), false);
			var group = stagesAttribute == null || stagesAttribute.Group == string.Empty ? string.Empty : $@"{stagesAttribute.Group}/";

			foreach (var property in properties.Where(p => p.PropertyType == typeof(Stage)))
			{
				var stage = (Stage)property.GetValue(null);
				_selectableStages.Add((group + stage.Name, stage));
			}
		}

		private static List<(string Name, Stage Stage)> _selectableStages;

		#endregion
	}
}