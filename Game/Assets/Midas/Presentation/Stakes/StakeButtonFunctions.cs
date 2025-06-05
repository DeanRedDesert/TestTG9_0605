using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.Stakes
{
	[ButtonFunctions("Stakes")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public static class StakeButtonFunctions
	{
		public static bool IsPlayButtonFunction(this ButtonFunction buttonFunction)
		{
			return Play.Equals(buttonFunction) || PlayButtons.Any(b => b.Equals(buttonFunction));
		}

		public static ButtonFunction Play => ButtonFunction.Create(ButtonFunctions.StakeBase + 0);

		#region Bet Button functions (ie, bet x credits)

		private const ButtonFunctions BetBase = ButtonFunctions.StakeBase + 10;
		public static ButtonFunction Bet1 => ButtonFunction.Create(BetBase + 0);
		public static ButtonFunction Bet2 => ButtonFunction.Create(BetBase + 1);
		public static ButtonFunction Bet3 => ButtonFunction.Create(BetBase + 2);
		public static ButtonFunction Bet4 => ButtonFunction.Create(BetBase + 3);
		public static ButtonFunction Bet5 => ButtonFunction.Create(BetBase + 4);
		public static ButtonFunction Bet6 => ButtonFunction.Create(BetBase + 5);
		public static ButtonFunction Bet7 => ButtonFunction.Create(BetBase + 6);
		public static ButtonFunction Bet8 => ButtonFunction.Create(BetBase + 7);
		public static ButtonFunction Bet9 => ButtonFunction.Create(BetBase + 8);
		public static ButtonFunction Bet10 => ButtonFunction.Create(BetBase + 9);

		public static IReadOnlyList<ButtonFunction> BetButtons => new ReadOnlyCollection<ButtonFunction>(new[] { Bet1, Bet2, Bet3, Bet4, Bet5, Bet6, Bet7, Bet8, Bet9, Bet10 });

		#endregion

		#region Play Button functions (ie, play x lines)

		private const ButtonFunctions PlayBase = ButtonFunctions.StakeBase + 30;
		public static ButtonFunction Play1 => ButtonFunction.Create(PlayBase + 0);
		public static ButtonFunction Play2 => ButtonFunction.Create(PlayBase + 1);
		public static ButtonFunction Play3 => ButtonFunction.Create(PlayBase + 2);
		public static ButtonFunction Play4 => ButtonFunction.Create(PlayBase + 3);
		public static ButtonFunction Play5 => ButtonFunction.Create(PlayBase + 4);
		public static ButtonFunction Play6 => ButtonFunction.Create(PlayBase + 5);
		public static ButtonFunction Play7 => ButtonFunction.Create(PlayBase + 6);
		public static ButtonFunction Play8 => ButtonFunction.Create(PlayBase + 7);
		public static ButtonFunction Play9 => ButtonFunction.Create(PlayBase + 8);
		public static ButtonFunction Play10 => ButtonFunction.Create(PlayBase + 9);

		public static IReadOnlyList<ButtonFunction> PlayButtons => new ReadOnlyCollection<ButtonFunction>(new[] { Play1, Play2, Play3, Play4, Play5, Play6, Play7, Play8, Play9, Play10 });

		#endregion

		#region Cost to cover stake buttons (ie, play x credits)

		private const ButtonFunctions StakeBase = ButtonFunctions.StakeBase + 50;
		public static ButtonFunction Stake01 => ButtonFunction.Create(StakeBase + 0);
		public static ButtonFunction Stake02 => ButtonFunction.Create(StakeBase + 1);
		public static ButtonFunction Stake03 => ButtonFunction.Create(StakeBase + 2);
		public static ButtonFunction Stake04 => ButtonFunction.Create(StakeBase + 3);
		public static ButtonFunction Stake05 => ButtonFunction.Create(StakeBase + 4);
		public static ButtonFunction Stake06 => ButtonFunction.Create(StakeBase + 5);
		public static ButtonFunction Stake07 => ButtonFunction.Create(StakeBase + 6);
		public static ButtonFunction Stake08 => ButtonFunction.Create(StakeBase + 7);
		public static ButtonFunction Stake09 => ButtonFunction.Create(StakeBase + 8);
		public static ButtonFunction Stake10 => ButtonFunction.Create(StakeBase + 9);
		public static ButtonFunction Stake11 => ButtonFunction.Create(StakeBase + 10);
		public static ButtonFunction Stake12 => ButtonFunction.Create(StakeBase + 11);
		public static ButtonFunction Stake13 => ButtonFunction.Create(StakeBase + 12);
		public static ButtonFunction Stake14 => ButtonFunction.Create(StakeBase + 13);
		public static ButtonFunction Stake15 => ButtonFunction.Create(StakeBase + 14);
		public static ButtonFunction Stake16 => ButtonFunction.Create(StakeBase + 15);
		public static ButtonFunction Stake17 => ButtonFunction.Create(StakeBase + 16);
		public static ButtonFunction Stake18 => ButtonFunction.Create(StakeBase + 17);
		public static ButtonFunction Stake19 => ButtonFunction.Create(StakeBase + 18);
		public static ButtonFunction Stake20 => ButtonFunction.Create(StakeBase + 19);

		public static IReadOnlyList<ButtonFunction> StakeButtons => new ReadOnlyCollection<ButtonFunction>(new[]
		{
			Stake01, Stake02, Stake03, Stake04, Stake05, Stake06, Stake07, Stake08, Stake09, Stake10,
			Stake11, Stake12, Stake13, Stake14, Stake15, Stake16, Stake17, Stake18, Stake19, Stake20
		});

		#endregion
	}
}