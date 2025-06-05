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
	public sealed class CostToCoverStakeButtonController : StakeButtonController
	{
		private AutoUnregisterHelper stakeButtonRegistrationHelper = new AutoUnregisterHelper();
		private readonly Dictionary<ButtonFunction, IStakeCombination> stakeButtonFunctionMapping = new Dictionary<ButtonFunction, IStakeCombination>();
		private readonly HashSet<ButtonFunction> buttonFunctionsToDisable = new HashSet<ButtonFunction>();

		#region ButtonController Overrides

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			RegisterPropertyChangedHandler<IReadOnlyList<ButtonFunction>>(StakesStatus, nameof(StakesStatus.StakeButtonFunctions), OnStakeButtonsChanged);
			RegisterStakeButtons();
		}

		protected override void UnregisterEvents()
		{
			stakeButtonRegistrationHelper.UnRegisterAll();
			base.UnregisterEvents();
		}

		protected override void UpdateButtonStates()
		{
			foreach (var buttonFunction in buttonFunctionsToDisable)
				AddButtonState(buttonFunction, ButtonState.DisabledHide);
			buttonFunctionsToDisable.Clear();

			if (StakesStatus.StakeMode != StakeMode.CostToCover)
				return;

			if (StatusDatabase.GameStatus.StakeCombinations == null)
				return;

			Log.Instance.InfoFormat("Configuring Cost To Cover panel, {0} buttons", StakesStatus.StakeButtonFunctions.Count);

			CreateStakeButtonStates(GameStatus.SelectedStakeCombination);
		}

		#endregion

		#region Private Methods

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

		private void OnStakeButtonsChanged(StatusBlock sender, string propertyname, IReadOnlyList<ButtonFunction> newButtonFunctions, IReadOnlyList<ButtonFunction> oldButtonFunctions)
		{
			EvaluateFunctionChanges(newButtonFunctions, oldButtonFunctions);
			stakeButtonRegistrationHelper.UnRegisterAll();
			RegisterStakeButtons();
			RequestButtonUpdate();
		}

		private void RegisterStakeButtons()
		{
			if (StakesStatus.StakeButtonFunctions == null)
				return;

			foreach (var betButton in StakesStatus.StakeButtonFunctions)
				stakeButtonRegistrationHelper.RegisterButtonPressListener(betButton, OnStakeButtonPressed);
		}

		private void OnStakeButtonPressed(ButtonEventData buttonEventData)
		{
			if (stakeButtonFunctionMapping.TryGetValue(buttonEventData.ButtonFunction, out var stakeCombination) &&
				CanSelectStakeButton(stakeCombination))
			{
				StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
				ButtonQueueController.Enqueue(buttonEventData, IsStakeActionHindered, ExecutePlayStakeAction);
			}
		}

		private void ExecutePlayStakeAction(ButtonEventData buttonEventData)
		{
			if (stakeButtonFunctionMapping.TryGetValue(buttonEventData.ButtonFunction, out var stakeCombination) &&
				CanSelectStakeButton(stakeCombination))
			{
				var index = StatusDatabase.GameStatus.StakeCombinations.FindIndex(stakeCombination);
				Communication.ToLogicSender.Send(new GameStakeCombinationMessage(index));
				if (!StakesStatus.PlayFromPlayButtonOnly)
					StartGame();
			}
		}

		private ButtonFunctionHinderedReasons IsStakeActionHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseGameCycleActive | ButtonFunctionHinderedReasons.BecauseInterruptable);
		}

		private void CreateStakeButtonStates(IStakeCombination selectedStakeCombo)
		{
			stakeButtonFunctionMapping.Clear();
			for (var i = 0; i < StakesStatus.StakeButtonFunctions.Count; i++)
			{
				var buttonFunction = StakesStatus.StakeButtonFunctions[i];
				if (i < StatusDatabase.GameStatus.StakeCombinations.Count)
				{
					var stakeCombination = StatusDatabase.GameStatus.StakeCombinations[i];
					var enabled = CanSelectStakeButton(stakeCombination);
					var isVisible = StatusDatabase.GameFunctionStatus.GameButtonBehaviours.BetButtons.IsVisible();
					var isSelected = ReferenceEquals(selectedStakeCombo, stakeCombination);
					var playButtonData = new PlayButtonSpecificData(stakeCombination, isSelected, true);
					var bs = isVisible ? enabled ? ButtonState.Enabled : ButtonState.DisabledShow : ButtonState.DisabledHide;

					Log.Instance.InfoFormat("Creating stake button ({0}) Function {1}", string.Join(", ", playButtonData.StakeCombination.Values.Select(v => $"{v.Key}:{v.Value}")), buttonFunction.Name);

					stakeButtonFunctionMapping.Add(buttonFunction, stakeCombination);
					AddButtonState(buttonFunction, bs, playButtonData);
				}
				else
				{
					AddButtonState(buttonFunction, ButtonState.DisabledHide);
				}
			}
		}

		private bool CanSelectStakeButton(IStakeCombination stakeCombination)
		{
			return CanEnableButton(Money.FromCredit(stakeCombination.TotalBet));
		}

		#endregion
	}
}