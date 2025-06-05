using System;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Midas.Presentation.Audio
{
	public sealed class SoundPlayerRandom : SoundPlayerBase
	{
		private ISound[] soundCache;
		private ISound currentSound;
		private float? volume;

		[SerializeField]
		private bool stopPreviousPlayedSound;

		[SerializeField]
		private SoundId[] sounds = Array.Empty<SoundId>();

		public override void Play()
		{
			if (stopPreviousPlayedSound)
			{
				currentSound?.Stop();
			}

			currentSound = GetRandomSound();
			base.Play();
		}

		public override float Volume
		{
			get => volume ?? Sound.Volume;
			set
			{
				volume = value;
				foreach (var s in soundCache)
					if (s != null)
						s.Volume = value;
			}
		}

		public override ISound Sound => currentSound ??= GetRandomSound();

		private void OnDestroy()
		{
			if (soundCache != null)
			{
				AudioService.DestroySounds(soundCache);
				soundCache = null;
			}
		}

		private ISound GetRandomSound()
		{
			if (soundCache == null)
			{
				if (sounds.Length == 0)
				{
					Log.Instance.Fatal($"Need at least 1 sound defined in {gameObject.GetPath()}", this);
				}

				soundCache = new ISound[sounds.Length];
			}

			var random = Random.Range(0, sounds.Length);
			var sound = soundCache[random] ?? (soundCache[random] = AudioService.CreateSound(sounds[random]));
			if (sound == null)
			{
				Log.Instance.Fatal($"Could not find sound '{soundCache[random]}' at {gameObject.GetPath()}", this);
			}
			else if (volume.HasValue)
			{
				sound.Volume = volume.Value;
			}

			return sound;
		}
	}
}