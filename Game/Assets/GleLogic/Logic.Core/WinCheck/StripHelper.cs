using System;
using System.Collections.Generic;
using System.Text;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// Methods to help in the creation of IStrip objects.
	/// </summary>
	public static class StripHelper
	{
		/// <summary>
		/// Create a strip with an associated symbol list from a collection of stops, ignoring any weight information.
		/// </summary>
		public static ISymbolListStrip CreateStrip(this IReadOnlyList<Stop> stops, SymbolList symbolList)
		{
			var numArray = new int[stops.Count];

			for (var index = 0; index < stops.Count; ++index)
				numArray[index] = symbolList.IndexOf(stops[index].Symbol);

			return new SymbolListStrip(symbolList, numArray);
		}

		/// <summary>
		/// Create a strip object from a collection of stops. By default, if a stop with zero weight is found on creation an exception will be thrown.
		/// </summary>
		public static ISymbolListStrip CreateWeightedStrip(this IReadOnlyList<Stop> stops, SymbolList symbolList, bool supportsZeroWeight = false)
		{
			var symbolIndexArray = new int[stops.Count];
			var weightArray = new ulong[stops.Count];

			for (var index = 0; index < stops.Count; ++index)
			{
				symbolIndexArray[index] = symbolList.IndexOf(stops[index].Symbol);
				weightArray[index] = stops[index].Weight;
			}

			return new WeightedSymbolListStrip(symbolList, symbolIndexArray, weightArray, supportsZeroWeight);
		}

		/// <summary>
		/// Calculate and return the index at the weight specified assuming the cumulative weight of the strip items.
		/// </summary>
		/// <param name="weight">The target weight</param>
		/// <param name="stripLength">The strip length</param>
		/// <param name="getWeight">A function to get the associated weight at a particular index</param>
		/// <returns>The index at that target weight</returns>
		// ReSharper disable once MemberCanBePrivate.Global
		public static ulong GetIndexAtWeight(ulong weight, ulong stripLength, Func<ulong, ulong> getWeight)
		{
			var currentWeight = 0UL;

			for (var i = 0ul; i < stripLength; i++)
			{
				currentWeight += getWeight(i);

				if (weight < currentWeight)
					return i;
			}

			throw new ArgumentException($"GetIndexAtWeight1: the item weights sum to {currentWeight} but you have requested the item at weight {weight}. It is out of bounds.");
		}

		/// <summary>
		/// Calculate and return the index at the weight specified assuming the cumulative weight of the strip items.
		/// </summary>
		/// <param name="weight">The target weight</param>
		/// <param name="stripLength">The strip length</param>
		/// <param name="totalWeight">The specified total weight</param>
		/// <param name="getWeight">A function to get the associated weight at a particular index</param>
		/// <returns>The index at that target weight</returns>
		// ReSharper disable once MemberCanBePrivate.Global
		public static ulong GetIndexAtWeight(ulong weight, ulong stripLength, ulong totalWeight, Func<ulong, ulong> getWeight)
		{
			var currentWeight = 0UL;

			for (var i = 0UL; i < stripLength; i++)
			{
				currentWeight += getWeight(i);

				if (currentWeight > totalWeight)
					throw new ArgumentException($"GetIndexAtWeight2: the sum of the item weights ({currentWeight}) has exceeded the total weight specified ({totalWeight}). Perhaps the total weight is incorrect.");

				if (weight < currentWeight)
					return i;
			}

			throw new ArgumentException($"GetIndexAtWeight2: the item weights sum to {currentWeight} but you have requested the item at weight {weight}. It is out of bounds.");
		}

		/// <summary>
		/// Calculate and return the index at the weight specified assuming the cumulative weight of the strip items.
		/// </summary>
		/// <param name="weight">The target weight</param>
		/// <param name="stripLength">The strip length</param>
		/// <param name="skipIndexes">The indexes to skip when calculating the current cumulative weight</param>
		/// <param name="getWeight">A function to get the associated weight at a particular index</param>
		/// <returns>The index at that target weight</returns>
		// ReSharper disable once MemberCanBePrivate.Global
		public static ulong GetIndexAtWeight(ulong weight, ulong stripLength, ICollection<ulong> skipIndexes, Func<ulong, ulong> getWeight)
		{
			var currentWeight = 0UL;

			for (var i = 0UL; i < stripLength; i++)
			{
				if (skipIndexes.Contains(i))
					continue;

				currentWeight += getWeight(i);

				if (weight < currentWeight)
					return i;
			}

			throw new ArgumentException($"GetIndexAtWeight3: the item weights sum to {currentWeight} but you have requested the item at weight {weight}. It is out of bounds.");
		}

		/// <summary>
		/// Create a strip object using just the count of indexes and a way to get the symbol at the index specified.
		/// </summary>
		public static IStrip CreateStrip(ulong stripLength, Func<ulong, string> getSymbol)
		{
			return new SimpleStrip(stripLength, getSymbol);
		}

		/// <summary>
		/// Create a strip object using just the count of indexes and a way to get the symbol at the index specified.
		/// </summary>
		public static IStrip CreateStrip(ulong stripLength, Func<ulong, string> getSymbol, Func<ulong, ulong> getWeight)
		{
			return new SimpleWeightedStrip(stripLength, getSymbol, getWeight);
		}

		/// <summary>
		/// Create a strip object from a collection of stops.
		/// </summary>
		// ReSharper disable once UnusedMember.Global - Helper
		public static IStrip CreateStripNoWeights(this IReadOnlyList<Stop> stops)
		{
			return new SimpleStrip((ulong)stops.Count, i => stops[(int)i].Symbol);
		}

		/// <summary>
		/// Create a strip object from a collection of stops. If a stop with zero weight is found on creation an exception will be thrown.
		/// </summary>
		public static IStrip CreateStrip(this IReadOnlyList<Stop> stops)
		{
			return new StopsStrip(stops);
		}

		/// <summary>
		/// Create a strip object from a collection of stops with support for stops that have zero weight.
		/// </summary>
		public static IStrip CreateStripSupportingZeroWeights(this IReadOnlyList<Stop> stops)
		{
			return new StopsStrip(stops, true);
		}

		internal static IReadOnlyList<IReadOnlyList<int>> GetSymbolStripPositions(SymbolList symbolList, IReadOnlyList<int> symbolIndexes)
		{
			var temp = new List<List<int>>();

			foreach (var _ in symbolList)
				temp.Add(new List<int>());

			for (var i = 0; i < symbolIndexes.Count; i++)
				temp[symbolIndexes[i]].Add(i);
			return temp;
		}

		/// <inheritdoc cref="IToString.ToString(string?)" />
		internal static IResult StripToString(this IStrip strip, string format)
		{
			var count = strip.GetLength();
			var sb = new StringBuilder();

			for (var i = 0UL; i < count; i++)
			{
				if (i > 0)
					sb.Append(' ');

				sb.Append(strip.GetSymbol(i));

				var weight = strip.GetWeight(i);

				if (weight != 1)
					sb.Append($":{weight}");

				if (sb.Length > 3000 && count - i > 50)
				{
					sb.Append(" ... too long");
					break;
				}
			}

			return sb.ToString().ToSuccess();
		}
	}
}