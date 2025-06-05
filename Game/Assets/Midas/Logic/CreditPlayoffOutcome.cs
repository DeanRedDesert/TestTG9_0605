using System.Collections.Generic;
using Midas.Core;
using Midas.Core.General;

namespace Midas.Logic
{
	public sealed class CreditPlayoffOutcome : IOutcome
	{
		private sealed class CreditPlayoffPrize : IFoundationPrize
		{
			public string PrizeName => "CreditPlayoffPrize";
			public Money RiskAmount { get; }
			public Money Amount { get; }
			public IProgressiveHit ProgressiveHit => null;

			public CreditPlayoffPrize(Money riskAmount, Money amount)
			{
				RiskAmount = riskAmount;
				Amount = amount;
			}
		}

		public int FeatureIndex => 0;
		public IReadOnlyList<IFoundationPrize> Prizes { get; }
		public bool IsFinalOutcome { get; }

		private CreditPlayoffOutcome(Money riskAmount, Money amount)
		{
			IsFinalOutcome = amount.IsZero;
			Prizes = new[]
			{
				new CreditPlayoffPrize(riskAmount, amount)
			};
		}

		public static IOutcome CreateWin(Money remainingCash, Money currentBet)
		{
			return new CreditPlayoffOutcome(remainingCash, currentBet);
		}

		public static IOutcome CreateLoss(Money remainingCash)
		{
			return new CreditPlayoffOutcome(remainingCash, Money.Zero);
		}
	}
}