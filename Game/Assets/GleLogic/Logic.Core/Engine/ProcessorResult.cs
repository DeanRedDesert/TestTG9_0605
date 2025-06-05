namespace Logic.Core.Engine
{
	/// <summary>
	/// The result of an evaluation of a single processor.
	/// </summary>
	public sealed class ProcessorResult
	{
		public int ProcessorIndex { get; }
		public object[] Inputs { get; }
		public object Output { get; }

		public ProcessorResult(int processorIndex, object[] inputs, object output)
		{
			ProcessorIndex = processorIndex;
			Inputs = inputs;
			Output = output;
		}
	}
}