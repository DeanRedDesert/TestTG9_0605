using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// A pattern with one or more cells per cluster.
	/// </summary>
	public sealed class ClusterPattern
	{
		/// <summary>
		/// Construction
		/// </summary>
		public ClusterPattern(ReadOnlyMask[] positionsMask, int[][] positions)
		{
			PositionsMask = positionsMask;
			Positions = positions;
		}

		/// <summary>
		/// Cluster positions as a mask.
		/// </summary>
		public ReadOnlyMask[] PositionsMask { get; }

		/// <summary>
		/// Cluster positions as indexes.
		/// </summary>
		public int[][] Positions { get; }
	}
}