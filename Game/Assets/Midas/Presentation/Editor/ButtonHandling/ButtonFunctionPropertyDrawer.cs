using System.Linq;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.ButtonHandling
{
	[CustomPropertyDrawer(typeof(ButtonFunction))]
	public sealed class ButtonFunctionPropertyDrawer : PropertyDrawer
	{
		#region Public

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			using (new PropertyScope(ref position, label, property))
			{
				var idProperty = property.FindPropertyRelative("id");
				var nameProperty = property.FindPropertyRelative("name");
				if (idProperty != null && nameProperty != null)
				{
					if (string.IsNullOrEmpty(nameProperty.stringValue))
					{
						nameProperty.stringValue = ButtonHelpers.GetButtonFunctionNameOfId(idProperty.intValue);
						nameProperty.serializedObject.ApplyModifiedProperties();
					}

					var index = ButtonHelpers.GetButtonFunctionIndexOfId(idProperty.intValue);
					var options = ButtonHelpers.AllButtonFunctions.Select(b => b.PathName).ToArray();

					using (new EditorGUI.ChangeCheckScope())
					{
						index = EditorGUI.Popup(position, index, options);
						if (GUI.changed && index != -1)
						{
							var data = ButtonHelpers.AllButtonFunctions[index];
							idProperty.intValue = data.Function.Id;
							idProperty.serializedObject.ApplyModifiedProperties();
							nameProperty.stringValue = data.Function.Name;
							nameProperty.serializedObject.ApplyModifiedProperties();
						}
					}
				}
			}
		}

		#endregion
	}
}