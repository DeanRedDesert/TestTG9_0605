using Midas.Ascent.Ugp;
using Midas.Tools.Editor.ToolbarExt;
using UnityEditor;
using UnityEngine;
using static Midas.Ascent.Editor.Ugp.MachineConfigurationData;

namespace Midas.Ascent.Editor.Ugp
{
	[InitializeOnLoad]
	public class JurisdictionToolbar : IToolbarExtension
	{
		private Jurisdictions? lastJurisdiction;

		static JurisdictionToolbar()
		{
			ToolbarExtender.RightToolbarGUI.Add(new JurisdictionToolbar());
		}

		public bool IsDirty => lastJurisdiction != LastJurisdiction;

		public void OnGui()
		{
			var wasEnabled = GUI.enabled;

			if (EditorApplication.isPlaying)
			{
				GUI.enabled = false;
				EditorGUILayout.EnumPopup(LastJurisdiction, GUILayout.Width(100));
				GUI.enabled = wasEnabled;
			}
			else
			{
				var jur = (Jurisdictions)EditorGUILayout.EnumPopup(LastJurisdiction, GUILayout.Width(100));
				if (jur != LastJurisdiction)
				{
					UpdateJurisdictionData(jur, AustralianFoundationSettings.AscentOverrideSettings, AustralianFoundationSettings.MachineSettings, AustralianFoundationSettings.ReserveSettings, AustralianFoundationSettings.PidSettings, AustralianFoundationSettings.GameFunctionStatusSettings);
					StandaloneAustralianFoundationSettings.Save(AustralianFoundationSettings);
				}
			}

			lastJurisdiction = LastJurisdiction;
		}
	}
}