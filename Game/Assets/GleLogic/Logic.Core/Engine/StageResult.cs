// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Logic.Core.Engine
{
	public enum StageResultType
	{
		AwardCreditsList,
		ExitList,
		ProgressiveList,
		Presentation,
		VariablePermanent,
		VariableOneGame,
		VariableOneCycle
	}

	/// <summary>
	/// Contains a result from a single processor.
	/// </summary>
	public sealed class StageResult
	{
		/// <summary>
		/// The index of the stage that generated the result.
		/// </summary>
		public int StageIndex { get; }

		/// <summary>
		/// The index of the processor that generated the result.
		/// </summary>
		public int ProcessorIndex { get; }

		/// <summary>
		/// The name of the result, usually the 'OutputName' of the connection.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The type of the result.
		/// </summary>
		public StageResultType Type { get; }

		/// <summary>
		/// The value of the result.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Is this result any type of variable.
		/// </summary>
		public bool IsVariable => Type == StageResultType.VariablePermanent || Type == StageResultType.VariableOneCycle || Type == StageResultType.VariableOneGame;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stageIndex">The index of the stage that generated the result.</param>
		/// <param name="processorIndex">The index of the processor that generated the result.</param>
		/// <param name="name">The name of the result, usually the 'OutputName' of the connection.</param>
		/// <param name="type">The type of the result.</param>
		/// <param name="value">The value of the result.</param>
		public StageResult(int stageIndex, int processorIndex, string name, StageResultType type, object value)
		{
			StageIndex = stageIndex;
			ProcessorIndex = processorIndex;
			Name = name;
			Type = type;
			Value = value;
		}
	}
}