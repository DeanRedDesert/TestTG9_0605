using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Midas.Core.General;

namespace Midas.Core
{
	public enum Stake
	{
		BetMultiplier,
		LinesBet,
		Multiway,
		AnteBet
	}

	public enum BetCategory
	{
		BetCategory0,
		BetCategory1,
		BetCategory2,
		BetCategory3,
		BetCategory4,
		BetCategory5,
		BetCategory6,
		BetCategory7,
		BetCategory8,
		BetCategory9,
		BetCategory10,
		BetCategory11,
		BetCategory12,
		BetCategory13,
		BetCategory14,
		BetCategory15,
		BetCategory16,
		BetCategory17,
		BetCategory18,
		BetCategory19,
	}

	/// <summary>
	/// Represents a unique stake combination.
	/// </summary>
	public interface IStakeCombination
	{
		/// <summary>
		/// The stake values for this combination.
		/// </summary>
		IReadOnlyDictionary<Stake, long> Values { get; }

		/// <summary>
		/// The total bet that this stake combination represents.
		/// </summary>
		Credit TotalBet { get; }

		/// <summary>
		/// The bet category to assign to the stake combination. Used to differentiate Rtp between bets.
		/// </summary>
		BetCategory BetCategory { get; }
	}

	public static class StakeCombinationExtensionMethods
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long GetBetMultiplier(this IStakeCombination stakeCombination)
		{
			return stakeCombination.Values[Stake.BetMultiplier];
		}
	}
}