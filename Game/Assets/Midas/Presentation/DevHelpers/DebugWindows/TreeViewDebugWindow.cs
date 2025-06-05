using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public abstract class TreeViewDebugWindow : DebugWindow
	{
		protected abstract TreeViewControl TreeViewControl { get; set; }

		protected void OnEnable()
		{
			TreeViewControl.Refresh();
		}

		protected void OnDisable()
		{
			TreeViewControl = null;
		}

		protected override void RenderWindowContent()
		{
			TreeViewControl.LabelStyle = LabelStyle;
			TreeViewControl.ButtonStyle = ButtonStyle;
			TreeViewControl.TextFieldStyle = TextFieldStyle;
			TreeViewControl.ToggleStyle = ToggleStyle;

			var yOffset = LabelStyle.lineHeight;
			TreeViewControl.Draw(new Rect(0, yOffset, WindowRect.width, WindowRect.height - yOffset));
		}

		protected override void Reset()
		{
			base.Reset();
			WindowName = "Button Mapping";
			FontSize = 24;
			WindowRect = new Rect(10, 500, 1200, 1000);
			BackgroundColor = new Color(0, 0, 0.5f);
		}
	}
}