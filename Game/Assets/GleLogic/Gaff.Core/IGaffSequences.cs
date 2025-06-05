using System.Collections.Generic;

namespace Gaff.Core
{
	/// <summary>
	/// The interface used by the game and the logic player to access code generated gaff sequences.
	/// </summary>
	// ReSharper disable once UnusedType.Global - Used by Unity
	public interface IGaffSequences
	{
		/// <summary>
		/// Return a list of gaff sequences stored in the implementation of this interface.
		/// </summary>
		// ReSharper disable once UnusedMember.Global - Used by Unity
		IReadOnlyList<GaffSequence> GetSequences();
	}
}