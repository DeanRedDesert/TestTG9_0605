using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.Stakes
{
	/// <summary>
	/// Button controller that handles the classic 2 rows of 6 buttons DPP configuration where the top row has bet buttons and the bottom has play buttons
	/// </summary>
	public sealed class ClassicStakeButtonController : StakeButtonController
	{
		private readonly AutoUnregisterHelper betButtonRegistrationHelper = new AutoUnregisterHelper();
		private readonly AutoUnregisterHelper playButtonRegistrationHelper = new AutoUnregisterHelper();
		private readonly Dictionary<ButtonFunction, IStakeCombination> playButtonFunctionMapping = new Dictionary<ButtonFunction, IStakeCombination>();
		private readonly HashSet<ButtonFunction> buttonFunctionsToDisable = new HashSet<ButtonFunction>();

		#region ButtonController Overrides

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			RegisterPropertyChangedHandler<IReadOnlyList<ButtonFunction>>(StakesStatus, nameof(StakesStatus.BetButtonFunctions), OnBetButtonsChanged);
			RegisterPropertyChangedHandler<IReadOnlyList<ButtonFunction>>(StakesStatus, nameof(StakesStatus.PlayButtonFunctions), OnPlayButtonsChanged);
			RegisterBetButtons();
			RegisterPlayButtons();
		}

		protected override void UnregisterEvents()
		{
			betButtonRegistrationHelper.UnRegisterAll();
			playButtonRegistrationHelper.UnRegisterAll();
			base.UnregisterEvents();
		}

		private void EvaluateFunctionChanges(IReadOnlyList<ButtonFunction> newButtonFunctions, IReadOnlyList<ButtonFunction> oldButtonFunctions)
		{
			foreach (var buttonFunction in oldButtonFunctions)
			{
				if (newButtonFunctions.Contains(buttonFunction))
					buttonFunctionsToDisable.Remove(buttonFunction);
				else
					buttonFunctionsToDisable.Add(buttonFunction);
			}
		}

		protected override void UpdateButtonStates()
		{
			foreach (var buttonFunction in buttonFunctionsToDisable)
				AddButtonState(buttonFunction, ButtonState.DisabledHide);
			buttonFunctionsToDisable.Clear();

			if (StakesStatus.StakeMode != StakeMode.ClassicBetDominant && StakesStatus.StakeMode != StakeMode.ClassicStakeDominant)
				return;

			Log.Instance.InfoFormat("Configuring Classic panel, {0} bet and {1} play buttons", StakesStatus.BetButtonFunctions.Count, StakesStatus.PlayButtonFunctions.Count);

			var selectedStakeCombo = GameStatus.SelectedStakeCombination;
			if (StakesStatus.StakeGroups == null)
				return;

			var selectedStakeGroup = StakesStatus.StakeGroups.FindIndex(sg => sg.stakeMultiplier == selectedStakeCombo.GetBetMultiplier());

			CreateBetButtonStates(selectedStakeGroup);
			CreatePlayButtonStates(selectedStakeGroup, selectedStakeCombo);
		}

		#endregion

		#region Private Methods

		private void OnBetButtonsChanged(StatusBlock sender, string propertyname, IReadOnlyList<ButtonFunction> newButtonFunctions, IReadOnlyList<ButtonFunction> oldButtonFunctions)
		{
			EvaluateFunctionChanges(newButtonFunctions, oldButtonFunctions);
			betButtonRegistrationHelper.UnRegisterAll();
			RegisterBetButtons();
			RequestButtonUpdate();
		}

		private void OnPlayButtonsChanged(StatusBlock sender, string propertyname, IReadOnlyList<ButtonFunction> newButtonFunctions, IReadOnlyList<ButtonFunction> oldButtonFunctions)
		{
			EvaluateFunctionChanges(newButtonFunctions, oldButtonFunctions);
			playButtonRegistrationHelper.UnRegisterAll();
			RegisterPlayButtons();
			RequestButtonUpdate();
		}

		private void RegisterBetButtons()
		{
			if (StakesStatus.BetButtonFunctions == null)
				return;

			foreach (var betButton in StakesStatus.BetButtonFunctions)
				betButtonRegistrationHelper.RegisterButtonPressListener(betButton, OnBetButtonPressed);
		}

		private void RegisterPlayButtons()
		{
			if (StakesStatus.PlayButtonFunctions == null)
				return;

			foreach (var playButton in StakesStatus.PlayButtonFunctions)
				playButtonRegistrationHelper.RegisterButtonPressListener(playButton, OnPlayButtonPressed);
		}

		private void OnBetButtonPressed(ButtonEventData buttonEventData)
		{
			var stakeGroup = FindStakeGroup(buttonEventData.ButtonFunction);

			if (stakeGroup.HasValue && CanSelectBetButton(stakeGroup.Value))
			{
				StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
				ButtonQueueController.Enqueue(buttonEventData, IsStakeActionHindered, ExecuteBetStakeAction);
			}
		}

		private void ExecuteBetStakeAction(ButtonEventData buttonEventData)
		{
			var newStakeGroup = FindStakeGroup(buttonEventData.ButtonFunction);
			if (!newStakeGroup.HasValue || !CanSelectBetButton(newStakeGroup.Value))
				return;

			// Changing the bet requires us to find another matching stake combination in the new stake group.

			var selectedStakeCombo = GameStatus.SelectedStakeCombination;

			// Find one with the same stake information.

			var matchingStakeCombo = default(IStakeCombination);
			foreach (var stakeComboIndex in newStakeGroup.Value.stakeCombinationIndices)
			{
				var stakeCombo = StatusDatabase.GameStatus.StakeCombinations[stakeComboIndex];
				var isMatch = true;
				foreach (var kvp in selectedStakeCombo.Values)
				{
					if (kvp.Key != Stake.BetMultiplier && kvp.Value != stakeCombo.Values[kvp.Key])
					{
						isMatch = false;
						break;
					}
				}

				if (isMatch)
				{
					matchingStakeCombo = stakeCombo;
					break;
				}
			}

			if (matchingStakeCombo == null)
			{
				Log.Instance.ErrorFormat("Unable to match stake combinations between different bets");
			}

			var index = StatusDatabase.GameStatus.StakeCombinations.FindIndex(matchingStakeCombo);
			Communication.ToLogicSender.Send(new GameStakeCombinationMessage(index));
			if (StakesStatus.StakeMode == StakeMode.ClassicStakeDominant && !StakesStatus.PlayFromPlayButtonOnly)
				StartGame();
		}

		private void OnPlayButtonPressed(ButtonEventData buttonEventData)
		{
			if (playButtonFunctionMapping.TryGetValue(buttonEventData.ButtonFunction, out var stakeCombination) &&
				CanSelectPlayButton(stakeCombination))
			{
				StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
				ButtonQueueController.Enqueue(buttonEventData, IsStakeActionHindered, ExecutePlayStakeAction);
			}
		}

		private void ExecutePlayStakeAction(ButtonEventData buttonEventData)
		{
			var stakeCombo = playButtonFunctionMapping[buttonEventData.ButtonFunction];
			if (!CanSelectPlayButton(stakeCombo))
				return;

			var index = StatusDatabase.GameStatus.StakeCombinations.FindIndex(stakeCombo);
			Communication.ToLogicSender.Send(new GameStakeCombinationMessage(index));
			if (StakesStatus.StakeMode == StakeMode.ClassicBetDominant && !StakesStatus.PlayFromPlayButtonOnly)
				StartGame();
		}

		private ButtonFunctionHinderedReasons IsStakeActionHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseGameCycleActive | ButtonFunctionHinderedReasons.BecauseInterruptable);
		}

		private void CreateBetButtonStates(int selectedStakeGroup)
		{
			for (var i = 0; i < StakesStatus.BetButtonFunctions.Count; i++)
			{
				var buttonFunction = StakesStatus.BetButtonFunctions[i];
				if (i < StakesStatus.StakeGroups.Count)
				{
					var stakeGroup = StakesStatus.StakeGroups[i];
					var enabled = CanSelectBetButton(stakeGroup);
					var isVisible = StatusDatabase.GameFunctionStatus.GameButtonBehaviours.BetButtons.IsVisible();
					var isSelected = i == selectedStakeGroup;
					var hasAnte = stakeGroup.stakeCombinationIndices.Any(i => StatusDatabase.GameStatus.StakeCombinations[i].Values.ContainsKey(Stake.AnteBet));
					var betButtonData = new BetButtonSpecificData(stakeGroup.stakeMultiplier, hasAnte, isSelected);
					var bs = isVisible ? enabled ? ButtonState.Enabled : ButtonState.DisabledShow : ButtonState.DisabledHide;

					Log.Instance.InfoFormat("Creating bet {0} button Function {1}", betButtonData.StakeMultiplier, buttonFunction.Name);

					AddButtonState(buttonFunction, bs, betButtonData);
				}
				else
				{
					AddButtonState(buttonFunction, ButtonState.DisabledHide);
				}
			}
		}

		private bool CanSelectBetButton((long stakeMultiplier, IReadOnlyList<int> stakeCombinationIndices) betButtonInfo)
		{
			var stakeCombo = StatusDatabase.GameStatus.StakeCombinations[betButtonInfo.stakeCombinationIndices[0]];
			return CanEnableButton(Money.FromCredit(stakeCombo.TotalBet));
		}

		private (long stakeMultiplier, IReadOnlyList<int> stakeCombinationIndices)? FindStakeGroup(ButtonFunction buttonFunction)
		{
			var groupIndex = StakesStatus.BetButtonFunctions.FindIndex(buttonFunction);
			if (groupIndex == -1 || groupIndex >= StakesStatus.StakeGroups.Count)
				return null;

			return StakesStatus.StakeGroups[groupIndex];
		}

		private void CreatePlayButtonStates(int selectedStakeGroup, IStakeCombination selectedStakeCombo)
		{
			playButtonFunctionMapping.Clear();
			var stakeCombinationIndices = StakesStatus.StakeGroups[selectedStakeGroup].stakeCombinationIndices;
			for (var i = 0; i < StakeButtonFunctions.PlayButtons.Count; i++)
			{
				var buttonFunction = StakeButtonFunctions.PlayButtons[i];
				if (i < stakeCombinationIndices.Count)
				{
					var stakeCombination = StatusDatabase.GameStatus.StakeCombinations[stakeCombinationIndices[i]];
					var enabled = CanSelectPlayButton(stakeCombination);
					var isVisible = StatusDatabase.GameFunctionStatus.GameButtonBehaviours.BetButtons.IsVisible();
					var isSelected = ReferenceEquals(selectedStakeCombo, stakeCombination);
					var playButtonData = new PlayButtonSpecificData(stakeCombination, isSelected, false);
					var bs = isVisible ? enabled ? ButtonState.Enabled : ButtonState.DisabledShow : ButtonState.DisabledHide;

					Log.Instance.InfoFormat("Creating play button ({0}) Function {1}", string.Join(", ", playButtonData.StakeCombination.Values.Select(v => $"{v.Key}:{v.Value}")), buttonFunction.Name);

					playButtonFunctionMapping.Add(buttonFunction, stakeCombination);
					AddButtonState(buttonFunction, bs, playButtonData);
				}
				else
				{
					AddButtonState(buttonFunction, ButtonState.DisabledHide);
				}
			}
		}

		private bool CanSelectPlayButton(IStakeCombination stakeCombination)
		{
			return CanEnableButton(Money.FromCredit(stakeCombination.TotalBet));
		}

		#endregion
	}
}