using System;
using System.Threading;
using Midas.Core;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;
using static Midas.LogicToPresentation.Communication;

namespace Midas.Logic
{
	public partial class GameLogic
	{
		private readonly AutoResetEvent messageReceivedEvent = new AutoResetEvent(false);

		/// <summary>
		/// Wait for the specified presentation message
		/// </summary>
		/// <typeparam name="TMessage">Type of message to wait for</typeparam>
		/// <returns>The received message</returns>
		private TMessage WaitPresentation<TMessage>() where TMessage : class, IMessage
		{
			TMessage message = null;
			var onPresentationMessage = new Action<TMessage>(m => message = m);

			LogicDispatcher.AddHandler(onPresentationMessage);

			try
			{
				GameServices.EndBatch();
				ToPresentationSender.SendEnqueuedMessages();
				ProcessMessagesWhile(() => message == null);
			}
			finally
			{
				GameServices.BeginBatch();
				LogicDispatcher.RemoveHandler(onPresentationMessage);
			}

			return message;
		}

		/// <summary>
		/// Waits for a presentation message without processing logic messages.
		/// </summary>
		/// <typeparam name="TMessage"></typeparam>
		/// <returns></returns>
		public static TMessage WaitPresentationMessagesOnly<TMessage>() where TMessage : class, IMessage
		{
			TMessage message = null;

			using var receivedEvent = new AutoResetEvent(false);

			var onPresentationMessage = new Action<TMessage>(m =>
			{
				message = m;
				// ReSharper disable once AccessToDisposedClosure - only registered for the lifetime of this method.
				receivedEvent.Set();
			});

			try
			{
				LogicDispatcher.AddHandler(onPresentationMessage);

				while (!receivedEvent.WaitOne(1) /*&& !foundation.ShouldGameLogicExit*/)
				{
					LogicDispatcher.DispatchOne();
				}
			}
			finally
			{
				LogicDispatcher.RemoveHandler(onPresentationMessage);
			}

			return message;
		}

		/// <summary>
		///     Processes messages until shouldProcessMessages ist not true any more
		///     Also stops when game should exit
		///     If game is paused it will wait until game is not paused any more
		/// </summary>
		/// <param name="shouldProcessMessages"></param>
		private void ProcessMessagesWhile(Func<bool> shouldProcessMessages)
		{
			if (LogicDispatcher.HasMessagesToDispatch())
				messageReceivedEvent.Set();

			while (foundation.IsPaused || shouldProcessMessages())
			{
				foundation.ProcessEvents(messageReceivedEvent);
				while (shouldProcessMessages())
				{
					var filter = foundation.IsPaused ? PausedMessageFilter : (Predicate<IMessage>)null;
					if (!LogicDispatcher.DispatchOne(filter))
					{
						break;
					}
				}
			}
		}

		private static bool PausedMessageFilter(IMessage message)
		{
			return message is GameLogicPauseMessage || message is DebugMessage || (message as PidMessage)?.Action is PidAction.Deactivated;
		}

		private void OnLogicMessageQueueMessageAdded()
		{
			messageReceivedEvent.Set();
		}

