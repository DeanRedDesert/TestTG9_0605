using System;
using Midas.Core.General;
using Midas.Presentation.Audio;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using UnityEngine;

namespace Game.GameIdentity.Common
{
	public sealed class VolumeButtonPresentation : SimpleAnimatorButtonPresentation
	{

		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private ISound[] sounds;
		private float lastValue;

		[SerializeField]
		private SoundId[] volumeLevelSounds = Array.Empty<SoundId>();

		protected override void Awake()
		{
			base.Awake();

			sounds = new ISound[volumeLevelSounds.Length];
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			lastValue = StatusDatabase.VolumeStatus.VolumeConfig.CurrentVolume;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

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

		protected override void OnDisable()
		{
			autoUnregisterHelper.UnRegisterAll();

			base.OnDisable();
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

				var soundId = volumeLevelSounds[step];
				if (sounds[step] == null && soundId.IsValid)
					sounds[step] = AudioService.CreateSound(soundId);

				if (sounds[step] != null)
					sounds[step].Play();
			}
		}
	}
}