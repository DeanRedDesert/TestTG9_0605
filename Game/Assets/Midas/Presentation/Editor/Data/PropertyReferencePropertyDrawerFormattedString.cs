using System;
using System.Collections.Generic;
using Midas.Core.General;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Editor.Utilities;
using Midas.Presentation.General;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Data
{
	[CustomPropertyDrawer(typeof(PropertyReferenceFormattedString), true)]
	public sealed class PropertyReferencePropertyDrawerFormattedString : PropertyDrawer
	{
		#region Public

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var h = base.GetPropertyHeight(property, label);
			return h * 2 + 6;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var statusItemProperties = PropertyPathResolver.CollectProperties(typeof(object));

			using (new PropertyScope(ref position, label, property))
			{
				var formattingProperty = property.FindPropertyRelative("formattingOptions");
				var pathProperty = property.FindPropertyRelative("path");
				var itemPath = pathProperty.stringValue;

				var totalWidth = position.width;

				MoneyAndCreditDisplayMode displayMode = default;
				CreditDisplaySeparatorMode creditSeparatorMode = default;
				float monoSpacing = default;
				MonoSpacingMode monoSpacingMode = default;

				position.height = position.height / 2 - 2;
				var newItemIndex = PropertyPathHelper.DrawItemSelector(statusItemProperties, position, itemPath);

				if (newItemIndex == -1)
					return;

				itemPath = statusItemProperties[newItemIndex].Name;
				var needsFormatOptions = NeedsFormatOptions(statusItemProperties[newItemIndex].PropertyType);

				if (formattingProperty != null)
				{
					var formattingOptions = new string[formattingProperty.arraySize];
					for (var i = 0; i < formattingOptions.Length; ++i)
						formattingOptions[i] = formattingProperty.GetArrayElementAtIndex(i).stringValue;

					position.y += position.height;
					var ms = DrawMonoSpacing(position, formattingOptions, totalWidth * .3f);
					monoSpacing = ms.Item1;
					monoSpacingMode = ms.Item2;

					if (needsFormatOptions)
					{
						var spacing = totalWidth * .35f;
						position.width = spacing - 2;

						position.x += totalWidth * .3f + 1;
						displayMode = DrawDisplayMode(position, formattingOptions);

						position.x += spacing + 1;
						creditSeparatorMode = DrawSeparatorMode(position, formattingOptions);
					}
				}

				UpdateSerializedProperty(pathProperty, itemPath, needsFormatOptions, formattingProperty, displayMode, creditSeparatorMode, monoSpacing, monoSpacingMode);
			}
		}

		#endregion

		#region Private

		private static CreditDisplaySeparatorMode DrawSeparatorMode(Rect position, IReadOnlyList<string> formattingOptions)
		{
			var creditSeparatorMode = CreditDisplaySeparatorMode.Auto;
			if (formattingOptions.Count > 1 &&
				Enum.TryParse(formattingOptions[1], out CreditDisplaySeparatorMode parsedCreditSeparatorMode))
			{
				creditSeparatorMode = parsedCreditSeparatorMode;
			}

			creditSeparatorMode = (CreditDisplaySeparatorMode)EditorGUI.EnumPopup(position, creditSeparatorMode);
			return creditSeparatorMode;
		}

		private static MoneyAndCreditDisplayMode DrawDisplayMode(Rect position, IReadOnlyList<string> formattingOptions)
		{
			MoneyAndCreditDisplayMode displayMode = default;
			if (formattingOptions.Count > 0 &&
				Enum.TryParse(formattingOptions[0], out MoneyAndCreditDisplayMode parsedDisplayMode))
			{
				displayMode = parsedDisplayMode;
			}

			displayMode = (MoneyAndCreditDisplayMode)EditorGUI.EnumPopup(position, displayMode);
			return displayMode;
		}

		private static (float, MonoSpacingMode) DrawMonoSpacing(Rect position, IReadOnlyList<string> formattingOptions, float width)
		{
			if (formattingOptions.Count < 4)
				return (0, MonoSpacingMode.None);

			MonoSpacingMode monoSpacingMode = default;
			if (Enum.TryParse(formattingOptions[3], out MonoSpacingMode parsedDisplayMode))
				monoSpacingMode = parsedDisplayMode;

			var rect = new Rect(position);
			rect.width = width * .6f;
			monoSpacingMode = (MonoSpacingMode)EditorGUI.EnumPopup(rect, monoSpacingMode);

			rect.x += rect.width + 2;
			rect.width = width * .4f - 2;
			var f = float.Parse(formattingOptions[2]);
			f = EditorGUI.FloatField(rect, f);
			return (f, monoSpacingMode);
		}

		private static void UpdateSerializedProperty(SerializedProperty property, string propertyPath, bool needsFormatOptions,
			SerializedProperty formattingProperty,
			MoneyAndCreditDisplayMode displayMode, CreditDisplaySeparatorMode creditSeparatorMode, float monoSpacing, MonoSpacingMode monoSpacingMode)
		{
			if (property.stringValue != propertyPath)
			{
				property.stringValue = propertyPath;
				property.serializedObject.ApplyModifiedProperties();
			}

			if (formattingProperty != null)
			{
				while (formattingProperty.arraySize < 4)
				{
					formattingProperty.InsertArrayElementAtIndex(formattingProperty.arraySize);
				}

				if (formattingProperty.GetArrayElementAtIndex(0).stringValue != displayMode.ToString())
				{
					formattingProperty.GetArrayElementAtIndex(0).stringValue = displayMode.ToString();
					formattingProperty.GetArrayElementAtIndex(0).serializedObject.ApplyModifiedProperties();
				}

				if (formattingProperty.GetArrayElementAtIndex(1).stringValue != creditSeparatorMode.ToString())
				{
					formattingProperty.GetArrayElementAtIndex(1).stringValue = creditSeparatorMode.ToString();
					formattingProperty.GetArrayElementAtIndex(1).serializedObject.ApplyModifiedProperties();
				}

				if (formattingProperty.GetArrayElementAtIndex(2).stringValue != monoSpacing.ToString())
				{
					formattingProperty.GetArrayElementAtIndex(2).stringValue = monoSpacing.ToString();
					formattingProperty.GetArrayElementAtIndex(2).serializedObject.ApplyModifiedProperties();
				}

				if (formattingProperty.GetArrayElementAtIndex(3).stringValue != monoSpacingMode.ToString())
				{
					formattingProperty.GetArrayElementAtIndex(3).stringValue = monoSpacingMode.ToString();
					formattingProperty.GetArrayElementAtIndex(3).serializedObject.ApplyModifiedProperties();
				}
			}
		}

		private static bool NeedsFormatOptions(Type propertyType)
		{
			return propertyType == typeof(Credit) || propertyType == typeof(Money);
		}

		#endregion
	}
}