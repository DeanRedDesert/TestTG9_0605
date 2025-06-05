using Logic.Core.DecisionGenerator.Decisions;

namespace Midas.Gaff.ResultEditor
{
	public interface IDecision
	{
		bool Hold { get; }

		string Context { get; }

		Decision Decision { get; }
	}
}