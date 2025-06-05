using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator;
using Logic.Core.Engine;
using Logic.Core.Types;
using Logic.Core.Types.Exits;
using Logic.Core.Types.WeightScaling;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

// ReSharper disable MemberCanBePrivate.Global - Methods are referenced via reflection
// ReSharper disable UnusedMember.Global - Methods are referenced via reflection

namespace Logic
{
	public static partial class ProcessorFunctions
	{
		public static string ToCurrentCycleId(Cycles cycles) => cycles.Current.CycleId;

		public static object Select(SelectorItems items, params object[] args) => items.Select(args);

		public static Credits ToCredits(Money money, Money denom) => money.ToCredits(denom);

		public static Money ToMoney(Credits credits, Money denom) => credits.ToMoney(denom);

		public static IReadOnlyList<CellPrizeResult> Merge(params IReadOnlyList<CellPrizeResult>[] prizes) => prizes.SelectMany(p => p).ToArray();

		[return: NullOut]
		public static string CheckForProgressives(ScopedDecisionGenerator decisionGenerator, WeightedTable<string> weightedProgressives)
		{
			var chosenIndex = decisionGenerator.ChooseOneIndex(weightedProgressives, i => weightedProgressives.Items[(int)i].Value);
			return weightedProgressives.Items[(int)chosenIndex].Value;
		}

		public static SymbolWindowStructure ToSymbolWindowStructure(IReadOnlyList<CellPopulation> cellPopulations) => new SymbolWindowStructure(cellPopulations);

		/// <summary>
		/// Process the symbol window results based of the provided patterns and prizes. Process the resultant prizes if pay best is true then return the prize results.
		/// </summary>
		public static IReadOnlyList<CellPrizeResult> EvaluateLines(SymbolWindowResult symbolWindow, Patterns patterns, IReadOnlyList<MaskPrize> prizes, int linesSelected = -1, bool payBest = true, ulong multiplier = 1)
		{
			linesSelected = linesSelected == -1 ? patterns.LinePatterns?.Count ?? 0 : linesSelected;

			if (linesSelected == 0)
				return Array.Empty<CellPrizeResult>();

			var prizeResults = payBest ? (IList<CellPrizeResult>)new CellPrizeResult[linesSelected] : new List<CellPrizeResult>();
			var action = payBest ? (Action<IList<CellPrizeResult>, MaskPrize, int, Pattern, IReadOnlyList<Cell>>)LineEvaluation.ProcessPayBest : LineEvaluation.ProcessPayMany;
			var prizesCount = prizes.Count;

			for (var p = 0; p < prizesCount; p++)
			{
				var prize = prizes[p];
				var shouldPrizePay = prize.RequiredSymbolCounts != null ? (Func<SymbolWindowResult, MaskPrize, IReadOnlyList<Cell>, bool>)EvaluationHelper.CheckAddPrize : null;

				switch (prize.Strategy)
				{
					case PrizeStrategy.Left:
					{
						LineEvaluation.ProcessSimplePayFunc(symbolWindow, patterns, linesSelected, prize, prizeResults, action, LineEvaluation.GetWinningCellsLeft, LineEvaluation.EarlyOutLeft, shouldPrizePay);
						break;
					}
					case PrizeStrategy.Right:
					{
						LineEvaluation.ProcessSimplePayFunc(symbolWindow, patterns, linesSelected, prize, prizeResults, action, LineEvaluation.GetWinningCellsRight, LineEvaluation.EarlyOutRight, shouldPrizePay);
						break;
					}
					case PrizeStrategy.Any:
					{
						LineEvaluation.ProcessSimplePayFunc(symbolWindow, patterns, linesSelected, prize, prizeResults, action, LineEvaluation.GetWinningCellsAny, null, shouldPrizePay);
						break;
					}
					case PrizeStrategy.Both:
					{
						LineEvaluation.ProcessPayBothFunc(symbolWindow, patterns, linesSelected, prize, prizeResults, action, payBest, shouldPrizePay);
						break;
					}
					default:
						throw new NotSupportedException();
				}
			}

			if (payBest)
			{
				var winningPatterns = new List<CellPrizeResult>();
				var prizeResultsCount = prizeResults.Count;

				for (var i = 0; i < prizeResultsCount; i++)
				{
					var pr = prizeResults[i];

					if (pr != null)
						winningPatterns.Add(pr);
				}

				prizeResults = winningPatterns;
			}

			return multiplier == 1
				? (IReadOnlyList<CellPrizeResult>)prizeResults
				: ApplyMultiplier((IReadOnlyList<CellPrizeResult>)prizeResults, multiplier);
		}

