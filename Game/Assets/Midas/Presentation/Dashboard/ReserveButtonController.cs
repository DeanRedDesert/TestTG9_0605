using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.Dashboard
{
	public sealed class ReserveButtonController : ButtonController
	{
		private DashboardStatus dashboardStatus;
		private CreditPlayoffStatusBase creditPlayoffStatus;
		private DashboardController dashboardController;

		public override void Init()
		{
			dashboardStatus = StatusDatabase.DashboardStatus;
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>();
			dashboardController = GameBase.GameInstance.GetPresentationController<DashboardController>();
			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();
			dashboardStatus = null;
			creditPlayoffStatus = null;
			dashboardController = null;
		}

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(BankStatus.BankMeter));
			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.ReserveParameters));
			AddButtonConditionExpressionChanged(typeof(DashboardExpressions), nameof(DashboardExpressions.IsAnyPopupOpen));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameIsIdle));
			AddButtonConditionExpressionChanged(typeof(PopupStatus), nameof(PopupStatus.IsCreditPlayoffOpen));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.GameButtonBehaviours));
			RegisterButtonPressListener(DashboardButtonFunctions.Reserve, OnReserveButtonPressed);
		}

		private bool IsReserveButtonEnabled()
		{
			return StatusDatabase.GameStatus.InActivePlay &&
				StatusDatabase.GameStatus.GameIsIdle &&
				(!DashboardExpressions.IsAnyPopupOpen || PopupStatus.IsReserveOpen) &&
				!PopupStatus.IsCreditPlayoffOpen &&
				StatusDatabase.GameFunctionStatus.GameButtonBehaviours.ReserveButton.IsActive() &&
				(StatusDatabase.BankStatus.BankMeter == Money.Zero ? StatusDatabase.ConfigurationStatus.ReserveParameters.AllowedWithoutCredits : StatusDatabase.ConfigurationStatus.ReserveParameters.AllowedWithCredits);
		}

		protected override void UpdateButtonStates()
		{
			if (!StatusDatabase.GameFunctionStatus.GameButtonBehaviours.ReserveButton.IsVisible() || !StatusDatabase.ConfigurationStatus.ReserveParameters.IsAvailable)
			{
				AddButtonState(DashboardButtonFunctions.Reserve, ButtonState.DisabledHide);
				return;
			}

			var reserveEnabledState = ButtonState.DisabledHide;
			var reserveLightState = LightState.Off;

			if (StatusDatabase.ConfigurationStatus.ReserveParameters.IsAvailable && StatusDatabase.GameFunctionStatus.GameButtonBehaviours.ReserveButton.IsVisible())
			{
				var reserveEnabled = IsReserveButtonEnabled();
				reserveEnabledState = reserveEnabled ? ButtonState.Enabled : ButtonState.DisabledShow;
				reserveLightState = reserveEnabled ? LightState.On : LightState.Off;
			}

			AddButtonState(DashboardButtonFunctions.Reserve, reserveEnabledState, reserveLightState);
		}

		private void OnReserveButtonPressed(ButtonEventData eventData)
		{
			if (StatusDatabase.ConfigurationStatus.ReserveParameters.IsAvailable && IsReserveButtonEnabled())
				dashboardController.ReserveRequest(!PopupStatus.IsReserveOpen);
		}
	}
}