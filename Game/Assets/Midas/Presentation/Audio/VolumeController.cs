using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	public sealed class VolumeController : IPresentationController
	{
		private AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private bool? mutedByGamePause;

		private bool IsAnythingMuted => StatusDatabase.VolumeStatus.VolumeConfig.IsMuteAll || StatusDatabase.VolumeStatus.VolumeConfig.IsMuteSelected || (mutedByGamePause ?? true);

		public void Init()
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(GameStatus.GameLogicPaused), OnGameLogicPausedChanged);
			CabinetManager.Cabinet.VolumeConfigChanged += OnVolumeConfigChanged;
			CabinetManager.Cabinet.GameVolumeAttenuationChanged += OnGameVolumeAttenuationChanged;

			AudioListener.SetAdjustmentFactorForEGM(1.0f);
			mutedByGamePause = StatusDatabase.GameStatus.GameLogicPaused;
			InitVolumeConfig();
			ApplyGameVolumeAttenuation(CabinetManager.Cabinet.GetGameVolumeAttenuation());
		}

		public void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
		}

		public void Destroy() { }

		public void ChangeVolumeConfig(VolumeConfig volumeConfig)
		{
			StatusDatabase.VolumeStatus.VolumeConfig = volumeConfig;
			ApplyVolumeConfig();
		}

		private void ApplyVolumeConfig()
		{
			Log.Instance.Info(StatusDatabase.VolumeStatus.VolumeConfig);

			if (StatusDatabase.GameStatus.GameMode != FoundationGameMode.History)
				CabinetManager.Cabinet.SetVolumeConfig(StatusDatabase.VolumeStatus.VolumeConfig);

			var newVolume = IsAnythingMuted ? 0.0f : StatusDatabase.VolumeStatus.VolumeConfig.CurrentVolume;
			Log.Instance.Info($"AudioListener.volume={newVolume}");
			AudioListener.volume = newVolume;
		}

		private static void ApplyGameVolumeAttenuation(float attenuation)
		{
			StatusDatabase.VolumeStatus.GameVolumeAttenuation = attenuation;

			if (SoundDefinitionsDatabase.Instance.AudioMixers == null)
				return;

			foreach (var mixer in SoundDefinitionsDatabase.Instance.AudioMixers)
			{
				mixer.SetFloat("MainVolume", attenuation);
			}
		}

		private void OnGameLogicPausedChanged(StatusBlock sender, string propertyname)
		{
			var muted = StatusDatabase.GameStatus.GameLogicPaused;

			if (mutedByGamePause != muted)
			{
				mutedByGamePause = muted;
				ApplyVolumeConfig();
			}
		}

		private void OnVolumeConfigChanged(VolumeConfig volumeConfig)
		{
			StatusDatabase.VolumeStatus.VolumeConfig = volumeConfig;
			ApplyVolumeConfig();
		}

		private static void OnGameVolumeAttenuationChanged(float attenuation)
		{
			ApplyGameVolumeAttenuation(attenuation);
		}

		private void InitVolumeConfig()
		{
			var volumeStepLevels = StatusDatabase.ConfigurationStatus.GameIdentity switch
			{
				GameIdentityType.Anz => new[] { 0.0f, 0.08f, 0.31f, 0.54f, 0.77f, 1f },
				GameIdentityType.AnzHybrid => new[] { 0.0f, 0.08f, 0.31f, 0.54f, 0.77f, 1f },
				GameIdentityType.Global => new[] { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f },
				_ => null
			};

			var volConfig = CabinetManager.Cabinet.GetVolumeConfig().WithStepValues(volumeStepLevels);

			if (StatusDatabase.GameStatus.GameMode == FoundationGameMode.History)
				volConfig = volConfig.WithVolumeAndMute(0, true);
			else if (volConfig.IsPlayerMuteControlAllowed && volConfig.CurrentVolumeStep == 0 && volConfig.VolumeStepValues.Count > 1)
				volConfig = volConfig.WithStep(1);

			StatusDatabase.VolumeStatus.VolumeConfig = volConfig;
			ApplyVolumeConfig();
		}
	}
}