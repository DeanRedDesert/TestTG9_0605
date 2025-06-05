using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Game;

namespace Midas.Presentation.Sequencing
{
	public sealed class SequenceFinder : IPresentationController
	{
		public Sequence FindSequence(string sequenceName)
		{
			foreach (var i in Sequences)
			{
				if (i.Name == sequenceName)
					return i;
			}

			Log.Instance.Error($"can not find sequenceName:{sequenceName}");
			return null;
		}

		public IReadOnlyList<SequenceInfo> SequenceInfo
		{
			get => sequenceInfo ??= GenerateSequenceInfo(Sequences).ToArray();
		}

		public IReadOnlyList<Sequence> Sequences => sequences ??= FindSequences();

		public void Init()
		{
			// nothing to be done
		}

		public void DeInit()
		{
			sequences = Array.Empty<Sequence>();
		}

		public void Destroy()
		{
			// nothing to be done
		}

		private static IReadOnlyList<Sequence> FindSequences()
		{
			if (GameBase.GameInstance == null)
				return null;

			var sequences = GameBase.GameInstance.GetInterfaces<ISequenceOwner>().SelectMany(so => so.Sequences).ToArray();

			for (var i = 0; i < sequences.Length - 1; i++)
			{
				var seq = sequences[i];
				for (var j = sequences.Length - 1; j > i; j--)
				{
					if (seq.Name == sequences[j].Name)
						Log.Instance.Fatal($"Found two sequences with the same name {seq.Name}");
				}
			}

			return sequences;
		}

		private static IEnumerable<SequenceInfo> GenerateSequenceInfo(IEnumerable<Sequence> sequences)
		{
			var sequenceInfos = sequences
				.Select(s => new SequenceInfo(s, s.SequenceEventIds))
				.OrderBy(v => v.Sequence.Name);
			return sequenceInfos;
		}

		private IReadOnlyList<Sequence> sequences;
		private IReadOnlyList<SequenceInfo> sequenceInfo;
	}
}