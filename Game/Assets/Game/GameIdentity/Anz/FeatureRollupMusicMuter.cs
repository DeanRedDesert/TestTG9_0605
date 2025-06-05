using System;
using Midas.Core;
using Midas.Presentation.Audio;
using Midas.Presentation.Data;
using Midas.Presentation.Sequencing;
using UnityEngine;

namespace Game.GameIdentity.Anz
{
	public sealed class FeatureRollupMusicMuter : MonoBehaviour, ISequencePlayable
	{
		private TimeSpan finishTime;
		private FeatureMusicPlayer currentPlayer;

		#region ISequencePlayable Implementation

		public void StartPlay()
		{
			finishTime = FrameTime.CurrentTime + StatusDatabase.WinPresentationStatus.CountingTime;
			currentPlayer = FeatureMusicPlayer.GetFirstActiveInstance();
			currentPlayer?.MuteBackgroundMusic();
		}

		public void StopPlay(bool reset)
		{
			finishTime = TimeSpan.Zero;
			currentPlayer?.UnmuteBackgroundMusic();
			currentPlayer = null;
		}

		public bool IsPlaying() => currentPlayer != null && finishTime != TimeSpan.Zero && finishTime - FrameTime.CurrentTime > TimeSpan.Zero;

		#endregion
	}
}