using System;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.WinPresentation;

namespace Game.GameIdentity.Global
{
	public sealed class GiGlobalWinCountRanges : IWinCountRanges
	{
		public (int WinCountLevel, TimeSpan Duration, TimeSpan Delay) GetWinCountLevel(Credit winAmount, Credit betAmount)
		{
			var totalBetIndex = GetTotalBetTimeTableIndex(betAmount);

			var timeTable = GetTimeTable();
			var winValue = winAmount.Credits;
			for (var idx = 0; idx < timeTable.TimeMap.Length; idx++)
			{
				if (winValue < timeTable.TimeMap[idx].amounts[totalBetIndex])
				{
					return (1, timeTable.TimeMap[idx].countTime, TimeSpan.Zero);
				}
			}

			return (1, timeTable.TimeMap[timeTable.TimeMap.Length - 1].countTime, TimeSpan.Zero);
		}

		public bool IsSequenceEligible(int sequenceId) => true;

		private static int GetTotalBetTimeTableIndex(Credit totalBet)
		{
			var timeTable = GetTimeTable();

			var creditValue = totalBet.Credits;
			for (var i = 0; i < timeTable.ColumnSelector.Length; i++)
			{
				if (creditValue < timeTable.ColumnSelector[i])
					return i;
			}

			return timeTable.ColumnSelector.Length - 1;
		}

		private static TimeTable GetTimeTable()
		{
			return StatusDatabase.GameSpeedStatus.GameSpeed switch
			{
				GameSpeed.Fast => fastTimes,
				GameSpeed.SuperFast => superFastTimes,
				_ => normalTimes
			};
		}

		private sealed class TimeTable
		{
			public int[] ColumnSelector { get; set; }
			public (TimeSpan countTime, int[] amounts)[] TimeMap { get; set; }
		}

		private static readonly TimeTable normalTimes = new TimeTable
		{
			ColumnSelector = new[] { 6, 16, 31, 76, 100, 200, 301, 601, 1001, 2501, int.MaxValue },
			TimeMap = new (TimeSpan countTime, int[] amounts)[]
			{
				(TimeSpan.FromSeconds(0.6), new[] { 5, 10, 20, 20, 20, 30, 50, 80, 100, 150, 200 }),
				(TimeSpan.FromSeconds(1.0), new[] { 10, 20, 41, 41, 41, 61, 101, 151, 201, 301, 401 }),
				(TimeSpan.FromSeconds(1.4), new[] { 20, 30, 60, 60, 60, 100, 150, 200, 300, 450, 700 }),
				(TimeSpan.FromSeconds(1.6), new[] { 30, 40, 80, 80, 80, 150, 200, 300, 450, 600, 1000 }),
				(TimeSpan.FromSeconds(1.8), new[] { 40, 50, 100, 100, 125, 200, 300, 400, 600, 800, 1500 }),
				(TimeSpan.FromSeconds(2.5), new[] { 50, 70, 130, 130, 150, 250, 400, 500, 800, 1000, 2000 }),
				(TimeSpan.FromSeconds(3.0), new[] { 70, 100, 160, 160, 200, 300, 500, 750, 1000, 1500, 3000 }),
				(TimeSpan.FromSeconds(3.5), new[] { 90, 150, 200, 200, 250, 400, 650, 1000, 1500, 2000, 4000 }),
				(TimeSpan.FromSeconds(5.0), new[] { 120, 200, 300, 300, 400, 600, 800, 1500, 2000, 3000, 6000 }),
				(TimeSpan.FromSeconds(6.5), new[] { 160, 300, 400, 400, 600, 800, 1000, 2000, 3000, 4000, 8000 }),
				(TimeSpan.FromSeconds(8.0), new[] { 200, 400, 600, 600, 800, 1250, 1500, 3000, 4000, 6000, 10000 }),
				(TimeSpan.FromSeconds(10.0), new[] { 300, 600, 800, 800, 1000, 1750, 2500, 4000, 5000, 8000, 15000 }),
				(TimeSpan.FromSeconds(16.0), new[] { 500, 800, 1250, 1250, 1500, 2500, 3500, 5000, 7500, 10000, 20000 }),
				(TimeSpan.FromSeconds(25.0), new[] { int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue }),
			}
		};

