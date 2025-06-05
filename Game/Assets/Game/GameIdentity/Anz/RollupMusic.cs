using System;
using Midas.Core;
using Midas.Presentation.Audio;
using Midas.Presentation.Data;
using Midas.Presentation.Sequencing;
using UnityEngine;

namespace Game.GameIdentity.Anz
{
	public sealed class RollupMusic : MonoBehaviour, ISequencePlayable
	{
		private SoundPlayerBase currentSoundPlayer;
		private TimeSpan finishTime;

		[SerializeField]
		private SoundPlayerBase[] soundPlayers;

		#region ISequencePlayable Implementation

		public void StartPlay()
		{
			var soundIndex = StatusDatabase.WinPresentationStatus.CountingIntensity;
			if (soundIndex >= soundPlayers.Length)
				soundIndex = soundPlayers.Length - 1;

			var newSoundPlayer = soundPlayers[soundIndex];

			if (!ReferenceEquals(currentSoundPlayer, newSoundPlayer))
			{
				currentSoundPlayer?.ImmediateStop();
				newSoundPlayer.ImmediateStop();
				currentSoundPlayer = newSoundPlayer;
			}

			finishTime = FrameTime.UnscaledTime + StatusDatabase.WinPresentationStatus.CountingTime;
			if (currentSoundPlayer.Sound?.IsPaused == true)
				currentSoundPlayer.Sound.Play();
			else
				currentSoundPlayer.Play();
			currentSoundPlayer.Volume = 1f;
		}

		public void StopPlay(bool reset)
		{
			if (currentSoundPlayer == null)
				return;

			if (reset)
			{
				currentSoundPlayer.Pause();
				return;
			}

			// If we are near enough to the end of the rollup time, just let it play out, otherwise kill the sound

			if (finishTime - FrameTime.UnscaledTime > TimeSpan.FromSeconds(0.1))
				currentSoundPlayer.Stop();

			currentSoundPlayer = null;
		}

		public bool IsPlaying()
		{
			return currentSoundPlayer?.Sound?.IsPaused == false && finishTime - FrameTime.UnscaledTime > TimeSpan.Zero;
		}

		#endregion
	}
}