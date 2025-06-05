using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.SceneManagement;
using UnityEngine;

namespace Midas.Presentation.StageHandling
{
	/// <summary>
	/// Used to activate and initialise a stage (when interrupting or recovering).
	/// </summary>
	public sealed class StageActivator : MonoBehaviour
	{
		private static readonly HashSet<StageActivator> allStageActivators = new HashSet<StageActivator>();
		private static readonly HashSet<SceneRoot> allSceneRoots = new HashSet<SceneRoot>();

		[SerializeField]
		private Stage stage;

		[SerializeField]
		private List<SceneRoot> sceneRoots;

		/// <summary>
		/// Deactivate all scene elements.
		/// </summary>
		public static void DeactivateAll()
		{
			foreach (var sceneRoot in allSceneRoots)
				sceneRoot.Deactivate();
		}

		/// <summary>
		/// Activate and initialise all scene elements for the specified stage.
		/// </summary>
		public static void Activate(Stage stage)
		{
			var activeSceneRoots = new HashSet<SceneRoot>();
			foreach (var activator in allStageActivators)
			{
				if (activator.stage.Equals(stage))
					activeSceneRoots.UnionWith(activator.sceneRoots);
			}

			if (activeSceneRoots.Count == 0)
				Log.Instance.Fatal($"Stage {stage} has no scene roots to activate.");

			foreach (var sceneRoot in activeSceneRoots)
			{
				sceneRoot.Activate(stage);
			}

			foreach (var sceneRoot in allSceneRoots.Except(activeSceneRoots))
				sceneRoot.Deactivate();
		}

		/// <summary>
		/// Registers a scene root.
		/// </summary>
		public static void RegisterSceneRoot(SceneRoot sceneRoot)
		{
			allSceneRoots.Add(sceneRoot);
		}

		/// <summary>
		/// Deregisters a scene root.
		/// </summary>
		public static void DeregisterSceneRoot(SceneRoot sceneRoot)
		{
			allSceneRoots.Remove(sceneRoot);
		}

		private void Awake()
		{
			if (stage == null)
				Log.Instance.Fatal($"StageActivator {this.GetPath()} has no stage configured.");

			if (sceneRoots == null || sceneRoots.Count == 0)
				Log.Instance.Fatal($"StageActivator {this.GetPath()} has no scene roots configured.");

			allStageActivators.Add(this);
		}

		private void OnDestroy()
		{
			allStageActivators.Remove(this);
		}

		#region Editor

#if UNITY_EDITOR

		public void ConfigureForMakeGame(Stage stage, IReadOnlyList<SceneRoot> sceneRoots)
		{
			this.stage = stage;
			this.sceneRoots = sceneRoots.ToList();
		}

#endif

		#endregion
	}
}