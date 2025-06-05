namespace Logic.Core.DecisionGenerator.Decisions
{
	/// <summary>
	/// Represents a call <see cref="IDecisionGenerator.GetDecision"/>
	/// </summary>
	public sealed class SimpleDecision : DecisionDefinition
	{
		public ulong TrueWeight { get; }
		public ulong FalseWeight { get; }

		public SimpleDecision(string context, ulong trueWeight, ulong falseWeight)
			: base(context)
		{
			TrueWeight = trueWeight;
			FalseWeight = falseWeight;
		}
	}
}