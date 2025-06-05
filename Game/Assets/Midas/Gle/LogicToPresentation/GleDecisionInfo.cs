using System;
using System.Collections;
using System.Collections.Generic;
using Logic.Core.DecisionGenerator.Decisions;

namespace Midas.Gle.LogicToPresentation
{
	public sealed class GleDecisionInfo
	{
		private sealed class PickDecisionList : IReadOnlyList<string>
		{
			private readonly Func<ulong, string> getOption;
			private readonly ulong optionCount;

			public PickDecisionList(Func<ulong, string> getOption, ulong optionCount)
			{
				this.getOption = getOption;
				this.optionCount = optionCount;
			}

			public IEnumerator<string> GetEnumerator()
			{
				for (ulong i = 0; i < optionCount; i++)
					yield return getOption(i);
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public int Count => (int)optionCount;
			public string this[int index] => getOption((ulong)index);
		}

		private readonly PickIndexesDecision pickIndexesDecision;

		public string Name => pickIndexesDecision.Context;
		public IReadOnlyList<string> Options { get; }
		public int MinSelections => (int)pickIndexesDecision.MinCount;
		public int MaxSelections => (int)pickIndexesDecision.MaxCount;
		public bool AllowDuplicates => pickIndexesDecision.AllowDuplicates;

		public GleDecisionInfo(PickIndexesDecision pickDecision)
		{
			Options = new PickDecisionList(pickDecision.GetName, pickDecision.IndexCount);
			pickIndexesDecision = pickDecision;
		}

		public override string ToString() => $"{Name}, {(MinSelections == MaxSelections ? MinSelections.ToString() : $"[{MinSelections}-{MaxSelections}]")} / {Options.Count} options{(AllowDuplicates ? ", duplicates allowed" : "")}";
	}
}