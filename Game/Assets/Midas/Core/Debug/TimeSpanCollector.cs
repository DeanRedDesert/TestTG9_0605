using System;
using System.Diagnostics;

namespace Midas.Core.Debug
{
	public sealed class TimeSpanCollector
	{
		private bool hasLaps;
		private double totalTime;
		private ulong samples;

		private readonly Stopwatch stopwatch = new Stopwatch();

		public double Current { get; private set; }

		public double Min { get; private set; } = double.MaxValue;

		public double Max { get; private set; }

		public double Average { get { return samples == 0 ? 0 : totalTime / samples; } }

		public void Start()
		{
			stopwatch.Restart();
		}

		public void Lap()
		{
			if (UpdateCurrentTimeSpan())
			{
				hasLaps = true;
			}
		}

		public void Stop()
		{
			UpdateCurrentTimeSpan();
			UpdateTimeSpans();
		}

		public void Reset()
		{
			Current = 0;
			totalTime = 0;
			samples = 0;
			Min = double.MaxValue;
			Max = 0;
			hasLaps = false;
		}

		public override string ToString()
		{
			return ToString(true);
		}

		public string ToString(bool withPropertyText)
		{
			return withPropertyText
				? $"CUR:{Current,8:0.00}, MIN:{Min,8:0.00}, MAX:{Max,9:0.00}, AVG:{Average,8:0.00}"
				: $"{Current,8:0.000} {Min,8:0.000} {Max,9:0.000} {Average,8:0.000}";
		}

		private bool UpdateCurrentTimeSpan()
		{
			if (!stopwatch.IsRunning)
				return false;

			stopwatch.Stop();
			Current = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond + (hasLaps ? Current : 0d);
			return true;
		}

		private void UpdateTimeSpans()
		{
			if (Current > Max)
				Max = Current;

			if (Current < Min)
				Min = Current;

			totalTime += Current;
			samples++;

			hasLaps = false;
		}
	}
}