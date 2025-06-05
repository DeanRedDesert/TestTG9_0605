using System;
using System.Collections.Generic;

namespace Logic.Core.DecisionGenerator
{
	/// <summary>
	/// Defines an interface for generating decisions based on probabilities.
	/// </summary>
	public interface IDecisionGenerator
	{
		/// <summary>
		/// Returns a random boolean decision based on a true and false weight.
		/// </summary>
		/// <param name="trueWeight">The weight of a true result.</param>
		/// <param name="falseWeight">The weight of a false result.</param>
		/// <param name="getContext">Provides context to the call to request a decision.</param>
		/// <returns>Returns the result of the decision.</returns>
		bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext);

		/// <summary>
		/// Randomly chooses a number of indexes in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="indexCount">The total number of indexes to choose from.</param>
		/// <param name="count">The number of indexes to choose.</param>
		/// <param name="allowDuplicates">
		/// If true, then a given index can be returned multiple times.
		/// If false, then a given index can only be returned once.
		/// True is equivalent to 'with replacement'.
		/// False is equivalent to 'without replacement'.
		/// </param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getContext">Provides context to the call to request a decision.</param>
		/// <returns>The list of indexes chosen.</returns>
		IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext);

		/// <summary>
		/// Randomly chooses a number of indexes in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="indexCount">The total number of indexes to choose from.</param>
		/// <param name="count">The number of indexes to choose.</param>
		/// <param name="allowDuplicates">
		/// If true, then a given index can be returned multiple times.
		/// If false, then a given index can only be returned once.
		/// True is equivalent to 'with replacement'.
		/// False is equivalent to 'without replacement'.
		/// </param>
		/// <param name="getWeight">Specifies a callback to get weight of a specific index.</param>
		/// <param name="totalWeight">The total weight to use when working with the indexes. </param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getContext">Provides context to the call to request a decision.</param>
		/// <returns>The list of indexes chosen.</returns>
		IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext);

		/// <summary>
		/// Randomly chooses a number of indexes in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="weights">The weight information used to choose the indexes.</param>
		/// <param name="count">The number of indexes to choose.</param>
		/// <param name="allowDuplicates">
		/// If true, then a given index can be returned multiple times.
		/// If false, then a given index can only be returned once.
		/// True is equivalent to 'with replacement'.
		/// False is equivalent to 'without replacement'.
		/// </param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getContext">Provides context to the call to request a decision.</param>
		/// <returns>The list of indexes chosen.</returns>
		IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext);

		/// <summary>
		/// Resolves indexes in a non-random way. Used for extra inputs that the user must select from.
		/// </summary>
		/// <param name="indexCount">The number of indexes available.</param>
		/// <param name="minCount">The minimum number that the user must pick.</param>
		/// <param name="maxCount">The maximum number that the user must pick.</param>
		/// <param name="allowDuplicates">Are duplicates allowed?</param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getContext">Provides context to the call to request a decision.</param>
		/// <returns>The list of indexes picked.</returns>
		IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext);
	}
}