namespace Midas.LogicToPresentation.Messages
{
	public sealed class RunTimeDenomSelectionMessage : IMessage
	{
		public bool IsActive { get; }

		public RunTimeDenomSelectionMessage(bool isActive)
		{
			IsActive = isActive;
		}
	}
}