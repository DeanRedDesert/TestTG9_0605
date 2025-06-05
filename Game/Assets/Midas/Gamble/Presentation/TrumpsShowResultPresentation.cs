using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.Coroutine;
using Midas.Gamble.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.SceneManagement;
using Midas.Presentation.StageHandling;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Gamble.Presentation
{
	public sealed class TrumpsShowResultPresentation : MonoBehaviour, ITrumpsPresenter, ISceneInitialiser
	{
		private TrumpsStatus trumpsStatus;
		private Coroutine revealCoroutine;

		[SerializeField]
		private TrumpsCard card;

		[SerializeField]
		private TrumpsMessage message;

		[SerializeField]
		private TrumpsCardHistory cardHistory;

		private void Awake()
		{
			trumpsStatus = StatusDatabase.QueryStatusBlock<TrumpsStatus>();
		}

		private void OnEnable()
		{
			foreach (var reg in GameBase.GameInstance.GetInterfaces<ITrumpsPresenterSubscriber>())
				reg.RegisterTrumpsPresenter(this);

			Initialise();
		}

		private void OnDisable()
		{
			Abort();

			foreach (var reg in GameBase.GameInstance.GetInterfaces<ITrumpsPresenterSubscriber>())
				reg.UnregisterTrumpsPresenter(this);
		}

		private void Initialise()
		{
			switch (StatusDatabase.GameStatus.CurrentGameState)
			{
				case GameState.History:
					card.Initialise(trumpsStatus.CurrentResult.Suit);
					ShowResultMessage();
					cardHistory.UpdateHistory(trumpsStatus.History, trumpsStatus.Results.Take(trumpsStatus.CurrentResultIndex + 1).ToArray());
					break;

				case GameState.StartingGamble:
				case GameState.ShowGambleResult:
					card.Initialise(null);
					message.Clear();
					cardHistory.UpdateHistory(trumpsStatus.History ?? Array.Empty<TrumpsSuit>(), trumpsStatus.Results?.Take(trumpsStatus.CurrentResultIndex + (StatusDatabase.GameStatus.CurrentGameState == GameState.ShowGambleResult ? 0 : 1)).ToArray() ?? Array.Empty<TrumpsCycleData>());
					break;
			}
		}

		private void ShowResultMessage()
		{
			switch (trumpsStatus.CurrentResult.Result)
			{
				case TrumpsResult.Loss:
					message.ShowGameOverMessage();
					break;
				case TrumpsResult.Win:
					message.ShowGameWinMessage();
					break;
			}
		}

		private void ShowCompleteMessage()
		{
			switch (trumpsStatus.CurrentResult.GambleCompleteReason)
			{
				case GambleCompleteReason.CycleLimit:
					message.ShowCycleLimitReached();
					break;
				case GambleCompleteReason.MoneyLimit:
					message.ShowWinLimitReached();
					break;
			}
		}

		private IEnumerator<CoroutineInstruction> TrumpsRevealCoroutine()
		{
			yield return new CoroutineRun(card.Reveal(trumpsStatus.CurrentResult.Suit));
			ShowResultMessage();
			cardHistory.UpdateHistory(trumpsStatus.History, trumpsStatus.Results);

			yield return new CoroutineDelay(1f);

			if (trumpsStatus.CurrentResult.GambleCompleteReason != GambleCompleteReason.None)
			{
				ShowCompleteMessage();

				yield return new CoroutineDelay(2f);
			}
			else
			{
				yield return new CoroutineRun(card.Hide());
				message.Clear();
			}

			revealCoroutine = null;
		}

		#region ITrumpsPresenter implementation

		public bool IsRevealing => revealCoroutine != null;

		public void Reveal()
		{
			revealCoroutine = FrameUpdateService.Update.StartCoroutine(TrumpsRevealCoroutine());
		}

		public void Abort()
		{
			revealCoroutine?.Stop();
			revealCoroutine = null;
			Initialise();
		}

		#endregion

		#region ISceneInitialiser implementation

		public bool RemoveAfterFirstInit => false;
		public void SceneInit(Stage stage) => Initialise();

		#endregion
	}
}