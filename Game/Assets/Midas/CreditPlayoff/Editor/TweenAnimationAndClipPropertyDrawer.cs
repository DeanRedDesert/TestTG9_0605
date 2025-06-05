using System.Linq;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.Tween;
using UnityEditor;
using UnityEngine;

namespace Midas.CreditPlayoff.Editor
{
	[CustomPropertyDrawer(typeof(TweenAnimationAndClip))]
	public sealed class TweenAnimationAndClipPropertyDrawer : PropertyDrawer
	{
		#region Public

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var tweenAnimationProperty = property.FindPropertyRelative("tweenAnimation");
			return EditorGUI.GetPropertyHeight(tweenAnimationProperty) + EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new PropertyScope(ref position, label, property, false))
			{
				var tweenAnimationProperty = property.FindPropertyRelative("tweenAnimation");
				position.height = EditorGUI.GetPropertyHeight(tweenAnimationProperty);
				EditorGUI.PropertyField(position, tweenAnimationProperty);

				position.y += position.height;
				position.height = EditorGUIUtility.singleLineHeight;
				ShowGTweenAnimation(position, property);
			}
		}

		#endregion

		#region Private

		private static void ShowGTweenAnimation(Rect position, SerializedProperty property)
		{
			var tweenAnimationProperty = property.FindPropertyRelative("tweenAnimation");
			if (tweenAnimationProperty.objectReferenceValue is TweenAnimation tweenAnimation)
			{
				var animationComponentClipNameProperty = property.FindPropertyRelative("clipName");

				var animClipNames = tweenAnimation.GetTweenClips().Select(t => t.Name).ToArray();

				using (new EditorGUI.ChangeCheckScope())
				{
					var index = animationComponentClipNameProperty.stringValue != null ? animClipNames.FindIndex(animationComponentClipNameProperty.stringValue) : -1;
					var animationComponentClipIndex = EditorGUI.Popup(position, "Tween clip", index, animClipNames);
					if (GUI.changed && index != animationComponentClipIndex)
					{
						animationComponentClipNameProperty.stringValue = animClipNames[animationComponentClipIndex];
						animationComponentClipNameProperty.serializedObject.ApplyModifiedProperties();
					}
				}
			}
		}

		#endregion
	}
}