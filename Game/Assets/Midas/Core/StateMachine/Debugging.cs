// Copyright (c) 2021 IGT

namespace Midas.Core.StateMachine
{
	public static class Debugging
	{
		public static BreakMode BreakMode { get; set; } = BreakMode.Off;
		public static BreakPosition BreakPosition { get; set; }
		public static bool BreakTriggered { get; internal set; }
		public static bool NextDebugStep { get; set; }

		public static bool CheckDebugBreak(BreakPosition breakPosition, State state)
		{
			if (BreakPosition == breakPosition && state.DebugBreak)
			{
				BreakTriggered = true;
				if (!NextDebugStep)
				{
					return false;
				}

				NextDebugStep = false;
				BreakTriggered = false;
			}

			return true;
		}
	}
}