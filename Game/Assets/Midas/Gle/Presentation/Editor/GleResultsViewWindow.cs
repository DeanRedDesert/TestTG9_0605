using Midas.Presentation.DevHelpers;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Midas.Gle.Presentation.Editor
{
	public sealed class GleResultsViewWindow : TreeViewEditorWindow
	{
		private GleResultsView treeViewControl;
		private Font fixedFont;

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= CreateView();
			set => treeViewControl = (GleResultsView)value;
		}

		private GleResultsView CreateView()
		{
			fixedFont = Resources.Load<Font>("CONSOLA");
			return new GleResultsView(fixedFont);
		}

		[MenuItem("Window/Midas/GLE Results Viewer")]
		[MenuItem("Midas/Window/GLE Results Viewer")]
		public static void ShowWindow() => GetWindow<GleResultsViewWindow>("Midas-GLE Results Viewer");
	}
}