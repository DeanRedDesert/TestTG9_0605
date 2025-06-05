using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugGameObjectWindow : TreeViewDebugWindow
	{
		private TreeViewControl treeViewControl;

		protected override void Reset()
		{
			base.Reset();

			WindowName = "Game Objects";
			FontSize = 24;
			WindowRect = new Rect(10, 500, 1200, 1000);
			BackgroundColor = new Color(0, 0.5f, 0.5f);
		}

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new GameObjectView();
			set => treeViewControl = value;
		}
	}
}