using Logic.Core.DecisionGenerator;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// An abstraction of a strip of symbols that is used by the decision generator to produce random selections and sequences.
	/// </summary>
	public interface IStrip : IWeights
	{
		/// <summary>
		/// The symbol at the index specified.
		/// </summary>
		string GetSymbol(ulong index);
	}
}