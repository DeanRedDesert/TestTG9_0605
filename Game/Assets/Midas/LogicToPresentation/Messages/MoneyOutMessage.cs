using Midas.Core;
using Midas.Core.General;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class MoneyOutMessage : IMessage
	{
		public Money Amount { get; }
		public MoneyTarget MoneyTarget { get; }

		public MoneyOutMessage(Money amount, MoneyTarget moneyTarget)
		{
			Amount = amount;
			MoneyTarget = moneyTarget;
		}
	}
}