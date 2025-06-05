using Midas.Presentation.DevHelpers;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;

namespace Midas.Presentation.Editor.DevHelpers
{
	public sealed class GameObjectEditorWindow : TreeViewEditorWindow
	{
		private TreeViewControl treeViewControl;

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new GameObjectView();
			set => treeViewControl = value;
		}

		[MenuItem("Window/Midas/Game Object Explorer")]
		[MenuItem("Midas/Window/Game Object Explorer")]
		public static void ShowWindow()
		{
			GetWindow<GameObjectEditorWindow>("Midas-Game Object Explorer");
		}
	}
}