		/// <summary>
		/// Process the symbol window results based of the provided patterns and prizes then return the prize results.
		/// </summary>
		public static IReadOnlyList<CellPrizeResult> EvaluateScatters(SymbolWindowResult symbolWindow, Patterns patterns, IReadOnlyList<MaskPrize> prizes, bool payBest = true, ulong multiplier = 1)
		{
			// If the patterns have been auto converted to line then use the line evaluation.
			var prizeResults = patterns.ClusterPatterns != null
				? ScatterEvaluation.Evaluate(symbolWindow, patterns, prizes, payBest)
				: EvaluateLines(symbolWindow, patterns, prizes, -1, false);

			return multiplier == 1
				? prizeResults
				: ApplyMultiplier(prizeResults, multiplier);
		}

		/// <summary>
		/// Process the symbol window results based of the provided patterns and prizes then return the prize results.
		/// </summary>
		public static IReadOnlyList<CellPrizeResult> EvaluateWays(SymbolWindowResult symbolWindow, Patterns patterns, IReadOnlyList<MaskPrize> prizes, ulong multiplier = 1)
		{
			// If the patterns have been auto converted to line then use the line evaluation.
			if (patterns.ClusterPatterns == null)
			{
				var prizeResults = EvaluateLines(symbolWindow, patterns, prizes, -1, false);
				return multiplier != 1 ? ApplyMultiplier(prizeResults, multiplier) : prizeResults;
			}

			var winningPrizes = new List<CellPrizeResult>();

			for (var p = 0; p < prizes.Count; p++)
			{
				var prize = prizes[p];
				switch (prize.Strategy)
				{
					case PrizeStrategy.Left: WaysEvaluation.ProcessPayLeft(symbolWindow, patterns, prize, winningPrizes); break;
					case PrizeStrategy.Any: WaysEvaluation.ProcessPayAny(symbolWindow, patterns, prize, winningPrizes); break;
					case PrizeStrategy.Right:
					case PrizeStrategy.Both:
					default: throw new NotImplementedException();
				}
			}

			return multiplier != 1
				? ApplyMultiplier(winningPrizes, multiplier)
				: winningPrizes;
		}

		/// <summary>
		/// Generate a random symbol window result.
		/// </summary>
		public static SymbolWindowResult CreateSymbolWindow(ScopedDecisionGenerator decisionGenerator, SymbolWindowStructure symbolWindowStructure, IReadOnlyList<ISymbolListStrip> strips)
		{
			var stripsCount = strips.Count;
			var chosenStripIndexes = new ulong[stripsCount];

			for (var i = 0; i < stripsCount; i++)
			{
				var strip = strips[i];
				var ii = i + 1;
				chosenStripIndexes[i] = decisionGenerator.ChooseOneIndex(strip, strip.GetSymbol, () => $"Reel{ii}");
			}

			return strips.CreateSymbolWindowResult(symbolWindowStructure, chosenStripIndexes);
		}

		/// <summary>
		/// Generate a random locked symbol window result.
		/// </summary>
		public static LockedSymbolWindowResult CreateLockedSymbolWindow(ScopedDecisionGenerator decisionGenerator, SymbolWindowStructure symbolWindowStructure, IReadOnlyList<ISymbolListStrip> strips, ILockData lockData)
		{
			var chosenStripIndexes = new ulong[strips.Count];
			var lockMask = lockData.GetLockMask();

			for (var i = 0; i < strips.Count; i++)
			{
				if (lockMask[i])
					continue;

				var strip = strips[i];
				var ii = i + 1;
				chosenStripIndexes[i] = decisionGenerator.ChooseOneIndex(strip, strip.GetSymbol, () => $"Reel{ii}");
			}

			return strips.CreateLockedSymbolWindowResult(symbolWindowStructure, chosenStripIndexes, lockData);
		}

