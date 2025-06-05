using Midas.Core.General;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class ChangeGameDenomMessage : IMessage
	{
		public Money Denom { get; }

		public ChangeGameDenomMessage(Money denom)
		{
			Denom = denom;
		}
	}

	public sealed class ChangeGameDenomCancelledMessage : IMessage
	{
	}
}