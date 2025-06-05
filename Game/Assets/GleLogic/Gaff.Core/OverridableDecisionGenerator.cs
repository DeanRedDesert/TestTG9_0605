using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.WinCheck;

namespace Gaff.Core
{
	/// <summary>
	/// Records any decisions made using the specified decision generator to provide the values stored with the DecisionDefinition objects.
	/// A GetDecision uses a provided decision result if found otherwise calls off to the result().
	/// </summary>
	public sealed class OverridableDecisionGenerator : AlternateDecisionGenerator
	{
		private readonly Dictionary<string, Decision> decisionLookup;

		public OverridableDecisionGenerator(IDecisionGenerator decisionGenerator, IReadOnlyList<Decision> overrides)
			: base(decisionGenerator)
		{
			decisionLookup = overrides.ToDictionary(kv => kv.DecisionDefinition.Context, kv => kv);
		}

		protected override Decision GetDecision<T>(string context, Func<T> decisionDefinition, Func<object> result)
		{
			var dd = decisionDefinition();
			Decision decisionResult;

			// Check for an existing decision and check to see if its structure (number of items, counts, etc.) is compatible with the new decision.
			// If so use the current result data with the new decision definition to ensure the correct data is available.

			if (decisionLookup.TryGetValue(context, out var lookUp) && AreDecisionsEquivalent(dd, lookUp.DecisionDefinition))
				decisionResult = new Decision(dd, lookUp.Result);
			else
				decisionResult = new Decision(dd, result());

			return decisionResult;
		}

		private static bool AreDecisionsEquivalent(DecisionDefinition decision1, DecisionDefinition decision2)
		{
			if (ReferenceEquals(decision1, decision2))
				return true;

			if (ReferenceEquals(decision1, null))
				return false;

			if (ReferenceEquals(decision2, null))
				return false;

			if (decision1.Context != decision2.Context)
				return false;

			if (decision1.GetType() != decision2.GetType())
				return false;

			switch (decision1)
			{
				case SimpleDecision d1: return AreEquivalent(d1, (SimpleDecision)decision2);
				case WeightedIndexesDecision d1: return AreEquivalent(d1, (WeightedIndexesDecision)decision2);
				case IndexesDecision d1: return AreEquivalent(d1, (IndexesDecision)decision2);
				case PickIndexesDecision d1: return AreEquivalent(d1, (PickIndexesDecision)decision2);
				case WeightsIndexesDecision d1: return AreEquivalent(d1, (WeightsIndexesDecision)decision2);
				default: throw new NotSupportedException();
			}
		}

		private static bool AreEquivalent(SimpleDecision d1, SimpleDecision d2)
			=> d1.TrueWeight == d2.TrueWeight && d1.FalseWeight == d2.FalseWeight;

		private static bool AreEquivalent(IndexesDecision d1, IndexesDecision d2)
			=> d1.IndexCount == d2.IndexCount && d1.Count == d2.Count && d1.AllowDuplicates == d2.AllowDuplicates;

		private static bool AreEquivalent(PickIndexesDecision d1, PickIndexesDecision d2)
			=> d1.IndexCount == d2.IndexCount && d1.MinCount == d2.MinCount && d1.MaxCount == d2.MaxCount && d1.AllowDuplicates == d2.AllowDuplicates;

		private static bool AreEquivalent(WeightedIndexesDecision d1, WeightedIndexesDecision d2)
			=> AreEquivalent((IndexesDecision)d1, d2) && d1.TotalWeight == d2.TotalWeight;

		private static bool AreEquivalent(WeightsIndexesDecision d1, WeightsIndexesDecision d2)
		{
			if (d1.Count != d2.Count || d1.AllowDuplicates != d2.AllowDuplicates)
				return false;

			var w1 = d1.Weights;
			var w2 = d2.Weights;

			if (ReferenceEquals(w1, w2))
				return true;

			if (w1.GetType() != w2.GetType())
				return false;

			if (w1.GetLength() != w2.GetLength())
				return false;

			var length = w1.GetLength();

			// Don't bother comparing if the list of weights is too long.
			if (length > 5000UL)
				return false;

			// If we make it here then we need to inspect the specific known types.
			switch (w1)
			{
				case IEquivalent s1: return s1.IsEquivalent(w2); // WeightedTable<T> and custom user types
				case ISymbolListStrip s1: // WeightedSymbolListStrip, SymbolListStrip and SymbolReplacementStrip
				{
					var s2 = (ISymbolListStrip)w2;

					if (s1.GetSymbolList().GetId() != s2.GetSymbolList().GetId())
						return false;

					for (var i = 0UL; i < length; ++i)
					{
						if (s1.GetSymbolIndex(i) != s2.GetSymbolIndex(i) || s1.GetWeight(i) != s2.GetWeight(i))
							return false;
					}

					return true;
				}
				case IStrip s1: // SimpleStrip, SimpleWeightedStrip and StopsStrip
				{
					var s2 = (IStrip)w2;

					for (var i = 0UL; i < length; ++i)
					{
						if (s1.GetSymbol(i) != s2.GetSymbol(i) || s1.GetWeight(i) != s2.GetWeight(i))
							return false;
					}

					return true;
				}
				default: return false;
			}
		}
	}
}