using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.GameIdentity;
using Midas.Presentation.Info;
using Midas.Presentation.Interruption;
using Midas.Presentation.Stakes;
using Midas.Presentation.WinPresentation;
using static Midas.Presentation.Game.GameBase;

namespace Midas.Presentation.Dashboard
{
	public sealed class CashoutButtonController : ButtonController
	{
		private DashboardStatus dashboardStatus;
		private InfoStatus infoStatus;
		private CashoutConfirmController cashoutConfirmController;
		private WinCountController winCountController;
		private InterruptController interruptController;

		public override void Init()
		{
			dashboardStatus = StatusDatabase.DashboardStatus;
			infoStatus = StatusDatabase.InfoStatus;
			cashoutConfirmController = GameInstance.GetPresentationController<CashoutConfirmController>();
			winCountController = GameInstance.GetPresentationController<WinCountController>();
			interruptController = GameInstance.GetPresentationController<InterruptController>();
			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();
			// AutoPlayStatus = null;
			dashboardStatus = null;
			infoStatus = null;
			cashoutConfirmController = null;
			winCountController = null;
			interruptController = null;
		}

		protected override void RegisterEvents()
		{
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.CurrentGameState));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(BankStatus.BankMeter));
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(BankStatus.IsCashoutAvailable));
			AddButtonConditionPropertyChanged(StatusDatabase.PopupStatus, nameof(PopupStatus.Status));
			AddButtonConditionPropertyChanged(StatusDatabase.AutoPlayStatus, nameof(AutoPlayStatus.State));
			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.ServiceConfig));
			AddButtonConditionExpressionChanged(typeof(PopupStatus), nameof(PopupStatus.IsCashoutConfirmOpen));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.GameButtonBehaviours));
			interruptController.InterruptableStateChanged += RequestButtonUpdate;
			winCountController.CountingStateChanged += RequestButtonUpdate;
			RegisterButtonPressListener(DashboardButtonFunctions.Cashout, OnCashOutButtonPressed);
			RegisterButtonPressListener(DashboardButtonFunctions.CashoutYes, OnCashOutYesButtonPressed);
			RegisterButtonPressListener(DashboardButtonFunctions.CashoutNo, OnCashOutNoButtonPressed);
			RegisterButtonPressListener(DashboardButtonFunctions.SimulatedHardwareCashout, OnSimulatedHardwareCashoutButtonPressed);
		}

		protected override void UnregisterEvents()
		{
			interruptController.InterruptableStateChanged -= RequestButtonUpdate;
			winCountController.CountingStateChanged -= RequestButtonUpdate;
		}

		private bool IsCashoutButtonEnabled()
		{
			var gs = StatusDatabase.GameStatus.CurrentGameState;
			var isInCashOutGameState = gs == GameState.Idle || gs == GameState.OfferGamble;

			return StatusDatabase.GameStatus.InActivePlay &&
				isInCashOutGameState &&
				(StatusDatabase.AutoPlayStatus.State == AutoPlayState.Idle || StatusDatabase.AutoPlayStatus.State == AutoPlayState.WaitPlayerConfirm) &&
				StatusDatabase.BankStatus.IsCashoutAvailable &&
				!PopupStatus.IsInfoOpen &&
				!ButtonEventDataQueueStatus.ButtonFunction.IsPlayButtonFunction() &&
				StatusDatabase.GameFunctionStatus.GameButtonBehaviours.CollectButton.IsActive() &&
				CheckButtonFunction(ButtonEventDataQueueStatus.ButtonFunction) &&
				StatusDatabase.BankStatus.BankMeter > Money.Zero;

			bool CheckButtonFunction(ButtonFunction buttonFunction)
			{
				return !buttonFunction.Equals(StakeButtonFunctions.Play) &&
					!buttonFunction.Equals(AutoPlayButtonFunctions.StartAutoPlay) &&
					!buttonFunction.Equals(AutoPlayButtonFunctions.AutoPlayConfirmNo) &&
					!buttonFunction.Equals(AutoPlayButtonFunctions.AutoPlayConfirmYes);
			}
		}

		protected override void UpdateButtonStates()
		{
			var cashOutEnabled = IsCashoutButtonEnabled();

			if (!StatusDatabase.GameFunctionStatus.GameButtonBehaviours.CollectButton.IsVisible())
			{
				AddButtonState(DashboardButtonFunctions.Cashout, ButtonState.DisabledHide);
				AddButtonState(DashboardButtonFunctions.SimulatedHardwareCashout, ButtonState.DisabledHide);
			}
			else
			{
				AddButtonState(DashboardButtonFunctions.Cashout, cashOutEnabled, PopupStatus.IsCashoutConfirmOpen);
				if (StatusDatabase.ConfigurationStatus.ServiceConfig.IsCashOutButtonSimulationNeeded)
					AddButtonState(DashboardButtonFunctions.SimulatedHardwareCashout, cashOutEnabled, PopupStatus.IsCashoutConfirmOpen);
				else
					AddButtonState(DashboardButtonFunctions.SimulatedHardwareCashout, ButtonState.DisabledHide);
			}

			var confirmButtonEnabled = cashOutEnabled && PopupStatus.IsCashoutConfirmOpen;
			AddButtonState(DashboardButtonFunctions.CashoutYes, confirmButtonEnabled);
			AddButtonState(DashboardButtonFunctions.CashoutNo, confirmButtonEnabled);
		}

		private void OnCashOutButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled())
			{
				if (StatusDatabase.ConfigurationStatus.ServiceConfig.IsCashOutConfirmationNeeded && StatusDatabase.ConfigurationStatus.GameIdentity?.IsGlobalGi() == true)
					ButtonQueueController.Enqueue(buttonEventData, IsCashoutHindered, ExecuteCashoutDialogToggleAction);
				else
					ButtonQueueController.Enqueue(buttonEventData, IsCashoutHindered, ExecuteCashoutAction);

				StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
			}
		}

		private void OnCashOutYesButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled() && cashoutConfirmController.IsOpen())
			{
				ButtonQueueController.Enqueue(buttonEventData, IsCashoutConfirmHindered, ExecuteCashoutConfirmAction);
			}
		}

		private void OnCashOutNoButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled() && cashoutConfirmController.IsOpen())
			{
				ButtonQueueController.Enqueue(buttonEventData, IsCashoutConfirmHindered, ExecuteCashoutCancelAction);
			}
		}

		private void OnSimulatedHardwareCashoutButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled() && StatusDatabase.ConfigurationStatus.ServiceConfig.IsCashOutButtonSimulationNeeded)
			{
				ButtonQueueController.Enqueue(buttonEventData, IsSimulatedHardwareCashoutHindered, ExecuteSimulateHardwareCashOutAction);
			}
		}

		private void ExecuteCashoutAction(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled())
			{
				if (StatusDatabase.ConfigurationStatus.ServiceConfig.IsCashOutConfirmationNeeded && StatusDatabase.ConfigurationStatus.GameIdentity?.IsGlobalGi() == true)
				{
					if (cashoutConfirmController.IsOpen())
						Communication.ToLogicSender.Send(new CashoutMessage());
					else
						cashoutConfirmController.Open();
				}
				else
				{
					Communication.ToLogicSender.Send(new CashoutMessage());
				}
			}
		}

		private void ExecuteCashoutDialogToggleAction(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled() &&
				StatusDatabase.ConfigurationStatus.ServiceConfig.IsCashOutConfirmationNeeded)
			{
				cashoutConfirmController.Toggle();
			}
		}

		private void ExecuteCashoutConfirmAction(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled() && cashoutConfirmController.IsOpen())
			{
				Communication.ToLogicSender.Send(new CashoutMessage());
				cashoutConfirmController.Close();
			}
		}

		private void ExecuteCashoutCancelAction(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled() && cashoutConfirmController.IsOpen())
				cashoutConfirmController.Close();
		}

		private void ExecuteSimulateHardwareCashOutAction(ButtonEventData buttonEventData)
		{
			if (IsCashoutButtonEnabled() && StatusDatabase.ConfigurationStatus.ServiceConfig.IsCashOutButtonSimulationNeeded)
			{
				// The foundation will treat this as though a physical cash out button was pressed.
				CabinetManager.Cabinet.RequestCashout();
			}
		}

		private ButtonFunctionHinderedReasons IsCashoutHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseGameCycleActive);
		}

		private ButtonFunctionHinderedReasons IsCashoutConfirmHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.None);
		}

		private ButtonFunctionHinderedReasons IsSimulatedHardwareCashoutHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.None);
		}
	}
}