		public static SchemafyResult SchemafySymbolWindow(ScopedDecisionGenerator decisionGenerator, SymbolWindowResult symbolWindow, SchemaRuntimeData schemaRuntime)
		{
			var selectedReplacementIndex = decisionGenerator.ChooseOneIndex(schemaRuntime.ReplacementStrip, schemaRuntime.ReplacementStrip.GetSymbol);
			var selectedReplacement = schemaRuntime.SchemafySymbolIndexes[(int)selectedReplacementIndex];
			var populationCount = symbolWindow.SymbolWindowStructure.Populations.Count;

			if (selectedReplacement.Count != populationCount)
				throw new Exception("The selected ReplacementSymbols count does not match the CellPopulations count.");

			if (schemaRuntime.SymbolIndexes.Count != populationCount)
				throw new Exception("The SymbolsToReplace count does not match the CellPopulations count.");

			var symbolMasks = new List<ReadOnlyMask>(symbolWindow.SymbolMasks);

			for (var i = 0; i < populationCount; i++)
				symbolMasks.ChangeSymbolIndex(symbolWindow.SymbolWindowStructure, i, schemaRuntime.SymbolIndexes[i], schemaRuntime.SchemafySymbolIndexes[(int)selectedReplacementIndex][i]);

			return new SchemafyResult(symbolWindow.CreateSymbolWindowResult(symbolMasks), new SchemaData(schemaRuntime.SourceData.SymbolsToReplace, schemaRuntime.SourceData.SchemaEntries[(int)selectedReplacementIndex].ReplacementSymbols));
		}

		public static SymbolWindowResult SmashSymbolOverPopulation(SymbolWindowResult symbolWindow, string symbol)
		{
			var symbolIndex = symbolWindow.SymbolList.IndexOf(symbol); // TODO make an optimised version of this inside the symbol window result
			var symbolMask = symbolWindow.SymbolMasks[symbolIndex];

			// No target symbols we return the original result.
			if (symbolMask == null)
				return symbolWindow;

			ReadOnlyMask newMask = null;

			// Loop through each population mask and if the target symbol is found add the whole populations positions to the new mask.
			for (var i = 0; i < symbolWindow.SymbolWindowStructure.Populations.Count; i++)
			{
				if (symbolWindow.SymbolWindowStructure.Populations[i].AndNotEmpty(symbolMask))
				{
					newMask = newMask == null
						? symbolWindow.SymbolWindowStructure.Populations[i]
						: newMask.Or(symbolWindow.SymbolWindowStructure.Populations[i]);
				}
			}

			// If the new mask is never modified then we return the original result.
			if (newMask == null)
				return symbolWindow;

			// Create a new collection of the original symbol masks.

			var symbolMasks = new ReadOnlyMask[symbolWindow.SymbolMasks.Count];

			// Loop through each symbol mask clearing any positions that were in the populations that were found to have the target symbol.
			for (var i = 0; i < symbolWindow.SymbolMasks.Count; i++)
			{
				// The new symbol mask for the target symbol.
				if (i == symbolIndex)
					symbolMasks[i] = newMask;
				// Remove any positions that were in the populations the became the target symbol.
				else if (symbolWindow.SymbolMasks[i] != null)
					symbolMasks[i] = symbolWindow.SymbolMasks[i].And(newMask.Not());
			}

			return symbolWindow.CreateSymbolWindowResult(symbolMasks);
		}

		private static IReadOnlyList<CellPrizeResult> ApplyMultiplier(IReadOnlyList<CellPrizeResult> prizes, ulong multiplier)
		{
			var newPrizes = new List<CellPrizeResult>();

			foreach (var p in prizes)
				newPrizes.Add(p.CloneWithValue(checked((int)((ulong)p.Value * multiplier))));

			return newPrizes;
		}

