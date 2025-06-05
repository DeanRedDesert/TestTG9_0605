using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.General
{
	public sealed class ScrollViewScope : EditorGUILayout.ScrollViewScope
	{
		#region Public

		public ScrollViewScope(ref Vector2 scrollPosition, params GUILayoutOption[] options)
			: base(scrollPosition, options)
		{
			scrollPosition = this.scrollPosition;
		}

		#endregion
	}
}