using Midas.Presentation.DevHelpers;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;

namespace Midas.Presentation.Editor.DevHelpers
{
	public sealed class GameServicesEditorWindow : TreeViewEditorWindow
	{
		private TreeViewControl treeViewControl;

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new GameServicesView();
			set => treeViewControl = value;
		}

		[MenuItem("Window/Midas/Game Services Explorer")]
		[MenuItem("Midas/Window/Game Services Explorer")]
		public static void ShowWindow()
		{
			GetWindow<GameServicesEditorWindow>("Midas-Game Services Explorer");
		}
	}
}