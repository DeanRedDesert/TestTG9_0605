using System.Collections.Generic;
using IGT.Game.Core.Presentation.PeripheralLights;
using IGT.Game.Core.Presentation.PeripheralLights.Devices;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming.Choreography;

namespace Midas.Ascent.Cabinet.Lights
{
	internal abstract class ChoreographyBase
	{
		private readonly Dictionary<CabinetType, Choreography> choreographyDict = new Dictionary<CabinetType, Choreography>();

		public abstract string Name { get; }
		public Color FallbackColor { get; }

		protected ChoreographyBase(Color fallbackColor)
		{
			FallbackColor = fallbackColor;
		}

		public Choreography GetChoreography(CabinetType cabinetType)
		{
			if (!choreographyDict.TryGetValue(cabinetType, out var choreography))
			{
				choreography = CreateChoreography(cabinetType);
				if (choreography != null)
					choreographyDict.Add(cabinetType, choreography);
			}

			return choreography;
		}

		public abstract LightSequence GetLightSequence(string uniqueId);

		protected abstract Choreography CreateChoreography(CabinetType cabinetType);
	}
}