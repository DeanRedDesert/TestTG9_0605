using Midas.LogicToPresentation;

namespace Midas.Gamble.LogicToPresentation
{
	public sealed class TrumpsDialUpMessage : IMessage
	{
		public TrumpsSuit Suit { get; }

		public TrumpsDialUpMessage(TrumpsSuit suit)
		{
			Suit = suit;
		}
	}
}