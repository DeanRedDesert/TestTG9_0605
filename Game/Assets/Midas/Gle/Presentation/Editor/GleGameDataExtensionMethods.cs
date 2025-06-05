using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Logic.Core.Engine;
using Logic.Core.Types;
using Logic.Core.WinCheck;
using Midas.Presentation.Editor.GameData;
using Midas.Gle.LogicToPresentation;

namespace Midas.Gle.Presentation.Editor
{
	public static class GleGameDataExtensionMethods
	{
		public static IReadOnlyList<(string name, object patterns)> GetPatterns(this GleGameData.GleStageData stageData)
		{
			return stageData.StageData.Where(d => IsOfType<Patterns>(d.o)).ToArray();
		}

		public static IReadOnlyList<ISymbolListStrip> GetAllSymbolStrips(this GleGameData.GleStageData stageData)
		{
			return stageData.StageData.Select(field => GetAllOfType<ISymbolListStrip>(field.o)).SelectMany(v => v).ToArray();
		}

		public static bool HasReels(this GleGameData.GleStageData stageData)
		{
			return stageData.GetSymbolWindowResultProperties().Count != 0;
		}

		public static bool CanAwardProgressives(this GleGameData.GleStageData stageData)
		{
			return stageData.Result.Props.Any(p => p.stageResultType == StageResultType.ProgressiveList);
		}

		public static List<(PropertyInfo Prop, StageResultType StageResultType)> GetSymbolWindowResultProperties(this GleGameData.GleStageData stageData)
		{
			return stageData.Result.Props.Where(p => p.stageResultType == StageResultType.Presentation && IsSymbolWindowResult(p.prop.PropertyType)).ToList();
		}

		private static bool IsSymbolWindowResult(Type type)
		{
			return type == typeof(SymbolWindowResult) || type == typeof(LockedSymbolWindowResult);
		}

		public static IReadOnlyList<SymbolWindowStructure> GetSymbolWindowStructures(this GleGameData.GleStageData stageData)
		{
			return stageData.StageData.SelectMany(s => GetAllOfType<SymbolWindowStructure>(s.o)).ToArray();
		}

		public static IReadOnlyList<ReelInformation> GetReelInfoEntries(this GleGameData.GleStageData stageData)
		{
			var symbolWindows = stageData.StageData.Where(d => IsOfType<SymbolWindowStructure>(d.o)).ToArray();

			var result = new List<ReelInformation>();
			foreach (var o in symbolWindows)
			{
				if (o.o is SymbolWindowStructure sw)
				{
					result.Add(new ReelInformation(o.name, sw.PopulationsAsIndexes.Select(i => sw.IndexesToCells(i).Select(c => (c.Row, c.Column)).ToArray()).ToArray()));
				}
			}

			return result;
		}

		public static IReadOnlyList<LinePatternInformation> GetLinePatternInfo(this GleGameData.GleStageData stageData)
		{
			var patterns = stageData.StageData.Where(d => IsOfType<Patterns>(d.o)).ToArray();

			var result = new List<LinePatternInformation>();
			foreach (var o in patterns)
			{
				if (o.o is Patterns p && p.LinePatterns != null)
				{
					var sourcePatterns = p.GetSourcePatterns();
					result.Add(new LinePatternInformation(o.name, sourcePatterns.Select(sp => sp.ToLineInfo()).ToArray()));
				}
			}

			return result;
		}

		private static LineDefinition ToLineInfo(this Pattern sourcePattern)
		{
			var cells = new List<(int Column, int Row)>(sourcePattern.Clusters.Count);
			foreach (var cluster in sourcePattern.Clusters)
				cells.Add((cluster.Cells[0].Column, cluster.Cells[0].Row));

			return new LineDefinition(sourcePattern.Name, cells);
		}

		public static IReadOnlyList<string> GetAwardPrizesResultProperties(this GleGameData.GleStageData stageData)
		{
			return stageData.Result.Props.Where(p => p.stageResultType == StageResultType.AwardCreditsList).Select(p => p.prop.Name).ToList();
		}

		private static bool IsOfType<T>(object o)
		{
			if (o is SelectorItem[] sia)
				return SelectorContains<T>(sia);

			return o is T;
		}

		private static bool SelectorContains<T>(SelectorItem[] sia)
		{
			return sia.Any(item => IsOfType<T>(item.Data));
		}

		private static IEnumerable<T> GetAllOfType<T>(object o)
		{
			return o switch
			{
				T oAsT => new[] { oAsT },
				IEnumerable<T> oAsEnumT => oAsEnumT,
				SelectorItems sia => GetAllFromSelector<T>(sia),
				_ => Array.Empty<T>()
			};
		}

		private static IEnumerable<T> GetAllFromSelector<T>(SelectorItems sia)
		{
			var r = new List<T>();

			for (var i = 0; i < sia.Count; i++)
				r.AddRange(GetAllOfType<T>(sia[i].Data));

			return r;
		}
	}
}