using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Dashboard
{
	public sealed partial class DashboardController : IPresentationController
	{
		private Coroutine clockUpdateCoroutine;
		private Coroutine reserveCoroutine;
		private Coroutine externalJackpotCoroutine;
		private Coroutine gameNameCoroutine;
		private DashboardStatus dashboardStatus;
		private bool showReserve;
		private List<(IMessageHandler, Coroutine)> messageHandlers = new List<(IMessageHandler, Coroutine)>();

		public DashboardController() => dashboardStatus = StatusDatabase.DashboardStatus;

		public void Init()
		{
			ButtonManager.AddButtonEventListener(OnButtonEvent);
			Communication.PresentationDispatcher.AddHandler<MoneyInMessage>(OnMoneyIn);
			clockUpdateCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(ClockUpdate(), "Clock");
			if (StatusDatabase.ConfigurationStatus.MachineConfig != null)
				dashboardStatus.CurrentTime = DateTime.Now.ToString(StatusDatabase.ConfigurationStatus.ClockFormat);
			reserveCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(ReserveHandler(), "Reserve");
			externalJackpotCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(ExternalJackpotsHandler(), "ExternalJackpot");
			gameNameCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(GameNameCoroutine(), "GameName");
			InitVolume();
			InitPaidMeter();
			InitLanguage();
			InitMacau();
			InitGameMessages();
		}

		public void DeInit()
		{
			ButtonManager.RemoveButtonEventListener(OnButtonEvent);
			Communication.PresentationDispatcher.RemoveHandler<MoneyInMessage>(OnMoneyIn);
			clockUpdateCoroutine?.Stop();
			clockUpdateCoroutine = null;
			reserveCoroutine?.Stop();
			reserveCoroutine = null;
			externalJackpotCoroutine?.Stop();
			externalJackpotCoroutine = null;
			gameNameCoroutine?.Stop();
			gameNameCoroutine = null;
			DeInitVolume();
			DeInitPaidMeter();
			DeInitLanguage();
			DeInitMacau();
			DeInitGameMessages();
		}

		public void Destroy()
		{
			dashboardStatus = null;
		}

		private void OnButtonEvent(ButtonEventData eventData)
		{
			LanguageButtonCheck(eventData);
			VolumeButtonCheck(eventData);
		}

		private void OnMoneyIn(MoneyInMessage obj)
		{
			LanguageOnMoneyIn();
			VolumeOnMoneyIn();
		}

		public void ReserveRequest(bool show) => showReserve = show;

		public void RegisterMessageHandler(IMessageHandler messageHandler) => messageHandlers.Add((messageHandler, StateMachineService.FrameUpdateRoot.StartCoroutine(MessageHandler(messageHandler), "Message Strip")));

		public void UnregisterMessageHandler(IMessageHandler messageHandler)
		{
			var index = messageHandlers.FindIndex(h => ReferenceEquals(h.Item1, messageHandler));
			if (index == -1)
			{
				Log.Instance.Fatal("Attempted to unregister a message handler that wasn't registered.");
				return;
			}

			messageHandlers[index].Item2.Stop();
			messageHandlers[index].Item1.DisplayMessage(string.Empty);
			messageHandlers.RemoveAt(index);
		}

		private IEnumerator<CoroutineInstruction> ClockUpdate()
		{
			while (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				yield return null;

			if (StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction == "MCAU")
				yield break;

			bool ShouldHideClock() => !StatusDatabase.ConfigurationStatus.IsClockVisible;
			Func<bool> shouldHideFunc = ShouldHideClock;

			while (true)
			{
				if (StatusDatabase.ConfigurationStatus.IsClockVisible == false)
				{
					dashboardStatus.CurrentTime = string.Empty;
					while (StatusDatabase.ConfigurationStatus.IsClockVisible == false)
						yield return null;
				}

				while (StatusDatabase.ConfigurationStatus?.ClockFormat == null)
					yield return null;

				dashboardStatus.CurrentTime = DateTime.Now.ToString(StatusDatabase.ConfigurationStatus.ClockFormat);
				yield return new CoroutineDelayWithPredicate(TimeSpan.FromSeconds(0.5), shouldHideFunc);
			}
		}

		private IEnumerator<CoroutineInstruction> ReserveHandler()
		{
			dashboardStatus.ReserveTimeRemainingText = string.Empty;

			while (true)
			{
				while (!showReserve)
					yield return null;

				var reserveTime = StatusDatabase.BankStatus.BankMeter == Money.Zero ? StatusDatabase.ConfigurationStatus.ReserveParameters.TimeoutWithoutCredits : StatusDatabase.ConfigurationStatus.ReserveParameters.TimeoutWithCredits;
				var reserveEndTime = FrameTime.CurrentTime + reserveTime;
				StatusDatabase.PopupStatus.Open(Popup.Reserve);
				var reserveTimeRemaining = reserveEndTime - FrameTime.CurrentTime;
				dashboardStatus.ReserveTimeRemainingText = reserveTimeRemaining.Add(TimeSpan.FromSeconds(1)).ToString("m\\:ss");

				while (showReserve)
				{
					yield return null;
					reserveTimeRemaining = reserveEndTime - FrameTime.CurrentTime;
					if (StatusDatabase.GameStatus.GameLogicPaused || reserveTimeRemaining <= TimeSpan.Zero)
						showReserve = false;
					else
						dashboardStatus.ReserveTimeRemainingText = reserveTimeRemaining.Add(TimeSpan.FromSeconds(1)).ToString("m\\:ss");
				}

				StatusDatabase.PopupStatus.Close(Popup.Reserve);
				dashboardStatus.ReserveTimeRemainingText = string.Empty;
			}
		}

		private IEnumerator<CoroutineInstruction> ExternalJackpotsHandler()
		{
			while (true)
			{
				while (!StatusDatabase.ExternalJackpotsStatus.IsVisible)
					yield return null;

				var displayIndex = 0;
				var timeout = FrameTime.CurrentTime + TimeSpan.FromSeconds(4);

				var previousJackpots = StatusDatabase.ExternalJackpotsStatus.ExternalJackpots;
				var visibleJackpots = StatusDatabase.ExternalJackpotsStatus.ExternalJackpots.Where(j => j.IsVisible).ToArray();
				while (StatusDatabase.ExternalJackpotsStatus.IsVisible)
				{
					dashboardStatus.ExternalJackpotIsIconVisible = StatusDatabase.ExternalJackpotsStatus.IconId != 0;
					if (!ReferenceEquals(previousJackpots, StatusDatabase.ExternalJackpotsStatus.ExternalJackpots))
						visibleJackpots = StatusDatabase.ExternalJackpotsStatus.ExternalJackpots.Where(j => j.IsVisible).ToArray();

					if (displayIndex >= visibleJackpots.Length)
						displayIndex = 0;

					if (visibleJackpots.Length == 0)
					{
						displayIndex = 0;
						timeout = FrameTime.CurrentTime + TimeSpan.FromSeconds(4);
						dashboardStatus.ExternalJackpotHasVisibleJackpots = false;
						dashboardStatus.ExternalJackpotLabel = string.Empty;
						dashboardStatus.ExternalJackpotValue = string.Empty;
						yield return null;
						continue;
					}

					dashboardStatus.ExternalJackpotHasVisibleJackpots = true;
					dashboardStatus.ExternalJackpotLabel = visibleJackpots[displayIndex].Name;
					dashboardStatus.ExternalJackpotValue = visibleJackpots[displayIndex].Value;
					yield return null;

					if (FrameTime.CurrentTime < timeout)
						continue;

					timeout = FrameTime.CurrentTime + TimeSpan.FromSeconds(4);
					displayIndex++;
				}

				dashboardStatus.ExternalJackpotLabel = string.Empty;
				dashboardStatus.ExternalJackpotValue = string.Empty;
			}
		}

		private enum MessageState
		{
			Idle,
			NextMessage,
			ShowMessage
		}

		private IEnumerator<CoroutineInstruction> MessageHandler(IMessageHandler messageHandler)
		{
			messageHandler.DisplayMessage(string.Empty);
			IReadOnlyList<object> messages = new List<object>();
			var messageState = MessageState.Idle;

			IReadOnlyList<string> systemMessages = null;
			IReadOnlyList<string> testMessages = null;
			GameMessage? gameMessages = null;
			var currentMessageIndex = -1;

			IReadOnlyList<object> CheckForNewMessages()
			{
				var newSystemMessages = dashboardStatus.SystemMessages;
				var newTestMessages = dashboardStatus.TestMessages;

				if (!ReferenceEquals(newSystemMessages, systemMessages) || gameMessages != dashboardStatus.GameMessages ||
					!ReferenceEquals(newTestMessages, testMessages))
				{
					testMessages = dashboardStatus.TestMessages;
					systemMessages = newSystemMessages;
					gameMessages = dashboardStatus.GameMessages;

					var newMessages = new List<object>();
					if ((gameMessages & GameMessage.GameOver) != 0)
						newMessages.Add(GameMessage.GameOver);
					if ((gameMessages & GameMessage.PressTakeWin) != 0)
						newMessages.Add(GameMessage.PressTakeWin);
					if ((gameMessages & GameMessage.GambleAvailable) != 0)
						newMessages.Add(GameMessage.GambleAvailable);
					if (systemMessages != null)
						newMessages.AddRange(systemMessages);
					if (testMessages != null)
						newMessages.AddRange(testMessages);

					if (!messages.SequenceEqual(newMessages))
						return newMessages;
				}

				return null;
			}

			for (;;)
			{
				var newMessages = CheckForNewMessages();

				if (newMessages != null)
				{
					if (currentMessageIndex != -1)
					{
						// Check the current message's position in the new message list compared to the old one.
						// If there are new priority messages in the new message list, reset the current message back to the first one in the list.

						var message = messages[currentMessageIndex];
						var newMessageIndex = newMessages.FindIndex(message);

						if (newMessageIndex >= 0 && newMessageIndex <= currentMessageIndex)
							currentMessageIndex = newMessageIndex;
						else
							currentMessageIndex = -1;
					}

					messages = newMessages;
					if (currentMessageIndex == -1)
					{
						if (messages.Count == 0)
						{
							messageState = MessageState.Idle;
							messageHandler.DisplayMessage(string.Empty);
						}
						else
						{
							messageState = MessageState.NextMessage;
						}
					}
				}

				switch (messageState)
				{
					case MessageState.Idle:
						if (messages.Count > 0)
							messageState = MessageState.NextMessage;
						break;

					case MessageState.NextMessage:
					{
						currentMessageIndex = (currentMessageIndex + 1) % messages.Count;
						switch (messages[currentMessageIndex])
						{
							case string s: messageHandler.DisplayMessage(s); break;
							case GameMessage gm: messageHandler.DisplayMessage(gm); break;
						}

						messageState = MessageState.ShowMessage;
						break;
					}

					case MessageState.ShowMessage:
					{
						if (messageHandler.MessageDisplayDone)
							messageState = MessageState.NextMessage;

						break;
					}

					default:
						throw new ArgumentOutOfRangeException();
				}

				yield return null;
			}
		}
	}
}