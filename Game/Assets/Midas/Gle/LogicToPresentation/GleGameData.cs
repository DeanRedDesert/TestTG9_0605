using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game;
using Logic.Core.Engine;

namespace Midas.Gle.LogicToPresentation
{
	public static partial class GleGameData
	{
		public static Runner Runner { get; }
		public static InputSets InputSets { get; }
		public static GaffSequences GaffSequences { get; }
		public static Progressives Progressives { get; }
		public static IPid PidProvider { get; }
		public static IReadOnlyDictionary<string, object> AllData { get; }
		public static GleStageData EntryStage { get; }
		public static IReadOnlyList<GleStageData> Stages { get; }
		public static IReadOnlyList<GleStageConnection> StageConnections { get; }
		public static CycleState BaseCycle { get; }

		static GleGameData()
		{
			Runner = new Runner();
			InputSets = new InputSets();
			GaffSequences = new GaffSequences();
			PidProvider = new PidProvider();
			Progressives = new Progressives();
			BaseCycle = InputSets.GetInputs(0, new int[InputSets.GetInputCount()]).GetCycles().Current;

			var stageData = new List<GleStageData>();
			var t = Runner.GetType();

			AllData = GetAllGameDataFields();

			var startingStage = t.GetCustomAttribute<EntryStageAttribute>()?.EntryStageName;
			foreach (var stageName in t.GetCustomAttribute<StageNamesAttribute>().StageNames)
			{
				var stage = new GleStageData(Runner, stageName, stageName == startingStage);

				if (stage.IsEntryStage)
					EntryStage = stage;

				stageData.Add(stage);
			}

			Stages = stageData;

			var stageConnections = new List<GleStageConnection>();
			var runnerConnections = (IReadOnlyList<StageConnection>)t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).Single(f => f.FieldType == typeof(IReadOnlyList<StageConnection>)).GetValue(Runner);

			foreach (var stageConnection in runnerConnections)
				stageConnections.Add(new GleStageConnection(Stages.Single(s => s.Name == stageConnection.InitialStage), Stages.Single(s => s.Name == stageConnection.FinalStage), stageConnection.ExitName));

			StageConnections = stageConnections;

			IReadOnlyDictionary<string, object> GetAllGameDataFields()
			{
				var fields = Runner.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
				return fields.Where(field => field.GetCustomAttribute<StageNamesAttribute>() != null).ToDictionary(field => field.Name, field => field.GetValue(Runner));
			}
		}
	}
}