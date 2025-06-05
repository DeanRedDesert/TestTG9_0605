using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Gamble;

namespace Midas.Presentation.Gaff
{
	public enum GaffMenuMode
	{
		NotAllowed,
		Idle,
		CustomDialUp
	}

	public sealed class GaffButtonController : ButtonController
	{
		private const int GaffButtonCount = 8;
		private static readonly Money maxMoneyIn = Money.FromMinorCurrency(100000000);
		private static readonly Money maxMoneyInShow = Money.FromMinorCurrency(100000);
		private static readonly Money minMoneyInShow = Money.FromMinorCurrency(10000);
		private static readonly int[] multipliers = { 1, 2, 5 };
		private GaffStatus gaffStatus;
		private GambleStatus gambleStatus;
		private GaffType? selectedGaffType = null;
		private int topGaffIndex;
		private IReadOnlyList<IGaffSequence> filteredGaffSequences;
		private ButtonMappingHandle buttonMappingHandle;
		private readonly List<DateTime> fastForwardPresses = new List<DateTime>();
		private bool isSkippingFeature;

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			gaffStatus = StatusDatabase.GaffStatus;
			gambleStatus = StatusDatabase.QueryStatusBlock<GambleStatus>();

			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(StatusDatabase.ConfigurationStatus.MachineConfig));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState));
			AddButtonConditionPropertyChanged(gaffStatus, nameof(GaffStatus.IsPopupActive));
			AddButtonConditionPropertyChanged(gaffStatus, nameof(GaffStatus.AddCreditsAmount));
			AddButtonConditionPropertyChanged(gaffStatus, nameof(GaffStatus.GaffSequences));
			AddButtonConditionPropertyChanged(gaffStatus, nameof(GaffStatus.IsDialUpActive));
			AddButtonConditionPropertyChanged(gaffStatus, nameof(GaffStatus.IsSelfPlayActive));
			AddButtonConditionPropertyChanged(gaffStatus, nameof(GaffStatus.IsSelfPlayAddCreditsActive));
			AddButtonConditionPropertyChanged(gaffStatus, nameof(GaffStatus.SelectedGaffIndex));

			RegisterButtonPressListener(GaffButtonFunctions.ToggleGaffMenu, OnToggleGaffMenu);
			RegisterButtonPressListener(GaffButtonFunctions.IncAddCreditsAmount, OnIncCreditsAmount);
			RegisterButtonPressListener(GaffButtonFunctions.DecAddCreditsAmount, OnDecCreditsAmount);
			RegisterButtonPressListener(GaffButtonFunctions.AddCredits, OnAddCredits);

			RegisterButtonPressListener(GaffButtonFunctions.GaffsUp, OnGaffsUp);
			RegisterButtonPressListener(GaffButtonFunctions.GaffsDown, OnGaffsDown);

			foreach (var gaffButton in GaffButtonFunctions.AllGaffButtons)
			{
				RegisterButtonPressListener(gaffButton, OnGaffButtonPressed);
			}

			RegisterButtonPressListener(GaffButtonFunctions.ClearGaff, OnClearGaff);
			RegisterButtonPressListener(GaffButtonFunctions.RepeatGaff, OnRepeatGaff);
			RegisterButtonPressListener(GaffButtonFunctions.ToggleTestMessages, OnToggleTestMessages);
			RegisterButtonPressListener(GaffButtonFunctions.ToggleSelfPlay, OnToggleSelfPlay);
			RegisterButtonPressListener(GaffButtonFunctions.ToggleSelfPlayAddCredits, OnToggleSelfPlayAddCredits);
			RegisterButtonPressListener(GaffButtonFunctions.ToggleDialUp, OnToggleDialUp);
			RegisterButtonPressListener(GaffButtonFunctions.ToggleDebug, OnToggleDebug);
			RegisterButtonPressListener(GaffButtonFunctions.SelfPlayAbort, OnSelfPlayAbort);
			RegisterButtonPressListener(GaffButtonFunctions.ChangeGaffFilter, OnChangeGaffFilter);

			RegisterButtonPressListener(GaffButtonFunctions.SpeedUp, OnSpeedUp);
			RegisterButtonPressListener(GaffButtonFunctions.SpeedDown, OnSpeedDown);
			RegisterButtonPressListener(GaffButtonFunctions.FastForwardFeature, OnFastForwardFeature);
			RegisterButtonPressListener(GaffButtonFunctions.DialUpAgain, OnDialUpUseResult);
			RegisterButtonPressListener(GaffButtonFunctions.DialUpContinue, OnDialUpUseResult);
		}

		protected override void UpdateButtonStates()
		{
			if (StatusDatabase.ConfigurationStatus.MachineConfig == null)
				return;

			if (!StatusDatabase.ConfigurationStatus.MachineConfig.AreShowFeaturesEnabled)
				return;

			if (!StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
				selectedGaffType = GaffType.Show;

			var gaffmode = GetGaffMenuMode();

			AddButtonState(GaffButtonFunctions.ToggleGaffMenu, gaffmode == GaffMenuMode.Idle ? ButtonState.Enabled : ButtonState.DisabledShow, LightState.Off);

			if (gaffStatus.IsPopupActive)
			{
				var isIdlePopup = gaffmode == GaffMenuMode.Idle;
				var idlePopupDisableState = isIdlePopup ? ButtonState.DisabledShow : ButtonState.DisabledHide;
				var addCreditsAllowed = StatusDatabase.GameStatus.CurrentGameState == GameState.Idle;
				AddButtonState(GaffButtonFunctions.AddCredits, addCreditsAllowed, idlePopupDisableState);
				AddButtonState(GaffButtonFunctions.IncAddCreditsAmount, addCreditsAllowed && CanIncCreditsAmount(), idlePopupDisableState);
				var (baseValue, multiplierIndex) = GetCurrentMultiplierDetails();
				AddButtonState(GaffButtonFunctions.DecAddCreditsAmount, addCreditsAllowed && CanDecCreditsAmount(baseValue, multiplierIndex), idlePopupDisableState);
				AddButtonState(GaffButtonFunctions.DialUpAgain, gaffmode == GaffMenuMode.CustomDialUp, ButtonState.DisabledHide);
				AddButtonState(GaffButtonFunctions.DialUpContinue, gaffmode == GaffMenuMode.CustomDialUp, ButtonState.DisabledHide);

				UpdateGaffButtonStates();
				AddButtonState(GaffButtonFunctions.GaffsUp, topGaffIndex > 0);
				AddButtonState(GaffButtonFunctions.GaffsDown, topGaffIndex + GaffButtonCount < AvailableGaffs());

				AddButtonState(GaffButtonFunctions.ClearGaff, isIdlePopup && gaffStatus.SelectedGaffIndex != null);

				if (StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
				{
					AddButtonState(GaffButtonFunctions.ChangeGaffFilter, isIdlePopup, selectedGaffType);
					AddButtonState(GaffButtonFunctions.RepeatGaff, isIdlePopup && gaffStatus.SelectedGaffIndex != null, gaffStatus.RepeatSelectedGaff);
					AddButtonState(GaffButtonFunctions.ToggleSelfPlay, isIdlePopup, gaffStatus.IsSelfPlayActive);
					AddButtonState(GaffButtonFunctions.ToggleSelfPlayAddCredits, isIdlePopup && gaffStatus.IsSelfPlayActive, gaffStatus.IsSelfPlayAddCreditsActive);
					AddButtonState(GaffButtonFunctions.ToggleDialUp, isIdlePopup, gaffStatus.IsDialUpActive);
					AddButtonState(GaffButtonFunctions.ToggleDebug, isIdlePopup, gaffStatus.IsDebugEnabled);

#if UNITY_EDITOR
					AddButtonState(GaffButtonFunctions.ToggleTestMessages, true, StatusDatabase.DashboardStatus.TestMessages != null);
#endif
				}
			}

			AddButtonState(GaffButtonFunctions.SelfPlayAbort, StatusDatabase.GameStatus.InActivePlay && gaffStatus.IsSelfPlayActive && !gaffStatus.IsPopupActive, ButtonState.DisabledHide);
			UpdateSpeedButtonStates();
		}

		private void UpdateGaffButtonStates()
		{
			ApplyGaffFilter();

			var selectedGaff = GetGaffMenuMode() == GaffMenuMode.CustomDialUp ? gaffStatus.CustomGaffs[gaffStatus.SelectedCustomGaffIndex] : gaffStatus.SelectedGaff;

			for (var i = 0; i < GaffButtonCount; i++)
			{
				var button = GaffButtonFunctions.AllGaffButtons[i];
				var gaffIndex = i + topGaffIndex;
				var gaff = gaffIndex < filteredGaffSequences.Count ? filteredGaffSequences[gaffIndex] : null;

				AddButtonState(button, gaff != null, (gaff, gaff != null && gaff == selectedGaff));
			}
		}

		private void UpdateSpeedButtonStates()
		{
			AddButtonState(GaffButtonFunctions.SpeedUp, true);
			AddButtonState(GaffButtonFunctions.SpeedDown, true);

			var gs = StatusDatabase.GameStatus.CurrentGameState;
			if (gs == GameState.Idle || gs == GameState.OfferGamble || gs == GameState.History)
			{
				isSkippingFeature = false;
				if (buttonMappingHandle != null)
				{
					CabinetManager.Cabinet.PopButtonMapping(buttonMappingHandle);
					buttonMappingHandle = null;
				}

				AddButtonState(GaffButtonFunctions.FastForwardFeature, ButtonState.DisabledHide, LightState.Off);
			}
			else
			{
				if (buttonMappingHandle == null)
				{
					var cashOutButtons = CabinetManager.Cabinet.GetDefaultButtonsFromButtonFunction(DashboardButtonFunctions.Cashout).ToArray();
					buttonMappingHandle = CabinetManager.Cabinet.PushButtonMapping(() => cashOutButtons.Select(b => (b, GaffButtonFunctions.FastForwardFeature)));
				}

				AddButtonState(GaffButtonFunctions.FastForwardFeature, ButtonState.Enabled, LightState.Off);
			}
		}

		private void ApplyGaffFilter()
		{
			switch (GetGaffMenuMode())
			{
				case GaffMenuMode.Idle:
					if (selectedGaffType.HasValue)
						filteredGaffSequences = gaffStatus.GaffSequences?.Where(s => s.GaffType == selectedGaffType.Value).ToArray() ?? Array.Empty<IGaffSequence>();
					else
						filteredGaffSequences = gaffStatus.GaffSequences ?? Array.Empty<IGaffSequence>();

					break;

				case GaffMenuMode.CustomDialUp:
					if (!ReferenceEquals(filteredGaffSequences, gaffStatus.CustomGaffs))
					{
						filteredGaffSequences = gaffStatus.CustomGaffs;
						topGaffIndex = 0;
					}

					break;
			}
		}

		private int AvailableGaffs()
		{
			return filteredGaffSequences.Count;
		}

		private GaffMenuMode GetGaffMenuMode()
		{
			if (StatusDatabase.ConfigurationStatus.MachineConfig?.AreShowFeaturesEnabled == true && StatusDatabase.GameStatus.InActivePlay)
			{
				switch (StatusDatabase.GameStatus.CurrentGameState)
				{
					case GameState.Idle:
					case GameState.OfferGamble:
					case GameState.StartingGamble when gambleStatus.AwaitingSelection:
						return GaffMenuMode.Idle;

					case GameState.StartingGamble:
					case GameState.StartingCreditPlayoff:
						return GaffMenuMode.CustomDialUp;
				}
			}

			return GaffMenuMode.NotAllowed;
		}

		private bool IsGaffAllowed()
		{
			return GetGaffMenuMode() == GaffMenuMode.Idle;
		}

		private void OnToggleGaffMenu(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed())
				gaffStatus.IsPopupActive = !gaffStatus.IsPopupActive;
		}

		private (long baseValue, int multiplierIndex) GetCurrentMultiplierDetails()
		{
			var baseValue = 1;
			var currentMinorCurrency = gaffStatus.AddCreditsAmount.AsMinorCurrency;
			while (baseValue * 10 <= currentMinorCurrency)
				baseValue *= 10;

			var multIndex = 0;

			for (; multIndex < multipliers.Length; multIndex++)
			{
				if (multipliers[multIndex] * baseValue >= currentMinorCurrency)
					break;
			}

			return (baseValue, multIndex);
		}

		private void UpdateCreditAmount(long baseValue, int multiplierIndex)
		{
			gaffStatus.AddCreditsAmount = Money.FromMinorCurrency(multipliers[multiplierIndex] * baseValue);
		}

		private bool CanIncCreditsAmount()
		{
			var mm = StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled ? maxMoneyIn : maxMoneyInShow;
			return gaffStatus.AddCreditsAmount < mm;
		}

		private bool CanDecCreditsAmount(long baseValue, int multiplierIndex)
		{
			if (!StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
				return gaffStatus.AddCreditsAmount > minMoneyInShow;

			return baseValue > 1 || multiplierIndex > 0;
		}

		private void OnIncCreditsAmount(ButtonEventData buttonEvent)
		{
			var (baseValue, multiplierIndex) = GetCurrentMultiplierDetails();
			if (IsGaffAllowed() && gaffStatus.IsPopupActive && CanIncCreditsAmount())
			{
				multiplierIndex++;
				if (multiplierIndex >= multipliers.Length)
				{
					baseValue *= 10;
					multiplierIndex = 0;
				}

				UpdateCreditAmount(baseValue, multiplierIndex);
			}
		}

		private void OnDecCreditsAmount(ButtonEventData buttonEvent)
		{
			var (baseValue, multiplierIndex) = GetCurrentMultiplierDetails();
			if (IsGaffAllowed() && gaffStatus.IsPopupActive && CanDecCreditsAmount(baseValue, multiplierIndex))
			{
				multiplierIndex--;
				if (multiplierIndex < 0)
				{
					baseValue /= 10;
					multiplierIndex = multipliers.Length - 1;
				}

				UpdateCreditAmount(baseValue, multiplierIndex);
			}
		}

		private void OnAddCredits(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && StatusDatabase.GameStatus.CurrentGameState == GameState.Idle)
				Communication.ToLogicSender.Send(new ShowAddMoneyMessage(gaffStatus.AddCreditsAmount));
		}

		private void OnGaffsUp(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && gaffStatus.IsPopupActive && topGaffIndex > 0)
			{
				topGaffIndex -= GaffButtonCount;
				if (topGaffIndex < 0)
					topGaffIndex = 0;
				RequestButtonUpdate();
			}
		}

		private void OnGaffsDown(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && gaffStatus.IsPopupActive && topGaffIndex + GaffButtonCount < AvailableGaffs())
			{
				topGaffIndex += GaffButtonCount;
				RequestButtonUpdate();
			}
		}

		private void OnGaffButtonPressed(ButtonEventData buttonEvent)
		{
			if (GetGaffMenuMode() != GaffMenuMode.NotAllowed && gaffStatus.IsPopupActive)
			{
				var gaffIndex = topGaffIndex + buttonEvent.ButtonFunction.Id - GaffButtonFunctions.Gaff0.Id;

				if (gaffIndex >= 0 && gaffIndex < AvailableGaffs())
				{
					if (GetGaffMenuMode() == GaffMenuMode.CustomDialUp)
						gaffStatus.SelectedCustomGaffIndex = gaffIndex;
					else
						gaffStatus.SelectedGaffIndex = gaffStatus.GaffSequences.FindIndex(filteredGaffSequences[gaffIndex]);
				}

				RequestButtonUpdate();
			}
		}

		private void OnClearGaff(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && gaffStatus.IsPopupActive)
			{
				gaffStatus.SelectedGaffIndex = null;
				gaffStatus.RepeatSelectedGaff = false;
				RequestButtonUpdate();
			}
		}

		private void OnRepeatGaff(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && gaffStatus.IsPopupActive && gaffStatus.SelectedGaffIndex != null)
			{
				gaffStatus.RepeatSelectedGaff = !gaffStatus.RepeatSelectedGaff;
				RequestButtonUpdate();
			}
		}

		private void OnToggleTestMessages(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && gaffStatus.IsPopupActive && StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
			{
				if (StatusDatabase.DashboardStatus.TestMessages == null)
				{
					StatusDatabase.DashboardStatus.SetTestMessages(new[]
					{
						"This is a short message",
						"This is a really long test message that should cause scrolling to occur.",
						"Maximum Width Test Message WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
						"Toggle these test messages in the gaff menu."
					});
				}
				else
				{
					StatusDatabase.DashboardStatus.SetTestMessages(null);
				}

				RequestButtonUpdate();
			}
		}

		private void OnToggleSelfPlay(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && gaffStatus.IsPopupActive && StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
			{
				gaffStatus.IsSelfPlayActive = !gaffStatus.IsSelfPlayActive;
				if (!gaffStatus.IsSelfPlayActive)
					gaffStatus.IsSelfPlayAddCreditsActive = false;
				gaffStatus.IsDialUpActive = false;
				RequestButtonUpdate();
			}
		}

		private void OnToggleSelfPlayAddCredits(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && gaffStatus.IsPopupActive && gaffStatus.IsSelfPlayActive && StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
			{
				gaffStatus.IsSelfPlayAddCreditsActive = !gaffStatus.IsSelfPlayAddCreditsActive;
				RequestButtonUpdate();
			}
		}

		private void OnToggleDialUp(ButtonEventData buttonEvent)
		{
			if (GetGaffMenuMode() != GaffMenuMode.NotAllowed && StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
			{
				gaffStatus.IsDialUpActive = !gaffStatus.IsDialUpActive;
				RequestButtonUpdate();
			}
		}

		private void OnToggleDebug(ButtonEventData obj)
		{
			if (GetGaffMenuMode() != GaffMenuMode.NotAllowed && StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
			{
				gaffStatus.IsDebugEnabled = !gaffStatus.IsDebugEnabled;
				RequestButtonUpdate();
			}
		}

		private void OnSelfPlayAbort(ButtonEventData buttonEvent)
		{
			if (StatusDatabase.ConfigurationStatus.MachineConfig?.AreShowFeaturesEnabled == true && StatusDatabase.GameStatus.InActivePlay)
			{
				gaffStatus.IsSelfPlayActive = false;
				gaffStatus.IsSelfPlayAddCreditsActive = false;
				RequestButtonUpdate();
			}
		}

		private void OnChangeGaffFilter(ButtonEventData buttonEvent)
		{
			if (IsGaffAllowed() && StatusDatabase.ConfigurationStatus.MachineConfig.AreDevFeaturesEnabled)
			{
				if (selectedGaffType == null)
					selectedGaffType = GaffType.Show;
				else if (selectedGaffType == GaffType.History)
					selectedGaffType = null;
				else
					selectedGaffType++;

				ApplyGaffFilter();

				if (!filteredGaffSequences.Contains(gaffStatus.SelectedGaff))
				{
					gaffStatus.SelectedGaffIndex = null;
					gaffStatus.RepeatSelectedGaff = false;
				}

				RequestButtonUpdate();
			}
		}

		private void OnSpeedUp(ButtonEventData data)
		{
			if (gaffStatus.GameTimeScale < 1)
				gaffStatus.GameTimeScale = 1;
			else if (gaffStatus.GameTimeScale < 5)
				gaffStatus.GameTimeScale = 5;
			else if (gaffStatus.GameTimeScale < 10)
				gaffStatus.GameTimeScale = 10;
			else
				gaffStatus.GameTimeScale = 1;
		}

		private void OnSpeedDown(ButtonEventData data)
		{
			if (gaffStatus.GameTimeScale > 1)
				gaffStatus.GameTimeScale = 1;
			else if (gaffStatus.GameTimeScale > .5f)
				gaffStatus.GameTimeScale = .5f;
			else if (gaffStatus.GameTimeScale > .1f)
				gaffStatus.GameTimeScale = .1f;
			else
				gaffStatus.GameTimeScale = 1;
		}

		private void OnFastForwardFeature(ButtonEventData data)
		{
			if (StatusDatabase.ConfigurationStatus.MachineConfig.AreShowFeaturesEnabled && !isSkippingFeature)
			{
				var now = DateTime.Now;
				var halfSecondPast = now - TimeSpan.FromSeconds(0.5);
				fastForwardPresses.RemoveAll(ts => ts < halfSecondPast);
				fastForwardPresses.Add(now);
				if (fastForwardPresses.Count >= 3)
				{
					Communication.ToLogicSender.Send(new SkipFeatureMessage());
					isSkippingFeature = true;
					fastForwardPresses.Clear();
				}
			}

			gaffStatus.IsFeatureFastForwarding = !gaffStatus.IsFeatureFastForwarding;
		}

		private void OnDialUpUseResult(ButtonEventData data)
		{
			// In custom dialup, the game continues as soon as the popup is closed

			gaffStatus.IsPopupActive = false;
			gaffStatus.IsDialUpActive = data.ButtonFunction.Equals(GaffButtonFunctions.DialUpAgain);
		}
	}
}