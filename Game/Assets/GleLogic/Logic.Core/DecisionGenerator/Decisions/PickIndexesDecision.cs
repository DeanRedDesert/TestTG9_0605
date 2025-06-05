using System;

namespace Logic.Core.DecisionGenerator.Decisions
{
	/// <summary>
	/// Represents a call <see cref="IDecisionGenerator.PickIndexes(ulong,uint,uint,bool,Func{ulong,string},Func{string})"/>
	/// </summary>
	public sealed class PickIndexesDecision : DecisionDefinition
	{
		public ulong IndexCount { get; }
		public uint MinCount { get; }
		public uint MaxCount { get; }
		public bool AllowDuplicates { get; }
		public Func<ulong, string> GetName { get; }

		public PickIndexesDecision(string context, ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName)
			: base(context)
		{
			IndexCount = indexCount;
			MinCount = minCount;
			MaxCount = maxCount;
			AllowDuplicates = allowDuplicates;
			GetName = getName;
		}
	}
}