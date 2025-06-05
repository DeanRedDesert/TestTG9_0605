using Midas.Presentation.DevHelpers;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;

namespace Midas.Presentation.Editor.DevHelpers
{
	public sealed class DatabaseEditorWindow : TreeViewEditorWindow
	{
		private TreeViewControl treeViewControl;

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new StatusDatabaseView();
			set => treeViewControl = value;
		}

		[MenuItem("Window/Midas/Status Database Explorer")]
		[MenuItem("Midas/Window/Status Database Explorer")]
		public static void ShowWindow()
		{
			GetWindow<DatabaseEditorWindow>("Midas-Status Database Explorer");
		}
	}
}