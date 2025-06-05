using System.Collections.Generic;

namespace Midas.Presentation.Lights
{
	public interface ILightOwner
	{
		IReadOnlyList<LightsBase> Lights { get; }
	}
}