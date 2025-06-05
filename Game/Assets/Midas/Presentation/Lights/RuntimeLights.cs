using System;
using Midas.Presentation.Cabinet;

namespace Midas.Presentation.Lights
{
	public abstract class RuntimeLights : LightsBase, IRuntimeLights
	{
		public abstract TimeSpan Duration { get; }

		public override LightsHandle Register()
		{
			return CabinetManager.Cabinet.AddLights(this);
		}

		public abstract void CreateSequence(IRuntimeLightsFactory factory);
	}
}