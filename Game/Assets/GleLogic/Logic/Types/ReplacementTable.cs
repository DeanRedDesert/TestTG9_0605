using System;
using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Types
{
	public sealed class ReplacementTables : IToString
	{
		private readonly IReadOnlyList<ReplacementTable> set1FrameNonZeroTable;
		private readonly IReadOnlyList<ReplacementTable> set1NoFrameNonZeroTable;
		private readonly IReadOnlyList<ReplacementTable> set1NoFrameJpNonZeroTable;
		private readonly ulong totalWeightSet1Frame;
		private readonly ulong totalWeightSet1NoFrame;
		private readonly ulong totalWeightSet1NoFrameJP;

		private readonly IReadOnlyList<ReplacementTable> set2FrameNonZeroTable;
		private readonly IReadOnlyList<ReplacementTable> set2NoFrameNonZeroTable;
		private readonly IReadOnlyList<ReplacementTable> set2NoFrameJpNonZeroTable;
		private readonly ulong totalWeightSet2Frame;
		private readonly ulong totalWeightSet2NoFrame;
		private readonly ulong totalWeightSet2NoFrameJP;

		public IReadOnlyList<ReplacementTable> Set1 { get; }
		public IReadOnlyList<ReplacementTable> Set2 { get; }

		public ReplacementTables(IReadOnlyList<ReplacementTable> set1, IReadOnlyList<ReplacementTable> set2)
		{
			Set1 = set1;
			Set2 = set2;

			set1FrameNonZeroTable = GetAllNonZeroWeightedData(set1, r => r.Frame, out totalWeightSet1Frame);
			set1NoFrameNonZeroTable = GetAllNonZeroWeightedData(set1, r => r.NoFrame, out totalWeightSet1NoFrame);
			set1NoFrameJpNonZeroTable = GetAllNonZeroWeightedData(set1, r => r.NoFrameJp, out totalWeightSet1NoFrameJP);
			set2FrameNonZeroTable = GetAllNonZeroWeightedData(set2, r => r.Frame, out totalWeightSet2Frame);
			set2NoFrameNonZeroTable = GetAllNonZeroWeightedData(set2, r => r.NoFrame, out totalWeightSet2NoFrame);
			set2NoFrameJpNonZeroTable = GetAllNonZeroWeightedData(set2, r => r.NoFrameJp, out totalWeightSet2NoFrameJP);
		}

		public void GetTotalWeightSet1(out IReadOnlyList<ReplacementTable> frame, out IReadOnlyList<ReplacementTable> noFrame, out IReadOnlyList<ReplacementTable> noFrameJP, out ulong frameTotalWeight, out ulong noframeTotalWeight, out ulong noframeJPTotalWeight)
		{
			frame = set1FrameNonZeroTable;
			noFrame = set1NoFrameNonZeroTable;
			noFrameJP = set1NoFrameJpNonZeroTable;
			frameTotalWeight = totalWeightSet1Frame;
			noframeTotalWeight = totalWeightSet1NoFrame;
			noframeJPTotalWeight = totalWeightSet1NoFrameJP;
		}

		public void GetTotalWeightSet2(out IReadOnlyList<ReplacementTable> frame, out IReadOnlyList<ReplacementTable> noFrame, out IReadOnlyList<ReplacementTable> noFrameJP, out ulong frameTotalWeight, out ulong noframeTotalWeight, out ulong noframeJPTotalWeight)
		{
			frame = set2FrameNonZeroTable;
			noFrame = set2NoFrameNonZeroTable;
			noFrameJP = set2NoFrameJpNonZeroTable;
			frameTotalWeight = totalWeightSet2Frame;
			noframeTotalWeight = totalWeightSet2NoFrame;
			noframeJPTotalWeight = totalWeightSet2NoFrameJP;
		}

		private static IReadOnlyList<ReplacementTable> GetAllNonZeroWeightedData(IReadOnlyCollection<ReplacementTable> originalData, Func<ReplacementTable, ulong> getWeight, out ulong totalWeight)
		{
			var data = new List<ReplacementTable>(originalData.Count);
			totalWeight = 0;
			foreach (var item in originalData)
			{
				var weight = getWeight(item);
				if (weight == 0)
					continue;

				totalWeight += weight;
				data.Add(item);
			}

			return data;
		}

		public IResult ToString(string format)
		{
			if (format != "ML")
				return new NotSupported();

			var arr = new List<List<string>> { new List<string> { "Symbol", "Sym Index", "S1 No Frame JP", "S1 No Frame", "S1 Frame", "S2 No Frame JP", "S2 No Frame", "S2 Frame" } };

			for (var i = 0; i < Set1.Count; i++)
			{
				var set1 = Set1[i];
				var set2 = Set2[i];

				arr.Add(new List<string>
				{
					$"{set1.SymbolName}",
					$"{set1.SymbolIndex}",
					$"{set1.NoFrameJp}",
					$"{set1.NoFrame}",
					$"{set1.Frame}",
					$"{set2.NoFrameJp}",
					$"{set2.NoFrame}",
					$"{set2.Frame}"
				});
			}

			return arr.ToTableResult();
		}
	}

	public sealed class ReplacementTable
	{
		public string SymbolName { get; }
		public int SymbolIndex { get; }
		public ulong NoFrameJp { get; }
		public ulong NoFrame { get; }
		public ulong Frame { get; }

		public ReplacementTable(string symbolName, int symbolIndex, ulong noFrameJp, ulong noFrame, ulong frame)
		{
			SymbolName = symbolName;
			SymbolIndex = symbolIndex;
			NoFrameJp = noFrameJp;
			NoFrame = noFrame;
			Frame = frame;
		}
	}
}