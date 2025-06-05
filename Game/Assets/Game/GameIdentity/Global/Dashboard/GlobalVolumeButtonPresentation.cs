using System;
using Midas.Core.General;
using Midas.Presentation.Audio;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using UnityEngine;

namespace Game.GameIdentity.Global.Dashboard
{
	public sealed class GlobalVolumeButtonPresentation : GlobalDashboardButtonPres
	{
		[Serializable]
		public sealed class VolumeLevelInfo
		{
			public Sprite Sprite;
			public SoundId SoundId;
		}

		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private ISound[] sounds;
		private float lastValue;

		[SerializeField]
		private VolumeLevelInfo[] volumeLevelInfo = Array.Empty<VolumeLevelInfo>();

		[SerializeField]
		private SpriteRenderer iconSprite;

		protected override void Awake()
		{
			base.Awake();

			sounds = new ISound[volumeLevelInfo.Length];
		}

		private void OnEnable()
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.VolumeStatus, nameof(VolumeStatus.VolumeConfig), OnVolumeConfigChanged);
			lastValue = StatusDatabase.VolumeStatus.VolumeConfig.CurrentVolume;
			RefreshIcon();
		}

		private void OnDestroy()
		{
			if (Application.isEditor && !Application.isPlaying)
			{
				foreach (var sound in sounds)
					sound?.Destroy();
			}
			else
			{
				foreach (var sound in sounds)
				{
					if (sound != null)
						AudioService.DestroySound(sound);
				}
			}

			sounds = null;
		}

		private void OnDisable()
		{
			autoUnregisterHelper.UnRegisterAll();
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			base.RefreshVisualState(button, buttonStateData);

			if (button.ButtonState == Button.State.DisabledDown || button.ButtonState == Button.State.EnabledDown)
			{
				if (lastValue == StatusDatabase.VolumeStatus.VolumeConfig.CurrentVolume)
					return;

				lastValue = StatusDatabase.VolumeStatus.VolumeConfig.CurrentVolume;
				var step = StatusDatabase.VolumeStatus.VolumeConfig.CurrentVolumeStep;
				if (step >= sounds.Length)
					return;

				var info = volumeLevelInfo[step];
				if (sounds[step] == null && info.SoundId.IsValid)
					sounds[step] = AudioService.CreateSound(info.SoundId);

				if (sounds[step] != null)
					sounds[step].Play();
			}
		}

		private void OnVolumeConfigChanged(StatusBlock sender, string propertyName)
		{
			RefreshIcon();
		}

		private void RefreshIcon()
		{
			var step = StatusDatabase.VolumeStatus.VolumeConfig.CurrentVolumeStep;
			if (step >= volumeLevelInfo.Length)
				return;

			var info = volumeLevelInfo[step];
			iconSprite.sprite = info.Sprite;
		}
	}
}
