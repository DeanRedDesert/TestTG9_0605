using Midas.Core;
using Midas.Core.General;
using Midas.Logic;

namespace Midas.Presentation.Denom
{
	public enum DenomState
	{
		Normal,
		Attract,
		Active,
		Disable
	}

	public sealed class DenomButtonSpecificData
	{
		public Money Denom { get; }
		public DenomState DenomState { get; }
		public IDenomBetData DenomBetData { get; }

		public DenomButtonSpecificData(Money denom, DenomState denomState, IDenomBetData denomBetData)
		{
			Denom = denom;
			DenomState = denomState;
			DenomBetData = denomBetData;
		}
	}
}