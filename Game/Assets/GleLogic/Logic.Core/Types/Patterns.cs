using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

namespace Logic.Core.Types
{
	/// <summary>
	/// A collection of line patterns plus some data structures to improve evaluation performance.
	/// </summary>
	public sealed class Patterns : IToString
	{
		private readonly SymbolWindowStructure symbolWindowStructure;
		private readonly IReadOnlyList<Pattern> sourcePatterns;

		/// <summary>
		/// A mask per cluster indicating what cell positions are used by the entire pattern collection.
		/// </summary>
		public IReadOnlyList<ReadOnlyMask> ClusterSummary { get; }

		/// <summary>
		/// The collection of line patterns. This can be null if the instance contains cluster patterns.
		/// </summary>
		public IReadOnlyList<LinePattern> LinePatterns { get; }

		/// <summary>
		/// The collection of cluster patterns. This can be null if the instance contains line patterns.
		/// </summary>
		public IReadOnlyList<ClusterPattern> ClusterPatterns { get; }

		public Patterns(IReadOnlyList<Pattern> sourcePatterns, SymbolWindowStructure symbolWindowStructure)
		{
			this.sourcePatterns = sourcePatterns;
			this.symbolWindowStructure = symbolWindowStructure;

			var canBeLinePatterns = true;
			foreach (var pattern in sourcePatterns)
			{
				foreach (var cluster in pattern.Clusters)
				{
					if (cluster.Cells.Count == 1)
						continue;

					canBeLinePatterns = false;
					break;
				}
			}

			if (canBeLinePatterns)
			{
				CreateLinePatterns(out var summary, out var patterns);
				ClusterSummary = summary;
				LinePatterns = patterns;
			}
			else
			{
				CreateClusterPatterns(out var summary, out var patterns);
				ClusterSummary = summary;
				ClusterPatterns = patterns;
			}
		}

		private void CreateLinePatterns(out IReadOnlyList<ReadOnlyMask> summary, out IReadOnlyList<LinePattern> patterns)
		{
			var linePatterns = new List<LinePattern>();
			var maxClusterCount = sourcePatterns.Max(p => p.Clusters.Count);
			var clusterSummaryMasks = new Mask[maxClusterCount];

			for (var i = 0; i < clusterSummaryMasks.Length; i++)
				clusterSummaryMasks[i] = new Mask(symbolWindowStructure.Cells.Count);

			foreach (var pattern in sourcePatterns)
			{
				var patternSummaryMask = new Mask(symbolWindowStructure.Cells.Count);
				var positions = new int[pattern.Clusters.Count];

				for (var i = 0; i < pattern.Clusters.Count; i++)
				{
					if (pattern.Clusters[i].Cells.Count != 1)
						throw new Exception($"{pattern.Name} has more than one cell in a cluster.  Line patterns require on one cell per cluster.");

					foreach (var cell in pattern.Clusters[i].Cells)
					{
						var position = symbolWindowStructure.GetCellIndex(cell);
						positions[i] = position;
						clusterSummaryMasks[i][position] = true;
						patternSummaryMask[position] = true;
					}
				}

				linePatterns.Add(new LinePattern(patternSummaryMask.Lock(), positions));
			}

			summary = clusterSummaryMasks.ToReadOnlyMasks(symbolWindowStructure.Cells.Count);
			patterns = linePatterns;
		}

		private void CreateClusterPatterns(out IReadOnlyList<ReadOnlyMask> summary, out IReadOnlyList<ClusterPattern> patterns)
		{
			var indexes = new Dictionary<Cell, int>();

			for (var i = 0; i < symbolWindowStructure.Cells.Count; i++)
				indexes.Add(symbolWindowStructure.Cells[i], i);

			var sourcePatternsCount = sourcePatterns.Count;
			var clusterPatterns = new ClusterPattern[sourcePatternsCount];
			var summaryPatternMasks = new ReadOnlyMask[sourcePatternsCount];

			for (var i = 0; i < sourcePatternsCount; i++)
			{
				var pattern = sourcePatterns[i];
				var patternClustersCount = pattern.Clusters.Count;
				var positionsMask = new ReadOnlyMask[patternClustersCount];
				var positions = new int[patternClustersCount][];
				var patternSummaryMask = new Mask(symbolWindowStructure.Cells.Count);

				for (var j = 0; j < patternClustersCount; j++)
				{
					var cluster = pattern.Clusters[j];
					var clusterCellsCount = cluster.Cells.Count;
					var clusterIndexes = new int[clusterCellsCount];

					for (var k = 0; k < clusterCellsCount; k++)
					{
						var position = indexes[cluster.Cells[k]];
						patternSummaryMask[position] = true;
						clusterIndexes[k] = position;
					}

					positions[j] = clusterIndexes;
					positionsMask[j] = ReadOnlyMask.CreateFromIndexes(symbolWindowStructure.Cells.Count, clusterIndexes);
				}

				clusterPatterns[i] = new ClusterPattern(positionsMask, positions);
				summaryPatternMasks[i] = patternSummaryMask.Lock();
			}

			summary = summaryPatternMasks;
			patterns = clusterPatterns;
		}

		/// <summary>
		/// Get the source pattern that was used to generate the <permission cref="LinePattern"/> at <param name="index"/>.
		/// </summary>
		public Pattern GetSourcePattern(int index) => sourcePatterns[index];

		/// <summary>
		/// Get the source patterns that were used to generate the <permission cref="LinePattern"/>.
		/// </summary>
		// ReSharper disable once UnusedMember.Global - Used in presentation
		public IReadOnlyList<Pattern> GetSourcePatterns() => sourcePatterns;

		/// <summary>
		/// Get the <see cref="SymbolWindowStructure"/> that was used to generate <see cref="Patterns"/>.
		/// </summary>
		public SymbolWindowStructure GetSourceSymbolWindowStructure() => symbolWindowStructure;

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (format == "ML")
			{
				var arr = GetSourceSymbolWindowStructure().GetStructureArrays();
				var lines = new List<string>();

				if (ClusterPatterns != null)
				{
					foreach (var pattern in ClusterPatterns)
					{
						if (lines.Count > 0)
							lines.Add("");

						lines.AddRange(arr.ToTableLines(v =>
						{
							if (v != -1)
							{
								for (var i = 0; i < pattern.PositionsMask.Length; i++)
								{
									if (pattern.PositionsMask[i][v])
										return i.ToString();
								}
							}

							return "-";
						}));
					}
				}
				else if (LinePatterns != null)
				{
					foreach (var pattern in LinePatterns)
					{
						if (lines.Count > 0)
							lines.Add("");

						lines.AddRange(arr.ToTableLines(v =>
						{
							if (v != -1)
							{
								for (var i = 0; i < pattern.Positions.Length; i++)
								{
									if (v == pattern.Positions[i])
										return i.ToString();
								}
							}

							return "-";
						}));
					}
				}

				return lines.Join().ToSuccess();
			}

			return new NotSupported();
		}
	}
}