using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.WinCheck;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Logic
{
	/// <summary>
	/// This class holds all the various ways the replacement data could be specified and provides the <see cref="GetSymbolIndex"/> function to get a replacement symbol from a particular stop index.
	/// The <see cref="GetSymbolIndex"/> will return -1 if the stop index is not found.
	/// </summary>
	public sealed class Replacements
	{
		private readonly IReadOnlyList<int>[] symbolPositions;
		private readonly Func<int, IReadOnlyList<int>> getSymbolPositions;
		private readonly Func<ulong, int> getSymbolIndex;

		public ISymbolListStrip SourceStrip { get; }

		public static readonly Replacements Empty = new Replacements();

		public Replacements(ISymbolListStrip sourceStrip, IReadOnlyList<Replacements> replacements)
		{
			SourceStrip = sourceStrip;
			symbolPositions = new IReadOnlyList<int>[sourceStrip.GetSymbolList().Count];
			getSymbolIndex = SymbolIndex;
			getSymbolPositions = SymbolPositions;
			return;

			int SymbolIndex(ulong stopIndex)
			{
				// The list of replacements is ordered. This function returns the first non-negative stop replacement.
				foreach (var replacementObj in replacements)
				{
					var result = replacementObj.GetSymbolIndex(stopIndex);

					if (result < 0)
						continue;

					return result;
				}

				return -1;
			}
		}

		public Replacements(ISymbolListStrip strip, IReadOnlyList<int> stopReplacements)
		{
			SourceStrip = strip;
			symbolPositions = new IReadOnlyList<int>[strip.GetSymbolList().Count];
			getSymbolIndex = stopIndex => stopReplacements[(int)stopIndex];
			getSymbolPositions = SymbolPositions;
		}

		public Replacements(ISymbolListStrip strip, IReadOnlyList<ulong> positionIndexes, IReadOnlyList<int> symbolIndexes)
		{
			SourceStrip = strip;
			symbolPositions = new IReadOnlyList<int>[strip.GetSymbolList().Count];
			getSymbolIndex = SymbolIndex;
			getSymbolPositions = SymbolPositions;
			return;

			int SymbolIndex(ulong stopIndex)
			{
				for (var i = 0; i < positionIndexes.Count; i++)
				{
					if (positionIndexes[i] == stopIndex)
						return symbolIndexes.Count == 1 ? symbolIndexes[0] : symbolIndexes[i];
				}

				return -1;
			}
		}

		public Replacements(ISymbolListStrip strip, IReadOnlyList<int> positionIndexesAsInt, IReadOnlyList<int> symbolIndexes)
		{
			SourceStrip = strip;
			symbolPositions = new IReadOnlyList<int>[strip.GetSymbolList().Count];
			getSymbolIndex = SymbolIndex;
			getSymbolPositions = SymbolPositions;
			return;

			int SymbolIndex(ulong stopIndex)
			{
				for (var i = 0; i < positionIndexesAsInt.Count; i++)
				{
					if (positionIndexesAsInt[i] == (int)stopIndex)
						return symbolIndexes.Count == 1 ? symbolIndexes[0] : symbolIndexes[i];
					// if (positionIndexes[i] > stopIndex) // Assume ordered list of stops because of the GetSymbolPositions helper?
					// 	break;
				}

				return -1;
			}
		}

		public Replacements(ISymbolListStrip sourceStrip, IReadOnlyDictionary<ulong, int> dictionary)
		{
			SourceStrip = sourceStrip;
			symbolPositions = new IReadOnlyList<int>[sourceStrip.GetSymbolList().Count];
			// ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault - Cannot use GetValueOrDefault due to Unity support
			getSymbolIndex = stopIndex => dictionary.TryGetValue(stopIndex, out var value) ? value : -1;
			getSymbolPositions = SymbolPositions;
		}

		public Replacements(ISymbolListStrip sourceStrip, IReadOnlyDictionary<int, int> dictionary)
		{
			SourceStrip = sourceStrip;
			symbolPositions = new IReadOnlyList<int>[sourceStrip.GetSymbolList().Count];
			// ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault - Cannot use GetValueOrDefault due to Unity support
			getSymbolIndex = stopIndex => dictionary.TryGetValue(sourceStrip.GetSymbolIndex(stopIndex), out var value) ? value : -1;
			getSymbolPositions = SymbolPositions;
		}

		public Replacements(ISymbolListStrip sourceStrip, int originalSymbolIndex, int newSymbolIndex)
		{
			SourceStrip = sourceStrip;
			symbolPositions = new IReadOnlyList<int>[sourceStrip.GetSymbolList().Count];
			getSymbolIndex = stopIndex =>
			{
				var symbolIndex = sourceStrip.GetSymbolIndex(stopIndex);
				return symbolIndex == originalSymbolIndex ? newSymbolIndex : -1;
			};
			getSymbolPositions = symbolIndex =>
			{
				if (symbolIndex == newSymbolIndex)
					return sourceStrip.GetSymbolPositions(originalSymbolIndex).Concat(sourceStrip.GetSymbolPositions(newSymbolIndex)).OrderBy(i => i).ToArray();

				return symbolIndex == originalSymbolIndex ? Array.Empty<int>() : sourceStrip.GetSymbolPositions(symbolIndex);
			};
		}

		private Replacements()
		{
		}

		private IReadOnlyList<int> SymbolPositions(int symbolIndex)
		{
			var list = new List<int>();
			for (var i = 0; i < (int)SourceStrip.GetLength(); i++)
			{
				if (GetFinalSymbolIndex((ulong)i) == symbolIndex)
					list.Add(i);
			}

			return list;
		}

		/// <summary>
		/// Get a replacement symbol from a particular stop index.
		/// </summary>
		/// <param name="stopIndex"></param>
		/// <returns>The symbol index to use as a replacement.  -1 if the stop index is not found.</returns>
		public int GetSymbolIndex(ulong stopIndex) => getSymbolIndex(stopIndex);

		public int GetFinalSymbolIndex(ulong stopIndex)
		{
			var symbolIndex = getSymbolIndex(stopIndex);

			return symbolIndex < 0 ? SourceStrip.GetSymbolIndex(stopIndex) : symbolIndex;
		}

		public IReadOnlyList<int> GetSymbolPositions(int symbolIndex) => symbolPositions[symbolIndex] ?? (symbolPositions[symbolIndex] = getSymbolPositions(symbolIndex));

		public ISymbolListStrip ToStrip()
		{
			return new SymbolReplacementStrip(SourceStrip, (si, s) => GetFinalSymbolIndex(si), GetSymbolPositions);
		}
	}

	public sealed class IndexList
	{
		/// <summary>
		/// The strip that <see cref="PositionIndexes"/> are referring to.
		/// </summary>
		public ISymbolListStrip Strip { get; }

		/// <summary>
		/// A collection of target strip indexes.
		/// </summary>
		public IReadOnlyList<int> PositionIndexes { get; }

		public IndexList(ISymbolListStrip sourceList, IReadOnlyList<int> indexes)
		{
			Strip = sourceList;
			PositionIndexes = indexes;
		}

		public void Deconstruct(out ISymbolListStrip sourceList, out IReadOnlyList<int> indexes)
		{
			sourceList = Strip;
			indexes = PositionIndexes;
		}
	}

	public interface IReplacementBuilder
	{
		IReadOnlyList<ISymbolListStrip> OriginalStrips { get; }
		IReadOnlyList<ISymbolListStrip> FinalStrips { get; }
		IReadOnlyList<Replacements> Replacements { get; }
		IReplacementBuilder AddReplacement(int stripIndex, Replacements replacements);
		void Finalise();
	}

	public partial class ReplacementHelper
	{
		private sealed class ReplacementBuilder : IReplacementBuilder
		{
			private readonly string metricId;
			private List<Replacements>[] replacementsUlong;
			private IReadOnlyList<ISymbolListStrip> finalStrips;
			private IReadOnlyList<Replacements> finalReplacements;

			public IReadOnlyList<ISymbolListStrip> OriginalStrips { get; }

			public IReadOnlyList<ISymbolListStrip> FinalStrips => finalStrips ?? throw new Exception("Builder not finalised.  Call Finalise() before accessing FinalStrips.");
			public IReadOnlyList<Replacements> Replacements => finalReplacements ?? throw new Exception("Builder not finalised.  Call Finalise() before accessing Replacements.");

			public ReplacementBuilder(IReadOnlyList<ISymbolListStrip> strips, string metricId = null)
			{
				this.metricId = metricId;
				OriginalStrips = strips;
			}

			public IReplacementBuilder AddReplacement(int stripIndex, Replacements replacements)
			{
				// If we have initialized the strips we can no longer add replacement entries.
				if (finalStrips != null)
					throw new Exception();

				if (replacementsUlong == null)
					replacementsUlong = new List<Replacements>[OriginalStrips.Count];

				if (replacementsUlong[stripIndex] == null)
					replacementsUlong[stripIndex] = new List<Replacements>();

				replacementsUlong[stripIndex].Add(replacements);
				return this;
			}

			public void Finalise() => ProcessReplacements();

			private void ProcessReplacements()
			{
				var strips = new List<ISymbolListStrip>();
				var replacements = new List<Replacements>();

				for (var stripIndex = 0; stripIndex < OriginalStrips.Count; stripIndex++)
				{
					var replacementList = replacementsUlong?[stripIndex];
					if (replacementList == null)
					{
						strips.Add(OriginalStrips[stripIndex]);
						replacements.Add(Logic.Replacements.Empty);
						continue;
					}

					var replacement = replacementList.Count == 1 ? replacementList[0] : new Replacements(OriginalStrips[stripIndex], replacementList);
					var strip = replacement.ToStrip();

					if (metricId != null)
						strip = new MetricTrackerStrip(strip, $"{metricId}_{stripIndex}");

					strips.Add(strip);
					replacements.Add(replacement);
				}

				finalStrips = strips;
				finalReplacements = replacements;
			}
		}
	}
}