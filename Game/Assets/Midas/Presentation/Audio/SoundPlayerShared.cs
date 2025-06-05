// Copyright (c) 2022 IGT

#region Usings

using UnityEngine;

#endregion

namespace Midas.Presentation.Audio
{
	public sealed class SoundPlayerShared : SoundPlayerBase
	{
		private SharedSounds.Token token;

		[SerializeField]
		private SoundId soundId = new SoundId();

		public override ISound Sound => Token.Sound;

		public override float Volume
		{
			get => Sound.Volume;
			set => Sound.Volume = value;
		}

		public SoundId SoundId => soundId;

		private SharedSounds.Token Token => token ??= SharedSounds.Acquire(soundId);

		public override void Play()
		{
			if (!Sound.IsPlaying)
			{
				base.Play();
			}
		}

		private void OnDestroy()
		{
			if (token != null)
			{
				SharedSounds.Release(token);
				token = null;
			}
		}
	}
}