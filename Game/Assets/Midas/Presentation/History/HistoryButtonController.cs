using Midas.Core;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;

namespace Midas.Presentation.History
{
	public class HistoryButtonController : ButtonController
	{
		private HistoryStatus historyStatus;
		
		protected override void RegisterEvents()
		{
			base.RegisterEvents();

			historyStatus = StatusDatabase.HistoryStatus;

			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(StatusDatabase.GameStatus.GameMode));
			AddButtonConditionPropertyChanged(historyStatus, nameof(HistoryStatus.HistoryStepCount));
			AddButtonConditionPropertyChanged(historyStatus, nameof(HistoryStatus.HistoryStepIndex));

			RegisterButtonPressListener(HistoryButtonFunctions.NextCycle, OnNextCycleButtonPressed);
			RegisterButtonPressListener(HistoryButtonFunctions.PreviousCycle, OnPreviousCycleButtonPressed);
			RegisterButtonPressListener(HistoryButtonFunctions.FirstCycle, OnFirstCycleButtonPressed);
			RegisterButtonPressListener(HistoryButtonFunctions.LastCycle, OnLastCycleButtonPressed);
		}

		protected override void UpdateButtonStates()
		{
			var showButton = StatusDatabase.GameStatus.GameMode == FoundationGameMode.History;

			if (!showButton)
			{
				AddButtonState(HistoryButtonFunctions.NextCycle, ButtonState.DisabledHide);
				AddButtonState(HistoryButtonFunctions.PreviousCycle, ButtonState.DisabledHide);
				AddButtonState(HistoryButtonFunctions.FirstCycle, ButtonState.DisabledHide);
				AddButtonState(HistoryButtonFunctions.LastCycle, ButtonState.DisabledHide);
			}
			else
			{
				var index = historyStatus.HistoryStepIndex;
				var count = historyStatus.HistoryStepCount;

				var enabled = index < count - 1;
				AddButtonState(HistoryButtonFunctions.NextCycle, enabled);
				AddButtonState(HistoryButtonFunctions.LastCycle, enabled);

				enabled = index > 0;
				AddButtonState(HistoryButtonFunctions.PreviousCycle, enabled);
				AddButtonState(HistoryButtonFunctions.FirstCycle, enabled);
			}
		}

		private void OnNextCycleButtonPressed(ButtonEventData buttonEvent)
		{
			if (historyStatus.HistoryStepIndex + 1 < historyStatus.HistoryStepCount)
				Communication.ToLogicSender.Send(new HistoryPresentationCommand(HistoryCommand.NextStep));
		}

		private void OnPreviousCycleButtonPressed(ButtonEventData buttonEvent)
		{
			if (historyStatus.HistoryStepIndex != 0)
				Communication.ToLogicSender.Send(new HistoryPresentationCommand(HistoryCommand.PreviousStep));
		}

		private void OnFirstCycleButtonPressed(ButtonEventData buttonEvent)
		{
			if (historyStatus.HistoryStepIndex != 0)
				Communication.ToLogicSender.Send(new HistoryPresentationCommand(HistoryCommand.FirstStep));
		}

		private void OnLastCycleButtonPressed(ButtonEventData buttonEvent)
		{
			if (historyStatus.HistoryStepIndex + 1 < historyStatus.HistoryStepCount)
				Communication.ToLogicSender.Send(new HistoryPresentationCommand(HistoryCommand.LastStep));
		}
	}
}