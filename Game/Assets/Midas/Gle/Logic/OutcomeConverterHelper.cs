using System.Globalization;
using Midas.Core;
using Midas.Core.General;

namespace Midas.Gle.Logic
{
	/// <summary>
	/// Helper class for all outcome converters.
	/// </summary>
	public static class OutcomeConverterHelper
	{
		#region Nested Types

		private sealed class CreditPrize : IFoundationPrize
		{
			public string PrizeName { get; }
			public Money RiskAmount => Money.Zero;
			public Money Amount { get; }
			public IProgressiveHit ProgressiveHit { get; }

			public CreditPrize(IProgressiveHit progressiveHit)
			{
				PrizeName = "";
				Amount = Money.Zero;
				ProgressiveHit = progressiveHit;
			}

			public CreditPrize(string prizeName, int count, Money amount)
			{
				PrizeName = CreatePrizeName(count, prizeName);
				Amount = amount;
			}

			public CreditPrize(string prizeName, int count, Money amount, IProgressiveHit progressiveHit)
				: this(prizeName, count, amount)
			{
				ProgressiveHit = progressiveHit;
			}
		}

		private sealed class Progressive : IProgressiveHit
		{
			public string LevelId { get; }
			public string SourceName { get; }
			public string SourceDetails { get; }

			public Progressive(string triggerId)
			{
				LevelId = triggerId;
			}

			public Progressive(string triggerId, string sourceName, string sourceDetails)
				: this(triggerId)
			{
				SourceName = sourceName;
				SourceDetails = sourceDetails;
			}
		}

		#endregion

		#region Public Methods

		public static IFoundationPrize CreateSummarisedCreditPrize(int prizeCount, long totalAwardedCredits)
		{
			return new CreditPrize("Summarised Credit Prizes (" + prizeCount + ")", 1, Money.FromCredit(Credit.FromLong(totalAwardedCredits)));
		}

		public static IFoundationPrize CreatePrizeMappedProgressive(string prizeName, int prizeCount, string levelId, string resultName, int resultIndex)
		{
			return new CreditPrize(prizeName, prizeCount, Money.Zero, new Progressive(levelId, resultName, resultIndex.ToString()));
		}

		public static IFoundationPrize CreateNonPrizedMappedProgressive(string levelId)
		{
			return new CreditPrize(new Progressive(levelId));
		}

		/// <summary>
		/// Gets the prize name that will be used for the outcome list.
		/// </summary>
		/// <param name="count">The number of symbols involved in the win.</param>
		/// <param name="prizeName">The name of the primary symbol.</param>
		/// <returns>The prize name used in the outcome list.</returns>
		public static string CreatePrizeName(int count, string prizeName)
		{
			return count.ToString(CultureInfo.InvariantCulture) + prizeName;
		}

		#endregion
	}
}