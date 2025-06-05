using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.WinCheck;

// ReSharper disable MemberCanBePrivate.Global

namespace Logic
{
	public static partial class ReplacementHelper
	{
		/// <summary>
		/// Create a strip builder that will collate and apply the replacements.
		/// </summary>
		/// <param name="strips">The list of strips to apply replacements to.</param>
		/// <returns>The builder that will collate and apply the replacements.</returns>
		public static IReplacementBuilder CreateBuilder(IReadOnlyList<ISymbolListStrip> strips) => new ReplacementBuilder(strips);

		/// <summary>
		/// Create a strip builder that will collate and apply the replacements. This version has metric tracking to get an idea on what type of usage are occuring on the final strips.
		/// </summary>
		/// <param name="strips">The list of strips to apply replacements to.</param>
		/// <param name="metricId">The id to use for metric tracking this strip over multiple creations.</param>
		/// <returns>The builder that will collate and apply the replacements.</returns>
		public static IReplacementBuilder CreateBuilderWithMetrics(IReadOnlyList<ISymbolListStrip> strips, string metricId) => new ReplacementBuilder(strips, metricId);

		/// <summary>
		/// Create an object that stores a requested symbols index, the stop indexes within a strip for that symbol and the original strip.
		/// </summary>
		public static IndexList GetSymbolIndexesPerStrip(this ISymbolListStrip strip, int symbolIndex)
		{
			return new IndexList(strip, strip.GetSymbolPositions(symbolIndex));
		}

		/// <summary>
		/// Create an object that stores a requested symbols index, the stop indexes within a strip for that symbol and the original strip.
		/// </summary>
		public static IndexList GetSymbolIndexesPerStrip(this ISymbolListStrip strip, IReadOnlyList<int> symbolIndexes)
		{
			var positions = new List<int>();
			for (var i = 0UL; i < strip.GetLength(); i++)
			{
				if (symbolIndexes.Contains(strip.GetSymbolIndex(i)))
					positions.Add((int)i);
			}

			return new IndexList(strip, positions);
		}

		/// <summary>
		/// Create an array of objects that store a requested symbols index, the stop indexes within a strip for that symbol and the original strip for a collection of strips.
		/// </summary>
		public static IReadOnlyList<IndexList> GetSymbolIndexesPerStrip(this IReadOnlyList<ISymbolListStrip> strips, int symbolIndex)
		{
			var symbolIndexesPerStrip = new List<IndexList>();

			for (var stripIndex = 0; stripIndex < strips.Count; stripIndex++)
				symbolIndexesPerStrip.Add(strips[stripIndex].GetSymbolIndexesPerStrip(symbolIndex));

			return symbolIndexesPerStrip;
		}

		/// <summary>
		/// Create an array of objects that store a requested symbols index, the stop indexes within a strip for that symbol and the original strip for a collection of strips.
		/// </summary>
		public static IReadOnlyList<IndexList> GetSymbolIndexesPerStrip(this IReadOnlyList<ISymbolListStrip> strips, IReadOnlyList<int> symbolIndexes)
		{
			var symbolIndexesPerStrip = new List<IndexList>();

			for (var stripIndex = 0; stripIndex < strips.Count; stripIndex++)
				symbolIndexesPerStrip.Add(strips[stripIndex].GetSymbolIndexesPerStrip(symbolIndexes));

			return symbolIndexesPerStrip;
		}

		/// <summary>
		/// Create replacement strips that use an array of stops, instead of a dictionary of stop index, symbol index pairs.  Can be more efficient in scenarios where strip lengths are not super long.
		/// Have each reel ask for its own set of replacements.
		/// </summary>
		/// <param name="strips">The list of strips to apply replacements to.</param>
		/// <param name="symbolIndex">The symbol index to replace in all <see cref="strips"/></param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <returns>The strips with replacements applied.</returns>
		public static IReadOnlyList<ISymbolListStrip> CreateDirectIndependentUsingArray(this IReadOnlyList<ISymbolListStrip> strips, int symbolIndex, Func<int, int, IReadOnlyList<int>> getReplacements)
		{
			return strips.ProcessIndependent(symbolIndex, getReplacements, (stripIndex, indexList, replacements) => indexList.PositionIndexes.Count != 0 ? indexList.CreateStripWithReplacement(replacements) : indexList.Strip);
		}

