using Midas.Core;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;
using Midas.Presentation.Stakes;
using Midas.Presentation.WinPresentation;

namespace Midas.Presentation.Info
{
	public sealed class InfoButtonController : ButtonController
	{
		private InfoController infoController;
		private CreditPlayoffStatusBase creditPlayoffStatus;

		private WinCountController winCountController;
		private InterruptController interruptController;

		public override void Init()
		{
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>();
			infoController = GameBase.GameInstance.GetPresentationController<InfoController>();
			winCountController = GameBase.GameInstance.GetPresentationController<WinCountController>();
			interruptController = GameBase.GameInstance.GetPresentationController<InterruptController>();
			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();
			infoController = null;
			creditPlayoffStatus = null;
			winCountController = null;
			interruptController = null;
		}

		protected override void RegisterEvents()
		{
			base.RegisterEvents();
			AddButtonConditionPropertyChanged(StatusDatabase.InfoStatus, nameof(InfoStatus.ActiveMode));

			foreach (var function in InfoButtonFunctions.InfoButtons)
				RegisterButtonPressListener(function, OnInfoPageButtonPressed);

			foreach (var function in InfoButtonFunctions.InfoLobbyButtons)
				RegisterButtonPressListener(function, OnLobbyInfoButtonPressed);

			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameIsActive));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.AutoPlayStatus, nameof(AutoPlayStatus.State));
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(BankStatus.BankMeter));
			AddButtonConditionPropertyChanged(StatusDatabase.InfoStatus, nameof(InfoStatus.ActiveMode));
			AddButtonConditionPropertyChanged(ButtonEventDataQueueStatus, nameof(ButtonEventDataQueueStatus.ButtonFunction));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.GameButtonBehaviours));
			AddButtonConditionExpressionChanged(typeof(DashboardExpressions), nameof(DashboardExpressions.IsAnyPopupOpen));
			AddButtonConditionExpressionChanged(typeof(PopupStatus), nameof(PopupStatus.IsCreditPlayoffOpen));

			interruptController.InterruptableStateChanged += RequestButtonUpdate;
			winCountController.CountingStateChanged += RequestButtonUpdate;
			RegisterButtonPressListener(DashboardButtonFunctions.Info, OnInfoButtonPressed);
		}

		protected override void UnregisterEvents()
		{
			base.UnregisterEvents();
			interruptController.InterruptableStateChanged -= RequestButtonUpdate;
			winCountController.CountingStateChanged -= RequestButtonUpdate;
		}

		protected override void UpdateButtonStates()
		{
			if (!StatusDatabase.GameFunctionStatus.GameButtonBehaviours.InfoButton.IsVisible())
				AddButtonState(DashboardButtonFunctions.Info, ButtonState.DisabledHide);
			else
				AddButtonState(DashboardButtonFunctions.Info, IsInfoButtonEnabled());

			var enabled = CanPressInfoPageButton();

			AddButtonState(InfoButtonFunctions.ExitInfo, enabled);
			AddButtonState(InfoButtonFunctions.StartSession, enabled);
			AddButtonState(InfoButtonFunctions.StopSession, enabled);

			var rulesPage = StatusDatabase.InfoStatus.ActiveMode == InfoMode.Rules;
			AddButtonState(InfoButtonFunctions.NextRulesPage, rulesPage, ButtonState.DisabledHide);
			AddButtonState(InfoButtonFunctions.PreviousRulesPage, rulesPage, ButtonState.DisabledHide);

			if (StatusDatabase.InfoStatus.ActiveMode == InfoMode.Lobby)
			{
				var bn = infoController.GetLobbyButtonNames();
				for (var index = 0; index < InfoButtonFunctions.InfoLobbyButtons.Count; index++)
				{
					var function = InfoButtonFunctions.InfoLobbyButtons[index];
					var lbEnabled = enabled && index < bn.Count;
					AddButtonState(function, lbEnabled, ButtonState.DisabledHide, index < bn.Count ? bn[index] : string.Empty);
				}
			}
		}

		private bool IsInfoButtonEnabled() => StatusDatabase.GameStatus.InActivePlay
			&& !StatusDatabase.GameStatus.GameIsActive
			&& (StatusDatabase.AutoPlayStatus.State == AutoPlayState.Idle || StatusDatabase.AutoPlayStatus.State == AutoPlayState.WaitPlayerConfirm)
			&& (!DashboardExpressions.IsAnyPopupOpen || StatusDatabase.PopupStatus.IsOpen(Popup.Info | Popup.CashoutConfirm | Popup.DenomMenu))
			&& !PopupStatus.IsCreditPlayoffOpen
			&& StatusDatabase.GameFunctionStatus.GameButtonBehaviours.InfoButton.IsActive()
			&& !ButtonEventDataQueueStatus.ButtonFunction.IsPlayButtonFunction()
			&& !StatusDatabase.ConfigurationStatus.MachineConfig.IsShowMode;

		private void OnInfoButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsInfoButtonEnabled())
				infoController.InfoRequest(StatusDatabase.InfoStatus.ActiveMode == InfoMode.None);
			else
				infoController.InfoRequest(false);
		}

		private static bool CanPressInfoPageButton() => StatusDatabase.InfoStatus.ActiveMode != InfoMode.None;

		private void OnInfoPageButtonPressed(ButtonEventData eventData)
		{
			if (StatusDatabase.InfoStatus.ActiveMode == InfoMode.None)
				return;

			if (eventData.ButtonFunction.Equals(InfoButtonFunctions.ExitInfo))
			{
				infoController.InfoRequest(false);
				return;
			}

			if (eventData.ButtonFunction.Equals(InfoButtonFunctions.NextRulesPage))
			{
				infoController.RequestRulesPageChange(true);
				return;
			}

			if (eventData.ButtonFunction.Equals(InfoButtonFunctions.PreviousRulesPage))
			{
				infoController.RequestRulesPageChange(false);
				return;
			}

			if (eventData.ButtonFunction.Equals(InfoButtonFunctions.StartSession))
			{
				infoController.RequestSessionChange(true);
				return;
			}

			if (eventData.ButtonFunction.Equals(InfoButtonFunctions.StopSession))
				infoController.RequestSessionChange(false);
		}

		private void OnLobbyInfoButtonPressed(ButtonEventData eventData)
		{
			if (StatusDatabase.InfoStatus.ActiveMode == InfoMode.None)
				return;

			var name = infoController.GetLobbyButtonNames()[eventData.ButtonFunction.Id - InfoButtonFunctions.InfoLobbyButtons[0].Id];
			infoController.RequestLobbyOption(infoController.InfoModeFromName(name));
		}
	}
}