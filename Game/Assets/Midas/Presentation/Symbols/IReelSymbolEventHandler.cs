namespace Midas.Presentation.Symbols
{
	/// <summary>
	/// Provides event handlers for reel symbol events.
	/// </summary>
	public interface IReelSymbolEventHandler
	{
		/// <summary>
		/// Called when a symbol is placed on a reel.
		/// </summary>
		void SymbolOnReel(ReelSymbol symbol);

		/// <summary>
		/// Called when a symbol is removed from a reel.
		/// </summary>
		void SymbolOffReel(ReelSymbol symbol);

		/// <summary>
		/// Called when a symbol changes position on a reel.
		/// </summary>
		void SymbolPositionChanged(ReelSymbol symbol);
	}
}