		/// <summary>
		/// Create replacement strips that use a dictionary of stop index, symbol index pairs, instead of an array of stops.   Can be more efficient in scenarios where strip lengths are super long.
		/// Have each reel ask for its own set of replacements.
		/// </summary>
		/// <param name="strips">The list of strips to apply replacements to.</param>
		/// <param name="symbolIndex">The symbol index to replace in all <see cref="strips"/></param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <returns>The strips with replacements applied.</returns>
		public static IReadOnlyList<ISymbolListStrip> CreateDirectIndependentUsingDictionary(this IReadOnlyList<ISymbolListStrip> strips, int symbolIndex, Func<int, int, IReadOnlyList<int>> getReplacements)
		{
			return strips.ProcessIndependent(symbolIndex, getReplacements, (stripIndex, indexList, replacements) =>
			{
				var (sourceList, indexes) = indexList;
				if (indexList.PositionIndexes.Count == 0)
					return sourceList;

				var replacementDictionary = new Dictionary<ulong, int>();
				for (var i = 0; i < replacements.Count; i++)
					replacementDictionary.Add((ulong)indexes[i], replacements[i]);

				return CreateStripWithReplacement(sourceList, replacementDictionary);
			});
		}

		/// <summary>
		/// Create replacement strips that use an array of stops, instead of a dictionary of stop index, symbol index pairs.  Can be more efficient in scenarios where strip lengths are not super long.
		/// Have a single set of symbols replacements for the target symbol across all strips.
		/// </summary>
		/// <param name="strips">The list of strips to apply replacements to.</param>
		/// <param name="symbolIndex">The symbol index to replace in all <see cref="strips"/></param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <returns>The strips with replacements applied.</returns>
		public static IReadOnlyList<ISymbolListStrip> CreateDirectDependentUsingArray(this IReadOnlyList<ISymbolListStrip> strips, int symbolIndex, Func<int, IReadOnlyList<int>> getReplacements)
		{
			return strips.ProcessDependent(symbolIndex, getReplacements, (stripIndex, indexList, replacements) => indexList.PositionIndexes.Count != 0 ? indexList.CreateStripWithReplacement(replacements) : indexList.Strip);
		}

		/// <summary>
		/// Create replacement strips that use a dictionary of stop index, symbol index pairs, instead of an array of stops.   Can be more efficient in scenarios where strip lengths are super long.
		/// Have a single set of symbols replacements for the target symbol across all strips.
		/// </summary>
		/// <param name="strips">The list of strips to apply replacements to.</param>
		/// <param name="symbolIndex">The symbol index to replace in all <see cref="strips"/></param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <returns>The strips with replacements applied.</returns>
		public static IReadOnlyList<ISymbolListStrip> CreateDirectDependentUsingDictionary(this IReadOnlyList<ISymbolListStrip> strips, int symbolIndex, Func<int, IReadOnlyList<int>> getReplacements)
		{
			return strips.ProcessDependent(symbolIndex, getReplacements, (stripIndex, indexList, replacements) =>
			{
				var (sourceList, indexes) = indexList;

				if (indexList.PositionIndexes.Count == 0)
					return sourceList;

				var replacementDictionary = new Dictionary<ulong, int>();
				for (var i = 0; i < replacements.Count; i++)
					replacementDictionary.Add((ulong)indexes[i], replacements[i]);

				return CreateStripWithReplacement(sourceList, replacementDictionary);
			});
		}

