namespace Midas.LogicToPresentation.Messages
{
	public enum DemoHistoryRecordChangeDirection
	{
		Previous,
		Next
	}

	public sealed class DemoChangeHistoryRecordMessage : DebugMessage
	{
		public DemoHistoryRecordChangeDirection Direction { get; }

		public DemoChangeHistoryRecordMessage(DemoHistoryRecordChangeDirection direction)
		{
			Direction = direction;
		}
	}
}