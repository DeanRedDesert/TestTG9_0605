// ReSharper disable UnusedAutoPropertyAccessor.Global - Required for serialisation
// ReSharper disable MemberCanBePrivate.Global - Required for serialisation

namespace Logic.Core.Types
{
	/// <summary>
	/// Associates a progressive trigger to a prize.
	/// </summary>
	public sealed class PrizeCountWithProgressiveLevel
	{
		/// <summary>
		/// The name of the prize.
		/// </summary>
		public string PrizeName { get; }

		/// <summary>
		/// The count of winning clusters required to trigger this prize.
		/// </summary>
		public int Count { get; }

		/// <summary>
		/// The identifier of the progressive level to trigger when this prize is awarded.
		/// </summary>
		public string ProgressiveIdentifier { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="prizeName">The name of the prize.</param>
		/// <param name="count">The clusters required for the value to be awarded.</param>
		/// <param name="progressiveIdentifier">The identifier of the progressive level to award.</param>
		public PrizeCountWithProgressiveLevel(string prizeName, int count, string progressiveIdentifier)
		{
			PrizeName = prizeName;
			Count = count;
			ProgressiveIdentifier = progressiveIdentifier;
		}
	}
}