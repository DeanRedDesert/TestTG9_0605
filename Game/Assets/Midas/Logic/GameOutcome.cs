using System.Collections.Generic;
using Midas.Core;

namespace Midas.Logic
{
	/// <summary>
	/// Returned by IGame when a result is generated to pass outcomes to the foundation.
	/// </summary>
	public sealed class GameOutcome : IOutcome
	{
		/// <summary>
		/// The feature index of the outcome.
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

		public GameOutcome(int featureIndex, IReadOnlyList<IFoundationPrize> prizes, bool isFinalOutcome)
		{
			FeatureIndex = featureIndex;
			Prizes = prizes;
			IsFinalOutcome = isFinalOutcome;
		}
	}
}