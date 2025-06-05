using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Presentation.Audio;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.Dashboard
{
	public sealed class VolumeButtonController : ButtonController, IPlayerSessionReset
	{
		private VolumeController volumeController;
		private DashboardController dashboardController;

		public override void Init()
		{
			base.Init();

			volumeController = GameBase.GameInstance.GetPresentationController<VolumeController>();
			dashboardController = GameBase.GameInstance.GetPresentationController<DashboardController>();
		}

		protected override void RegisterEvents()
		{
			RegisterButtonPressListener(DashboardButtonFunctions.Volume, OnVolumePopupButtonPressed);

			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameLogicPaused));
			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(VolumeStatus.VolumeConfig));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.GameButtonBehaviours));
		}

		public override void DeInit()
		{
			base.DeInit();
			dashboardController = null;
		}

		public void ResetForNewPlayerSession(IReadOnlyList<PlayerSessionParameterType> pendingResetParams, IList<PlayerSessionParameterType> resetDoneParams)
		{
			if (!pendingResetParams.Contains(PlayerSessionParameterType.PlayerVolume))
				return;
			resetDoneParams.Add(PlayerSessionParameterType.PlayerVolume);
			dashboardController.VolumeResetToDefault();
		}

		private static bool IsButtonEnabled() =>
			!StatusDatabase.GameStatus.GameLogicPaused &&
			StatusDatabase.VolumeStatus.VolumeConfig.IsPlayerVolumeControlAllowed &&
			!StatusDatabase.VolumeStatus.VolumeConfig.IsMuteAll &&
			StatusDatabase.GameFunctionStatus.GameButtonBehaviours.VolumeButton.IsActive() &&
			StatusDatabase.VolumeStatus.VolumeConfig.VolumeStepValues.Count > 1;

		protected override void UpdateButtonStates()
		{
			if (!StatusDatabase.GameFunctionStatus.GameButtonBehaviours.VolumeButton.IsVisible())
			{
				AddButtonState(DashboardButtonFunctions.Volume, ButtonState.DisabledHide);
				return;
			}

			AddButtonState(DashboardButtonFunctions.Volume, IsButtonEnabled());
		}

		private void OnVolumePopupButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsButtonEnabled())
				dashboardController.VolumeRequest();
		}
	}
}