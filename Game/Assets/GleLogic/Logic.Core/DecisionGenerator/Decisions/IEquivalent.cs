namespace Logic.Core.DecisionGenerator.Decisions
{
	/// <summary>
	/// Implement this interface for comparing custom IWeights objects so that they refresh the dial-up UI correctly
	/// </summary>
	public interface IEquivalent
	{
		/// <summary>
		/// Returns true if this instance and the obj are equivalent for the dial-up UI.
		/// </summary>
		bool IsEquivalent(object obj);
	}
}