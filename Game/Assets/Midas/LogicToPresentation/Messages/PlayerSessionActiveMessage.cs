namespace Midas.LogicToPresentation.Messages
{
	public sealed class PlayerSessionActiveMessage : IMessage
	{
		public bool IsActive { get; }

		public PlayerSessionActiveMessage(bool isActive) => IsActive = isActive;
	}
}