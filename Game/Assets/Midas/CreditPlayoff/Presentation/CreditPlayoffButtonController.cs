using System.Linq;
using Midas.CreditPlayoff.LogicToPresentation;
using Midas.LogicToPresentation;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.Stakes;

namespace Midas.CreditPlayoff.Presentation
{
	public sealed class CreditPlayoffButtonController : ButtonController
	{
		private CreditPlayoffStatus creditPlayoffStatus;
		private ButtonMappingHandle buttonMappingHandle;

		public override void Init()
		{
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatus>();

			base.Init();
		}

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(StatusDatabase.BankStatus.IsPlayerWagerAvailable));
			AddButtonConditionPropertyChanged(creditPlayoffStatus, nameof(CreditPlayoffStatus.State));
			AddButtonConditionPropertyChanged(creditPlayoffStatus, nameof(CreditPlayoffStatus.IsReadyToPlay));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState));
			AddButtonConditionExpressionChanged(typeof(DashboardExpressions), nameof(DashboardExpressions.IsAnyPopupOpen));

			RegisterButtonPressListener(CreditPlayoffButtonFunctions.ShowCreditPlayoff, OnShowCreditPlayoff);
			RegisterButtonPressListener(CreditPlayoffButtonFunctions.ReturnToGame, OnDeclineCreditPlayoff);
			RegisterButtonPressListener(CreditPlayoffButtonFunctions.PlayCreditPlayoff, OnPlay);
		}

		protected override void UpdateButtonStates()
		{
			AddButtonState(CreditPlayoffButtonFunctions.ShowCreditPlayoff, CanStartCreditPlayoff(), LightState.FlashMedium);
			AddButtonState(CreditPlayoffButtonFunctions.ReturnToGame, CanReturnToGame());
			AddButtonState(CreditPlayoffButtonFunctions.PlayCreditPlayoff, CanPlayGame());

			CanStartCreditPlayoff();
		}

		private bool CanStartCreditPlayoff()
		{
			var canStartCreditPlayoff = StatusDatabase.GameStatus.InActivePlay &&
				StatusDatabase.BankStatus.IsPlayerWagerAvailable &&
				creditPlayoffStatus.State == CreditPlayoffState.Available &&
				!DashboardExpressions.IsAnyPopupOpen;

			switch (canStartCreditPlayoff)
			{
				case true when buttonMappingHandle == null:
				{
					var playButtons = CabinetManager.Cabinet.GetDefaultButtonsFromButtonFunction(StakeButtonFunctions.Play).ToArray();
					buttonMappingHandle = CabinetManager.Cabinet.PushButtonMapping(() => playButtons.Select(b => (b, CreditPlayoffButtonFunctions.ShowCreditPlayoff)));
					break;
				}
				case false when buttonMappingHandle != null:
					CabinetManager.Cabinet.PopButtonMapping(buttonMappingHandle);
					buttonMappingHandle = null;
					break;
			}

			return canStartCreditPlayoff;
		}

		private bool CanReturnToGame() => StatusDatabase.GameStatus.InActivePlay && creditPlayoffStatus.State == CreditPlayoffState.Idle;

		private bool CanPlayGame() => StatusDatabase.GameStatus.InActivePlay && creditPlayoffStatus.State == CreditPlayoffState.Idle && creditPlayoffStatus.IsReadyToPlay;

		private void OnShowCreditPlayoff(ButtonEventData buttonEvent)
		{
			if (CanStartCreditPlayoff())
				Communication.ToLogicSender.Send(new CreditPlayoffActivatedMessage());
		}

		private void OnDeclineCreditPlayoff(ButtonEventData buttonEvent)
		{
			if (CanReturnToGame())
				Communication.ToLogicSender.Send(new CreditPlayoffReturnToGameMessage());
		}

		private void OnPlay(ButtonEventData buttonEvent)
		{
			if (CanPlayGame())
			{
				StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
				ButtonQueueController.Enqueue(buttonEvent, IsStakeActionHindered, ExecutePlayAction);
			}
		}

		private void ExecutePlayAction(ButtonEventData buttonEventData)
		{
			if (!CanPlayGame())
				return;

			GameInitiator.StartGame();
		}

		private ButtonFunctionHinderedReasons IsStakeActionHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseInterruptable);
		}
	}
}