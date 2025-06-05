using System;

namespace Logic.Core.DecisionGenerator.Decisions
{
	/// <summary>
	/// Represents a call <see cref="IDecisionGenerator.ChooseIndexes(ulong,uint,bool,Func{ulong,ulong},ulong,Func{ulong,string},Func{string})"/>
	/// </summary>
	public sealed class WeightedIndexesDecision : IndexesDecision
	{
		public ulong TotalWeight { get; }
		public Func<ulong, ulong> GetWeight { get; }

		public WeightedIndexesDecision(string context, ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, ulong totalWeight, Func<ulong, ulong> getWeight)
			: base(context, indexCount, count, allowDuplicates, getName)
		{
			TotalWeight = totalWeight;
			GetWeight = getWeight;
		}
	}
}