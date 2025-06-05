using System;

namespace Midas.Core
{
	public static class FrameTime
	{
		public static TimeSpan CurrentTime { get; private set; }
		public static TimeSpan DeltaTime { get; private set; }
		public static TimeSpan UnscaledTime { get; private set; }
		public static long FrameNumber { get; private set; }

		public static void SetNextFrame(TimeSpan currentTime, TimeSpan deltaTime, TimeSpan unscaledTime)
		{
			CurrentTime = currentTime;
			DeltaTime = deltaTime;
			UnscaledTime = unscaledTime;
			FrameNumber++;
		}
	}
}