		public static IReadOnlyList<CellPrizeResult> ApplySymbolMultiplier(IReadOnlyList<CellPrizeResult> prizes, SymbolWindowResult symbolWindow, SymbolMultiplierDetails symbolMultiplierDetails)
		{
			return ApplySymbolMultiplier(prizes, symbolWindow, symbolMultiplierDetails.SymbolMultipliers, symbolMultiplierDetails.MultiplierUsage, symbolMultiplierDetails.ExcludedPrizes);
		}

		public static IReadOnlyList<CellPrizeResult> ApplySymbolMultiplier(IReadOnlyList<CellPrizeResult> prizes, SymbolWindowResult symbolWindow, IReadOnlyList<SymbolMultiplier> symbolMultipliers, MultiplierUsage multiplierUsage, IReadOnlyList<string> excludedPrizes = null)
		{
			var structure = symbolWindow.SymbolWindowStructure;
			var symbolList = symbolWindow.SymbolList;
			var processedPrizes = new List<CellPrizeResult>();

			foreach (var prize in prizes)
			{
				// Apply any symbol multipliers to the prize.

				if (excludedPrizes != null && excludedPrizes.Contains(prize.Name))
				{
					processedPrizes.Add(prize);
					continue;
				}

				var winningSymbolsMask = structure.CellsToMask(prize.WinningMask);
				var multiplier = multiplierUsage == MultiplierUsage.Add ? 0.0 : 1.0;
				var activated = false;

				for (var i = 0; i < symbolMultipliers.Count; i++)
				{
					var symbolIndex = symbolList.IndexOf(symbolMultipliers[i].SymbolName);
					var relevantMask = winningSymbolsMask.And(symbolWindow.SymbolMasks[symbolIndex]);

					if (relevantMask.IsEmpty)
						continue;

					activated = true;

					switch (multiplierUsage)
					{
						case MultiplierUsage.Best:
						{
							multiplier = symbolMultipliers[i].Multiplier > multiplier ? symbolMultipliers[i].Multiplier : multiplier;
							break;
						}
						case MultiplierUsage.Add:
						{
							multiplier += symbolMultipliers[i].Multiplier * relevantMask.TrueCount;
							break;
						}
						case MultiplierUsage.Multiply:
						{
							multiplier *= Math.Pow(symbolMultipliers[i].Multiplier, relevantMask.TrueCount);
							break;
						}
						default:
							throw new NotSupportedException();
					}
				}

				processedPrizes.Add(activated ? prize.CloneWithValue((int)(prize.Value * multiplier)) : prize);
			}

			return processedPrizes.ToList();
		}

		/// <summary>
		/// Checks a prize list for triggers and generates a trigger cycle modifier using the cycle id.
		/// </summary>
		/// <param name="prizes">The collection of prizes to check.</param>
		/// <param name="triggers">The trigger data.</param>
		/// <param name="triggerPosition">Where to insert the trigger in the cycles.</param>
		/// <returns>A collection of triggers and prizes for the triggers.</returns>
		public static TriggerCycleSetsResult CheckForPrizeTriggers(IReadOnlyList<CellPrizeResult> prizes, IReadOnlyList<PrizeCountWithCycleSet> triggers, TriggerPosition triggerPosition = TriggerPosition.AtEnd)
		{
			var exits = new List<ICyclesModifier>();
			var resultSource = new List<CellPrizeMapping>();

			for (var i = 0; i < prizes.Count; i++)
			{
				var prize = prizes[i];
				for (var triggerIndex = 0; triggerIndex < triggers.Count; triggerIndex++)
				{
					var trigger = triggers[triggerIndex];
					if (trigger.PrizeName != prize.Name || trigger.Count != prize.Count)
						continue;

					var exit = new Trigger(trigger.CycleCountToAward, trigger.CycleId, triggerPosition);
					exits.Add(exit);
					resultSource.Add(new CellPrizeMapping(prize, trigger));
				}
			}

			return new TriggerCycleSetsResult(exits, resultSource);
		}

