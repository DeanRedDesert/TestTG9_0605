using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.StageHandling;
using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.SceneManagement
{
	public sealed class GameStageSceneInit : MonoBehaviour, ISceneInitialiser
	{
		[SerializeField]
		[Tooltip("Set to true if init is not wanted on denom change")]
		private bool oncePerSceneLoad;

		[SerializeField]
		private Stage[] stages;

		[SerializeField]
		private UnityEvent onMatch = new UnityEvent();

		[SerializeField]
		private UnityEvent onNoMatch = new UnityEvent();

		public bool RemoveAfterFirstInit => oncePerSceneLoad;

		public void SceneInit(Stage currentStage)
		{
			if (stages?.Contains(currentStage) is true)
				onMatch.Invoke();
			else
				onNoMatch.Invoke();
		}

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(IReadOnlyList<Stage> s, (UnityAction<bool> Action, bool Value) onMatchAction, (UnityAction<bool> Action, bool Value) onNoMatchAction)
		{
			stages = s.ToArray();

			UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(onMatch, onMatchAction.Action, onMatchAction.Value);
			UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(onNoMatch, onNoMatchAction.Action, onNoMatchAction.Value);
		}
#endif

		#endregion
	}
}