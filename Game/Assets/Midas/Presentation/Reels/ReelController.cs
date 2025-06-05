using System.Collections.Generic;
using System.Linq;
using Midas.Core.Coroutine;
using Midas.Presentation.Data;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Game;
using Midas.Presentation.SceneManagement;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Reels
{
	public sealed class ReelController : MonoBehaviour, ISceneInitialiser, ISequencePlayable
	{
		private Coroutine reelControllerCoroutine;
		private bool requestSpin;
		private bool abortSpin;
		private bool forceInit;

		[SerializeField]
		private Stage stage;

		[SerializeField]
		private List<ReelData> reelData;

		[SerializeField]
		private ReelSpinController reelSpin;

		public bool IsSpinning => reelSpin.IsSpinning || requestSpin;

		void ISequencePlayable.StartPlay()
		{
			if (IsSpinning)
				return;

			requestSpin = true;
			abortSpin = false;
		}

		void ISequencePlayable.StopPlay(bool reset)
		{
			if (reset)
				AbortSpin();
		}

		bool ISequencePlayable.IsPlaying() => IsSpinning;

		public void AbortSpin()
		{
			requestSpin = false;
			if (reelSpin.IsSpinning)
				abortSpin = true;
		}

		public bool RemoveAfterFirstInit => false;
		public void SceneInit(Stage currentStage) => forceInit = true;

		private void OnEnable()
		{
			if (GameBase.GameInstance == null)
				return;

			requestSpin = false;

			if (reelData == null)
				Log.Instance.Error($"Cannot enable reels {this.GetPath()} because reelData is not defined.");
			else
				reelControllerCoroutine = FrameUpdateService.Update.StartCoroutine(ReelControllerCoroutine());
		}

		private void OnDisable()
		{
			reelControllerCoroutine?.Stop();
			reelControllerCoroutine = null;
		}

		private void OnDestroy()
		{
			foreach (var rd in reelData)
				rd.DeInit();
		}

		private IEnumerator<CoroutineInstruction> ReelControllerCoroutine()
		{
			// Do nothing until we have a game state.

			while (!StatusDatabase.GameStatus.CurrentGameState.HasValue || reelData == null)
				yield return null;

			Initialise();
			forceInit = false;

			while (true)
			{
				while (!requestSpin)
				{
					if (forceInit)
					{
						Initialise();
						forceInit = false;
					}
					else
						yield return null;
				}

				reelSpin.SpinReels(stage, reelData, StatusDatabase.ConfigurationStatus.GetGameTime());

				requestSpin = false;

				while (reelSpin.IsSpinning)
				{
					if (abortSpin)
					{
						reelSpin.AbortSpin();
						Initialise();
						abortSpin = false;
					}
					else
						yield return null;
				}
			}
		}

		private void Initialise()
		{
			foreach (var rd in reelData)
			{
				if (rd.ReelSpinStateEvent)
					rd.ReelSpinStateEvent.Interrupt(true);

				rd.ReelContainer.Initialise(rd.DataProvider.GetInitReelStrips(stage, rd.ReelContainer), rd.SymbolList);
			}
		}

		#region Editor

#if UNITY_EDITOR

		public void ConfigureForMakeGame(Stage stage, IReadOnlyList<ReelData> reelData, ReelSpinController reelSpin)
		{
			this.stage = stage;
			this.reelData = reelData.ToList();
			this.reelSpin = reelSpin;
		}

#endif

		#endregion
	}
}