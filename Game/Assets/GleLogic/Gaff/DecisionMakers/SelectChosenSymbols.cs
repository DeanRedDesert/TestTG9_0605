using System;
using System.Collections.Generic;
using System.Linq;
using Gaff.Conditions;
using Gaff.Core.DecisionMakers;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Utility;

namespace Gaff.DecisionMakers
{
	/// <summary>
	/// Choose what symbols will be chosen
	/// </summary>
	public sealed class SelectChosenSymbols : DecisionMaker
	{
		public StringCondition ContextCondition { get; }
		public string Selections { get; }
		public int SymbolWindow { get; }

		public SelectChosenSymbols(StringCondition contextCondition, string selections, int symbolWindow)
		{
			ContextCondition = contextCondition;
			Selections = selections;
			SymbolWindow = symbolWindow;
		}

		public override DecisionOutcome Create(DecisionDefinition decisionData, object stateData)
		{
			var originalSelections = Selections.SplitAndTrim(" ");
			var indexedSelections = originalSelections.Where(s => s.Contains(':')).Select(s =>
			{
				var item = s.SplitAndTrim(":");
				return (Symbol: item[0], ChosenIndex: uint.Parse(item[1]));
			}).OrderBy(e => e.ChosenIndex).ToList();
			var countSelections = originalSelections.Where(s => s.Contains('?')).Select(s =>
			{
				var item = s.SplitAndTrim("?");
				return (Symbol: item[0], ChosenIndex: int.Parse(item[1]));
			}).ToList();

			var selections = originalSelections.Where(s => !s.Contains(':') && !s.Contains('?')).ToList();
			var indexCount = GetIndexCount(decisionData);
			var chosenCount = GetChosenCount(decisionData);
			var allowDuplicates = GetAllowDuplicates(decisionData);
			var getName = GetNameFunc(decisionData);
			var result = new ulong[chosenCount];
			var bagOfNumbers = new BigBagOfNumbers(indexCount, allowDuplicates, getName);
			var remainingChosenIndexes = new HashSet<uint>(Enumerable.Range(0, (int)chosenCount).Select(i => (uint)i));

			foreach (var indexedSelection in indexedSelections)
			{
				if (bagOfNumbers.Next(indexedSelection.Symbol, SymbolWindow, out var newIndex))
				{
					result[indexedSelection.ChosenIndex] = newIndex;
					remainingChosenIndexes.Remove(indexedSelection.ChosenIndex);
				}
				else
					return new DecisionOutcome($"Failed to find '{indexedSelection.Symbol}'");
			}

			selections = selections.Concat(countSelections.SelectMany(s => Enumerable.Repeat(s.Symbol, s.ChosenIndex))).ToList();

			foreach (var selection in selections)
			{
				if (remainingChosenIndexes.Count == 0)
					return new DecisionOutcome("Not enough chosen indexes to fulfil request.");

				if (bagOfNumbers.Next(selection, SymbolWindow, out var newIndex))
				{
					var nextIndex = remainingChosenIndexes.First();
					result[nextIndex] = newIndex;
					remainingChosenIndexes.Remove(nextIndex);
				}
				else
					return new DecisionOutcome($"Failed to find '{selection}'");
			}

			foreach (var remainingChosenIndex in remainingChosenIndexes)
			{
				if (bagOfNumbers.Next(out var newIndex))
					result[remainingChosenIndex] = newIndex;
				else
					return new DecisionOutcome("Not enough indexes to fulfil request.");
			}

			return new DecisionOutcome("Success.", true, new Decision(decisionData, result));
		}

		private static ulong GetIndexCount(DecisionDefinition decisionDefinition)
		{
			switch (decisionDefinition)
			{
				case WeightedIndexesDecision d: return d.IndexCount;
				case IndexesDecision d: return d.IndexCount;
				case PickIndexesDecision d: return d.IndexCount;
				case WeightsIndexesDecision d: return d.Weights.GetLength();
				default: throw new ArgumentOutOfRangeException(nameof(decisionDefinition));
			}
		}

		private static uint GetChosenCount(DecisionDefinition decisionDefinition)
		{
			switch (decisionDefinition)
			{
				case WeightedIndexesDecision d: return d.Count;
				case IndexesDecision d: return d.Count;
				case PickIndexesDecision d:
				{
					if (d.MinCount == d.MaxCount)
						return d.MinCount;
					var count = d.MaxCount - d.MinCount + 1;
					return d.MinCount + (uint)ConditionHelpers.NextULong(count);
				}
				case WeightsIndexesDecision d: return d.Count;
				default: throw new ArgumentOutOfRangeException(nameof(decisionDefinition));
			}
		}

		private static bool GetAllowDuplicates(DecisionDefinition decisionDefinition)
		{
			switch (decisionDefinition)
			{
				case WeightedIndexesDecision d: return d.AllowDuplicates;
				case IndexesDecision d: return d.AllowDuplicates;
				case PickIndexesDecision d: return d.AllowDuplicates;
				case WeightsIndexesDecision d: return d.AllowDuplicates;
				default: throw new ArgumentOutOfRangeException(nameof(decisionDefinition));
			}
		}

		private static Func<ulong, string> GetNameFunc(DecisionDefinition decisionDefinition)
		{
			switch (decisionDefinition)
			{
				case WeightedIndexesDecision d: return d.GetName;
				case IndexesDecision d: return d.GetName;
				case PickIndexesDecision d: return d.GetName;
				case WeightsIndexesDecision d: return d.GetName;
				default: throw new ArgumentOutOfRangeException(nameof(decisionDefinition));
			}
		}

		public override bool Valid(string context, Func<DecisionDefinition> decisionDefinition, ref object stateData)
		{
			var def = decisionDefinition();
			return ContextCondition.Check(context) && (def is WeightedIndexesDecision || def is IndexesDecision || def is PickIndexesDecision || def is WeightsIndexesDecision);
		}

		public override IResult ToString(string format)
			=> $"If context '{ContextCondition}' then check for the decision selections '{Selections}'{(SymbolWindow == 1 ? "." : $" within {SymbolWindow} stops.")}".ToSuccess();
	}
}