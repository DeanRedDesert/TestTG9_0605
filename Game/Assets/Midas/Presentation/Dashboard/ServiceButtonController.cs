using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.Dashboard
{
	public sealed class ServiceButtonController : ButtonController
	{
		protected override void RegisterEvents()
		{
			RegisterButtonPressListener(DashboardButtonFunctions.Service, OnServiceButtonPressed);

			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameIsActive));
			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.ServiceConfig));
		}

		protected override void UpdateButtonStates()
		{
			if (!StatusDatabase.ConfigurationStatus.ServiceConfig.IsServiceButtonSimulationNeeded)
			{
				AddButtonState(DashboardButtonFunctions.Service, ButtonState.DisabledHide);
				return;
			}

			var enabled = IsButtonEnabled();
			var serviceActive = enabled && StatusDatabase.ConfigurationStatus.ServiceConfig.IsServiceRequestActive;

			AddButtonState(DashboardButtonFunctions.Service, enabled, serviceActive);
		}

		private static bool IsButtonEnabled() => StatusDatabase.GameStatus.InActivePlay && StatusDatabase.ConfigurationStatus.ServiceConfig.IsServiceButtonSimulationNeeded;

		private static void OnServiceButtonPressed(ButtonEventData buttonEventData)
		{
			if (!IsButtonEnabled())
				return;

			CabinetManager.Cabinet.RequestService();
		}
	}
}