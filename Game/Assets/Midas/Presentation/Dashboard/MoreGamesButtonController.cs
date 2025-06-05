using Midas.Core;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Info;
using Midas.Presentation.Interruption;
using Midas.Presentation.Stakes;
using Midas.Presentation.WinPresentation;
using static Midas.Presentation.Game.GameBase;

namespace Midas.Presentation.Dashboard
{
	public sealed class MoreGamesButtonController : ButtonController
	{
		private InfoStatus infoStatus;
		private CreditPlayoffStatusBase creditPlayoffStatus;

		private WinCountController winCountController;
		private InterruptController interruptController;

		public override void Init()
		{
			infoStatus = StatusDatabase.InfoStatus;
			creditPlayoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>();
			winCountController = GameInstance.GetPresentationController<WinCountController>();
			interruptController = GameInstance.GetPresentationController<InterruptController>();
			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();
			infoStatus = null;
			creditPlayoffStatus = null;
			winCountController = null;
			interruptController = null;
		}

		protected override void RegisterEvents()
		{
			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.IsChooserAvailable));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameIsActive));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(ButtonEventDataQueueStatus, nameof(ButtonEventDataQueueStatus.ButtonFunction));
			AddButtonConditionExpressionChanged(typeof(PopupStatus), nameof(PopupStatus.IsInfoOpen));
			AddButtonConditionExpressionChanged(typeof(PopupStatus), nameof(PopupStatus.IsCreditPlayoffOpen));
			AddButtonConditionPropertyChanged(StatusDatabase.GameFunctionStatus, nameof(GameFunctionStatus.GameButtonBehaviours));
			AddButtonConditionPropertyChanged(StatusDatabase.AutoPlayStatus, nameof(AutoPlayStatus.State));
			interruptController.InterruptableStateChanged += RequestButtonUpdate;
			winCountController.CountingStateChanged += RequestButtonUpdate;
			RegisterButtonPressListener(DashboardButtonFunctions.MoreGames, OnMoreGamesButtonPressed);
		}

		protected override void UnregisterEvents()
		{
			interruptController.InterruptableStateChanged -= RequestButtonUpdate;
			winCountController.CountingStateChanged -= RequestButtonUpdate;
		}

		private bool IsMoreGamesButtonEnabled()
		{
			return StatusDatabase.GameStatus.InActivePlay &&
				!StatusDatabase.GameStatus.GameIsActive &&
				StatusDatabase.AutoPlayStatus.State == AutoPlayState.Idle &&
				!StatusDatabase.PopupStatus.AreOpen(Popup.Info | Popup.CreditPlayoff) &&
				StatusDatabase.GameFunctionStatus.GameButtonBehaviours.MoreGamesButton.IsActive() &&
				!ButtonEventDataQueueStatus.ButtonFunction.IsPlayButtonFunction();
		}

		protected override void UpdateButtonStates()
		{
			if (!StatusDatabase.ConfigurationStatus.IsChooserAvailable || !StatusDatabase.GameFunctionStatus.GameButtonBehaviours.MoreGamesButton.IsVisible())
			{
				AddButtonState(DashboardButtonFunctions.MoreGames, ButtonState.DisabledHide);
				return;
			}

			AddButtonState(DashboardButtonFunctions.MoreGames, IsMoreGamesButtonEnabled());
		}

		private void OnMoreGamesButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsMoreGamesButtonEnabled())
			{
				if (!StatusDatabase.GameStatus.GameIsActive)
				{
					ButtonQueueController.Enqueue(buttonEventData, IsMoreGamesHindered, ExecuteMoreGamesAction);
					StatusDatabase.GameStatus.OfferGambleRequest = OfferGambleRequest.TakeWin;
				}
				else
				{
					// I don't really know why this is here...

					ButtonQueueController.Enqueue(buttonEventData, IsMoreGamesHindered, ExecuteNoMoreGamesAction);
				}
			}
		}

		private static void ExecuteNoMoreGamesAction(ButtonEventData buttonEventData)
		{
			//nothing to do here
		}

		private static void ExecuteMoreGamesAction(ButtonEventData buttonEventData)
		{
			Communication.ToLogicSender.Send(new RequestThemeSelectionMenuMessage());
		}

		private ButtonFunctionHinderedReasons IsMoreGamesHindered()
		{
			return ButtonQueueController.IsHindered(ButtonFunctionHinderedReasons.BecauseGameCycleActive);
		}
	}
}