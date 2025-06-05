using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator;
using Logic.Core.Engine;
using Logic.Core.Types;
using Logic.Core.Types.Exits;
using Logic.Core.WinCheck;
using Logic.Types;

// ReSharper disable UnusedMember.Global

namespace Logic
{
	/// <summary>
	/// All the custom processes required for the cash ching feature.
	/// </summary>
	public static partial class ProcessorFunctions
	{
		public static WeightedSet WeightedSetSelect(WeightedSets weightedSets, Money denom, string percentage, Credits linesBet, long betMultiplier) => weightedSets.GetItem(denom, betMultiplier, linesBet, percentage);

		public static string SetChoice(ScopedDecisionGenerator decisionGenerator, WeightedSet weightedSet)
		{
			var decision = decisionGenerator.GetDecision(weightedSet.Set1Weight, weightedSet.Set2Weight);
			return decision ? "SET1" : "SET2";
		}

		public static int ToReplacementSymbol(GeneralData generalData, bool isRespin) => isRespin ? generalData.TargetSymbolIndexForRespin : generalData.TargetSymbolIndexForBaseAndFree;

		public static IReadOnlyList<ISymbolListStrip> StripSymbolReplacement(ScopedDecisionGenerator decisionGenerator, IReadOnlyList<ISymbolListStrip> strips, string setChoice, ReplacementTables replacementTables, int symbolIndex)
		{
			IReadOnlyList<ReplacementTable> noFrameSetReplacements;
			ulong twNoFrame;

			if (setChoice == "SET1")
				replacementTables.GetTotalWeightSet1(out _, out noFrameSetReplacements, out _, out _, out twNoFrame, out _);
			else
				replacementTables.GetTotalWeightSet2(out _, out noFrameSetReplacements, out _, out _, out twNoFrame, out _);

			return strips.CreateDirectIndependentUsingArray(symbolIndex,
				(stripIndex, totalReplacementsFound) =>
				{
					var chosenIndexes = decisionGenerator.ChooseIndexes((ulong)noFrameSetReplacements.Count, (uint)totalReplacementsFound, true, r => noFrameSetReplacements[(int)r].NoFrame, twNoFrame, item => noFrameSetReplacements[(int)item].SymbolName, () => new[] { $"Strip_{totalReplacementsFound}_{stripIndex}" });
					var replacementSymbolIndexes = new int[chosenIndexes.Count];

					for (var i = 0; i < chosenIndexes.Count; i++)
						replacementSymbolIndexes[i] = noFrameSetReplacements[(int)chosenIndexes[i]].SymbolIndex;

					return replacementSymbolIndexes;
				});
		}

		public static IReadOnlyList<ISymbolListStrip> RespinStripSymbolReplacement(ScopedDecisionGenerator decisionGenerator, RespinState respinState, IReadOnlyList<ISymbolListStrip> strips, string setChoice, ReplacementTables replacementTables, int symbolIndex)
		{
			int[] DoReplacement(IReadOnlyList<ReplacementTable> table, ulong totalWeight, int count, int stripIndex, string desc, Func<ReplacementTable, ulong> getWeight)
			{
				var selectedPrizes = decisionGenerator.ChooseIndexes((ulong)table.Count, (uint)count, true, r => getWeight(table[(int)r]), totalWeight, item => table[(int)item].SymbolName, () => new[] { $"Strip_{desc}_{stripIndex}" });
				var replacements = new int[count];
				for (var i = 0; i < count; i++)
					replacements[i] = table[(int)selectedPrizes[i]].SymbolIndex;
				return replacements;
			}

			IReadOnlyList<ReplacementTable> noFrameJpSetReplacements;
			IReadOnlyList<ReplacementTable> noFrameSetReplacements;
			IReadOnlyList<ReplacementTable> frameSetReplacements;
			ulong twNoFrameJp;
			ulong twNoFrame;
			ulong twFrame;

			if (setChoice == "SET1")
				replacementTables.GetTotalWeightSet1(out frameSetReplacements, out noFrameSetReplacements, out noFrameJpSetReplacements, out twFrame, out twNoFrame, out twNoFrameJp);
			else
				replacementTables.GetTotalWeightSet2(out frameSetReplacements, out noFrameSetReplacements, out noFrameJpSetReplacements, out twFrame, out twNoFrame, out twNoFrameJp);

			var lockedCells = respinState.GetLockMask() ?? ReadOnlyMask.CreateAllFalse(respinState.Frames.BitLength);
			var firstNonFrame = true;

			return strips.ProcessIndependent(symbolIndex, (stripIndex, symbolStopCount) =>
			{
				if (lockedCells[stripIndex])
					return Array.Empty<int>();

				var isFrameStrip = respinState.Frames[stripIndex];

				if (isFrameStrip)
					return DoReplacement(frameSetReplacements, twFrame, symbolStopCount, stripIndex, "Frame", r => r.Frame);

				if (firstNonFrame)
				{
					firstNonFrame = false;
					return DoReplacement(noFrameJpSetReplacements, twNoFrameJp, symbolStopCount, stripIndex, "NoFrameJP", r => r.NoFrameJp);
				}

				return DoReplacement(noFrameSetReplacements, twNoFrame, symbolStopCount, stripIndex, "NoFrame", r => r.NoFrame);
			}, (stripIndex, indexList, replacements) => indexList.CreateStripWithReplacement(replacements));
		}