		/// <summary>
		/// Add a collection of strips with replacement data added to the replacement builder.
		/// Have each reel choose ask for its own set of replacements.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="symbolIndex">The symbol index to replace in all strips</param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes. The function is of the form IReadOnlyList'int' GetReplacements(int stripIndex, int symbolIndexCountRequested)</param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddIndependentStripReplacements(this IReplacementBuilder builder, int symbolIndex, Func<int, int, IReadOnlyList<int>> getReplacements)
		{
			builder.OriginalStrips.ProcessIndependent(symbolIndex, getReplacements, (stripIndex, indexList, replacements) => replacements.Count != 0 ? builder.AddReplacement(stripIndex, symbolIndex, replacements) : builder);
			return builder;
		}

		/// <summary>
		/// Add a collection of strips with replacement data added to the replacement builder.
		/// Have a single set of symbols replacements for the target symbol across all strips.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="symbolIndex">The symbol index to replace in all strips</param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddDependentStripReplacements(this IReplacementBuilder builder, int symbolIndex, Func<int, IReadOnlyList<int>> getReplacements)
		{
			builder.OriginalStrips.ProcessDependent(symbolIndex, getReplacements, (stripIndex, indexList, replacements) => replacements.Count != 0 ? builder.AddReplacement(stripIndex, symbolIndex, replacements) : builder);
			return builder;
		}

		/// <summary>
		/// Add a collection of strips with replacement data added to the replacement builder.
		/// Have a single set of symbols replacements for the target symbol across all strips.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="symbolIndexes">The symbol indexes to replace in all strips</param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddDependentStripReplacements(this IReplacementBuilder builder, IReadOnlyList<int> symbolIndexes, Func<int, IReadOnlyList<int>> getReplacements)
		{
			builder.OriginalStrips.ProcessDependent(symbolIndexes, getReplacements, (stripIndex, indexList, replacements) => replacements.Count != 0 ? builder.AddReplacement(stripIndex, indexList.PositionIndexes, replacements) : builder);
			return builder;
		}

		/// <summary>
		/// A helper function to allow processing of a set of strips for replacements.  The function
		/// IReadOnlyList[int] getReplacements(int replacementCount)
		/// T produceResult(int stripIndex, IndexList indexList, IReadOnlyList[int] replacementsForStrip)
		/// </summary>
		/// <param name="strips">The list of strips to process.</param>
		/// <param name="symbolIndex">The symbol index to replace in all <see cref="strips"/></param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <param name="produceResult">A function to take the chosen replacement symbols for  a particular strip and produce a result.</param>
		/// <typeparam name="T">The type of object to produce.</typeparam>
		public static IReadOnlyList<T> ProcessIndependent<T>(this IReadOnlyList<ISymbolListStrip> strips, int symbolIndex, Func<int, int, IReadOnlyList<int>> getReplacements, Func<int, IndexList, IReadOnlyList<int>, T> produceResult)
		{
			var indexesPerStrip = GetSymbolIndexesPerStrip(strips, symbolIndex);

			return ProcessIndependent(indexesPerStrip, getReplacements, produceResult);
		}

		/// <summary>
		/// A helper function to allow processing of a set of strips for replacements.  The function
		/// IReadOnlyList[int] getReplacements(int replacementCount)
		/// T produceResult(int stripIndex, IndexList indexList, IReadOnlyList[int] replacementsForStrip)
		/// </summary>
		/// <param name="strips">The list of strips to process.</param>
		/// <param name="symbolIndexes">The symbol indexes to replace in all <see cref="strips"/></param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <param name="produceResult">A function to take the chosen replacement symbols for  a particular strip and produce a result.</param>
		/// <typeparam name="T">The type of object to produce.</typeparam>
		public static IReadOnlyList<T> ProcessIndependent<T>(this IReadOnlyList<ISymbolListStrip> strips, IReadOnlyList<int> symbolIndexes, Func<int, int, IReadOnlyList<int>> getReplacements, Func<int, IndexList, IReadOnlyList<int>, T> produceResult)
		{
			var indexesPerStrip = GetSymbolIndexesPerStrip(strips, symbolIndexes);

			return ProcessIndependent(indexesPerStrip, getReplacements, produceResult);
		}

