using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.WinPresentation;

namespace Midas.Presentation.AutoPlay
{
	public enum AutoPlayState
	{
		Idle,
		WaitPlayerConfirm,
		Starting,
		Active,
		Stopping
	}

	public sealed class AutoPlayController : IPresentationController
	{
		private Coroutine autoPlayCoroutine;
		private Coroutine autoPlayAutoStop;

		private readonly AutoUnregisterHelper unregisterHelper = new AutoUnregisterHelper();
		private AutoPlayStatus autoPlayStatus = StatusDatabase.AutoPlayStatus;

		private WinCountController winCountController;
		private ButtonEventDataQueueStatus buttonEventDataQueueStatus;

		public void Init()
		{
			autoPlayCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(AutoPlayStateMachine(), "AutoPlay");
			autoPlayAutoStop = StateMachineService.FrameUpdateRoot.StartCoroutine(AutoPlayAutoStop(), "AutoPlayAutoStop");

			winCountController = GameBase.GameInstance.GetPresentationController<WinCountController>();
			buttonEventDataQueueStatus = StatusDatabase.ButtonEventDataQueueStatus;

			unregisterHelper.RegisterMessageHandler<AutoPlayMessage>(Communication.PresentationDispatcher, OnAutoPlayMessageReceived);
			unregisterHelper.RegisterButtonEventListener(OnAnyButtonPressed);
		}

		private void OnAnyButtonPressed(ButtonEventData eventData)
		{
			if (autoPlayStatus.State == AutoPlayState.WaitPlayerConfirm &&
				!eventData.ButtonFunction.Equals(AutoPlayButtonFunctions.StartAutoPlay) &&
				!eventData.ButtonFunction.Equals(AutoPlayButtonFunctions.AutoPlayConfirmYes) &&
				!eventData.ButtonFunction.Equals(AutoPlayButtonFunctions.AutoPlayConfirmNo) &&
				!eventData.ButtonFunction.Equals(DashboardButtonFunctions.Service))
			{
				autoPlayStatus.State = AutoPlayState.Idle;
			}
		}

		public void DeInit()
		{
			autoPlayCoroutine?.Stop();
			autoPlayCoroutine = null;
			autoPlayAutoStop?.Stop();
			autoPlayAutoStop = null;

			unregisterHelper.UnRegisterAll();
			winCountController = null;
		}

		public void Destroy()
		{
		}

		/// <summary>
		/// Check if the player can request credit playoff to start.
		/// </summary>
		public bool CanRequestStart()
		{
			return autoPlayStatus.State == AutoPlayState.Idle &&
				!StatusDatabase.GameStatus.GameIsActive &&
				IsPlayInitiatingState(StatusDatabase.GameStatus.CurrentGameState) &&
				!PopupStatus.IsCashoutConfirmOpen &&
				!PopupStatus.IsInfoOpen &&
				CanActivate();

			bool IsPlayInitiatingState(GameState? state) => state == GameState.Idle || state == GameState.OfferGamble;
		}

		/// <summary>
		/// Request that autoplay starts.
		/// </summary>
		public void RequestStart()
		{
			if (CanRequestStart())
			{
				if (StatusDatabase.ConfigurationStatus.GameConfig.IsAutoPlayConfirmationRequired)
					autoPlayStatus.State = AutoPlayState.WaitPlayerConfirm;
				else
				{
					autoPlayStatus.State = AutoPlayState.Starting;
					Communication.ToLogicSender.Send(new AutoPlayMessage(AutoPlayMode.Start, AutoPlayRequestSource.Player));
				}
			}
		}

		public bool CanConfirmStart()
		{
			return autoPlayStatus.State == AutoPlayState.WaitPlayerConfirm &&
				!StatusDatabase.GameStatus.GameIsActive &&
				IsPlayInitiatingState(StatusDatabase.GameStatus.CurrentGameState) &&
				!StatusDatabase.PopupStatus.AreOpen(Popup.CashoutConfirm | Popup.Info) &&
				CanActivate();

			bool IsPlayInitiatingState(GameState? state) => state == GameState.Idle || state == GameState.OfferGamble;
		}

		public void ConfirmStart(bool start)
		{
			if (CanConfirmStart())
			{
				if (start)
				{
					autoPlayStatus.State = AutoPlayState.Starting;
					Communication.ToLogicSender.Send(new AutoPlayMessage(AutoPlayMode.Start, AutoPlayRequestSource.Player));
				}
				else
				{
					autoPlayStatus.State = AutoPlayState.Idle;
				}
			}
		}

		public bool CanRequestStop()
		{
			return autoPlayStatus.State != AutoPlayState.Idle;
		}

		public void RequestStop(bool playerRequest)
		{
			if (!CanRequestStop())
				return;

			autoPlayStatus.State = AutoPlayState.Stopping;
			Communication.ToLogicSender.Send(new AutoPlayMessage(AutoPlayMode.Stop, playerRequest ? AutoPlayRequestSource.Player : AutoPlayRequestSource.GameFlow));
		}

		private bool CanActivate()
		{
			return StatusDatabase.GameStatus.InActivePlay &&
				StatusDatabase.BankStatus.IsPlayerWagerAvailable &&
				StatusDatabase.BankStatus.BankMeter >= Money.FromCredit(GameStatus.TotalBet);
		}

