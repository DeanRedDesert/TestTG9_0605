using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.Denom
{
	public sealed class DenomButtonController : ButtonController
	{
		private DenomStatus denomStatus;
		private CreditPlayoffStatusBase creditPlayoffStatus;
		private DenomController denomController;

		public override void Init()
		{
			denomStatus = StatusDatabase.DenomStatus;
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>();
			denomController = GameBase.GameInstance.GetPresentationController<DenomController>();
			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();
			denomStatus = null;
			creditPlayoffStatus = null;
			denomController = null;
		}

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.DenomConfig));
			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.DenomBetData));
			AddButtonConditionExpressionChanged(typeof(DashboardExpressions), nameof(DashboardExpressions.IsAnyPopupOpen));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameIsIdle));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameLogicPaused));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.GameButtonBehaviours));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.DenominationPlayableStatus));
			AddButtonConditionAnyPropertyChanged(denomStatus);
			AddButtonConditionExpressionChanged(typeof(PopupStatus), nameof(PopupStatus.IsCreditPlayoffOpen));
			AddButtonConditionPropertyChanged(StatusDatabase.AutoPlayStatus, nameof(AutoPlayStatus.State));

			RegisterButtonPressListener(DenomButtonFunctions.ChangeDenom, OnChangeDenomButtonPressed);
			RegisterButtonPressListener(DenomButtonFunctions.ConfirmYes, OnConfirmYesButtonPressed);
			RegisterButtonPressListener(DenomButtonFunctions.ConfirmNo, OnConfirmNoButtonPressed);

			for (var i = 0; i < DenomButtonFunctions.DenomButtons.Count; i++)
				RegisterButtonPressListener(DenomButtonFunctions.DenomButtons[i], OnDenomButtonPressed);
		}

		protected override void UpdateButtonStates()
		{
			// TODO: We could have an issue if the current denom is not in the visible denoms and
			// then the visible denom count goes to 1

			var visibleDenoms = DenomExpressions.VisibleDenoms;
			if (!StatusDatabase.GameFunctionStatus.GameButtonBehaviours.DenominationSelectionButtons.IsVisible() || visibleDenoms.Count <= 1)
			{
				AddButtonState(DenomButtonFunctions.ChangeDenom, ButtonState.DisabledHide);
				AddButtonState(DenomButtonFunctions.ConfirmYes, ButtonState.DisabledHide);
				AddButtonState(DenomButtonFunctions.ConfirmNo, ButtonState.DisabledHide);

				// TODO: Need to verify if the denom functions need a disable as well

				return;
			}

			var buttonEnabled = IsChangeDenomButtonEnabled();

			AddButtonState(DenomButtonFunctions.ChangeDenom, buttonEnabled);

			buttonEnabled = IsConfirmEnabled();
			AddButtonState(DenomButtonFunctions.ConfirmYes, buttonEnabled);
			AddButtonState(DenomButtonFunctions.ConfirmNo, buttonEnabled);

			for (var i = 0; i < visibleDenoms.Count; i++)
			{
				buttonEnabled = IsDenomButtonEnabled() && visibleDenoms[i].Status.IsActive();
				var denom = visibleDenoms[i].Denom;
				var buttonFunction = DenomButtonFunctions.DenomButtons[i];

				var denomState = DenomState.Normal;
				var isHighlighted = !StatusDatabase.GameStatus.GameLogicPaused && denom.Equals(denomStatus.SelectedDenom);

				if (isHighlighted)
				{
					switch (denomStatus.DenomMenuState)
					{
						case DenomMenuState.Attract:
							denomState = DenomState.Attract;
							break;

						case DenomMenuState.Hidden:
						case DenomMenuState.Confirm:
						case DenomMenuState.WaitForChange:
							denomState = DenomState.Active;
							break;
					}
				}

				if (!isHighlighted && denomStatus.DenomMenuState == DenomMenuState.Confirm)
					denomState = DenomState.Disable;

				if (!buttonEnabled)
					denomState = DenomState.Disable;

				StatusDatabase.ConfigurationStatus.DenomBetData.TryGetValue(denom, out var betData);
				AddButtonState(buttonFunction, buttonEnabled, new DenomButtonSpecificData(denom, denomState, betData));
			}
		}

		private bool IsChangeDenomButtonEnabled()
		{
			return StatusDatabase.GameStatus.InActivePlay &&
				StatusDatabase.GameStatus.GameIsIdle &&
				DenomExpressions.VisibleDenoms.Count > 1 &&
				StatusDatabase.GameFunctionStatus.GameButtonBehaviours.DenominationSelectionButtons.IsActive() &&
				(!DashboardExpressions.IsAnyPopupOpen || denomStatus.DenomMenuState != DenomMenuState.Hidden || PopupStatus.IsCashoutConfirmOpen) &&
				(StatusDatabase.AutoPlayStatus.State == AutoPlayState.Idle || StatusDatabase.AutoPlayStatus.State == AutoPlayState.WaitPlayerConfirm) &&
				!PopupStatus.IsCreditPlayoffOpen;
		}

		private bool IsDenomButtonEnabled()
		{
			return StatusDatabase.GameStatus.InActivePlay &&
				StatusDatabase.GameStatus.GameIsIdle &&
				DenomExpressions.VisibleDenoms.Count > 1 &&
				StatusDatabase.GameFunctionStatus.GameButtonBehaviours.DenominationSelectionButtons.IsActive() &&
				(!DashboardExpressions.IsAnyPopupOpen || denomStatus.DenomMenuState != DenomMenuState.Hidden) &&
				StatusDatabase.AutoPlayStatus.State == AutoPlayState.Idle &&
				!PopupStatus.IsCreditPlayoffOpen;
		}

		private bool IsConfirmEnabled()
		{
			return IsDenomButtonEnabled() && denomStatus.DenomMenuState == DenomMenuState.Confirm;
		}

		private void OnChangeDenomButtonPressed(ButtonEventData eventData)
		{
			if (IsChangeDenomButtonEnabled())
			{
				if (denomStatus.DenomMenuState == DenomMenuState.Hidden)
					denomController.ShowMenu();
				else
					denomController.HideMenu();
			}
		}

		private void OnConfirmYesButtonPressed(ButtonEventData eventData)
		{
			if (IsConfirmEnabled())
			{
				denomController.ConfirmGameDenom(true);
			}
		}

		private void OnConfirmNoButtonPressed(ButtonEventData eventData)
		{
			if (IsConfirmEnabled())
			{
				denomController.ConfirmGameDenom(false);
			}
		}

		private void OnDenomButtonPressed(ButtonEventData eventData)
		{
			if (IsDenomButtonEnabled())
			{
				var denomIndex = DenomButtonFunctions.DenomButtons.FindIndex(eventData.ButtonFunction);
				var denom = DenomExpressions.VisibleDenoms[denomIndex].Denom;

				if (StatusDatabase.ConfigurationStatus.DenomConfig.IsConfirmationRequired)
					denomController.SelectGameDenom(denom);
				else
					denomController.ChangeDenomImmediately(denom);
			}
		}
	}
}