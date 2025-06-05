using System;
using System.Linq;
using Midas.Presentation.Data;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugReelSpinTimingsWindow : DebugWindow
	{
		private bool lastIsHidden;
		private Rect originalSize;

		[SerializeField]
		private GameReelSpinTimings spinTimings;

		public void Update()
		{
			var shouldHide = StatusDatabase.GameStatus.GameIsActive;

			if (lastIsHidden == shouldHide)
				return;

			if (shouldHide)
				originalSize = Resize(new Rect(WindowRect.x, WindowRect.y, WindowRect.width, 80));
			else
				Resize(originalSize);
			lastIsHidden = shouldHide;

			Rect Resize(Rect rect)
			{
				var wr = WindowRect;
				WindowRect = rect;
				return wr;
			}
		}

		private void OnEnable()
		{
			spinTimings.StartTracking();
		}

		private void OnDisable()
		{
			spinTimings.StopTracking();
		}

		protected override void RenderWindowContent()
		{
			if (!spinTimings.IsValid)
				return;

			var stopDeltas = new TimeSpan[spinTimings.StopTimes.Count];
			stopDeltas[0] = spinTimings.StopTimes[0];
			for (var i = 1; i < spinTimings.StopTimes.Count; i++)
				stopDeltas[i] = spinTimings.StopTimes[i] - spinTimings.StopTimes[i - 1];

			GUILayout.Label("                         Current    Min       Max       Av", LabelStyle);
			GUILayout.Label($"Idle Delay Time (ms):   {spinTimings.IdleDelayTime.Current,-10:F0}{spinTimings.IdleDelayTime.Min,-10:F0}{spinTimings.IdleDelayTime.Max,-10:F0}{spinTimings.IdleDelayTime.Average,-10:F0}", LabelStyle);
			GUILayout.Label($"Total Spin Time (ms):   {spinTimings.TotalSpinTime.Current - spinTimings.IdleDelayTime.Current,-10:F0}{spinTimings.TotalSpinTime.Min - spinTimings.IdleDelayTime.Min,-10:F0}{spinTimings.TotalSpinTime.Max - spinTimings.IdleDelayTime.Max,-10:F0}{spinTimings.TotalSpinTime.Average - spinTimings.IdleDelayTime.Average,-10:F0}", LabelStyle);
			GUILayout.Label($"Total Game Time (ms):   {spinTimings.TotalSpinTime.Current,-10:F0}{spinTimings.TotalSpinTime.Min,-10:F0}{spinTimings.TotalSpinTime.Max,-10:F0}{spinTimings.TotalSpinTime.Average,-10:F0}", LabelStyle);

			GUILayout.Label("");

			GUILayout.Label($"Bounce Timing (ms):     {string.Join("", spinTimings.BounceTimes.Select(bt => $"{bt.TotalMilliseconds,6:F0}"))}", LabelStyle);
			GUILayout.Label($"Stop Timing (ms):       {string.Join("", spinTimings.StopTimes.Select(st => $"{st.TotalMilliseconds,6:F0}"))}", LabelStyle);
			GUILayout.Label($"Stop Deltas (ms):       {string.Join("", stopDeltas.Select(sd => $"{sd.TotalMilliseconds,6:F0}"))}", LabelStyle);

			GUILayout.Label("", LabelStyle);
			if (GUILayout.Button("Reset", ButtonStyle))
			{
				spinTimings.IdleDelayTime.Reset();
				spinTimings.TotalSpinTime.Reset();
			}
		}
	}
}