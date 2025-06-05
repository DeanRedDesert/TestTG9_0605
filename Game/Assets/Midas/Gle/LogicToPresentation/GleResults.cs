using System.Collections;
using System.Collections.Generic;
using Midas.Core;

namespace Midas.Gle.LogicToPresentation
{
	public sealed class GleResults : IReadOnlyList<GleResult>
	{
		private readonly IReadOnlyList<GleResult> results;

		public IStakeCombination StakeCombination { get; }

		public GleResults(IReadOnlyList<GleResult> results, IStakeCombination stakeCombination)
		{
			this.results = results;
			StakeCombination = stakeCombination;
		}

		#region IReadOnlyList<GleResult> Implementation

		public IEnumerator<GleResult> GetEnumerator() => results.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => results.Count;
		public GleResult this[int index] => results[index];

		#endregion
	}
}