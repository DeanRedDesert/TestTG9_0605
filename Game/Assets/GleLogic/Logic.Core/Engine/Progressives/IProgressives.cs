using System.Collections.Generic;

namespace Logic.Core.Engine.Progressives
{
	/// <summary>
	/// The interface used by the code gen system to give access to the progressives.
	/// </summary>
	public interface IProgressives
	{
		/// <summary>
		/// Returns the progressive levels associated with the inputs specified.
		/// </summary>
		/// <param name="inputs">The inputs to use when selecting the relevant progressive levels.</param>
		/// <returns>The relevant progressive level definitions.</returns>
		IReadOnlyList<ProgressiveLevel> GetProgressiveLevels(IReadOnlyList<Input> inputs);
	}
}