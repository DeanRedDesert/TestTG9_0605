using System;
using System.Collections.Generic;
using Gaff.Conditions;
using Gaff.Core.DecisionMakers;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Utility;

// ReSharper disable MemberCanBePrivate.Global

namespace Gaff.DecisionMakers
{
	/// <summary>
	/// For each decision context that meets the <see cref="ContextCondition"/> check the symbol at each index with <see cref="SymbolCondition"/> and if we find a valid one place it within <see cref="SymbolWindow"/>
	/// positions.
	/// </summary>
	public sealed class SelectSymbols : DecisionMaker
	{
		public SelectSymbols(StringCondition contextCondition, StringCondition symbolCondition, uint symbolWindow, ulong trueWeight, ulong falseWeight)
		{
			ContextCondition = contextCondition;
			SymbolCondition = symbolCondition;
			SymbolWindow = symbolWindow;
			TrueWeight = trueWeight;
			FalseWeight = falseWeight;
		}

		/// <summary>
		/// The context condition determines which decisions are searched.
		/// </summary>
		public StringCondition ContextCondition { get; }

		/// <summary>
		/// The symbol condition determines which indexes are selected.
		/// </summary>
		public StringCondition SymbolCondition { get; }

		/// <summary>
		/// The symbol window determines the maximum adjustment to the valid index for interesting variations.
		/// </summary>
		public uint SymbolWindow { get; }

		/// <summary>
		/// The true weight is used with the false weight to determine how often a valid decision context will choose an index that meets the <see cref="SymbolCondition"/>.
		/// </summary>
		public ulong TrueWeight { get; }

		/// <summary>
		/// The false weight is used with the true weight to determine how often a valid decision context will choose an index that meets the <see cref="SymbolCondition"/>.
		/// </summary>
		public ulong FalseWeight { get; }

		/// <inheritdoc />
		public override DecisionOutcome Create(DecisionDefinition decisionData, object stateData)
		{
			bool CheckCondition(string s)
			{
				var useSymbolCondition = GaffDecisionHelper.GetDecision(TrueWeight, FalseWeight);
				var condition = SymbolCondition.Check(s);
				return useSymbolCondition ? condition : !condition;
			}

			switch (decisionData)
			{
				case WeightedIndexesDecision d:
				{
					var items = new List<ulong>();
					GaffDecisionHelper.Fill(i => d.GetName(i), CheckCondition, d.AllowDuplicates, d.IndexCount, d.Count, SymbolWindow, items);

					return new Decision(decisionData, items);
				}
				case IndexesDecision d:
				{
					var items = new List<ulong>();
					GaffDecisionHelper.Fill(i => d.GetName(i), CheckCondition, d.AllowDuplicates, d.IndexCount, d.Count, SymbolWindow, items);

					return new Decision(decisionData, items);
				}
				case WeightsIndexesDecision d:
				{
					var items = new List<ulong>();
					GaffDecisionHelper.Fill(i => d.GetName(i), CheckCondition, d.AllowDuplicates, d.Weights.GetLength(), d.Count, SymbolWindow, items);

					return new Decision(decisionData, items);
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(decisionData));
			}
		}

		/// <inheritdoc />
		public override bool Valid(string context, Func<DecisionDefinition> decisionData, ref object stateData)
		{
			if (!ContextCondition.Check(context))
				return false;

			var decisionDataObj = decisionData();

			return !(decisionDataObj is SimpleDecision || decisionDataObj is PickIndexesDecision);
		}

		/// <inheritdoc />
		public override IResult ToString(string format)
		{
			return $"If the context {ContextCondition} then {CreateHitText()} for each chosen index, find a stop that has a symbol {SymbolCondition} within {SymbolWindow} stops".ToSuccess();
		}

		private string CreateHitText()
		{
			if (TrueWeight == 0 && FalseWeight == 0)
				return "Error!  Both weights are 0.";
			if (TrueWeight == FalseWeight)
				return "half of the time";

			return FalseWeight == 0 ? "" : $"{TrueWeight} / {TrueWeight + FalseWeight} of the time";
		}
	}
}