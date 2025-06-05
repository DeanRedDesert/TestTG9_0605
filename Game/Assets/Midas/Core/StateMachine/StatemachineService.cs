using System.Collections.Generic;
using System.Linq;

namespace Midas.Core.StateMachine
{
	public static class StateMachineService
	{
		private static StateMachine frameUpdateRoot;

		public static StateMachine FrameUpdateRoot => frameUpdateRoot;

		public static void Init()
		{
			// per frame updated state machine
			State frameIdle = new State(nameof(frameIdle), true);
			var stayInIdle = new Rule(frameIdle, new Transition(() => false, frameIdle));
			frameUpdateRoot = StateMachines.Create("FrameUpdateRoot", new List<Rule> { stayInIdle });
		}

		public static int Update()
		{
			int numCycles = 1;
			if (frameUpdateRoot != null)
			{
				while (frameUpdateRoot.Step(FrameTime.FrameNumber))
				{
					numCycles++;
				}
			}

			return numCycles;
		}

		public static void DeInit()
		{
			StateMachines.Destroy(ref frameUpdateRoot);

			if (StateMachines.AllStateMachines.Count > 0)
			{
				var copyOfStateMachines = StateMachines.AllStateMachines.ToArray();
				foreach (var s in copyOfStateMachines)
				{
					Log.Instance.Error($"StateMachine {s.Name} not destroyed");
					// Todo: This is just for safety
					// should be removed in future and change the log to fatal
					StateMachines.Destroy(s);
				}
			}
		}
	}
}