namespace Logic.Core.Types
{
	/// <summary>
	/// Results of a schemafy operation.
	/// </summary>
	public sealed class SchemafyResult
	{
		private readonly SchemaData schemaData;

		/// <summary>
		/// The stop cells for the schemafy operation.
		/// </summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		// ReSharper disable once MemberCanBePrivate.Global
		public SymbolWindowResult SymbolWindow { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="symbolWindowResult">The generated population.</param>
		/// <param name="schemaData">The schema data used to generate the stop cells.</param>
		public SchemafyResult(SymbolWindowResult symbolWindowResult, SchemaData schemaData)
		{
			SymbolWindow = symbolWindowResult;
			this.schemaData = schemaData;
		}

		/// <summary>
		/// The schema data used to generate the stop cells.
		/// </summary>
		// ReSharper disable once UnusedMember.Global - Used in presentation
		public SchemaData GetSchemaData() => schemaData;
	}
}