namespace Logic.Core.Engine
{
	/// <summary>
	/// Represents a set of cycles plus a little bit of state data describing follow on cycles.
	/// </summary>
	public sealed class CycleState
	{
		/// <summary>
		/// Identifier for this CycleState, which is unique in the scope of one game.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// The cycle that was running when this cycle state was initially triggered.
		/// </summary>
		public CycleState TriggeringCycle { get; }

		/// <summary>
		/// The name of the stage to process during this cycle.
		/// </summary>
		public string Stage { get; }

		/// <summary>
		/// The name of the cycle id for this cycle.
		/// </summary>
		public string CycleId { get; }

		/// <summary>
		/// The number of cycles to run (or, how many times to run this exit before it is removed from the stack).
		/// </summary>
		public int TotalCycles { get; }

		/// <summary>
		/// The count of completed cycles in the cycle state, starts at zero and will be incremented after each cycle is played.
		/// Once it is the same as TotalCycles the cycle state will be considered complete.
		/// </summary>
		public int CompletedCycles { get; }

		internal CycleState(int id, CycleState triggeringCycle, string stage, string cycleId, int totalCycles, int completedCycles)
		{
			Id = id;
			TriggeringCycle = triggeringCycle;
			Stage = stage;
			CycleId = cycleId;
			TotalCycles = totalCycles;
			CompletedCycles = completedCycles;
		}

		public bool IsFinished => CompletedCycles == TotalCycles;
	}
}