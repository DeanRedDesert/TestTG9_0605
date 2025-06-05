using System.Collections.Generic;
using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// Population information for use with creating symbol sequences and CellPopulation results.
	/// </summary>
	public interface IPopulation
	{
		/// <summary>
		/// The name of this population request
		/// </summary>
		// ReSharper disable once UnusedMember.Global - Used in presentation
		string GetPopulationName();

		/// <summary>
		/// The offset into the strip that will be applied to the chosen index before generating the sequential indexes to be associated with the cells.
		/// </summary>
		// ReSharper disable once UnusedMember.Global - Used in presentation
		ulong GetStripOffset();

		/// <summary>
		/// The cell to be associated with each chosen index.
		/// </summary>
		IReadOnlyList<Cell> GetCells();
	}
}