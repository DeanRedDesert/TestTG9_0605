using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IGT.Game.Fuel.Data.TranslationTable;
using Midas.Tools;
using Midas.Tools.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace Midas.Fuel.Editor.Screenshot
{
	public static class ScreenshotController
	{
		/// <summary>
		/// Method which can be used to notify others that the screenshots have completed
		/// And send them the results.
		/// </summary>
		/// <param name="screenshotTranslationInformation">Screenshot info collected during the screenshot process.</param>
		public delegate void ScreenshotsComplete(IReadOnlyList<TranslationInformation> screenshotTranslationInformation);

		#region Private Members

		/// <summary>
		/// Backing field for <see cref="ScreenshotableTranslators"/>.
		/// </summary>
		private static List<IScreenshotableTranslator> screenshotableTranslators;

		/// <summary>
		/// Class to hold any info needed by the database or server that will be sent
		/// with the screenshots
		/// </summary>
		private static List<TranslationInformation> gameScreenshotInfo;

		/// <summary>
		/// Method which can be used to tell a caller that the screenshots have completed.
		/// </summary>
		private static ScreenshotsComplete completeCallback;

		/// <summary>
		/// The screenshot routine that is being run.
		/// </summary>
		private static IEnumerator executingRoutine;

		/// <summary>
		/// The original main monitor screen height before we forced it to 1080.
		/// </summary>
		private static int originalMainMonitorScreenHeight;

		/// <summary>
		/// The original main monitor screen width before we forced it to 1920.
		/// </summary>
		private static int originalMainMonitorScreenWidth;

		#region Control Variables

		/// <summary>
		/// The scenes that were loaded when the tool started.
		/// </summary>
		private static List<string> previouslyLoadedScenes;

		/// <summary>
		/// The scene that was active when the tool started.
		/// </summary>
		private static string previousActiveScene;

		/// <summary>
		/// Close the status window when screenshots are finished.
		/// </summary>
		private static bool closeStatusWindow = true;

		#endregion

		/// <summary>
		/// Keep track of and update the status message and progress for the status window.
		/// </summary>
		private sealed class ProgressStatus
		{
			private string message = string.Empty;
			private string languageName = string.Empty;
			private string translatorObject = string.Empty;
			private int processed;

			/// <summary>
			/// Sync data for the current progress information.
			/// </summary>
			public SyncData SyncStatus { get; set; }

			/// <summary>
			/// Set the main message to be displayed.
			/// </summary>
			public string Message
			{
				set
				{
					message = value;
					UpdateStatus();
				}
			}

			/// <summary>
			/// Set the current language name.
			/// </summary>
			public string LanguageName
			{
				set
				{
					languageName = value;
					UpdateStatus();
				}
			}

			/// <summary>
			/// Set the current game object being screenshot.
			/// </summary>
			public string TranslatorObject
			{
				set
				{
					translatorObject = value;
					UpdateStatus();
				}
			}

			/// <summary>
			/// The number of screenshots that will be taken for this scene.
			/// </summary>
			public int TotalScreenshots { get; set; }

			/// <summary>
			/// Update the messages in the sync status window.
			/// </summary>
			private void UpdateStatus()
			{
				SyncStatus.Status =
					"Screenshots can take a while to complete.\r\n" +
					"\r\n" +
					$"{message}" +
					"\r\n" +
					$"\tLanguage: {languageName}\r\n" +
					$"\tGame Object: {translatorObject}\r\n" +
					$"\tScreenshot {processed} of {TotalScreenshots}";

				if (TotalScreenshots > 0)
				{
					SyncStatus.Progress = 100 * processed / TotalScreenshots;
				}
				else
				{
					SyncStatus.Progress = 0;
				}
			}

			/// <summary>
			/// Increment the number of screenshots processed and update the status.
			/// </summary>
			public void IncrementProcessed()
			{
				processed++;
				UpdateStatus();
			}

			/// <summary>
			/// Reset the number processed back to zero.
			/// </summary>
			public void ResetProcessed()
			{
				processed = 0;
				UpdateStatus();
			}
		}

		private static ProgressStatus status;

		#endregion

		/// <summary>
		/// Run screenshot system on the game.
		/// </summary>
		/// <param name="callback">Method which will be called when the screenshots have completed.</param>
		/// <param name="syncStatus">
		/// This is the SyncData object that enables communication between user and Coroutine.
		/// </param>
		public static IEnumerator GetScreenshots(
			SyncData syncStatus,
			ScreenshotsComplete callback)
		{
			LoadScenesForScreenshots();

			IntilializeTranslators();

			closeStatusWindow = false;

			executingRoutine = SetupScreenshotCoroutine(syncStatus, callback);

			yield return EditorCoroutineRunner.StartEditorCoroutine(executingRoutine);
		}

		/// <summary>
		/// Translators that will have screenshots taken.
		/// </summary>
		private static IEnumerable<IScreenshotableTranslator> ScreenshotableTranslators
		{
			get
			{
				return screenshotableTranslators.Where(translator => translator != null && translator.GameObject != null);
			}
			set
			{
				screenshotableTranslators = value.Where(translator => translator != null && translator.GameObject != null).ToList();
			}
		}

		#region Menu Option

		/// <summary>
		/// Development menu option to run the screen shot tool.
		/// </summary>
		[MenuItem("Midas/Fuel/Test All Screenshots")]
		private static void DevelopmentScreenshotOnProject()
		{
			// Prompt user to save.  Do not screenshot if canceled.
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				var statusHeader = "Taking Translation Screenshots...";

				LoadScenesForScreenshots();
				IntilializeTranslators();

				executingRoutine = SetupScreenshotCoroutine(statusHeader);

				EditorCoroutineRunner.StartEditorCoroutine(executingRoutine);
			}
		}

		#endregion

		/// <summary>
		/// Load scenes that screenshots will be taken in.
		/// </summary>
		private static void LoadScenesForScreenshots()
		{
			LoadAllScenesInBuild(out previouslyLoadedScenes, out previousActiveScene);
		}

		/// <summary>
		/// Ensure all translators have been initialized.
		/// </summary>
		private static void IntilializeTranslators()
		{
			LocalizationSettings.SelectedLocale = LocalizationSettings.ProjectLocale;
			// // Ensure all translators have been initialized.
			// FuelEditorUtilities.ApplyActionLoadedScenes<ContentTranslator>(
			// 	(translators) =>
			// 	{
			// 		foreach (var translator in translators)
			// 		{
			// 			translator.Init();
			//
			// 			if (translator.HasChanges)
			// 			{
			// 				Debug.Log(
			// 					$"Initialized Translation data for {translator.gameObject.name}",
			// 					translator.gameObject);
			// 			}
			// 		}
			// 	});
			//
			// EditorSceneManager.SaveOpenScenes();
		}

		/// <summary>
		/// Sets up the game to be screenshot
		/// This will change the game to the first scene in the build settings, and
		/// Create a new folder to store screenshots in the UnityProject folder.
		///
		/// It will then cycle through all scenes looking for translated content
		/// and screenshot them individually if they have a static visual representation.
		/// If there is no visual aspect (Audio files) then the file itself will be saved.
		/// It will then close the Unity Editor when all content has been screenshot
		/// and saved to the created folder
		/// </summary>
		private static IEnumerator Init(SyncData syncStatus)
		{
			var initMessage = "Initializing Screenshot system";
			Debug.Log(initMessage);
			syncStatus.Status = initMessage;

			//Get the current game name to create a folder to use when sending the
			//screenshots to the server
			gameScreenshotInfo = new List<TranslationInformation>();

			syncStatus.Status = "Screenshot system initialized";

			//Save old monitor settings in PlayerSettings
			originalMainMonitorScreenHeight = PlayerSettings.defaultScreenHeight;
			originalMainMonitorScreenWidth = PlayerSettings.defaultScreenWidth;

			//Force them to our screenshot width and height.
			PlayerSettings.defaultScreenHeight = ScreenshotUtilities.ScreenshotMonitorHeight;
			PlayerSettings.defaultScreenWidth = ScreenshotUtilities.ScreenshotMonitorWidth;

			yield return EditorCoroutineRunner.StartEditorCoroutine(RunScreenshot());
		}

		/// <summary>
		/// Run the screenshot system, yield when a delay is required to update the screen.
		/// </summary>
		private static IEnumerator RunScreenshot()
		{
			status.Message = "Taking Screenshots:";

			try
			{
				status.SyncStatus.RunningScreenshots = true;

				// Configure the scene so we can take screen shots.
				if (SetUpSceneForScreenshots())
				{
					var idsInScene = new HashSet<string>();

					foreach (var translator in screenshotableTranslators.Where(trans => !trans.IgnoreScreenshot))
					{
						idsInScene.Add(translator.TranslationId);
					}

					status.TotalScreenshots = idsInScene.Count * LocalizationSettings.AvailableLocales.Locales.Count;
					status.ResetProcessed();

					// Step through each language.
					yield return EditorCoroutineRunner.StartEditorCoroutine(RunLanguageScreenshot());
				}

				status.SyncStatus.Status = "Saving screenshot data...";

				var completeMessage = "Screenshots Complete!";
				Debug.Log(completeMessage);
				status.SyncStatus.Status = completeMessage;

				completeCallback?.Invoke(gameScreenshotInfo);

				status.SyncStatus.RunningScreenshots = false;
			}
			finally
			{
				// Make sure OnExitCleanUp is called to prevent flags from locking the code out of future runs.
				OnExitCleanUp();
			}
		}

		/// <summary>
		/// Translate all objects in the scene.
		/// </summary>
		/// <param name="locale">Target language to translate to.</param>
		/// <returns>Enumerator with a time to delay before running the next step</returns>
		private static IEnumerator Translate(Locale locale)
		{
			status.LanguageName = locale.Identifier.Code;

			// Enable all objects so they can be translated.
			EnableAllSceneObjectsForScreenshots();

			try
			{
				LocalizationSettings.SelectedLocale = locale;
			}
			catch (Exception ex)
			{
				Debug.LogError(
					"Screenshot exception: Change language failed." +
					$" Language:{locale.Identifier.Code}" +
					$" Reason: {ex.Message}\n" +
					$"Stack:\n{ex.StackTrace}");
			}

			yield return new EditorCoroutineRunner.WaitForFrames(3);

			// Disable all objects so that only what is needed will be shown for individual screenshots.
			DisableAllSceneObjectsForScreenshots();
		}

		/// <summary>
		/// Perform screen shot operations for each language supported by this game.
		/// </summary>
		/// <returns>Enumerator with a time to delay before running the next step</returns>
		private static IEnumerator RunLanguageScreenshot()
		{
			//var unsortedLanguages = FuelEditorUtilities.GetSupportedLanguages();

			// Created a sorted list of languages so that order of screenshots can be more easily followed.
			// Initially omit the base language so that it can placed as the first.
			var sortedLanguages =
				new List<Locale>(
					from language in LocalizationSettings.AvailableLocales.Locales
					where language != LocalizationSettings.ProjectLocale
					orderby language.Identifier.Code
					select language);

			// Add the base language as the first language to screenshot.
			sortedLanguages.Insert(0, LocalizationSettings.ProjectLocale);

			// Translate into any language other than the first language to screenshot.
			// This seems to help ensure translations are shown correctly for
			// the first language that gets screenshots.
			if (sortedLanguages.Count > 1)
			{
				yield return Translate(sortedLanguages.Last());
			}

			// Run screenshots for each language.
			foreach (var language in sortedLanguages)
			{
				status.LanguageName = language.Identifier.Code;

				yield return Translate(language);

				// Step through all translators in the scene.
				yield return new EditorCoroutineRunner.WaitForFrames(1);
				yield return EditorCoroutineRunner.StartEditorCoroutine(RunTranslatorScreenshot());
			}

			status.LanguageName = string.Empty;
		}

		/// <summary>
		/// Perform screen shot operations for each translator found in the scene.
		/// </summary>
		/// <returns>Enumerator with a time to delay before running the next step</returns>
		private static IEnumerator RunTranslatorScreenshot()
		{
			// Note: At this point we should be filtering by selection if asked to.

			var sceneTranslatorsToScreenshot = ScreenshotableTranslators;
			var idsProcessed = new HashSet<string>();

			foreach (var screenshotTranslator in sceneTranslatorsToScreenshot)
			{
				status.TranslatorObject = screenshotTranslator.GameObject.name;

				if (screenshotTranslator.IgnoreScreenshot)
				{
					continue;
				}

				if (!idsProcessed.Add(screenshotTranslator.TranslationId))
					continue;

				// Show objects that are part of this screenshot.
				try
				{
					ScreenshotUtilities.EnableScreenShotObject(screenshotTranslator.GameObject);
				}
				catch (Exception ex)
				{
					LogScreenshotException(
						"Screenshot exception: Enable screen shot object failed.",
						screenshotTranslator,
						ex);
				}

				yield return new EditorCoroutineRunner.WaitForFrames(3);

				// Position the screenshot.
				try
				{
					ScreenshotUtilities.PositionCamera(
						screenshotTranslator.GameObject,
						Quaternion.Euler(screenshotTranslator.ScreenshotCameraRotation),
						screenshotTranslator.ScreenshotDistance);
				}
				catch (Exception ex)
				{
					LogScreenshotException(
						"Screenshot exception: Position camera failed.",
						screenshotTranslator,
						ex);
				}

				yield return new EditorCoroutineRunner.WaitForFrames(3);

				// Set up the screenshot enumerator.
				var shotEnumerator = screenshotTranslator.GetScreenShots(LocalizationSettings.SelectedLocale);

				// Take screenshot for each translator.
				do
				{
					try
					{
						// Take the next step for generating screenshots for this object.
						if (shotEnumerator.MoveNext() == false)
						{
							// Get out if we are done taking the screenshots for this translator..
							break;
						}
					}
					catch (Exception ex)
					{
						LogScreenshotException(
							"Screenshot exception: Running screenshot.",
							screenshotTranslator,
							ex);

						break;
					}

					switch (shotEnumerator.Current)
					{
						case TranslationInformation data:
							status.IncrementProcessed();

							// Wait for the file to be saved.
							yield return new EditorCoroutineRunner.WaitForFrames(3);

							gameScreenshotInfo.Add(data);
							break;

						case float value:
							yield return value;
							break;

						default:
							break;
					}
				}
				// The code will break out when done enumerating through the screenshot.
				while (true);

				ScreenshotUtilities.ScreenshotCamera.transform.parent = null;
				ScreenshotUtilities.DisableScreenShotObject(screenshotTranslator.GameObject);
				DisableAllSceneObjectsForScreenshots();
			}

			status.TranslatorObject = string.Empty;
		}

		/// <summary>
		/// Clean up some variables when the program is done running.
		/// </summary>
		private static void OnExitCleanUp()
		{
			Debug.Log("Ending Screenshots");

			//Reset the project monitor settings to the cached monitor settings.
			PlayerSettings.defaultScreenHeight = originalMainMonitorScreenHeight;
			PlayerSettings.defaultScreenWidth = originalMainMonitorScreenWidth;

			LoadScenes(previouslyLoadedScenes, previousActiveScene);

			status = null;
			if (closeStatusWindow)
			{
				DatabaseSyncWindow.HideWindow();
			}

			executingRoutine = null;
		}

		/// <summary>
		/// Set up coroutine fro running screenshots.
		/// </summary>
		/// <param name="statusDialogHeading">Header of status dialog.</param>
		/// <param name="onComplete"><see cref="ScreenshotsComplete"/> to call when screenshots are complete.</param>
		/// <returns><see cref="IEnumerator"/> for running screenshots.</returns>
		private static IEnumerator SetupScreenshotCoroutine(string statusDialogHeading, ScreenshotsComplete onComplete = null)
		{
			// Create screenshot window.
			var syncStatus = new SyncData();

			DatabaseSyncWindow.HeadingText = statusDialogHeading;
			DatabaseSyncWindow.ShowWindow(syncStatus);

			closeStatusWindow = true;

			return SetupScreenshotCoroutine(syncStatus, onComplete);
		}

		/// <summary>
		/// Set up coroutine fro running screenshots.
		/// </summary>
		/// <param name="syncStatus"><see cref="SyncData"/> showing progress.</param>
		/// <param name="onComplete"><see cref="ScreenshotsComplete"/> to call when screenshots are complete.</param>
		/// <returns><see cref="IEnumerator"/> for running screenshots.</returns>
		private static IEnumerator SetupScreenshotCoroutine(SyncData syncStatus, ScreenshotsComplete onComplete = null)
		{
			status = new ProgressStatus { SyncStatus = syncStatus };

			completeCallback = onComplete;

			return Init(syncStatus);
		}

		/// <summary>
		/// Sets the current scene up for screenshots
		/// </summary>
		/// <returns>Flag indicating if the scene was correctly setup to take screenshots.</returns>
		private static bool SetUpSceneForScreenshots()
		{
			var success = true;

			// Hide all objects in the scene.
			DisableAllSceneObjectsForScreenshots();

			var screenShotCameraObject = new GameObject("Screenshot Camera", typeof(Camera));

			var camera = screenShotCameraObject.GetComponent<Camera>();
			camera.orthographic = true;
			camera.orthographicSize = ScreenshotUtilities.ScreenshotMonitorHeight / 2f;
			camera.aspect = ScreenshotUtilities.ScreenshotMonitorAspect;
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.farClipPlane = 1000f;
			camera.nearClipPlane = 0f;

			ScreenshotUtilities.ScreenshotCamera = camera;

			success &= FindSceneTranslatorObjects();

			return success;
		}

		private static IEnumerable<IScreenshotableTranslator> GetScreenshotableTranslators(GameObjectLocalizer localizer)
		{
			// Work out what kind of object it is and return the right screenshotable, or null if not supported

			foreach (var o in localizer.TrackedObjects)
			{
				foreach (var prop in o.TrackedProperties)
				{
					switch (prop)
					{
						case LocalizedAssetProperty localizedAsset:
						{
							var lo = localizedAsset.LocalizedObject;
							var tableEntry = LocalizationSettings.AssetDatabase.GetTableEntry(lo.TableReference, lo.TableEntryReference);
							yield return new ScreenshotableTranslator(localizer.gameObject, tableEntry.Table.SharedData, tableEntry.Entry.SharedEntry, "Sprite", locale => GetTranslation(localizedAsset.LocalizedObject, locale));

							string GetTranslation(LocalizedReference localisedAsset, Locale locale)
							{
								var dte = LocalizationSettings.AssetDatabase.GetTableEntry(localisedAsset.TableReference, localisedAsset.TableEntryReference, locale);
								var transcription = dte.Entry.MetadataEntries.OfType<TranscriptionMetadata>().SingleOrDefault();
								return transcription?.Text ?? "Transcription metadata not found";
							}

							break;
						}

						case LocalizedStringProperty localizedString:
						{
							var ls = localizedString.LocalizedString;
							var tableEntry = LocalizationSettings.StringDatabase.GetTableEntry(ls.TableReference, ls.TableEntryReference);

							yield return new ScreenshotableTranslator(localizer.gameObject, tableEntry.Table.SharedData, tableEntry.Entry.SharedEntry, "Text", locale => GetTranslation(localizedString.LocalizedString, locale));

							string GetTranslation(LocalizedReference localisedString, Locale locale)
							{
								var dte = LocalizationSettings.StringDatabase.GetTableEntry(localisedString.TableReference, localisedString.TableEntryReference, locale);
								return dte.Entry?.Value ?? "Translation not found";
							}

							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Find all Fuel translation objects in the scene.
		/// </summary>
		/// <returns>True if any translators were found.</returns>
		private static bool FindSceneTranslatorObjects()
		{
			// Find all the screenshotable translators in the scene.

			ScreenshotableTranslators = SceneHelper.GetComponentsInAllLoadedScenes<GameObjectLocalizer>(true)
				.SelectMany(GetScreenshotableTranslators)
				.ToList();

			return screenshotableTranslators != null && screenshotableTranslators.Any();
		}

		/// <summary>
		/// Disable all game objects in the scene, and make sure some required ones are enabled.
		/// </summary>
		private static void DisableAllSceneObjectsForScreenshots()
		{
			// We choose here to not deactivate any gameObjects that are not
			// visible in the scene hierarchy.  This keeps us from killing very
			// necessary objects such as the SceneCamera, scene lights, etc...
			// If some of these objects are deactivated, then we have to restart
			// unity or reload the layout to get back to a valid scene view.
			foreach (var gameObject in FindAllGameObjects())
			{
				if (gameObject.hideFlags == HideFlags.None)
				{
					gameObject.SetActive(false);
				}
			}

			// Ensure the Screenshot Camera becomes enabled.
			if (ScreenshotUtilities.ScreenshotCamera != null)
			{
				var parent = ScreenshotUtilities.ScreenshotCamera.transform.parent;
				ScreenshotUtilities.ScreenshotCamera.gameObject.SetActive(true);
				while (parent != null)
				{
					parent.gameObject.SetActive(true);
					parent = parent.gameObject.transform.parent;
				}
			}
		}

		/// <summary>
		/// Enable all game objects in the scene, and make sure some requires ones are enabled.
		/// </summary>
		private static void EnableAllSceneObjectsForScreenshots()
		{
			// We choose here to not touch gameObjects that are not
			// visible in the scene hierarchy.  This keeps us from killing very
			// necessary objects such as the SceneCamera, scene lights, etc...
			// If some of these objects are deactivated, then we have to restart
			// unity or reload the layout to get back to a valid scene view.
			foreach (var gameObject in FindAllGameObjects())
			{
				if (gameObject.hideFlags == HideFlags.None)
				{
					gameObject.SetActive(true);
				}
			}
		}

		/// <summary>
		/// Find all game objects in the scene, including inactive objects.
		/// </summary>
		/// <returns>Collection of game objects from scene.</returns>
		private static IEnumerable<GameObject> FindAllGameObjects()
		{
			IEnumerable<GameObject> gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

#if UNITY_EDITOR

			gameObjects = gameObjects?.Where(gameObject => !AssetDatabase.Contains(gameObject));

#endif

			return gameObjects;
		}

		/// <summary>
		/// Log a exception associated with a screenshotable translator.
		/// </summary>
		/// <param name="summary">Summary of error.</param>
		/// <param name="screenshotTranslator">Item to log error about.</param>
		/// <param name="ex"><see cref="Exception"/> caught.</param>
		private static void LogScreenshotException(
			string summary,
			IScreenshotableTranslator screenshotTranslator,
			Exception ex)
		{
			var objectName =
				screenshotTranslator != null && screenshotTranslator.GameObject != null ? screenshotTranslator.GameObject.name : "Null";

			var objectType =
				screenshotTranslator != null && screenshotTranslator.GameObject != null ? screenshotTranslator.GetType().Name : "";

			Debug.LogError(
				$"{summary}" +
				$"  ObjectName: {objectName}" +
				$" Translator: {objectType}" +
				$" Reason: {ex.Message}\n" +
				$"Stack:\n{ex.StackTrace}");
		}

		/// <summary>
		/// Load a list of scenes.
		/// </summary>
		/// <param name="scenePaths">Paths of scenes to load additively.</param>
		/// <param name="activeScene">Path of scene to load as active scene.</param>
		private static void LoadScenes(List<string> scenePaths, string activeScene)
		{
			EditorSceneManager.OpenScene(activeScene, OpenSceneMode.Single);
			foreach (var sceneToAdd in scenePaths.Where(path => !path.Equals(activeScene)))
			{
				EditorSceneManager.OpenScene(sceneToAdd, OpenSceneMode.Additive);
			}
		}

		/// <summary>
		/// Load all the scenes that are in the build additively to the scenes that are open.
		/// </summary>
		/// <param name="initiallyLoadedScenePaths">List of paths for scenes that were initially loaded before this function was called.</param>
		/// <param name="initiallyActiveScenePath">Path of scene that was loaded before this function was called.</param>
		/// <param name="enabledOnly">Flag for whether or not to load scenes that are not enabled in build settings.</param>
		private static void LoadAllScenesInBuild(
			out List<string> initiallyLoadedScenePaths,
			out string initiallyActiveScenePath,
			bool enabledOnly = false)
		{
			initiallyLoadedScenePaths = new List<string>();
			initiallyActiveScenePath = SceneManager.GetActiveScene().path;

			for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
			{
				initiallyLoadedScenePaths.Add(SceneManager.GetSceneAt(sceneIndex).path);
			}

			// Gather scenes that are in the build settings but no loaded.
			var scenesToLoad = from buildSettingScene in GetValidBuildScenes()
				where !enabledOnly || buildSettingScene.enabled
				let scene = SceneManager.GetSceneByPath(buildSettingScene.path)
				where !scene.isLoaded
				select buildSettingScene.path;

			// Load the scenes.
			foreach (var scenePath in scenesToLoad)
			{
				try
				{
					EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
				}
				catch (ArgumentException)
				{
					Debug.LogWarningFormat("Failed to load \"{0}\"", scenePath);
				}
			}
		}

		private static IEnumerable<EditorBuildSettingsScene> GetValidBuildScenes()
		{
			var basePath = Path.Combine(Application.dataPath, @"..\");
			return EditorBuildSettings.scenes
				.Where(scene => File.Exists(Path.Combine(basePath, scene.path)));
		}
	}
}