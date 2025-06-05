using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic.Core.Utility;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Logic.Core.DecisionGenerator
{
	public sealed class ScopedDecisionGenerator : IToString
	{
		private readonly IDecisionGenerator decisionGenerator;

		public string Scope { get; }

		public ScopedDecisionGenerator(IDecisionGenerator decisionGenerator, string scope)
		{
			this.decisionGenerator = decisionGenerator;
			Scope = scope;
		}

		/// <summary>
		/// Returns a random boolean decision based on a true and false weight.
		/// </summary>
		/// <param name="trueWeight">The weight of a true result.</param>
		/// <param name="falseWeight">The weight of a false result.</param>
		/// <param name="getAdditionalScope">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>Returns the result of the decision.</returns>
		public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getAdditionalScope) => decisionGenerator.GetDecision(trueWeight, falseWeight, () => GetContext(getAdditionalScope));

		/// <summary>
		/// Returns a random boolean decision based on a true and false weight.
		/// </summary>
		/// <param name="trueWeight">The weight of a true result.</param>
		/// <param name="falseWeight">The weight of a false result.</param>
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>Returns the result of the decision.</returns>
		public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> decisionGenerator.GetDecision(trueWeight, falseWeight, () => GetContext(getAdditionalScopes));

		/// <summary>
		/// Returns a random boolean decision based on a win weight and a total weight.
		/// </summary>
		/// <param name="weights">
		/// A weights object which contains ONE weight and a total weight.
		/// </param>
		/// <param name="getAdditionalScope">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>Returns the result of the decision.</returns>
		/// <exception cref="NotSupportedException">
		/// Thrown if the <see cref="weights"/> has a length that is not ONE or if the totalWeight is less than or equal to the win weight.
		/// </exception>
		public bool GetDecision(IWeights weights, Func<string> getAdditionalScope)
		{
			if (weights.GetLength() == 1)
			{
				var winWeight = weights.GetWeight(0);
				var totalWeight = weights.GetTotalWeight();

				if (winWeight < totalWeight)
				{
					var lossWeight = totalWeight - winWeight;
					return decisionGenerator.GetDecision(winWeight, lossWeight, () => GetContext(getAdditionalScope));
				}
			}

			throw new NotSupportedException($"Decisions are required to have one weight which has a weight less than {weights.GetTotalWeight()}");
		}

		/// <summary>
		/// Returns a random boolean decision based on a win weight and a total weight.
		/// </summary>
		/// <param name="weights">
		/// A weights object which contains ONE weight and a total weight.
		/// </param>
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>Returns the result of the decision.</returns>
		/// <exception cref="NotSupportedException">
		/// Thrown if the <see cref="weights"/> has a length that is not ONE or if the totalWeight is less than or equal to the win weight.
		/// </exception>
		public bool GetDecision(IWeights weights, Func<IReadOnlyList<string>> getAdditionalScopes = null)
		{
			if (weights.GetLength() == 1)
			{
				var winWeight = weights.GetWeight(0);
				var totalWeight = weights.GetTotalWeight();

				if (winWeight < totalWeight)
				{
					var lossWeight = totalWeight - winWeight;
					return decisionGenerator.GetDecision(winWeight, lossWeight, () => GetContext(getAdditionalScopes));
				}
			}

			throw new NotSupportedException($"Decisions are required to have one weight which has a weight less than {weights.GetTotalWeight()}");
		}

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
		/// <param name="getAdditionalScope">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The list of indexes chosen.</returns>
		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getAdditionalScope)
			=> decisionGenerator.ChooseIndexes(indexCount, count, allowDuplicates, getName, () => GetContext(getAdditionalScope));

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
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The list of indexes chosen.</returns>
		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> decisionGenerator.ChooseIndexes(indexCount, count, allowDuplicates, getName, () => GetContext(getAdditionalScopes));

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
		/// <param name="getAdditionalScope">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The list of indexes chosen.</returns>
		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getAdditionalScope)
			=> decisionGenerator.ChooseIndexes(indexCount, count, allowDuplicates, getWeight, totalWeight, getName, () => GetContext(getAdditionalScope));

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
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The list of indexes chosen.</returns>
		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> decisionGenerator.ChooseIndexes(indexCount, count, allowDuplicates, getWeight, totalWeight, getName, () => GetContext(getAdditionalScopes));

		/// <summary>
		/// Randomly chooses a number of indexes in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="weights">The information used to choose the indexes.</param>
		/// <param name="count">The number of indexes to choose.</param>
		/// <param name="allowDuplicates">
		/// If true, then a given index can be returned multiple times.
		/// If false, then a given index can only be returned once.
		/// True is equivalent to 'with replacement'.
		/// False is equivalent to 'without replacement'.
		/// </param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScope">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.s
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The list of indexes chosen.</returns>
		public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getAdditionalScope)
			=> decisionGenerator.ChooseIndexes(weights, count, allowDuplicates, getName, () => GetContext(getAdditionalScope));

		/// <summary>
		/// Randomly chooses a number of indexes in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="weights">The information used to choose the indexes.</param>
		/// <param name="count">The number of indexes to choose.</param>
		/// <param name="allowDuplicates">
		/// If true, then a given index can be returned multiple times.
		/// If false, then a given index can only be returned once.
		/// True is equivalent to 'with replacement'.
		/// False is equivalent to 'without replacement'.
		/// </param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The list of indexes chosen.</returns>
		public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> decisionGenerator.ChooseIndexes(weights, count, allowDuplicates, getName, () => GetContext(getAdditionalScopes));

		/// <summary>
		/// Randomly chooses one index in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="indexCount">The total number of indexes to choose from.</param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScope">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The index chosen.</returns>
		public ulong ChooseOneIndex(ulong indexCount, Func<ulong, string> getName, Func<string> getAdditionalScope)
			=> ChooseIndexes(indexCount, 1, true, getName, getAdditionalScope)[0];

		/// <summary>
		/// Randomly chooses one index in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="indexCount">The total number of indexes to choose from.</param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The index chosen.</returns>
		public ulong ChooseOneIndex(ulong indexCount, Func<ulong, string> getName, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> ChooseIndexes(indexCount, 1, true, getName, getAdditionalScopes)[0];

		/// <summary>
		/// Randomly chooses one index in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="weights">The information used to choose the indexes.</param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScope">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The index chosen.</returns>
		public ulong ChooseOneIndex(IWeights weights, Func<ulong, string> getName, Func<string> getAdditionalScope)
			=> ChooseIndexes(weights, 1, true, getName, getAdditionalScope)[0];

		/// <summary>
		/// Randomly chooses one index in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="weights">The information used to choose the indexes.</param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The index chosen.</returns>
		public ulong ChooseOneIndex(IWeights weights, Func<ulong, string> getName, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> ChooseIndexes(weights, 1, true, getName, getAdditionalScopes)[0];

		/// <summary>
		/// Randomly chooses one index in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="indexCount">The total number of indexes to choose from.</param>
		/// <param name="getWeight">Specifies a callback to get weight of a specific index.</param>
		/// <param name="totalWeight">The total weight to use when working with the indexes. </param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScope">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The index chosen.</returns>
		public ulong ChooseOneIndex(ulong indexCount, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getAdditionalScope)
			=> decisionGenerator.ChooseIndexes(indexCount, 1, true, getWeight, totalWeight, getName, () => GetContext(getAdditionalScope))[0];

		/// <summary>
		/// Randomly chooses one index in the range: 0 to (indexCount - 1).
		/// </summary>
		/// <param name="indexCount">The total number of indexes to choose from.</param>
		/// <param name="getWeight">Specifies a callback to get weight of a specific index.</param>
		/// <param name="totalWeight">The total weight to use when working with the indexes. </param>
		/// <param name="getName">Specifies a callback to get the name of a specific item.</param>
		/// <param name="getAdditionalScopes">
		/// Additional scopes to allow the gaff system to distinguish between decision calls.
		/// Do all string creation inside this func as it will speed up profiling.
		/// </param>
		/// <returns>The index chosen.</returns>
		public ulong ChooseOneIndex(ulong indexCount, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<IReadOnlyList<string>> getAdditionalScopes = null)
			=> decisionGenerator.ChooseIndexes(indexCount, 1, true, getWeight, totalWeight, getName, () => GetContext(getAdditionalScopes))[0];

		private string GetContext(Func<string> getAdditionalScope)
		{
			if (getAdditionalScope == null)
				return Scope;

			var s1 = getAdditionalScope();
			var s2 = getAdditionalScope();

			if (s1 != s2)
				throw new Exception($"Additional scopes should be consistent every time: {s1} vs {s2}");

			return $"{Scope}_{s1}";
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private string GetContext(Func<IReadOnlyList<string>> getAdditionalScopes)
		{
			if (getAdditionalScopes == null)
				return Scope;

			var additionalScopes = getAdditionalScopes();

			if (additionalScopes.Count == 0)
				return Scope;

			var additionalScopes2 = getAdditionalScopes();

			if (!additionalScopes.SequenceEqual(additionalScopes2))
				throw new Exception($"Additional scopes should be consistent every time: {string.Join("_", additionalScopes)} vs {string.Join("_", additionalScopes2)}");

			var sb = new StringBuilder();
			sb.Append(Scope);

			foreach (var scope in additionalScopes)
			{
				if (string.IsNullOrWhiteSpace(scope))
					continue;

				sb.Append('_');
				sb.Append(scope);
			}

			return sb.ToString();
		}

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => $"Scope: {Scope}".ToSuccess();

		#endregion
	}
}