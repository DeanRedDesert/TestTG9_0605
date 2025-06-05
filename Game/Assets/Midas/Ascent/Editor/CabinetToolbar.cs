using Midas.Tools.Editor.ToolbarExt;
using UnityEditor;
using UnityEngine;

namespace Midas.Ascent.Editor
{
	[InitializeOnLoad]
	public class CabinetToolbar : IToolbarExtension
	{
		private CabinetConfig? lastCabinetConfig;

		static CabinetToolbar()
		{
			ToolbarExtender.RightToolbarGUI.Add(new CabinetToolbar());
		}

		public bool IsDirty => lastCabinetConfig != CabinetSetupSwitcher.GetCurrentCabinet();

		public void OnGui()
		{
			var wasEnabled = GUI.enabled;

			if (EditorApplication.isPlaying)
			{
				GUI.enabled = false;
				EditorGUILayout.EnumPopup(CabinetSetupSwitcher.GetCurrentCabinet(), GUILayout.Width(100));
				GUI.enabled = wasEnabled;
			}
			else
			{
				var cab = (CabinetConfig)EditorGUILayout.EnumPopup(CabinetSetupSwitcher.GetCurrentCabinet(), GUILayout.Width(100));
				if (cab != CabinetSetupSwitcher.GetCurrentCabinet())
				{
					CabinetSetupSwitcher.SetCabinet(cab);
				}
			}

			lastCabinetConfig = CabinetSetupSwitcher.GetCurrentCabinet();
		}
	}
}