using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.General
{
	// [CustomPropertyDrawer(typeof(SerializableIntHashSet), true)]
	// [CustomPropertyDrawer(typeof(SerializableStringHashSet), true)]
	public class SerializableDictionaryPropertyDrawer : PropertyDrawer
	{
		#region Nested Types

		private sealed class ConflictState
		{
			public object ConflictKey { get; set; }
			public object ConflictValue { get; set; }
			public int ConflictIndex { get; set; } = -1;
			public int ConflictOtherIndex { get; set; } = -1;
			public bool ConflictKeyPropertyExpanded { get; set; }
			public bool ConflictValuePropertyExpanded { get; set; }
			public float ConflictLineHeight { get; set; }
		}

		private struct PropertyIdentity
		{
			[SuppressMessage("ReSharper", "NotAccessedField.Local")]
			private Object instance;

			[SuppressMessage("ReSharper", "NotAccessedField.Local")]
			private string propertyPath;

			public PropertyIdentity(SerializedProperty property)
			{
				instance = property.serializedObject.targetObject;
				propertyPath = property.propertyPath;
			}
		}

		private readonly struct EnumerationEntry
		{
			public SerializedProperty KeyProperty { get; }
			public SerializedProperty ValueProperty { get; }
			public int Index { get; }

			public EnumerationEntry(SerializedProperty keyProperty, SerializedProperty valueProperty, int index)
			{
				KeyProperty = keyProperty;
				ValueProperty = valueProperty;
				Index = index;
			}
		}

		#endregion

		#region Fields

		private static readonly GUIContent iconPlus = IconContent("Toolbar Plus", "Add entry");
		private static readonly GUIContent iconMinus = IconContent("Toolbar Minus", "Remove entry");
		private static readonly GUIContent warningIconConflict = IconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");
		private static readonly GUIContent warningIconOther = IconContent("console.infoicon.sml", "Conflicting key");
		private static readonly GUIContent warningIconNull = IconContent("console.warnicon.sml", "Null key, this entry will be lost");
		private static readonly GUIStyle buttonStyle = GUIStyle.none;
		private static readonly GUIContent tempContent = new GUIContent();
		private static readonly Dictionary<PropertyIdentity, ConflictState> conflictStateDict = new Dictionary<PropertyIdentity, ConflictState>();
		private static readonly Dictionary<SerializedPropertyType, PropertyInfo> serializedPropertyValueAccessorsDict = CreatePropertyTypeToNameDictionary();

		#endregion

		#region PropertyDrawer Overrides

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (var scope = new PropertyScope(ref position, label, property, false))
			{
				var buttonAction = Action.None;
				var buttonActionIndex = 0;

				var keyArrayProperty = property.FindPropertyRelative(KeysFieldName);
				var valueArrayProperty = property.FindPropertyRelative(ValuesFieldName);

				var conflictState = GetConflictState(property);

				if (conflictState.ConflictIndex != -1)
				{
					keyArrayProperty.InsertArrayElementAtIndex(conflictState.ConflictIndex);
					var keyProperty = keyArrayProperty.GetArrayElementAtIndex(conflictState.ConflictIndex);
					SetPropertyValue(keyProperty, conflictState.ConflictKey);
					keyProperty.isExpanded = conflictState.ConflictKeyPropertyExpanded;

					if (valueArrayProperty != null)
					{
						valueArrayProperty.InsertArrayElementAtIndex(conflictState.ConflictIndex);
						var valueProperty = valueArrayProperty.GetArrayElementAtIndex(conflictState.ConflictIndex);
						SetPropertyValue(valueProperty, conflictState.ConflictValue);
						valueProperty.isExpanded = conflictState.ConflictValuePropertyExpanded;
					}
				}

				var buttonWidth = buttonStyle.CalcSize(iconPlus).x;

				var labelPosition = position;
				labelPosition.height = EditorGUIUtility.singleLineHeight;
				if (property.isExpanded)
					labelPosition.xMax -= buttonStyle.CalcSize(iconPlus).x;

				EditorGUI.PropertyField(labelPosition, property, scope.Content, false);
				if (property.isExpanded)
				{
					var buttonPosition = position;
					buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
					buttonPosition.height = EditorGUIUtility.singleLineHeight;
					EditorGUI.BeginDisabledGroup(conflictState.ConflictIndex != -1);
					if (GUI.Button(buttonPosition, iconPlus, buttonStyle))
					{
						buttonAction = Action.Add;
						buttonActionIndex = keyArrayProperty.arraySize;
					}

					EditorGUI.EndDisabledGroup();

					EditorGUI.indentLevel++;
					var linePosition = position;
					linePosition.y += EditorGUIUtility.singleLineHeight;
					linePosition.xMax -= buttonWidth;

					foreach (var entry in EnumerateEntries(keyArrayProperty, valueArrayProperty))
					{
						var keyProperty = entry.KeyProperty;
						var valueProperty = entry.ValueProperty;
						var i = entry.Index;

						var lineHeight = DrawKeyValueLine(keyProperty, valueProperty, linePosition);

						buttonPosition = linePosition;
						buttonPosition.x = linePosition.xMax;
						buttonPosition.height = EditorGUIUtility.singleLineHeight;
						if (GUI.Button(buttonPosition, iconMinus, buttonStyle))
						{
							buttonAction = Action.Remove;
							buttonActionIndex = i;
						}

						if (i == conflictState.ConflictIndex && conflictState.ConflictOtherIndex == -1)
						{
							var iconPosition = linePosition;
							iconPosition.size = buttonStyle.CalcSize(warningIconNull);
							GUI.Label(iconPosition, warningIconNull);
						}
						else if (i == conflictState.ConflictIndex)
						{
							var iconPosition = linePosition;
							iconPosition.size = buttonStyle.CalcSize(warningIconConflict);
							GUI.Label(iconPosition, warningIconConflict);
						}
						else if (i == conflictState.ConflictOtherIndex)
						{
							var iconPosition = linePosition;
							iconPosition.size = buttonStyle.CalcSize(warningIconOther);
							GUI.Label(iconPosition, warningIconOther);
						}

						linePosition.y += lineHeight;
					}

					EditorGUI.indentLevel--;
				}

				if (buttonAction == Action.Add)
				{
					keyArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
					valueArrayProperty?.InsertArrayElementAtIndex(buttonActionIndex);
				}
				else if (buttonAction == Action.Remove)
				{
					DeleteArrayElementAtIndex(keyArrayProperty, buttonActionIndex);
					if (valueArrayProperty != null)
						DeleteArrayElementAtIndex(valueArrayProperty, buttonActionIndex);
				}

				conflictState.ConflictKey = null;
				conflictState.ConflictValue = null;
				conflictState.ConflictIndex = -1;
				conflictState.ConflictOtherIndex = -1;
				conflictState.ConflictLineHeight = 0f;
				conflictState.ConflictKeyPropertyExpanded = false;
				conflictState.ConflictValuePropertyExpanded = false;

				foreach (var entry1 in EnumerateEntries(keyArrayProperty, valueArrayProperty))
				{
					var keyProperty1 = entry1.KeyProperty;
					var i = entry1.Index;
					var keyProperty1Value = GetPropertyValue(keyProperty1);

					if (keyProperty1Value == null)
					{
						var valueProperty1 = entry1.ValueProperty;
						SaveProperty(keyProperty1, valueProperty1, i, -1, conflictState);
						DeleteArrayElementAtIndex(keyArrayProperty, i);
						if (valueArrayProperty != null)
							DeleteArrayElementAtIndex(valueArrayProperty, i);

						break;
					}

					foreach (var entry2 in EnumerateEntries(keyArrayProperty, valueArrayProperty, i + 1))
					{
						var keyProperty2 = entry2.KeyProperty;
						var j = entry2.Index;
						var keyProperty2Value = GetPropertyValue(keyProperty2);

						if (ComparePropertyValues(keyProperty1Value, keyProperty2Value))
						{
							var valueProperty2 = entry2.ValueProperty;
							SaveProperty(keyProperty2, valueProperty2, j, i, conflictState);
							DeleteArrayElementAtIndex(keyArrayProperty, j);
							if (valueArrayProperty != null)
								DeleteArrayElementAtIndex(valueArrayProperty, j);

							goto breakLoops;
						}
					}
				}

				breakLoops: ;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var propertyHeight = EditorGUIUtility.singleLineHeight;

			if (property.isExpanded)
			{
				var keysProperty = property.FindPropertyRelative(KeysFieldName);
				var valuesProperty = property.FindPropertyRelative(ValuesFieldName);

				foreach (var entry in EnumerateEntries(keysProperty, valuesProperty))
				{
					var keyProperty = entry.KeyProperty;
					var valueProperty = entry.ValueProperty;
					var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
					var valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
					var lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
					propertyHeight += lineHeight;
				}

				var conflictState = GetConflictState(property);

				if (conflictState.ConflictIndex != -1)
					propertyHeight += conflictState.ConflictLineHeight;
			}

			return propertyHeight;
		}

		#endregion

		#region Private Methods

		private const float IndentWidth = 15f;

		private const string KeysFieldName = "keys";
		private const string ValuesFieldName = "values";

		private enum Action
		{
			None,
			Add,
			Remove
		}

		private static Dictionary<SerializedPropertyType, PropertyInfo> CreatePropertyTypeToNameDictionary()
		{
			var serializedPropertyValueAccessorsName = new[]
			{
				(SerializedPropertyType.Integer, "intValue"),
				(SerializedPropertyType.Boolean, "boolValue"),
				(SerializedPropertyType.Float, "floatValue"),
				(SerializedPropertyType.String, "stringValue"),
				(SerializedPropertyType.Color, "colorValue"),
				(SerializedPropertyType.ObjectReference, "objectReferenceValue"),
				(SerializedPropertyType.LayerMask, "intValue"),
				(SerializedPropertyType.Enum, "intValue"),
				(SerializedPropertyType.Vector2, "vector2Value"),
				(SerializedPropertyType.Vector3, "vector3Value"),
				(SerializedPropertyType.Vector4, "vector4Value"),
				(SerializedPropertyType.Rect, "rectValue"),
				(SerializedPropertyType.ArraySize, "intValue"),
				(SerializedPropertyType.Character, "intValue"),
				(SerializedPropertyType.AnimationCurve, "animationCurveValue"),
				(SerializedPropertyType.Bounds, "boundsValue"),
				(SerializedPropertyType.Quaternion, "quaternionValue")
			};
			var serializedPropertyType = typeof(SerializedProperty);

			var flags = BindingFlags.Instance | BindingFlags.Public;

			var dic = new Dictionary<SerializedPropertyType, PropertyInfo>();
			foreach (var (propertyType, name) in serializedPropertyValueAccessorsName)
			{
				var propertyInfo = serializedPropertyType.GetProperty(name, flags);
				dic.Add(propertyType, propertyInfo);
			}

			return dic;
		}

		private static object GetPropertyValue(SerializedProperty p)
		{
			if (serializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out var propertyInfo))
				return propertyInfo.GetValue(p, null);

			if (p.isArray)
				return GetPropertyValueArray(p);
			return GetPropertyValueGeneric(p);
		}

		private static float DrawKeyValueLine(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition)
		{
			if (valueProperty != null)
				return DrawKeyValueLineSimple(keyProperty, valueProperty, null, null, linePosition);
			return DrawKeyLine(keyProperty, linePosition, null);
		}

		private static float DrawKeyValueLineSimple(SerializedProperty keyProperty, SerializedProperty valueProperty, string keyLabel, string valueLabel, Rect linePosition)
		{
			var labelWidth = EditorGUIUtility.labelWidth;
			var labelWidthRelative = labelWidth / linePosition.width;

			var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
			var valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);

			var keyPosition = linePosition;
			keyPosition.height = keyPropertyHeight;
			keyPosition.width = labelWidth - IndentWidth;
			EditorGUIUtility.labelWidth = keyPosition.width * labelWidthRelative;
			var keyLabelContent = keyLabel != null ? TempContent(keyLabel) : GUIContent.none;
			EditorGUI.PropertyField(keyPosition, keyProperty, keyLabelContent, true);

			var valuePosition = linePosition;
			valuePosition.height = valuePropertyHeight;
			valuePosition.xMin += labelWidth;
			EditorGUIUtility.labelWidth = valuePosition.width * labelWidthRelative;
			var valueLabelContent = keyLabel != null ? TempContent(valueLabel) : GUIContent.none;
			EditorGUI.indentLevel--;
			EditorGUI.PropertyField(valuePosition, valueProperty, valueLabelContent, true);
			EditorGUI.indentLevel++;

			EditorGUIUtility.labelWidth = labelWidth;

			return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
		}

		private static float DrawKeyLine(SerializedProperty keyProperty, Rect linePosition, string keyLabel)
		{
			var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
			var keyPosition = linePosition;
			keyPosition.height = keyPropertyHeight;
			keyPosition.width = linePosition.width;

			var keyLabelContent = keyLabel != null ? TempContent(keyLabel) : GUIContent.none;
			EditorGUI.PropertyField(keyPosition, keyProperty, keyLabelContent, true);

			return keyPropertyHeight;
		}

		private static void SaveProperty(SerializedProperty keyProperty, SerializedProperty valueProperty, int index, int otherIndex, ConflictState conflictState)
		{
			conflictState.ConflictKey = GetPropertyValue(keyProperty);
			conflictState.ConflictValue = valueProperty != null ? GetPropertyValue(valueProperty) : null;
			var keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
			var valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
			var lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
			conflictState.ConflictLineHeight = lineHeight;
			conflictState.ConflictIndex = index;
			conflictState.ConflictOtherIndex = otherIndex;
			conflictState.ConflictKeyPropertyExpanded = keyProperty.isExpanded;
			conflictState.ConflictValuePropertyExpanded = valueProperty?.isExpanded ?? false;
		}

		private static ConflictState GetConflictState(SerializedProperty property)
		{
			var propId = new PropertyIdentity(property);
			if (!conflictStateDict.TryGetValue(propId, out var conflictState))
			{
				conflictState = new ConflictState();
				conflictStateDict.Add(propId, conflictState);
			}

			return conflictState;
		}

		private static GUIContent IconContent(string name, string tooltip)
		{
			var builtinIcon = EditorGUIUtility.IconContent(name);
			return new GUIContent(builtinIcon.image, tooltip);
		}

		private static GUIContent TempContent(string text)
		{
			tempContent.text = text;
			return tempContent;
		}

		private static void DeleteArrayElementAtIndex(SerializedProperty arrayProperty, int index)
		{
			var property = arrayProperty.GetArrayElementAtIndex(index);
			if (property.propertyType == SerializedPropertyType.ObjectReference)
				property.objectReferenceValue = null;

			arrayProperty.DeleteArrayElementAtIndex(index);
		}

		private static void SetPropertyValue(SerializedProperty p, object v)
		{
			if (serializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out var propertyInfo))
			{
				propertyInfo.SetValue(p, v, null);
			}
			else
			{
				if (p.isArray)
					SetPropertyValueArray(p, v);
				else
					SetPropertyValueGeneric(p, v);
			}
		}

		private static object GetPropertyValueArray(SerializedProperty property)
		{
			var array = new object[property.arraySize];
			for (var i = 0; i < property.arraySize; i++)
			{
				var item = property.GetArrayElementAtIndex(i);
				array[i] = GetPropertyValue(item);
			}

			return array;
		}

		private static object GetPropertyValueGeneric(SerializedProperty property)
		{
			var dict = new Dictionary<string, object>();
			var iterator = property.Copy();
			if (iterator.Next(true))
			{
				var end = property.GetEndProperty();
				do
				{
					var name = iterator.name;
					var value = GetPropertyValue(iterator);
					dict.Add(name, value);
				} while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
			}

			return dict;
		}

		private static void SetPropertyValueArray(SerializedProperty property, object v)
		{
			var array = (object[])v;
			property.arraySize = array.Length;
			for (var i = 0; i < property.arraySize; i++)
			{
				var item = property.GetArrayElementAtIndex(i);
				SetPropertyValue(item, array[i]);
			}
		}

		private static void SetPropertyValueGeneric(SerializedProperty property, object v)
		{
			var dict = (Dictionary<string, object>)v;
			var iterator = property.Copy();
			if (iterator.Next(true))
			{
				var end = property.GetEndProperty();
				do
				{
					var name = iterator.name;
					SetPropertyValue(iterator, dict[name]);
				} while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
			}
		}

		private static bool ComparePropertyValues(object value1, object value2)
		{
			if (value1 is Dictionary<string, object> dict1 && value2 is Dictionary<string, object> dict2)
				return CompareDictionaries(dict1, dict2);
			return Equals(value1, value2);
		}

		private static bool CompareDictionaries(Dictionary<string, object> dict1, IReadOnlyDictionary<string, object> dict2)
		{
			if (dict1.Count != dict2.Count)
				return false;

			foreach (var kvp1 in dict1)
			{
				var key1 = kvp1.Key;
				var value1 = kvp1.Value;

				if (!dict2.TryGetValue(key1, out var value2))
					return false;

				if (!ComparePropertyValues(value1, value2))
					return false;
			}

			return true;
		}

		private static IEnumerable<EnumerationEntry> EnumerateEntries(SerializedProperty keyArrayProperty, SerializedProperty valueArrayProperty, int startIndex = 0)
		{
			if (keyArrayProperty.arraySize > startIndex)
			{
				var index = startIndex;
				var keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
				var valueProperty = valueArrayProperty?.GetArrayElementAtIndex(startIndex);
				var endProperty = keyArrayProperty.GetEndProperty();

				do
				{
					yield return new EnumerationEntry(keyProperty, valueProperty, index);
					index++;
				} while (keyProperty.Next(false)
						&& (valueProperty?.Next(false) ?? true)
						&& !SerializedProperty.EqualContents(keyProperty, endProperty));
			}
		}

		#endregion
	}
}