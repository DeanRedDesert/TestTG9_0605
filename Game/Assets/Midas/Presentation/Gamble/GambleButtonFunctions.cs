using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.Gamble
{
	[ButtonFunctions("Gamble")]
	public class GambleButtonFunctions
	{
		public static ButtonFunction EnterGamble { get; } = ButtonFunction.Create(ButtonFunctions.AncillaryGameBase + 0);
		public static ButtonFunction TakeWin { get; } = ButtonFunction.Create(ButtonFunctions.AncillaryGameBase + 1);
	}
}