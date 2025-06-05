using System.Collections.Generic;

namespace Logic.Core.Types
{
	public sealed class SchemafySymbolWindowData
	{
		/// <summary>
		/// A list of symbols to be replaced in the process of schemafying.
		/// </summary>
		public IReadOnlyList<string> SymbolsToReplace { get; }

		/// <summary>
		/// A list of the SchemaEntry items.
		/// </summary>
		public IReadOnlyList<SchemaEntry> SchemaEntries { get; }

		/// <summary>
		/// Constructor to initialise any required data.
		/// </summary>
		/// <param name="symbolsToReplace">The list of symbols to replace.</param>
		/// <param name="schemaEntries">The list of schema entries.</param>
		public SchemafySymbolWindowData(IReadOnlyList<string> symbolsToReplace, IReadOnlyList<SchemaEntry> schemaEntries)
		{
			SymbolsToReplace = symbolsToReplace;
			SchemaEntries = schemaEntries;
		}
	}
}