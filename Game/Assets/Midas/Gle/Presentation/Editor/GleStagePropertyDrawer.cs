using System.Linq;
using Logic.Core.Utility;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using GleGameData = Midas.Gle.LogicToPresentation.GleGameData;

namespace Midas.Gle.Presentation.Editor
{
	[CustomPropertyDrawer(typeof(GleStageAttribute))]
	public sealed class GleStagePropertyDrawer : PropertyDrawer
	{
		#region Public

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new PropertyScope(ref position, label, property))
			{
				var options = GleGameData.Stages.Select(b => b.Name).ToArray();
				var index = options.IndexOf(property.stringValue);

				using (new EditorGUI.ChangeCheckScope())
				{
					index = EditorGUI.Popup(position, index, options);
					if (index != -1)
					{
						var selectedStage = options[index];
						if (GUI.changed ||
							selectedStage != property.stringValue) // we compare this for old files where the name was not stored
						{
							property.stringValue = selectedStage;
							property.serializedObject.ApplyModifiedProperties();
						}
					}
				}
			}
		}

		#endregion
	}
}