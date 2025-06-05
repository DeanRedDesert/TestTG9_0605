using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global - Required for serialisation
// ReSharper disable MemberCanBePrivate.Global - Required for serialisation

namespace Logic.Core.Types
{
	/// <summary>
	/// The schema entry is the item that encapsulates the weight and symbols to swap in.
	/// </summary>
	public sealed class SchemaEntry
	{
		/// <summary>
		/// The weight of this schema entry.
		/// </summary>
		public ulong Weight { get; }

		/// <summary>
		/// The symbols to swap in place of the SymbolsToReplace.
		/// </summary>
		public IReadOnlyList<string> ReplacementSymbols { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="weight">The weight of the entry.</param>
		/// <param name="replacementSymbols">The list of replacement symbols.</param>
		public SchemaEntry(ulong weight, IReadOnlyList<string> replacementSymbols)
		{
			Weight = weight;
			ReplacementSymbols = replacementSymbols;
		}
	}
}