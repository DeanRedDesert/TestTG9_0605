using System;

namespace Logic.Core.DecisionGenerator.Decisions
{
	/// <summary>
	/// Represents a call <see cref="IDecisionGenerator.ChooseIndexes(IWeights,uint,bool,Func{ulong,string},Func{string})"/>
	/// </summary>
	public sealed class WeightsIndexesDecision : DecisionDefinition
	{
		public IWeights Weights { get; }
		public uint Count { get; }
		public bool AllowDuplicates { get; }
		public Func<ulong, string> GetName { get; }

		public WeightsIndexesDecision(string context, IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName)
			: base(context)
		{
			Weights = weights;
			Count = count;
			AllowDuplicates = allowDuplicates;
			GetName = getName;
		}
	}
}