namespace Logic.Core.Engine
{
	/// <summary>
	/// A connection between two stages, it starts at a specific
	/// exit on the initial stage and goes to the final stage.
	/// </summary>
	public sealed class StageConnection
	{
		/// <summary>
		/// The initial/stating stage name of this connection.
		/// </summary>
		public string InitialStage { get; }

		/// <summary>
		/// The final/destination stage name of this connection.
		/// </summary>
		public string FinalStage { get; }

		/// <summary>
		/// The name of the exit that is leaving the InitialStage for this connection.
		/// </summary>
		public string ExitName { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="initialStage">The name of the initial/starting stage of this connection.</param>
		/// <param name="exitName">The name of the exit that is leaving the InitialStage for this connection.</param>
		/// <param name="finalStage">The name of the final/destination stage of this connection.</param>
		public StageConnection(string initialStage, string exitName, string finalStage)
		{
			InitialStage = initialStage;
			ExitName = exitName;
			FinalStage = finalStage;
		}
	}
}