		/// <summary>
		/// Checks a prize list for triggers and generates a trigger (or retrigger) cycle modifier using the cycle id.
		/// </summary>
		/// <param name="prizes">The collection of prizes to check.</param>
		/// <param name="triggers">The trigger data.</param>
		/// <param name="retrigger">If set to true, the <see cref="Retrigger"/> cycle modifier is used instead of Trigger.</param>
		/// <returns>A collection of triggers and prizes for the triggers.</returns>
		public static TriggerCycleSetsResult CheckForPrizeTriggers(IReadOnlyList<CellPrizeResult> prizes, IReadOnlyList<PrizeCountWithCycleSet> triggers, bool retrigger)
		{
			var exits = new List<ICyclesModifier>();
			var resultSource = new List<CellPrizeMapping>();

			for (var i = 0; i < prizes.Count; i++)
			{
				var prize = prizes[i];
				for (var triggerIndex = 0; triggerIndex < triggers.Count; triggerIndex++)
				{
					var trigger = triggers[triggerIndex];
					if (trigger.PrizeName != prize.Name || trigger.Count != prize.Count)
						continue;

					var exit = retrigger
						? (ICyclesModifier)new Retrigger(trigger.CycleCountToAward, trigger.CycleId)
						: new Trigger(trigger.CycleCountToAward, trigger.CycleId);

					exits.Add(exit);
					resultSource.Add(new CellPrizeMapping(prize, trigger));
				}
			}

			return new TriggerCycleSetsResult(exits, resultSource);
		}

		/// <summary>
		/// Checks a prize list for triggers and generates a trigger or retrigger cycle modifier. The cycle id parameter is only used for the trigger case. The retrigger parameter
		/// causes a <see cref="Retrigger"/> cycle modifier to be created using the current cycle id.
		/// </summary>
		/// <param name="prizes">The collection of prizes to check.</param>
		/// <param name="triggers">The trigger data.</param>
		/// <param name="cycles">The current cycles.</param>
		/// <param name="triggerCycleId">The cycle id to use for a trigger. Not used when retrigger is set to true.</param>
		/// <param name="retrigger">If set to true, the <see cref="Retrigger"/> cycle modifier is used instead of Trigger. This also uses the current cycle id</param>
		/// <param name="triggerPosition">Where to insert the trigger in the cycles.</param>
		/// <returns>A collection of triggers and prizes for the triggers.</returns>
		public static TriggerCycleSetsResult CheckForPrizeTriggers(IReadOnlyList<CellPrizeResult> prizes, IReadOnlyList<PrizeCount> triggers, Cycles cycles, string triggerCycleId = "", bool retrigger = false, TriggerPosition triggerPosition = TriggerPosition.AtEnd)
		{
			var exits = new List<ICyclesModifier>();
			var resultSource = new List<CellPrizeMapping>();

			for (var i = 0; i < prizes.Count; i++)
			{
				var prize = prizes[i];
				for (var triggerIndex = 0; triggerIndex < triggers.Count; triggerIndex++)
				{
					var trigger = triggers[triggerIndex];
					if (trigger.PrizeName != prize.Name || trigger.Count != prize.Count)
						continue;

					var exit = retrigger
						? (ICyclesModifier)new Retrigger(trigger.CycleCountToAward, cycles.Current.CycleId, triggerPosition)
						: new Trigger(trigger.CycleCountToAward, triggerCycleId, triggerPosition);
					exits.Add(exit);
					resultSource.Add(new CellPrizeMapping(prize, trigger));
				}
			}

			return new TriggerCycleSetsResult(exits, resultSource);
		}

		/// <summary>
		/// Checks a prize list for progressives.
		/// </summary>
		/// <param name="prizes">The collection of prizes to check.</param>
		/// <param name="triggers">The trigger data.</param>
		/// <returns>A collection of progressive triggers.</returns>
		public static IReadOnlyList<ProgressivePrizeResult> CheckForProgressive(IReadOnlyList<CellPrizeResult> prizes, IReadOnlyList<PrizeCountWithProgressiveLevel> triggers)
		{
			var progressives = new List<ProgressivePrizeResult>();

			for (var i = 0; i < prizes.Count; i++)
			{
				var prize = prizes[i];
				for (var triggerIndex = 0; triggerIndex < triggers.Count; triggerIndex++)
				{
					var trigger = triggers[triggerIndex];
					if (trigger.PrizeName == prize.Name && trigger.Count == prize.Count)
						progressives.Add(new ProgressivePrizeResult(prize.Name, prize.Count, trigger.ProgressiveIdentifier, prize.Pattern, prize.WinningMask));
				}
			}

			return progressives;
		}

