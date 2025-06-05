using Midas.Presentation.DevHelpers;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;

namespace Midas.Presentation.Editor.DevHelpers
{
	public sealed class ButtonStatusEditorWindow : TreeViewEditorWindow
	{
		private TreeViewControl treeViewControl;

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new ButtonStatusView();
			set => treeViewControl = value;
		}

		[MenuItem("Window/Midas/Button Status")]
		[MenuItem("Midas/Window/Button Status")]
		public static void ShowWindow()
		{
			GetWindow<ButtonStatusEditorWindow>("Midas-Button Status");
		}
	}
}