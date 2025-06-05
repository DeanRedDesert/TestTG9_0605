using System;

namespace Midas.Core.StateMachine
{
	public readonly struct StateStep
	{
		public readonly long FrameNumber;
		public readonly TimeSpan Time;
		public readonly State State;

		public StateStep(long frameNumber, TimeSpan time, State state)
		{
			FrameNumber = frameNumber;
			Time = time;
			State = state;
		}
	}
}