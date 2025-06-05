using System.Collections.Generic;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Types;
using Midas.Core;

namespace Midas.Gle.Logic
{
	public interface IGleOutcomeConverter
	{
		/// <summary>
		/// Generate an outcome list from the game cycle results.
		/// </summary>
		/// <param name="cycleResults">The cycle results.</param>
		/// <returns>A list of foundation prizes.</returns>
		IReadOnlyList<IFoundationPrize> GenerateFoundationPrizes(CycleResult cycleResult);
	}

	public class DefaultOutcomeConverter : IGleOutcomeConverter
	{
		#region IOutcomeConverter implementation

		/// <summary>
		/// Generate an outcome list from the game cycle results.
		/// </summary>
		/// <param name="cycleResult">The cycle results.</param>
		/// <returns>A collection of foundation prizes.</returns>
		public IReadOnlyList<IFoundationPrize> GenerateFoundationPrizes(CycleResult cycleResult)
		{
			var result = new List<IFoundationPrize>();

			GetData(cycleResult, out var progressives, out var prizes, out var credits);

			var unhandledProgressives = cycleResult.Progressives.ToList();

			foreach (var progressive in progressives)
				unhandledProgressives.Remove(progressive.result.Progressive);

			AwardPrizes(prizes, credits, result);
			AwardProgressivePrizes(progressives, result);
			AddNonPrizeMappedProgressives(unhandledProgressives, result);

			return result;
		}

		#endregion

		#region Private Methods

		private void AwardProgressivePrizes(IReadOnlyList<(string resultName, int index, ProgressivePrizeResult result)> progressives, ICollection<IFoundationPrize> result)
		{
			foreach (var progressive in progressives)
				result.Add(OutcomeConverterHelper.CreatePrizeMappedProgressive(progressive.result.Name, progressive.result.Count, progressive.result.Progressive, progressive.resultName, progressive.index));
		}

		private static void AwardPrizes(IReadOnlyList<CellPrizeResult> prizeResults, Credits awardedCredits, ICollection<IFoundationPrize> prizes)
		{
			var totalAwardedCredits = 0L;
			var prizeCount = 0;

			foreach (var prize in prizeResults)
			{
				totalAwardedCredits += prize.Value;
				prizeCount++;
			}

			if (awardedCredits != Credits.Zero)
			{
				totalAwardedCredits += (long)awardedCredits.ToUInt64();
				prizeCount += 1;
			}

			if (prizeCount > 0)
				prizes.Add(OutcomeConverterHelper.CreateSummarisedCreditPrize(prizeCount, totalAwardedCredits));
		}

		private static void GetData(CycleResult gameCycleResults, out IReadOnlyList<(string resultName, int index, ProgressivePrizeResult result)> progressives, out IReadOnlyList<CellPrizeResult> prizeResults, out Credits awardedCredits)
		{
			var prog = new List<(string resultName, int index, ProgressivePrizeResult result)>();
			var pr = new List<CellPrizeResult>();
			awardedCredits = Credits.Zero;

			foreach (var result in gameCycleResults.StageResults)
			{
				switch (result.Type)
				{
					case StageResultType.ProgressiveList:
					{
						if (!(result.Value is IReadOnlyList<ProgressivePrizeResult> ppr))
							continue;

						prog.AddRange(ppr.Select((p, i) => (result.Name, i, p)));
						break;
					}

					case StageResultType.AwardCreditsList:
					{
						switch (result.Value)
						{
							case IReadOnlyList<CellPrizeResult> cpr:
								pr.AddRange(cpr);
								break;
							case IReadOnlyList<Credits> cpc:
								foreach (var c in cpc)
									awardedCredits += c;
								break;
						}

						break;
					}
				}
			}

			progressives = prog;
			prizeResults = pr;
		}

		private void AddNonPrizeMappedProgressives(IReadOnlyList<string> unhandledProgressives, ICollection<IFoundationPrize> prizes)
		{
			foreach (var progressive in unhandledProgressives)
				prizes.Add(OutcomeConverterHelper.CreateNonPrizedMappedProgressive(progressive));
		}

		#endregion
	}
}