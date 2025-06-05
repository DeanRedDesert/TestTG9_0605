namespace Midas.LogicToPresentation.Messages
{
	public sealed class HistoryDetailsMessage : IMessage
	{
		public int CurrentStepIndex { get; }
		public int TotalSteps { get; }
		public HistoryStepType HistoryStepType { get; }
		
		public int HistoryRecordCount { get; }
		public bool IsNextHistoryRecordAvailable { get; }
		public bool IsPreviousHistoryRecordAvailable { get; }

		public HistoryDetailsMessage(int currentStepIndex, int totalSteps, HistoryStepType historyStepType, int historyRecordCount, bool isNextHistoryRecordAvailable, bool isPreviousHistoryRecordAvailable)
		{
			CurrentStepIndex = currentStepIndex;
			TotalSteps = totalSteps;
			HistoryStepType = historyStepType;
			HistoryRecordCount = historyRecordCount;
			IsNextHistoryRecordAvailable = isNextHistoryRecordAvailable;
			IsPreviousHistoryRecordAvailable = isPreviousHistoryRecordAvailable;
		}
	}

	public enum HistoryStepType
	{
		Game,
		Gamble,
		CreditPlayoff
	}
}