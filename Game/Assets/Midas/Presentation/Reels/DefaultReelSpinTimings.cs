using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	[CreateAssetMenu(menuName = "Midas/Reels/Default Spin Timings")]
	public sealed class DefaultReelSpinTimings : ReelSpinTimings
	{
		#region Inspector Fields

		[Tooltip("The minimum time that a reel can spin in seconds. Stop timings may be adjusted to fit this.")]
		[SerializeField]
		private float minSpinTime;

		[Tooltip("The minimum time between reel stops. Stop timings may be adjusted if the spin time is too short.")]
		[SerializeField]
		private float minStopInterval;

		[Tooltip("The maximum time between reel stops. Stop timings may be adjusted if the spin time is too short.")]
		[SerializeField]
		private float maxStopInterval;

		[Tooltip("The speed that reels should spin in symbols per second.")]
		[SerializeField]
		private float spinSpeed;

		[Tooltip("The windup distance in symbols.")]
		[SerializeField]
		private float windupDistance;

		[Tooltip("The windup time in seconds.")]
		[SerializeField]
		private float windupTime;

		[Tooltip("The overshoot distance in symbols.")]
		[SerializeField]
		private float overshootDistance;

		[Tooltip("The recovery time in seconds.")]
		[SerializeField]
		private float recoveryTime;

		[Tooltip("The framerate that the spin should run at. A value of 0 defaults to whatever framerate the game is running at.")]
		[SerializeField]
		private int spinFrameRate;

		#endregion

		#region Overrides of ReelSpinTimings

		public override IReadOnlyList<TimeSpan> GetStopTimings(TimeSpan spinTime, int groupCount, TimeSpan?[] overrideStopInterval)
		{
			if (groupCount <= 0)
				throw new InvalidOperationException("Trying to get stop timings for 0 columns");

			var stopInterval = maxStopInterval;
			var stopTime = (float)spinTime.TotalSeconds - stopInterval * (groupCount - 1);

			if (stopTime < minSpinTime)
			{
				stopTime = minSpinTime;
				stopInterval = ((float)spinTime.TotalSeconds - stopTime) / (groupCount - 1);

				if (stopInterval < minStopInterval)
					stopInterval = minStopInterval;
			}

			var stopTimes = new TimeSpan[groupCount];

			for (var i = 0; i < groupCount; i++)
			{
				stopTimes[i] = TimeSpan.FromSeconds(stopTime);

				if (i + 1 < groupCount)
				{
					stopTime += (float)(overrideStopInterval?[i + 1]?.TotalSeconds ?? stopInterval);
				}
			}

			return stopTimes;
		}

		public override IReadOnlyList<SpinSettings> GetSpinSettings(int columnCount)
		{
			if (columnCount <= 0)
				throw new InvalidOperationException("Trying to get spin settings for 0 columns");

			var settings = new SpinSettings(spinSpeed, windupDistance, windupTime, overshootDistance, recoveryTime, spinFrameRate);
			return Enumerable.Repeat(settings, columnCount).ToArray();
		}

		public void UpdateTimings(float minSpinTime, float minStopInterval, float maxStopInterval, float spinSpeed, float windupDistance, float windupTime, float overshootDistance, float recoveryTime, int spinFrameRate)
		{
			this.minSpinTime = minSpinTime;
			this.minStopInterval = minStopInterval;
			this.maxStopInterval = maxStopInterval;
			this.spinSpeed = spinSpeed;
			this.windupDistance = windupDistance;
			this.windupTime = windupTime;
			this.overshootDistance = overshootDistance;
			this.recoveryTime = recoveryTime;
			this.spinFrameRate = spinFrameRate;
		}

		#endregion
	}
}