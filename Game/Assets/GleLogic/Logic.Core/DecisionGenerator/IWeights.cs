using System.Collections.Generic;

namespace Logic.Core.DecisionGenerator
{
	/// <summary>
	/// An abstraction of a list of weights that are used by the decision generator to produce random selections and sequences.
	/// </summary>
	public interface IWeights
	{
		/// <summary>
		/// The sum of all item weights in the strip.
		/// </summary>
		ulong GetTotalWeight();

		/// <summary>
		/// Calculate the index that is found at the specified weight.
		/// </summary>
		/// <param name="weight">Valid weight values are 0 to TotalWeight - 1</param>
		/// <returns>An index into the strip at the weight specified</returns>
		ulong GetIndexAtWeight(ulong weight);

		/// <summary>
		/// Calculate the index that is found at the specified weight.
		/// </summary>
		/// <param name="weight">Valid weight values are 0 to TotalWeight - 1</param>
		/// <param name="indexesToSkip">The item indexes that should be skipped when processing accumulated weight</param>
		ulong GetIndexAtWeight(ulong weight, ICollection<ulong> indexesToSkip);

		/// <summary>
		/// The number of weights.
		/// </summary>
		ulong GetLength();

		/// <summary>
		/// The weight at the index specified.
		/// </summary>
		ulong GetWeight(ulong index);
	}
}