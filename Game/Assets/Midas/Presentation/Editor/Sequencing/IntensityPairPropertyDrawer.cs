using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.DevHelpers;
using Midas.Presentation.Editor.Utilities;
using Midas.Presentation.Sequencing;
using Midas.Presentation.Tween;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Sequencing
{
	[CustomPropertyDrawer(typeof(ComponentSelectorAttribute))]
	public class ComponentSelectorPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			var compPos = new Rect(position.x, position.y, position.width / 2, position.height);
			var comp = (Component)EditorGUI.ObjectField(compPos, property.objectReferenceValue, typeof(Component), true);
			if (comp)
			{
				var options = comp.GetComponents(typeof(Component));
				var selectedIndex = options.FindIndex(comp);
				var optionPos = new Rect(compPos.xMax, position.y, position.xMax - compPos.xMax, position.height);
				selectedIndex = EditorGUI.Popup(optionPos, selectedIndex, options.Select(o => o.GetType().Name).ToArray());
				comp = options[selectedIndex];
				if (GUI.changed && comp != property.objectReferenceValue)
				{
					property.objectReferenceValue = comp;
					property.serializedObject.ApplyModifiedProperties();
				}
			}

			EditorGUI.EndChangeCheck();
		}
	}

	[CustomPropertyDrawer(typeof(SequenceComponent))]
	public sealed class IntensityDetailsPropertyDrawer : PropertyDrawer
	{
		#region Public

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new PropertyScope(ref position, label, property, false))
			{
				var intensityProperty = property.FindPropertyRelative("intensity");
				var compProperty = property.FindPropertyRelative("component");
				var animationComponentClipIndexProperty = property.FindPropertyRelative("animationComponentClipIndex");
				var animationComponentClipNameProperty = property.FindPropertyRelative("animationComponentClipName");
				var durationOverrideProperty = property.FindPropertyRelative("durationOverride");
				var durationProperty = property.FindPropertyRelative("duration");

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("At Intensity", GUILayout.ExpandWidth(false));
				EditorGUI.BeginChangeCheck();
				var intensity = Math.Max(-1, Math.Min(EditorGUILayout.IntField(intensityProperty.intValue, GUILayout.MaxWidth(20)), 10));
				if (GUI.changed && intensity != intensityProperty.intValue)
				{
					intensityProperty.intValue = intensity;
					intensityProperty.serializedObject.ApplyModifiedProperties();
				}

				EditorGUI.EndChangeCheck();
				EditorGUILayout.PropertyField(compProperty);

				switch (compProperty.objectReferenceValue)
				{
					case Animation anim:
						ShowAnimation(anim, animationComponentClipIndexProperty, animationComponentClipNameProperty);
						break;
					// case GAnimation gAnim:
					// 	ShowGAnimation(gAnim, animationComponentClipIndexProperty, animationComponentClipNameProperty);
					// 	break;
					case TweenAnimation gTweenAnim:
						ShowTweenAnimation(gTweenAnim, animationComponentClipNameProperty, animationComponentClipIndexProperty);
						break;
				}

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();

				EditorGUI.BeginChangeCheck();
				var durationOverride = GUILayout.Toggle(durationOverrideProperty.boolValue, "Auto-Reset with WC, offset ms");
				if (GUI.changed && durationOverride != durationOverrideProperty.boolValue)
				{
					durationOverrideProperty.boolValue = durationOverride;
					durationOverrideProperty.serializedObject.ApplyModifiedProperties();
				}

				EditorGUI.EndChangeCheck();
				EditorGUI.BeginChangeCheck();
				var duration = EditorGUILayout.FloatField(durationProperty.floatValue);
				if (GUI.changed && duration != durationProperty.floatValue)
				{
					durationProperty.floatValue = duration;
				}

				EditorGUI.EndChangeCheck();
				EditorGUILayout.EndHorizontal();
			}
		}

		#endregion

		#region Private

		private static void ShowAnimationCommon(List<string> animClipNames, SerializedProperty animationComponentClipNameProperty,
			SerializedProperty animationComponentClipIndexProperty)
		{
			EditorGUI.BeginChangeCheck();
			var index = animClipNames.IndexOf(animationComponentClipNameProperty.stringValue);
			if (index == -1 && animationComponentClipIndexProperty.intValue < animClipNames.Count)
			{
				index = animationComponentClipIndexProperty.intValue;
			}

			var animationComponentClipIndex = EditorGUILayout.Popup(index, animClipNames.ToArray());
			if (GUI.changed)
			{
				animationComponentClipIndexProperty.intValue = animationComponentClipIndex;
				animationComponentClipIndexProperty.serializedObject.ApplyModifiedProperties();
				animationComponentClipNameProperty.stringValue = animClipNames[animationComponentClipIndex];
				animationComponentClipNameProperty.serializedObject.ApplyModifiedProperties();
			}
			else if (animationComponentClipIndex >= 0 && animClipNames[animationComponentClipIndex] != animationComponentClipNameProperty.stringValue)
			{
				animationComponentClipNameProperty.stringValue = animClipNames[animationComponentClipIndex];
				animationComponentClipNameProperty.serializedObject.ApplyModifiedProperties();
			}
			else if (animationComponentClipIndex >= 0 && animationComponentClipIndex != animationComponentClipIndexProperty.intValue)
			{
				animationComponentClipIndexProperty.intValue = animationComponentClipIndex;
				animationComponentClipIndexProperty.serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.EndChangeCheck();
		}

		private static void ShowTweenAnimation(TweenAnimation gTweenAnim, SerializedProperty animationComponentClipNameProperty,
			SerializedProperty animationComponentClipIndexProperty)
		{
			var animClipNames = new List<string>();
			foreach (var tweenClip in gTweenAnim.GetTweenClips())
			{
				animClipNames.Add(tweenClip.Name);
			}

			ShowAnimationCommon(animClipNames, animationComponentClipNameProperty, animationComponentClipIndexProperty);
		}

		// private static void ShowGAnimation(GAnimation gAnim, SerializedProperty animationComponentClipIndexProperty, SerializedProperty animationComponentClipNameProperty)
		// {
		// 	var animClipNames = new List<string>();
		// 	foreach (AnimationClip clip in gAnim.Clips)
		// 	{
		// 		animClipNames.Add(clip.name);
		// 	}
		//
		// 	ShowAnimationCommon(animClipNames, animationComponentClipNameProperty, animationComponentClipIndexProperty);
		// }

		private static void ShowAnimation(Animation anim, SerializedProperty animationComponentClipIndexProperty, SerializedProperty animationComponentClipNameProperty)
		{
			var animClipNames = new List<string>();
			foreach (AnimationState state in anim)
			{
				animClipNames.Add(state.clip.name);
			}

			ShowAnimationCommon(animClipNames, animationComponentClipNameProperty, animationComponentClipIndexProperty);
		}

		#endregion
	}
}