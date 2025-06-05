using System.Linq;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.General;

namespace Midas.Presentation.Info
{
	public sealed partial class InfoController
	{
		private void UpdateGameInfoStatus()
		{
			var jackpotRtp = GetGameLinkJackpotRtp();

			var mintrtp = StatusDatabase.PidStatus.MinGameRtp + jackpotRtp;
			var maxtrtp = StatusDatabase.PidStatus.MaxGameRtp + jackpotRtp;
			var gamesPlayedPerWin = StatusDatabase.PidStatus.GamesPerWin;
			var minimumMaximumBets = GetMinimumAndMaximumBets();

			var config = StatusDatabase.PidStatus.Config;
			var hasJackpot = config.TotalNumberLinkEnrolments > 0;

			var jackpotMsg = string.Empty;
			if (hasJackpot)
				jackpotMsg = config.ShowLinkJackpotCount ? $"This game is part of {config.TotalNumberLinkEnrolments} linked jackpot/s" : "This game is part of a linked jackpot";

			var minRtpString = $"{mintrtp:F2}%";
			var maxRtpString = $"{maxtrtp:F2}%";
			var jackpotRtpString = $"{jackpotRtp:F2}";

			var rtpString = minRtpString;
			if (minRtpString != maxRtpString)
				rtpString = $"{minRtpString} - {maxRtpString}";

			var minBet = StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter.GetFormatted(MoneyAndCreditDisplayMode.MoneyWholePlusBase, minimumMaximumBets.minimumBet, CreditDisplaySeparatorMode.NoSeparator);
			var maxBet = StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter.GetFormatted(MoneyAndCreditDisplayMode.MoneyWholePlusBase, minimumMaximumBets.maximumBet, CreditDisplaySeparatorMode.NoSeparator);

			infoStatus.GameInformation = $"{jackpotMsg}\n" +
				$"{(hasJackpot ? "Total " : string.Empty)}Theoretical {GetRtpString()} of this game = {rtpString}\n" +
				$"{(hasJackpot ? $"Jackpot Contribution = {jackpotRtpString}%" : string.Empty)}\n" +
				$"{NzSapInformation()}\n" +
				$"\nTheoretical number of individual games played per win = {gamesPlayedPerWin:F2}\n" +
				$"Minimum Bet = {minBet} Maximum Bet = {maxBet}\n";

			infoStatus.ChanceOfWinningText = GetChancesOfWinningString();
			UpdatePrizeCombinations();
		}

		private static (Money minimumBet, Money maximumBet) GetMinimumAndMaximumBets()
		{
			var min = StatusDatabase.GameStatus.StakeCombinations.Min(sc => sc.TotalBet);
			var max = StatusDatabase.GameStatus.StakeCombinations.Max(sc => sc.TotalBet);

			var minimumBet = Money.FromCredit(min);
			var maximumBet = Money.FromCredit(max);
			if (!StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction.Contains("CRN") || minimumBet <= maximumBet)
				return (minimumBet, maximumBet);

			return (Money.Zero, Money.Zero);
		}

		private static string GetRtpString() => StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction.Contains("VSI") || StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction.Contains("CRN") || StatusDatabase.ConfigurationStatus.QcomJurisdiction == 3
			? "Return to Players"
			: "Return to Player";

		private void UpdatePrizeCombinations()
		{
			var lPrizes = string.Join("\n", StatusDatabase.PidStatus.LargestPrizes.Select(lp => lp.Prize));
			var lOdds = string.Join("\n", StatusDatabase.PidStatus.LargestPrizes.Select(lp => $"1 in {lp.Odds}"));
			var sPrizes = string.Join("\n", StatusDatabase.PidStatus.LargestPrizes.Select(lp => lp.Prize));
			var sOdds = string.Join("\n", StatusDatabase.PidStatus.LargestPrizes.Select(lp => $"1 in {lp.Odds}"));

			infoStatus.TopFiveOdds = lOdds;
			infoStatus.TopFivePrizes = lPrizes;
			infoStatus.BottomFivePrizes = sPrizes;
			infoStatus.BottomFiveOdds = sOdds;
		}

		private static double GetGameLinkJackpotRtp()
		{
			var jackpotRtp = StatusDatabase.PidStatus.Config.LinkRtpForGameRtp;
			if (jackpotRtp == 0.0)
				double.TryParse(StatusDatabase.PidStatus.Config.TotalLinkPercentageContributions, out jackpotRtp);

			return jackpotRtp;
		}

		private static string NzSapInformation()
		{
			var isNz = StatusDatabase.ConfigurationStatus.MachineConfig.Jurisdiction.Contains("NZ") || StatusDatabase.ConfigurationStatus.QcomJurisdiction == 2;
			if (!isNz)
				return string.Empty;

			var numberOfSapProgressives = 0;
			var sapRtp = 0.0;
			foreach (var progressiveLevel in StatusDatabase.ProgressiveStatus.ProgressiveLevels)
			{
				if (!progressiveLevel.IsStandalone)
					continue;
				numberOfSapProgressives++;
				sapRtp += progressiveLevel.Rtp;
			}

			return numberOfSapProgressives == 0 ? string.Empty : $"\nThis game is part of {numberOfSapProgressives} standalone jackpot/s\nStandalone Jackpot Contribution = {sapRtp:F2}%";
		}

		private static string GetChancesOfWinningString() => $"(Based on a 1 line, 1 credit bet with no feature wins)";
	}
}