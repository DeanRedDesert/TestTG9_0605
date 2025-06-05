using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;

namespace Midas.Presentation.Gamble
{
	public sealed class OfferGambleNode : IPresentationNode
	{
		private enum State
		{
			Idle,
			ReturnToBaseGame,
			Offering
		}

		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private GambleStatus gambleStatus;
		private StageController stageController;
		private Coroutine offerGambleCoroutine;
		private bool showRequested;
		private bool isShowing;

		public string NodeId => "OfferGamble";

		public bool ReadyToStart { get; private set; }

		public bool IsMainActionComplete => !(showRequested || isShowing);

		public void Init()
		{
			gambleStatus = StatusDatabase.QueryStatusBlock<GambleStatus>();
			stageController = GameBase.GameInstance.GetPresentationController<StageController>();
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState), OnGameStateChanged);
			offerGambleCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(RunOfferGamble, State.Idle, "OfferGamble");
		}

		public void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
			offerGambleCoroutine?.Stop();
			offerGambleCoroutine = null;
		}

		public void Destroy() { }

		public void Show()
		{
			showRequested = true;
			ReadyToStart = false;
		}

		private void OnGameStateChanged(StatusBlock sender, string propertyname)
		{
			if (StatusDatabase.GameStatus.CurrentGameState == GameState.OfferGamble)
			{
				StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.None;
				ReadyToStart = true;
			}
		}

		private IEnumerator<CoroutineInstruction> RunOfferGamble(IStateInfo<State> stateInfo)
		{
			while (true)
			{
				while (!showRequested)
					yield return null;

				if (StatusDatabase.GameFlowStatus.RunningNodes.Count != 1)
				{
					showRequested = false;
					ReadyToStart = true;
					continue;
				}

				showRequested = false;
				isShowing = true;

				yield return stateInfo.SetNextState(State.ReturnToBaseGame);
				stageController.SwitchTo(StatusDatabase.GameStatus.NextLogicStage);
				while (stageController.IsTransitioning())
					yield return null;

				yield return stateInfo.SetNextState(State.Offering);
				StatusDatabase.GameStatus.GameIsActive = false;
				yield return new CoroutineRun(DoOffering(), "Offering");

				isShowing = false;
				yield return stateInfo.SetNextState(State.Idle);
			}

			// ReSharper disable once IteratorNeverReturns - by design
		}

		private IEnumerator<CoroutineInstruction> DoOffering()
		{
			if (GetInitialRequest() == OfferGambleRequest.None)
			{
				AddMessages();

				while (StatusDatabase.GameStatus.OfferGambleRequest == OfferGambleRequest.None)
				{
					yield return null;

					if (StatusDatabase.GameStatus.GameLogicPaused)
					{
						RemoveMessages();

						while (StatusDatabase.GameStatus.GameLogicPaused)
							yield return null;

						AddMessages();
					}
				}

				RemoveMessages();
			}

			void AddMessages() => StatusDatabase.DashboardStatus.AddGameMessages(GameMessage.PressTakeWin | GameMessage.GambleAvailable);
			void RemoveMessages() => StatusDatabase.DashboardStatus.RemoveGameMessages(GameMessage.PressTakeWin | GameMessage.GambleAvailable);
		}

		private OfferGambleRequest GetInitialRequest()
		{
			if (StatusDatabase.GameStatus.OfferGambleRequest != OfferGambleRequest.None)
				return StatusDatabase.GameStatus.OfferGambleRequest;

			return StatusDatabase.GameStatus.OfferGambleRequest =
				StatusDatabase.ConfigurationStatus.AncillaryConfig.Enabled == false ||
				StatusDatabase.BankStatus.TotalAward == Money.Zero ||
				StatusDatabase.GaffStatus?.IsSelfPlayActive == true ||
				StatusDatabase.GameStatus.HasWinCapBeenReached ||
				!gambleStatus.IsGambleOfferable
					? OfferGambleRequest.TakeWin
					: OfferGambleRequest.None;
		}
	}
}