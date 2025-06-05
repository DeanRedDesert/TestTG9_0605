namespace Logic.Core.Types
{
	/// <summary>
	/// Maps a prize name to a count.
	/// </summary>
	// ReSharper disable once ClassNeverInstantiated.Global - Helper class (may be used by custom components)
	public sealed class PrizeCount
	{
		/// <summary>
		/// Gets or sets the name of the prize.
		/// </summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Global - Used in presentation
		public string PrizeName { get; }

		/// <summary>
		/// The count of winning clusters required to trigger this prize.
		/// </summary>
		// ReSharper disable once UnusedAutoPropertyAccessor.Global - Used in presentation
		public int Count { get; }

		/// <summary>
		/// The number of cycles to be awarded.
		/// </summary>
		public int CycleCountToAward { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="prizeName">The name of the prize.</param>
		/// <param name="count">The clusters required for the value to be awarded.</param>
		/// <param name="cycleCountToAward">The number of cycles to be awarded.</param>
		public PrizeCount(string prizeName, int count, int cycleCountToAward)
		{
			PrizeName = prizeName;
			Count = count;
			CycleCountToAward = cycleCountToAward;
		}
	}
}