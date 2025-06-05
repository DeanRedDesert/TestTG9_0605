using JetBrains.Annotations;
using Midas.Presentation.Sequencing;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Sequencing
{
	[CustomEditor(typeof(SequenceActivator), true)]
	public sealed class SequenceActivatorEditor : UnityEditor.Editor
	{
		private SerializedProperty activateAction;
		private SerializedProperty completionNotifier;
		private SerializedProperty sequenceEventPair;
		private SerializedProperty components;
		private SerializedProperty canBeInterrupted;
		private SerializedProperty interruptPrio;

		private SerializedProperty activateEvents;
		private SerializedProperty resetEvents;

		public override void OnInspectorGUI()
		{
			var darkBack = new GUIStyle { normal = { background = MakeTex(600, 1, new Color(0.15f, 0.15f, 0.15f, 1.0f)) } };

			EditorGUILayout.BeginVertical();
			EditorGUILayout.PropertyField(sequenceEventPair);
			EditorGUILayout.PropertyField(activateAction);
			EditorGUILayout.PropertyField(canBeInterrupted, GUILayout.ExpandWidth(false));
			if (canBeInterrupted.boolValue)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginDisabledGroup(!canBeInterrupted.boolValue);
				EditorGUILayout.PropertyField(interruptPrio, GUILayout.ExpandWidth(false));
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.PropertyField(completionNotifier);
			GUILayout.Label("Components to (De-)Activate");
			for (var compIndx = 0; compIndx < components.arraySize; compIndx++)
			{
				EditorGUILayout.BeginVertical(darkBack);
				SerializedProperty componentProperty = components.GetArrayElementAtIndex(compIndx);
				EditorGUILayout.PropertyField(componentProperty);
				if (GUILayout.Button("-"))
				{
					components.DeleteArrayElementAtIndex(compIndx);
					components.serializedObject.ApplyModifiedProperties();
					activateEvents.DeleteArrayElementAtIndex(compIndx);
					activateEvents.serializedObject.ApplyModifiedProperties();
					resetEvents.DeleteArrayElementAtIndex(compIndx);
					resetEvents.serializedObject.ApplyModifiedProperties();
					return;
				}

				EditorGUILayout.LabelField("OnActivate");
				EditorGUILayout.PropertyField(activateEvents.GetArrayElementAtIndex(compIndx));
				EditorGUILayout.LabelField("OnReset");
				EditorGUILayout.PropertyField(resetEvents.GetArrayElementAtIndex(compIndx));
				EditorGUILayout.EndVertical();
				if (compIndx < components.arraySize - 1)
				{
					EditorGUILayout.Separator();
				}
			}

			if (GUILayout.Button("+"))
			{
				int idx = components.arraySize;
				components.InsertArrayElementAtIndex(idx); //array of IntensityPairs
				var newItem = components.GetArrayElementAtIndex(idx);
				newItem.FindPropertyRelative("intensity").intValue = -1;
				components.serializedObject.ApplyModifiedProperties();
				activateEvents.InsertArrayElementAtIndex(idx);
				activateEvents.serializedObject.ApplyModifiedProperties();
				resetEvents.InsertArrayElementAtIndex(idx);
				resetEvents.serializedObject.ApplyModifiedProperties();
			}

			EditorGUILayout.EndVertical();
			serializedObject.ApplyModifiedProperties();
		}

		private void OnEnable()
		{
			activateAction = serializedObject.FindProperty("action");
			completionNotifier = serializedObject.FindProperty("completionNotifierBehaviour");
			sequenceEventPair = serializedObject.FindProperty("sequenceEventPair");

			activateEvents = serializedObject.FindProperty("activateEvents");
			resetEvents = serializedObject.FindProperty("resetEvents");

			components = serializedObject.FindProperty("components");

			canBeInterrupted = serializedObject.FindProperty("canBeInterrupted");
			interruptPrio = serializedObject.FindProperty("interruptPrio");
		}

		private static Texture2D MakeTex(int width, int height, Color col)
		{
			var pix = new Color[width * height];

			for (var i = 0; i < pix.Length; i++)
			{
				pix[i] = col;
			}

			var result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();

			return result;
		}
	}
}