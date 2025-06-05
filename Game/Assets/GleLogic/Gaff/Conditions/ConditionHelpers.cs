using System;
using System.Collections.Generic;
using System.Linq;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;

namespace Gaff.Conditions
{
	public static class ConditionHelpers
	{
		private static readonly Random random = new Random();

		/// <summary>
		/// Check the stage name and cycle id of the next Cycles and return true if it matches <param name="stageName"/>.
		/// </summary>
		public static bool CheckStage(this Cycles cycles, string stageName)
		{
			var cyclesCurrent = cycles.Current;

			return cyclesCurrent != null && cyclesCurrent.Stage == stageName;
		}

		/// <summary>
		/// Check the stage name and cycle id of the next Cycles and return true if it matches <param name="stageName"/> and <param name="cycleId"/>.
		/// </summary>
		public static bool CheckStageAndCycleId(this Cycles cycles, string stageName, string cycleId)
		{
			if (!cycles.CheckStage(stageName))
				return false;

			var cyclesCurrent = cycles.Current;

			if (cyclesCurrent == null)
				return false;

			var cycleIdMatches = string.IsNullOrEmpty(cyclesCurrent.CycleId) && string.IsNullOrEmpty(cycleId) || cyclesCurrent.CycleId == cycleId;

			return cyclesCurrent.Stage == stageName && cycleIdMatches;
		}

		/// <summary>
		/// Split a string into sub strings based on <exception cref="splitOn"/>.  Remove all empty entries and trim each sub string of any leading or trailing whitespace.
		/// </summary>
		public static string[] SplitAndTrim(this string text, string splitOn)
		{
			return text.Split(new[] { splitOn }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
		}

		/// <summary>
		/// Jam two random int's together to make a random ulong.
		/// </summary>
		public static ulong NextULong(ulong count)
		{
			// Generate two random 32-bit integers
			var high = random.Next();
			var low = random.Next();

			// Combine them into a 64-bit unsigned integer
			var result = ((ulong)(uint)high << 32) | (uint)low;
			return result % count;
		}

		/// <summary>
		/// Used to test if at collection of sets can create a new set where one unique symbol is chosen from each set.
		/// The PrizesCondition uses this function to make sure the list of prizes meets each individual prize condition without reusing a particular prize.
		/// </summary>
		public static bool CanFindUniqueSymbols<T>(this IReadOnlyList<HashSet<T>> sets)
		{
			// Build the bipartite graph
			var numSets = sets.Count;
			var allSymbols = new HashSet<T>();
			foreach (var set in sets)
			{
				allSymbols.UnionWith(set);
			}

			var symbolToIndex = new Dictionary<T, int>();

			var index = 0;
			foreach (var symbol in allSymbols)
			{
				symbolToIndex[symbol] = index;
				index++;
			}

			var adj = new List<int>[numSets];
			for (var i = 0; i < numSets; i++)
			{
				adj[i] = new List<int>();
				foreach (var symbol in sets[i])
				{
					adj[i].Add(symbolToIndex[symbol]);
				}
			}

			// Initialize match arrays
			var matchU = Enumerable.Repeat(-1, numSets).ToArray();
			var matchV = Enumerable.Repeat(-1, allSymbols.Count).ToArray();

			// Try to find maximum matching
			var result = 0;
			for (var i = 0; i < numSets; i++)
			{
				var seen = new bool[allSymbols.Count];
				if (FindAugmentingPath(i, matchU, matchV, seen, adj))
				{
					result++;
				}
			}

			return result == numSets;
		}

		/// <summary>
		/// Create a sequence from the start of <param name="initialResultForStep"/> to the end of <param name="sequenceUpToNow"/>.
		/// </summary>
		public static IReadOnlyList<CycleResult> StepCycleResults(this IReadOnlyList<StageGaffResult> sequenceUpToNow, CycleResult initialResultForStep)
		{
			var stepResults = new List<CycleResult>();
			var startStepResults = false;

			foreach (var gaffResult in sequenceUpToNow)
			{
				if (gaffResult.CycleResult == initialResultForStep)
					startStepResults = true;
				if (startStepResults)
					stepResults.Add(gaffResult.CycleResult);
			}

			return stepResults;
		}

		private static bool FindAugmentingPath(int u, IList<int> matchedSymbolsForSets, IList<int> matchedSetsForSymbols, IList<bool> visitedSymbols, IReadOnlyList<List<int>> adjacencyList)
		{
			foreach (var v in adjacencyList[u])
			{
				if (!visitedSymbols[v])
				{
					visitedSymbols[v] = true;
					if (matchedSetsForSymbols[v] == -1 || FindAugmentingPath(matchedSetsForSymbols[v], matchedSymbolsForSets, matchedSetsForSymbols, visitedSymbols, adjacencyList))
					{
						matchedSymbolsForSets[u] = v;
						matchedSetsForSymbols[v] = u;
						return true;
					}
				}
			}

			return false;
		}
	}
}