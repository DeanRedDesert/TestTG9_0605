using Midas.Core.General;

namespace Midas.Presentation.Meters
{
	public sealed class StaticMoneyMeter : RollingMoneyMeter
	{
		public override void SetValue(Money value)
		{
			var smc = value.SubMinorCurrency;
			if (!smc.IsZero)
				value = Money.FromRationalNumber(value.Value - smc);

			base.SetValue(value);
		}
	}
}