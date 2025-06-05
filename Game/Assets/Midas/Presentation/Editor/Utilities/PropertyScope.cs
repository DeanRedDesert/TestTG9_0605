using System;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Utilities
{
	public sealed class PropertyScope : IDisposable
	{
		#region Public

		public PropertyScope(ref Rect position, GUIContent label, SerializedProperty property, bool showPrefixLabel = true)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			Content = EditorGUI.BeginProperty(position, label, property);
			if (showPrefixLabel && label != GUIContent.none)
			{
				position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			}
		}

		public void Dispose()
		{
			EditorGUI.EndProperty();
		}

		public GUIContent Content { get; }

		#endregion
	}
}