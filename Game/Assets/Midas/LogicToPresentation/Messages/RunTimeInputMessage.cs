namespace Midas.LogicToPresentation.Messages
{
	public sealed class RunTimeInputMessage : IMessage
	{
		public bool Status { get; }

		public RunTimeInputMessage(bool status)
		{
			Status = status;
		}
	}
}