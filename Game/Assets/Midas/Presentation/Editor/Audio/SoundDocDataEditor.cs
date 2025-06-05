using Midas.Core.ExtensionMethods;
using Midas.Presentation.Audio;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Audio
{
	[CustomPropertyDrawer(typeof(SoundDocData))]
	public sealed class SoundDocDataEditor : PropertyDrawer
	{
		private const int VerticalSpace = 4;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var transcriptProperty = property.FindPropertyRelative("transcript");
			var hasTranscript = transcriptProperty.stringValue != "NA";

			var descriptionProperty = property.FindPropertyRelative("description");
			var hasDescription = descriptionProperty.stringValue != "Highlight" && descriptionProperty.stringValue != "Effect";

			var rowHeight = EditorGUIUtility.singleLineHeight + VerticalSpace;

			var rows = 4;
			rows += hasTranscript ? 1 : 0;
			rows += hasDescription ? 1 : 0;

			return rowHeight * rows;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var errorStyle = new GUIStyle(EditorStyles.label);
			errorStyle.normal.textColor = Color.red;
			var currentY = position.y;
			var totalWidth = position.width;
			var left = position.x;
			var rowHeight = EditorGUIUtility.singleLineHeight + VerticalSpace;

			var transcriptProperty = property.FindPropertyRelative("transcript");
			var descriptionProperty = property.FindPropertyRelative("description");
			var filenameProperty = property.FindPropertyRelative("filename");

			EditorGUI.BeginProperty(position, label, property);

			EditorGUI.LabelField(new Rect(left, position.y, totalWidth, rowHeight), new GUIContent(filenameProperty.stringValue), EditorStyles.boldLabel);
			currentY += rowHeight;

			currentY += OptionsRow(left, currentY, rowHeight, transcriptProperty, descriptionProperty);
			currentY += HandleDescriptionDisplay(new Rect(left, currentY, totalWidth, rowHeight), descriptionProperty, rowHeight, errorStyle);
			currentY += HandleTranscriptionDisplay(new Rect(left, currentY, totalWidth, rowHeight), rowHeight, transcriptProperty, errorStyle);
			HandleActivationListDisplay(new Rect(left, currentY, totalWidth, rowHeight * 2), property, rowHeight, errorStyle);

			EditorGUI.EndProperty();
		}

		private static float OptionsRow(float left, float currentY, float rowHeight, SerializedProperty transcriptProperty, SerializedProperty descriptionProperty)
		{
			// Has Voice.

			var hasVoice = EditorGUI.ToggleLeft(new Rect(left, currentY, 100, rowHeight), "Has Voice", transcriptProperty.stringValue != "NA");
			if (!hasVoice)
				transcriptProperty.stringValue = "NA";
			else if (transcriptProperty.stringValue == "NA")
				transcriptProperty.stringValue = "TODO";

			// Description.

			EditorGUI.LabelField(new Rect(left + 100, currentY, 100, rowHeight), new GUIContent("Description:"));

			var options = new[] { new GUIContent("Highlight"), new GUIContent("Effect"), new GUIContent("Other") };
			var descriptionIndex = options.FindIndex(o => o.text == descriptionProperty.stringValue);
			if (descriptionIndex == -1)
				descriptionIndex = 2;
			descriptionIndex = EditorGUI.Popup(new Rect(left + 200, currentY, 100, rowHeight), descriptionIndex, options);

			switch (descriptionIndex)
			{
				case 0:
					descriptionProperty.stringValue = "Highlight";
					break;
				case 1:
					descriptionProperty.stringValue = "Effect";
					break;
				case 2:
					if (descriptionProperty.stringValue == "Highlight" || descriptionProperty.stringValue == "Effect")
						descriptionProperty.stringValue = "TODO";
					break;
			}

			return rowHeight;
		}

		private static void HandleActivationListDisplay(Rect position, SerializedProperty property, float rowHeight, GUIStyle errorStyle)
		{
			var totalWidth = position.width;
			position.width = 100;
			position.height = rowHeight;
			var activationList = property.FindPropertyRelative("activationList");
			var isValid = activationList.stringValue != "TODO";
			EditorGUI.LabelField(position, new GUIContent("Activation List:"), isValid ? EditorStyles.label : errorStyle);
			position.x += 100;
			position.height = rowHeight * 2;
			position.width = totalWidth - 100;
			EditorStyles.textField.wordWrap = true;
			activationList.stringValue = EditorGUI.TextField(position, activationList.stringValue);
		}

		private static float HandleTranscriptionDisplay(Rect position, float rowHeight, SerializedProperty transcriptProperty, GUIStyle errorStyle)
		{
			if (transcriptProperty.stringValue == "NA")
				return 0;

			var totalWidth = position.width;
			position.width = 100;
			var isValid = transcriptProperty.stringValue != "TODO";
			EditorGUI.LabelField(position, new GUIContent("Transcript:"), isValid ? EditorStyles.label : errorStyle);
			position.x += 100;
			position.width = totalWidth - 100;
			EditorGUI.PropertyField(position, transcriptProperty, GUIContent.none);

			return rowHeight;
		}

		private static float HandleDescriptionDisplay(Rect position, SerializedProperty descriptionProperty, float rowHeight, GUIStyle errorStyle)
		{
			if (descriptionProperty.stringValue == "Highlight" || descriptionProperty.stringValue == "Effect")
				return 0;

			var totalWidth = position.width;
			var isValid = descriptionProperty.stringValue != "TODO";
			position.width = 100;
			EditorGUI.LabelField(position, new GUIContent("Description:"), isValid ? EditorStyles.label : errorStyle);
			position.x += 100;
			position.width = totalWidth - 100;
			EditorGUI.PropertyField(position, descriptionProperty, GUIContent.none);
			return rowHeight;
		}
	}
}