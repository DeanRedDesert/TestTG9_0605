namespace Midas.LogicToPresentation.Messages
{
	public sealed class OfferGambleCompleteMessage : IMessage
	{
		public bool GambleRequested { get; }

		public OfferGambleCompleteMessage(bool gambleRequested)
		{
			GambleRequested = gambleRequested;
		}
	}
}