		/// <summary>
		/// A helper function to allow processing of a set of strips for replacements.  The function
		/// IReadOnlyList[int] getReplacements(int replacementCount)
		/// T produceResult(int stripIndex, IndexList indexList, IReadOnlyList[int] replacementsForStrip)
		/// </summary>
		/// <param name="indexLists">The index lists to work with.</param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <param name="produceResult">A function to take the chosen replacement symbols for  a particular strip and produce a result.</param>
		/// <typeparam name="T">The type of object to produce.</typeparam>
		public static IReadOnlyList<T> ProcessIndependent<T>(this IReadOnlyList<IndexList> indexLists, Func<int, int, IReadOnlyList<int>> getReplacements, Func<int, IndexList, IReadOnlyList<int>, T> produceResult)
		{
			var result = new T[indexLists.Count];
			var indexesPerStrip = indexLists;

			for (var stripIndex = 0; stripIndex < indexLists.Count; stripIndex++)
			{
				var stripSymbolIndexes = indexesPerStrip[stripIndex].PositionIndexes;

				if (stripSymbolIndexes.Count == 0)
				{
					result[stripIndex] = produceResult(stripIndex, indexesPerStrip[stripIndex], Array.Empty<int>());
					continue;
				}

				var replacements = getReplacements(stripIndex, stripSymbolIndexes.Count);

				result[stripIndex] = produceResult(stripIndex, indexesPerStrip[stripIndex], replacements);
			}

			return result;
		}

		/// <summary>
		/// A helper function to allow processing of a set of strips for replacements.  The function
		/// IReadOnlyList[int] getReplacements(int replacementCount)
		/// T produceResult(int stripIndex, IndexList indexList, IReadOnlyList[int] replacementsForStrip)
		/// </summary>
		/// <param name="strips">The list of strips to process.</param>
		/// <param name="symbolIndex">The symbol index to replace in all <see cref="strips"/></param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <param name="produceResult">A function to take the chosen replacement symbols for  a particular strip and produce a result.</param>
		/// <typeparam name="T">The type of object to produce.</typeparam>
		public static IReadOnlyList<T> ProcessDependent<T>(this IReadOnlyList<ISymbolListStrip> strips, int symbolIndex, Func<int, IReadOnlyList<int>> getReplacements, Func<int, IndexList, IReadOnlyList<int>, T> produceResult)
		{
			var indexesPerStrip = GetSymbolIndexesPerStrip(strips, symbolIndex);

			return ProcessDependent(indexesPerStrip, getReplacements, produceResult);
		}

		/// <summary>
		/// A helper function to allow processing of a set of strips for replacements.  The function
		/// IReadOnlyList[int] getReplacements(int replacementCount)
		/// T produceResult(int stripIndex, IndexList indexList, IReadOnlyList[int] replacementsForStrip)
		/// </summary>
		/// <param name="strips">The list of strips to process.</param>
		/// <param name="symbolIndexes">The symbol indexes to replace in all <see cref="strips"/></param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <param name="produceResult">A function to take the chosen replacement symbols for  a particular strip and produce a result.</param>
		/// <typeparam name="T">The type of object to produce.</typeparam>
		public static IReadOnlyList<T> ProcessDependent<T>(this IReadOnlyList<ISymbolListStrip> strips, IReadOnlyList<int> symbolIndexes, Func<int, IReadOnlyList<int>> getReplacements, Func<int, IndexList, IReadOnlyList<int>, T> produceResult)
		{
			var indexesPerStrip = GetSymbolIndexesPerStrip(strips, symbolIndexes);

			return ProcessDependent(indexesPerStrip, getReplacements, produceResult);
		}

