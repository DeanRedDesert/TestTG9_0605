using System.Collections.Generic;
using System.Text;
using Logic.Core.Utility;

namespace Logic.Core.Types.WeightScaling
{
	/// <summary>
	/// A pairing of <exception cref="WeightDistributorEntry"/> entries that will be used to adjust the <see cref="PrizeTable"/> according to bet related information.
	/// </summary>
	public sealed class PrizeReplacerEntry : IToString
	{
		/// <summary>
		/// The distribution entries to use when making the new weighted list of ids.
		/// </summary>
		public IReadOnlyList<WeightDistributorEntry> DistributorInfo { get; }

		/// <summary>
		/// The prize table to use when making the new weighted list of ids.
		/// </summary>
		public PrizeTable PrizeTable { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="distributorInfo">The distribution entries to use when making the new weighted list of ids.</param>
		/// <param name="prizeTable">The prize table to use when making the new weighted list of ids.</param>
		public PrizeReplacerEntry(IReadOnlyList<WeightDistributorEntry> distributorInfo, PrizeTable prizeTable)
		{
			DistributorInfo = distributorInfo;
			PrizeTable = prizeTable;
		}

		/// <inheritdoc />
		public IResult ToString(string format)
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine(((StringSuccess)PrizeTable.ToString("ML")).Value);
			return sb.ToString().ToSuccess();
		}
	}
}