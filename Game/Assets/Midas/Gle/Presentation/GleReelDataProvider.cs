using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;
using Midas.Gle.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Presentation.Reels;
using Midas.Presentation.StageHandling;

namespace Midas.Gle.Presentation
{
	public abstract class GleReelDataProvider : ReelDataProvider
	{
		private GleResult latestResult;
		private ReelStrip[] strips;

		protected abstract SymbolWindowResult GetInitReelResult(Stage stage);
		protected abstract (SymbolWindowResult SymbolWindowResult, ReadOnlyMask LockMask) GetReelResult(Stage stage);

		public override IReadOnlyList<ReelStrip> GetInitReelStrips(Stage stage, ReelContainer reelContainer)
		{
			var symbolWindowResult = GetInitReelResult(stage);
			var initStrips = new List<ReelStrip>(reelContainer.Reels.Count);

			foreach (var reel in reelContainer.Reels)
			{
				var initStrip = GetInitStrip(reel, symbolWindowResult);
				initStrips.Add(new ReelStrip(symIndex => initStrip[(int)symIndex], initStrip.Length, reel.SymbolsAbove));
			}

			return initStrips;
		}

		public override IReadOnlyList<ReelStrip> GetReelStrips(Stage stage)
		{
			Refresh(stage);
			return strips;
		}

		private void Refresh(Stage stage)
		{
			var currentResult = StatusDatabase.QueryStatusBlock<GleStatus>().CurrentGameResult;
			if (latestResult != currentResult)
			{
				latestResult = currentResult;

				var reelDetails = GetReelResult(stage);
				var reelCount = reelDetails.SymbolWindowResult.SourceStrips.Count;

				// Build new strips and final symbol structures

				strips = new ReelStrip[reelCount];

				// Fill the new structures with data

				for (var i = 0; i < reelCount; i++)
				{
					var sourceStrip = reelDetails.SymbolWindowResult.SourceStrips[i];
					var stop = reelDetails.SymbolWindowResult.StripSelections[i];
					var locked = reelDetails.LockMask[i];
					strips[i] = locked ? null : new ReelStrip(symIndex => sourceStrip.GetSymbol((ulong)symIndex), (long)sourceStrip.GetLength(), (long)stop);
				}
			}
		}

		private static string[] GetInitStrip(Reel reel, SymbolWindowResult swr)
		{
			// Check if the result has a populator that matches the reel

			var pops = swr.SymbolWindowStructure.GetSourcePopulations();
			for (var index = 0; index < pops.Count; index++)
			{
				var pop = pops[index];
				var cells = pop.GetCells();
				var firstCell = cells[0];

				if (firstCell.Column == reel.Column && firstCell.Row == reel.Row && cells.Count == reel.VisibleSymbols)
					return GetNormalInitStrip(reel, swr, index);
			}

			// No? Adapt the result to match the reel

			return GetAdaptedInitStrip(reel, swr);
		}

