using System;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	public sealed class SoundPlayerSwitchable : SoundPlayerBase
	{
		private ISound[] sounds;
		private int prevIdx = -1;
		private float? volume;

		[SerializeField]
		private int idx = -1;

		[SerializeField]
		private SwitchingBehaviour switchingBehaviour = SwitchingBehaviour.NewCopiesStateFromPrev;

		[SerializeField]
		private SoundId[] soundIds = Array.Empty<SoundId>();

		private ISound PrevSound => GetSound(prevIdx);
		public override ISound Sound => GetSound(idx);

		public int Index
		{
			get => idx;
			set
			{
				if (idx == value)
				{
					return;
				}

				prevIdx = idx;
				idx = value;
				if (PrevSound == null || switchingBehaviour == SwitchingBehaviour.ContinuePrev)
				{
					return;
				}

				if (switchingBehaviour == SwitchingBehaviour.StopPrev)
				{
					PrevSound.Stop();
				}

				//NewCopiesStateFromPrev
				if (PrevSound.IsPlaying)
				{
					PrevSound.Stop();
					Sound?.Play();
				}
				else if (PrevSound.IsPaused)
				{
					PrevSound.Stop();
					Sound?.Play();
					Sound?.Pause();
				}
				else
				{
					Sound?.Stop(); //prev was stopped
				}
			}
		}

		public override float Volume
		{
			get => volume ?? Sound.Volume;
			set
			{
				volume = value;
				foreach (var s in sounds)
					s.Volume = value;
			}
		}

		private enum SwitchingBehaviour
		{
			ContinuePrev, //but dont start new sound on Index switch
			StopPrev, //but dont start new sound on Index switch
			NewCopiesStateFromPrev, //play=play, pause=pause, stopped=stopped, prev gets stopped
		}

		private ISound GetSound(int i)
		{
			sounds ??= new ISound[soundIds.Length];

			if (i < 0 || i >= sounds.Length)
			{
				return null;
			}

			var sound = sounds[i] ?? (sounds[i] = AudioService.CreateSound(soundIds[i]));
			if (sound == null)
			{
				Log.Instance.Fatal($"Could not find sound '{soundIds[i]}' at {gameObject.GetPath()}");
			}
			else if (volume.HasValue)
			{
				sound.Volume = volume.Value;
			}

			return sound;
		}

		private void OnDestroy()
		{
			if (sounds != null)
			{
				foreach (var sound in sounds)
				{
					AudioService.DestroySound(sound);
				}

				sounds = null;
			}
		}
	}
}