using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// Evaluator helper for scatter prizes
	/// </summary>
	public static class ScatterEvaluation
	{
		public static IReadOnlyList<CellPrizeResult> Evaluate(SymbolWindowResult symbolWindow, Patterns patterns, IReadOnlyList<MaskPrize> prizes, bool payBest)
		{
			var patternResults = payBest ? (IList<CellPrizeResult>)new CellPrizeResult[patterns.ClusterSummary.Count] : new List<CellPrizeResult>();
			var action = payBest ? (Action<IList<CellPrizeResult>, MaskPrize, int, Pattern, IReadOnlyList<Cell>>)ProcessPayBest : ProcessPayMany;
			var prizesCount = prizes.Count;

			for (var p = 0; p < prizesCount; p++)
			{
				var prize = prizes[p];
				var shouldPrizePay = prize.RequiredSymbolCounts != null ? (Func<SymbolWindowResult, MaskPrize, IReadOnlyList<Cell>, bool>)EvaluationHelper.CheckAddPrize : null;

				switch (prize.Strategy)
				{
					case PrizeStrategy.Left:
					{
						ProcessSimplePayFunc(symbolWindow, patterns, prize, patternResults, action, GetWinningCellsLeft, shouldPrizePay);
						break;
					}
					case PrizeStrategy.Right:
					{
						ProcessSimplePayFunc(symbolWindow, patterns, prize, patternResults, action, GetWinningCellsRight, shouldPrizePay);
						break;
					}
					case PrizeStrategy.Any:
					{
						ProcessSimplePayFunc(symbolWindow, patterns, prize, patternResults, action, GetWinningCellsAny, shouldPrizePay);
						break;
					}
					case PrizeStrategy.Both:
					{
						ProcessPayBothFunc(symbolWindow, patterns, prize, patternResults, action, payBest, shouldPrizePay);
						break;
					}
					default:
						throw new NotSupportedException();
				}
			}

			if (payBest)
			{
				var winningPatterns = new List<CellPrizeResult>();
				var patternResultsCount = patternResults.Count;

				for (var i = 0; i < patternResultsCount; i++)
				{
					var pr = patternResults[i];

					if (pr != null)
						winningPatterns.Add(pr);
				}

				return winningPatterns;
			}

			return (IReadOnlyList<CellPrizeResult>)patternResults;
		}

		private static void ProcessSimplePayFunc(
			SymbolWindowResult symbolWindow,
			Patterns patterns,
			MaskPrize prize,
			IList<CellPrizeResult> winningPrizes,
			Action<IList<CellPrizeResult>, MaskPrize, int, Pattern, IReadOnlyList<Cell>> processFunction,
			Func<ClusterPattern, SymbolWindowResult, ReadOnlyMask, MaskPrize, IReadOnlyList<Cell>> evaluateFunction,
			Func<SymbolWindowResult, MaskPrize, IReadOnlyList<Cell>, bool> shouldPrizePay)
		{
			var prizeSymbols = symbolWindow.SymbolMasks.GetCombinedSymbolMask(prize.Symbols);

			// First early out
			if (prizeSymbols.IsEmpty)
				return;

			for (var j = 0; j < patterns.ClusterPatterns.Count; j++)
			{
				var winningCells = evaluateFunction(patterns.ClusterPatterns[j], symbolWindow, prizeSymbols, prize);

				if (winningCells.Count == 0)
					continue;

				if (shouldPrizePay == null || shouldPrizePay(symbolWindow, prize, winningCells))
					processFunction(winningPrizes, prize, j, patterns.GetSourcePattern(j), winningCells);
			}
		}

		private static void ProcessPayBest(IList<CellPrizeResult> winningPatterns, MaskPrize prize, int j, Pattern pattern, IReadOnlyList<Cell> winningCells)
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

		private static void ProcessPayMany(IList<CellPrizeResult> winningPatterns, MaskPrize prize, int j, Pattern pattern, IReadOnlyList<Cell> winningCells)
		{
			var count = winningCells.Count;
			var prizeHitIndex = count - prize.StartingHitCount >= prize.PrizePays.Count ? prize.PrizePays.Count - 1 : count - prize.StartingHitCount;
			winningPatterns.Add(new CellPrizeResult(prize.Name, count, prize.PrizePays[prizeHitIndex], pattern, winningCells));
		}

		private static void ProcessPayBothFunc(
			SymbolWindowResult symbolWindow,
			Patterns patterns,
			MaskPrize prize,
			IList<CellPrizeResult> winningPatterns,
			Action<IList<CellPrizeResult>, MaskPrize, int, Pattern, IReadOnlyList<Cell>> processFunction,
			bool payBest,
			Func<SymbolWindowResult, MaskPrize, IReadOnlyList<Cell>, bool> shouldPrizePay)
		{
			var prizeSymbols = symbolWindow.SymbolMasks.GetCombinedSymbolMask(prize.Symbols);

			// First early out
			if (prizeSymbols.IsEmpty)
				return;

			for (var j = 0; j < patterns.ClusterPatterns.Count; j++)
			{
				GetPayBothWinMask(patterns.ClusterPatterns[j], prizeSymbols, symbolWindow.SymbolWindowStructure.Cells, out var leftPositions, out var rightPositions);

				if (leftPositions.Count == patterns.ClusterPatterns[j].Positions.Length)
				{
					if (shouldPrizePay == null || shouldPrizePay(symbolWindow, prize, leftPositions))
						processFunction(winningPatterns, prize, j, patterns.GetSourcePattern(j), leftPositions);
				}
				else
				{
					if (leftPositions.Count >= prize.StartingHitCount)
					{
						if (shouldPrizePay == null || shouldPrizePay(symbolWindow, prize, leftPositions))
							processFunction(winningPatterns, prize, j, patterns.GetSourcePattern(j), leftPositions);
					}

					if (rightPositions.Count >= prize.StartingHitCount && (!payBest || leftPositions.Count < prize.StartingHitCount))
					{
						if (shouldPrizePay == null || shouldPrizePay(symbolWindow, prize, rightPositions))
							processFunction(winningPatterns, prize, j, patterns.GetSourcePattern(j), rightPositions);
					}
				}
			}
		}

		private static void GetPayBothWinMask(ClusterPattern clusterPattern, ReadOnlyMask prizeSymbols, IReadOnlyList<Cell> cells, out IReadOnlyList<Cell> leftPositions, out IReadOnlyList<Cell> rightPositions)
		{
			var leftWinMask = new List<Cell>();
			var rightWinMask = new List<Cell>();
			var leftActive = true;
			var rightActive = true;

			for (var i = 0; i < clusterPattern.Positions.Length; i++)
			{
				if (leftActive)
				{
					var trueCount = prizeSymbols.AndTrueCount(clusterPattern.PositionsMask[i], out var leftClusterWinMask);

					switch (trueCount)
					{
						case 0: leftActive = false; break;
						// Scatters take the first winning symbol in a cluster
						case 1: leftWinMask.Add(cells[leftClusterWinMask.EnumerateIndexes().First()]); break;
						default: throw new NotSupportedException(" Scatter prizes with required counts cannot have multiple visible winning symbols in a cluster.");
					}
				}

				if (rightActive)
				{
					var fromRightIndex = clusterPattern.Positions.Length - 1 - i;
					var trueCount = prizeSymbols.AndTrueCount(clusterPattern.PositionsMask[fromRightIndex], out var rightClusterWinMask);

					switch (trueCount)
					{
						case 0: rightActive = false; break;
						// Scatters take the first winning symbol in a cluster
						case 1: rightWinMask.Add(cells[rightClusterWinMask.EnumerateIndexes().First()]); break;
						default: throw new NotSupportedException(" Scatter prizes with required counts cannot have multiple visible winning symbols in a cluster.");
					}
				}

				if (!leftActive && !rightActive)
					break;
			}

			leftPositions = leftWinMask;
			rightPositions = rightWinMask;
		}

		private static IReadOnlyList<Cell> GetWinningCellsLeft(ClusterPattern clusterPattern, SymbolWindowResult symbolWindow, ReadOnlyMask prizeSymbols, MaskPrize maskPrize)
		{
			var winningCells = new List<Cell>();

			for (var p = 0; p < clusterPattern.Positions.Length; p++)
			{
				var trueCount = prizeSymbols.AndTrueCount(clusterPattern.PositionsMask[p], out var clusterWinMask);

				if (trueCount == 0)
					break;

				if (trueCount == 1)
					winningCells.Add(symbolWindow.SymbolWindowStructure.Cells[clusterWinMask.EnumerateIndexes().First()]);
				else
					throw new NotSupportedException(" Scatter prizes with required counts cannot have multiple visible winning symbols in a cluster.");
			}

			if (winningCells.Count >= maskPrize.StartingHitCount)
				return winningCells;

			return Array.Empty<Cell>();
		}

		private static IReadOnlyList<Cell> GetWinningCellsRight(ClusterPattern clusterPattern, SymbolWindowResult symbolWindow, ReadOnlyMask prizeSymbols, MaskPrize maskPrize)
		{
			var winningCells = new List<Cell>();

			for (var p = 0; p < clusterPattern.Positions.Length; p++)
			{
				var index = clusterPattern.Positions.Length - 1 - p;
				var trueCount = prizeSymbols.AndTrueCount(clusterPattern.PositionsMask[index], out var clusterWinMask);

				if (trueCount == 0)
					break;

				if (trueCount == 1)
					winningCells.Add(symbolWindow.SymbolWindowStructure.Cells[clusterWinMask.EnumerateIndexes().First()]);
				else
					throw new NotSupportedException(" Scatter prizes with required counts cannot have multiple visible winning symbols in a cluster.");
			}

			if (winningCells.Count >= maskPrize.StartingHitCount)
				return winningCells;

			return Array.Empty<Cell>();
		}

		private static IReadOnlyList<Cell> GetWinningCellsAny(ClusterPattern clusterPattern, SymbolWindowResult symbolWindow, ReadOnlyMask prizeSymbols, MaskPrize maskPrize)
		{
			var winningCells = new List<Cell>();

			for (var i = 0; i < clusterPattern.Positions.Length; i++)
			{
				var trueCount = prizeSymbols.AndTrueCount(clusterPattern.PositionsMask[i], out var clusterWinMask);

				if (trueCount == 1)
					winningCells.Add(symbolWindow.SymbolWindowStructure.Cells[clusterWinMask.EnumerateIndexes().First()]);
				else if (trueCount > 1)
					throw new NotSupportedException(" Scatter prizes with required counts cannot have multiple visible winning symbols in a cluster.");
			}

			if (winningCells.Count >= maskPrize.StartingHitCount)
				return winningCells;

			return Array.Empty<Cell>();
		}
	}
}