		public static RespinDetectorResult EvaluateRespinTrigger(SymbolWindowResult symbolWindow, Patterns patterns, IReadOnlyList<RespinPrize> respinPrizes, int triggerCount, ulong multiplier, int respinCount, string cycleId)
		{
			var symbolCount = ReadOnlyMask.CreateAllFalse(symbolWindow.SymbolWindowStructure.Cells.Count);
			var prizes = new List<CellPrizeResult>();
			var bonusPrizes = new List<CellPrizeResult>();

			foreach (var respinPrize in respinPrizes)
			{
				var symbolMask = symbolWindow.SymbolMasks[respinPrize.SymbolIndex];

				if (symbolMask == null)
					continue;

				symbolCount = symbolCount.Or(symbolMask);

				foreach (var index in symbolMask.EnumerateIndexes())
				{
					if (respinPrize.IsProgressive)
						throw new NotSupportedException("Progressive prizes in Base and Free are not allowed");

					if (respinPrize.IsBonus)
						bonusPrizes.Add(new CellPrizeResult(respinPrize.PrizeName, 1, (int)respinPrize.Value, patterns.GetSourcePattern(index), new[] { symbolWindow.SymbolWindowStructure.Cells[index] }));
					else
						prizes.Add(new CellPrizeResult(respinPrize.PrizeName, 1, (int)(respinPrize.Value * multiplier), patterns.GetSourcePattern(index), new[] { symbolWindow.SymbolWindowStructure.Cells[index] }));
				}
			}

			return symbolCount.TrueCount < triggerCount
				? new RespinDetectorResult(null, null, Array.Empty<CellPrizeResult>(), Array.Empty<CellPrizeResult>())
				: new RespinDetectorResult(new Respin(respinCount, cycleId), new RespinState(symbolCount, null), prizes, bonusPrizes);
		}

