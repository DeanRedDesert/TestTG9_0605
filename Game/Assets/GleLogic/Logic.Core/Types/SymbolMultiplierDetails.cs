using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// A class containing configuration details for the ApplySymbolMultiplier processor function.
	/// </summary>
	public sealed class SymbolMultiplierDetails : IToString
	{
		/// <summary>
		/// The list of multipliers and the symbols they are associated with.
		/// </summary>
		public IReadOnlyList<SymbolMultiplier> SymbolMultipliers { get; }

		/// <summary>
		/// The way the symbol multipliers will be combined.
		/// </summary>
		public MultiplierUsage MultiplierUsage { get; }

		/// <summary>
		/// The prizes to exclude from symbol multiplier evaluations.
		/// </summary>
		public IReadOnlyList<string> ExcludedPrizes { get; }

		public SymbolMultiplierDetails(IReadOnlyList<SymbolMultiplier> symbolMultipliers, MultiplierUsage multiplierUsage, IReadOnlyList<string> excludedPrizes)
		{
			SymbolMultipliers = symbolMultipliers;
			MultiplierUsage = multiplierUsage;
			ExcludedPrizes = excludedPrizes;
		}

		public IResult ToString(string format)
		{
			var tableData = new List<List<string>>();

			tableData.Add(new List<string> { "Multiplier Usage", MultiplierUsage.ToString() });
			var workingRow = new List<string>();
			workingRow.Add("Excluded Prizes");

			foreach (var prize in ExcludedPrizes)
				workingRow.Add(prize);

			tableData.Add(workingRow);
			tableData.Add(new List<string>());
			tableData.Add(new List<string> { "Symbol", "Multiplier" });

			for (var i = 0; i < SymbolMultipliers.Count; i++)
			{
				workingRow = new List<string>();
				workingRow.Add(SymbolMultipliers[i].SymbolName);
				workingRow.Add(SymbolMultipliers[i].Multiplier.ToString());
				tableData.Add(workingRow);
			}

			return tableData.ToTableResult("");
		}
	}
}