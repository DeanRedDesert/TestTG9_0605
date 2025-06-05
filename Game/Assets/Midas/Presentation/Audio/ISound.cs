using System;

namespace Midas.Presentation.Audio
{
	public interface ISound
	{
		/// <summary>
		/// Starts the sound from the beginning
		/// </summary>
		void Play();

		/// <summary>
		/// If a sound is playing it can be paused with this function
		/// To continue playing at this position call <see cref="UnPause" />
		/// </summary>
		void Pause();

		/// <summary>
		/// Starts playing a sound where it was paused
		/// If the sound is not paused nothing happens
		/// </summary>
		void UnPause();

		/// <summary>
		///     Independent if a sound is playing or paused it is stopped
		/// </summary>
		void Stop();

		/// <summary>
		/// Destroys internal allocated resources like AudioSource
		/// It is mandatory to call this function before releasing the last reference to the sound instance
		/// If the sound is created <see cref="AudioService.CreateSound" /> use <see cref="AudioService.DestroySound" /> which
		/// then calls this Destroy method/>
		/// </summary>
		void Destroy();

		/// <summary>
		/// Fade the sound from -> to specified volume.
		/// The sound must already be in the playing state that the fading starts
		/// </summary>
		/// <param name="fromVolume">Start value for fading <see cref="Volume" /></param>
		/// <param name="toVolume">End value for fading <see cref="Volume" /></param>
		/// <param name="fadeTime">Time how long it should fade</param>
		/// <param name="onFadeDone">Callback when fading is done</param>
		void Fade(float fromVolume, float toVolume, TimeSpan fadeTime, Action onFadeDone = null);

		/// <summary>
		/// Current volume value (0-1)
		/// </summary>
		float Volume { get; set; }

		/// <summary>
		/// Effective volume value (0-1): SoundDefintion.Volume*Voume
		/// </summary>
		float EffectiveVolume { get; }

		bool IsFading { get; }
		bool IsPlaying { get; }
		bool IsPaused { get; }
		bool IsLooped { get; }

		/// <summary>
		///     Unique ID of this sound object
		/// </summary>
		string Id { get; }

		/// <summary>
		///     <para>The length of the audio clip in seconds. (Read Only)</para>
		/// </summary>
		public float Length { get; }
	}
}