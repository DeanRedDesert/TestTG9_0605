using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator;
using Logic.Core.WinCheck;

namespace Midas.Gle.Logic
{
	public sealed class SceneInitDecisionGenerator : IDecisionGenerator
	{
		#region Implementation of IDecisionGenerator

		public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext) => false;

		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			return allowDuplicates
				? new ulong[count]
				: Enumerable.Range(0, (int)count).Select(i => (ulong)i).ToArray();
		}

		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
		{
			return allowDuplicates
				? new ulong[count]
				: Enumerable.Range(0, (int)count).Select(i => (ulong)i).ToArray();
		}

		public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			return allowDuplicates
				? new ulong[count]
				: Enumerable.Range(0, (int)count).Select(i => (ulong)i).ToArray();
		}

		public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			return allowDuplicates
				? new ulong[minCount]
				: Enumerable.Range(0, (int)minCount).Select(i => (ulong)i).ToArray();
		}

		#endregion
	}
}