using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator;

namespace Gaff.DecisionMakers
{
	public static class GaffDecisionHelper
	{
		private sealed class StandardRandom : RandomNumberGenerator
		{
			private readonly Random random = new Random();

			public override ulong NextULong()
			{
				var data = new byte[8];
				random.NextBytes(data);
				return BitConverter.ToUInt64(data, 0);
			}
		}

		private static readonly RandomNumberGenerator rng = new StandardRandom();

		/// <summary>
		/// Returns a random boolean decision based on a true and false weight.
		/// </summary>
		/// <param name="trueWeight">The weight of a true result.</param>
		/// <param name="falseWeight">The weight of a false result.</param>
		/// <returns>Returns the result of the decision.</returns>
		public static bool GetDecision(ulong trueWeight, ulong falseWeight)
		{
			ulong totalWeight;

			checked
			{
				totalWeight = trueWeight + falseWeight;
			}

			if (totalWeight > 0x2000000000000)
				throw new ArgumentException("Total weight cannot exceed 0x2000000000000", $"trueWeight (0x{trueWeight:X}) + falseWeight (0x{falseWeight:X})");

			if (totalWeight == 0UL)
				throw new ArgumentException("Total weight cannot be zero", $"trueWeight (0x{trueWeight:X}) + falseWeight (0x{falseWeight:X})");

			return rng.Next(totalWeight) < trueWeight;
		}

		/// <summary>
		/// Choose random index that meets the requirements found in the <see cref="check"/> function. If none can be found use the <see cref="startingPoint"/>.
		/// </summary>
		/// <param name="getSymbol">A function to get the symbol name.</param>
		/// <param name="check">A function to check if a symbol meets the requirements.</param>
		/// <param name="indexCount">The total amount of indexes.</param>
		/// <param name="symbolWindow">The window in which to place the symbol that meets the requirements.</param>
		/// <param name="result">The chosen index.</param>
		/// <param name="startingPoint">The index to start the search at.  It will also be the index that will be returned on failure.</param>
		/// <returns>Returns true if an index that met the requirements was chosen.</returns>
		public static bool ConditionalIndexSelection(Func<ulong, string> getSymbol, Func<string, bool> check, ulong indexCount, uint symbolWindow, out ulong result, ulong startingPoint)
		{
			for (ulong i = 0; i < indexCount; i++)
			{
				var index = (startingPoint + i) % indexCount;

				if (check(getSymbol(index)))
				{
					result = (indexCount + index - rng.Next(symbolWindow)) % indexCount;
					return true;
				}
			}

			// Returns a valid value and an indication of failure.
			result = startingPoint;
			return false;
		}

		/// <summary>
		/// Choose random index that meets the requirements found in the <see cref="check"/> function. If none can be found use the random starting point that was chosen.
		/// </summary>
		/// <param name="getSymbol">A function to get the symbol name.</param>
		/// <param name="check">A function to check if a symbol meets the requirements.</param>
		/// <param name="indexCount">The total amount of indexes.</param>
		/// <param name="symbolWindow">The window in which to place the symbol that meets the requirements.</param>
		/// <param name="result">The chosen index.</param>
		/// <returns>Returns true if an index that met the requirements was chosen.</returns>
		public static bool ConditionalIndexSelection(Func<ulong, string> getSymbol, Func<string, bool> check, ulong indexCount, uint symbolWindow, out ulong result)
		{
			return ConditionalIndexSelection(getSymbol, check, indexCount, symbolWindow, out result, rng.Next(indexCount));
		}

		/// <summary>
		/// Fill a list with indexes that fit the <see cref="check"/> function.  If none can be found use the random starting point that was chosen.
		/// </summary>
		/// <param name="getSymbol">A function to get the symbol name.</param>
		/// <param name="check">A function to check if a symbol meets the requirements.</param>
		/// <param name="allowDuplicates">Are duplicate indexes allowed in the result.</param>
		/// <param name="indexCount">The total amount of indexes.</param>
		/// <param name="count">The requested amount of indexes.</param>
		/// <param name="symbolWindow">The window in which to place the symbol that meets the requirements.</param>
		/// <param name="items">The list of chosen indexes.</param>
		public static void Fill(Func<ulong, string> getSymbol, Func<string, bool> check, bool allowDuplicates, ulong indexCount, uint count, uint symbolWindow, List<ulong> items)
		{
			if (allowDuplicates)
			{
				while (items.Count < count)
				{
					ConditionalIndexSelection(getSymbol, check, indexCount, symbolWindow, out var decision);
					items.Add(decision);
				}
			}
			else
			{
				var itemsHash = new HashSet<ulong>(items);

				while (items.Count < count)
				{
					ConditionalIndexSelection(getSymbol, check, indexCount, symbolWindow, out var next);

					while (itemsHash.Contains(next))
					{
						ConditionalIndexSelection(getSymbol, check, indexCount, symbolWindow, out var index);
						next = index;
					}

					items.Add(next);
					itemsHash.Add(next);
				}
			}
		}

		/// <summary>
		/// Choose some random indexes.
		/// </summary>
		/// <param name="indexCount">The count of indexes available.</param>
		/// <param name="count">How many indexes to choose.</param>
		/// <param name="allowDuplicates">Are duplicates allowed.</param>
		/// <returns>A list if random indexes less than <see cref="indexCount"/></returns>
		public static IReadOnlyList<int> ChooseRandomIndexes(int indexCount, uint count, bool allowDuplicates)
		{
			if (indexCount == 0)
				throw new ArgumentException("indexCount must be greater than zero.");

			if (count == 0U)
				throw new ArgumentException("count must be greater than zero.");

			if (!allowDuplicates && count > indexCount)
				throw new ArgumentException("count must be less than or equal to indexCount.");

			var chosenIndexes = new List<int>();

			if (allowDuplicates)
			{
				for (var i = 0U; i < count; i++)
					chosenIndexes.Add((int)rng.Next((ulong)indexCount));
			}
			else
			{
				for (var i = 0U; i < count; i++)
				{
					var next = (int)rng.Next((ulong)(indexCount - i));

					// Need to adjust next to account for values that have already been selected.

					foreach (var c in chosenIndexes.OrderBy(c => c))
					{
						if (c <= next)
							next++;
						else
							break;
					}

					chosenIndexes.Add(next);
				}
			}

			return chosenIndexes;
		}
	}
}