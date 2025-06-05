namespace Midas.LogicToPresentation.Messages
{
	public sealed class GameLogicTimingsMessage : IMessage
	{
		public string LogicTimings { get; }

		public GameLogicTimingsMessage(string logicTimings)
		{
			LogicTimings = logicTimings;
		}
	}

	public sealed class GameLogicTimingsResetMessage : IMessage
	{
	}
}