		/// <summary>
		/// Apply replacements from <param name="prizeReplacementData"/> using <param name="scalingFactors"></param>.
		/// </summary>
		/// <param name="decisionGenerator">The decision generator ti use when choosing weights for each replacement.</param>
		/// <param name="symbolWindow">The source symbol window result.</param>
		/// <param name="prizeReplacementData">The replacement data.</param>
		/// <param name="scalingFactors">The bet related scaling data.</param>
		/// <param name="previousReplacements">The list of previous replacements made.</param>
		/// <returns></returns>
		public static ApplyReplacementsResult ApplyReplacementsToSymbolWindow(ScopedDecisionGenerator decisionGenerator, SymbolWindowResult symbolWindow, IReadOnlyList<(int, PrizeReplacerEntry)> prizeReplacementData, IReadOnlyDictionary<ScalingMethod, ulong> scalingFactors, IReadOnlyDictionary<int, ReadOnlyMask> previousReplacements = null)
		{
			var symbolWindowStructure = symbolWindow.SymbolWindowStructure;
			var symbolList = symbolWindow.SymbolList;
			var cells = symbolWindowStructure.Cells;
			var cellCount = cells.Count;
			var newSymbolMasks = symbolWindow.SymbolMasks.ToArray();

			if (previousReplacements == null)
				previousReplacements = new Dictionary<int, ReadOnlyMask>();

			foreach (var kvp in previousReplacements)
				newSymbolMasks.AddToSymbolPositionMask(kvp.Key, kvp.Value);

			foreach (var table in prizeReplacementData)
			{
				// If the table contains no entries then skip it.

				var symbolToReplaceIndex = table.Item1;
				var symbolMask = newSymbolMasks[symbolToReplaceIndex];

				if (symbolMask.IsEmpty)
					continue;

				newSymbolMasks[symbolToReplaceIndex] = ReadOnlyMask.CreateAllFalse(cells.Count);

				var indexesToReplace = (IReadOnlyList<int>)symbolMask.EnumerateIndexes().ToArray();

				var entries = table.Item2.PrizeTable;
				var distributorEntries = table.Item2.DistributorInfo;
				var newWeights = WeightDistributorHelper.CreateScaledWeights(entries, distributorEntries, scalingFactors);

				var chosenItems = decisionGenerator.ChooseIndexes(entries.GetLength(), (uint)indexesToReplace.Count, true, e => newWeights[(int)e], entries.GetTotalWeight(), e => entries.GetId(e), () => $"{symbolToReplaceIndex}_Stuff");

				var newReplacementSymbolMasks = previousReplacements.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

				for (var i = 0; i < indexesToReplace.Count; i++)
				{
					var replacementSymbolIndex = symbolList.IndexOf(entries.GetId(chosenItems[i]));
					var replacementSymbolMask = ReadOnlyMask.CreateFromIndexes(cellCount, new[] { indexesToReplace[i] });

					newSymbolMasks[replacementSymbolIndex] = newSymbolMasks[replacementSymbolIndex].Or(replacementSymbolMask);
					newReplacementSymbolMasks[replacementSymbolIndex] = newReplacementSymbolMasks.TryGetValue(replacementSymbolIndex, out var mask) ? mask.Or(replacementSymbolMask) : replacementSymbolMask;
				}

				previousReplacements = newReplacementSymbolMasks;
			}

			return new ApplyReplacementsResult(new SymbolWindowResult(symbolWindow.SymbolList, symbolWindow.SymbolWindowStructure, newSymbolMasks), previousReplacements);
		}
	}
}