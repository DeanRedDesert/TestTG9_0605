using System.Collections.Generic;

namespace Logic.Core.Types
{
	/// <summary>
	/// The result information of a SchemafyProcessor.  Returned along with the modified stops.
	/// </summary>
	public sealed class SchemaData
	{
		/// <summary>
		/// The symbols that we are replacing.
		/// </summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Global - Used in presentation
		public IReadOnlyList<string> SymbolsToReplace { get; }

		/// <summary>
		/// The symbols that were chosen to replace the SymbolsToReplace.
		/// </summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Global - Used in presentation
		public IReadOnlyList<string> ReplacementSymbols { get; }

		/// <summary>
		/// Initialises a new instance of this class.
		/// </summary>
		/// <param name="symbolsToReplace">The symbols that we are replacing.</param>
		/// <param name="replacementSymbols">The symbols that were chosen to replace the SymbolsToReplace.</param>
		public SchemaData(IReadOnlyList<string> symbolsToReplace, IReadOnlyList<string> replacementSymbols)
		{
			SymbolsToReplace = symbolsToReplace;
			ReplacementSymbols = replacementSymbols;
		}
	}
}