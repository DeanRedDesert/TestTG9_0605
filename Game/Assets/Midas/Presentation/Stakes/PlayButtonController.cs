using Midas.Core.General;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;

namespace Midas.Presentation.Stakes
{
	/// <summary>
	/// Handles the play button (smash button)
	/// </summary>
	public sealed class PlayButtonController : StakeButtonController
	{
		private CreditPlayoffStatusBase creditPlayoffStatus;
		private InterruptController interruptController;

		public override void Init()
		{
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>(false);
			interruptController = GameBase.GameInstance.GetPresentationController<InterruptController>();

			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();

			interruptController.InterruptableStateChanged += RequestButtonUpdate;
			interruptController = null;
		}

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			if (creditPlayoffStatus != null)
				AddButtonConditionAnyPropertyChanged(creditPlayoffStatus);

			interruptController.InterruptableStateChanged += RequestButtonUpdate;
			RegisterButtonPressListener(StakeButtonFunctions.Play, OnPlayButtonPressed);
		}

		protected override void UpdateButtonStates()
		{
			var enabled = CanPressPlayButton() || CanPressInterruptButton();
			var lightState = CanPressInterruptButton() ? LightState.On : enabled ? LightState.FlashMedium : LightState.Off;

			AddButtonState(StakeButtonFunctions.Play, enabled, lightState);
		}

		private bool CanPressPlayButton()
		{
			var stakeCombination = GameStatus.SelectedStakeCombination;

			if (stakeCombination == null || !CanEnableButton())
				return false;

			if (StatusDatabase.GameStatus.InUtilityMode)
				return true;

			if (creditPlayoffStatus.IsReadyToPlay)
				return StatusDatabase.BankStatus.WagerableMeter > Money.Zero;

			var currentBet = Money.FromCredit(stakeCombination.TotalBet);
			return StatusDatabase.BankStatus.WagerableMeter >= currentBet;
		}

		private bool CanPressInterruptButton()
		{
			return StatusDatabase.GameStatus.GameIsActive &&
				interruptController.IsAnythingToInterrupt;
		}

		private void OnPlayButtonPressed(ButtonEventData buttonEventData)
		{
			if (CanPressPlayButton())
			{
				StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
				ButtonQueueController.Enqueue(buttonEventData, IsStakeActionHindered, ExecutePlayAction);
			}
			else if (CanPressInterruptButton())
			{
				interruptController.Interrupt(false);
			}
		}

		private ButtonFunctionHinderedReasons IsStakeActionHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseGameCycleActive | ButtonFunctionHinderedReasons.BecauseInterruptable);
		}

		private void ExecutePlayAction(ButtonEventData buttonEventData)
		{
			if (!CanPressPlayButton())
				return;

			StartGame();
		}
	}
}