		/// <summary>
		/// A helper function to allow processing of a set of strips for replacements.  The function
		/// IReadOnlyList[int] getReplacements(int replacementCount)
		/// T produceResult(int stripIndex, IndexList indexList, IReadOnlyList[int] replacementsForStrip)
		/// </summary>
		/// <param name="indexLists">The index lists to work with.</param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <param name="produceResult">A function to take the chosen replacement symbols for  a particular strip and produce a result.</param>
		/// <typeparam name="T">The type of object to produce.</typeparam>
		public static IReadOnlyList<T> ProcessDependent<T>(this IReadOnlyList<IndexList> indexLists, Func<int, IReadOnlyList<int>> getReplacements, Func<int, IndexList, IReadOnlyList<int>, T> produceResult)
		{
			var result = new T[indexLists.Count];
			var totalAvailableIndexes = indexLists.Sum(ri => ri.PositionIndexes.Count);
			var replacementSymbolIndexes = getReplacements(totalAvailableIndexes);
			var replacementIndex = 0;

			for (var stripIndex = 0; stripIndex < indexLists.Count; stripIndex++)
			{
				var stripSymbolIndexes = indexLists[stripIndex].PositionIndexes;

				if (stripSymbolIndexes.Count == 0)
				{
					result[stripIndex] = produceResult(stripIndex, indexLists[stripIndex], Array.Empty<int>());
					continue;
				}

				var replacements = new int[stripSymbolIndexes.Count];

				for (var i = 0; i < stripSymbolIndexes.Count; i++)
					replacements[i] = replacementSymbolIndexes[replacementIndex + i];

				result[stripIndex] = produceResult(stripIndex, indexLists[stripIndex], replacements);
				replacementIndex += stripSymbolIndexes.Count;
			}

			return result;
		}

		/// <summary>
		/// A helper function to allow processing of a set of strips for replacements.  The function
		/// IReadOnlyList[int] getReplacements(int replacementCount)
		/// void processResult(int stripIndex, IndexList indexList, IReadOnlyList[int] replacementsForStrip)
		/// </summary>
		/// <param name="indexLists">The index lists to work with.</param>
		/// <param name="getReplacements">A function to produce a set of replacement symbol indexes.</param>
		/// <param name="processResult">An action to take the chosen replacement symbols for a particular strip and process for a result.</param>
		public static void ProcessDependent(this IReadOnlyList<IndexList> indexLists, Func<int, IReadOnlyList<int>> getReplacements, Action<int, IndexList, IReadOnlyList<int>> processResult)
		{
			var totalAvailableIndexes = indexLists.Sum(ri => ri.PositionIndexes.Count);
			var replacementSymbolIndexes = getReplacements(totalAvailableIndexes);
			var replacementIndex = 0;

			for (var stripIndex = 0; stripIndex < indexLists.Count; stripIndex++)
			{
				var stripSymbolIndexes = indexLists[stripIndex].PositionIndexes;

				if (stripSymbolIndexes.Count == 0)
				{
					processResult(stripIndex, indexLists[stripIndex], Array.Empty<int>());
					continue;
				}

				var replacements = new int[stripSymbolIndexes.Count];

				for (var i = 0; i < stripSymbolIndexes.Count; i++)
					replacements[i] = replacementSymbolIndexes[replacementIndex + i];

				processResult(stripIndex, indexLists[stripIndex], replacements);
				replacementIndex += stripSymbolIndexes.Count;
			}
		}

		/// <summary>
		/// Create a new strip that contains the original strip and when the specified symbol indexes are found returns an alternate symbol index.
		/// </summary>
		/// <param name="indexList"></param>
		/// <param name="symbolIndexes"></param>
		/// <returns>Returns the constructed <see cref="WeightedSymbolListStrip"/> as an <see cref="ISymbolListStrip"/>.</returns>
		public static ISymbolListStrip CreateStripWithReplacement(this IndexList indexList, IReadOnlyList<int> symbolIndexes)
		{
			var (sourceList, indexes) = indexList;
			return indexes.Count > 0 ? sourceList.CreateStripWithReplacement(indexes, symbolIndexes) : sourceList;
		}

