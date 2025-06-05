using System.Collections.Generic;

namespace Midas.Presentation.ButtonHandling
{
	public sealed class PhysicalButtons
	{
		private readonly Dictionary<string, PhysicalButton> cachedButtons = new Dictionary<string, PhysicalButton>();

		public PhysicalButton Cashout { get; }
		public PhysicalButton Ghost { get; }
		public PhysicalButton LeftSmash { get; }
		public PhysicalButton RightSmash { get; }
		public PhysicalButton TakeWin { get; }
		public PhysicalButton Gamble { get; }
		public PhysicalButton Info { get; }
		public PhysicalButton Row11 { get; }
		public PhysicalButton Row12 { get; }
		public PhysicalButton Row13 { get; }
		public PhysicalButton Row14 { get; }
		public PhysicalButton Row15 { get; }
		public PhysicalButton Row21 { get; }
		public PhysicalButton Row22 { get; }
		public PhysicalButton Row23 { get; }
		public PhysicalButton Row24 { get; }
		public PhysicalButton Row25 { get; }

		// ReSharper disable once FunctionComplexityOverflow
		public PhysicalButtons(PhysicalButton? cashOut = null,
			PhysicalButton? ghost = null,
			PhysicalButton? leftSmash = null,
			PhysicalButton? rightSmash = null,
			PhysicalButton? takeWin = null,
			PhysicalButton? gamble = null,
			PhysicalButton? info = null,
			PhysicalButton? row11 = null,
			PhysicalButton? row12 = null,
			PhysicalButton? row13 = null,
			PhysicalButton? row14 = null,
			PhysicalButton? row15 = null,
			PhysicalButton? row21 = null,
			PhysicalButton? row22 = null,
			PhysicalButton? row23 = null,
			PhysicalButton? row24 = null,
			PhysicalButton? row25 = null)
		{
			PhysicalButton CacheButtonValue(PhysicalButton? b, string buttonName)
			{
				if (b == null)
					return PhysicalButton.Undefined;
				cachedButtons.Add(buttonName, b.Value);
				return b.Value;
			}

			Cashout = CacheButtonValue(cashOut, nameof(Cashout));
			Ghost = CacheButtonValue(ghost, nameof(Ghost));
			LeftSmash = CacheButtonValue(leftSmash, nameof(LeftSmash));
			RightSmash = CacheButtonValue(rightSmash, nameof(RightSmash));
			TakeWin = CacheButtonValue(takeWin, nameof(TakeWin));
			Gamble = CacheButtonValue(gamble, nameof(Gamble));
			Info = CacheButtonValue(info, nameof(Info));
			Row11 = CacheButtonValue(row11, nameof(Row11));
			Row12 = CacheButtonValue(row12, nameof(Row12));
			Row13 = CacheButtonValue(row13, nameof(Row13));
			Row14 = CacheButtonValue(row14, nameof(Row14));
			Row15 = CacheButtonValue(row15, nameof(Row15));
			Row21 = CacheButtonValue(row21, nameof(Row21));
			Row22 = CacheButtonValue(row22, nameof(Row22));
			Row23 = CacheButtonValue(row23, nameof(Row23));
			Row24 = CacheButtonValue(row24, nameof(Row24));
			Row25 = CacheButtonValue(row25, nameof(Row25));
		}

		public PhysicalButton? GetButtonFromName(string buttonName)
		{
			if (buttonName != "" && buttonName != "Undefined")
			{
				if (cachedButtons.TryGetValue(buttonName, out var physicalButton))
					return physicalButton;
			}

			return null;
		}
	}
}