using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.Stakes
{
	public sealed class StakesController : IPresentationController, IButtonControllerOwner, IPlayerSessionReset
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private StakesStatus stakesStatus;
		private readonly int maxStakeButtons;

		public IReadOnlyList<IButtonController> ButtonControllers { get; } = new IButtonController[]
		{
			new ClassicStakeButtonController(),
			new CostToCoverStakeButtonController()
		};

		public void Init()
		{
			stakesStatus = StatusDatabase.StakesStatus;

			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.StakeCombinations), OnStakeCombinationsChanged);
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.SelectedStakeCombinationIndex), OnSelectedStakeCombinationChanged);

			// By default, configure for 6 buttons classic bet dominant layout.

			ConfigureClassicButtons(6, 6);
		}

		public void DeInit()
		{
			autoUnregisterHelper.UnRegisterAll();
		}

		public void Destroy()
		{
		}

		public void ConfigureClassicButtons(int betButtonCount, int playButtonCount, bool stakeDominant = false, bool playFromPlayButtonOnly = false)
		{
			if (betButtonCount > StakeButtonFunctions.BetButtons.Count)
			{
				Log.Instance.Warn($"Attempted to create {betButtonCount} bet buttons, max is {StakeButtonFunctions.BetButtons.Count}");
				betButtonCount = StakeButtonFunctions.BetButtons.Count;
			}

			if (playButtonCount > StakeButtonFunctions.PlayButtons.Count)
			{
				Log.Instance.Warn($"Attempted to create {playButtonCount} play buttons, max is {StakeButtonFunctions.PlayButtons.Count}");
				playButtonCount = StakeButtonFunctions.PlayButtons.Count;
			}

			stakesStatus.StakeMode = stakeDominant ? StakeMode.ClassicStakeDominant : StakeMode.ClassicBetDominant;
			stakesStatus.PlayFromPlayButtonOnly = playFromPlayButtonOnly;
			stakesStatus.BetButtonFunctions = StakeButtonFunctions.BetButtons.Take(betButtonCount).ToArray();
			stakesStatus.PlayButtonFunctions = StakeButtonFunctions.PlayButtons.Take(playButtonCount).ToArray();
			stakesStatus.StakeButtonFunctions = Array.Empty<ButtonFunction>();
			UpdateButtonInformation();
		}

		public void ConfigureCostToCoverButtons(int buttonCount, bool playFromPlayButtonOnly = false)
		{
			if (buttonCount > StakeButtonFunctions.StakeButtons.Count)
			{
				Log.Instance.Warn($"Attempted to create {buttonCount} cost to cover buttons, max is {StakeButtonFunctions.StakeButtons.Count}");
				buttonCount = StakeButtonFunctions.StakeButtons.Count;
			}

			stakesStatus.StakeMode = StakeMode.CostToCover;
			stakesStatus.PlayFromPlayButtonOnly = playFromPlayButtonOnly;
			stakesStatus.BetButtonFunctions = Array.Empty<ButtonFunction>();
			stakesStatus.PlayButtonFunctions = Array.Empty<ButtonFunction>();
			stakesStatus.StakeButtonFunctions = StakeButtonFunctions.StakeButtons.Take(buttonCount).ToArray();
			UpdateButtonInformation();
		}

		public void ResetForNewPlayerSession(IReadOnlyList<PlayerSessionParameterType> pendingResetParams, IList<PlayerSessionParameterType> resetDoneParams)
		{
			if (!pendingResetParams.Contains(PlayerSessionParameterType.BetSelection))
				return;

			resetDoneParams.Add(PlayerSessionParameterType.BetSelection);

			var defaultStakeSettings = StatusDatabase.ConfigurationStatus.GameConfig.DefaultStakeSettings;
			var useMaxBetMultiplier = defaultStakeSettings.UseMaximumBetMultiplier.GetValueOrDefault();
			var useMaxStake = defaultStakeSettings.UseMaximumNumberOfLines.GetValueOrDefault();
			var includeSideBet = defaultStakeSettings.IncludeSideBet.GetValueOrDefault();

			var betStakeCombos = GetBetMultiplierStakeCombos();
			var canCheckSideBet = CanCheckSideBet();

			var lines = MinOrMax(betStakeCombos.Select(GetStake), useMaxStake);

			var validStakeCombinations = betStakeCombos.Where(sc => GetStake(sc) == lines &&
				(!canCheckSideBet || HasValidAnteBet(sc) == includeSideBet)).ToArray();

			var newIndex = 0;
			if (validStakeCombinations.Length > 0)
			{
				var ddd = validStakeCombinations.OrderBy(vsc => vsc.TotalBet.Credits).ToArray();
				newIndex = StatusDatabase.GameStatus.StakeCombinations.FindIndex(ddd[0]);
			}

			Communication.ToLogicSender.Send(new GameStakeCombinationMessage(newIndex));

			IStakeCombination[] GetBetMultiplierStakeCombos()
			{
				var stakeGroups = StatusDatabase.StakesStatus.StakeGroups;
				var betMultiplier = useMaxBetMultiplier ? stakeGroups[StatusDatabase.StakesStatus.StakeGroups.Count - 1] : stakeGroups[0];
				return betMultiplier.stakeCombinationIndices.Select(i => StatusDatabase.GameStatus.StakeCombinations[i]).ToArray();
			}

			long MinOrMax(IEnumerable<long> values, bool max) => max ? values.Max() : values.Min();

			long GetStake(IStakeCombination stakeCombination)
			{
				stakeCombination.Values.TryGetValue(Stake.LinesBet, out var linesBet);
				stakeCombination.Values.TryGetValue(Stake.BetMultiplier, out var multiwayBet);
				return linesBet + multiwayBet;
			}

			bool HasValidAnteBet(IStakeCombination stakeCombination)
			{
				if (stakeCombination.Values.TryGetValue(Stake.AnteBet, out var anteBet))
					return anteBet != 0;
				return false;
			}

			bool CanCheckSideBet()
			{
				if (!defaultStakeSettings.IncludeSideBet.HasValue)
					return false;

				// Only check if a bet has side bet if all the possible bets have an ante bet.
				var numSideBetCombinations = betStakeCombos.Count(HasValidAnteBet);
				return numSideBetCombinations > 0 && numSideBetCombinations == betStakeCombos.Length;
			}
		}

		private void OnStakeCombinationsChanged(StatusBlock sender, string propertyname)
		{
			// When the stake combinations change, we refresh the stake groups to configure the bet/play buttons.

			var stakeCombos = StatusDatabase.GameStatus.StakeCombinations;
			var stakeGroups = stakeCombos
				.GroupBy(sc => sc.GetBetMultiplier())
				.OrderBy(g => g.Key)
				.Select(g => (g.Key, (IReadOnlyList<int>)g.OrderBy(c => c.TotalBet).Select(c => stakeCombos.FindIndex(c)).ToArray()))
				.ToArray();

			Log.Instance.InfoFormat("Found {0} stake multipliers", stakeGroups.Length);

			stakesStatus.StakeGroups = stakeGroups;
			UpdateButtonInformation();
			RefreshSelectedStakeGroup();
		}

		private void UpdateButtonInformation()
		{
			if (StatusDatabase.GameStatus.StakeCombinations == null || stakesStatus.StakeGroups == null)
			{
				stakesStatus.BetButtonCount = 0;
				stakesStatus.PlayButtonCount = 0;
				stakesStatus.StakeButtonCount = 0;
				return;
			}

			switch (stakesStatus.StakeMode)
			{
				case StakeMode.ClassicBetDominant:
				case StakeMode.ClassicStakeDominant:
					stakesStatus.BetButtonCount = Math.Min(stakesStatus.StakeGroups.Count, stakesStatus.BetButtonFunctions.Count);
					stakesStatus.PlayButtonCount = Math.Min(stakesStatus.StakeGroups?.Max(g => g.stakeCombinationIndices.Count) ?? 0, stakesStatus.PlayButtonFunctions.Count);
					stakesStatus.StakeButtonCount = 0;
					break;

				case StakeMode.CostToCover:
					stakesStatus.BetButtonCount = 0;
					stakesStatus.PlayButtonCount = 0;
					stakesStatus.StakeButtonCount = Math.Min(StatusDatabase.GameStatus.StakeCombinations.Count, stakesStatus.StakeButtonFunctions?.Count ?? 0);
					break;
			}
		}

		private void OnSelectedStakeCombinationChanged(StatusBlock sender, string propertyname)
		{
			RefreshSelectedStakeGroup();
		}

		private void RefreshSelectedStakeGroup()
		{
			if (GameStatus.SelectedStakeCombination == null)
				return;

			var sm = GameStatus.SelectedStakeCombination.GetBetMultiplier();
			stakesStatus.SelectedStakeGroup = stakesStatus.StakeGroups.FindIndex(sg => sg.stakeMultiplier == sm);
		}
	}
}