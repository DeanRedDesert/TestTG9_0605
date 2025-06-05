using System.Collections.Generic;
using Midas.Core.StateMachine.JsonConverters;
using Newtonsoft.Json;

namespace Midas.Core.StateMachine
{
	public static class StateMachines
	{
		/// <summary>
		/// The higher the priority the state machine will be executed first
		/// </summary>
		public const int DefaultPriority = 0;

		private static readonly List<StateMachine> allStateMachines = new List<StateMachine>();
		private static readonly List<StateMachine> allRootStateMachines = new List<StateMachine>();

		public static IReadOnlyCollection<StateMachine> AllStateMachines => allStateMachines;
		public static IReadOnlyCollection<StateMachine> AllRootStateMachines => allRootStateMachines;

		public static StateMachine Create(string name, List<Rule> rules, StateMachine parent = null, int priority = DefaultPriority)
		{
			var stateMachine = new StateMachine(name, rules, priority);

			if (parent != null)
			{
				parent.AddChildStateMachine(stateMachine);
			}
			else
			{
				allRootStateMachines.Add(stateMachine);
			}

			allStateMachines.Add(stateMachine);

			return stateMachine;
		}

		public static void Destroy(StateMachine stateMachine)
		{
			if (stateMachine.Parent != null)
			{
				stateMachine.Parent?.RemoveChildStateMachine(stateMachine);
			}
			else
			{
				allRootStateMachines.Remove(stateMachine);
			}

			allStateMachines.Remove(stateMachine);
		}

		/// <summary>
		/// Destroys the state machine (unregister from parent) and sets the ref to null
		/// </summary>
		/// <param name="stateMachine"></param>
		public static void Destroy(ref StateMachine stateMachine)
		{
			Destroy(stateMachine);
			stateMachine = null;
		}

		public static void ResetAllMaxStepCounters()
		{
			allStateMachines.ForEach(s => s.MaxStepCounter = 0);
		}

		public static string GetJsonDump(bool niceFormat)
		{
			var settings = new JsonSerializerSettings
			{
				Converters =
				{
					new StateMachineListConverter(),
					new StateMachineConverter(),
					new StateConverter(),
					new StateStepConverter()
				},
				Formatting = niceFormat ? Formatting.Indented : Formatting.None,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			};

			return JsonConvert.SerializeObject(allStateMachines, settings);
		}
	}
}