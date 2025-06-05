using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;

namespace Midas.Presentation.Stakes
{
	public abstract class StakeButtonController : ButtonController
	{
		protected StakesStatus StakesStatus { get; private set; }
		protected DashboardStatus DashboardStatus { get; private set; }

		public override void Init()
		{
			StakesStatus = StatusDatabase.StakesStatus;
			DashboardStatus = StatusDatabase.DashboardStatus;

			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();

			StakesStatus = null;
			DashboardStatus = null;
		}

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			AddButtonConditionAnyPropertyChanged(StatusDatabase.ConfigurationStatus);
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.InUtilityMode));
			AddButtonConditionPropertyChanged(StakesStatus, nameof(StakesStatus.StakeGroups));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.SelectedStakeCombinationIndex));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.GamePlayRequested));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.LogicGameActive));
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(StatusDatabase.BankStatus.WagerableMeter));
			AddButtonConditionPropertyChanged(StatusDatabase.BankStatus, nameof(StatusDatabase.BankStatus.TotalAward));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.OfferGambleRequest));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.HasWinCapBeenReached));
			AddButtonConditionPropertyChanged(StatusDatabase.AutoPlayStatus, nameof(AutoPlayStatus.State));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.GameButtonBehaviours));
			AddButtonConditionPropertyChanged(StatusDatabase.StageStatus, nameof(StageStatus.CurrentStage));
			AddButtonConditionExpressionChanged(typeof(DashboardExpressions), nameof(DashboardExpressions.IsAnyPopupOpen));
		}

		protected bool CanEnableButton()
		{
			var currentState = StatusDatabase.GameStatus.CurrentGameState;
			return (StatusDatabase.GameStatus.InActivePlay || StatusDatabase.GameStatus.InUtilityMode) &&
				!StatusDatabase.GameStatus.GamePlayRequested &&
				!StatusDatabase.GameStatus.LogicGameActive &&
				!DashboardExpressions.IsAnyPopupOpen &&
				StatusDatabase.GameFunctionStatus.GameButtonBehaviours.BetButtons.IsActive() &&
				(currentState == GameState.Idle || currentState == GameState.OfferGamble && !StatusDatabase.GameStatus.HasWinCapBeenReached) &&
				StatusDatabase.AutoPlayStatus.State == AutoPlayState.Idle &&
				StatusDatabase.StageStatus.CurrentStage == GameBase.GameInstance.BaseGameStage;
		}

		protected bool CanEnableButton(Money buttonBetValue)
		{
			if (StatusDatabase.GameStatus.InUtilityMode)
				return CanEnableButton();

			return CanEnableButton() &&
				Money.FromCredit(StatusDatabase.ConfigurationStatus.GameMaximumBet) >= buttonBetValue &&
				StatusDatabase.BankStatus.WagerableMeter >= buttonBetValue;
		}

		protected static void StartGame()
		{
			GameInitiator.StartGame();
		}
	}
}