		private bool CanStartAutoPlayGame()
		{
			return StatusDatabase.BankStatus.BankMeter >= Money.FromCredit(GameStatus.TotalBet) &&
				StatusDatabase.GameFlowStatus.AutoPlayCanStart &&
				!StatusDatabase.GameStatus.GameIsActive &&
				StatusDatabase.GameStatus.InActivePlay &&
				StatusDatabase.BankStatus.IsPlayerWagerAvailable &&
				!buttonEventDataQueueStatus.ButtonFunction.Equals(AutoPlayButtonFunctions.StopAutoPlay) &&
				autoPlayStatus.State == AutoPlayState.Active;
		}

		private IEnumerator<CoroutineInstruction> AutoPlayStateMachine()
		{
			while (true)
			{
				switch (autoPlayStatus.State)
				{
					case AutoPlayState.Active:
						yield return new CoroutineRun(DoAutoPlayActive(), "Active");
						break;

					case AutoPlayState.WaitPlayerConfirm:
						yield return new CoroutineDelayWithPredicate(TimeSpan.FromSeconds(5), () => autoPlayStatus.State != AutoPlayState.WaitPlayerConfirm);
						if (autoPlayStatus.State == AutoPlayState.WaitPlayerConfirm)
							autoPlayStatus.State = AutoPlayState.Idle;
						break;

					default:
						var s = autoPlayStatus.State;
						while (s == autoPlayStatus.State)
							yield return null;
						break;
				}
			}
		}

		private IEnumerator<CoroutineInstruction> DoAutoPlayActive()
		{
			// On entry, we could be waiting for take win

			if (StatusDatabase.GameStatus.CurrentGameState == GameState.OfferGamble)
				StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;

			var oldSpeed = StatusDatabase.GameSpeedStatus.GameSpeed;
			if (StatusDatabase.ConfigurationStatus.GameConfig.IsAutoPlayChangeSpeedAllowed)
				StatusDatabase.GameSpeedStatus.IncreaseSpeedIfAllowed();

			while (autoPlayStatus.State == AutoPlayState.Active)
			{
				// If idle, initiate a new game.

				if (CanStartAutoPlayGame())
					GameInitiator.StartGame();

				if (StatusDatabase.GameStatus.GameIsActive)
				{
					while (StatusDatabase.GameStatus.GameIsActive)
						yield return null;

					var t = FrameTime.CurrentTime + TimeSpan.FromSeconds(0.5);

					while (true)
					{
						if (autoPlayStatus.State != AutoPlayState.Active)
							break;
						if (StatusDatabase.GameStatus.CurrentGameState != GameState.OfferGamble)
							break;
						if (t < FrameTime.CurrentTime)
						{
							StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
							break;
						}

						yield return null;
					}
				}
				else
					yield return null;
			}

			if (StatusDatabase.ConfigurationStatus.GameConfig.IsAutoPlayChangeSpeedAllowed &&
				StatusDatabase.GameSpeedStatus.IsChangeGameSpeedAllowed)
			{
				StatusDatabase.GameSpeedStatus.GameSpeed = oldSpeed;
			}
		}

		private IEnumerator<CoroutineInstruction> AutoPlayAutoStop()
		{
			while (true)
			{
				while (autoPlayStatus.State != AutoPlayState.Active)
					yield return null;

				var gameStatus = StatusDatabase.GameStatus;
				while (autoPlayStatus.State == AutoPlayState.Active)
				{
					if (gameStatus.CurrentGameState == GameState.Continuing && autoPlayStatus.ShouldStopAutoplayInFeature)
					{
						// We have entered a feature state, so stop.
						RequestStop(false);
					}

					yield return null;
				}
			}
		}

		private void OnAutoPlayMessageReceived(AutoPlayMessage msg)
		{
			switch (msg.Mode)
			{
				case AutoPlayMode.Start:
				{
					// Can be requested by foundation or player

					switch (msg.Source)
					{
						case AutoPlayRequestSource.Foundation:
							if (CanActivate() &&
								(autoPlayStatus.State == AutoPlayState.Idle || autoPlayStatus.State == AutoPlayState.WaitPlayerConfirm || autoPlayStatus.State == AutoPlayState.Starting))
							{
								autoPlayStatus.State = AutoPlayState.Active;
							}

							break;

						case AutoPlayRequestSource.Player:
							if (CanActivate() &&
								autoPlayStatus.State == AutoPlayState.Starting)
							{
								autoPlayStatus.State = AutoPlayState.Active;
							}

							break;
					}

					break;
				}

				case AutoPlayMode.StartCancelled:
					// Autoplay start request may be denied by the foundation

					if (autoPlayStatus.State == AutoPlayState.Starting)
						autoPlayStatus.State = AutoPlayState.Idle;

					break;

				case AutoPlayMode.Stop:
					// Stop can come from the foundation, game flow or a player request

					if (autoPlayStatus.State == AutoPlayState.Active || autoPlayStatus.State == AutoPlayState.Stopping)
						autoPlayStatus.State = AutoPlayState.Idle;
					break;
			}
		}
	}
}