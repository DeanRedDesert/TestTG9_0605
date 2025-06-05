using System.Collections;
using System.Collections.Generic;

namespace Logic.Core.Engine
{
	public abstract class StageResults : IReadOnlyList<StageResult>
	{
		private readonly IReadOnlyList<StageResult> results;

		public int Count => results.Count;
		public StageResult this[int index] => results[index];

		protected StageResults(IReadOnlyList<StageResult> results) => this.results = results;

		public IEnumerator<StageResult> GetEnumerator() => results.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}