using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// Evaluator helper for line prizes
	/// </summary>
	public static class LineEvaluation
	{
		public static void ProcessSimplePayFunc(
			SymbolWindowResult symbolWindow,
			Patterns patterns,
			int linesSelected,
			MaskPrize prize,
			IList<CellPrizeResult> winningPrizes,
			Action<IList<CellPrizeResult>, MaskPrize, int, Pattern, IReadOnlyList<Cell>> processFunction,
			Func<LinePattern, SymbolWindowResult, ReadOnlyMask, MaskPrize, IReadOnlyList<Cell>> evaluateFunction,
			Func<IReadOnlyList<ReadOnlyMask>, ReadOnlyMask, int, bool> earlyOut,
			Func<SymbolWindowResult, MaskPrize, IReadOnlyList<Cell>, bool> shouldPrizePay)
		{
			var prizeSymbols = symbolWindow.SymbolMasks.GetCombinedSymbolMask(prize.Symbols);

			// First early out
			if (prizeSymbols.IsEmpty)
				return;

			// Second early out
			if (earlyOut != null && earlyOut(patterns.ClusterSummary, prizeSymbols, prize.StartingHitCount))
				return;

			for (var j = 0; j < linesSelected; j++)
			{
				var winningCells = evaluateFunction(patterns.LinePatterns[j], symbolWindow, prizeSymbols, prize);

				if (winningCells.Count == 0)
					continue;

				if (shouldPrizePay == null || shouldPrizePay(symbolWindow, prize, winningCells))
					processFunction(winningPrizes, prize, j, patterns.GetSourcePattern(j), winningCells);
			}
		}

		public static void ProcessPayBest(IList<CellPrizeResult> winningPatterns, MaskPrize prize, int j, Pattern pattern, IReadOnlyList<Cell> winningCells)
		{
			var count = winningCells.Count;
			var prizeHitIndex = count - prize.StartingHitCount >= prize.PrizePays.Count ? prize.PrizePays.Count - 1 : count - prize.StartingHitCount;

			if (winningPatterns[j] == null)
			{
				winningPatterns[j] = new CellPrizeResult(prize.Name, count, prize.PrizePays[prizeHitIndex], pattern, winningCells);
			}
			else
			{
				if (prize.PrizePays[prizeHitIndex] > winningPatterns[j].Value)
					winningPatterns[j] = new CellPrizeResult(prize.Name, count, prize.PrizePays[prizeHitIndex], pattern, winningCells);
			}
		}

		public static void ProcessPayMany(IList<CellPrizeResult> winningPatterns, MaskPrize prize, int j, Pattern pattern, IReadOnlyList<Cell> winningCells)
		{
			var count = winningCells.Count;
			var prizeHitIndex = count - prize.StartingHitCount >= prize.PrizePays.Count ? prize.PrizePays.Count - 1 : count - prize.StartingHitCount;
			winningPatterns.Add(new CellPrizeResult(prize.Name, count, prize.PrizePays[prizeHitIndex], pattern, winningCells));
		}

		public static bool EarlyOutLeft(IReadOnlyList<ReadOnlyMask> clusterSummary, ReadOnlyMask prizeSymbols, int startingHitCount)
		{
			for (var i = 0; i < startingHitCount; i++)
			{
				if (clusterSummary[i].AndIsEmpty(prizeSymbols))
					return true;
			}

			return false;
		}

		public static bool EarlyOutRight(IReadOnlyList<ReadOnlyMask> clusterSummary, ReadOnlyMask prizeSymbols, int startingHitCount)
		{
			var last = clusterSummary.Count - 1;

			for (var i = 0; i < startingHitCount; i++)
			{
				if (clusterSummary[last - i].AndIsEmpty(prizeSymbols))
					return true;
			}

			return false;
		}

		public static void ProcessPayBothFunc(SymbolWindowResult symbolWindow, Patterns patterns, int linesSelected, MaskPrize prize, IList<CellPrizeResult> winningPatterns,
			Action<IList<CellPrizeResult>, MaskPrize, int, Pattern, IReadOnlyList<Cell>> processFunction, bool payBest, Func<SymbolWindowResult, MaskPrize, IReadOnlyList<Cell>, bool> shouldPrizePay)
		{
			var prizeSymbols = symbolWindow.SymbolMasks.GetCombinedSymbolMask(prize.Symbols);

			// First early out
			if (prizeSymbols.IsEmpty)
				return;

			var leftActive = true;
			var rightActive = true;

			// Second early out
			for (var i = 0; i < prize.StartingHitCount; i++)
			{
				var index = patterns.ClusterSummary.Count - 1 - i;

				if (patterns.ClusterSummary[i].AndIsEmpty(prizeSymbols))
					leftActive = false;

				if (patterns.ClusterSummary[index].AndIsEmpty(prizeSymbols))
					rightActive = false;

				if (!leftActive && !rightActive)
					return;
			}

			for (var j = 0; j < linesSelected; j++)
			{
				GetPayBothWinMask(patterns.LinePatterns[j], prizeSymbols, out var leftCount, out var leftPositions, out var rightCount, out var rightPositions);

				if (leftCount == patterns.LinePatterns[j].Positions.Length)
				{
					var winningCells = new List<Cell>();

					for (var i = 0; i < leftCount; i++)
						winningCells.Add(symbolWindow.SymbolWindowStructure.Cells[leftPositions[i]]);

					if (shouldPrizePay == null || shouldPrizePay(symbolWindow, prize, winningCells))
						processFunction(winningPatterns, prize, j, patterns.GetSourcePattern(j), winningCells);
				}
				else
				{
					if (leftCount >= prize.StartingHitCount)
					{
						var winningCells = new List<Cell>();

						for (var i = 0; i < leftCount; i++)
							winningCells.Add(symbolWindow.SymbolWindowStructure.Cells[leftPositions[i]]);

						if (shouldPrizePay == null || shouldPrizePay(symbolWindow, prize, winningCells))
							processFunction(winningPatterns, prize, j, patterns.GetSourcePattern(j), winningCells);
					}

					if (rightCount >= prize.StartingHitCount && (!payBest || leftCount < prize.StartingHitCount))
					{
						var winningCells = new List<Cell>();

						for (var i = 0; i < rightCount; i++)
							winningCells.Add(symbolWindow.SymbolWindowStructure.Cells[rightPositions[i]]);

						if (shouldPrizePay == null || shouldPrizePay(symbolWindow, prize, winningCells))
							processFunction(winningPatterns, prize, j, patterns.GetSourcePattern(j), winningCells);
					}
				}
			}
		}

		private static void GetPayBothWinMask(LinePattern linePattern, ReadOnlyMask prizeSymbolMask, out int leftCount, out int[] leftPositions, out int rightCount, out int[] rightPositions)
		{
			var positions = linePattern.Positions;
			var positionsLength = positions.Length;

			leftPositions = new int[positionsLength];
			rightPositions = new int[positionsLength];
			leftCount = 0;
			rightCount = 0;

			var leftActive = true;
			var rightActive = true;

			for (var i = 0; i < positionsLength; i++)
			{
				if (leftActive)
				{
					var pos = positions[i];

					if (prizeSymbolMask[pos])
					{
						leftPositions[leftCount] = pos;
						leftCount++;
					}
					else
						leftActive = false;
				}

				if (rightActive)
				{
					var pos = positions[positionsLength - 1 - i];

					if (prizeSymbolMask[pos])
					{
						rightPositions[rightCount] = pos;
						rightCount++;
					}
					else
						rightActive = false;
				}

				if (!leftActive && !rightActive)
					return;
			}
		}

		public static IReadOnlyList<Cell> GetWinningCellsLeft(LinePattern linePattern, SymbolWindowResult symbolWindow, ReadOnlyMask prizeSymbolMask, MaskPrize maskPrize)
		{
			var cells = symbolWindow.SymbolWindowStructure.Cells;
			var positions = linePattern.Positions;
			var positionsLength = positions.Length;
			var winningCells = new List<Cell>(positionsLength);
			var count = 0;

			for (var i = 0; i < positionsLength; i++)
			{
				var pos = positions[i];

				if (!prizeSymbolMask[pos])
					break;

				winningCells.Add(cells[pos]);
				count++;
			}

			if (count < maskPrize.StartingHitCount)
				return Array.Empty<Cell>();

			return winningCells;
		}

		public static IReadOnlyList<Cell> GetWinningCellsRight(LinePattern linePattern, SymbolWindowResult symbolWindow, ReadOnlyMask prizeSymbolMask, MaskPrize maskPrize)
		{
			var cells = symbolWindow.SymbolWindowStructure.Cells;
			var positions = linePattern.Positions;
			var positionsLength = positions.Length;
			var positionsArray = new int[positionsLength];
			var count = 0;

			for (var i = 0; i < positionsLength; i++)
			{
				var index = positionsLength - 1 - i;
				var pos = positions[index];

				if (!prizeSymbolMask[pos])
					break;

				positionsArray[count] = pos;
				count++;
			}

			if (count < maskPrize.StartingHitCount)
				return Array.Empty<Cell>();

			var winningCells = new List<Cell>();

			for (var i = 0; i < count; i++)
				winningCells.Add(cells[positionsArray[i]]);

			return winningCells;
		}

		public static IReadOnlyList<Cell> GetWinningCellsAny(LinePattern linePattern, SymbolWindowResult symbolWindow, ReadOnlyMask prizeSymbolMask, MaskPrize maskPrize)
		{
			if (prizeSymbolMask == null)
				return Array.Empty<Cell>();

			var winMask = prizeSymbolMask.And(linePattern.PositionsMask);

			if (winMask.IsEmpty)
				return Array.Empty<Cell>();

			var trueCount = winMask.TrueCount;

			if (trueCount < maskPrize.StartingHitCount)
				return Array.Empty<Cell>();

			var indexes = winMask.EnumerateIndexes().ToArray();
			var cells = symbolWindow.SymbolWindowStructure.Cells;
			var winningCells = new Cell[trueCount];

			for (var i = 0; i < trueCount; i++)
				winningCells[i] = cells[indexes[i]];

			return winningCells;
		}
	}
}