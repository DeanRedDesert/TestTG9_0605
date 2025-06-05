using Logic.Core.DecisionGenerator.Decisions;

namespace Gaff.Core.DecisionMakers
{
	public sealed class DecisionOutcome
	{
		public Decision Decision { get; }
		public bool Success { get; }
		public string Reason { get; }

		public static implicit operator DecisionOutcome(Decision decision) => new DecisionOutcome(null, true, decision);

		public DecisionOutcome(string message = null, bool success = false, Decision decision = null)
		{
			Decision = decision;
			Success = success;
			Reason = message;
		}
	}
}