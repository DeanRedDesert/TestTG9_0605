using System;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Utility;

namespace Gaff.Core.DecisionMakers
{
	/// <summary>
	/// A decision maker overrides logic decisions under specific conditions.
	/// </summary>
	public abstract class DecisionMaker : IToString
	{
		/// <summary>
		/// Make the decision result based on the knowledge it is valid using the state data provided.
		/// The state data provided is as it was after the Valid call.
		/// </summary>
		public abstract DecisionOutcome Create(DecisionDefinition decisionData, object stateData);

		/// <summary>
		/// Check if the decision is valid of this decision maker.
		/// The <param name="decisionDefinition"/> is a func so we can do some quick checks with <param name="context"/> before we have to create a whole decision data object.
		/// </summary>
		public abstract bool Valid(string context, Func<DecisionDefinition> decisionDefinition, ref object stateData);

		/// <summary>
		/// Need to implement 'SL' format for display in the UI.
		/// </summary>
		public abstract IResult ToString(string format);
	}
}