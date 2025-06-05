using System.Collections.Generic;
using System.Diagnostics;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.Data;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Presentation.Dashboard
{
	public sealed class DashboardStatus : StatusBlock
	{
		private StatusProperty<bool> isAnyPopupOpen;
		private StatusProperty<string> currentTime;
		private StatusProperty<string> reserveTimeRemainingText;
		private StatusProperty<bool> externalJackpotHasVisibleJackpots;
		private StatusProperty<string> externalJackpotValue;
		private StatusProperty<string> externalJackpotLabel;
		private StatusProperty<bool> externalJackpotIsIconVisible;
		private StatusProperty<IReadOnlyList<string>> systemMessages;
		private StatusProperty<GameMessage> gameMessages;
		private StatusProperty<IReadOnlyList<string>> testMessages;
		private StatusProperty<bool> volumePopupEnabled;
		private StatusProperty<float> volumeSliderPosition;
		private StatusProperty<bool> showWinMeter;
		private StatusProperty<bool> showPaidMeter;
		private StatusProperty<bool> showMacauInfo;
		private StatusProperty<bool> isMacauClockVisible;
		private StatusProperty<GameMessageRight> gameMessageRight;
		private StatusProperty<GameMessageLeft> gameMessageLeft;

		public string CurrentTime
		{
			get => currentTime.Value;
			set => currentTime.Value = value;
		}

		public string ReserveTimeRemainingText
		{
			get => reserveTimeRemainingText.Value;
			set => reserveTimeRemainingText.Value = value;
		}

		public bool ExternalJackpotHasVisibleJackpots
		{
			get => externalJackpotHasVisibleJackpots.Value;
			set => externalJackpotHasVisibleJackpots.Value = value;
		}

		public string ExternalJackpotValue
		{
			get => externalJackpotValue.Value;
			set => externalJackpotValue.Value = value;
		}

		public string ExternalJackpotLabel
		{
			get => externalJackpotLabel.Value;
			set => externalJackpotLabel.Value = value;
		}

		public bool ExternalJackpotIsIconVisible
		{
			get => externalJackpotIsIconVisible.Value;
			set => externalJackpotIsIconVisible.Value = value;
		}

		public IReadOnlyList<string> SystemMessages => systemMessages.Value;

		public GameMessage GameMessages => gameMessages.Value;

		public IReadOnlyList<string> TestMessages => testMessages.Value;

		public bool VolumePopupEnabled
		{
			get => volumePopupEnabled.Value;
			set => volumePopupEnabled.Value = value;
		}

		public float VolumeSliderPosition
		{
			get => volumeSliderPosition.Value;
			set => volumeSliderPosition.Value = value;
		}

		public bool ShowWinMeter
		{
			get => showWinMeter.Value;
			set
			{
				if (showWinMeter.Value != value)
					showWinMeter.Value = value;
			}
		}

		public bool ShowPaidMeter
		{
			get => showPaidMeter.Value;
			set
			{
				if (showPaidMeter.Value != value)
					showPaidMeter.Value = value;
			}
		}

		public bool ShowMacauInfo
		{
			get => showMacauInfo.Value;
			set
			{
				if (showMacauInfo.Value != value)
					showMacauInfo.Value = value;
			}
		}

		public bool IsMacauClockVisible
		{
			get => isMacauClockVisible.Value;
			set => isMacauClockVisible.Value = value;
		}

		public GameMessageRight GameMessageRight
		{
			get => gameMessageRight.Value;
			set => gameMessageRight.Value = value;
		}

		public GameMessageLeft GameMessageLeft
		{
			get => gameMessageLeft.Value;
			set => gameMessageLeft.Value = value;
		}

		public DashboardStatus()
			: base(nameof(DashboardStatus))
		{
		}

		public void AddGameMessages(GameMessage gameMessage)
		{
			gameMessages.Value |= gameMessage;
		}

		public void RemoveGameMessages(GameMessage gameMessage)
		{
			gameMessages.Value &= ~gameMessage;
		}

		[Conditional("UNITY_EDITOR")]
		public void SetTestMessages(IReadOnlyList<string> messages)
		{
			testMessages.Value = messages;
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.ShowGameOverMessage, v =>
			{
				if (v)
					AddGameMessages(GameMessage.GameOver);
				else
					RemoveGameMessages(GameMessage.GameOver);
			});
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.Messages, v => systemMessages.Value = v);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			currentTime = AddProperty<string>(nameof(CurrentTime), null);
			reserveTimeRemainingText = AddProperty<string>(nameof(ReserveTimeRemainingText), null);
			systemMessages = AddProperty(nameof(SystemMessages), (IReadOnlyList<string>)new List<string>());
			gameMessages = AddProperty(nameof(GameMessages), GameMessage.None);
			testMessages = AddProperty<IReadOnlyList<string>>(nameof(TestMessages), null);
			externalJackpotHasVisibleJackpots = AddProperty(nameof(ExternalJackpotHasVisibleJackpots), false);
			externalJackpotValue = AddProperty<string>(nameof(ExternalJackpotValue), null);
			externalJackpotLabel = AddProperty<string>(nameof(ExternalJackpotLabel), null);
			externalJackpotIsIconVisible = AddProperty(nameof(ExternalJackpotIsIconVisible), false);
			volumePopupEnabled = AddProperty(nameof(VolumePopupEnabled), false);
			volumeSliderPosition = AddProperty(nameof(VolumeSliderPosition), 0f);
			showWinMeter = AddProperty(nameof(ShowWinMeter), true);
			showPaidMeter = AddProperty(nameof(ShowPaidMeter), false);
			showMacauInfo = AddProperty(nameof(ShowMacauInfo), false);
			isMacauClockVisible = AddProperty(nameof(IsMacauClockVisible), false);
			gameMessageRight = AddProperty(nameof(Dashboard.GameMessageRight), GameMessageRight.Nothing);
			gameMessageLeft = AddProperty(nameof(Dashboard.GameMessageLeft), GameMessageLeft.Nothing);
		}
	}
}