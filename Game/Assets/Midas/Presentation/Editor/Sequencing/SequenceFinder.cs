using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;

namespace Midas.Presentation.Editor.Sequencing
{
	public static class SequenceFinder
	{
		private static (Sequence sequence, IReadOnlyList<(string eventName, int eventId)>)[] sequenceInfo;

		public static IReadOnlyList<(Sequence sequence, IReadOnlyList<(string eventName, int eventId)>)> SequenceInfo
		{
			get
			{
				FindSequences();
				return sequenceInfo;
			}
		}

		private static void FindSequences()
		{
			if (sequenceInfo != null || GameBase.GameInstance == null)
				return;

			var sequenceOwners = GameBase.GameInstance.GetInterfaces<ISequenceOwner>().ToArray();

			var seq = sequenceOwners.SelectMany(so => so.Sequences).ToArray();

			sequenceInfo = seq
				.Select(s => (s, (IReadOnlyList<(string eventName, int eventId)>)s.GenerateSequenceEventIds()))
				.OrderBy(v => v.Item1.Name)
				.ToArray();
		}
	}
}