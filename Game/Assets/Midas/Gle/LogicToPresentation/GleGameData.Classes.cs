using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game;
using Logic.Core.Engine;

namespace Midas.Gle.LogicToPresentation
{
	public static partial class GleGameData
	{
		public sealed class GleStageResult
		{
			public Type ResultType { get; }
			public IReadOnlyList<(PropertyInfo prop, StageResultType stageResultType)> Props { get; }

			public GleStageResult(Type resultType)
			{
				ResultType = resultType;

				var props = new List<(PropertyInfo prop, StageResultType stageResultType)>();
				foreach (var prop in resultType.GetProperties())
				{
					var rta = prop.GetCustomAttribute<ResultTypeAttribute>();
					if (rta != null)
						props.Add((prop, rta.ResultType));
				}

				Props = props;
			}
		}

		public sealed class GleStageConnection
		{
			public GleStageData InitialStage { get; }
			public GleStageData FinalStage { get; }
			public string ExitName { get; }

			public GleStageConnection(GleStageData initialStage, GleStageData finalStage, string exitName)
			{
				InitialStage = initialStage;
				FinalStage = finalStage;
				ExitName = exitName;
			}
		}

		public sealed class GleStageData
		{
			public string Name { get; }
			public bool IsEntryStage { get; }
			public IReadOnlyList<(string name, object o)> StageData { get; }
			public GleStageResult Result { get; }

			public GleStageData(Runner runner, string stageName, bool isEntryStage)
			{
				Name = stageName;
				IsEntryStage = isEntryStage;

				var fields = runner.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
				var stageData = new List<(string, object)>();

				foreach (var field in fields)
				{
					var stageNames = field.GetCustomAttribute<StageNamesAttribute>()?.StageNames;
					if (stageNames != null && stageNames.Contains(Name))
					{
						var o = field.GetValue(runner);
						stageData.Add((field.Name, o));
					}
				}

				StageData = stageData;

				foreach (var resultType in runner.GetType().Assembly.GetTypes().Where(t => t != typeof(StageResults) && typeof(StageResults).IsAssignableFrom(t)))
				{
					if (!resultType.GetCustomAttribute<StageNamesAttribute>().StageNames.Contains(stageName))
						continue;

					Result = new GleStageResult(resultType);
					break;
				}
			}
		}
	}
}