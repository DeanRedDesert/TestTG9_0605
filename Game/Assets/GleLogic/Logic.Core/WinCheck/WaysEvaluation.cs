using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// Evaluator helper for multiway prizes
	/// </summary>
	public static class WaysEvaluation
	{
		public static void ProcessPayLeft(SymbolWindowResult symbolWindow, Patterns patterns, MaskPrize prize, ICollection<CellPrizeResult> winningPatterns)
		{
			var prizeSymbols = symbolWindow.SymbolMasks.GetCombinedSymbolMask(prize.Symbols);

			if (prizeSymbols.IsEmpty)
				return;

			for (var j = 0; j < patterns.ClusterPatterns.Count; j++)
			{
				var clusterPattern = patterns.ClusterPatterns[j];
				var hitCount = 0;
				var winMaskCluster = new ReadOnlyMask[clusterPattern.Positions.Length];

				for (var i = 0; i < clusterPattern.Positions.Length; i++)
				{
					if (prizeSymbols.AndIsEmpty(clusterPattern.PositionsMask[i], out var andResult))
						break;

					winMaskCluster[hitCount] = andResult;
					hitCount++;
				}

				if (hitCount >= prize.StartingHitCount)
					GeneratePrizes(prize, patterns.GetSourcePattern(j), winMaskCluster, hitCount, symbolWindow, winningPatterns);
			}
		}

		public static void ProcessPayAny(SymbolWindowResult symbolWindow, Patterns patterns, MaskPrize prize, ICollection<CellPrizeResult> winningPatterns)
		{
			var prizeSymbols = symbolWindow.SymbolMasks.GetCombinedSymbolMask(prize.Symbols);

			if (prizeSymbols.IsEmpty)
				return;

			for (var j = 0; j < patterns.ClusterPatterns.Count; j++)
			{
				var clusterPattern = patterns.ClusterPatterns[j];
				var hitCount = 0;
				var winMaskCluster = new ReadOnlyMask[clusterPattern.Positions.Length];

				for (var i = 0; i < clusterPattern.Positions.Length; i++)
				{
					if (prizeSymbols.AndIsEmpty(clusterPattern.PositionsMask[i], out var andResult))
						continue;

					winMaskCluster[hitCount] = andResult;
					hitCount++;
				}

				if (hitCount >= prize.StartingHitCount)
					GeneratePrizes(prize, patterns.GetSourcePattern(j), winMaskCluster, hitCount, symbolWindow, winningPatterns);
			}
		}

		/// <summary>
		/// Generates CellPrizeResult for each prize to insert into the outcome data.
		/// </summary>
		private static void GeneratePrizes(MaskPrize prize, Pattern pattern, IReadOnlyList<ReadOnlyMask> winningClusters, int hitCount, SymbolWindowResult symbolWindowResult, ICollection<CellPrizeResult> cellPrizes)
		{
			var prizePays = prize.PrizePays;

			if (prizePays == null)
				return;

			// There should only be one prize pay whose count matches the winning clusters count.
			int? winningPayIndex = null;

			for (var payIndex = 0; payIndex < prizePays.Count; payIndex++)
			{
				if (prize.StartingHitCount + payIndex == hitCount)
				{
					winningPayIndex = payIndex;
					break;
				}
			}

			if (winningPayIndex == null)
				return;

			var patterns = GetAllUniquePatterns(symbolWindowResult.SymbolWindowStructure.Cells, winningClusters, hitCount);

			var reqSymIndexes = new List<int>();

			if (prize.RequiredSymbolCounts != null)
			{
				for (var i = 0; i < prize.Symbols.Count; i++)
				{
					if (prize.RequiredSymbolCounts[i] > 0)
						reqSymIndexes.Add(i);
				}
			}

			foreach (var p in patterns)
			{
				var p1 = p;
				// Final check to exclude the prize if it doesn't have enough required symbols.
				if (reqSymIndexes.Count > 0)
					if (!reqSymIndexes.All(si => p1.Count(c => symbolWindowResult.GetSymbolIndexAt(c) == prize.Symbols[si]) >= prize.RequiredSymbolCounts[si])) //TODO: REMOVE LINQ...not as important because very few game use required symbol counts
						continue;

				cellPrizes.Add(new CellPrizeResult(prize.Name, prize.StartingHitCount + winningPayIndex.Value, prize.PrizePays[winningPayIndex.Value], pattern, p));
			}
		}

		/// <summary>
		/// Permutes the clusters into all individual patterns so we can test for wins on them.
		/// </summary>
		/// <remarks>
		/// E.g. inputClusters of (a, b, c) (d, e) and (f, g, h)
		/// will permute into output patterns of
		/// (a, d, f)
		/// (a, d, g)
		/// (a, d, h)
		/// (a, e, f)
		/// (a, e, g)
		/// (a, e, h)
		/// (b, d, f)
		/// (b, d, g)
		/// (b, d, h)
		/// (b, e, f)
		/// (b, e, g)
		/// (b, e, h)
		/// (c, d, f)
		/// (c, d, g)
		/// (c, d, h)
		/// (c, e, f)
		/// (c, e, g)
		/// (c, e, h)
		/// </remarks>
		private static Cell[][] GetAllUniquePatterns(IReadOnlyList<Cell> cells, IReadOnlyList<ReadOnlyMask> winningClusters, int hitCount)
		{
			// Get the number of cells in each cluster and combine them to generate the total patterns.

			var totalPatterns = 1;
			var counts = new int[hitCount];

			for (var i = 0; i < hitCount; i++)
			{
				var c = winningClusters[i].TrueCount;
				counts[i] = c;
				totalPatterns *= c;
			}

			var groupSizes = new int[hitCount];

			for (var i = 0; i < hitCount; i++)
			{
				// Group sizes for each cluster determine how many times a items are repeated in each column.
				// They are based on the total combinations of subsequent clusters.
				// For the example in the remarks these would be 6 3 and 1. The last group size is always 1.

				var gs = 1;

				for (var j = i + 1; j < hitCount; j++)
					gs *= counts[j];

				groupSizes[i] = gs;
			}

			var patterns = new Cell[totalPatterns][];

			for (var p = 0; p < totalPatterns; p++)
			{
				var pattern = new Cell[hitCount];

				for (var c = 0; c < hitCount; c++)
				{
					// Find the source index of the cell in the cluster using the group sizes.
					// Then add that CellSymbol to the pattern.
					var sourceIndex = p / groupSizes[c] % counts[c];
					pattern[c] = cells[winningClusters[c].EnumerateIndexes().ElementAt(sourceIndex)];
				}

				patterns[p] = pattern;
			}

			return patterns;
		}
	}
}