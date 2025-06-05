using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Midas.Presentation.Sequencing
{
	public sealed class SimpleSequence : Sequence
	{
		private readonly Enum[] sequenceEventIds;

		public static SimpleSequence Create<T>(string name) where T : Enum
		{
			var values = Enum.GetValues(typeof(T)).Cast<Enum>().ToArray();
			return new SimpleSequence(name, values);
		}

		public SimpleSequence(string name, Enum sequenceEventId)
			: this(name, new[] { sequenceEventId })
		{
		}

		public SimpleSequence(string name, params Enum[] sequenceEventIds)
			: base(name)
		{
			this.sequenceEventIds = sequenceEventIds;
		}

		public override IReadOnlyList<(string eventName, int eventId)> GenerateSequenceEventIds()
		{
			var result = new List<(string eventName, int eventId)>();
			foreach (IConvertible c in sequenceEventIds)
				result.Add((c.ToString(CultureInfo.InvariantCulture), c.ToInt32(null)));

			return result;
		}
	}
}