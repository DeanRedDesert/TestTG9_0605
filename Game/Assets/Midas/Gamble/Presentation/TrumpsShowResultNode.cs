using System.Collections.Generic;
using System.Linq;
using Midas.Core.Coroutine;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.WinPresentation;

namespace Midas.Gamble.Presentation
{
	public sealed class TrumpsShowResultNode : IPresentationNode, ITrumpsPresenterSubscriber
	{
		private Coroutine trumpsFeatureCoroutine;
		private bool showRequested;
		private bool isShowing;
		private readonly List<ITrumpsPresenter> trumpsPresenters = new List<ITrumpsPresenter>();
		private WinCountController winCountingController;

		#region IPresentaitonNode Implementation

		public string NodeId => "TrumpsShowResult";

		public void Init()
		{
			trumpsFeatureCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(DoTrumpsFeature(), "TrumpsFeature");
			winCountingController = GameBase.GameInstance.GetPresentationController<WinCountController>();
		}

		public void DeInit()
		{
			trumpsFeatureCoroutine?.Stop();
			trumpsFeatureCoroutine = null;
		}

		public void Destroy()
		{
		}

		public bool ReadyToStart => StatusDatabase.GameStatus.CurrentGameState == GameState.ShowGambleResult && !isShowing;

		public bool IsMainActionComplete { get; private set; }

		public void Show()
		{
			showRequested = true;
			IsMainActionComplete = false;
		}

		#endregion

		#region Private Methods

		private IEnumerator<CoroutineInstruction> DoTrumpsFeature()
		{
			while (true)
			{
				while (!showRequested)
					yield return null;

				isShowing = true;
				showRequested = false;

				Reveal();

				winCountingController.Reset(true);

				while (trumpsPresenters.Any(p => p.IsRevealing))
				{
					if (StatusDatabase.GameStatus.GameLogicPaused)
					{
						winCountingController.Reset(false);

						foreach (var presenter in trumpsPresenters)
							presenter.Abort();

						while (StatusDatabase.GameStatus.GameLogicPaused)
							yield return null;

						Reveal();
					}

					yield return null;
				}

				IsMainActionComplete = true;

				while (StatusDatabase.GameStatus.CurrentGameState == GameState.ShowGambleResult)
					yield return null;

				isShowing = false;

				void Reveal()
				{
					foreach (var presenter in trumpsPresenters)
						presenter.Reveal();
				}
			}

			// ReSharper disable once IteratorNeverReturns - By design
		}

		#endregion

		public void RegisterTrumpsPresenter(ITrumpsPresenter trumpsPresenter) => trumpsPresenters.Add(trumpsPresenter);

		public void UnregisterTrumpsPresenter(ITrumpsPresenter trumpsPresenter) => trumpsPresenters.Remove(trumpsPresenter);
	}
}