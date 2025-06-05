using System;
using System.Collections.Generic;
using System.Linq;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Types;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

namespace Gaff.Conditions
{
	/// <summary>
	/// Use result if specific symbol window conditions are met.
	/// M1 - anywhere on screen
	/// M1:0.2 - top row, second column from the left.
	/// M1:?.2 - any row, second column from the left.
	/// M1:0.? - top row, any column.
	/// M1:*.2 - all rows, second column from the left.
	/// M1:0.* - top row, all columns.
	/// M1|M2|M3:0.2 Symbol string can be multiple options.
	/// Adding an '^' to the start of an entry will invert the condition:
	/// ^M1 - nowhere on screen
	/// ^M1:0.2 - not in the top row, second column from the left.
	/// ^M1:?.2 - not in any position of the second column from the left.
	/// </summary>
	public sealed class SymbolVisibleCondition : ResultCondition
	{
		public SymbolVisibleCondition(StringCondition symbolWindowOutputName, string symbolConditionText)
		{
			SymbolWindowOutputName = symbolWindowOutputName;
			SymbolConditionText = symbolConditionText;
		}

		public StringCondition SymbolWindowOutputName { get; }
		public string SymbolConditionText { get; }

		public override bool CheckCondition(CycleResult result, CycleResult _, IReadOnlyList<StageGaffResult> __)
		{
			var symWindow = (IReadOnlyList<SymbolWindowResult>)result.StageResults.Where(r => SymbolWindowOutputName.Check(r.Name) && (r.Value is SymbolWindowResult || r.Value is LockedSymbolWindowResult)).Select(r => (SymbolWindowResult)(r.Value is LockedSymbolWindowResult lsw ? lsw.SymbolWindowResult : r.Value)).ToList();

			if (symWindow.Count == 0)
				return false;

			var originalSelections = SymbolConditionText.SplitAndTrim(" ");
			var rangeSelections = originalSelections.Where(s => s.Contains(':')).Select(s =>
			{
				var item = s.SplitAndTrim(":");
				return (Symbol: item[0], Range: ParseRange(item[1]));
			}).Concat(originalSelections.Where(s => !s.Contains(':')).Select(s => (Symbol: s, Range: (int.MinValue, int.MinValue)))).ToList();

			foreach (var symbolWindowResult in symWindow)
			{
				foreach (var rangeSelection in rangeSelections)
				{
					if (!CheckRange(rangeSelection, symbolWindowResult))
						return false;
				}
			}

			return true;
		}

		private static bool CheckRange((string Symbol, (int Row, int Column) Range) rangeSelection, SymbolWindowResult symWindow)
		{
			var notCondition = rangeSelection.Symbol.StartsWith("^");
			var structure = symWindow.SymbolWindowStructure;
			var symbolText = notCondition ? new string(rangeSelection.Symbol.Skip(1).ToArray()) : rangeSelection.Symbol;
			var symbols = symbolText.SplitAndTrim("|");
			var symIndexes = symbols.Select(s => symWindow.SymbolList.IndexOf(s)).Where(i => i >= 0).ToList();

			if (symIndexes.Count == 0)
				return false;

			var range = rangeSelection.Range;
			var targetMask = symWindow.SymbolMasks.GetCombinedSymbolMask(symIndexes);

			return CheckRangeCore(targetMask, range, structure, notCondition);
		}

