using System;
using System.IO;
using Midas.Presentation.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Midas.Presentation.Editor.Audio
{
	[CustomEditor(typeof(SoundDefinitions))]
	public sealed class SoundDefinitionsEditor : UnityEditor.Editor
	{
		private const string SoundDefinitionsEditorAutodetectLoop = "SoundDefinitionsEditor_AutodetectLoop";
		private string newId = string.Empty;
		private AudioClip newClip;
		private AudioMixerGroup mixerGroup;

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Add new SoundDefinition");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Id", GUILayout.Width(25));
			newId = EditorGUILayout.TextField(new GUIContent("", "Unique Id required!"), newId);

			var oldClip = newClip;
			EditorGUILayout.LabelField("Clip", GUILayout.Width(35));
			newClip = (AudioClip)EditorGUILayout.ObjectField(newClip, typeof(AudioClip), false);
			if (newClip != oldClip && newClip != null)
			{
				if (string.IsNullOrEmpty(newId) || oldClip == null || newId == oldClip.name)
				{
					newId = newClip.name;
				}
			}

			EditorGUILayout.EndHorizontal();

			// Enables looping if name or last foldername contains "loop"

			var autodetectLoop = EditorPrefs.GetBool(SoundDefinitionsEditorAutodetectLoop, false);
			autodetectLoop = GUILayout.Toggle(autodetectLoop, "Autodetect Loop", GUILayout.Width(120));
			if (GUI.changed)
			{
				EditorPrefs.SetBool(SoundDefinitionsEditorAutodetectLoop, autodetectLoop);
			}

			var list = serializedObject.FindProperty("soundDefinitionsList");
			var oldEnabled = GUI.enabled;
			GUI.enabled = !string.IsNullOrEmpty(newId) && newClip != null && !HasSoundId(list, newId);
			if (GUILayout.Button("Add", GUILayout.Width(50)))
			{
				list.InsertArrayElementAtIndex(list.arraySize);
				var soundDefinitionProperty = list.GetArrayElementAtIndex(list.arraySize - 1);
				soundDefinitionProperty.FindPropertyRelative("id").stringValue = newId;
				soundDefinitionProperty.FindPropertyRelative("clip").objectReferenceValue = newClip;
				soundDefinitionProperty.FindPropertyRelative("volume").floatValue = 1f;
				soundDefinitionProperty.FindPropertyRelative("group").objectReferenceValue = SoundDefinitionsDatabase.Instance.DefaultAudioMixerGroup;
				if (autodetectLoop)
				{
					soundDefinitionProperty.FindPropertyRelative("isLooped").boolValue =
						newClip != null && newClip.name.IndexOf("loop", StringComparison.OrdinalIgnoreCase) >= 0 ||
						newId.IndexOf("loop", StringComparison.OrdinalIgnoreCase) >= 0;
				}

				serializedObject.ApplyModifiedProperties();
				TryToSwitchToNextClip(newClip, list);
			}

			GUI.enabled = oldEnabled;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Change Mixer Group");
			EditorGUILayout.BeginHorizontal();
			mixerGroup = (AudioMixerGroup)EditorGUILayout.ObjectField(mixerGroup, typeof(AudioMixerGroup), false);

			if (GUILayout.Button("Update All", GUILayout.Width(100)))
			{
				for (var i = 0; i < list.arraySize; i++)
				{
					var soundDefinitionProperty = list.GetArrayElementAtIndex(i);
					soundDefinitionProperty.FindPropertyRelative("group").objectReferenceValue = mixerGroup;
				}

				serializedObject.ApplyModifiedProperties();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			base.OnInspectorGUI();
		}

		private static bool HasSoundId(SerializedProperty list, string id)
		{
			for (var i = 0; i < list.arraySize; i++)
			{
				if (list.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue == id)
				{
					return true;
				}
			}

			return false;
		}

		private static bool HasAudioClip(SerializedProperty list, AudioClip clip)
		{
			for (var i = 0; i < list.arraySize; i++)
			{
				if (list.GetArrayElementAtIndex(i).FindPropertyRelative("clip").objectReferenceValue == clip)
				{
					return true;
				}
			}

			return false;
		}

		private void TryToSwitchToNextClip(AudioClip clip, SerializedProperty list)
		{
			//try to find next clip
			var path = AssetDatabase.GetAssetPath(clip);
			var directory = Path.GetDirectoryName(path);
			var newClipGuid = AssetDatabase.AssetPathToGUID(path);
			newClip = null;
			if (!string.IsNullOrEmpty(newClipGuid))
			{
				var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { directory });
				for (var i = 0; i < guids.Length - 1; i++)
				{
					if (guids[i] == newClipGuid)
					{
						//found the current, now take the next
						var nextClip = (AudioClip)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i + 1]), typeof(AudioClip));
						if (!HasSoundId(list, nextClip.name) && !HasAudioClip(list, nextClip))
						{
							newId = nextClip.name;
							newClip = nextClip;
						}
					}
				}
			}
		}
	}
}