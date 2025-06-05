using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Midas.Presentation.ButtonHandling;

namespace Midas.Gamble.Presentation
{
	[ButtonFunctions("Trumps")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public static class TrumpsButtonFunctions
	{
		public static ButtonFunction DeclineTrumps => ButtonFunction.Create(ButtonFunctions.GambleFeatureBase + 0);
		public static ButtonFunction Red => ButtonFunction.Create(ButtonFunctions.GambleFeatureBase + 1);
		public static ButtonFunction Black => ButtonFunction.Create(ButtonFunctions.GambleFeatureBase + 2);
		public static ButtonFunction Heart => ButtonFunction.Create(ButtonFunctions.GambleFeatureBase + 3);
		public static ButtonFunction Diamond => ButtonFunction.Create(ButtonFunctions.GambleFeatureBase + 4);
		public static ButtonFunction Club => ButtonFunction.Create(ButtonFunctions.GambleFeatureBase + 5);
		public static ButtonFunction Spade => ButtonFunction.Create(ButtonFunctions.GambleFeatureBase + 6);

		public static IReadOnlyList<ButtonFunction> TrumpsButtons => new ReadOnlyCollection<ButtonFunction>(new[] { DeclineTrumps, Red, Black, Heart, Diamond, Club, Spade });
	}
}