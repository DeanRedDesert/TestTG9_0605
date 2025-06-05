using System.Collections.Generic;
using System.Collections.ObjectModel;
using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.Denom
{
	[ButtonFunctions("Denom")]
	public static class DenomButtonFunctions
	{
		public static ButtonFunction ChangeDenom => ButtonFunction.Create(ButtonFunctions.DenomBase + 0);
		public static ButtonFunction ConfirmYes => ButtonFunction.Create(ButtonFunctions.DenomBase + 1);
		public static ButtonFunction ConfirmNo => ButtonFunction.Create(ButtonFunctions.DenomBase + 2);

		private const ButtonFunctions DenomSelBase = ButtonFunctions.DenomBase + 10;
		public static ButtonFunction Denom1 { get; } = ButtonFunction.Create(DenomSelBase + 0);
		public static ButtonFunction Denom2 { get; } = ButtonFunction.Create(DenomSelBase + 1);
		public static ButtonFunction Denom3 { get; } = ButtonFunction.Create(DenomSelBase + 2);
		public static ButtonFunction Denom4 { get; } = ButtonFunction.Create(DenomSelBase + 3);
		public static ButtonFunction Denom5 { get; } = ButtonFunction.Create(DenomSelBase + 4);
		public static ButtonFunction Denom6 { get; } = ButtonFunction.Create(DenomSelBase + 5);
		public static ButtonFunction Denom7 { get; } = ButtonFunction.Create(DenomSelBase + 6);
		public static ButtonFunction Denom8 { get; } = ButtonFunction.Create(DenomSelBase + 7);
		public static ButtonFunction Denom9 { get; } = ButtonFunction.Create(DenomSelBase + 8);
		public static ButtonFunction Denom10 { get; } = ButtonFunction.Create(DenomSelBase + 9);
		public static ButtonFunction Denom11 { get; } = ButtonFunction.Create(DenomSelBase + 10);
		public static ButtonFunction Denom12 { get; } = ButtonFunction.Create(DenomSelBase + 11);
		public static ButtonFunction Denom13 { get; } = ButtonFunction.Create(DenomSelBase + 12);
		public static ButtonFunction Denom14 { get; } = ButtonFunction.Create(DenomSelBase + 13);
		public static ButtonFunction Denom15 { get; } = ButtonFunction.Create(DenomSelBase + 14);
		public static ButtonFunction Denom16 { get; } = ButtonFunction.Create(DenomSelBase + 15);
		public static ButtonFunction Denom17 { get; } = ButtonFunction.Create(DenomSelBase + 16);
		public static ButtonFunction Denom18 { get; } = ButtonFunction.Create(DenomSelBase + 17);
		public static ButtonFunction Denom19 { get; } = ButtonFunction.Create(DenomSelBase + 18);
		public static ButtonFunction Denom20 { get; } = ButtonFunction.Create(DenomSelBase + 19);

		public static IReadOnlyList<ButtonFunction> DenomButtons => new ReadOnlyCollection<ButtonFunction>(new[] { Denom1, Denom2, Denom3, Denom4, Denom5, Denom6, Denom7, Denom8, Denom9, Denom10, Denom11, Denom12, Denom13, Denom14, Denom15, Denom16, Denom17, Denom18, Denom19, Denom20 });
	}
}