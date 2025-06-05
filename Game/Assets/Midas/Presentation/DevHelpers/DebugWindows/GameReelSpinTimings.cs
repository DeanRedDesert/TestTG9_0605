using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Midas.Core.Debug;
using Midas.Presentation.Data;
using Midas.Presentation.Reels;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class GameReelSpinTimings : MonoBehaviour
	{
		private Coroutine coroutine;
		public bool IsValid { get; private set; }
		public TimeSpanCollector TotalSpinTime { get; private set; }
		public TimeSpanCollector IdleDelayTime { get; private set; }
		public IReadOnlyList<TimeSpan> BounceTimes { get; private set; }
		public IReadOnlyList<TimeSpan> StopTimes { get; private set; }

		public void StartTracking()
		{
			StopTracking();
			coroutine = StartCoroutine(SpinTiming());
		}

		public void StopTracking()
		{
			if (coroutine != null)
				StopCoroutine(coroutine);
			coroutine = null;
		}

		public void OnDestroy()
		{
			StopTracking();
		}

		private IEnumerator SpinTiming()
		{
			var idleTime = new TimeSpanCollector();
			var gameTime = new TimeSpanCollector();
			while (true)
			{
				var reelContainer = FindObjectsOfType<ReelContainer>().FirstOrDefault(rc => rc.gameObject.activeInHierarchy);
				if (reelContainer == null)
				{
					yield return null;
					continue;
				}

				var stopWatch = new Stopwatch();
				var reelGroups = reelContainer.GetReelGroups();
				var stopTiming = new TimeSpan[reelGroups.Count];
				var bounceTiming = new TimeSpan[reelGroups.Count];

				while (!StatusDatabase.GameStatus.GamePlayRequested)
					yield return null;

				IsValid = false;

				idleTime.Start();
				gameTime.Start();

				stopWatch.Start();

				while (reelContainer.IsIdle)
					yield return null;

				idleTime.Stop();

				var spinStates = new ReelSpinState[reelGroups.Count];
				while (!reelContainer.IsIdle)
				{
					yield return null;

					for (var i = 0; i < reelContainer.Reels.Count; i++)
					{
						switch (spinStates[i])
						{
							case ReelSpinState.Idle:
								if (reelContainer.Reels[i].SpinState == ReelSpinState.Spinning)
									spinStates[i] = ReelSpinState.Spinning;
								break;

							case ReelSpinState.Spinning:
								if (reelContainer.Reels[i].SpinState == ReelSpinState.Overshooting)
								{
									spinStates[i] = ReelSpinState.Overshooting;
									bounceTiming[i] = stopWatch.Elapsed;
								}

								break;
							case ReelSpinState.Overshooting:
								if (reelContainer.Reels[i].SpinState == ReelSpinState.Idle)
								{
									spinStates[i] = ReelSpinState.Idle;
									stopTiming[i] = stopWatch.Elapsed;
								}

								break;
						}
					}
				}

				gameTime.Stop();
				TotalSpinTime = gameTime;
				IdleDelayTime = idleTime;
				BounceTimes = bounceTiming;
				StopTimes = stopTiming;
				IsValid = true;

				while (StatusDatabase.GameStatus.GameIsActive)
					yield return null;
			}
		}
	}
}