		private void RegisterMessages()
		{
			LogicDispatcher.AddHandler<GameStakeCombinationMessage>(OnStakeCombination);
			LogicDispatcher.AddHandler<CashoutMessage>(OnCashout);
			LogicDispatcher.AddHandler<RequestThemeSelectionMenuMessage>(OnRequestThemeSelectionMenu);
			LogicDispatcher.AddHandler<PidMessage>(OnPidMessage);
			LogicDispatcher.AddHandler<RunTimeInputMessage>(OnRunTimeInputMessage);
			LogicDispatcher.AddHandler<RunTimeDenomSelectionMessage>(OnRunTimeDenomSelectionMessage);
			LogicDispatcher.AddHandler<ChangeGameDenomMessage>(OnChangeGameDenomMessage);
			LogicDispatcher.AddHandler<GameLogicTimingsResetMessage>(OnGameLogicTimingsResetMessage);
			LogicDispatcher.AddHandler<PresentationDataChangeMessage>(OnPresentationDataChangeMessage);
			LogicDispatcher.AddHandler<AutoPlayMessage>(OnAutoPlayMessage);
			LogicDispatcher.AddHandler<ChangeLanguageMessage>(OnChangeLanguage);
			LogicDispatcher.AddHandler<PlayerSessionReportParametersBeingResetMessage>(OnPlayerSessionReportParametersBeingReset);
			LogicDispatcher.AddHandler<PlayerSessionInitiateResetMessage>(OnPlayerSessionInitiateResetMessage);
			LogicDispatcher.AddHandler<PlayerSessionActiveMessage>(OnPlayerSessionActiveMessage);
			LogicDispatcher.AddHandler<GamePostTiltMessage>(OnGamePostTiltMessage);
			LogicDispatcher.AddHandler<GameClearTiltMessage>(OnGameClearTiltMessage);

			if (foundation.ShowMode != FoundationShowMode.None)
			{
				LogicDispatcher.AddHandler<ShowAddMoneyMessage>(OnShowAddMoney);
				LogicDispatcher.AddHandler<DebugDisplayStateMessage>(OnDebugDisplayState);
				LogicDispatcher.AddHandler<DemoChangeModeMessage>(OnDemoChangeMode);
				LogicDispatcher.AddHandler<DemoActivateGaffMessage>(OnDemoActivateGaff);
				LogicDispatcher.AddHandler<DemoGaffResultsMessage>(OnDemoGaffResults);
				LogicDispatcher.AddHandler<RequestGaffDataMessage>(OnDemoRequestGaffDataMessage);
				LogicDispatcher.AddHandler<DemoChangeHistoryRecordMessage>(OnDemoChangeHistoryRecord);
				LogicDispatcher.AddHandler<SkipFeatureMessage>(OnSkipFeatureMessage);

				if (foundation.ShowMode == FoundationShowMode.Development)
					LogicDispatcher.AddHandler<DevCrashLogicMessage>(OnDevCrashLogic);
			}

			LogicDispatcher.AddHandler<RequestUtilityDataMessage>(OnRequestUtilityDataMessage);
			LogicDispatcher.AddHandler<UtilityResultsMessage>(OnUtilityResultsMessage);
		}

		private void DeregisterMessages()
		{
			LogicDispatcher.RemoveHandler<GameStakeCombinationMessage>(OnStakeCombination);
			LogicDispatcher.RemoveHandler<CashoutMessage>(OnCashout);
			LogicDispatcher.RemoveHandler<RequestThemeSelectionMenuMessage>(OnRequestThemeSelectionMenu);
			LogicDispatcher.RemoveHandler<PidMessage>(OnPidMessage);
			LogicDispatcher.RemoveHandler<RunTimeInputMessage>(OnRunTimeInputMessage);
			LogicDispatcher.RemoveHandler<RunTimeDenomSelectionMessage>(OnRunTimeDenomSelectionMessage);
			LogicDispatcher.RemoveHandler<ChangeGameDenomMessage>(OnChangeGameDenomMessage);
			LogicDispatcher.RemoveHandler<GameLogicTimingsResetMessage>(OnGameLogicTimingsResetMessage);
			LogicDispatcher.RemoveHandler<PresentationDataChangeMessage>(OnPresentationDataChangeMessage);
			LogicDispatcher.RemoveHandler<AutoPlayMessage>(OnAutoPlayMessage);
			LogicDispatcher.RemoveHandler<ChangeLanguageMessage>(OnChangeLanguage);
			LogicDispatcher.RemoveHandler<PlayerSessionReportParametersBeingResetMessage>(OnPlayerSessionReportParametersBeingReset);
			LogicDispatcher.RemoveHandler<PlayerSessionInitiateResetMessage>(OnPlayerSessionInitiateResetMessage);
			LogicDispatcher.RemoveHandler<PlayerSessionActiveMessage>(OnPlayerSessionActiveMessage);

			if (foundation.ShowMode != FoundationShowMode.None)
			{
				LogicDispatcher.RemoveHandler<ShowAddMoneyMessage>(OnShowAddMoney);
				LogicDispatcher.RemoveHandler<DebugDisplayStateMessage>(OnDebugDisplayState);
				LogicDispatcher.RemoveHandler<DemoChangeModeMessage>(OnDemoChangeMode);
				LogicDispatcher.RemoveHandler<DemoActivateGaffMessage>(OnDemoActivateGaff);
				LogicDispatcher.RemoveHandler<DemoGaffResultsMessage>(OnDemoGaffResults);
				LogicDispatcher.RemoveHandler<RequestGaffDataMessage>(OnDemoRequestGaffDataMessage);
				LogicDispatcher.RemoveHandler<DemoChangeHistoryRecordMessage>(OnDemoChangeHistoryRecord);
				LogicDispatcher.RemoveHandler<SkipFeatureMessage>(OnSkipFeatureMessage);

				if (foundation.ShowMode == FoundationShowMode.Development)
					LogicDispatcher.RemoveHandler<DevCrashLogicMessage>(OnDevCrashLogic);
			}
		}

