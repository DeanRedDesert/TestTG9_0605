using Midas.Presentation.DevHelpers;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Utilities
{
	public abstract class TreeViewEditorWindow : EditorWindow
	{
		#region Protected

		protected abstract TreeViewControl TreeViewControl { get; set; }

		#endregion

		#region Private

		private void OnGUI()
		{
			TreeViewControl.Draw(new Rect(0, 0, position.width, position.height));

			if (EditorApplication.isPlaying || EditorApplication.isPaused)
				Repaint();
		}

		private void OnEnable()
		{
			TreeViewControl.Refresh();
		}

		private void OnDisable()
		{
			TreeViewControl = null;
		}

		#endregion
	}
}