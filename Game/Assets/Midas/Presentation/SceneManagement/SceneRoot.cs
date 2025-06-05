using System.Collections;
using System.Linq;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.StageHandling;
using UnityEngine;

namespace Midas.Presentation.SceneManagement
{
	public sealed class SceneRoot : MonoBehaviour
	{
		private ISceneInitialiser[] initialisers;

		[SerializeField]
		[Tooltip("Content that you wish to be turned on or instantiated on Awake.")]
		private GameObject[] content;

		private void Awake()
		{
			// Instantiate any content that is prefab, set all content active.
			gameObject.InstantiateOrActivateChildContent(content);

			StageActivator.RegisterSceneRoot(this);
			Deactivate();
		}

		private void OnDestroy()
		{
			StageActivator.DeregisterSceneRoot(this);
		}

		public void Activate(Stage currentStage)
		{
			if (!gameObject.activeSelf)
				gameObject.SetActive(true);

			initialisers ??= GetComponentsInChildren<ISceneInitialiser>(true);

			// Reinitialise the scene root.

			foreach (var initialiser in initialisers)
			{
				if (initialiser is MonoBehaviour c && !c)
					continue;

				initialiser.SceneInit(currentStage);
			}

			initialisers = initialisers.Where(i => !i.RemoveAfterFirstInit).ToArray();
		}

		public void Deactivate()
		{
			if (gameObject.activeSelf)
				gameObject.SetActive(false);
		}

		public void DeactivateDelayed(float seconds)
		{
			StartCoroutine(DelayDeactivate());

			IEnumerator DelayDeactivate()
			{
				yield return new WaitForSeconds(seconds);
				Deactivate();
			}
		}
	}
}