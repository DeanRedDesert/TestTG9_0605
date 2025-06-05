using Midas.Core.General;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.Dashboard
{
	public sealed class CashoutConfirmController : IPresentationController
	{
		private readonly AutoUnregisterHelper unregisterHelper = new AutoUnregisterHelper();

		public void Init()
		{
			unregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.BankStatus, nameof(BankStatus.IsCashoutAvailable), OnCashoutAvailableChanged);
			unregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.ServiceConfig), OnServiceConfigChanged);
			unregisterHelper.RegisterButtonStateChangedListener(DashboardButtonFunctions.Cashout, OnCashoutButtonStateChanged);
			unregisterHelper.RegisterButtonEventListener(OnAnyButtonPressed);
		}

		public void DeInit()
		{
			unregisterHelper.UnRegisterAll();
		}

		public void Destroy()
		{
		}

		public bool IsOpen()
		{
			return PopupStatus.IsCashoutConfirmOpen;
		}

		public void Close()
		{
			StatusDatabase.PopupStatus.Close(Popup.CashoutConfirm);
		}

		public void Open()
		{
			if (StatusDatabase.BankStatus.IsCashoutAvailable)
			{
				StatusDatabase.PopupStatus.Open(Popup.CashoutConfirm);
			}
		}

		public void Toggle()
		{
			if (IsOpen())
				Close();
			else
				Open();
		}

		private void OnCashoutAvailableChanged(StatusBlock sender, string propertyname)
		{
			if (IsOpen() && !StatusDatabase.BankStatus.IsCashoutAvailable)
				Close();
		}

		private void OnServiceConfigChanged(StatusBlock sender, string propertyname)
		{
			if (IsOpen() && !StatusDatabase.ConfigurationStatus.ServiceConfig.IsCashOutConfirmationNeeded)
				Close();
		}

		private void OnCashoutButtonStateChanged(ButtonStateData buttonStateData)
		{
			if (IsOpen() && buttonStateData.ButtonState != ButtonState.Enabled)
				Close();
		}

		private void OnAnyButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsOpen() &&
				!buttonEventData.ButtonFunction.Equals(DashboardButtonFunctions.Cashout) &&
				!buttonEventData.ButtonFunction.Equals(DashboardButtonFunctions.CashoutYes) &&
				!buttonEventData.ButtonFunction.Equals(DashboardButtonFunctions.CashoutNo) &&
				!buttonEventData.ButtonFunction.Equals(DashboardButtonFunctions.Service) &&
				!buttonEventData.ButtonFunction.Equals(DashboardButtonFunctions.SimulatedHardwareCashout) &&
				!buttonEventData.HasPhysicalOrigins)
			{
				Close();
			}
		}
	}
}