		public static RespinResult EvaluateRespin(Cycles cycles, SymbolWindowResult symbolWindow, Patterns patterns, IReadOnlyList<RespinPrize> respinPrizes, RespinState respinState, ulong totalBet, int respinCount)
		{
			var empty = ReadOnlyMask.CreateAllFalse(symbolWindow.SymbolWindowStructure.Cells.Count);

			var grandSymbolIndex = respinPrizes.Single(rp => rp.SymbolName == "GRAND").SymbolIndex;
			var majorSymbolIndex = respinPrizes.Single(rp => rp.SymbolName == "MAJOR").SymbolIndex;

			var prizes = new List<CellPrizeResult>();
			var bonusPrizes = new List<CellPrizeResult>();
			var progressivePrizes = new List<ProgressivePrizeResult>();
			var lockedCells = respinState.GetLockMask();

			var cellsOfInterest = empty;

			// Create locked data for each symbol.

			var updateLockedCells = CloneLockedSymbols(symbolWindow, empty, respinState);

			// Get a mask of all the cells that have a required symbol.

			foreach (var prize in respinPrizes)
			{
				var symbolMask = symbolWindow.SymbolMasks[prize.SymbolIndex];
				if (symbolMask == null)
					continue;

				cellsOfInterest = cellsOfInterest.Or(symbolMask);

				// Update the locked cells for credit prizes.

				updateLockedCells[prize.SymbolIndex] = updateLockedCells[prize.SymbolIndex].Or(symbolMask.And(respinState.Frames.Not()));
			}

			// Update cells with frames.

			var frames = respinState.Frames;
			var frameSymbolIndex = symbolWindow.SymbolList.IndexOf("FRAME");
			var frameMask = symbolWindow.SymbolMasks[frameSymbolIndex];

			if (frameMask != null)
			{
				frames = frames.Or(frameMask);
				cellsOfInterest = cellsOfInterest.Or(frameMask);
			}

			// Update respin count.

			var hit = cellsOfInterest.AndNotEmpty(lockedCells == null ? empty.Not() : lockedCells.Not());
			var isFinished = !hit && cycles.Current.TotalCycles - cycles.Current.CompletedCycles == 1;

			for (var i = 0; i < cellsOfInterest.BitLength; i++)
			{
				if (!cellsOfInterest[i])
					continue;

				var symbolIndex = symbolWindow.GetSymbolIndexAt(i);

				// Ignore Frame Symbols.

				if (symbolIndex == frameSymbolIndex)
					continue;

				// Pay the prize if landed in a frame, respins are finished or a progressive.

				if (!(lockedCells == null ? empty : lockedCells)[i] && (symbolIndex == majorSymbolIndex || symbolIndex == grandSymbolIndex))
				{
					var symbolName = symbolWindow.SymbolList[symbolIndex];
					var prize = respinPrizes.First(pm => pm.SymbolName == symbolName);
					progressivePrizes.Add(new ProgressivePrizeResult(prize.PrizeName, 1, prize.PrizeName, patterns.GetSourcePattern(i), new[] { symbolWindow.SymbolWindowStructure.Cells[i] }));
				}
				else if (frames[i] || isFinished)
				{
					var symbolName = symbolWindow.SymbolList[symbolIndex];
					var prize = respinPrizes.First(pm => pm.SymbolName == symbolName);
					if (prize.IsProgressive)
					{
						// Do nothing as progressives are paid immediately.
					}
					else if (prize.IsBonus)
					{
						bonusPrizes.Add(new CellPrizeResult(prize.PrizeName, 1, (int)prize.Value, patterns.GetSourcePattern(i), new[] { symbolWindow.SymbolWindowStructure.Cells[i] }));
					}
					else
					{
						prizes.Add(new CellPrizeResult(prize.PrizeName, 1, (int)(prize.Value * totalBet), patterns.GetSourcePattern(i), new[] { symbolWindow.SymbolWindowStructure.Cells[i] }));
					}
				}
			}

			ICyclesModifier retrigger = hit ? new RespinReset(respinCount) : null;
			return new RespinResult(retrigger, new RespinState(frames, updateLockedCells), prizes, bonusPrizes, progressivePrizes);
		}

		public static IReadOnlyList<ISymbolListStrip> RespinStripProvider(IReadOnlyList<ISymbolListStrip> noFrameStrips, IReadOnlyList<ISymbolListStrip> frameStrips, RespinState respinState)
		{
			var framesMask = respinState.Frames;
			var lockedCells = respinState.GetLockMask();

			var strips = new List<ISymbolListStrip>();

			var frameIndex = 0;
			var nonFrameIndex = framesMask.TrueCount;

			// Go through the intra data train and figure out frame cells, locked cells and open cells.

			for (var i = 0; i < noFrameStrips.Count; i++)
			{
				if (framesMask[i])
				{
					strips.Add(frameStrips[frameIndex]);
					frameIndex++;
				}
				else if (lockedCells != null && lockedCells[i])
				{
					// Use the first non frame strip as a placeholder.
					strips.Add(noFrameStrips[0]);
				}
				else
				{
					strips.Add(noFrameStrips[nonFrameIndex]);
					nonFrameIndex++;
				}
			}

			return strips;
		}

		private static List<ReadOnlyMask> CloneLockedSymbols(SymbolWindowResult symbolWindow, ReadOnlyMask empty, RespinState respinStateValue)
		{
			List<ReadOnlyMask> updateLockedCells;
			if (respinStateValue.Locked == null)
			{
				updateLockedCells = new List<ReadOnlyMask>();
				for (var i = 0; i < symbolWindow.SymbolList.Count; i++)
					updateLockedCells.Add(empty);
			}
			else
			{
				updateLockedCells = new List<ReadOnlyMask>(respinStateValue.Locked);
			}

			return updateLockedCells;
		}
	}
}