using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Denom;
using Midas.Presentation.Game;
using Midas.Presentation.Info;

namespace Midas.Presentation.AutoPlay
{
	public sealed class AutoPlayButtonController : ButtonController
	{
		private AutoPlayController autoPlayController;

		public override void Init()
		{
			autoPlayController = GameBase.GameInstance.GetPresentationController<AutoPlayController>();
			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();
			autoPlayController = null;
		}

		protected override void RegisterEvents()
		{
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(BankStatus.BankMeter));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFlowStatus, nameof(GameFlowStatus.AutoPlayCanStart));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.CurrentGameState));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameIsActive));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.PopupStatus, nameof(PopupStatus.Status));
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(BankStatus.IsPlayerWagerAvailable));
			AddButtonConditionPropertyChanged(StatusDatabase.ButtonEventDataQueueStatus, nameof(ButtonEventDataQueueStatus.ButtonFunction));
			AddButtonConditionPropertyChanged(StatusDatabase.AutoPlayStatus, nameof(AutoPlayStatus.State));
			AddButtonConditionPropertyChanged(StatusDatabase.DenomStatus, nameof(DenomStatus.DenomMenuState));
			AddButtonConditionExpressionChanged(typeof(DashboardExpressions), nameof(DashboardExpressions.IsAnyPopupOpen));

			RegisterButtonPressListener(AutoPlayButtonFunctions.StartAutoPlay, OnStartAutoPlay);
			RegisterButtonPressListener(AutoPlayButtonFunctions.StopAutoPlay, OnStopAutoPlay);
			RegisterButtonPressListener(AutoPlayButtonFunctions.AutoPlayConfirmYes, OnAutoPlayConfirmYes);
			RegisterButtonPressListener(AutoPlayButtonFunctions.AutoPlayConfirmNo, OnAutoPlayConfirmNo);
		}

		protected override void UpdateButtonStates()
		{
			if (StatusDatabase.ConfigurationStatus.GameConfig == null || !StatusDatabase.ConfigurationStatus.GameConfig.IsPlayerSelectableAutoplayAllowed)
			{
				AddButtonState(AutoPlayButtonFunctions.StartAutoPlay, ButtonState.DisabledHide);
				AddButtonState(AutoPlayButtonFunctions.StopAutoPlay, ButtonState.DisabledHide);
				AddButtonState(AutoPlayButtonFunctions.AutoPlayConfirmYes, ButtonState.DisabledHide);
				AddButtonState(AutoPlayButtonFunctions.AutoPlayConfirmNo, ButtonState.DisabledHide);
				return;
			}

			var isStopVisible = StatusDatabase.AutoPlayStatus.State == AutoPlayState.Active || StatusDatabase.AutoPlayStatus.State == AutoPlayState.Stopping;
			var buttonState = !isStopVisible
				? autoPlayController.CanRequestStart() || autoPlayController.CanConfirmStart() ? ButtonState.Enabled : ButtonState.DisabledShow
				: ButtonState.DisabledHide;

			AddButtonState(AutoPlayButtonFunctions.StartAutoPlay, buttonState, buttonState == ButtonState.Enabled && StatusDatabase.AutoPlayStatus.State == AutoPlayState.WaitPlayerConfirm);

			var enabled = autoPlayController.CanConfirmStart();

			AddButtonState(AutoPlayButtonFunctions.AutoPlayConfirmYes, enabled);
			AddButtonState(AutoPlayButtonFunctions.AutoPlayConfirmNo, enabled);

			buttonState = isStopVisible
				? autoPlayController.CanRequestStop() ? ButtonState.Enabled : ButtonState.DisabledShow
				: ButtonState.DisabledHide;

			AddButtonState(AutoPlayButtonFunctions.StopAutoPlay, buttonState);
		}

		private void OnStartAutoPlay(ButtonEventData eventData)
		{
			// If confirm is visible, treat as a cancel confirm (and do it immediately)

			if (StatusDatabase.AutoPlayStatus.State == AutoPlayState.WaitPlayerConfirm)
				autoPlayController.ConfirmStart(false);
			else if (autoPlayController.CanRequestStart())
			{
				ButtonQueueController.Enqueue(eventData, IsStartHindered, ExecuteStart);
			}
		}

		private void OnAutoPlayConfirmYes(ButtonEventData eventData)
		{
			if (autoPlayController.CanConfirmStart())
				ButtonQueueController.Enqueue(eventData, IsConfirmHindered, ExecuteConfirm);
		}

		private void OnAutoPlayConfirmNo(ButtonEventData eventData)
		{
			// ConfirmStart() calls CanConfirmStart() for us
			autoPlayController.ConfirmStart(false);
		}

		private void OnStopAutoPlay(ButtonEventData eventData)
		{
			if (autoPlayController.CanRequestStop())
				ButtonQueueController.Enqueue(eventData, IsStopHindered, ExecuteStop);
		}

		private void ExecuteStart(ButtonEventData eventData)
		{
			// RequestStart() calls CanRequestStart() for us

			autoPlayController.RequestStart();
		}

		private void ExecuteConfirm(ButtonEventData eventData)
		{
			// ConfirmStart() calls CanConfirmStart() for us

			autoPlayController.ConfirmStart(true);
		}

		private void ExecuteStop(ButtonEventData eventData)
		{
			// RequestStop() calls CanRequestStop() for us

			autoPlayController.RequestStop(true);
		}

		private ButtonFunctionHinderedReasons IsStartHindered()
		{
			const ButtonFunctionHinderedReasons reasons = ButtonFunctionHinderedReasons.BecauseInterruptable | ButtonFunctionHinderedReasons.BecauseNotPlayerWagerOfferable;

			return AutoPlayExpressions.IsAutoPlayConfirmationRequired
				? ButtonQueueController.IsHindered(reasons)
				: ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseGameCycleActive | reasons);
		}

		private ButtonFunctionHinderedReasons IsConfirmHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseGameCycleActive);
		}

		private ButtonFunctionHinderedReasons IsStopHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseInterruptableWinPres);
		}
	}
}