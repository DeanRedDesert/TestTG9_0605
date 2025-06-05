using System;
using Midas.Core;
using Midas.Presentation.Audio;
using Midas.Presentation.Data;
using Midas.Presentation.Sequencing;
using UnityEngine;

namespace Game.GameIdentity.Anz
{
	public sealed class FeatureRollupMusic : MonoBehaviour, ISequencePlayable
	{
		private TimeSpan finishTime;
		private FeatureMusicPlayer currentPlayer;

		#region ISequencePlayable Implementation

		public void StartPlay()
		{
			finishTime = FrameTime.UnscaledTime + StatusDatabase.WinPresentationStatus.CountingTime;
			currentPlayer = FeatureMusicPlayer.GetFirstActiveInstance();
			currentPlayer?.StartWin();
		}

		public void StopPlay(bool reset)
		{
			finishTime = TimeSpan.Zero;
			currentPlayer?.StopWin();
			currentPlayer = null;
		}

		public bool IsPlaying() => currentPlayer != null && finishTime != TimeSpan.Zero && finishTime - FrameTime.UnscaledTime > TimeSpan.Zero;

		#endregion
	}
}