using System.Collections.Generic;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Used by the system to gather all sequences so they can be managed together.
	/// </summary>
	public interface ISequenceOwner
	{
		/// <summary>
		/// Gets the list of sequences owned by this object.
		/// </summary>
		IReadOnlyList<Sequence> Sequences { get; }
	}
}