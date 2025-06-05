using Logic.Core.Engine;
using Logic.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
	public class PidProvider : IPid
	{
		private sealed class Pid
		{
			public double GamesPerWin { get; }
			public IReadOnlyList<(string, int)> LargestPrizes { get; }
			public IReadOnlyList<(string, int)> SmallestPrizes { get; }

			public Pid(double g, IReadOnlyList<(string, int)> l, IReadOnlyList<(string, int)> s)
			{
				GamesPerWin = g;
				LargestPrizes = l;
				SmallestPrizes = s;
			}
		}

		private readonly SelectorItems pidList = new SelectorItems(
			new List<SelectorItem>()
			{
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL)) }, new Pid(41.51, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 11234), ("4 TURTLE", 3458), ("5 LIONFISH", 9423), ("5 CLOWNFISH", 12014) }, new List<(string, int)>() { ("3 NINE", 1807), ("3 TEN", 878), ("3 JACK", 650), ("3 QUEEN", 520), ("3 KING", 994) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(5UL), new Money(10UL)) }, new Pid(41.51, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 11234), ("5 LIONFISH", 9423), ("5 CLOWNFISH", 12014), ("5 SHELL", 7951) }, new List<(string, int)>() { ("3 NINE", 1807), ("3 TEN", 878), ("3 JACK", 650), ("3 QUEEN", 520), ("3 KING", 994) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(17.63, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 12912), ("5 CLOWNFISH", 15111), ("5 SHELL", 7326), ("5 STARFISH", 6571) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 90), ("3 NINE", 1121), ("3 TEN", 487) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)) }, new Pid(17.65, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 12912), ("5 CLOWNFISH", 15111), ("5 SHELL", 7326), ("5 STARFISH", 6571) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 90), ("3 NINE", 1121), ("3 TEN", 521) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL)) }, new Pid(41.5, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 11702), ("4 TURTLE", 3458), ("5 LIONFISH", 9832), ("5 CLOWNFISH", 12014) }, new List<(string, int)>() { ("3 NINE", 1784), ("3 TEN", 804), ("3 JACK", 630), ("3 QUEEN", 549), ("3 KING", 1006) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(5UL), new Money(10UL)) }, new Pid(41.5, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 11702), ("5 LIONFISH", 9832), ("5 CLOWNFISH", 12014), ("5 SHELL", 8448) }, new List<(string, int)>() { ("3 NINE", 1784), ("3 TEN", 804), ("3 JACK", 630), ("3 QUEEN", 549), ("3 KING", 1006) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(17.68, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 13630), ("5 CLOWNFISH", 15111), ("5 SHELL", 7733), ("5 STARFISH", 7077) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 90), ("3 NINE", 1121), ("3 TEN", 541) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)) }, new Pid(17.62, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 14431), ("5 CLOWNFISH", 16274), ("5 SHELL", 7733), ("5 STARFISH", 6815) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 90), ("3 NINE", 1121), ("3 TEN", 515) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL)) }, new Pid(41.68, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 13653), ("4 TURTLE", 3458), ("5 LIONFISH", 10816), ("5 CLOWNFISH", 10813) }, new List<(string, int)>() { ("3 NINE", 1784), ("3 TEN", 804), ("3 JACK", 622), ("3 QUEEN", 556), ("3 KING", 678) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(5UL), new Money(10UL)) }, new Pid(41.68, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 13653), ("5 LIONFISH", 10816), ("5 CLOWNFISH", 10813), ("5 SHELL", 9386) }, new List<(string, int)>() { ("3 NINE", 1784), ("3 TEN", 804), ("3 JACK", 622), ("3 QUEEN", 556), ("3 KING", 678) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(18.09, new List<(string, int)>() { ("5 WHALESHARK", 19309), ("5 LIONFISH", 15333), ("5 CLOWNFISH", 18055), ("5 SHELL", 9280), ("5 STARFISH", 8593) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 103), ("3 NINE", 1121), ("3 TEN", 555) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL), new Money(200UL)) }, new Pid(18.09, new List<(string, int)>() { ("5 WHALESHARK", 18102), ("5 LIONFISH", 14431), ("5 CLOWNFISH", 16851), ("5 SHELL", 9280), ("5 STARFISH", 7519) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 103), ("3 NINE", 1134), ("3 TEN", 548) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(17.65, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 12912), ("5 CLOWNFISH", 15111), ("5 SHELL", 7326), ("5 STARFISH", 6571) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 90), ("3 NINE", 1108), ("3 TEN", 521) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL)) }, new Pid(17.63, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 15839), ("4 TURTLE", 3458), ("5 LIONFISH", 12912), ("5 CLOWNFISH", 15111) }, new List<(string, int)>() { ("3 NINE", 1121), ("3 TEN", 487), ("3 JACK", 538), ("3 QUEEN", 472), ("3 KING", 819) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(17.68, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 13630), ("5 CLOWNFISH", 15111), ("5 SHELL", 7733), ("5 STARFISH", 7077) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 90), ("3 NINE", 1121), ("3 TEN", 541) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL)) }, new Pid(17.62, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 15839), ("4 TURTLE", 3458), ("5 LIONFISH", 14431), ("5 CLOWNFISH", 16274) }, new List<(string, int)>() { ("3 NINE", 1134), ("3 TEN", 521), ("3 JACK", 445), ("3 QUEEN", 478), ("3 KING", 819) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(18.09, new List<(string, int)>() { ("5 WHALESHARK", 18102), ("5 LIONFISH", 14431), ("5 CLOWNFISH", 16851), ("5 SHELL", 8838), ("5 STARFISH", 8021) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 TURTLE", 155), ("2 WHALESHARK", 103), ("3 NINE", 1134), ("3 TEN", 548) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL)) }, new Pid(18.4, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 18798), ("4 TURTLE", 3458), ("5 LIONFISH", 15333), ("5 CLOWNFISH", 16274) }, new List<(string, int)>() { ("3 NINE", 1148), ("3 TEN", 503), ("3 JACK", 435), ("3 QUEEN", 472), ("3 KING", 670) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL)) }, new Pid(41.5, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 11234), ("4 TURTLE", 3458), ("5 LIONFISH", 9423), ("5 CLOWNFISH", 12014) }, new List<(string, int)>() { ("3 NINE", 1807), ("3 TEN", 866), ("3 JACK", 614), ("3 QUEEN", 542), ("3 KING", 1006) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(5UL), new Money(10UL)) }, new Pid(41.5, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 11234), ("5 LIONFISH", 9423), ("5 CLOWNFISH", 12014), ("5 SHELL", 7951) }, new List<(string, int)>() { ("3 NINE", 1807), ("3 TEN", 866), ("3 JACK", 614), ("3 QUEEN", 542), ("3 KING", 1006) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(17.61, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 12912), ("5 CLOWNFISH", 15111), ("5 SHELL", 7326), ("5 STARFISH", 6815) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1121), ("3 TEN", 509), ("3 JACK", 486), ("3 QUEEN", 472) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL)) }, new Pid(17.64, new List<(string, int)>() { ("5 WHALESHARK", 13761), ("5 LIONFISH", 12912), ("5 CLOWNFISH", 15111), ("5 SHELL", 7326), ("5 STARFISH", 6815) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1229), ("3 TEN", 497), ("3 JACK", 486), ("3 QUEEN", 472) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(200UL)) }, new Pid(17.61, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 12912), ("5 CLOWNFISH", 15111), ("5 SHELL", 7326), ("5 STARFISH", 6815) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1134), ("3 TEN", 503), ("3 JACK", 480), ("3 QUEEN", 472) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL)) }, new Pid(41.46, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 11702), ("4 TURTLE", 3458), ("5 LIONFISH", 9832), ("5 CLOWNFISH", 12014) }, new List<(string, int)>() { ("3 NINE", 1784), ("3 TEN", 878), ("3 JACK", 591), ("3 QUEEN", 549), ("3 KING", 994) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(5UL), new Money(10UL)) }, new Pid(41.46, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 11702), ("5 LIONFISH", 9832), ("5 CLOWNFISH", 12014), ("5 SHELL", 8448) }, new List<(string, int)>() { ("3 NINE", 1784), ("3 TEN", 878), ("3 JACK", 591), ("3 QUEEN", 549), ("3 KING", 994) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(17.68, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 13630), ("5 CLOWNFISH", 15111), ("5 SHELL", 7733), ("5 STARFISH", 7077) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1121), ("3 TEN", 587), ("3 JACK", 430), ("3 QUEEN", 478) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL)) }, new Pid(17.71, new List<(string, int)>() { ("5 WHALESHARK", 14621), ("5 LIONFISH", 13630), ("5 CLOWNFISH", 15111), ("5 SHELL", 7733), ("5 STARFISH", 7077) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1200), ("3 TEN", 587), ("3 JACK", 425), ("3 QUEEN", 490) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(200UL)) }, new Pid(17.68, new List<(string, int)>() { ("5 WHALESHARK", 15839), ("5 LIONFISH", 13630), ("5 CLOWNFISH", 15111), ("5 SHELL", 7733), ("5 STARFISH", 7077) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1121), ("3 TEN", 587), ("3 JACK", 430), ("3 QUEEN", 478) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(1UL), new Money(2UL)) }, new Pid(41.58, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 13653), ("4 TURTLE", 3458), ("5 LIONFISH", 10816), ("5 CLOWNFISH", 10813) }, new List<(string, int)>() { ("3 NINE", 1829), ("3 TEN", 855), ("3 JACK", 583), ("3 QUEEN", 556), ("3 KING", 717) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(5UL), new Money(10UL)) }, new Pid(41.58, new List<(string, int)>() { ("5 TURTLE", 225268), ("5 WHALESHARK", 13653), ("5 LIONFISH", 10816), ("5 CLOWNFISH", 10813), ("5 SHELL", 9386) }, new List<(string, int)>() { ("3 NINE", 1829), ("3 TEN", 855), ("3 JACK", 583), ("3 QUEEN", 556), ("3 KING", 717) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(50UL)) }, new Pid(18.09, new List<(string, int)>() { ("5 WHALESHARK", 17037), ("5 LIONFISH", 14431), ("5 CLOWNFISH", 16851), ("5 SHELL", 8838), ("5 STARFISH", 8021) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1134), ("3 TEN", 587), ("3 JACK", 366), ("3 QUEEN", 461) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(100UL)) }, new Pid(18.12, new List<(string, int)>() { ("5 WHALESHARK", 14431), ("5 LIONFISH", 14431), ("5 CLOWNFISH", 16851), ("5 SHELL", 8838), ("5 STARFISH", 8021) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1215), ("3 TEN", 594), ("3 JACK", 370), ("3 QUEEN", 455) })),
				new SelectorItem(new Requirement[] { Requirement.Create("Denom", new Money(200UL)) }, new Pid(18.09, new List<(string, int)>() { ("5 WHALESHARK", 18102), ("5 LIONFISH", 14431), ("5 CLOWNFISH", 16851), ("5 SHELL", 8838), ("5 STARFISH", 8021) }, new List<(string, int)>() { ("2 LIONFISH", 55), ("3 NINE", 1134), ("3 TEN", 601), ("3 JACK", 370), ("3 QUEEN", 455) }))
			}
		);

		public double GetGamesPerWin(Inputs inputs)
		{
			return Select(inputs).GamesPerWin;
		}

		public IReadOnlyList<(string, int)> GetLargestPrizes(Inputs inputs)
		{
			return Select(inputs).LargestPrizes;
		}

		public IReadOnlyList<(string, int)> GetSmallestPrizes(Inputs inputs)
		{
			return Select(inputs).SmallestPrizes;
		}

		private Pid Select(Inputs inputs)
		{
			if (pidList.Count == 0)
				throw new Exception($"No pid data found");

			if (pidList[0].Requirements.Count == 0)
				return (Pid)pidList[0].Data;

			for (var i = 0; i < pidList.Count; i++)
			{
				var item = pidList[i];
				var allMatch = true;

				foreach (var req in item.Requirements)
				{
					var value = inputs.GetInput(req.Hint);

					if (!req.Contains(value))
					{
						allMatch = false;
						break;
					}
				}

				if (allMatch)
					return (Pid)item.Data;
			}

			throw new Exception($"No match found in Select");
		}
	}
}
