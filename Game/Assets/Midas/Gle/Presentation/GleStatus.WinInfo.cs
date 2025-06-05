using System.Collections.Generic;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Types;
using Midas.Core.General;
using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Gle.Presentation
{
	public sealed partial class GleStatus
	{
		private sealed class CellPrizeComparer : IComparer<CellPrizeResult>
		{
			/// <inheritdoc />
			public int Compare(CellPrizeResult x, CellPrizeResult y)
			{
				// Detect line prizes and treat all other prizes as a "scatter" for ordering purposes.
				var xIsScatter = x.Pattern == null || x.Pattern.Name.StartsWith("Scatter");
				var yIsScatter = y.Pattern == null || y.Pattern.Name.StartsWith("Scatter");

				if (xIsScatter && !yIsScatter)
					return -1;
				if (!xIsScatter && yIsScatter)
					return 1;

				if (x.Value > y.Value)
					return -1;
				if (x.Value < y.Value)
					return 1;
				if (x.WinningMask.Count > y.WinningMask.Count)
					return -1;
				if (x.WinningMask.Count < y.WinningMask.Count)
					return 1;

				int.TryParse(x.Pattern.Name.Replace("Line ", ""), out var xLineNumber);
				int.TryParse(y.Pattern.Name.Replace("Line ", ""), out var yLineNumber);

				return xLineNumber - yLineNumber;
			}
		}

		private sealed class CellPrizeResultWinInfo : IWinInfo
		{
			public string PrizeName { get; }
			public string PatternName { get; }
			public Credit Value { get; }
			public IReadOnlyList<(int Column, int Row)> WinningPositions { get; }
			public int? LineNumber { get; }
			public IReadOnlyList<(int Column, int Row)> LinePattern { get; }

			public CellPrizeResultWinInfo(CellPrizeResult cpr)
			{
				PrizeName = cpr.Name;
				PatternName = cpr.Pattern.Name;
				Value = Credit.FromLong(cpr.Value);
				WinningPositions = cpr.WinningMask.Select(cell => (cell.Column, cell.Row)).ToArray();

				if (cpr.Pattern != null && cpr.Pattern.Name.StartsWith("Line "))
				{
					LineNumber = int.Parse(cpr.Pattern.Name.Replace("Line ", ""));
					LinePattern = cpr.Pattern.Clusters.Select(c => (c.Cells[0].Column, c.Cells[0].Row)).ToArray();
				}
			}
		}

		private static readonly IComparer<CellPrizeResult> defaultComparer = new CellPrizeComparer();
		private readonly IComparer<CellPrizeResult> prizeComparer;
		private IReadOnlyList<IWinInfo> currentWinInfo;

		public override IReadOnlyList<IWinInfo> GetWinInfo()
		{
			if (currentWinInfo == null)
			{
				var allCellPrizeResults = new List<CellPrizeResult>();
				foreach (var r in CurrentGameResult.Current.StageResults)
				{
					if (r.Type == StageResultType.AwardCreditsList && r.Value is IReadOnlyList<CellPrizeResult> cpr)
						allCellPrizeResults.AddRange(cpr);
				}

				allCellPrizeResults.Sort(prizeComparer);
				currentWinInfo = allCellPrizeResults.Select(p => new CellPrizeResultWinInfo(p)).ToArray();
			}

			return currentWinInfo;
		}
	}
}