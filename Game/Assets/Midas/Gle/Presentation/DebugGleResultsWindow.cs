using Midas.Presentation.DevHelpers;
using Midas.Presentation.DevHelpers.DebugWindows;
using UnityEngine;

namespace Midas.Gle.Presentation
{
	public sealed class DebugGleResultsWindow : TreeViewDebugWindow
	{
		private TreeViewControl treeViewControl;

		protected override void Reset()
		{
			base.Reset();

			WindowName = "GLE Results Viewer";
			FontSize = 24;
			WindowRect = new Rect(10, 500, 1200, 1000);
			BackgroundColor = new Color(0, 0.5f, 0.5f);
		}

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new GleResultsView(Font);
			set => treeViewControl = value;
		}
	}
}