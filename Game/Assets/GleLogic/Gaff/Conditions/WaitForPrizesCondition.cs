using System.Collections.Generic;
using System.Linq;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	/// <summary>
	/// When a single result in the step meets the prize conditions this step condition returns success.
	/// Each prize condition when met will remove that prize from consideration for the following conditions.
	/// M1^? - at least one M1 prize of any count.
	/// M1^? M1^? - at least two M1 prizes of any count.
	/// M1^?|M2^? - at least one M1 or M2 prize of any count.
	/// </summary>
	public sealed class WaitForPrizesCondition : StepCondition
	{
		public WaitForPrizesCondition(StringCondition prizesOutputName, string prizesConditionText)
		{
			PrizesOutputName = prizesOutputName;
			PrizeConditionText = prizesConditionText;
		}

		public StringCondition PrizesOutputName { get; }
		public string PrizeConditionText { get; }

		public override StepConditionResult CheckCondition(CycleResult resdult, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow, ref object stateData)
		{
			var stepResults = sequenceUpToNow.StepCycleResults(initialResultForStep);

			foreach (var stepResult in stepResults)
			{
				var prizes = (IReadOnlyList<CellPrizeResult>)stepResult.StageResults.Where(r => PrizesOutputName.Check(r.Name) && r.Value is IReadOnlyList<CellPrizeResult>).SelectMany(r => (IReadOnlyList<CellPrizeResult>)r.Value).ToList();

				if (prizes.Count == 0)
					continue;

				var originalSelections = PrizeConditionText.SplitAndTrim(" ");

				var validPrizes = new List<HashSet<CellPrizeResult>>();

				foreach (var selection in originalSelections)
				{
					var localValidPrizes = new HashSet<CellPrizeResult>();
					var pcs = GetPrizeInfo(selection);
					foreach (var prizeResult in prizes)
					{
						if (pcs.Any(pc => prizeResult.Name == pc.PrizeName && (pc.HitCount == int.MinValue || pc.HitCount == prizeResult.Count)))
							localValidPrizes.Add(prizeResult);
					}

					validPrizes.Add(localValidPrizes);
				}

				if (validPrizes.All(l => l.Count != 0) && validPrizes.CanFindUniqueSymbols())
					return StepConditionResult.Found;
			}

			return StepConditionResult.KeepSearching;

			IReadOnlyList<(string PrizeName, int HitCount)> GetPrizeInfo(string selection)
			{
				var resultList = new List<(string PrizeName, int HitCount)>();
				var prizeConditions = selection.SplitAndTrim("|");

				foreach (var pc in prizeConditions)
				{
					var args = pc.SplitAndTrim("^");
					if (args.Length != 2)
						continue;
					resultList.Add((args[0], args[1] == "?" ? int.MinValue : int.Parse(args[1])));
				}

				return resultList;
			}
		}

		public override IResult ToString(string format)
		{
			return $"If output name '{PrizesOutputName}' then check the prizes condition '{PrizeConditionText}'.".ToSuccess();
		}
	}
}