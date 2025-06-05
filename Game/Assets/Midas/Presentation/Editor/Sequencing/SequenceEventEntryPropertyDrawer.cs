using Midas.Presentation.Editor.Utilities;
using Midas.Presentation.Sequencing;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Sequencing
{
	[CustomPropertyDrawer(typeof(SequenceEvent))]
	public sealed class SequenceEventEntryPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 2 * EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!SequenceEventPairPropertyDrawer.UpdateSelectableSequences())
				return;

			using (new PropertyScope(ref position, label, property, false))
			{
				position.height = EditorGUIUtility.singleLineHeight;
				var sequenceIndex = SequenceEventPairPropertyDrawer.DrawSequence(position, property);

				position.y += EditorGUIUtility.singleLineHeight;
				SequenceEventPairPropertyDrawer.DrawActivatesOn(position, property, sequenceIndex, "eventName");
			}
		}
	}
}