using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// Maps a prize name to a count and the number of cycles to be awarded.
	/// </summary>
	public sealed class PrizeCountWithCycleSet : IToString
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
		/// The identifier of the cycle set to award.
		/// </summary>
		public string CycleId { get; }

		/// <summary>
		/// The number of cycles to be awarded.
		/// </summary>
		public int CycleCountToAward { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="prizeName">The name of the prize.</param>
		/// <param name="count">The clusters required for the value to be awarded.</param>
		/// <param name="cycleId">The identifier of the cycle set to award.</param>
		/// <param name="cycleCountToAward">The number of cycles to be awarded.</param>
		public PrizeCountWithCycleSet(string prizeName, int count, string cycleId, int cycleCountToAward)
		{
			PrizeName = prizeName;
			Count = count;
			CycleId = cycleId;
			CycleCountToAward = cycleCountToAward;
		}

		#region IToString

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => new NotSupported();

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format == "ML" && list is IReadOnlyList<PrizeCountWithCycleSet> prizes)
			{
				return prizes.ToStringArrays(
						new[] { "Prize", "Count", "CycleId", "CycleCount" },
						p => new[] { p.PrizeName, p.Count.ToString(), p.CycleId, p.CycleCountToAward.ToString() })
					.ToTableResult();
			}

			return new NotSupported();
		}

		#endregion
	}
}