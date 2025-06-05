using Midas.Presentation.DevHelpers;
using Midas.Presentation.Editor.Utilities;
using UnityEditor;

namespace Midas.Presentation.Editor.DevHelpers
{
	public sealed class StateMachineEditorWindow : TreeViewEditorWindow
	{
		#region Public

		[MenuItem("Window/Midas/State Machine Explorer")]
		[MenuItem("Midas/Window/State Machine Explorer")]
		public static void ShowWindow()
		{
			GetWindow<StateMachineEditorWindow>("Midas-State Machine Explorer");
		}

		#endregion

		#region Protected

		protected override TreeViewControl TreeViewControl
		{
			get => treeViewControl ??= new StateMachineView();
			set => treeViewControl = value;
		}

		#endregion

		#region Private

		private TreeViewControl treeViewControl;

		#endregion
	}
}