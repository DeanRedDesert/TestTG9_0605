using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// A pattern with one cell per cluster.
	/// </summary>
	public sealed class LinePattern
	{
		/// <summary>
		/// Construction
		/// </summary>
		public LinePattern(ReadOnlyMask positionsMask, int[] positions)
		{
			PositionsMask = positionsMask;
			Positions = positions;
		}

		/// <summary>
		/// Cluster positions as a mask.
		/// </summary>
		public ReadOnlyMask PositionsMask { get; }

		/// <summary>
		/// Cluster positions as indexes.
		/// </summary>
		public int[] Positions { get; }
	}
}