		private static bool CheckRangeCore(ReadOnlyMask targetMask, (int Row, int Column) range, SymbolWindowStructure structure, bool notCondition)
		{
			var cells = structure.Cells;

			if (notCondition)
			{
				if (targetMask.IsEmpty)
					return true;

				if (IsAny(range))
					return targetMask.IsEmpty;

				if (IsAll(range))
					return targetMask.TrueCount != cells.Count;

				if (IsAllRow(range) && IsAnyColumn(range))
				{
					foreach (var rowOfColumns in cells.GroupBy(c => c.Row))
					{
						var mask = structure.CellsToMask(rowOfColumns.ToList());
						if (targetMask.And(mask).Equals(mask))
							return false;
					}

					return true;
				}

				if (IsAnyRow(range) && IsAllColumn(range))
				{
					foreach (var rowOfColumns in cells.GroupBy(c => c.Row))
					{
						var mask = structure.CellsToMask(rowOfColumns.ToList());
						if (targetMask.And(mask).Equals(mask))
							return false;
					}

					return true;
				}

				if (IsAllRow(range) && IsSpecificColumn(range))
				{
					var mask = structure.CellsToMask(cells.Where(c => c.Column == range.Column).ToList());
					return !targetMask.And(mask).Equals(mask);
				}

				if (IsAnyRow(range) && IsSpecificColumn(range))
				{
					var mask = structure.CellsToMask(cells.Where(c => c.Column == range.Column).ToList());
					return targetMask.AndIsEmpty(mask);
				}

				if (IsSpecificRow(range) && IsAllColumn(range))
				{
					var mask = structure.CellsToMask(cells.Where(c => c.Row == range.Row).ToList());
					return !targetMask.And(mask).Equals(mask);
				}

				if (IsSpecificRow(range) && IsAnyColumn(range))
				{
					var mask = structure.CellsToMask(cells.Where(c => c.Row == range.Row).ToList());
					return targetMask.AndIsEmpty(mask);
				}

				if (IsSpecificRow(range) && IsSpecificColumn(range))
					return !targetMask[structure.GetCellIndex(new Cell(range.Column, range.Row))];

				throw new NotSupportedException();
			}

			if (targetMask.IsEmpty)
				return false;

			if (IsAny(range))
				return targetMask.TrueCount > 0;

			if (IsAll(range))
				return targetMask.TrueCount == cells.Count;

			if (IsAllRow(range) && IsAnyColumn(range))
			{
				foreach (var rowOfColumns in cells.GroupBy(c => c.Row))
				{
					var mask = structure.CellsToMask(rowOfColumns.ToList());
					if (targetMask.And(mask).Equals(mask))
						return true;
				}

				return false;
			}

			if (IsAnyRow(range) && IsAllColumn(range))
			{
				foreach (var rowOfColumns in cells.GroupBy(c => c.Row))
				{
					var mask = structure.CellsToMask(rowOfColumns.ToList());
					if (targetMask.And(mask).Equals(mask))
						return true;
				}

				return false;
			}

			if (IsAllRow(range) && IsSpecificColumn(range))
			{
				var mask = structure.CellsToMask(cells.Where(c => c.Column == range.Column).ToList());
				return targetMask.And(mask).Equals(mask);
			}

			if (IsAnyRow(range) && IsSpecificColumn(range))
			{
				var mask = structure.CellsToMask(cells.Where(c => c.Column == range.Column).ToList());
				return targetMask.AndNotEmpty(mask);
			}

			if (IsSpecificRow(range) && IsAllColumn(range))
			{
				var mask = structure.CellsToMask(cells.Where(c => c.Row == range.Row).ToList());
				return targetMask.And(mask).Equals(mask);
			}

			if (IsSpecificRow(range) && IsAnyColumn(range))
			{
				var mask = structure.CellsToMask(cells.Where(c => c.Row == range.Row).ToList());
				return targetMask.AndNotEmpty(mask);
			}

			if (IsSpecificRow(range) && IsSpecificColumn(range))
				return targetMask[structure.GetCellIndex(new Cell(range.Column, range.Row))];

			throw new NotSupportedException();

			bool IsAny((int Row, int Column) r) => IsAnyRow(r) && IsAnyColumn(r);
			bool IsAll((int Row, int Column) r) => IsAllRow(r) && IsAllColumn(r);
			bool IsAnyRow((int Row, int Column) r) => r.Row == int.MinValue;
			bool IsAnyColumn((int Row, int Column) r) => r.Column == int.MinValue;
			bool IsAllRow((int Row, int Column) r) => r.Row == int.MaxValue;
			bool IsAllColumn((int Row, int Column) r) => r.Column == int.MaxValue;
			bool IsSpecificRow((int Row, int Column) r) => r.Row != int.MinValue && r.Row != int.MaxValue;
			bool IsSpecificColumn((int Row, int Column) r) => r.Column != int.MinValue && r.Column != int.MaxValue;
		}

		private static (int Row, int Column) ParseRange(string s)
		{
			var range = s.SplitAndTrim(".");
			(int Row, int Column) returnResult = (-1, -1);

			if (range.Length != 2)
				return (-1, -1);

			switch (range[0])
			{
				case "?": returnResult.Row = int.MinValue; break;
				case "*": returnResult.Row = int.MaxValue; break;
				default: returnResult.Row = int.Parse(range[0]); break;
			}

			switch (range[1])
			{
				case "?": returnResult.Column = int.MinValue; break;
				case "*": returnResult.Column = int.MaxValue; break;
				default: returnResult.Column = int.Parse(range[1]); break;
			}

			return returnResult;
		}

		public override IResult ToString(string format)
		{
			return $"if output name '{SymbolWindowOutputName}' then check the symbol window condition '{SymbolConditionText}'.".ToSuccess();
		}
	}
}