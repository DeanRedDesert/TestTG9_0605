namespace Midas.LogicToPresentation.Messages
{
	public abstract class GameServiceUpdateMessage : IMessage
	{
		public abstract void DeliverChanges();
	}
}