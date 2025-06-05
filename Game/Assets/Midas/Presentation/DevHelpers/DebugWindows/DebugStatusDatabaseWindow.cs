using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugStatusDatabaseWindow : TreeViewDebugWindow
	{
		private TreeViewControl treeViewControl;

		protected override void Reset()
		{
			base.Reset();

			WindowName = "Status Database";
			FontSize = 24;
			WindowRect = new Rect(10, 500, 1200, 1000);
			BackgroundColor = new Color(0, 0.5f, 0.5f);
		}

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new StatusDatabaseView();
			set => treeViewControl = value;
		}
	}
}