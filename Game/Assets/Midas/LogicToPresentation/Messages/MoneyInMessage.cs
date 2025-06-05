using Midas.Core;
using Midas.Core.General;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class MoneyInMessage : IMessage
	{
		public Money Amount { get; }
		public MoneySource MoneySource { get; }

		public MoneyInMessage(Money amount, MoneySource moneySource)
		{
			Amount = amount;
			MoneySource = moneySource;
		}
	}
}