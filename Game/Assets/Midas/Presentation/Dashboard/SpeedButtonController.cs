using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;
using Midas.Presentation.WinPresentation;

namespace Midas.Presentation.Dashboard
{
	public sealed class SpeedButtonController : ButtonController, IPlayerSessionReset
	{
		private DashboardStatus dashboardStatus;
		private CreditPlayoffStatusBase creditPlayoffStatus;
		private WinCountController winCountController;
		private InterruptController interruptController;

		public override void Init()
		{
			base.Init();

			dashboardStatus = StatusDatabase.DashboardStatus;
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>();
			winCountController = GameBase.GameInstance.GetPresentationController<WinCountController>();
			interruptController = GameBase.GameInstance.GetPresentationController<InterruptController>();
		}

		protected override void RegisterEvents()
		{
			RegisterButtonPressListener(DashboardButtonFunctions.Speed, OnSpeedButtonPressed);

			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameLogicPaused));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.CurrentGameState));
			AddButtonConditionPropertyChanged(StatusDatabase.GameSpeedStatus, nameof(GameSpeedStatus.GameSpeed));
			AddButtonConditionPropertyChanged(StatusDatabase.GameSpeedStatus, nameof(GameSpeedStatus.IsChangeGameSpeedAllowed));
			AddButtonConditionExpressionChanged(typeof(PopupStatus), nameof(PopupStatus.IsCreditPlayoffOpen));
			AddButtonConditionExpressionChanged(typeof(DashboardExpressions), nameof(DashboardExpressions.IsAnyPopupOpen));
		}

		public override void DeInit()
		{
			dashboardStatus = null;
			creditPlayoffStatus = null;
			winCountController = null;
			interruptController = null;
			base.DeInit();
		}

		public void ResetForNewPlayerSession(IReadOnlyList<PlayerSessionParameterType> pendingResetParams, IList<PlayerSessionParameterType> resetDoneParams)
		{
			if (!pendingResetParams.Contains(PlayerSessionParameterType.GameSpeed))
				return;

			resetDoneParams.Add(PlayerSessionParameterType.GameSpeed);

			// TODO. Need to adjust default game speed based on foundation and category.
			var newSpeed = StatusDatabase.GameSpeedStatus.IsChangeGameSpeedAllowed ? StatusDatabase.GameSpeedStatus.DefaultGameSpeed : GameSpeed.Normal;
			if (newSpeed == StatusDatabase.GameSpeedStatus.GameSpeed)
				return;

			Log.Instance.Info($"ReSetting GameSpeed to DefaultGameSpeed={StatusDatabase.GameSpeedStatus.DefaultGameSpeed}");
			StatusDatabase.GameSpeedStatus.GameSpeed = newSpeed;

			// TODO. Send to foundation.
		}

		private bool IsButtonEnabled() =>
			!StatusDatabase.GameStatus.GameLogicPaused &&
			StatusDatabase.GameSpeedStatus.IsChangeGameSpeedAllowed &&
			(!DashboardExpressions.IsAnyPopupOpen || PopupStatus.IsCashoutConfirmOpen) &&
			StatusDatabase.GameStatus.CurrentGameState != GameState.StartingGamble &&
			StatusDatabase.GameStatus.CurrentGameState != GameState.ShowGambleResult &&
			StatusDatabase.GameStatus.CurrentGameState != GameState.StartingCreditPlayoff &&
			StatusDatabase.GameStatus.CurrentGameState != GameState.ShowCreditPlayoffResult &&
			!PopupStatus.IsCreditPlayoffOpen;

		protected override void UpdateButtonStates()
		{
			if (!StatusDatabase.GameSpeedStatus.IsChangeGameSpeedAllowed)
			{
				AddButtonState(DashboardButtonFunctions.Speed, ButtonState.DisabledHide);
				return;
			}

			AddButtonState(DashboardButtonFunctions.Speed, IsButtonEnabled());
		}

		private void OnSpeedButtonPressed(ButtonEventData buttonEventData)
		{
			if (!IsButtonEnabled())
				return;

			if (winCountController.IsCounting)
				interruptController.Interrupt(false);

			StatusDatabase.GameSpeedStatus.GameSpeed = StatusDatabase.GameSpeedStatus.GameSpeed switch
			{
				GameSpeed.Normal => GameSpeed.Fast,
				GameSpeed.Fast => GameSpeed.SuperFast,
				GameSpeed.SuperFast => GameSpeed.Normal,
				_ => GameSpeed.Normal
			};
		}
	}
}