using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.Gaff
{
	[ButtonFunctions("Gaff")]
	public static class GaffButtonFunctions
	{
		public static ButtonFunction ToggleGaffMenu { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 0);
		public static ButtonFunction AddCredits { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 1);
		public static ButtonFunction IncAddCreditsAmount { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 2);
		public static ButtonFunction DecAddCreditsAmount { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 3);
		public static ButtonFunction GaffsUp { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 4);
		public static ButtonFunction GaffsDown { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 5);

		public static ButtonFunction ClearGaff { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 6);
		public static ButtonFunction RepeatGaff { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 7);
		public static ButtonFunction ToggleTestMessages { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 8);
		public static ButtonFunction ToggleDialUp { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 9);
		public static ButtonFunction ToggleSelfPlay { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 10);
		public static ButtonFunction ToggleSelfPlayAddCredits { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 11);
		public static ButtonFunction SelfPlayAbort { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 12);
		public static ButtonFunction ChangeGaffFilter { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 13);
		public static ButtonFunction SpeedUp { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 14);
		public static ButtonFunction SpeedDown { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 15);
		public static ButtonFunction FastForwardFeature { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 16);
		public static ButtonFunction DialUpAgain { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 17);
		public static ButtonFunction DialUpContinue { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 18);
		public static ButtonFunction ToggleDebug { get; } = ButtonFunction.Create(ButtonFunctions.GaffBase + 19);

		public static ButtonFunctions GaffSelectionBase { get; } = ButtonFunctions.GaffBase + 20;
		public static ButtonFunction Gaff0 { get; } = ButtonFunction.Create(GaffSelectionBase + 0);
		public static ButtonFunction Gaff1 { get; } = ButtonFunction.Create(GaffSelectionBase + 1);
		public static ButtonFunction Gaff2 { get; } = ButtonFunction.Create(GaffSelectionBase + 2);
		public static ButtonFunction Gaff3 { get; } = ButtonFunction.Create(GaffSelectionBase + 3);
		public static ButtonFunction Gaff4 { get; } = ButtonFunction.Create(GaffSelectionBase + 4);
		public static ButtonFunction Gaff5 { get; } = ButtonFunction.Create(GaffSelectionBase + 5);
		public static ButtonFunction Gaff6 { get; } = ButtonFunction.Create(GaffSelectionBase + 6);
		public static ButtonFunction Gaff7 { get; } = ButtonFunction.Create(GaffSelectionBase + 7);

		public static ButtonFunction[] AllGaffButtons => new[] { Gaff0, Gaff1, Gaff2, Gaff3, Gaff4, Gaff5, Gaff6, Gaff7 };
	}
}
