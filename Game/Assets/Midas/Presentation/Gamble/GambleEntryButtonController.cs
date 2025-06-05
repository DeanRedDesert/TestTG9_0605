using Midas.Core.General;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;

namespace Midas.Presentation.Gamble
{
	public sealed class GambleEntryButtonController : ButtonController
	{
		private GambleStatus gambleStatus;

		public override void Init()
		{
			gambleStatus = StatusDatabase.QueryStatusBlock<GambleStatus>();
			base.Init();
		}

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(StatusDatabase.ConfigurationStatus.AncillaryConfig));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.OfferGambleRequest));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState));
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(StatusDatabase.BankStatus.TotalAward));
			AddButtonConditionPropertyChanged(gambleStatus, nameof(GambleStatus.AwaitingSelection));
			AddButtonConditionPropertyChanged(StatusDatabase.PopupStatus, nameof(PopupStatus.IsInfoOpen));
			AddButtonConditionPropertyChanged(StatusDatabase.StageStatus, nameof(StageStatus.CurrentStage));
			RegisterButtonPressListener(GambleButtonFunctions.TakeWin, OnTakeWinButtonPressed);
			RegisterButtonPressListener(GambleButtonFunctions.EnterGamble, OnEnterGambleButtonPressed);
		}

		protected override void UpdateButtonStates()
		{
			if (StatusDatabase.ConfigurationStatus.AncillaryConfig == null)
				return;

			AddButtonState(GambleButtonFunctions.TakeWin, CanPressTakeWinButton());

			if (StatusDatabase.ConfigurationStatus.AncillaryConfig.Enabled)
				AddButtonState(GambleButtonFunctions.EnterGamble, CanPressEnterGambleButton());
			else
				AddButtonState(GambleButtonFunctions.EnterGamble, ButtonState.DisabledHide);
		}

		private bool CanPressEnterGambleButton()
		{
			return StatusDatabase.GameStatus.InActivePlay &&
				StatusDatabase.ConfigurationStatus.AncillaryConfig.Enabled &&
				gambleStatus.IsGambleOfferable &&
				StatusDatabase.GameStatus.CurrentGameState == GameState.OfferGamble &&
				StatusDatabase.GameStatus.OfferGambleRequest == OfferGambleRequest.None &&
				StatusDatabase.BankStatus.TotalAward != Money.Zero &&
				StatusDatabase.StageStatus.CurrentStage == GameBase.GameInstance.BaseGameStage;
		}

		private bool CanPressTakeWinButton()
		{
			if (!StatusDatabase.GameStatus.InActivePlay || PopupStatus.IsInfoOpen)
				return false;

			switch (StatusDatabase.GameStatus.CurrentGameState)
			{
				case GameState.OfferGamble:
					return StatusDatabase.GameStatus.OfferGambleRequest == OfferGambleRequest.None &&
						StatusDatabase.BankStatus.TotalAward != Money.Zero &&
						StatusDatabase.StageStatus.CurrentStage == GameBase.GameInstance.BaseGameStage;

				case GameState.StartingGamble:
					return gambleStatus.AwaitingSelection;
			}

			return false;
		}

		private void OnTakeWinButtonPressed(ButtonEventData buttonEvent)
		{
			if (CanPressTakeWinButton())
				ButtonQueueController.Enqueue(buttonEvent, IsTakeWinHindered, ExecuteTakeWin);
		}

		private ButtonFunctionHinderedReasons IsTakeWinHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseInterruptable);
		}

		private void ExecuteTakeWin(ButtonEventData buttonEventData)
		{
			if (!CanPressTakeWinButton())
				return;

			switch (StatusDatabase.GameStatus.CurrentGameState)
			{
				case GameState.OfferGamble:
					StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
					break;

				case GameState.StartingGamble:
					gambleStatus.TakeWin();
					break;
			}
		}

		private void OnEnterGambleButtonPressed(ButtonEventData buttonEvent)
		{
			if (CanPressEnterGambleButton())
				ButtonQueueController.Enqueue(buttonEvent, IsEnterGambleHindered, ExecuteEnterGamble);
		}

		private ButtonFunctionHinderedReasons IsEnterGambleHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseInterruptable);
		}

		private void ExecuteEnterGamble(ButtonEventData buttonEventData)
		{
			if (!CanPressEnterGambleButton())
				return;

			StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.Gamble;
		}
	}
}
