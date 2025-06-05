using System;
using UnityEditor;
using UnityEngine;

namespace Midas.CreditPlayoff.Editor
{
	public sealed class PropertyScope : IDisposable
	{
		private bool disposeValue;

		public GUIContent Content { get; }

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

		~PropertyScope()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposeValue)
			{
				if (disposing)
				{
					EditorGUI.EndProperty();
				}

				disposeValue = true;
			}
		}
	}
}