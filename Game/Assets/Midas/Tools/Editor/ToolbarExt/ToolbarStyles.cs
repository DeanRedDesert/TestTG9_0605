using UnityEngine;

namespace Midas.Tools.Editor.ToolbarExt
{
	public static class ToolbarStyles
	{
		public static readonly GUIStyle CommandButtonStyle;
		public static readonly GUIStyle CommandHighlightedButtonStyle;

		static ToolbarStyles()
		{
			CommandButtonStyle = new GUIStyle("Command")
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold,
				fixedWidth = 0
			};

			CommandHighlightedButtonStyle = new GUIStyle("Command")
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Bold,
				fixedWidth = 0
			};
			CommandHighlightedButtonStyle.normal.textColor = Color.green;
		}
	}
}