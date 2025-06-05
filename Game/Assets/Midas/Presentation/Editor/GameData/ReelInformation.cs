using System.Collections.Generic;

namespace Midas.Presentation.Editor.GameData
{
	/// <summary>
	/// Provides information about reels in the stage model.
	/// </summary>
	public sealed class ReelInformation
	{
		#region Properties

		/// <summary>
		/// The name of the processor that this information came from.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the populations.
		/// </summary>
		public IReadOnlyList<IReadOnlyList<(int row, int column)>> Populations { get; }

		/// <summary>
		/// Gets the visible symbols for a column.
		/// </summary>
		public IReadOnlyList<int> ColumnVisibleSymbols { get; }

		/// <summary>
		/// Gets the number of columns.
		/// </summary>
		public int ColumnCount => ColumnVisibleSymbols.Count;

		#endregion

		#region Construction

		/// <summary>
		/// Constructor.
		/// </summary>
		public ReelInformation(string name, IReadOnlyList<IReadOnlyList<(int Row, int Column)>> cellPopulations)
		{
			Name = name;
			Populations = cellPopulations;
			ColumnVisibleSymbols = GetVisibleSymbols(cellPopulations);
		}

		#endregion

		#region Private Methods

		private static IReadOnlyList<int> GetVisibleSymbols(IReadOnlyList<IReadOnlyList<(int Row, int Column)>> cellPopulations)
		{
			var minColumn = int.MaxValue;
			var maxColumn = int.MinValue;
			var windowExtents = new List<int[]>();
			foreach (var pop in cellPopulations)
			{
				foreach (var cell in pop)
				{
					while (windowExtents.Count <= cell.Column)
						windowExtents.Add(null);

					var extent = windowExtents[cell.Column];
					if (extent == null)
					{
						extent = new[] { cell.Row, cell.Row };
						windowExtents[cell.Column] = extent;
					}
					else
					{
						if (cell.Row < extent[0])
							extent[0] = cell.Row;
						if (cell.Row > extent[1])
							extent[1] = cell.Row;
					}

					if (cell.Column < minColumn)
						minColumn = cell.Column;
					if (cell.Column > maxColumn)
						maxColumn = cell.Column;
				}
			}

			var visibleSymbols = new List<int>();
			for (var i = minColumn; i <= maxColumn; i++)
			{
				var extent = windowExtents[i];

				if (extent == null)
				{
					// This shouldn't happen if the patterns are defined properly.

					visibleSymbols.Add(0);
				}
				else
				{
					visibleSymbols.Add(extent[1] - extent[0] + 1);
				}
			}

			return visibleSymbols;
		}

		#endregion
	}
}