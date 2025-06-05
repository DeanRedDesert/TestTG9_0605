using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Midas.Presentation.Audio
{
	public sealed class SoundSingle : ISound
	{
		private Status status = Status.Stopped;
		private AudioSource assignedAudioSource;
		private FloatInterpolator currentInterpolator;
		private SoundDefinition soundDefinition;
		private float volume = 1.0f;

		private enum Status
		{
			Stopped,
			Playing,
			Paused,
			StopScheduled
		}

		private bool IsStopScheduled => status == Status.StopScheduled;

		public float Volume
		{
			get => volume;
			set
			{
				volume = value;
				if (assignedAudioSource != null)
				{
					assignedAudioSource.volume = EffectiveVolume;
				}
			}
		}

		public bool IsFading => currentInterpolator is { IsRunning: true };

		public bool IsPlaying => status == Status.Playing;

		public bool IsPaused => status == Status.Paused;

		public bool IsLooped => soundDefinition.IsLooped;

		public string Id => soundDefinition.Id;

		public float Length
		{
			get => Clip.length;
		}

		public AudioClip Clip
		{
			get => soundDefinition.Clip;
			set => soundDefinition = soundDefinition.CreateWithChangedClip(value);
		}

		public AudioMixerGroup Group => soundDefinition.Group;

		public float EffectiveVolume => volume * soundDefinition.Volume;

		public SoundSingle(SoundDefinition soundDefinition)
		{
			this.soundDefinition = soundDefinition;
		}

		public void Play()
		{
			StopInternal();

			Log.Instance.Debug($"Starting sound {soundDefinition}");

			AcquireAudioSource();
			assignedAudioSource.Play();
			status = Status.Playing;

			RegisterFrameUpdate();
		}

		public void Pause()
		{
			if (IsPlaying)
			{
				Log.Instance.Debug($"Pause sound {soundDefinition}");

				UnRegisterFrameUpdate();

				assignedAudioSource.Pause();
				currentInterpolator?.Pause();
				status = Status.Paused;
			}
		}

		public void UnPause()
		{
			if (IsPaused)
			{
				status = Status.Playing;
				assignedAudioSource.UnPause();
				currentInterpolator?.UnPause();

				RegisterFrameUpdate();
			}
		}

		public void Stop()
		{
			if (IsPlaying || IsPaused)
			{
				UnRegisterFrameUpdate();
				RegisterFrameLateUpdate();

				status = Status.StopScheduled;
			}
		}

		public void Destroy()
		{
			StopInternal();
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

		private void StopInternal()
		{
			if (IsPlaying || IsPaused || IsStopScheduled)
			{
				Log.Instance.Debug($"Stopping sound {soundDefinition}");
				UnRegisterFrameUpdate();
				UnRegisterFrameLateUpdate();

				status = Status.Stopped;
				// this can only happen if the game object was already destroyed
				// only in case of shutdown in unity editor
				if (assignedAudioSource != null)
				{
					assignedAudioSource.Stop();
				}

				StopInterpolator();
				ReleaseAudioSource();
			}
		}

		private void AcquireAudioSource()
		{
			assignedAudioSource = AudioService.AcquireAudioSource();
			if (assignedAudioSource == null)
			{
				Log.Instance.Error("_assignedAudioSource is NULL.");
			}
			else if (soundDefinition == null)
			{
				Log.Instance.Error("_soundDefinition is NULL.");
			}
			else if (soundDefinition.Clip == null)
			{
				Log.Instance.Error("_soundDefinition._clip is NULL.");
			}
			else
			{
				assignedAudioSource.clip = soundDefinition.Clip;
				assignedAudioSource.loop = soundDefinition.IsLooped;
				assignedAudioSource.outputAudioMixerGroup = soundDefinition.Group == null ? SoundDefinitionsDatabase.Instance.DefaultAudioMixerGroup : soundDefinition.Group;
				assignedAudioSource.volume = EffectiveVolume;
			}
		}

		private void ReleaseAudioSource()
		{
			if (assignedAudioSource != null)
			{
				AudioService.ReleaseAudioSource(assignedAudioSource);
				assignedAudioSource = null;
			}
		}

		private void RegisterFrameUpdate()
		{
			Log.Instance.Debug($"Register frame update {soundDefinition}");
			FrameUpdateService.Update.OnFrameUpdate += FrameUpdate;
		}

		private void UnRegisterFrameUpdate()
		{
			if (IsPlaying)
			{
				Log.Instance.Debug($"UnRegister frame update {soundDefinition}");
				FrameUpdateService.Update.OnFrameUpdate -= FrameUpdate;
			}
		}

		private void FrameUpdate()
		{
			if (IsPlaying &&
				assignedAudioSource != null && !assignedAudioSource.isPlaying)
			{
				ReleaseAudioSource();
				UnRegisterFrameUpdate();

				status = Status.Stopped;
			}
		}

		private void RegisterFrameLateUpdate()
		{
			Log.Instance.Debug($"Register frame late update {soundDefinition}");
			FrameUpdateService.LateUpdate.OnFrameUpdate += FrameLateUpdate;
		}

		private void UnRegisterFrameLateUpdate()
		{
			if (IsStopScheduled)
			{
				Log.Instance.Debug($"UnRegister frame late update {soundDefinition}");
				FrameUpdateService.LateUpdate.OnFrameUpdate -= FrameLateUpdate;
			}
		}

		private void FrameLateUpdate()
		{
			if (IsStopScheduled)
			{
				StopInternal();
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