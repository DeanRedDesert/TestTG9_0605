using Midas.LogicToPresentation;

namespace Midas.CreditPlayoff.LogicToPresentation
{
	public sealed class CreditPlayoffDialUpMessage : IMessage
	{
		public bool Win { get; }

		public CreditPlayoffDialUpMessage(bool win)
		{
			Win = win;
		}
	}
}