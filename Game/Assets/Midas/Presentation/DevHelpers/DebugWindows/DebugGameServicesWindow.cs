using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugGameServicesWindow : TreeViewDebugWindow
	{
		private TreeViewControl gameServicesView;

		protected override void Reset()
		{
			base.Reset();

			WindowName = "Game Services";
			FontSize = 24;
			WindowRect = new Rect(10, 500, 1200, 1000);
			BackgroundColor = new Color(0, 0.5f, 0.5f);
		}

		protected override TreeViewControl TreeViewControl
		{
			get => gameServicesView ??= new GameServicesView();
			set => gameServicesView = value;
		}
	}
}