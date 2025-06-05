using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.General;

namespace Midas.Logic
{
	/// <summary>
	/// Returned by IGame when a result is generated to pass outcomes to the foundation.
	/// </summary>
	public sealed class GambleOutcome : IOutcome
	{
		private sealed class GamblePrize : IFoundationPrize
		{
			public string PrizeName => "GamblePrize";
			public Money RiskAmount { get; }
			public Money Amount { get; }
			public IProgressiveHit ProgressiveHit => null;

			public GamblePrize(Money riskAmount, Money amount)
			{
				RiskAmount = riskAmount;
				Amount = amount;
			}
		}

		/// <summary>
		/// Used when the player cancels gamble.
		/// </summary>
		public static readonly GambleOutcome CancelGambleOutcome = new GambleOutcome(Array.Empty<IFoundationPrize>(), true);

		public int FeatureIndex => 0;

		/// <summary>
		/// The collection of prizes for the outcome.
		/// </summary>
		public IReadOnlyList<IFoundationPrize> Prizes { get; }

		/// <summary>
		/// Is this is the final outcome for the game.
		/// </summary>
		public bool IsFinalOutcome { get; }

		/// <summary>
		/// Creates a win outcome for the gamble feature. Results in the end of gamble if isFinalOutcome is true.
		/// </summary>
		/// <param name="riskAmount">The amount that was risked.</param>
		/// <param name="winAmount">The amount that was won. In the case of a tie, riskAmount == winAmount.</param>
		/// <param name="isFinalOutcome">True if this is the final outcome.</param>
		public static GambleOutcome CreateWinOutcome(Money riskAmount, Money winAmount, bool isFinalOutcome)
		{
			return new GambleOutcome(new[] { new GamblePrize(riskAmount, winAmount) }, isFinalOutcome);
		}

		/// <summary>
		/// Creates a loss outcome for the gamble feature. Always results in the end of gamble.
		/// </summary>
		/// <param name="riskAmount">The amount that was risked.</param>
		public static GambleOutcome CreateLossOutcome(Money riskAmount)
		{
			return new GambleOutcome(new[] { new GamblePrize(riskAmount, Money.Zero) }, true);
		}

		private GambleOutcome(IReadOnlyList<IFoundationPrize> prizes, bool isFinalOutcome)
		{
			Prizes = prizes;
			IsFinalOutcome = isFinalOutcome;
		}
	}
}