		private static string[] GetAdaptedInitStrip(Reel reel, SymbolWindowResult swr)
		{
			var symbols = new string[reel.TotalSymbolCount];
			var stopIndex = reel.Row - reel.SymbolsAbove;
			var symIndex = 0;

			if (stopIndex < 0)
			{
				var aboveReelPopIndex = GetAboveReelPopulationIndex();
				var aboveReelStrip = swr.SourceStrips[aboveReelPopIndex];
				var aboveReelStop = (long)swr.StripSelections[aboveReelPopIndex] + (long)aboveReelStrip.GetLength() + stopIndex;
				aboveReelStop %= (long)aboveReelStrip.GetLength();

				while (stopIndex < 0)
				{
					symbols[symIndex++] = aboveReelStrip.GetSymbol((ulong)aboveReelStop);
					aboveReelStop = (aboveReelStop + 1) % (long)aboveReelStrip.GetLength();
					stopIndex++;
				}
			}

			while (symIndex < symbols.Length)
			{
				// Assuming cells are contiguous

				var symListIndex = GetSymbolIndexAt(new Cell(reel.Column, stopIndex));
				if (symListIndex == -1)
					break;

				symbols[symIndex++] = swr.SymbolList[symListIndex];
				stopIndex++;
			}

			// Still more symbols to get

			if (symIndex < symbols.Length)
			{
				var belowReelPopIndex = GetBelowReelPopulationIndex();
				var belowReelStrip = swr.SourceStrips[belowReelPopIndex];
				var popRow = swr.SymbolWindowStructure.GetSourcePopulations()[belowReelPopIndex].GetCells().Min(c => c.Row);
				var belowReelStop = (long)swr.StripSelections[belowReelPopIndex] + stopIndex - popRow;
				belowReelStop %= (long)belowReelStrip.GetLength();

				while (symIndex < symbols.Length)
				{
					symbols[symIndex++] = belowReelStrip.GetSymbol((ulong)belowReelStop);
					belowReelStop = (belowReelStop + 1) % (long)belowReelStrip.GetLength();
					stopIndex++;
				}
			}

			return symbols;

			int GetAboveReelPopulationIndex()
			{
				var pops = swr.SymbolWindowStructure.GetSourcePopulations();
				var row = int.MaxValue;
				var minRowPopIndex = -1;
				for (var popIndex = 0; popIndex < pops.Count; popIndex++)
				{
					var pop = pops[popIndex];
					var cells = pop.GetCells();
					if (cells[0].Column != reel.Column)
						continue;
					var minRow = cells.Min(c => c.Row);
					if (minRow < row)
					{
						row = minRow;
						minRowPopIndex = popIndex;
					}
				}

				return minRowPopIndex;
			}

			int GetBelowReelPopulationIndex()
			{
				var pops = swr.SymbolWindowStructure.GetSourcePopulations();
				var row = int.MinValue;
				var maxRowPopIndex = -1;
				for (var popIndex = 0; popIndex < pops.Count; popIndex++)
				{
					var pop = pops[popIndex];
					var cells = pop.GetCells();
					if (cells[0].Column != reel.Column)
						continue;
					var maxRow = cells.Max(c => c.Row);
					if (maxRow > row)
					{
						row = maxRow;
						maxRowPopIndex = popIndex;
					}
				}

				return maxRowPopIndex;
			}

			int GetSymbolIndexAt(Cell cell)
			{
				var cellIndex = swr.SymbolWindowStructure.GetCellIndexOrDefault(cell);
				if (cellIndex == -1)
					return -1;

				for (var i = 0; i < swr.SymbolMasks.Count; i++)
				{
					if (swr.SymbolMasks[i] == null)
						continue;
					if (swr.SymbolMasks[i][cellIndex])
						return i;
				}

				return -1;
			}
		}

		private static string[] GetNormalInitStrip(Reel reel, SymbolWindowResult swr, int reelPop)
		{
			// The populator matches, so use it
			var symbols = new string[reel.TotalSymbolCount];
			var stopIndex = -reel.SymbolsAbove;
			var symIndex = 0;
			var strip = swr.SourceStrips[reelPop];
			var stop = (long)swr.StripSelections[reelPop] + (long)strip.GetLength() + stopIndex;
			stop %= (long)strip.GetLength();

			// Above the population window, symbols come from the strip

			while (stopIndex < 0)
			{
				symbols[symIndex++] = strip.GetSymbol((ulong)stop);
				stop = (stop + 1) % (long)strip.GetLength();
				stopIndex++;
			}

			// Inside the population window, symbols come from the result (to include locked symbols)

			while (stopIndex < reel.VisibleSymbols)
			{
				symbols[symIndex++] = swr.GetSymbolAt(new Cell(reel.Column, reel.Row + stopIndex++));
				stop++;
			}

			// Below the population window, symbols come from the strip

			stop %= (long)strip.GetLength();

			while (symIndex < symbols.Length)
			{
				symbols[symIndex++] = strip.GetSymbol((ulong)stop);
				stop = (stop + 1) % (long)strip.GetLength();
				stopIndex++;
			}

			return symbols;
		}
	}
}