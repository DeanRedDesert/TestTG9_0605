using System;
using System.Collections.Generic;
using System.Globalization;
using Midas.CreditPlayoff.Presentation;
using Midas.Presentation.Data;

namespace Midas.Presentation.Sequencing
{
	public sealed class CreditPlayOffResultSequence : Sequence
	{
		private readonly Enum[] sequenceEventIds;
		private CreditPlayoffStatus creditPlayoffStatus;

		public CreditPlayOffResultSequence(string name, Enum sequenceEventId)
			: this(name, new[] { sequenceEventId })
		{
		}

		public CreditPlayOffResultSequence(string name, params Enum[] sequenceEventIds)
			: base(name)
		{
			this.sequenceEventIds = sequenceEventIds;
		}

		public override void Init()
		{
			base.Init();
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatus>();
		}

		public override IReadOnlyList<(string eventName, int eventId)> GenerateSequenceEventIds()
		{
			var result = new List<(string eventName, int eventId)>();
			foreach (IConvertible c in sequenceEventIds)
				result.Add((c.ToString(CultureInfo.InvariantCulture), c.ToInt32(null)));

			return result;
		}

		protected override SequenceEventArgs GetSequenceEventParameter(int sequenceEventId)
		{
			return new SequenceEventArgs(sequenceEventId, -1, creditPlayoffStatus.Result / (double)creditPlayoffStatus.TotalWeight);
		}
	}
}