using System;
using System.Collections.Generic;
using System.Linq;
using Gaff.Core.DecisionMakers;
using Logic.Core.DecisionGenerator.Decisions;

namespace Gaff.Core.GaffEditor
{
	public sealed class DecisionProvider
	{
		private IReadOnlyList<DecisionMaker> makers;

		public void SetMakers(IReadOnlyList<DecisionMaker> decisionMakers)
		{
			makers = decisionMakers;
		}

		public bool Create(string context, Func<DecisionDefinition> decisionData, Dictionary<DecisionMaker, object> stateData, out DecisionOutcome decisionResult)
		{
			decisionResult = null;

			var firstValid = makers?.FirstOrDefault(m =>
			{
				stateData.TryGetValue(m, out var sd);

				var result = m.Valid(context, decisionData, ref sd);

				if (sd != null)
					stateData[m] = sd;

				return result;
			});

			if (firstValid == null)
				return false;

			stateData.TryGetValue(firstValid, out var localStateData);

			decisionResult = firstValid.Create(decisionData(), localStateData);
			return true;
		}
	}
}