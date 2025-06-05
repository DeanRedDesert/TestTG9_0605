using System;

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
	/// <summary>
	/// Holds the parameters for reserve.
	/// </summary>
	[Serializable]
	public class ReserveParameters
	{
		/// <summary>
		/// True if the reserve can be enabled with credits.
		/// </summary>
		public bool IsReserveAllowedWithCredits { get; private set; }

		/// <summary>
		/// True if the reserve can be enabled without credits.
		/// </summary>
		public bool IsReserveAllowedWithoutCredits { get; private set; }

		/// <summary>
		/// Millisecond timeout when reserve is shown with credits on the credit meter.
		/// </summary>
		public long ReserveTimeWithCreditsMilliseconds { get; private set; }

		/// <summary>
		/// Millisecond timeout when reserve is shown without credits on the credit meter.
		/// </summary>
		public long ReserveTimeWithoutCreditsMilliseconds { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ReserveParameters(bool isReserveAllowedWithCredits, bool isReserveAllowedWithoutCredits, long reserveTimeWithCreditsMilliseconds, long reserveTimeWithoutCreditsMilliseconds)
		{
			IsReserveAllowedWithCredits = isReserveAllowedWithCredits;
			IsReserveAllowedWithoutCredits = isReserveAllowedWithoutCredits;
			ReserveTimeWithCreditsMilliseconds = reserveTimeWithCreditsMilliseconds;
			ReserveTimeWithoutCreditsMilliseconds = reserveTimeWithoutCreditsMilliseconds;
		}
	}
}