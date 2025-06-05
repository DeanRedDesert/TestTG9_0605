using System;
using JetBrains.Annotations;

namespace Midas.Presentation.Audio
{
	public static class SoundExt
	{
		public static void Fade(this ISound sound, float from, float to, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			sound.Fade(from, to, time, onFadeDone);
		}

		public static void FadeTo(this ISound sound, float to, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			sound.Fade(sound.Volume, to, time, onFadeDone);
		}

		public static void FadeIn(this ISound sound, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			sound.Fade(0, 1.0f, time, onFadeDone);
		}

		public static void PlayAndFadeIn(this ISound sound, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			sound.Play();
			sound.Fade(0, 1.0f, time, onFadeDone);
		}

		public static void FadeOut(this ISound sound, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			sound.Fade(sound.Volume, 0, time, onFadeDone);
		}

		public static void FadeOutAndStop(this ISound sound, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			sound.Fade(sound.Volume, 0, time, () =>
			{
				onFadeDone?.Invoke();
				sound.Stop();
			});
		}
	}
}