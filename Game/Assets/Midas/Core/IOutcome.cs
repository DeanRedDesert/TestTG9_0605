using System.Collections.Generic;
using Midas.Core.General;

namespace Midas.Core
{
	public interface IOutcome
	{
		/// <summary>
		/// The feature index of the outcome, if relevant. Required for the platform to be able to count features.
		/// </summary>
		public int FeatureIndex { get; }

		/// <summary>
		/// The collection of prizes for the outcome.
		/// </summary>
		public IReadOnlyList<IFoundationPrize> Prizes { get; }

		/// <summary>
		/// Is this is the final outcome for the game.
		/// </summary>
		public bool IsFinalOutcome { get; }
	}

	public static class OutcomeExtensionMethods
	{
		public static Money GetTotalPrizes(this IOutcome outcome)
		{
			var result = Money.Zero;
			var p = outcome.Prizes;

			for (var i = 0; i < p.Count; i++)
				result += p[i].Amount;

			return result;
		}
	}
}