		/// <summary>
		/// Create a new strip that contains the original strip and when the specified symbol indexes are found returns an alternate symbol index.
		/// </summary>
		/// <param name="originalStrip">The original strip.</param>
		/// <param name="targetIndexes">The strip indexes for fo replacement.</param>
		/// <param name="symbolIndexes">The new symbol indexes to use.</param>
		/// <returns>Returns the constructed <see cref="WeightedSymbolListStrip"/> as an <see cref="ISymbolListStrip"/>.</returns>
		public static ISymbolListStrip CreateStripWithReplacement(this ISymbolListStrip originalStrip, IReadOnlyList<int> targetIndexes, IReadOnlyList<int> symbolIndexes)
		{
			var stops = new int[originalStrip.GetLength()];

			for (var i = 0; i < stops.Length; i++)
				stops[i] = -1;

			for (var i = 0; i < targetIndexes.Count; i++)
				stops[targetIndexes[i]] = symbolIndexes.Count == 1 ? symbolIndexes[0] : symbolIndexes[i];

			return new SymbolReplacementStrip(originalStrip, GetSymbolIndex, si => GetLocalSymbolPositions(si, originalStrip, GetSymbolIndex));

			int GetSymbolIndex(ulong si, ISymbolListStrip og)
			{
				return stops[si] >= 0 ? stops[si] : og.GetSymbolIndex(si);
			}
		}

		/// <summary>
		/// Create a new strip that contains the original strip and when the specified symbol indexes are found returns an alternate symbol index.
		/// </summary>
		/// <param name="originalStrip">The original strip.</param>
		/// <param name="stopReplacements">The array of stops defaulting to -1 with any replacement symbol indexes set.</param>
		/// <returns>Returns the constructed <see cref="WeightedSymbolListStrip"/> as an <see cref="ISymbolListStrip"/>.</returns>
		public static ISymbolListStrip CreateStripWithReplacement(this ISymbolListStrip originalStrip, IReadOnlyList<int> stopReplacements)
		{
			return new SymbolReplacementStrip(originalStrip, GetSymbolIndex, si => GetLocalSymbolPositions(si, originalStrip, GetSymbolIndex));

			int GetSymbolIndex(ulong si, ISymbolListStrip og)
			{
				return stopReplacements[(int)si] >= 0 ? stopReplacements[(int)si] : og.GetSymbolIndex(si);
			}
		}

		/// <summary>
		/// Create a new strip that contains the original strip and when the specified symbol indexes are found returns an alternate symbol index.
		/// </summary>
		/// <param name="originalStrip">The original strip.</param>
		/// <param name="replacements">The replacements object.</param>
		/// <returns>Returns the constructed <see cref="WeightedSymbolListStrip"/> as an <see cref="ISymbolListStrip"/>.</returns>
		public static ISymbolListStrip CreateStripWithReplacement(this ISymbolListStrip originalStrip, Replacements replacements)
		{
			return new SymbolReplacementStrip(originalStrip, GetSymbolIndex, si => GetLocalSymbolPositions(si, originalStrip, GetSymbolIndex));

			int GetSymbolIndex(ulong si, ISymbolListStrip og)
			{
				if (!ReferenceEquals(replacements.SourceStrip, originalStrip))
					throw new Exception();
				return replacements.GetSymbolIndex(si);
			}
		}

		/// <summary>
		/// Create a new strip that contains the original strip and when the specified stop indexes are requested returns an alternate symbol index.
		/// </summary>
		/// <param name="originalStrip">The original strip.</param>
		/// <param name="replacedSymbols">The list of stop index/symbol index pairs. This list should not be modified after passing into this function.</param>
		/// <returns>Returns the constructed <see cref="WeightedSymbolListStrip"/> as an <see cref="ISymbolListStrip"/>.</returns>
		public static ISymbolListStrip CreateStripWithReplacement(this ISymbolListStrip originalStrip, IReadOnlyDictionary<ulong, int> replacedSymbols)
		{
			return new SymbolReplacementStrip(originalStrip, GetSymbolIndex, si => GetLocalSymbolPositions(si, originalStrip, GetSymbolIndex));

			int GetSymbolIndex(ulong si, ISymbolListStrip og)
			{
				return replacedSymbols.TryGetValue(si, out var replacedSymbolIndex)
					? replacedSymbolIndex
					: og.GetSymbolIndex(si);
			}
		}

