namespace Logic.Core.DecisionGenerator.Decisions
{
	/// <summary>
	/// The decision data is an object representing a call to a particular function in the decision generator and the parameters specified.
	/// It has 4 derived types each representing a function in the IDecisionGenerator interface.
	/// </summary>
	public abstract class DecisionDefinition
	{
		public string Context { get; }

		protected DecisionDefinition(string context) => Context = context;
	}
}