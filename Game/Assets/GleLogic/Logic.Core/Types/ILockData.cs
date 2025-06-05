namespace Logic.Core.Types
{
	/// <summary>
	/// Represents the standardised data that is required of a respin game.
	/// </summary>
	public interface ILockData
	{
		/// <summary>
		/// This lock mask holds the lock data that is a result of processing a stage.
		/// </summary>
		ReadOnlyMask GetLockMask();

		/// <summary>
		/// For each population index, get the locked symbol.
		/// </summary>
		/// <param name="structureIndex">The population structure index.</param>
		/// <returns>The symbol index.</returns>
		int GetLockedSymbolIndexAt(int structureIndex);
	}
}