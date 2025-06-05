using System;
using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

namespace Logic
{
	/// <summary>
	/// A class to track how many times per instance the various <see cref="ISymbolListStrip"/> functions are called.
	/// </summary>
	public static class StripAccessMetrics
	{
		private static readonly Dictionary<string, StripAccessMetricsData> overallMetrics = new Dictionary<string, StripAccessMetricsData>();

		/// <summary>
		/// Create a new session with the specified <see cref="id"/> or if the session already exists increment the <see cref="StripAccessMetricsData.CreatedStrips"/> count.
		/// </summary>
		/// <param name="id">The unique session id for your particular strip. E.g. 'Main_Reel_1' or just use the context id of the decision generator plus some extra text you will recognise.</param>
		public static void CreateSession(string id)
		{
			lock (overallMetrics)
			{
				if (!overallMetrics.TryGetValue(id, out var sessionData))
				{
					sessionData = new StripAccessMetricsData();
					overallMetrics.Add(id, sessionData);
				}

				sessionData.CreatedStrips++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id">The unique session id for your particular strip. E.g. 'Main_Reel_1' or just use the context id of the decision generator plus some extra text you will recognise.</param>
		/// <param name="update">A function to update the <see cref="StripAccessMetricsData"/>.  Change the data directly, don't make a copy.</param>
		public static void UpdateMetrics(string id, Action<StripAccessMetricsData> update)
		{
			lock (overallMetrics)
			{
				update(overallMetrics[id]);
			}
		}

		/// <summary>
		/// Generate a formatted table in lines.
		/// Each row is a session id and each column corresponds to the average calls per created strip.
		/// The columns are as follows:
		/// ID : The unique session id for your particular strip. E.g. 'Main_Reel_1' or just use the context id of the decision generator plus some extra text you will recognise.
		/// Total: Total created strips.
		/// E : Average calls to Equals.
		/// TW: Average calls to GetTotalWeight.
		/// SIAWA: Average calls to ulong GetIndexAtWeight(ulong weight).
		/// SIAWB: Average calls to ulong GetIndexAtWeight(ulong weight, ICollection'ulong' indexesToSkip).
		/// L: Average calls to GetLength.
		/// W: Average calls to GetWeight.
		/// S: Average calls to GetSymbol.
		/// SC: Average calls to GetSymbolIndex.
		/// SL: Average calls to GetSymbolList.
		/// </summary>
		/// <returns>A formatted table.</returns>
		public static IReadOnlyList<string> GenerateReport()
		{
			lock (overallMetrics)
			{
				var displayData = new List<List<string>>();

				var header = new List<string>
				{
					"ID",
					"Total",
					"E",
					"TW",
					"SIAWA",
					"SIAWB",
					"L",
					"W",
					"S",
					"SI",
					"SL"
				};

				displayData.Add(header);

				foreach (var metric in overallMetrics)
				{
					var average = GetAverage(metric.Value);
					var rowData = new List<string>();
					rowData.Add(metric.Key);
					rowData.Add(metric.Value.CreatedStrips.ToString());
					rowData.Add(average.GetEqualsCount.ToString("0.00"));
					rowData.Add(average.GetTotalWeightCount.ToString("0.00"));
					rowData.Add(average.GetSymbolIndexAtWeightACount.ToString("0.00"));
					rowData.Add(average.GetSymbolIndexAtWeightBCount.ToString("0.00"));
					rowData.Add(average.GetLengthCount.ToString("0.00"));
					rowData.Add(average.GetWeightCount.ToString("0.00"));
					rowData.Add(average.GetSymbolCount.ToString("0.00"));
					rowData.Add(average.GetSymbolIndexCount.ToString("0.00"));
					rowData.Add(average.GetSymbolListCount.ToString("0.00"));
					displayData.Add(rowData);
				}

				return displayData.ToTableLines();
			}
		}

		private static StripAccessMetricsData GetAverage(StripAccessMetricsData list)
		{
			var totalCount = list.CreatedStrips;
			var result = new StripAccessMetricsData(list);

			result.GetEqualsCount /= totalCount;
			result.GetTotalWeightCount /= totalCount;
			result.GetSymbolIndexAtWeightACount /= totalCount;
			result.GetSymbolIndexAtWeightBCount /= totalCount;
			result.GetLengthCount /= totalCount;
			result.GetSymbolCount /= totalCount;
			result.GetWeightCount /= totalCount;
			result.GetSymbolIndexCount /= totalCount;
			result.GetSymbolListCount /= totalCount;

			return result;
		}
	}

	public sealed class MetricTrackerStrip : ISymbolListStrip
	{
		private readonly ISymbolListStrip actualStrip;
		private readonly string id;

		public MetricTrackerStrip(ISymbolListStrip actualStrip, string id)
		{
			this.actualStrip = actualStrip;
			this.id = id;
			StripAccessMetrics.CreateSession(id);
		}

		public bool Equals(IStrip other)
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetEqualsCount++);
			return actualStrip.Equals(other);
		}

