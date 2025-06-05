namespace Logic.Core.Types
{
	/// <summary>
	/// Maps a symbol name to a symbol multiplier.
	/// </summary>
	public sealed class SymbolMultiplier
	{
		/// <summary>
		/// The name of the symbol to multiply.
		/// </summary>
		public string SymbolName { get; }

		/// <summary>
		/// The multiplier value for the symbol.
		/// </summary>
		public double Multiplier { get; }

		/// <summary>
		/// Constructor to initialise any required data.
		/// </summary>
		/// <param name="symbolName">The name of the symbol.</param>
		/// <param name="multiplier">The multiplier for the symbol.</param>
		public SymbolMultiplier(string symbolName, double multiplier)
		{
			SymbolName = symbolName;
			Multiplier = multiplier;
		}
	}
}