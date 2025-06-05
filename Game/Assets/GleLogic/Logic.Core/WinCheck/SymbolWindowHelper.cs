using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;

// ReSharper disable UnusedMember.Global

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// A collection of helper functions for use with the SymbolWindowResult and related classes.
	/// </summary>
	public static class SymbolWindowHelper
	{
		/// <summary>
		/// Create a <see cref="SymbolWindowResult"/> based on an existing result and a set of new symbol masks.
		/// </summary>
		public static SymbolWindowResult CreateSymbolWindowResult(this SymbolWindowResult sourceResult, IReadOnlyList<ReadOnlyMask> newMasks)
		{
			return new SymbolWindowResult(sourceResult.SymbolList, sourceResult.SymbolWindowStructure, newMasks, sourceResult.SourceStrips, sourceResult.StripSelections);
		}

		/// <summary>
		/// Create a <see cref="SymbolWindowResult"/> based on the symbol list, structure, and an array of arrays of symbol name.
		/// </summary>
		public static SymbolWindowResult CreateSymbolWindowResult(this SymbolWindowStructure symbolWindowStructure, IReadOnlyList<IReadOnlyList<string>> symbolWindowSymbols, SymbolList symbolList)
		{
			var pops = symbolWindowStructure.PopulationsAsIndexes;
			var popsCount = pops.Count;

			if (symbolWindowSymbols.Count != popsCount)
				throw new Exception("Not enough populations");

			Mask[] workingData = null;
			var cellsCount = symbolWindowStructure.Cells.Count;

			for (var i = 0; i < popsCount; i++)
			{
				var pop = pops[i];
				var sym = symbolWindowSymbols[i];
				var popCount = pop.Count;
				var symCount = sym.Count;

				if (popCount != symCount)
					throw new Exception($"Not enough symbols in population at index {i}.");

				if (workingData == null)
					workingData = new Mask[symbolList.Count];

				for (var j = 0; j < popCount; j++)
				{
					var symbolIndex = symbolList.IndexOf(sym[j]);

					ref var wd = ref workingData[symbolIndex];

					if (wd == null)
						wd = new Mask(cellsCount);

					wd[pop[j]] = true;
				}
			}

			if (workingData == null)
				throw new Exception("Symbol masks failed to generate");

			return new SymbolWindowResult(symbolList, symbolWindowStructure, workingData.ToReadOnlyMasks(symbolWindowStructure.Cells.Count));
		}

		/// <summary>
		/// Create a <see cref="SymbolWindowResult"/> based on the symbol list, strips and structure using the chosen indexes (usually from RNG but not always).
		/// </summary>
		public static SymbolWindowResult CreateSymbolWindowResult(this IReadOnlyList<ISymbolListStrip> strips, SymbolWindowStructure symbolWindowStructure, IReadOnlyList<ulong> chosenIndexes)
		{
			var pops = symbolWindowStructure.PopulationsAsIndexes;
			var popsCount = pops.Count;

			if (chosenIndexes.Count != popsCount)
				throw new Exception("Not enough chosen indexes");

			Mask[] workingData = null;
			SymbolList symbolList = null;
			var cellsCount = symbolWindowStructure.Cells.Count;

			for (var i = 0; i < popsCount; i++)
			{
				var strip = strips[i];
				var stripLength = strip.GetLength();

				if (symbolList == null)
					symbolList = strip.GetSymbolList();
				else if (symbolList.GetId() != strip.GetSymbolList().GetId())
					throw new Exception("Mixed symbol lists not supported");

				if (workingData == null)
					workingData = new Mask[symbolList.Count];

				var pop = pops[i];
				var popCount = pop.Count;
				var chosenIndex = chosenIndexes[i];

				for (var j = 0; j < popCount; j++)
				{
					var position = (chosenIndex + stripLength + (ulong)j) % stripLength;
					var symbolIndex = strip.GetSymbolIndex(position);

					ref var wd = ref workingData[symbolIndex];

					if (wd == null)
						wd = new Mask(cellsCount);

					wd[pop[j]] = true;
				}
			}

			if (workingData == null)
				throw new Exception("Symbol masks failed to generate");

			return new SymbolWindowResult(symbolList, symbolWindowStructure, workingData.ToReadOnlyMasks(cellsCount), strips, chosenIndexes);
		}

		/// <summary>
		/// Create a <see cref="LockedSymbolWindowResult"/> based on the symbol list, strips and structure using the chosen indexes (usually from RNG but not always).  Use the ILockData object to only generate cells that are not locked.
		/// </summary>
		public static LockedSymbolWindowResult CreateLockedSymbolWindowResult(this IReadOnlyList<ISymbolListStrip> strips, SymbolWindowStructure symbolWindowStructure, IReadOnlyList<ulong> chosenIndexes, ILockData lockData)
		{
			var pops = symbolWindowStructure.PopulationsAsIndexes;
			var popsCount = pops.Count;

			if (chosenIndexes.Count != popsCount)
				throw new Exception("Not enough chosen indexes");

			Mask[] workingData = null;
			SymbolList symbolList = null;
			var lockMask = lockData.GetLockMask();
			var cellsCount = symbolWindowStructure.Cells.Count;

			for (var i = 0; i < popsCount; i++)
			{
				var strip = strips[i];
				var stripLength = strip.GetLength();

				if (symbolList == null)
					symbolList = strip.GetSymbolList();
				else if (symbolList.GetId() != strip.GetSymbolList().GetId())
					throw new Exception("Mixed symbol lists not supported");

				if (workingData == null)
					workingData = new Mask[symbolList.Count];

				var isLocked = lockMask[i];
				var pop = pops[i];
				var popCount = pop.Count;
				var chosenIndex = chosenIndexes[i];

				for (var j = 0; j < popCount; j++)
				{
					var popIndex = pop[j];
					var symbolIndex = isLocked
						? lockData.GetLockedSymbolIndexAt(popIndex)
						: strip.GetSymbolIndex((chosenIndex + stripLength + (ulong)j) % stripLength);

					ref var wd = ref workingData[symbolIndex];

					if (wd == null)
						wd = new Mask(cellsCount);

					wd[popIndex] = true;
				}
			}

			if (workingData == null)
				throw new Exception("Symbol masks failed to generate");

			return new LockedSymbolWindowResult(new SymbolWindowResult(symbolList, symbolWindowStructure, workingData.ToReadOnlyMasks(symbolWindowStructure.Cells.Count), strips, chosenIndexes), lockMask);
		}

		/// <summary>
		/// Take a list of cell structure indexes and return a collection of the cells they represent.
		/// </summary>
		public static IReadOnlyList<Cell> IndexesToCells(this SymbolWindowStructure structure, IReadOnlyList<int> indexes)
		{
			var indexesCount = indexes.Count;
			var cells = new Cell[indexesCount];

			for (var i = 0; i < indexesCount; i++)
				cells[i] = structure.Cells[indexes[i]];

			return cells;
		}

		/// <summary>
		/// Take a list of cells and return a list of their structure indexes.
		/// </summary>
		public static IReadOnlyList<int> CellsToIndexes(this SymbolWindowStructure structure, IReadOnlyList<Cell> cells)
		{
			var cellsCount = cells.Count;
			var indexes = new int[cellsCount];

			for (var i = 0; i < cellsCount; i++)
				indexes[i] = structure.GetCellIndex(cells[i]);

			return indexes;
		}

		/// <summary>
		/// Take a list of cells and return a list of their structure indexes.
		/// </summary>
		public static ReadOnlyMask CellsToMask(this SymbolWindowStructure structure, IReadOnlyList<Cell> cells)
		{
			var newMask = new Mask(structure.Cells.Count);

			foreach (var cell in cells)
				newMask[structure.GetCellIndex(cell)] = true;

			return newMask.Lock();
		}

		/// <summary>
		/// Create set of mask prizes from a collection of traditional prizes.
		/// </summary>
		public static IReadOnlyList<MaskPrize> CreateMaskPrizes(this SymbolList symbolList, IReadOnlyList<Prize> prizes)
		{
			var prizesCount = prizes.Count;
			var maskPrizes = new MaskPrize[prizesCount];

			for (var i = 0; i < prizesCount; i++)
				maskPrizes[i] = symbolList.CreateMaskPrize(prizes[i]);

			return maskPrizes;
		}

		/// <summary>
		/// Create a mask prize from a traditional prize.
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		public static MaskPrize CreateMaskPrize(this SymbolList symbolList, Prize prize)
		{
			var startCount = prize.PrizePays.Min(p => p.Count);
			var maxCount = prize.PrizePays.Max(p => p.Count);
			var prizes = new List<int>();
			var symbols = new List<int>();
			var symbolRequiredCounts = new List<int>();

			for (var i = startCount; i <= maxCount; i++)
			{
				var pp = prize.PrizePays.SingleOrDefault(p => p.Count == i)?.Value ?? 0;
				prizes.Add(pp);
			}

			var anyRequired = false;
			var prizeSymbols = prize.Symbols;
			var prizeSymbolsCount = prizeSymbols.Count;

			for (var i = 0; i < prizeSymbolsCount; i++)
			{
				var ps = prizeSymbols[i];
				var symIndex = symbolList.IndexOf(ps.SymbolName);

				if (symIndex < 0)
					throw new Exception();

				symbols.Add(symIndex);
				symbolRequiredCounts.Add(ps.Required);

				if (ps.Required > 0)
					anyRequired = true;
			}

			return new MaskPrize(prize.Name, prize.Strategy, startCount, prizes, symbols, anyRequired ? symbolRequiredCounts : null);
		}

		/// <summary>
		/// Take symbol masks for the specified symbol indexes and efficiently OR them together.
		/// </summary>
		public static ReadOnlyMask GetCombinedSymbolMask(this IReadOnlyList<ReadOnlyMask> symbolMasks, IReadOnlyList<int> targetSymbolIndexes)
		{
			switch (targetSymbolIndexes.Count)
			{
				case 0: throw new NotSupportedException();
				// If only one sub then return it without construction
				case 1: return symbolMasks[targetSymbolIndexes[0]];
				// If only two sub then return OR of the two of them.
				case 2: return symbolMasks[targetSymbolIndexes[0]].Or(symbolMasks[targetSymbolIndexes[1]]);
			}

			// Finally if we have more than two, OR them all together.
			var mask = symbolMasks[targetSymbolIndexes[0]];

			for (var i = 1; i < targetSymbolIndexes.Count; i++)
				mask = mask.Or(symbolMasks[targetSymbolIndexes[i]]);

			return mask;
		}

		/// <summary>
		/// Check for <param name="fromSymbolIndex"/> in the cells specified by the mask <param name="cells"/> and change it to <param name="toSymbolIndex"/>
		/// Gets the mask at symbol index <param name="fromSymbolIndex"/> for the cells specified in the mask<param name="cells"/> and OR's it into the
		/// destination symbol index <param name="toSymbolIndex"/>
		/// </summary>
		public static void ChangeSymbolIndex(this IList<ReadOnlyMask> symbolMasks, ReadOnlyMask cells, int fromSymbolIndex, int toSymbolIndex)
		{
			// No target symbols we bail.
			if (symbolMasks[fromSymbolIndex] == null)
				return;

			if (cells.AndIsEmpty(symbolMasks[fromSymbolIndex], out var symbolMask))
				return;

			symbolMasks[fromSymbolIndex] = symbolMasks[fromSymbolIndex].And(cells.Not());
			symbolMasks[toSymbolIndex] = symbolMasks[toSymbolIndex].Or(symbolMask);
		}

		/// <summary>
		/// Check for <param name="fromSymbolIndexes"/> in the cells specified by the mask <param name="cells"/> and change it to <param name="toSymbolIndex"/>
		/// Gets the masks at symbol indexes <param name="fromSymbolIndexes"/> for the cells specified in the mask<param name="cells"/> and OR's them into the
		/// destination symbol index <param name="toSymbolIndex"/>
		/// </summary>
		public static void ChangeSymbolIndexes(this IList<ReadOnlyMask> symbolMasks, ReadOnlyMask cells, IReadOnlyList<int> fromSymbolIndexes, int toSymbolIndex)
		{
			foreach (var i in fromSymbolIndexes)
			{
				if (symbolMasks[i] == null)
					continue;

				if (cells.AndIsEmpty(symbolMasks[i], out var symbolMask))
					continue;

				symbolMasks[i] = symbolMasks[i].And(cells.Not());
				symbolMasks[toSymbolIndex] = symbolMasks[toSymbolIndex].Or(symbolMask);
			}
		}

		/// <summary>
		/// Check for <param name="fromSymbolIndex"/> in a population and change it to <param name="toSymbolIndex"/>
		/// Gets the mask at symbol index <param name="fromSymbolIndex"/> for the population at <param name="populationIndex"/> and OR's it into the
		/// destination symbol index <param name="toSymbolIndex"/>
		/// </summary>
		public static void ChangeSymbolIndex(this IList<ReadOnlyMask> symbolMasks, SymbolWindowStructure symbolWindowStructure, int populationIndex, int fromSymbolIndex, int toSymbolIndex)
		{
			symbolMasks.ChangeSymbolIndex(symbolWindowStructure.Populations[populationIndex], fromSymbolIndex, toSymbolIndex);
		}

		/// <summary>
		/// Check for <param name="fromSymbolIndexes"/> in a population and change it to <param name="toSymbolIndex"/>
		/// Gets the masks at symbol indexes <param name="fromSymbolIndexes"/> for the population at <param name="populationIndex"/> and OR's them into the
		/// destination symbol index <param name="toSymbolIndex"/>
		/// </summary>
		public static void ChangeSymbolIndexes(this IList<ReadOnlyMask> symbolMasks, SymbolWindowStructure symbolWindowStructure, int populationIndex, IReadOnlyList<int> fromSymbolIndexes, int toSymbolIndex)
		{
			symbolMasks.ChangeSymbolIndexes(symbolWindowStructure.Populations[populationIndex], fromSymbolIndexes, toSymbolIndex);
		}
	}
}