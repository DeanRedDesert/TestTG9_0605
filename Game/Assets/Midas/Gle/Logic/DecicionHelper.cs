using System;
using System.Collections.Generic;
using System.Linq;
using Gaff.Core.GaffEditor;
using Logic.Core.DecisionGenerator;
using Logic.Core.DecisionGenerator.Decisions;

namespace Midas.Gle.Logic
{
	public static class DecisionHelper
	{
		public static IReadOnlyList<ulong> ConvertToRng(Decision decision)
		{
			var list = new List<ulong>();
			switch (decision.DecisionDefinition)
			{
				case SimpleDecision sd:
					var r = (bool)decision.Result;
					list.Add(r ? 0 : sd.TrueWeight);
					break;
				case WeightsIndexesDecision wsid:
				{
					ConvertWeightedIndexes((IReadOnlyList<ulong>)decision.Result, wsid.AllowDuplicates, w => wsid.Weights.GetWeight(w), list);
					break;
				}
				case WeightedIndexesDecision wid:
				{
					ConvertWeightedIndexes((IReadOnlyList<ulong>)decision.Result, wid.AllowDuplicates, wid.GetWeight, list);
					break;
				}
				case IndexesDecision sid:
					ConvertIndexes(sid.AllowDuplicates);
					break;

				case PickIndexesDecision pid:
					list.Add((uint)((IReadOnlyList<ulong>)decision.Result).Count - pid.MinCount);
					ConvertIndexes(pid.AllowDuplicates);
					break;
			}

			return list;

			void ConvertIndexes(bool allowDuplicates)
			{
				if (allowDuplicates)
				{
					list.AddRange((IReadOnlyList<ulong>)decision.Result);
				}
				else
				{
					var res = (IReadOnlyList<ulong>)decision.Result;
					var tempList = new List<ulong>();
					foreach (var item in res)
					{
						ulong below = 0;
						for (var resIndex = 0; resIndex < tempList.Count; resIndex++)
						{
							if (res[resIndex] < item)
								below++;
						}

						tempList.Add(item - below);
					}

					list.AddRange(tempList);
				}
			}
		}

		private static void ConvertWeightedIndexes(IReadOnlyList<ulong> decisionIndexes, bool allowDuplicates, Func<ulong, ulong> getWeight, List<ulong> list)
		{
			if (allowDuplicates)
			{
				foreach (var item in decisionIndexes)
				{
					ulong lowerBoundary = 0;
					for (ulong resIndex = 0; resIndex < item; resIndex++)
						lowerBoundary += getWeight(resIndex);

					list.Add(lowerBoundary);
				}
			}
			else
			{
				var used = new List<ulong>();
				foreach (var item in decisionIndexes)
				{
					ulong lowerBoundary = 0;
					for (ulong resIndex = 0; resIndex < item; resIndex++)
					{
						if (used.Contains(resIndex))
							continue;

						lowerBoundary += getWeight(resIndex);
					}

					used.Add(item);
					list.Add(lowerBoundary);
				}
			}
		}
	}
}