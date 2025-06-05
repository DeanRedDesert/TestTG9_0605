using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.Dashboard
{
	[ButtonFunctions("Dashboard")]
	public static class DashboardButtonFunctions
	{
		public static ButtonFunction MoreGames { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 0);
		public static ButtonFunction Volume { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 1);
		public static ButtonFunction Cashout { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 2);
		public static ButtonFunction Info { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 3);
		public static ButtonFunction Reserve { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 4);
		public static ButtonFunction Speed { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 5);
		public static ButtonFunction Service { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 6);
		public static ButtonFunction CashoutYes { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 7);
		public static ButtonFunction CashoutNo { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 8);
		public static ButtonFunction SimulatedHardwareCashout { get; } = ButtonFunction.Create(ButtonFunctions.DashboardBase + 9);
	}
}