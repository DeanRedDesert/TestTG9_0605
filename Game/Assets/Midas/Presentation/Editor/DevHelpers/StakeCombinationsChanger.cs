using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.Data;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.DevHelpers
{
	public static class StakeCombinationsChanger
	{
		private static IReadOnlyList<IStakeCombination> trueStakes;
		private static int selectedStake;

		private sealed class FakeStakeCombination : IStakeCombination
		{
			public IReadOnlyDictionary<Stake, long> Values { get; }
			public Credit TotalBet { get; }
			public BetCategory BetCategory => BetCategory.BetCategory0;

			public FakeStakeCombination(IReadOnlyDictionary<Stake, long> values, Credit totalBet)
			{
				Values = values;
				TotalBet = totalBet;
			}
		}

		private const string StakeComboMenu = "Midas/Debug/Stake Combos/";

		[MenuItem(StakeComboMenu + "Restore", validate = true)]
		private static bool CanRestoreStakes() => Application.isPlaying && trueStakes != null;

		[MenuItem(StakeComboMenu + "Restore")]
		private static void RestoreStakes()
		{
			StatusDatabase.GameStatus.OverrideStakeCombinations(trueStakes, selectedStake);
		}

		[MenuItem(StakeComboMenu + "Lines", validate = true)]
		[MenuItem(StakeComboMenu + "Multiway", validate = true)]
		[MenuItem(StakeComboMenu + "Lines+Multiway", validate = true)]
		[MenuItem(StakeComboMenu + "Lines+Ante", validate = true)]
		[MenuItem(StakeComboMenu + "Multiway+Ante", validate = true)]
		private static bool CanChangeStakeCombo()
		{
			return Application.isPlaying;
		}

		[MenuItem(StakeComboMenu + "Lines")]
		private static void LinesStakeCombos()
		{
			var stakes = new[] { 1L, 2L, 3L, 5L, 10L };
			var lines = new[] { 1L, 5L, 10L, 20L, 50L };

			var stakeCombinations = new List<IStakeCombination>();

			foreach (var stake in stakes)
			{
				foreach (var line in lines)
				{
					var bet = Credit.FromLong(stake * line);
					var inputs = new Dictionary<Stake, long>
					{
						{ Stake.BetMultiplier, stake },
						{ Stake.LinesBet, line }
					};

					stakeCombinations.Add(new FakeStakeCombination(inputs, bet));
				}
			}

			SaveTrueStakes();
			StatusDatabase.GameStatus.OverrideStakeCombinations(stakeCombinations.OrderBy(c => c.TotalBet).ToArray());
		}

		[MenuItem(StakeComboMenu + "Multiway")]
		private static void MultiwayStakeCombos()
		{
			var stakes = new[] { 1L, 2L, 3L, 5L, 10L };
			var ways = new[] { 1L, 5L, 10L, 20L, 50L };

			var stakeCombinations = new List<IStakeCombination>();

			foreach (var stake in stakes)
			{
				foreach (var way in ways)
				{
					var bet = Credit.FromLong(stake * way);
					var inputs = new Dictionary<Stake, long>
					{
						{ Stake.BetMultiplier, stake },
						{ Stake.Multiway, way }
					};

					stakeCombinations.Add(new FakeStakeCombination(inputs, bet));
				}
			}

			SaveTrueStakes();
			StatusDatabase.GameStatus.OverrideStakeCombinations(stakeCombinations.OrderBy(c => c.TotalBet).ToArray());
		}

		[MenuItem(StakeComboMenu + "Lines+Multiway")]
		private static void LinesMultiwayStakeCombos()
		{
			var stakes = new[] { 1L, 2L, 3L, 5L, 10L };
			var lines = new[] { 1L, 2L, 5L, 10L, 20L, 0L };
			var ways = new[] { 0L, 0L, 0L, 0L, 0L, 30L };

			var stakeCombinations = new List<IStakeCombination>();

			foreach (var stake in stakes)
			{
				for (var index = 0; index < ways.Length; index++)
				{
					var way = ways[index];
					var line = lines[index];
					var bet = Credit.FromLong(stake * (way + line));
					var inputs = new Dictionary<Stake, long>
					{
						{ Stake.BetMultiplier, stake },
						{ Stake.Multiway, way },
						{ Stake.LinesBet, line }
					};

					stakeCombinations.Add(new FakeStakeCombination(inputs, bet));
				}
			}

			SaveTrueStakes();
			StatusDatabase.GameStatus.OverrideStakeCombinations(stakeCombinations.OrderBy(c => c.TotalBet).ToArray());
		}

		[MenuItem(StakeComboMenu + "Lines+Ante")]
		private static void LinesAnteStakeCombos()
		{
			var stakes = new[] { 1L, 2L, 3L, 5L, 6L, 10L };
			var lines = new[] { 10L, 20L };
			var antes = new[] { 10L, 10L };

			var stakeCombinations = new List<IStakeCombination>();

			foreach (var stake in stakes)
			{
				for (var index = 0; index < lines.Length; index++)
				{
					var ante = antes[index];
					var line = lines[index];
					var bet = Credit.FromLong(stake * (ante + line));
					var inputs = new Dictionary<Stake, long>
					{
						{ Stake.BetMultiplier, stake },
						{ Stake.AnteBet, ante },
						{ Stake.LinesBet, line }
					};

					stakeCombinations.Add(new FakeStakeCombination(inputs, bet));
				}
			}

			SaveTrueStakes();
			StatusDatabase.GameStatus.OverrideStakeCombinations(stakeCombinations.OrderBy(c => c.TotalBet).ToArray());
		}

		[MenuItem(StakeComboMenu + "Multiway+Ante")]
		private static void MultiwayAnteStakeCombos()
		{
			var stakes = new[] { 1L, 2L, 3L, 5L, 6L, 10L };
			var ways = new[] { 10L, 20L };
			var antes = new[] { 10L, 10L };

			var stakeCombinations = new List<IStakeCombination>();

			foreach (var stake in stakes)
			{
				for (var index = 0; index < ways.Length; index++)
				{
					var ante = antes[index];
					var way = ways[index];
					var bet = Credit.FromLong(stake * (ante + way));
					var inputs = new Dictionary<Stake, long>
					{
						{ Stake.BetMultiplier, stake },
						{ Stake.AnteBet, ante },
						{ Stake.Multiway, way }
					};

					stakeCombinations.Add(new FakeStakeCombination(inputs, bet));
				}
			}

			SaveTrueStakes();
			StatusDatabase.GameStatus.OverrideStakeCombinations(stakeCombinations.OrderBy(c => c.TotalBet).ToArray());
		}

		private static void SaveTrueStakes()
		{
			if (trueStakes != null)
				return;

			trueStakes = StatusDatabase.GameStatus.StakeCombinations;
			selectedStake = StatusDatabase.GameStatus.SelectedStakeCombinationIndex;
		}
	}
}