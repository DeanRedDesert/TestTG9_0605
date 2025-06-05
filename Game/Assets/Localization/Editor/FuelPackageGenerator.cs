using Game;
using Midas.Ascent.Editor;
using Midas.Fuel.Editor;
using UnityEditor;
using UnityEngine;

namespace Localization.Editor
{
	public static class FuelPackageGenerator
	{
		[MenuItem("Midas/Fuel/Generate Full Package", validate = true)]
		public static bool CanGenerateFull()
		{
			if (Application.isPlaying)
				return false;

			var sysConfig = SystemConfigurationHelper.Load();
			return sysConfig.PaytableList != null && sysConfig.PaytableList.Count > 0;
		}

		[MenuItem("Midas/Fuel/Generate Full Package")]
		public static void GenerateFull()
		{
			var themeName = GameConfigurator.GameName;
			var sysConfig = SystemConfigurationHelper.Load();
			var themeId = sysConfig.PaytableList[0].ThemeIdentifier;

			SyncTranslations.ExportFuelPackage(themeId, themeName);
		}
	}
}