		public ulong GetTotalWeight()
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetTotalWeightCount++);
			return actualStrip.GetTotalWeight();
		}

		public ulong GetIndexAtWeight(ulong weight)
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetSymbolIndexAtWeightACount++);
			return actualStrip.GetIndexAtWeight(weight);
		}

		public ulong GetIndexAtWeight(ulong weight, ICollection<ulong> indexesToSkip)
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetSymbolIndexAtWeightBCount++);
			return actualStrip.GetIndexAtWeight(weight, indexesToSkip);
		}

		public ulong GetLength()
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetLengthCount++);
			return actualStrip.GetLength();
		}

		public string GetSymbol(ulong stripIndex)
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetSymbolCount++);
			return actualStrip.GetSymbol(stripIndex);
		}

		public ulong GetWeight(ulong stripIndex)
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetWeightCount++);
			return actualStrip.GetWeight(stripIndex);
		}

		public int GetSymbolIndex(ulong stripIndex)
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetSymbolIndexCount++);
			return actualStrip.GetSymbolIndex(stripIndex);
		}

		public SymbolList GetSymbolList()
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetSymbolListCount++);
			return actualStrip.GetSymbolList();
		}

		public IReadOnlyList<int> GetSymbolPositions(int symbolIndex)
		{
			StripAccessMetrics.UpdateMetrics(id, d => d.GetSymbolPositionsCount++);
			return actualStrip.GetSymbolPositions(symbolIndex);
		}
	}

	/// <summary>
	/// The metrics file for tracking function calls to the <see cref="ISymbolListStrip"/> interface.
	/// </summary>
	public sealed class StripAccessMetricsData
	{
		/// <summary>
		/// Total created strips.
		/// </summary>
		public int CreatedStrips;

		/// <summary>
		/// Count of calls to Equals.
		/// </summary>
		public double GetEqualsCount;

		/// <summary>
		/// Count of calls to GetTotalWeight.
		/// </summary>
		public double GetTotalWeightCount;

		/// <summary>
		/// Count of calls to ulong GetIndexAtWeight(ulong weight).
		/// </summary>
		public double GetSymbolIndexAtWeightACount;

		/// <summary>
		/// Count of calls to ulong GetIndexAtWeight(ulong weight, ICollection'ulong' indexesToSkip).
		/// </summary>
		public double GetSymbolIndexAtWeightBCount;

		/// <summary>
		/// Count of calls to GetLength.
		/// </summary>
		public double GetLengthCount;

		/// <summary>
		/// Count of calls to GetWeight.
		/// </summary>
		public double GetWeightCount;

		/// <summary>
		/// Count of calls to GetSymbol.
		/// </summary>
		public double GetSymbolCount;

		/// <summary>
		/// Count of calls to GetSymbolIndex.
		/// </summary>
		public double GetSymbolIndexCount;

		/// <summary>
		/// Count of calls to GetSymbolList.
		/// </summary>
		public double GetSymbolListCount;

		/// <summary>
		/// Count of calls to GetSymbolPositions.
		/// </summary>
		public double GetSymbolPositionsCount;

		public StripAccessMetricsData()
		{
		}

		public StripAccessMetricsData(StripAccessMetricsData item)
		{
			GetEqualsCount = item.GetEqualsCount;
			GetTotalWeightCount = item.GetTotalWeightCount;
			GetSymbolIndexAtWeightACount = item.GetSymbolIndexAtWeightACount;
			GetSymbolIndexAtWeightBCount = item.GetSymbolIndexAtWeightBCount;
			GetLengthCount = item.GetLengthCount;
			GetWeightCount = item.GetWeightCount;
			GetSymbolCount = item.GetSymbolCount;
			GetSymbolIndexCount = item.GetSymbolIndexCount;
			GetSymbolListCount = item.GetSymbolListCount;
			GetSymbolPositionsCount = item.GetSymbolPositionsCount;
		}
	}
}