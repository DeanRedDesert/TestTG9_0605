// Copyright (c) 2022 IGT

#region Usings

using System;
using Midas.Core;
using UnityEngine;

#endregion

namespace Midas.Presentation.Audio
{
	public sealed class FloatInterpolator
	{
		private readonly float fromValue;
		private readonly float toValue;
		private readonly TimeSpan duration;
		private TimeSpan currentTime;
		private readonly Action<float> callBack;
		private bool isPaused;

		public event Action OnDone;

		public bool IsRunning { get; private set; }

		public FloatInterpolator(float from, float to, TimeSpan duration, Action<float> callBack)
		{
			fromValue = from;
			toValue = to;
			this.callBack = callBack;
			this.duration = duration;
			FrameUpdateService.Update.OnFrameUpdate += FrameUpdate;
			IsRunning = true;
			isPaused = false;
			this.callBack.Invoke(fromValue);
		}

		public void Pause()
		{
			isPaused = true;
		}

		public void UnPause()
		{
			isPaused = false;
		}

		public void ForceStop()
		{
			ApplyFinalValue();
		}

		private void ApplyFinalValue()
		{
			if (IsRunning)
			{
				IsRunning = false;
				isPaused = false;
				callBack.Invoke(toValue);
				FrameUpdateService.Update.OnFrameUpdate -= FrameUpdate;
			}
		}

		private void FrameUpdate()
		{
			if (IsRunning && !isPaused)
			{
				if (currentTime < duration)
				{
					var t = (float)(currentTime.TotalSeconds / duration.TotalSeconds);
					callBack.Invoke(Mathf.Lerp(fromValue, toValue, t));
				}
				else
				{
					ApplyFinalValue();
					OnDone?.Invoke();
				}

				currentTime += FrameTime.DeltaTime;
			}
		}
	}
}