		private void OnDebugDisplayState(DebugDisplayStateMessage message)
		{
			foundation.DemoSetDisplayState(message.NewDisplayState);
		}

		private void OnShowAddMoney(ShowAddMoneyMessage message)
		{
			if (foundation.ShouldGameLogicExit)
				return;

			if (foundation.GameMode == FoundationGameMode.Play)
				foundation.ShowAddMoney(message.Amount);
		}

		private void OnDemoChangeMode(DemoChangeModeMessage message)
		{
			if (message.GameMode == FoundationGameMode.Utility)
				foundation.DemoEnterUtilityMode(message.UtilityTheme, message.UtilityPaytables, message.UtilityDenomination);
			else
				foundation.DemoEnterGameMode(message.GameMode);
		}

		private void OnDemoActivateGaff(DemoActivateGaffMessage message)
		{
			game.SetGaffActive(message.GaffIndex);
		}

		private void OnDemoGaffResults(DemoGaffResultsMessage message) => game.SetGaffActive(message.GaffResults);

		private void OnDemoRequestGaffDataMessage(RequestGaffDataMessage obj) => ToPresentationSender.Send(new RequestGaffDataResponse(game.GetGaffData()));

		private void OnRequestUtilityDataMessage(RequestUtilityDataMessage obj)
		{
			if (GameServices.MachineStateService.GameModeService.Value != FoundationGameMode.Utility)
				throw new ArgumentException($"Attempting to read utility data when not in utility mode - {GameServices.MachineStateService.GameModeService.Value}");

			ToPresentationSender.Send(new RequestUtilityDataResponse(game.GetGaffData()));
		}

		private void OnUtilityResultsMessage(UtilityResultsMessage message)
		{
			if (GameServices.MachineStateService.GameModeService.Value != FoundationGameMode.Utility)
				throw new ArgumentException($"Attempting to set utility results when not in utility mode - {GameServices.MachineStateService.GameModeService.Value}");

			game.SetGaffActive(message.UtilityResults);
		}

		private void OnStakeCombination(GameStakeCombinationMessage message)
		{
			if (foundation.GameMode == FoundationGameMode.History)
			{
				Log.Instance.Warn("Received GameStakeCombinationMessage during History.");
				return;
			}

			if (!canChangeStakeCombo)
			{
				Log.Instance.Warn("Attempted to change input combination when not allowed.");
				return;
			}

			if (logicState.SelectedStakeCombinationIndex != message.SelectedStakeCombinationIndex)
			{
				logicState.SelectedStakeCombinationIndex = message.SelectedStakeCombinationIndex;
				SaveLogicState();
				GameServices.BetService.SelectedStakeComboService.SetValue(message.SelectedStakeCombinationIndex);
				creditPlayoff?.Decline();
			}
		}

		private void OnDemoChangeHistoryRecord(DemoChangeHistoryRecordMessage message)
		{
			switch (message.Direction)
			{
				case DemoHistoryRecordChangeDirection.Next:
					foundation.DemoNextHistoryRecord();
					break;
				case DemoHistoryRecordChangeDirection.Previous:
					foundation.DemoPreviousHistoryRecord();
					break;
			}
		}

		private void OnCashout(CashoutMessage obj)
		{
			if (foundation.ShouldGameLogicExit)
				return;

			if (foundation.GameMode == FoundationGameMode.Play)
			{
				foundation.RequestCashout();
				creditPlayoff?.Decline();
			}
		}

		private void OnRequestThemeSelectionMenu(RequestThemeSelectionMenuMessage message)
		{
			if (foundation.ShouldGameLogicExit)
				return;

			if (foundation.GameMode == FoundationGameMode.Play)
			{
				if (foundation.RequestThemeSelectionMenu())
				{
					UpdateChooserRequestedState(true);
					creditPlayoff?.Decline();
					ToPresentationSender.Send(message);
				}
			}
		}

