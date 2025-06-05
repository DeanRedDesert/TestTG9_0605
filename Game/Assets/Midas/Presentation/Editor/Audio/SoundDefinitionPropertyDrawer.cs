using UnityEditor;
using UnityEngine;
using Midas.Presentation.Audio;

namespace Midas.Presentation.Editor.Audio
{
	[CustomPropertyDrawer(typeof(SoundDefinition))]
	public sealed class SoundDefinitionPropertyDrawer : PropertyDrawer
	{
		private const int HorizontalSpace = 5;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			const int labelWidth = 15;
			var partWidth = (position.width - 4 * HorizontalSpace) / 4;

			position.height = EditorGUIUtility.singleLineHeight;
			position.width = partWidth;
			var idProperty = property.FindPropertyRelative("id");
			EditorGUI.PropertyField(position, idProperty, GUIContent.none);

			position.x += position.width + HorizontalSpace;
			position.width = partWidth;
			var clipProperty = property.FindPropertyRelative("clip");
			EditorGUI.PropertyField(position, clipProperty, GUIContent.none);

			position.x += position.width + HorizontalSpace;
			position.width = partWidth;
			var groupProperty = property.FindPropertyRelative("group");
			EditorGUI.PropertyField(position, groupProperty, GUIContent.none);

			position.x += position.width;
			position.width = labelWidth * 3;
			EditorGUI.LabelField(position, "Vol");
			position.x += position.width;
			position.width = partWidth * 0.7f - labelWidth * 3;
			var volumeProperty = property.FindPropertyRelative("volume");
			EditorGUI.PropertyField(position, volumeProperty, GUIContent.none);

			position.x += position.width + HorizontalSpace;
			position.width = labelWidth;
			EditorGUI.LabelField(position, "L");
			position.x += position.width;
			position.width = partWidth * 0.3f - labelWidth;
			var loopProperty = property.FindPropertyRelative("isLooped");
			EditorGUI.PropertyField(position, loopProperty, GUIContent.none);
		}
	}
}