		/// <summary>
		/// Create a new strip that contains the original strip and when the specified symbol indexes are found returns an alternate symbol index.
		/// </summary>
		/// <param name="originalStrip">The original strip.</param>
		/// <param name="replacedSymbols">The list of symbol index/symbol index pairs. This list should not be modified after passing into this function.</param>
		/// <returns>Returns the constructed <see cref="WeightedSymbolListStrip"/> as an <see cref="ISymbolListStrip"/>.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static ISymbolListStrip CreateStripWithReplacement(this ISymbolListStrip originalStrip, IReadOnlyDictionary<int, int> replacedSymbols)
		{
			return new SymbolReplacementStrip(originalStrip, GetSymbolIndex, si => GetLocalSymbolPositions(si, originalStrip, GetSymbolIndex));

			int GetSymbolIndex(ulong si, ISymbolListStrip og)
			{
				var originalSymbolIndex = og.GetSymbolIndex(si);
				// ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
				return replacedSymbols.TryGetValue(originalSymbolIndex, out var val) ? val : originalSymbolIndex;
			}
		}

		/// <summary>
		/// Create a new strip that contains the original strip and when the specified symbol indexes are found returns an alternate symbol index.
		/// </summary>
		/// <param name="originalStrip">The original strip.</param>
		/// <param name="originalSymbolIndex">The symbol to be replaced.</param>
		/// <param name="newSymbolIndex">The replacement symbol.</param>
		/// <returns>Returns the constructed <see cref="WeightedSymbolListStrip"/> as an <see cref="ISymbolListStrip"/>.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static ISymbolListStrip CreateStripWithReplacement(this ISymbolListStrip originalStrip, int originalSymbolIndex, int newSymbolIndex)
		{
			return originalSymbolIndex == newSymbolIndex ? originalStrip : new SymbolReplacementStrip(originalStrip, GetSymbolIndex, si => GetSymbolPositions(si, originalStrip, GetSymbolIndex));

			IReadOnlyList<int> GetSymbolPositions(int symbolIndex, ISymbolListStrip strip, Func<ulong, ISymbolListStrip, int> getSymbolIndex)
			{
				if (symbolIndex == newSymbolIndex)
					return strip.GetSymbolPositions(originalSymbolIndex).Concat(strip.GetSymbolPositions(newSymbolIndex)).OrderBy(i => i).ToArray();

				return symbolIndex == originalSymbolIndex ? Array.Empty<int>() : strip.GetSymbolPositions(symbolIndex);
			}

			int GetSymbolIndex(ulong si, ISymbolListStrip og)
			{
				var symbolIndex = og.GetSymbolIndex(si);
				return symbolIndex == originalSymbolIndex ? newSymbolIndex : symbolIndex;
			}
		}

		/// <summary>
		///  Add a set of replacements to the builder for a strip at a particular index.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="stripIndex">The strip index into the builders ordered strip collection.</param>
		/// <param name="symbolIndex">The symbol index to search for.</param>
		/// <param name="replacementSymbolIndexes">The replacement indexes to use on stop positions that contain <param name="symbolIndex"/></param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddReplacement(this IReplacementBuilder builder, int stripIndex, int symbolIndex, IReadOnlyList<int> replacementSymbolIndexes) =>
			builder.AddReplacement(stripIndex, new Replacements(builder.OriginalStrips[stripIndex], builder.OriginalStrips[stripIndex].GetSymbolPositions(symbolIndex), replacementSymbolIndexes));

