using System;
using System.Collections.Generic;
using Gaff.Core.DecisionMakers;
using Logic.Core.DecisionGenerator;
using Logic.Core.DecisionGenerator.Decisions;

namespace Gaff.Core.GaffEditor
{
	/// <summary>
	/// Records any decisions made using the specified decision generator to provide the values stored with the DecisionDefinition objects.
	/// A GetDecision uses a provided DecisionProvider to ask for a valid result and then checks if our decision provider has succeeded.  If we have met conditions but failed to provide a
	/// result then we post an error message and move on.
	/// </summary>
	public sealed class GaffDecisionGenerator : AlternateDecisionGenerator
	{
		private readonly DecisionProvider decisionProvider;
		private readonly Dictionary<DecisionMaker, object> stateData;
		private readonly List<string> errorMessages = new List<string>();

		public GaffDecisionGenerator(IDecisionGenerator decisionGenerator, DecisionProvider decisionProvider)
			: base(decisionGenerator)
		{
			this.decisionProvider = decisionProvider;
			stateData = new Dictionary<DecisionMaker, object>();
		}

		public IReadOnlyList<string> GetErrorMessages() => errorMessages;

		protected override Decision GetDecision<T>(string context, Func<T> decisionDefinition, Func<object> result)
		{
			if (!decisionProvider.Create(context, decisionDefinition, stateData, out var decisionMakeResult))
				return new Decision(decisionDefinition(), result());

			if (decisionMakeResult.Success)
				return decisionMakeResult.Decision;

			// If we are here we met the criteria to run a maker but failed to make a decision... post the error create a default result and move on.
			errorMessages.Add(decisionMakeResult.Reason);
			return new Decision(decisionDefinition(), result());
		}
	}
}