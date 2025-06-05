using System.Collections.Generic;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

namespace Logic.Core.Types
{
	/// <summary>
	/// A symbol window result is an efficient representation of the set of cells and associated symbols that will be used to evaluate prizes and other features.
	/// </summary>
	public sealed class SymbolWindowResult : IToString
	{
		#region Properties

		/// <summary>
		/// The symbol list used to create this result.
		/// Each index of this array is a symbol index associated with the symbol at that position.
		/// </summary>
		public SymbolList SymbolList { get; }

		/// <summary>
		/// The <see cref="SymbolWindowStructure"/> used to create this result.
		/// </summary>
		public SymbolWindowStructure SymbolWindowStructure { get; }

		/// <summary>
		/// The actual result.  A collection of masks, one per symbol.  Each symbol index contains a mask is based on the position of a cell and whether that symbol is in that position.
		/// </summary>
		public IReadOnlyList<ReadOnlyMask> SymbolMasks { get; }

		/// <summary>
		/// Strips used when creating this result.
		/// </summary>
		public IReadOnlyList<ISymbolListStrip> SourceStrips { get; }

		/// <summary>
		/// Strip selections used to create the result.
		/// </summary>
		public IReadOnlyList<ulong> StripSelections { get; }

		#endregion

		#region Construction

		/// <summary>
		/// Constructor.
		/// </summary>
		public SymbolWindowResult(SymbolList symbolList, SymbolWindowStructure symbolWindowStructure, IReadOnlyList<ReadOnlyMask> symbolMasks, IReadOnlyList<ISymbolListStrip> strips, IReadOnlyList<ulong> selections)
		{
			SourceStrips = strips;
			StripSelections = selections;
			SymbolList = symbolList;
			SymbolWindowStructure = symbolWindowStructure;
			SymbolMasks = symbolMasks;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public SymbolWindowResult(SymbolList symbolList, SymbolWindowStructure symbolWindowStructure, IReadOnlyList<ReadOnlyMask> symbolMasks)
		{
			SourceStrips = null;
			StripSelections = null;
			SymbolList = symbolList;
			SymbolWindowStructure = symbolWindowStructure;
			SymbolMasks = symbolMasks;
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Get the symbol at the structural index <param name="structureIndex"/>.
		/// </summary>
		public string GetSymbolAt(int structureIndex)
		{
			return SymbolList[GetSymbolIndexAt(structureIndex)];
		}

		/// <summary>
		/// Get the symbol at the cell <param name="cell"/>.
		/// </summary>
		public string GetSymbolAt(Cell cell)
		{
			return SymbolList[GetSymbolIndexAt(cell)];
		}

		/// <summary>
		/// Get the symbol index at the structural index <param name="structureIndex"/>.
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		public int GetSymbolIndexAt(int structureIndex)
		{
			for (var i = 0; i < SymbolMasks.Count; i++)
			{
				if (SymbolMasks[i] == null)
					continue;
				if (SymbolMasks[i][structureIndex])
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Get the symbol index at the cell <param name="cell"/>.
		/// </summary>
		public int GetSymbolIndexAt(Cell cell)
		{
			for (var i = 0; i < SymbolMasks.Count; i++)
			{
				if (SymbolMasks[i] == null)
					continue;
				if (SymbolMasks[i][SymbolWindowStructure.GetCellIndex(cell)])
					return i;
			}

			return -1;
		}

		#endregion

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (format == "ML")
			{
				return SymbolWindowStructure.GetStructureArrays()
					.ToTableResult(v => v == -1 ? "" : GetSymbolAt(v));
			}

			return new NotSupported();
		}
	}
}