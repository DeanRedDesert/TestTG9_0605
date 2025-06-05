using System;
using System.Linq;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	internal sealed class SoundSequence : ISound
	{
		private enum Status
		{
			Stopped,
			Playing,
			Paused
		}

		private Status status = Status.Stopped;
		private readonly SoundSequenceDefinition soundSequenceDefinition;
		private int currentPlayedIndex;
		private FloatInterpolator currentInterpolator;
		private ISound[] sounds;

		public float EffectiveVolume
		{
			get => sounds.Length > 0 ? sounds[0].EffectiveVolume : 0;
		}

		public bool IsFading => sounds.Select(s => s.IsFading).Any();

		public bool IsPlaying => status == Status.Playing;

		public bool IsPaused => status == Status.Paused;

		public string Id => soundSequenceDefinition.id;

		public bool IsLooped => soundSequenceDefinition.looped;

		public float Length
		{
			get => sounds.Select(s => s.Length).Sum();
		}

		public SoundSequence(SoundSequenceDefinition soundSequenceDefinition)
		{
			this.soundSequenceDefinition = soundSequenceDefinition;
			if (Application.isPlaying)
			{
				SetupSounds();
			}
		}

		public void Play()
		{
			Stop();

			Log.Instance.Debug($"Starting sound {soundSequenceDefinition}");

			currentPlayedIndex = 0;
			sounds[currentPlayedIndex].Play();
			status = Status.Playing;

			FrameUpdateService.Update.OnFrameUpdate += FrameUpdate;
		}

		public void Pause()
		{
			if (IsPlaying)
			{
				status = Status.Paused;
				sounds[currentPlayedIndex].Pause();

				FrameUpdateService.Update.OnFrameUpdate -= FrameUpdate;
			}
		}

		public void UnPause()
		{
			if (IsPaused)
			{
				status = Status.Playing;
				sounds[currentPlayedIndex].UnPause();

				FrameUpdateService.Update.OnFrameUpdate += FrameUpdate;
			}
		}

		public void Stop()
		{
			if (IsPlaying || IsPaused)
			{
				Log.Instance.Debug($"Stopping sound {soundSequenceDefinition}");
				if (!IsPaused)
				{
					FrameUpdateService.Update.OnFrameUpdate -= FrameUpdate;
				}

				status = Status.Stopped;
				StopInterpolator();
				sounds[currentPlayedIndex].Stop();
			}
		}

		public void Destroy()
		{
			if (IsPlaying || IsPaused)
			{
				Log.Instance.Debug($"Destroy sound {soundSequenceDefinition}");
				if (!IsPaused)
				{
					FrameUpdateService.Update.OnFrameUpdate -= FrameUpdate;
				}

				status = Status.Stopped;
				StopInterpolator();
			}

			AudioService.DestroySounds(sounds);
			sounds = null;
		}

		public void Fade(float fromVolume, float toVolume, TimeSpan fadeTime, Action onFadeDone)
		{
			if (IsPlaying && !IsPaused)
			{
				StopInterpolator();
				currentInterpolator = new FloatInterpolator(fromVolume, toVolume, fadeTime, v => Volume = v);
				if (onFadeDone != null)
				{
					currentInterpolator.OnDone += onFadeDone;
				}
			}
		}

		public float Volume
		{
			get => sounds.Length > 0 ? sounds[0].Volume : 0;
			set
			{
				foreach (var s in sounds)
					s.Volume = value;
			}
		}

		private void SetupSounds()
		{
			sounds = new ISound[soundSequenceDefinition.soundIdsList.Count];
			for (var index = 0; index < soundSequenceDefinition.soundIdsList.Count; index++)
			{
				var soundId = soundSequenceDefinition.soundIdsList[index];
				sounds[index] = AudioService.CreateSound(soundId);
				if (sounds[index] == null)
				{
					Log.Instance.Fatal($"Sound for soundId='{soundId}' not found");
				}
			}
		}

		private void FrameUpdate()
		{
			if (status == Status.Playing && !sounds[currentPlayedIndex].IsPlaying)
			{
				SwitchToNextSound();
			}
		}

		private void SwitchToNextSound()
		{
			if (soundSequenceDefinition.looped)
			{
				currentPlayedIndex = ++currentPlayedIndex % sounds.Length;
				sounds[currentPlayedIndex].Play();
			}
			else
			{
				currentPlayedIndex++;
				// we are at the end
				if (currentPlayedIndex == sounds.Length)
				{
					status = Status.Stopped;
					currentPlayedIndex = 0;
					FrameUpdateService.Update.OnFrameUpdate -= FrameUpdate;
				}
				else
				{
					sounds[currentPlayedIndex].Play();
				}
			}
		}

		private void StopInterpolator()
		{
			if (currentInterpolator != null)
			{
				currentInterpolator.ForceStop();
				currentInterpolator = null;
			}
		}
	}
}