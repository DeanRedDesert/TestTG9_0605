using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.History
{
	[ButtonFunctions("History")]
	public static class HistoryButtonFunctions
	{
		public static ButtonFunction NextCycle { get; } = ButtonFunction.Create(ButtonFunctions.HistoryBase + 0);
		public static ButtonFunction PreviousCycle { get; } = ButtonFunction.Create(ButtonFunctions.HistoryBase + 1);
		public static ButtonFunction FirstCycle { get; } = ButtonFunction.Create(ButtonFunctions.HistoryBase + 2);
		public static ButtonFunction LastCycle { get; } = ButtonFunction.Create(ButtonFunctions.HistoryBase + 3);
	}
}