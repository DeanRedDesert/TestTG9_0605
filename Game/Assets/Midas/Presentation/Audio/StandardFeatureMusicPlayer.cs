using Midas.Presentation.Data;
using Midas.Presentation.StageHandling;
using UnityEngine;

namespace Midas.Presentation.Audio
{
	public sealed class StandardFeatureMusicPlayer : FeatureMusicPlayer
	{
		private SharedSounds.Token backgroundToken;
		private SharedSounds.Token backgroundTerminatorToken;
		private SharedSounds.Token winToken;
		private SharedSounds.Token winTerminatorToken;

		[SerializeField]
		private Stage stage;

		[SerializeField]
		private SoundId backgroundSoundId = new SoundId();

		[SerializeField]
		private SoundId backgroundTerminatorSoundId = new SoundId();

		[SerializeField]
		private SoundId winSoundId = new SoundId();

		[SerializeField]
		private SoundId winTerminatorSoundId = new SoundId();

		private SharedSounds.Token BackgroundSoundToken => backgroundToken ??= GetToken(backgroundSoundId);
		private SharedSounds.Token BackgroundTerminatorSoundToken => backgroundTerminatorToken ??= GetToken(backgroundTerminatorSoundId);
		private SharedSounds.Token WinSoundToken => winToken ??= GetToken(winSoundId);
		private SharedSounds.Token WinSoundTerminatorToken => winTerminatorToken ??= GetToken(winTerminatorSoundId);

		private static SharedSounds.Token GetToken(SoundId soundId) => !(soundId is { IsValid: true }) ? null : SharedSounds.Acquire(soundId);

		public override bool IsEligibleForWinInteractivity() => StatusDatabase.StageStatus.CurrentStage == stage;

		public override void Play()
		{
			if (BackgroundSoundToken?.Sound is { IsPlaying: false })
				BackgroundSoundToken.Sound?.Play();

			if (WinSoundToken?.Sound is { IsPlaying: false })
			{
				WinSoundToken.Sound.Play();
				WinSoundToken.Sound.Volume = 0;
			}
		}

		public override void Stop()
		{
			if (BackgroundSoundToken?.Sound is { IsPlaying: true })
			{
				BackgroundTerminatorSoundToken?.Sound.Play();
				BackgroundSoundToken.Sound?.Stop();
			}

			if (WinSoundToken?.Sound is { IsPlaying: true })
			{
				WinSoundToken.Sound.Stop();
				WinSoundToken.Sound.Volume = 0;
			}
		}

		public override void StartWin()
		{
			if (WinSoundToken?.Sound is { IsPlaying: true })
				WinSoundToken.Sound.Volume = 1;
		}

		public override void StopWin()
		{
			if (WinSoundToken?.Sound is { IsPlaying: true })
			{
				WinSoundTerminatorToken?.Sound?.Play();
				WinSoundToken.Sound.Volume = 0;
			}
		}

		public override void MuteBackgroundMusic()
		{
			if (AllowsMutingBackgroundMusicDuringWins && BackgroundSoundToken?.Sound is { IsPlaying: true })
				BackgroundSoundToken.Sound.Volume = 0;
		}

		public override void UnmuteBackgroundMusic()
		{
			if (AllowsMutingBackgroundMusicDuringWins && BackgroundSoundToken?.Sound is { IsPlaying: true })
				BackgroundSoundToken.Sound.Volume = 1;
		}

		public override void ImmediateStop()
		{
			if (BackgroundSoundToken?.Sound is { IsPlaying: true })
				BackgroundSoundToken.Sound?.Stop();

			if (WinSoundToken?.Sound is { IsPlaying: true })
			{
				WinSoundToken.Sound.Stop();
				WinSoundToken.Sound.Volume = 0;
			}
		}

		private void OnEnable() => Play();

		private void OnDisable() => ImmediateStop();

		private void OnDestroy()
		{
			if (backgroundToken != null)
			{
				SharedSounds.Release(backgroundToken);
				backgroundToken = null;
			}

			if (backgroundTerminatorToken != null)
			{
				SharedSounds.Release(backgroundTerminatorToken);
				backgroundTerminatorToken = null;
			}

			if (winToken != null)
			{
				SharedSounds.Release(winToken);
				winToken = null;
			}

			if (winTerminatorToken != null)
			{
				SharedSounds.Release(winTerminatorToken);
				winTerminatorToken = null;
			}
		}

#if UNITY_EDITOR
		public void ConfigureForMakeGame(Stage value)
		{
			stage = value;
		}
#endif
	}
}