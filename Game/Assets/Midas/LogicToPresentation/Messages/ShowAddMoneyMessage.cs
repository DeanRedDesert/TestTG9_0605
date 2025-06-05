using Midas.Core.General;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class ShowAddMoneyMessage : DebugMessage
	{
		public Money Amount { get; }

		public ShowAddMoneyMessage(Money amount)
		{
			Amount = amount;
		}
	}
}