using System;
using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using Midas.Presentation.Cabinet;
using static Midas.Ascent.Cabinet.AscentCabinet;

namespace Midas.Ascent.Cabinet
{
	internal sealed class VolumeController : ICabinetController
	{
		private VolumeConfig volumeConfig = new VolumeConfig();
		private PlayerVolumeInfo playerVolumeInfo = new PlayerVolumeInfo(0, false);
		private float gameVolumeAttenuation;

		public event Action<VolumeConfig> VolumeConfigChanged;
		public event Action<float> GameVolumeAttenuationChanged;

		public void SetVolumeConfig(VolumeConfig newVolumeConfig)
		{
			volumeConfig = newVolumeConfig;
			if (playerVolumeInfo.VolumePlayerLevel == volumeConfig.CurrentVolume && playerVolumeInfo.PlayerMuteSelected == volumeConfig.IsMuteSelected)
				return;

			playerVolumeInfo.VolumePlayerLevel = volumeConfig.CurrentVolume;
			playerVolumeInfo.PlayerMuteSelected = volumeConfig.IsMuteSelected;
			CabinetLib.SetPlayerVolumeInfo(playerVolumeInfo);
		}

		public VolumeConfig GetVolumeConfig()
		{
			var playerVolumeSettings = CabinetLib.GetPlayerVolumeSettings();
			playerVolumeInfo = playerVolumeSettings.PlayerVolumeInfo;
			var volumeSelectableInfo = CabinetLib.GetVolumePlayerSelectableInfo();

			volumeConfig = new VolumeConfig(
				playerVolumeInfo.VolumePlayerLevel,
				playerVolumeInfo.PlayerMuteSelected,
				volumeSelectableInfo.PlayerVolumeSelectable,
				volumeSelectableInfo.PlayerMuteSelectable,
				playerVolumeSettings.DefaultVolumePlayerLevel ?? 0.5f,
				volumeConfig.VolumeStepValues,
				CabinetLib.IsMuteAll());

			return volumeConfig;
		}

		public float GetGameVolumeAttenuation()
		{
			return gameVolumeAttenuation = ConvertFoundationVolumeToDecibel(CabinetLib.GetVolume(GroupName.GAME_SOUNDS));
		}

		#region ICabinetController Implementation

		public void Init()
		{
		}

		public void OnBeforeLoadGame()
		{
		}

		public void Resume()
		{
			CabinetLib.SoundVolumeChangedEvent += CabinetLibOnSoundVolumeChangedEvent;
			CabinetLib.SoundVolumeMuteAllStatusChangedEvent += CabinetLibOnSoundVolumeMuteAllStatusChangedEvent;
			CabinetLib.SoundVolumePlayerSelectableStatusChangedEvent += CabinetLibOnSoundVolumePlayerSelectableStatusChangedEvent;
		}

		public void Pause()
		{
			CabinetLib.SoundVolumeChangedEvent -= CabinetLibOnSoundVolumeChangedEvent;
			CabinetLib.SoundVolumeMuteAllStatusChangedEvent -= CabinetLibOnSoundVolumeMuteAllStatusChangedEvent;
			CabinetLib.SoundVolumePlayerSelectableStatusChangedEvent -= CabinetLibOnSoundVolumePlayerSelectableStatusChangedEvent;
		}

		public void OnAfterUnLoadGame()
		{
		}

		public void DeInit()
		{
		}

		#endregion

		#region Private Methods

		private void CabinetLibOnSoundVolumePlayerSelectableStatusChangedEvent(object sender, SoundVolumePlayerSelectableStatusChangedEventArgs e)
		{
			Log.Instance.Info($"SoundVolumePlayerMuteSelectable={e.SoundVolumePlayerMuteSelectable}, SoundVolumePlayerSelectable={e.SoundVolumePlayerSelectable}");

			volumeConfig = new VolumeConfig(volumeConfig.CurrentVolume, volumeConfig.IsMuteSelected,
				e.SoundVolumePlayerSelectable, e.SoundVolumePlayerMuteSelectable,
				volumeConfig.DefaultVolume, volumeConfig.VolumeStepValues,
				volumeConfig.IsMuteAll);

			VolumeConfigChanged?.Invoke(volumeConfig);
		}

		private void CabinetLibOnSoundVolumeMuteAllStatusChangedEvent(object sender, SoundVolumeMuteAllStatusChangedEventArgs e)
		{
			Log.Instance.Info($"Received cabinet mute {e.MuteAll}");

			volumeConfig = new VolumeConfig(volumeConfig.CurrentVolume, volumeConfig.IsMuteSelected,
				volumeConfig.IsPlayerVolumeControlAllowed, volumeConfig.IsPlayerMuteControlAllowed,
				volumeConfig.DefaultVolume, volumeConfig.VolumeStepValues,
				e.MuteAll);

			VolumeConfigChanged?.Invoke(volumeConfig);
		}

		private void CabinetLibOnSoundVolumeChangedEvent(object sender, SoundVolumeChangedEventArgs e)
		{
			Log.Instance.Info($"{e.GroupVolumeSetting.SoundGroup}: {e.GetVolume()}");

			if (e.GroupVolumeSetting.SoundGroup == GroupName.GAME_SOUNDS)
			{
				gameVolumeAttenuation = ConvertFoundationVolumeToDecibel(e.GetVolume());
				GameVolumeAttenuationChanged?.Invoke(gameVolumeAttenuation);
			}
		}

		#endregion

		#region Attenuation Calculation Voodoo