		private static readonly TimeTable fastTimes = new TimeTable
		{
			ColumnSelector = new[] { 6, 16, 31, 76, 100, 200, 301, 601, 1001, 2501, int.MaxValue },
			TimeMap = new (TimeSpan countTime, int[] amounts)[]
			{
				(TimeSpan.FromSeconds(0.6), new[] { 10, 20, 41, 41, 41, 61, 101, 151, 201, 301, 401 }),
				(TimeSpan.FromSeconds(1.0), new[] { 20, 30, 60, 60, 60, 100, 150, 200, 300, 450, 700 }),
				(TimeSpan.FromSeconds(1.4), new[] { 30, 40, 80, 80, 80, 150, 200, 300, 450, 600, 1000 }),
				(TimeSpan.FromSeconds(1.6), new[] { 40, 50, 100, 100, 125, 200, 300, 400, 600, 800, 1500 }),
				(TimeSpan.FromSeconds(1.8), new[] { 50, 70, 130, 130, 150, 250, 400, 500, 800, 1000, 2000 }),
				(TimeSpan.FromSeconds(2.5), new[] { 70, 100, 160, 160, 200, 300, 500, 750, 1000, 1500, 3000 }),
				(TimeSpan.FromSeconds(3.0), new[] { 90, 150, 200, 200, 250, 400, 650, 1000, 1500, 2000, 4000 }),
				(TimeSpan.FromSeconds(3.5), new[] { 120, 200, 300, 300, 400, 600, 800, 1500, 2000, 3000, 6000 }),
				(TimeSpan.FromSeconds(5.0), new[] { 160, 300, 400, 400, 600, 800, 1000, 2000, 3000, 4000, 8000 }),
				(TimeSpan.FromSeconds(6.5), new[] { 200, 400, 600, 600, 800, 1250, 1500, 3000, 4000, 6000, 10000 }),
				(TimeSpan.FromSeconds(8.0), new[] { 300, 600, 800, 800, 1000, 1750, 2500, 4000, 5000, 8000, 15000 }),
				(TimeSpan.FromSeconds(10.0), new[] { 500, 800, 1250, 1250, 1500, 2500, 3500, 5000, 7500, 10000, 20000 }),
				(TimeSpan.FromSeconds(16.0), new[] { int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue }),
			}
		};

		private static readonly TimeTable superFastTimes = new TimeTable
		{
			ColumnSelector = new[] { 6, 16, 31, 76, 100, 200, 301, 601, 1001, 2501, int.MaxValue },
			TimeMap = new (TimeSpan countTime, int[] amounts)[]
			{
				(TimeSpan.FromSeconds(0.6), new[] { 20, 30, 60, 60, 60, 100, 150, 200, 300, 450, 700 }),
				(TimeSpan.FromSeconds(1.0), new[] { 30, 40, 80, 80, 80, 150, 200, 300, 450, 600, 1000 }),
				(TimeSpan.FromSeconds(1.4), new[] { 40, 50, 100, 100, 125, 200, 300, 400, 600, 800, 1500 }),
				(TimeSpan.FromSeconds(1.6), new[] { 50, 70, 130, 130, 150, 250, 400, 500, 800, 1000, 2000 }),
				(TimeSpan.FromSeconds(1.8), new[] { 70, 100, 160, 160, 200, 300, 500, 750, 1000, 1500, 3000 }),
				(TimeSpan.FromSeconds(2.5), new[] { 90, 150, 200, 200, 250, 400, 650, 1000, 1500, 2000, 4000 }),
				(TimeSpan.FromSeconds(3.0), new[] { 120, 200, 300, 300, 400, 600, 800, 1500, 2000, 3000, 6000 }),
				(TimeSpan.FromSeconds(3.5), new[] { 160, 300, 400, 400, 600, 800, 1000, 2000, 3000, 4000, 8000 }),
				(TimeSpan.FromSeconds(5.0), new[] { 200, 400, 600, 600, 800, 1250, 1500, 3000, 4000, 6000, 10000 }),
				(TimeSpan.FromSeconds(6.5), new[] { 300, 600, 800, 800, 1000, 1750, 2500, 4000, 5000, 8000, 15000 }),
				(TimeSpan.FromSeconds(8.0), new[] { 500, 800, 1250, 1250, 1500, 2500, 3500, 5000, 7500, 10000, 20000 }),
				(TimeSpan.FromSeconds(10.0), new[] { int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue }),
			}
		};
	}
}