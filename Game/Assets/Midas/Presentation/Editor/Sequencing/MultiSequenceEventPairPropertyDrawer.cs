using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.Editor.Utilities;
using Midas.Presentation.Sequencing;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Sequencing
{
	[CustomPropertyDrawer(typeof(MultiSequenceEventPair))]
	public sealed class MultiSequenceEventPairPropertyDrawer : PropertyDrawer
	{
		private static string[] sequenceNames;
		private static IReadOnlyList<(Sequence sequence, IReadOnlyList<(string eventName, int eventId)> events)> selectableSequences;

		private static string[] filteredSequenceNames;
		private static IReadOnlyList<(string eventName, int eventId)> filteredSelectableSequence;

		#region PropertyDrawer Overrides

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var sequenceNamesProperty = property.FindPropertyRelative("sequenceNames");
			var propertyHeight = EditorGUIUtility.singleLineHeight;
			return (5 + sequenceNamesProperty.arraySize - 1) * propertyHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			UpdateSelectableSequences();
			if (sequenceNames == null)
				return;

			using (new PropertyScope(ref position, label, property, false))
			{
				var sequenceIndex = -1;
				var sequenceNamesProperty = property.FindPropertyRelative("sequenceNames");
				var eventANameProperty = property.FindPropertyRelative("eventNameA");
				var eventBNameProperty = property.FindPropertyRelative("eventNameB");

				position.height = EditorGUIUtility.singleLineHeight;
				var halfWidth = position.width / 2.0f;
				var firstX = position.x;
				var secondX = position.x + halfWidth;
				var eightsWidth = position.width / 8.0f;
				var sevenEighthsWidth = position.width - eightsWidth;
				var oColor = GUI.color;

				EditorGUI.LabelField(position, "Sequence");
				if (sequenceNamesProperty.arraySize == 0)
				{
					sequenceNamesProperty.InsertArrayElementAtIndex(0);
					sequenceNamesProperty.serializedObject.ApplyModifiedProperties();
				}

				var prevSelectedSequences = new List<SerializedProperty>();
				for (var i = 0; i < sequenceNamesProperty.arraySize; ++i)
				{
					position.x = firstX;
					position.y += EditorGUIUtility.singleLineHeight;
					position.width = sevenEighthsWidth;
					if (i == 0)
					{
						EditorGUI.BeginChangeCheck();
						sequenceIndex = sequenceNames.FindIndex(sequenceNamesProperty.GetArrayElementAtIndex(i).stringValue);
						if (sequenceIndex == -1)
							GUI.color = Color.red;

						sequenceIndex = EditorGUI.Popup(position, sequenceIndex, sequenceNames);
						if (GUI.changed && sequenceIndex != -1)
						{
							sequenceNamesProperty.GetArrayElementAtIndex(i).stringValue = sequenceNames[sequenceIndex];
							sequenceNamesProperty.GetArrayElementAtIndex(i).serializedObject.ApplyModifiedProperties();
						}

						GUI.color = oColor;

						EditorGUI.EndChangeCheck();
					}
					else
					{
						UpdateFilteredSelectableSequences(sequenceNamesProperty.GetArrayElementAtIndex(0), prevSelectedSequences, sequenceNamesProperty.GetArrayElementAtIndex(i));
						EditorGUI.BeginChangeCheck();
						var currentSequenceNamesProperty = sequenceNamesProperty.GetArrayElementAtIndex(i);
						var seqIndex = filteredSequenceNames.FindIndex(currentSequenceNamesProperty.stringValue);
						var error = false;
						if (seqIndex < 0)
						{
							error = true;
						}
						else if (filteredSelectableSequence != null)
						{
							var currentSelectedSequence = selectableSequences[sequenceNames.FindIndex(currentSequenceNamesProperty.stringValue)];
							error = !filteredSelectableSequence.SequenceEqual(currentSelectedSequence.events);
						}

						if (error)
						{
							GUI.color = Color.red;
						}

						seqIndex = EditorGUI.Popup(position, seqIndex, filteredSequenceNames);
						if (GUI.changed && seqIndex != -1)
						{
							currentSequenceNamesProperty.stringValue = filteredSequenceNames[seqIndex];
							currentSequenceNamesProperty.serializedObject.ApplyModifiedProperties();
						}

						GUI.color = oColor;
						EditorGUI.EndChangeCheck();
					}

					prevSelectedSequences.Add(sequenceNamesProperty.GetArrayElementAtIndex(i));
					position.x = firstX + sevenEighthsWidth;
					position.width = eightsWidth;
					EditorGUI.BeginDisabledGroup(sequenceNamesProperty.arraySize == 1);
					EditorGUI.BeginChangeCheck();
					if (GUI.Button(position, "-"))
					{
						sequenceNamesProperty.DeleteArrayElementAtIndex(i);
						sequenceNamesProperty.serializedObject.ApplyModifiedProperties();
					}

					EditorGUI.EndChangeCheck();
					EditorGUI.EndDisabledGroup();
				}

				EditorGUI.BeginChangeCheck();
				position.y += EditorGUIUtility.singleLineHeight;
				position.x = firstX + sevenEighthsWidth;
				position.width = eightsWidth;

				if (GUI.Button(position, "+"))
				{
					sequenceNamesProperty.InsertArrayElementAtIndex(sequenceNamesProperty.arraySize);
					sequenceNamesProperty.serializedObject.ApplyModifiedProperties();
				}

				EditorGUI.EndChangeCheck();

				position.y += EditorGUIUtility.singleLineHeight;
				position.width = halfWidth;
				position.x = firstX;
				EditorGUI.LabelField(position, new GUIContent("Activates on", "Select a state of the sequence where to activate"));
				position.x = secondX;
				EditorGUI.LabelField(position, new GUIContent("Block sequence in", "Sequence will wait in this state until we are finished"));
				var popupY = position.y + EditorGUIUtility.singleLineHeight;
				//EventA
				if (sequenceIndex != -1)
				{
					var eventPairsOfSequence = selectableSequences[sequenceIndex].events;
					var eventNames = eventPairsOfSequence.Select(i => i.eventName).ToArray();
					var eventAIndex = eventNames.FindIndex(eventANameProperty.stringValue);

					EditorGUI.BeginChangeCheck();
					position.y = popupY;
					position.x = firstX;
					eventAIndex = EditorGUI.Popup(position, eventAIndex, eventNames);
					if (GUI.changed && eventAIndex != -1)
					{
						eventANameProperty.stringValue = eventPairsOfSequence[eventAIndex].eventName;
						eventANameProperty.serializedObject.ApplyModifiedProperties();
					}

					EditorGUI.EndChangeCheck();

					var eventBIndex = eventNames.FindIndex(eventBNameProperty.stringValue);
					EditorGUI.BeginChangeCheck();
					position.y = popupY;
					position.x = secondX;
					eventBIndex = EditorGUI.Popup(position, eventBIndex, eventNames);
					if (GUI.changed && eventBIndex != -1)
					{
						eventBNameProperty.stringValue = eventPairsOfSequence[eventBIndex].eventName;
						eventBNameProperty.serializedObject.ApplyModifiedProperties();
					}

					EditorGUI.EndChangeCheck();
				}
				else
				{
					//show empty and disabled popups
					EditorGUI.BeginDisabledGroup(true);
					position.y = popupY;
					position.x = firstX;
					EditorGUI.Popup(position, 0, new[] { "Select sequence first" });
					position.y = popupY;
					position.x = secondX;
					EditorGUI.Popup(position, 0, new[] { "Select sequence first" });
					EditorGUI.EndDisabledGroup();
				}
			}
		}

		#endregion

		#region Private Methods

		private static void UpdateFilteredSelectableSequences(SerializedProperty firstSelectedSequence, IReadOnlyCollection<SerializedProperty> prevSelectedSequences,
			SerializedProperty currentSelectedSequence)
		{
			filteredSequenceNames = null;
			filteredSelectableSequence = null;
			if (selectableSequences == null)
			{
				return;
			}

			var seqIndex = sequenceNames.FindIndex(firstSelectedSequence.stringValue);

			if (seqIndex < 0)
			{
				filteredSequenceNames = Array.Empty<string>();
				return;
			}

			filteredSelectableSequence = selectableSequences[seqIndex].events;

			var fsn = new List<string>();
			foreach (var s in selectableSequences)
			{
				if (prevSelectedSequences.Any(sp => sp.stringValue == s.sequence.Name))
					continue;

				if (s.sequence.Name == currentSelectedSequence.stringValue ||
					s.events.SequenceEqual(filteredSelectableSequence))
				{
					fsn.Add(s.sequence.Name);
				}
			}

			filteredSequenceNames = fsn.ToArray();
		}

		private static void UpdateSelectableSequences()
		{
			if (selectableSequences != null)
			{
				return;
			}

			selectableSequences = SequenceFinder.SequenceInfo;
			sequenceNames = selectableSequences?.Select(i => i.sequence.Name).ToArray();
		}

		#endregion
	}
}