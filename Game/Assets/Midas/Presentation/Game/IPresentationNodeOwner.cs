using System.Collections.Generic;

namespace Midas.Presentation.Game
{
	public interface IPresentationNodeOwner
	{
		public IReadOnlyList<IPresentationNode> PresentationNodes { get; }
	}
}