		/// <summary>
		/// </summary>
		/// <param name="sdkVolume">Value between 0 and 1. 0=mute, 1=full volume</param>
		/// <returns>an attenuation value in decibels which can be used with the Unity AudioGroupMixer 0 to -80db</returns>
		private static float ConvertFoundationVolumeToDecibel(float sdkVolume)
		{
			Log.Instance.Debug($"sdkVolume={sdkVolume}");
			sdkVolume = Math.Min(1.0f, Math.Max(0.0f, sdkVolume));

			var t = sdkVolume * 10000.0f;
			var avpVolume = 10000 - (int)Math.Round(t); //10000 is mute and 0 is full volume
			var directXVolume = AvpToDirectSoundVolume(avpVolume); //-10000=mute(-100db) to 0(full volume)
			var db = directXVolume / 100.0f; //https://learn.microsoft.com/en-us/previous-versions/windows/desktop/mt708939(v=vs.85)
			var clampedDb = Math.Min(0f, Math.Max(-80f, db));

			Log.Instance.Debug($"avpVolume={avpVolume}, directXVolume={directXVolume}, db={db}, clampedDb={clampedDb}");
			return clampedDb;
		}

		/// <summary>
		/// Code copied from
		/// AscentFoundation\Q3Series\projects\AVP\DynamiX\DynamiXEngine\SoundSystem\Windows\XP\src\DirectXSoundDevice.cpp
		/// This code seems to be used in the Volume Menu Pages
		/// Convert an AVP sound volume to a DirectSound volume
		/// </summary>
		/// <param name="avpVolume">
		/// This is a value is from 10000 to 0 hundredths of decibels of attenuation where 10000 is mute
		/// and 0 is full volume
		/// </param>
		/// <returns>Value -10000 to 0, where -10000 is mute(or according to DirectX 100db attenuation) and 0 is full volume.</returns>
		private static int AvpToDirectSoundVolume(int avpVolume)
		{
			// The AVP Volume we are provided with is the same one that comes from the machine volume setup
			// menu page.  This is a value is from 10000 to 0 hundredths of decibels of attenuation where
			// 10000 is mute and 0 is full volume.  Similarly, DirectSound uses scale of -10000 to 0,
			// where -10000 is mute and 0 is full volume.  One may be inclined to simply multiply the AVP
			// volume by -1 and use that for the DirectSound volume -- however this is incorrect.  The volume
			// needs to be scaled along a curve, to correspond to non-linear sensitivity of the human ear.
			// Since exponential operations are relatively expensive computationally, and we don't require a ton
			// of accuracy we can approximate the curve and setup, the resultant volume values in a simple lookup
			// table.  On AVP/QNX a similar table can be found in AC97CodecController, a.k.a. the sound driver.
			// The table below was generated through experimentation and is intended to match the final audio
			// output levels of an AVP machine.  Ideally after this conversion, when an AVP and Bronx EGM are
			// side by side and set to the same volume in the menu page the final output is identical.
			var dsoundVolumeConversionTable = new[]
			{
				-10000, -4183, -4124, -4069, -4015, -3963, -3912, -3862, -3812, -3763, //  0 -  9
				-3715, -3667, -3620, -3572, -3526, -3479, -3433, -3387, -3342, -3297, // 10 - 19
				-3252, -3207, -3162, -3118, -3074, -3030, -2986, -2942, -2898, -2855, // 20 - 29
				-2812, -2769, -2726, -2683, -2640, -2598, -2555, -2513, -2471, -2429, // 30 - 39
				-2387, -2345, -2303, -2262, -2220, -2179, -2137, -2096, -2055, -2014, // 40 - 49
				-1972, -1932, -1891, -1850, -1809, -1768, -1728, -1687, -1647, -1607, // 50 - 59
				-1566, -1526, -1486, -1446, -1406, -1366, -1326, -1286, -1246, -1207, // 60 - 69
				-1167, -1127, -1088, -1048, -1009, -969, -930, -891, -852, -812, // 70 - 79
				-773, -734, -695, -656, -617, -578, -539, -501, -462, -423, // 80 - 89
				-384, -346, -307, -269, -230, -192, -153, -115, -77, -38, 0 // 90 - 100
			};

			// I choose to keep the namings, because the code is copied from DirectXSoundDevice.cpp and I want to change as little as possible.
			// ReSharper disable once InconsistentNaming
			const int AVP_MIN_VOLUME = 10000;
			// ReSharper disable once InconsistentNaming
			const int AVP_MAX_VOLUME = 0;
			// ReSharper disable once InconsistentNaming
			const int DSB_VOLUME_MIN = -10000; // Defined in dsound.h
			// ReSharper disable once InconsistentNaming
			const int DSB_VOLUME_MAX = 0; // Defined in dsound.h
			int result;

			if (avpVolume < AVP_MAX_VOLUME)
			{
				result = DSB_VOLUME_MAX;
			}
			else if (avpVolume >= AVP_MIN_VOLUME)
			{
				result = DSB_VOLUME_MIN;
			}
			else
			{
				// Get the correct DirectSound equivalent volume.  Effectively, put the AVP Volume on a
				// range opf 0 to 100 and use that to index into the table.  Ensuring, AVP_volume is between
				// AVP_MAX_VOLUME and AVP_MIN_VOLUME also provides bounds checking for the look up.
				result = dsoundVolumeConversionTable[(AVP_MIN_VOLUME - avpVolume) / 100];
			}

			return result;
		}

		#endregion
	}
}