		/// <summary>
		///  Add a set of replacements to the builder for a strip at a particular index.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="stripIndex">The strip index into the builders ordered strip collection.</param>
		/// <param name="stopIndexes">The stop indexes to replace.</param>
		/// <param name="replacementSymbolIndexes">The replacement indexes to use on <param name="stopIndexes"/></param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddReplacement(this IReplacementBuilder builder, int stripIndex, IReadOnlyList<ulong> stopIndexes, IReadOnlyList<int> replacementSymbolIndexes) =>
			builder.AddReplacement(stripIndex, new Replacements(builder.OriginalStrips[stripIndex], stopIndexes, replacementSymbolIndexes));

		/// <summary>
		///  Add a set of replacements to the builder for a strip at a particular index.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="stripIndex">The strip index into the builders ordered strip collection.</param>
		/// <param name="stopIndexes">The stop indexes to replace.</param>
		/// <param name="replacementSymbolIndexes">The replacement indexes to use on <param name="stopIndexes"/></param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddReplacement(this IReplacementBuilder builder, int stripIndex, IReadOnlyList<int> stopIndexes, IReadOnlyList<int> replacementSymbolIndexes) =>
			builder.AddReplacement(stripIndex, new Replacements(builder.OriginalStrips[stripIndex], stopIndexes, replacementSymbolIndexes));

		/// <summary>
		///  Add a set of replacements to the builder for a strip at a particular index.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="stripIndex">The strip index into the builders ordered strip collection.</param>
		/// <param name="stopReplacements">The replacement indexes to use when a symbol is requested for a particular stop index./></param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddReplacement(this IReplacementBuilder builder, int stripIndex, IReadOnlyDictionary<ulong, int> stopReplacements) =>
			builder.AddReplacement(stripIndex, new Replacements(builder.OriginalStrips[stripIndex], stopReplacements));

		/// <summary>
		///  Add a set of replacements to the builder for a strip at a particular index.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="stripIndex">The strip index into the builders ordered strip collection.</param>
		/// <param name="symbolIndexReplacements">The replacement indexes to use when a symbol index is requested./></param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddReplacement(this IReplacementBuilder builder, int stripIndex, IReadOnlyDictionary<int, int> symbolIndexReplacements) =>
			builder.AddReplacement(stripIndex, new Replacements(builder.OriginalStrips[stripIndex], symbolIndexReplacements));

		/// <summary>
		///  Add a set of replacements to the builder for a strip at a particular index.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="stripIndex">The strip index into the builders ordered strip collection.</param>
		/// <param name="originalSymbolIndex">The symbol to be replaced.</param>
		/// <param name="newSymbolIndex">The replacement symbol.</param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddReplacement(this IReplacementBuilder builder, int stripIndex, int originalSymbolIndex, int newSymbolIndex) =>
			builder.AddReplacement(stripIndex, new Replacements(builder.OriginalStrips[stripIndex], originalSymbolIndex, newSymbolIndex));

		/// <summary>
		///  Add a set of replacements to the builder for a strip at a particular index.
		/// </summary>
		/// <param name="builder">A strip builder that will collate and apply the replacements.</param>
		/// <param name="stripIndex">The strip index into the builders ordered strip collection.</param>
		/// <param name="stopSpecificReplacements">The replacement indexes to use when a symbol is requested for a specific stop index./></param>
		/// <returns>The strip builder instance for fluent chaining of function calls.</returns>
		public static IReplacementBuilder AddReplacement(this IReplacementBuilder builder, int stripIndex, IReadOnlyList<int> stopSpecificReplacements) =>
			builder.AddReplacement(stripIndex, new Replacements(builder.OriginalStrips[stripIndex], stopSpecificReplacements));

		private static IReadOnlyList<int> GetLocalSymbolPositions(int symbolIndex, ISymbolListStrip originalStrip, Func<ulong, ISymbolListStrip, int> getSymbolIndex)
		{
			var stopIndexes = new List<int>();

			for (var i = 0UL; i < originalStrip.GetLength(); i++)
			{
				if (getSymbolIndex(i, originalStrip) == symbolIndex)
					stopIndexes.Add((int)i);
			}

			return stopIndexes;
		}
	}
}