// Copyright (c) 2022 IGT

#region Usings

using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Audio;
using Midas.Presentation.Editor.General;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;
using UnityEngine;

#endregion

namespace Midas.Presentation.Editor.Audio
{
	[CustomPropertyDrawer(typeof(SoundId))]
	public sealed class SoundIdPropertyDrawer : PropertyDrawer
	{
		private const string Unassigned = "<Unassigned>";
		private readonly List<string> nameList = new List<string>();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var accessor = SoundDefinitionsDatabase.Instance;
			if (accessor == null)
			{
				EditorGUI.HelpBox(position, " There was no GSoundDefinitionsAccessor found in any scene!", MessageType.Error);
				return;
			}

			using (new PropertyScope(ref position, label, property))
			{
				var idProperty = property.FindPropertyRelative("id");
				var definitionsNameProperty = property.FindPropertyRelative("definitionsName");
				var soundId = new SoundId(idProperty.stringValue, definitionsNameProperty.stringValue);

				UpdateNameList(accessor, soundId);

				var searchString = soundId.IsValid ? soundId.ToString() : Unassigned;
				var choiceIndex = nameList.FindIndex(x => x == searchString);

				var popRect = new Rect(position.x, position.y, position.width - 40 - 40 - 20, position.height);
				using (new ChangeGuiBackgroundColorScope(choiceIndex == 0 ? Color.cyan : GUI.backgroundColor))
					using (new EditorGUI.ChangeCheckScope())
					{
						choiceIndex = GuiUtil.CheckedPopup(popRect, searchString, nameList.ToArray());
						if (GUI.changed && choiceIndex != -1)
						{
							idProperty.stringValue = choiceIndex == 0 ? "" : nameList[choiceIndex].Split('/').Last();
							definitionsNameProperty.stringValue = choiceIndex == 0 ? "" : nameList[choiceIndex].Split('/').First();
						}
					}

				var sound = GetSoundSingle(property, choiceIndex, accessor, idProperty, definitionsNameProperty);
				if (sound != null)
				{
					DrawButtons(position, popRect, sound, definitionsNameProperty);
				}
			}
		}

		private void UpdateNameList(SoundDefinitionsDatabase accessor, SoundId soundId)
		{
			if (nameList.Count == 0 || !accessor.HasSound(soundId))
			{
				nameList.Clear();
				nameList.Add(Unassigned);
				nameList.AddRange(accessor.GetAllSoundNames());
			}
		}

		private static void DrawButtons(Rect position, Rect popRect, SoundSingle sound, SerializedProperty definitionsNameProperty)
		{
			if (GUI.Button(new Rect(popRect.xMax, position.y, 40, EditorGUIUtility.singleLineHeight), "Play", EditorStyles.miniButton))
			{
				SoundUtil.PlayClip(sound.Clip);
			}

			if (GUI.Button(new Rect(popRect.xMax + 40, position.y, 40, EditorGUIUtility.singleLineHeight), "Stop", EditorStyles.miniButton))
			{
				SoundUtil.StopAllClips();
			}

			if (GUI.Button(new Rect(popRect.xMax + 40 + 40, position.y, 20, EditorGUIUtility.singleLineHeight),
					new GUIContent("o", "Goto Definition"), EditorStyles.miniButton))
			{
				// ReSharper disable once RedundantAssignment Needed for Unity compiler
				var sd = default(SoundDefinitionsBase);
				if (SoundDefinitionsDatabase.Instance.SoundDefinitions?.TryGetValue(definitionsNameProperty.stringValue, out sd) == true)
					Selection.activeObject = sd;
			}
		}

		private static SoundSingle GetSoundSingle(SerializedProperty property, int choiceIndex, SoundDefinitionsDatabase accessor, SerializedProperty idProperty,
			SerializedProperty definitionsNameProperty)
		{
			SoundSingle sound = null;
			if (choiceIndex > 0)
			{
				if (property.serializedObject.targetObject is SoundPlayer gsound)
				{
					sound = gsound.Sound as SoundSingle;
				}
				else
				{
					sound = accessor.CreateSound(new SoundId(idProperty.stringValue, definitionsNameProperty.stringValue)) as SoundSingle;
				}
			}

			return sound;
		}
	}
}