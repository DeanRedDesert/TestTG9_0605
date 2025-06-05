using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Utility;
using Logic.Core.WinCheck;
using Midas.Presentation.Reels.SmartSymbols;
using Midas.Presentation.StageHandling;

namespace Midas.Gle.Presentation.SmartSymbols
{
	public sealed class StackedScatterAnySmartSymbolDetector : GleSmartSymbolDetector
	{
		private readonly IReadOnlyList<string> symbols;
		private readonly int minimumSymbolCount;
		private readonly bool enableAnticipation;
		private readonly bool anticipateAfterTrigger;

		private IReadOnlyList<int> symbolIndexes;
		private IReadOnlyList<string> symbolListUsed;

		public StackedScatterAnySmartSymbolDetector(Stage stage, string resultName, IReadOnlyList<string> symbols, int minimumSymbolCount, bool enableAnticipation, bool anticipateAfterTrigger)
			: base(stage, resultName)
		{
			this.symbols = symbols;
			this.minimumSymbolCount = minimumSymbolCount;
			this.enableAnticipation = enableAnticipation;
			this.anticipateAfterTrigger = anticipateAfterTrigger;
		}

		protected override SmartSymbolData FindSmartSymbols(SymbolWindowResult symbolWindow, ReadOnlyMask lockMask)
		{
			var symbolList = symbolWindow.SymbolList;

			if (!ReferenceEquals(symbolListUsed, symbolList))
			{
				symbolIndexes = GetSymbolIndexes(symbolList, symbols);
				symbolListUsed = symbolList;
			}

			var symbolMasks = symbolWindow.SymbolMasks;
			var symbolMask = symbolMasks.GetCombinedSymbolMask(symbolIndexes);

			var symbolWindowStructure = symbolWindow.SymbolWindowStructure;
			var populations = symbolWindowStructure.PopulationsAsIndexes;
			var cells = symbolWindowStructure.Cells;

			if (symbolMask == null)
				return null;

			var smartCells = new List<(int Column, int Row)>();
			var anticipationMask = new List<bool>();
			var qualifyingReelIndex = default(int?);
			var involvedReels = 0;

			for (var reelIndex = 0; reelIndex < populations.Count; reelIndex++)
			{
				if (enableAnticipation)
					anticipationMask.Add(IsTriggerPossibleNextStop(smartCells.Count, populations[reelIndex].Count) || anticipateAfterTrigger && smartCells.Count >= minimumSymbolCount);

				if (!IsTriggerStillPossible(smartCells.Count, reelIndex, populations))
					continue;

				var population = populations[reelIndex];
				var newSmartCells = 0;

				for (var cellIndex = 0; cellIndex < population.Count; cellIndex++)
				{
					var index = population[cellIndex];

					if (!symbolMask[index])
					{
						continue;
					}

					smartCells.Add((cells[index].Column, cells[index].Row));
					newSmartCells++;

					if (!qualifyingReelIndex.HasValue && smartCells.Count == minimumSymbolCount)
						qualifyingReelIndex = reelIndex;
				}

				if (newSmartCells > 0)
					involvedReels++;

				if (IsTriggerStillPossible(smartCells.Count, reelIndex + 1, populations))
				{
					continue;
				}

				if (newSmartCells > 0)
				{
					involvedReels--;
					while (newSmartCells > 0)
					{
						smartCells.RemoveAt(smartCells.Count - 1);
						--newSmartCells;
					}
				}
			}

			return new SmartSymbolData(smartCells, anticipationMask, qualifyingReelIndex, involvedReels);
		}

		private bool IsTriggerStillPossible(int currentTriggerCount, int nextReelIndex, IReadOnlyList<IReadOnlyList<int>> populations)
		{
			var remainingTriggerCount = minimumSymbolCount - currentTriggerCount;

			for (var i = nextReelIndex; i < populations.Count; i++)
			{
				remainingTriggerCount -= populations[i].Count;
			}

			return remainingTriggerCount <= 0;
		}

		private bool IsTriggerPossibleNextStop(int currentTriggerCount, int nextReelVisibleSymbols)
		{
			return minimumSymbolCount - currentTriggerCount <= nextReelVisibleSymbols;
		}

		private static IReadOnlyList<int> GetSymbolIndexes(IReadOnlyList<string> symbolList, IReadOnlyList<string> symbols)
		{
			var symbolIndexes = new int[symbols.Count];

			for (var i = 0; i < symbols.Count; i++)
			{
				symbolIndexes[i] = symbolList.IndexOf(symbols[i]);
			}

			return symbolIndexes;
		}
	}
}