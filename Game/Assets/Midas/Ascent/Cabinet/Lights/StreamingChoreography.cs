using System;
using System.Collections.Generic;
using System.IO;
using IGT.Game.Core.Presentation.PeripheralLights.Devices;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming.Choreography;
using Midas.Presentation.Cabinet;

namespace Midas.Ascent.Cabinet.Lights
{
	internal sealed class StreamingChoreography : ChoreographyBase
	{
		private static readonly Dictionary<CabinetType, string> cabinetCode = new Dictionary<CabinetType, string>
		{
			{ CabinetType.CrystalCoreDual27, "CD27" },
			{ CabinetType.CrystalCoreDual, "CC23" },
			{ CabinetType.CrystalCurve, "CRC" }
		};

		public IStreamingLights StreamingLights { get; }

		public StreamingChoreography(IStreamingLights streamingLights) : base(streamingLights.FallbackColor.ToPeripheralLightsColor())
		{
			StreamingLights = streamingLights;
		}

		public override string Name => StreamingLights.ChoreographyId;

		public override LightSequence GetLightSequence(string uniqueId) => throw new InvalidOperationException("Choreography sequences should be in the proper folder.");

		protected override Choreography CreateChoreography(CabinetType cabinetType)
		{
			// Check for xlightchor without cabinet prefix.
			var file = $@"EGMResources\StreamingLights\{cabinetType}\{StreamingLights.ChoreographyId}.xlightchor";

			if (File.Exists(file))
				return new Choreography(file);

			// Check for xlightchor with cabinet prefix.
			file = $@"EGMResources\StreamingLights\{cabinetType}\{StreamingLights.ChoreographyId}_{cabinetCode[cabinetType]}.xlightchor";

			if (File.Exists(file))
				return new Choreography(file);

			Log.Instance.Warn("Could not find choreography file: " + file);
			return null;
		}
	}
}