using System.Collections.Generic;

namespace Midas.Presentation.Sequencing
{
	public sealed class SequenceInfo
	{
		public SequenceInfo(Sequence sequence, IReadOnlyList<(string eventName, int eventId)> events)
		{
			Sequence = sequence;
			Events = events;
		}

		public Sequence Sequence { get; }
		public IReadOnlyList<(string eventName, int eventId)> Events { get; set; }
	}
}