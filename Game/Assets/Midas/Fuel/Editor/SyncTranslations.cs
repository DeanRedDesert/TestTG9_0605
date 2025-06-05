using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Fuel.Data;
using IGT.Game.Fuel.Data.TranslationTable;
using Midas.Fuel.Editor.Screenshot;
using Midas.Tools.Editor;
using UnityEngine.Localization.Settings;

namespace Midas.Fuel.Editor
{
	public static class SyncTranslations
	{
		/// <summary>
		/// Coroutine for sync.
		/// </summary>
		private static IEnumerator syncRoutine;

		public static void ExportFuelPackage(string themeId, string themeName)
		{
			syncRoutine = EditorCoroutineRunner.StartEditorCoroutine(RunExport(themeId, themeName, true));
		}

		/// <summary>
		/// Sets up the GUI and then displays it
		/// </summary>
		private static IEnumerator RunExport(string themeId, string themeName, bool closeOnEnd)
		{
			var syncData = new SyncData();

			DatabaseSyncWindow.HeadingText = "Exporting fuel package...";
			DatabaseSyncWindow.ShowWindow(syncData);

			IReadOnlyList<TranslationInformation> screenshotData = null;

			syncData.Status = "Running Screenshot process on game.";

			yield return EditorCoroutineRunner.StartEditorCoroutine(ScreenshotController.GetScreenshots(syncData, screenshotInfo => screenshotData = screenshotInfo));
			yield return EditorCoroutineRunner.StartEditorCoroutine(ExportTranslations(themeId, themeName, syncData, screenshotData));

			if (closeOnEnd)
			{
				DatabaseSyncWindow.HideWindow();
			}
		}

		private static IEnumerator ExportTranslations(string themeId, string themeName, SyncData syncStatus, IReadOnlyList<TranslationInformation> translationInformation)
		{
			syncStatus.Status = "Initializing sync data.";
			syncStatus.Progress = 0;

			yield return null;

			syncStatus.Status = "Exporting Supported Languages";

			var exportData = new FuelPackage
			{
				GameThemeId = themeId,
				FuelPackageType = PackageType.full,
				SupportedLanguages = LocalizationSettings.AvailableLocales.Locales.Select(l => l.Identifier.Code).ToList(),
				ThemeName = string.IsNullOrEmpty(themeName) ? string.Empty : themeName,
			};

			syncStatus.Status = "Getting all game translations";

			yield return null;

			foreach (var ti in translationInformation)
			{
				var tiCopy = ti.Copy();
				tiCopy.ResourceLocation = System.IO.Path.GetFileName(tiCopy.ResourceLocation);
				exportData.Translations.Add(tiCopy);
			}

			syncStatus.Status = "Saving fuel package";

			yield return null;
			yield return EditorCoroutineRunner.StartEditorCoroutine(exportData.GeneratePackage(exportData.Translations, syncStatus));
			yield return null;

			DatabaseSyncWindow.HideWindow();
		}

		/// <summary>
		/// Cancel sync process.
		/// </summary>
		public static void CancelSync()
		{
			if (syncRoutine != null)
			{
				EditorCoroutineRunner.CancelEditorCoroutine(syncRoutine);
				syncRoutine = null;
			}
		}
	}
}