using Midas.Gamble.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;

namespace Midas.Gamble.Presentation
{
	public sealed class TrumpsButtonController : ButtonController
	{
		private TrumpsStatus trumpsStatus;

		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			trumpsStatus = StatusDatabase.QueryStatusBlock<TrumpsStatus>();

			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(trumpsStatus, nameof(TrumpsStatus.IsIdleActive));
			AddButtonConditionPropertyChanged(trumpsStatus, nameof(TrumpsStatus.Selection));
			AddButtonConditionPropertyChanged(trumpsStatus, nameof(TrumpsStatus.Results));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.CurrentGameState));

			foreach (var function in TrumpsButtonFunctions.TrumpsButtons)
			{
				RegisterButtonPressListener(function, OnTrumpsButtonPressed);
			}
		}

		protected override void UpdateButtonStates()
		{
			var enabled = CanPressTrumpsButton();

			foreach (var function in TrumpsButtonFunctions.TrumpsButtons)
			{
				var isSelected = false;
				if (trumpsStatus.CurrentResult != null &&
					(StatusDatabase.GameStatus.CurrentGameState == GameState.History || StatusDatabase.GameStatus.CurrentGameState == GameState.ShowGambleResult))
				{
					isSelected = function.Id == (int)ButtonFunctions.GambleFeatureBase + (int)trumpsStatus.CurrentResult.Selection;
				}

				AddButtonState(function, enabled, isSelected);
			}
		}

		private bool CanPressTrumpsButton()
		{
			return StatusDatabase.GameStatus.InActivePlay &&
				trumpsStatus.IsIdleActive &&
				!trumpsStatus.Selection.HasValue;
		}

		private void OnTrumpsButtonPressed(ButtonEventData eventData)
		{
			trumpsStatus.Selection ??= (TrumpsSelection)(eventData.ButtonFunction.Id - ButtonFunctions.GambleFeatureBase);
		}
	}
}