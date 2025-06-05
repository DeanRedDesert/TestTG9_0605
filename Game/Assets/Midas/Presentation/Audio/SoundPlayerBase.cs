using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Audio
{
	public abstract class SoundPlayerBase : MonoBehaviour
	{
		private enum AutoStart
		{
			DoNotPlayAutomatically,
			PlayOnAwake,
			PlayOnEnable
		}

		private bool isStopping;

		[FormerlySerializedAs("_fadeOutDurationMs")]
		[SerializeField]
		private int fadeOutDurationMs;

		[FormerlySerializedAs("_autoStart")]
		[SerializeField]
		private AutoStart autoStart = AutoStart.DoNotPlayAutomatically;

		public abstract ISound Sound { get; }
		public abstract float Volume { get; set; }

		public virtual bool IsStopping => (Sound?.IsPlaying ?? false) && isStopping;

		public virtual void Play()
		{
			Sound?.Play();
		}

		public virtual void Stop()
		{
			if (Sound is { IsPlaying: true } && fadeOutDurationMs > 0) //Do fade out
			{
				isStopping = true;
				var volume = Volume;
				this.FadeOutAndStop(TimeSpan.FromMilliseconds(fadeOutDurationMs), () => { Volume = volume; });
				return;
			}

			ImmediateStop();
		}

		public virtual void ImmediateStop()
		{
			isStopping = false;
			Sound?.Stop();
		}

		public virtual void Pause()
		{
			Sound?.Pause();
		}

		public virtual void UnPause()
		{
			Sound?.UnPause();
		}

		/// <summary>
		/// <see cref="SoundBaseExt.FadeIn" />
		/// This is just a helper function to start fade in with unity events
		/// </summary>
		/// <param name="timeInMs"></param>
		public void FadeIn(float timeInMs)
		{
			this.FadeIn(TimeSpan.FromMilliseconds(timeInMs));
		}

		/// <summary>
		/// <see cref="SoundBaseExt.PlayAndFadeIn" />
		/// This is just a helper function to start sound and fade in with unity events
		/// </summary>
		/// <param name="timeInMs"></param>
		public void PlayAndFadeIn(float timeInMs)
		{
			this.PlayAndFadeIn(TimeSpan.FromMilliseconds(timeInMs));
		}

		/// <summary>
		/// <see cref="SoundBaseExt.FadeOut" />
		/// This is just a helper function to start fade out with unity events
		/// </summary>
		/// <param name="timeInMs"></param>
		public void FadeOut(float timeInMs)
		{
			this.FadeOut(TimeSpan.FromMilliseconds(timeInMs));
		}

		/// <summary>
		/// <see cref="SoundBaseExt.FadeOutAndStop" />
		/// This is just a helper function to start fade out and stop sound with unity events
		/// </summary>
		/// <param name="timeInMs"></param>
		public void FadeOutAndStop(float timeInMs)
		{
			this.FadeOutAndStop(TimeSpan.FromMilliseconds(timeInMs));
		}

		protected virtual void Awake()
		{
			if (autoStart == AutoStart.PlayOnAwake)
			{
				Play();
			}
		}

		protected virtual void OnEnable()
		{
			if (autoStart == AutoStart.PlayOnEnable)
			{
				Play();
			}
		}
	}
}