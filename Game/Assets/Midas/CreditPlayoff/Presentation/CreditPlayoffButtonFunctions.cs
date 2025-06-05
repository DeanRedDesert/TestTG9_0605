using Midas.Presentation.ButtonHandling;

namespace Midas.CreditPlayoff.Presentation
{
	[ButtonFunctions("Credit Playoff")]
	public static class CreditPlayoffButtonFunctions
	{
		public static ButtonFunction ReturnToGame => ButtonFunction.Create(ButtonFunctions.CreditPlayoffBase + 0);
		public static ButtonFunction ShowCreditPlayoff => ButtonFunction.Create(ButtonFunctions.CreditPlayoffBase + 1);
		public static ButtonFunction PlayCreditPlayoff => ButtonFunction.Create(ButtonFunctions.CreditPlayoffBase + 2);
	}
}