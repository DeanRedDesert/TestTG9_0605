using System.Collections.Generic;
using System.Linq;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.Editor.General;
using Midas.Presentation.Editor.Utilities;
using Midas.Presentation.Sequencing;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Sequencing
{
	[CustomPropertyDrawer(typeof(SequenceEventPair))]
	public sealed class SequenceEventPairPropertyDrawer : PropertyDrawer
	{
		private static string[] sequenceNames;
		private static IReadOnlyList<(Sequence sequence, IReadOnlyList<(string eventName, int eventId)> events)> selectableSequences;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var propertyHeight = EditorGUIUtility.singleLineHeight;
			return 3 * propertyHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!UpdateSelectableSequences())
			{
				return;
			}

			using (new PropertyScope(ref position, label, property, false))
			{
				position.height = EditorGUIUtility.singleLineHeight;
				var sequenceIndex = DrawSequence(position, property);

				position.y += EditorGUIUtility.singleLineHeight;
				DrawActivatesOn(position, property, sequenceIndex, "eventNameA");

				position.y += EditorGUIUtility.singleLineHeight;
				DrawBlocksSequenceIn(position, property, sequenceIndex);
			}
		}

		public static void DrawBlocksSequenceIn(Rect position, SerializedProperty property, int sequenceIndex)
		{
			var label = new GUIContent("Block Sequence In State", "Sequence will wait in this state until we are finished");
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			if (sequenceIndex != -1)
			{
				var eventBNameProperty = property.FindPropertyRelative("eventNameB");

				var eventPairsOfSequence = selectableSequences[sequenceIndex].events;
				var eventNames = eventPairsOfSequence.Select(i => i.eventName).ToArray();
				var eventBIndex = eventNames.FindIndex(eventBNameProperty.stringValue);

				using (new EditorGUI.ChangeCheckScope())
				{
					eventBIndex = EditorGUI.Popup(position, eventBIndex, eventNames);
					if (GUI.changed && eventBIndex != -1)
					{
						eventBNameProperty.stringValue = eventPairsOfSequence[eventBIndex].eventName;
						eventBNameProperty.serializedObject.ApplyModifiedProperties();
					}
				}
			}
		}

		public static void DrawActivatesOn(Rect position, SerializedProperty property, int sequenceIndex, string nameProperty)
		{
			var label = new GUIContent("Activates On State", "Select a state of the sequence where to activate");
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			if (sequenceIndex != -1)
			{
				var eventANameProperty = property.FindPropertyRelative(nameProperty);

				var eventPairsOfSequence = selectableSequences[sequenceIndex].events;
				var eventNames = eventPairsOfSequence.Select(i => i.eventName).ToArray();
				var eventAIndex = eventNames.FindIndex(eventANameProperty.stringValue);

				using (new EditorGUI.ChangeCheckScope())
				{
					eventAIndex = EditorGUI.Popup(position, eventAIndex, eventNames);
					if (GUI.changed && eventAIndex != -1)
					{
						eventANameProperty.stringValue = eventPairsOfSequence[eventAIndex].eventName;
						eventANameProperty.serializedObject.ApplyModifiedProperties();
					}
				}
			}
		}

		public static int DrawSequence(Rect position, SerializedProperty property)
		{
			var label = new GUIContent("Sequence", "Select a sequence to use for selecting the states");
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var sequenceNameProperty = property.FindPropertyRelative("sequenceName");
			int sequenceIndex;
			using (new EditorGUI.ChangeCheckScope())
			{
				sequenceIndex = GuiUtil.CheckedPopup(position, sequenceNameProperty.stringValue, sequenceNames);
				if (GUI.changed && sequenceIndex != -1)
				{
					sequenceNameProperty.stringValue = sequenceNames[sequenceIndex];
					sequenceNameProperty.serializedObject.ApplyModifiedProperties();
				}
			}

			return sequenceIndex;
		}

		public static bool UpdateSelectableSequences()
		{
			if (selectableSequences != null)
			{
				return true;
			}

			selectableSequences = SequenceFinder.SequenceInfo;
			sequenceNames = selectableSequences?.Select(i => i.sequence.Name).ToArray();

			return selectableSequences != null;
		}
	}
}