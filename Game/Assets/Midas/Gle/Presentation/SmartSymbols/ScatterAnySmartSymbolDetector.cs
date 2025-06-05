using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Utility;
using Logic.Core.WinCheck;
using Midas.Presentation.Reels.SmartSymbols;
using Midas.Presentation.StageHandling;

namespace Midas.Gle.Presentation.SmartSymbols
{
	public sealed class ScatterAnySmartSymbolDetector : GleSmartSymbolDetector
	{
		private readonly IReadOnlyList<string> symbols;
		private readonly int minimumSymbolCount;
		private readonly bool enableAnticipation;
		private readonly bool anticipateAfterTrigger;

		private IReadOnlyList<int> symbolIndexes;
		private IReadOnlyList<string> symbolListUsed;

		public ScatterAnySmartSymbolDetector(Stage stage, string resultName, IReadOnlyList<string> symbols, int minimumSymbolCount, bool enableAnticipation, bool anticipateAfterTrigger)
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
			var populationCount = populations.Count;

			var cells = symbolWindowStructure.Cells;

			if (symbolMask == null)
				return null;

			var smartCells = new List<(int Column, int Row)>();
			var anticipationMask = new List<bool>();
			var qualifyingReelIndex = default(int?);

			for (var reelIndex = 0; reelIndex < populations.Count; reelIndex++)
			{
				if (enableAnticipation)
					anticipationMask.Add(smartCells.Count == minimumSymbolCount - 1 || anticipateAfterTrigger && smartCells.Count >= minimumSymbolCount);

				if (populationCount - reelIndex < minimumSymbolCount - smartCells.Count)
					continue;

				var population = populations[reelIndex];

				for (var cellIndex = 0; cellIndex < population.Count; cellIndex++)
				{
					var index = population[cellIndex];

					if (!symbolMask[index])
					{
						continue;
					}

					smartCells.Add((cells[index].Column, cells[index].Row));

					if (!qualifyingReelIndex.HasValue && smartCells.Count == minimumSymbolCount)
						qualifyingReelIndex = reelIndex;
				}
			}

			return new SmartSymbolData(smartCells, anticipationMask, qualifyingReelIndex, qualifyingReelIndex.HasValue ? smartCells.Count : 0);
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