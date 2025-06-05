using UnityEditor;
using UnityEngine;

namespace Midas.Ascent.Editor
{
	/// <summary>
	/// This class adds the menu items for generating the appropriate system file and registry files.
	/// </summary>
	public static class GenerateSystemFileMenu
	{
		[MenuItem("Midas/Configuration/Generate SystemConfig.xml")]
		public static void GenerateRegistryFiles()
		{
			var editor = EditorWindow.GetWindow(typeof(GenerateSettingsEditor), false, "Generate SystemConfig.xml", true) as GenerateSettingsEditor;

			if (editor == null)
				return;

			editor.Init();
			editor.Show();
		}

		[MenuItem("Midas/Configuration/Generate SystemConfig.xml", validate = true)]
		public static bool ValidateGenerateSystemConfigFile()
		{
			return !Application.isPlaying;
		}
	}
}