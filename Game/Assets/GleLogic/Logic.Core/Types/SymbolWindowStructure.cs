using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

namespace Logic.Core.Types
{
	/// <summary>
	/// The structure of the symbol window that will be used for evaluation.
	/// Consists of a collection cells which each have a column and row coordinate.
	/// A set of population information that indicates how the cells will be populated with symbols.
	/// The indexed position of each cell in the <see cref="Cells"/> list is used to create masks and efficiently process symbol and prize related data.
	/// </summary>
	public sealed class SymbolWindowStructure : IToString
	{
		#region Fields

		private readonly IReadOnlyList<IPopulation> sourcePopulations;
		private readonly Lazy<IReadOnlyDictionary<Cell, int>> cellLookup;

		#endregion

		#region Properties

		/// <summary>
		/// The cells collection that is the basis of the <see cref="Populations"/> and <see cref="PopulationsAsIndexes"/> properties as well as symbol masks and pattern masks.
		/// </summary>
		public IReadOnlyList<Cell> Cells { get; }

		/// <summary>
		/// The masks showing which cells are populated by which populations.
		/// </summary>
		public IReadOnlyList<ReadOnlyMask> Populations { get; }

		/// <summary>
		/// The indexes showing which cells are populated by which populations.
		/// </summary>
		public IReadOnlyList<IReadOnlyList<int>> PopulationsAsIndexes { get; }

		#endregion

		#region Construction

		/// <summary>
		/// Create from a collection of <see cref="IPopulation"/>.
		/// </summary>
		public SymbolWindowStructure(IReadOnlyList<IPopulation> sourcePopulations)
		{
			var structure = new List<Cell>();
			var populations = new List<ReadOnlyMask>();

			for (var i = 0; i < sourcePopulations.Count; i++)
				structure.AddRange(sourcePopulations[i].GetCells());

			var initialPopulationIndex = 0;
			var popsAsIndexes = new List<IReadOnlyList<int>>();

			for (var i = 0; i < sourcePopulations.Count; i++)
			{
				var range = Range(initialPopulationIndex, sourcePopulations[i].GetCells().Count);

				popsAsIndexes.Add(range);
				populations.Add(ReadOnlyMask.CreateFromIndexes(structure.Count, range));
				initialPopulationIndex += sourcePopulations[i].GetCells().Count;
			}

			this.sourcePopulations = sourcePopulations;
			Cells = structure;
			Populations = populations;
			PopulationsAsIndexes = popsAsIndexes;

			cellLookup = new Lazy<IReadOnlyDictionary<Cell, int>>(() =>
			{
				var dict = new Dictionary<Cell, int>();

				for (var i = 0; i < Cells.Count; i++)
					dict.Add(Cells[i], i);

				return dict;
			}, LazyThreadSafetyMode.PublicationOnly);
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Returns the index into the <see cref="Cells"/> list for <param name="cell"/>.
		/// </summary>
		public int GetCellIndex(Cell cell) => cellLookup.Value[cell];

		/// <summary>
		/// Returns the index into the <see cref="Cells"/> list for <param name="cell"/>, or the <see cref="defaultValue"/> if the cell is not found.
		/// </summary>
		public int GetCellIndexOrDefault(Cell cell, int defaultValue = -1) => cellLookup.Value.TryGetValue(cell, out var v) ? v : defaultValue;

		/// <summary>
		/// Returns the populations that were used to create this <see cref="SymbolWindowStructure"/>.
		/// </summary>
		// ReSharper disable once UnusedMember.Global - Used by presentation
		public IReadOnlyList<IPopulation> GetSourcePopulations() => sourcePopulations;

		public Cell[] IndexesToCells(int[] indexes) => 
			indexes.Select(index => cellLookup.Value.FirstOrDefault(pair => pair.Value == index).Key).ToArray();

		#endregion

		#region Private Functions

		private static IReadOnlyList<int> Range(int startingValue, int count)
		{
			var range = new List<int>();

			for (var j = 0; j < count; j++)
				range.Add(startingValue + j);

			return range;
		}

		#endregion

		#region IToString Implementation

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (format == "ML")
			{
				return this
					.GetStructureArrays()
					.ToTableResult(v => v == -1 ? "" : PopulationsAsIndexes.IndexOf(l => l.Contains(v)).ToString());
			}

			return new NotSupported();
		}

		#endregion
	}
}