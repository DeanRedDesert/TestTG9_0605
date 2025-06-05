using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Midas.Presentation.SceneManagement
{
	public sealed class SceneLoader : MonoBehaviour, ISceneLoader
	{
		private bool registeredForSetupPresentationDone;
		private List<string> initialScenes;
		private List<string> initialScenesToLoad;
		private readonly HashSet<string> scenesMarkedForUnload = new HashSet<string>();
		private readonly HashSet<string> initialScenesMarkedForLoad = new HashSet<string>();

		[SerializeField]
		private GameObject[] loadingScreens = Array.Empty<GameObject>();

		public void LoadInitialScenesAsync()
		{
			Log.Instance.Info("Loading initial scenes");

			RegisterForSetupPresentationDone();
			LoadingScreenSetActive(!StatusDatabase.GameStatus.IsSetupPresentationDone);

			if (initialScenes == null)
			{
				initialScenes = new List<string>();

				// Add already loaded scenes.

				for (var i = 0; i < SceneManager.sceneCount; i++)
					initialScenes.Add(SceneManager.GetSceneAt(i).name);
			}

			if (initialScenesToLoad == null)
			{
				initialScenesToLoad = new List<string>();

				// Load any scenes that are not already loaded.

				for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
				{
					var sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

					if (!initialScenes.Contains(sceneName))
					{
						initialScenes.Add(sceneName);
						initialScenesToLoad.Add(sceneName);
					}
				}
			}

			SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;

			if (initialScenesToLoad != null)
			{
				foreach (var sceneName in initialScenesToLoad)
				{
					var scene = SceneManager.GetSceneByName(sceneName);
					if (!scene.IsValid())
					{
						Log.Instance.Info($"Loading {sceneName}");
						initialScenesMarkedForLoad.Add(sceneName);
						SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
					}
				}
			}

			CheckForAllInitialScenesLoaded();
		}

		public void UnloadScenesAsync()
		{
			Log.Instance.Info("Unloading scenes");

			UnregisterForSetupPresentationDone();
			LoadingScreenSetActive(!StatusDatabase.GameStatus.IsSetupPresentationDone);

			if (scenesMarkedForUnload.Count == 0)
			{
				SceneManager.sceneUnloaded += SceneManagerOnSceneUnloaded;
			}

			if (initialScenesToLoad != null)
			{
				foreach (var sceneName in initialScenesToLoad)
				{
					var scene = SceneManager.GetSceneByName(sceneName);
					if (scene.IsValid())
					{
						Log.Instance.Info($"Unloading {scene.name}");
						scenesMarkedForUnload.Add(scene.name);
						SceneManager.UnloadSceneAsync(scene);
					}
				}
			}

			CheckForAllScenesUnloaded();
		}

		public void UnloadScenes()
		{
			UnregisterForSetupPresentationDone();
			LoadingScreenSetActive(!StatusDatabase.GameStatus.IsSetupPresentationDone);

			if (initialScenesToLoad != null)
			{
				foreach (var sceneName in initialScenesToLoad)
				{
					var scene = SceneManager.GetSceneByName(sceneName);
					if (scene.IsValid())
					{
						Log.Instance.Info($"Unloading {scene.name}");
#pragma warning disable 618
						SceneManager.UnloadScene(scene);
#pragma warning restore 618
					}
				}
			}

			AllScenesUnloaded?.Invoke();
		}

		public event Action AllInitialScenesLoaded;
		public event Action AllScenesUnloaded;

		private void Awake()
		{
//			StatusDatabase.GameStatus.IsGameSetupPresentationDone = false;
		}

		private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			initialScenesMarkedForLoad.Remove(scene.name);
			CheckForAllInitialScenesLoaded();
		}

		private void SceneManagerOnSceneUnloaded(Scene scene)
		{
			scenesMarkedForUnload.Remove(scene.name);
			CheckForAllScenesUnloaded();
		}

		private void CheckForAllInitialScenesLoaded()
		{
			if (initialScenesMarkedForLoad.Count == 0)
			{
				SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
				AllInitialScenesLoaded?.Invoke();
				Log.Instance.Info("All initial scenes loaded");
			}
		}

		private void CheckForAllScenesUnloaded()
		{
			if (scenesMarkedForUnload.Count == 0)
			{
				SceneManager.sceneUnloaded -= SceneManagerOnSceneUnloaded;
				AllScenesUnloaded?.Invoke();
				Log.Instance.Info("All scenes unloaded");
			}
		}

		private void RegisterForSetupPresentationDone()
		{
			if (!registeredForSetupPresentationDone)
			{
				StatusDatabase.GameStatus.AddPropertyChangedHandler<bool>(nameof(GameStatus.IsSetupPresentationDone), OnIsInSetupPresentationChanged);
				registeredForSetupPresentationDone = true;
			}
		}

		private void UnregisterForSetupPresentationDone()
		{
			if (registeredForSetupPresentationDone)
			{
				StatusDatabase.GameStatus.RemovePropertyChangedHandler<bool>(nameof(GameStatus.IsSetupPresentationDone), OnIsInSetupPresentationChanged);
				registeredForSetupPresentationDone = false;
			}
		}

		private void OnIsInSetupPresentationChanged(StatusBlock sender, string _, bool newValue, bool oldValue)
		{
			LoadingScreenSetActive(!newValue);
		}

		private void LoadingScreenSetActive(bool active)
		{
			if (loadingScreens != null)
			{
				foreach (var g in loadingScreens)
				{
					if (g != null)
						g.SetActive(active);
				}
			}
		}
	}
}
