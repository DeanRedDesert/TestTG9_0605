namespace Midas.LogicToPresentation.Messages
{
	public enum HistoryCommand
	{
		FirstStep,
		PreviousStep,
		NextStep,
		LastStep
	}

	public sealed class HistoryPresentationCommand : IMessage
	{
		public HistoryCommand Command { get; }

		public HistoryPresentationCommand(HistoryCommand command)
		{
			Command = command;
		}
	}
}