		private void OnPidMessage(PidMessage pidMessage)
		{
			switch (pidMessage.Action)
			{
				case PidAction.Activated:
					foundation.PidActivated(true);
					break;
				case PidAction.Deactivated:
					foundation.PidActivated(false);
					break;
				case PidAction.GameInfoEntered:
					foundation.PidGameInfoEntered();
					break;
				case PidAction.SessionInfoEntered:
					foundation.PidSessionInfoEntered();
					break;
				case PidAction.StartSessionTracking:
					foundation.StartPidSessionTracking();
					break;
				case PidAction.StopSessionTracking:
					foundation.StopPidSessionTracking();
					break;
				case PidAction.ToggleService:
					foundation.ToggleServiceRequested();
					break;
			}
		}

		private void OnRunTimeInputMessage(RunTimeInputMessage runTimeInputMessage)
		{
			waitingForInputGeneric = runTimeInputMessage.Status;
			foundation.SendWaitingForInput(waitingForInputState || waitingForInputGeneric);
		}

		private void OnRunTimeDenomSelectionMessage(RunTimeDenomSelectionMessage runTimeDenomSelectionMessage)
		{
			if (runTimeDenomSelectionMessage.IsActive)
			{
				if (changeDenomState == ChangeDenomState.None)
				{
					UpdateChangeDenomState(ChangeDenomState.InMenu);
					foundation.SendDenomSelectionActive(true);
				}
			}
			else
			{
				// If we are in menu, the player has chosen to return to game.
				// If the denom state is Changing, then wait for the game to start up again.

				if (changeDenomState == ChangeDenomState.InMenu)
				{
					UpdateChangeDenomState(ChangeDenomState.None);
					foundation.SendDenomSelectionActive(false);
				}
			}
		}

		private void OnChangeGameDenomMessage(ChangeGameDenomMessage changeGameDenomMessage)
		{
			ChangeDenom(changeGameDenomMessage.Denom, false);
		}

		private void SetWaitingForInputState(bool fromState)
		{
			waitingForInputState = fromState;
			foundation.SendWaitingForInput(waitingForInputState || waitingForInputGeneric);
		}

		private void OnGameLogicTimingsResetMessage(GameLogicTimingsResetMessage obj)
		{
			foundation.ResetTimings();
		}

		private void OnSkipFeatureMessage(SkipFeatureMessage msg)
		{
			if (foundation.ShowMode != FoundationShowMode.None)
				isSkippingFeature = true;
		}

		private void OnPresentationDataChangeMessage(PresentationDataChangeMessage msg)
		{
			if (foundation.GameMode == FoundationGameMode.Play)
			{
				presentationData[msg.Name] = (msg.HistoryRequired, msg.Data);
				SavePresentationData();
			}
		}

		private void OnDevCrashLogic(DevCrashLogicMessage msg)
		{
			if (foundation.ShowMode == FoundationShowMode.Development)
				throw new Exception("Test logic crash");
		}

		private void OnChangeLanguage(ChangeLanguageMessage msg)
		{
			foundation.ChangeLanguage(msg.Language);
		}

		private void OnPlayerSessionReportParametersBeingReset(PlayerSessionReportParametersBeingResetMessage msg)
		{
			foundation.ReportParametersBeingReset(msg.ParametersBeingReset);
		}

		private void OnPlayerSessionInitiateResetMessage(PlayerSessionInitiateResetMessage msg)
		{
			foundation.InitiatePlayerSessionReset(msg.ParametersToReset);
		}

		private void OnPlayerSessionActiveMessage(PlayerSessionActiveMessage msg)
		{
			foundation.PlayerSessionActive(msg.IsActive);
		}

		private void OnGamePostTiltMessage(GamePostTiltMessage msg)
		{
			foundation.PostTilt(msg.TiltKey, msg.Priority, msg.Title, msg.Message, msg.IsBlocking, msg.DiscardOnGameShutdown, msg.UserInterventionRequired);
		}

		private void OnGameClearTiltMessage(GameClearTiltMessage msg)
		{
			foundation.ClearTilt(msg.TiltKey);
		}
	}
}