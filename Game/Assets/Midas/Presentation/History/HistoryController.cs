using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Presentation.History
{
	public sealed class HistoryController : IPresentationController
	{
		private HistoryStatus historyStatus;

		public void Init()
		{
			historyStatus = StatusDatabase.HistoryStatus;
			Communication.PresentationDispatcher.AddHandler<HistoryDetailsMessage>(OnHistoryDetails);
		}

		public void DeInit()
		{
			Communication.PresentationDispatcher.RemoveHandler<HistoryDetailsMessage>(OnHistoryDetails);
			historyStatus = null;
		}

		public void Destroy()
		{
		}

		private void OnHistoryDetails(HistoryDetailsMessage msg)
		{
			historyStatus.HistoryStepIndex = msg.CurrentStepIndex;
			historyStatus.HistoryStepCount = msg.TotalSteps;
			historyStatus.HistoryStepType = msg.HistoryStepType;
			historyStatus.HistoryStepDescription = $"Step {msg.CurrentStepIndex + 1} of {msg.TotalSteps}";
			historyStatus.HistoryRecordCount = msg.HistoryRecordCount;
			historyStatus.NextRecordAvailable = msg.IsNextHistoryRecordAvailable;
			historyStatus.PreviousRecordAvailable = msg.IsPreviousHistoryRecordAvailable;

			historyStatus.RefreshRequired = StatusDatabase.GameStatus.CurrentGameState == GameState.History;
		}
	}
}