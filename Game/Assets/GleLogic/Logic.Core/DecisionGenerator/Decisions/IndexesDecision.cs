using System;

namespace Logic.Core.DecisionGenerator.Decisions
{
	/// <summary>
	/// Represents a call <see cref="IDecisionGenerator.ChooseIndexes(ulong,uint,bool,Func{ulong,string},Func{string})"/>
	/// </summary>
	public class IndexesDecision : DecisionDefinition
	{
		public ulong IndexCount { get; }
		public uint Count { get; }
		public bool AllowDuplicates { get; }
		public Func<ulong, string> GetName { get; }

		public IndexesDecision(string context, ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName)
			: base(context)
		{
			IndexCount = indexCount;
			Count = count;
			AllowDuplicates = allowDuplicates;
			GetName = getName;
		}
	}
}