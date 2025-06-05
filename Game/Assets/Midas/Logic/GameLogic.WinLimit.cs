using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;

namespace Midas.Logic
{
	public sealed partial class GameLogic
	{
		private class WinCapAdjustment : IFoundationPrize
		{
			public string PrizeName => "Win Cap Adjustment";
			public Money RiskAmount => Money.Zero;
			public Money Amount { get; }
			public IProgressiveHit ProgressiveHit => null;

			public WinCapAdjustment(Money winCapLimit, Money totalPrize)
			{
				Amount = winCapLimit - totalPrize;
			}
		}

		private class WinCappedOutcome : IOutcome
		{
			public int FeatureIndex { get; }
			public IReadOnlyList<IFoundationPrize> Prizes { get; }
			public bool IsFinalOutcome => true;

			public WinCappedOutcome(IOutcome originalOutcome, Money winCapLimit, Money totalPrize)
			{
				FeatureIndex = originalOutcome.FeatureIndex;
				var newPrizes = originalOutcome.Prizes.ToList();
				newPrizes.Add(new WinCapAdjustment(winCapLimit, totalPrize));
				Prizes = newPrizes;
			}
		}

		private IOutcome CheckWinLimit(IOutcome currentResults)
		{
			if (foundation.WinCapStyle == WinCapStyle.None)
				return currentResults;

			var winCapLimit = foundation.WinCapLimit;
			if (winCapLimit.IsZero)
				return currentResults;

			var pendingProgressiveAward = GetProgressiveValue(currentResults);
			var totalPrize = GameServices.MetersService.TotalAwardService.Value + currentResults.GetTotalPrizes() + logicState.TotalProgressiveAwardedValue + pendingProgressiveAward;
			var winCapReached = totalPrize >= winCapLimit;

			// if (winCapStoreAndForward.Enabled)
			// 	ApplyFeatureAwardWinCap(framework, outcomeList, winCapReached);

			if (!winCapReached)
				return currentResults;

			currentResults = new WinCappedOutcome(currentResults, winCapLimit, totalPrize);

			logicState.HasWinCapBeenReached = true;
			GameServices.MachineStateService.HasWinCapBeenReachedService.SetValue(true);
			SaveLogicState();

			return currentResults;
		}

		private static Money GetProgressiveValue(IOutcome currentResults)
		{
			var progressiveData = GameServices.ProgressiveService.BroadcastDataService.Value;

			var triggeredProgressives = currentResults.Prizes.Where(p => p.ProgressiveHit != null).Select(p => p.ProgressiveHit.LevelId).ToList();

			if (triggeredProgressives.Count != triggeredProgressives.Distinct().Count())
				throw new Exception($"Multiple triggers of the same progressive is not supported for win capping {string.Join(",", triggeredProgressives)}.");

			var totalProgressiveWinAmount = Money.Zero;
			for (var index = 0; index < triggeredProgressives.Count; index++)
				totalProgressiveWinAmount += progressiveData.Single(kp => kp.LevelId == triggeredProgressives[index]).Value;

			return totalProgressiveWinAmount;
		}
	}
}