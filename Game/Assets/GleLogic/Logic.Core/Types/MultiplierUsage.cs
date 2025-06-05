namespace Logic.Core.Types
{
	/// <summary>
	/// Indicates the way a symbol multiplier will be interpreted by the Evaluator.
	/// </summary>
	public enum MultiplierUsage
	{
		/// <summary>
		/// Take the best symbol multiplier from winning positions.
		/// </summary>
		Best,

		/// <summary>
		/// Take the sum of the symbol multipliers found in winning positions.
		/// </summary>
		Add,

		/// <summary>
		/// Multiply together the symbol multipliers found in winning positions.
		/// </summary>
		Multiply
	}
}