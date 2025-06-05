using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.StateMachine;
using Midas.Presentation.Audio;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Dashboard
{
	public sealed partial class DashboardController
	{
		private VolumeController volumeController;
		private Coroutine volumeCoroutine;
		private TimeSpan volumeSliderTimeout;

		private void InitVolume()
		{
			volumeController = GameBase.GameInstance.GetPresentationController<VolumeController>();
			volumeCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(VolumeCoroutine(), "Volume");
		}

		private void DeInitVolume()
		{
			volumeCoroutine?.Stop();
			volumeCoroutine = null;
			volumeController = null;
		}

		public void VolumeResetToDefault()
		{
			if (PopupStatus.IsVolumeOpen)
				StatusDatabase.PopupStatus.Close(Popup.Volume);

			var volumeConfig = StatusDatabase.VolumeStatus.VolumeConfig;
			volumeConfig = volumeConfig.WithVolumeAndMute(volumeConfig.DefaultVolume, volumeConfig.IsMuteSelected);
			volumeController.ChangeVolumeConfig(volumeConfig);
		}

		public void VolumeRequest()
		{
			volumeSliderTimeout = FrameTime.CurrentTime + TimeSpan.FromSeconds(5);

			if (dashboardStatus.VolumePopupEnabled && !PopupStatus.IsVolumeOpen)
			{
				StatusDatabase.PopupStatus.Open(Popup.Volume);
				dashboardStatus.VolumeSliderPosition = StatusDatabase.VolumeStatus.VolumeConfig.CurrentVolume;
			}
			else
			{
				var volumeConfig = StatusDatabase.VolumeStatus.VolumeConfig;
				volumeConfig = volumeConfig.WithStep(GetNextLevel(volumeConfig));
				dashboardStatus.VolumeSliderPosition = volumeConfig.CurrentVolume;
				volumeController.ChangeVolumeConfig(volumeConfig);
			}

			int GetNextLevel(VolumeConfig volumeConfig)
			{
				var level = volumeConfig.CurrentVolumeStep;
				if (Mathf.Approximately(volumeConfig.VolumeStepValues[level], volumeConfig.CurrentVolume))
				{
					level = (level + 1) % volumeConfig.VolumeStepValues.Count;
				}

				// check if we are allowed to mute
				if (!volumeConfig.IsPlayerMuteControlAllowed && level == 0)
				{
					level = 1;
				}

				return level;
			}
		}

		private IEnumerator<CoroutineInstruction> VolumeCoroutine()
		{
			for (;;)
			{
				while (!PopupStatus.IsVolumeOpen)
					yield return null;

				var volumeConfig = StatusDatabase.VolumeStatus.VolumeConfig;

				while (PopupStatus.IsVolumeOpen)
				{
					if (volumeConfig.CurrentVolume != dashboardStatus.VolumeSliderPosition)
					{
						volumeConfig = volumeConfig.WithVolumeAndMute(dashboardStatus.VolumeSliderPosition, dashboardStatus.VolumeSliderPosition == 0);
						volumeController.ChangeVolumeConfig(volumeConfig);
						volumeSliderTimeout = FrameTime.CurrentTime + TimeSpan.FromSeconds(5);
					}

					yield return null;

					if (FrameTime.CurrentTime > volumeSliderTimeout || StatusDatabase.GameStatus.GameLogicPaused)
						StatusDatabase.PopupStatus.Close(Popup.Volume);
				}
			}
		}

		private void VolumeButtonCheck(ButtonEventData eventData)
		{
			if (!PopupStatus.IsVolumeOpen)
				return;

			if (!eventData.ButtonFunction.Equals(DashboardButtonFunctions.Volume))
				StatusDatabase.PopupStatus.Close(Popup.Volume);
		}

		private void VolumeOnMoneyIn()
		{
			StatusDatabase.PopupStatus.Close(Popup.Volume);
		}
	}
}