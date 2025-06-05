using System;
using JetBrains.Annotations;

namespace Midas.Presentation.Audio
{
	public static class SoundBaseExt
	{
		public static void PlayOrContinue(this SoundPlayerBase soundPlayer)
		{
			if (soundPlayer.Sound != null && soundPlayer.Sound.IsPaused)
			{
				soundPlayer.UnPause();
			}
			else
			{
				soundPlayer.Play();
			}
		}

		public static void Fade(this SoundPlayerBase soundPlayer, float from, float to, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			soundPlayer.Sound?.Fade(from, to, time, onFadeDone);
		}

		public static void FadeTo(this SoundPlayerBase soundPlayer, float to, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			soundPlayer.Sound?.FadeTo(to, time, onFadeDone);
		}

		public static void FadeIn(this SoundPlayerBase soundPlayer, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			soundPlayer.Sound?.FadeIn(time, onFadeDone);
		}

		public static void PlayAndFadeIn(this SoundPlayerBase soundPlayer, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			soundPlayer.Play();
			soundPlayer.FadeIn(time, onFadeDone);
		}

		public static void FadeOut(this SoundPlayerBase soundPlayer, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			soundPlayer.Sound?.FadeOut(time, onFadeDone);
		}

		public static void FadeOutAndStop(this SoundPlayerBase soundPlayer, TimeSpan time, [CanBeNull] Action onFadeDone = null)
		{
			soundPlayer.FadeOut(time, () =>
			{
				onFadeDone?.Invoke();
				soundPlayer.ImmediateStop();
			});
		}
	}
}