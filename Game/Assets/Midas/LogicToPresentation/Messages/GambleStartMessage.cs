namespace Midas.LogicToPresentation.Messages
{
	public sealed class GambleStartMessage : IMessage
	{
		public object Context { get; }

		public GambleStartMessage(object context)
		{
			Context = context;
		}
	}
}