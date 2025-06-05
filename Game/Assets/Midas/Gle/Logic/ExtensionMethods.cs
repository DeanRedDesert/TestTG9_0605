using Midas.Core.General;
using LogicMoney = Logic.Core.Types.Money;
using LogicCredits = Logic.Core.Types.Credits;

namespace Midas.Gle.Logic
{
	public static class ExtensionMethods
	{
		public static LogicMoney ToLogicMoney(this Money money) => LogicMoney.FromCents((ulong)money.AsMinorCurrency);

		public static Money ToMidasMoney(this LogicMoney money) => Money.FromMinorCurrency((long)money.ToCents());

		public static Money ToMidasMoney(this LogicCredits credits) => credits.ToMidasCredit().ToMoney();

		public static LogicCredits ToLogicCredits(this Credit credit) => new LogicCredits((ulong)credit.Credits);

		public static Credit ToMidasCredit(this LogicCredits credits) => Credit.FromULong(credits.ToUInt64());

		public static Credit ToMidasCredit(this LogicMoney money) => money.ToMidasMoney().ToCredit();
	}
}