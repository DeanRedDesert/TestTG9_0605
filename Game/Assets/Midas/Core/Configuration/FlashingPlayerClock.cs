using System;

namespace Midas.Core.Configuration
{
	public sealed class FlashingPlayerClock
	{
		public bool IsSessionActive { get; private set; }
		public bool FlashPlayerClockEnabled { get; private set; }
		public uint NumberOfFlashesPerSequence { get; private set; }
		public TimeSpan FlashSequenceLength { get; private set; }
		public TimeSpan MinutesBetweenSequences { get; private set; }

		public FlashingPlayerClock(bool isSessionActive, bool flashPlayerClockEnabled, uint numberOfFlashesPerSequence, TimeSpan flashSequenceLength, TimeSpan minutesBetweenSequences)
		{
			IsSessionActive = isSessionActive;
			FlashPlayerClockEnabled = flashPlayerClockEnabled;
			NumberOfFlashesPerSequence = numberOfFlashesPerSequence;
			FlashSequenceLength = flashSequenceLength;
			MinutesBetweenSequences = minutesBetweenSequences;
		}
	}
}