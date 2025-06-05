using Midas.Presentation.DevHelpers;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;

namespace Midas.Presentation.Editor.DevHelpers
{
	public sealed class SequencesEditorWindow : TreeViewEditorWindow
	{
		#region Public

		[MenuItem("Window/Midas/Sequences")]
		[MenuItem("Midas/Window/Sequences")]
		public static void ShowWindow()
		{
			GetWindow<SequencesEditorWindow>("Midas-Sequences");
		}

		#endregion

		#region Protected

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new SequencesView();
			set => treeViewControl = value;
		}

		#endregion

		#region Private

		private TreeViewControl treeViewControl;

		#endregion
	}
}