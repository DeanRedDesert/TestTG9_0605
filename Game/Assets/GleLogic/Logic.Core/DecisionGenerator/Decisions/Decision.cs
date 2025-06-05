namespace Logic.Core.DecisionGenerator.Decisions
{
	/// <summary>
	/// An object representing a call to the decision generator. It has the decision data that describes
	/// the kind of decision (which function was called and the parameters used) and the result of the call.
	/// </summary>
	public sealed class Decision
	{
		public Decision(DecisionDefinition decision, object result)
		{
			DecisionDefinition = decision;
			Result = result;
		}

		public DecisionDefinition DecisionDefinition { get; }
		public object Result { get; }
	}
}