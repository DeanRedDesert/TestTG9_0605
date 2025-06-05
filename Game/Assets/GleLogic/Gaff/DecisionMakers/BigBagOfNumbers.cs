using System;
using System.Collections.Generic;
using System.Linq;
using Gaff.Conditions;

namespace Gaff.DecisionMakers
{
	/// <summary>
	/// A class to centralise the tracking of handing out valid indexes with and without replacement (allowing or not allowing duplicates).
	/// The <see cref="Next(string, int, out ulong)"/> call using the format {SymbolName}[|{SymbolName}...] to specify the symbols to look for.
	/// E.g. 'M1|M2|SC', 'WW|M1' and MINI are all valid.
	/// </summary>
	public sealed class BigBagOfNumbers
	{
		private readonly ulong totalIndexes;
		private readonly bool allowDuplicates;
		private readonly Func<ulong, string> getName;
		private readonly ulong failCount;
		private readonly HashSet<ulong> taken;

		public BigBagOfNumbers(ulong totalIndexes, bool allowDuplicates, Func<ulong, string> getName, ulong failCount = 10000)
		{
			this.totalIndexes = totalIndexes;
			this.allowDuplicates = allowDuplicates;
			this.getName = getName;
			this.failCount = failCount;
			taken = new HashSet<ulong>();
		}

		public bool Next(out ulong number) => allowDuplicates ? NextWithDuplicates(out number) : NextWithoutDuplicates(out number);
		public bool Next(string symText, int symbolWindow, out ulong number) => allowDuplicates ? NextWithDuplicates(symText, symbolWindow, out number) : NextWithoutDuplicates(symText, symbolWindow, out number);

		private bool NextWithDuplicates(out ulong number)
		{
			number = ConditionHelpers.NextULong(totalIndexes);

			return true;
		}

		private bool NextWithDuplicates(string symText, int symbolWindow, out ulong number)
		{
			number = ulong.MinValue;
			var symbols = symText.SplitAndTrim("|");
			var next = ConditionHelpers.NextULong(totalIndexes);
			var tryCount = 0UL;

			while (getName == null || symbols.All(s => getName(next) != s))
			{
				if (tryCount > failCount)
					return false;

				next = ConditionHelpers.NextULong(totalIndexes);
				tryCount++;
			}

			number = (totalIndexes + next - ConditionHelpers.NextULong((ulong)symbolWindow)) % totalIndexes;
			return true;
		}

		private bool NextWithoutDuplicates(out ulong number)
		{
			number = ulong.MinValue;

			if (totalIndexes == (ulong)taken.Count)
				return false;

			var next = ConditionHelpers.NextULong(totalIndexes);
			var tryCount = 0UL;

			while (taken.Contains(next))
			{
				if (tryCount > failCount)
					return false;

				next = ConditionHelpers.NextULong(totalIndexes);
				tryCount++;
			}

			number = next;
			taken.Add(number);

			return true;
		}

		private bool NextWithoutDuplicates(string symText, int symbolWindow, out ulong number)
		{
			number = ulong.MinValue;

			if (totalIndexes == (ulong)taken.Count)
				return false;

			var next = ConditionHelpers.NextULong(totalIndexes);
			var symbols = symText.Split('|');
			var tryCount = 0UL;
			var localTaken = new HashSet<ulong>(taken);

			while (localTaken.Contains(next) || getName == null || symbols.All(s => getName(next) != s))
			{
				localTaken.Add(next);
				tryCount++;

				if (tryCount > failCount)
					return false;

				next = ConditionHelpers.NextULong(totalIndexes);
			}

			number = (totalIndexes + next - ConditionHelpers.NextULong((ulong)symbolWindow)) % totalIndexes;
			taken.Add(number);

			return true;
		}
	}
}