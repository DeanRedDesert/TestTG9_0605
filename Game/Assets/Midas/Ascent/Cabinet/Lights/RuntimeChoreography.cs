using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Presentation.PeripheralLights;
using IGT.Game.Core.Presentation.PeripheralLights.Devices;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming.Choreography;
using Midas.Presentation.Cabinet;
using Color = IGT.Game.Core.Presentation.PeripheralLights.Color;
using UnityColor = UnityEngine.Color;

namespace Midas.Ascent.Cabinet.Lights
{
	internal sealed class RuntimeChoreography : ChoreographyBase
	{
		private readonly Dictionary<CabinetType, List<LightSequence>> sequencesDict = new Dictionary<CabinetType, List<LightSequence>>();
		private readonly Dictionary<StreamingLightHardware, LightSequence> sequenceDict = new Dictionary<StreamingLightHardware, LightSequence>();
		private readonly Dictionary<string, LightSequence> idToSequenceDict = new Dictionary<string, LightSequence>();

		public IRuntimeLights RuntimeLights { get; }

		public RuntimeChoreography(IRuntimeLights runtimeLights) : base(runtimeLights.FallbackColor.ToPeripheralLightsColor())
		{
			RuntimeLights = runtimeLights;
		}

		public override string Name => RuntimeLights.Name;

		public override LightSequence GetLightSequence(string uniqueId)
		{
			idToSequenceDict.TryGetValue(uniqueId, out var sequence);
			return sequence;
		}

		protected override Choreography CreateChoreography(CabinetType cabinetType)
		{
			var duration = (float)RuntimeLights.Duration.TotalSeconds;
			var sequences = GetSequences(cabinetType);
			var step = new Step { Time = duration };

			foreach (var sequence in sequences)
				step.Actions.Add(new StreamingAction { Action = ActionType.Start, Sequence = sequence.UniqueId, GroupId = 0 });

			var choreography = new Choreography();
			choreography.Duration = duration;
			choreography.Name = RuntimeLights.Name;
			choreography.Steps.Add(step);

			return choreography;
		}

		private IReadOnlyList<LightSequence> GetSequences(CabinetType cabinetType)
		{
			if (!sequencesDict.TryGetValue(cabinetType, out var sequences))
			{
				sequences = new List<LightSequence>();

				foreach (var device in GetDevices(cabinetType))
				{
					if (!sequenceDict.TryGetValue(device, out var sequence))
					{
						sequence = CreateLightSequence(device);
						sequenceDict.Add(device, sequence);
						idToSequenceDict.Add(sequence.UniqueId, sequence);
					}

					if (sequence != null)
						sequences.Add(sequence);
				}

				sequencesDict.Add(cabinetType, sequences);
			}

			return sequences;
		}

		private LightSequence CreateLightSequence(StreamingLightHardware device)
		{
			var factory = new Factory(device);
			RuntimeLights.CreateSequence(factory);

			var sequence = new LightSequence(device, 2);
			sequence.AddSegment(factory.Segment);
			return sequence;
		}

		private static StreamingLightHardware[] GetDevices(CabinetType cabinetType)
		{
			switch (cabinetType)
			{
				case CabinetType.CrystalCoreDual:
					// Also Peak Dual / Slant
					return new[]
					{
						StreamingLightHardware.CrystalCore30EdgeLights,
						StreamingLightHardware.CrystalCoreGills,
						StreamingLightHardware.GenericBacklitTopper,
						StreamingLightHardware.StreamingDppLightRing,
						StreamingLightHardware.VideoTopper,
						StreamingLightHardware.AxxisVideoTopper,
					};
				case CabinetType.CrystalCoreDual27:
					// Unique to CD27
					return new[]
					{
						StreamingLightHardware.AxxisVideoTopper,
						StreamingLightHardware.CrystalCore27EdgeLights,
						StreamingLightHardware.CrystalCoreGills,
						StreamingLightHardware.StreamingDppLightRing,
						StreamingLightHardware.VideoTopper
					};
				case CabinetType.CrystalCurve:
					// Also Peak Curve / 49" Slant
					return new[]
					{
						StreamingLightHardware.AxxisVideoTopper,
						StreamingLightHardware.VideoTopper,
						StreamingLightHardware.CrystalCurveGills,
						StreamingLightHardware.CrystalCurveMonitorLights,
						StreamingLightHardware.StreamingDppLightRing
					};
				default:
					Log.Instance.Warn($"Cabinet type '{cabinetType}' not supported!");
					return Array.Empty<StreamingLightHardware>();
			}
		}

		private sealed class Factory : IRuntimeLightsFactory
		{
			private readonly StreamingLightHardware device;

			public Factory(StreamingLightHardware hardware)
			{
				device = hardware;
				Segment = new Segment();
				LightCount = hardware switch
				{
					StreamingLightHardware.AustraliaCrystalCoreTrim => 20,
					StreamingLightHardware.AxxisVideoTopper => 100,
					StreamingLightHardware.CrystalCore27EdgeLights => 304,
					StreamingLightHardware.CrystalCore30EdgeLights => 120,
					StreamingLightHardware.CrystalCore42EdgeLights => 160,
					StreamingLightHardware.CrystalCoreGills => 60,
					StreamingLightHardware.CrystalCurveGills => 40,
					StreamingLightHardware.CrystalCurveMonitorLights => 176,
					StreamingLightHardware.StreamingDppLightRing => 44,
					StreamingLightHardware.GenericBacklitTopper => 70,
					StreamingLightHardware.VideoTopper => 70,
					_ => throw new NotSupportedException()
				};
			}

			public Segment Segment { get; }

			public int LightCount { get; }

			public void AddFrame(IReadOnlyList<UnityColor?> frameData, TimeSpan displayTime)
			{
				AddFrame(frameData.Select(c => c?.ToPeripheralLightsColor() ?? Color.LinkedColor).ToArray(), displayTime);
			}

			public void AddAllLightsFrame(UnityColor color, TimeSpan displayTime)
			{
				var plColor = color.ToPeripheralLightsColor();
				AddFrame(Enumerable.Repeat(plColor, LightCount).ToArray(), displayTime);
			}

			public void AddFacingLightsFrame(UnityColor color, TimeSpan displayTime)
			{
				switch (device)
				{
					case StreamingLightHardware.AustraliaCrystalCoreTrim:
					case StreamingLightHardware.AxxisVideoTopper:
					case StreamingLightHardware.CrystalCore30EdgeLights:
					case StreamingLightHardware.CrystalCore42EdgeLights:
					case StreamingLightHardware.CrystalCoreGills:
					case StreamingLightHardware.CrystalCurveGills:
					case StreamingLightHardware.StreamingDppLightRing:
					case StreamingLightHardware.GenericBacklitTopper:
					case StreamingLightHardware.VideoTopper:
					{
						// No face lights!
						AddAllLightsFrame(color, displayTime);
						break;
					}
					case StreamingLightHardware.CrystalCore27EdgeLights:
					{
						var plColor = color.ToPeripheralLightsColor();
						var frameData = new List<Color>(LightCount);

						// top-front (right)
						for (var i = 0; i < 41 + 0; i++)
							frameData.Add(plColor);

						// top-back (right)
						for (var i = 41; i < 76; i++)
							frameData.Add(Color.LinkedColor);

						// top-front (left)
						for (var i = 76; i < 117; i++)
							frameData.Add(plColor);

						// top-back (left)
						for (var i = 117; i < 152; i++)
							frameData.Add(Color.LinkedColor);

						// bottom-front (right)
						for (var i = 152; i < 193; i++)
							frameData.Add(plColor);

						// bottom-back (right)
						for (var i = 193; i < 228; i++)
							frameData.Add(Color.LinkedColor);

						// bottom-front (left)
						for (var i = 228; i < 269; i++)
							frameData.Add(plColor);

						// bottom-back (left)
						for (var i = 269; i < 304; i++)
							frameData.Add(Color.LinkedColor);

						AddFrame(frameData, displayTime);
						break;
					}
					case StreamingLightHardware.CrystalCurveMonitorLights:
					{
						var frameData = new List<Color>(LightCount);

						for (var i = 0; i < 76; i++)
							frameData.Add(color.ToPeripheralLightsColor());

						AddFrame(frameData, displayTime);
						break;
					}
					default:
						throw new NotSupportedException();
				}
			}

			public void AddBackingLightsFrame(UnityColor color, TimeSpan displayTime)
			{
				switch (device)
				{
					case StreamingLightHardware.AustraliaCrystalCoreTrim:
					case StreamingLightHardware.AxxisVideoTopper:
					case StreamingLightHardware.CrystalCore30EdgeLights:
					case StreamingLightHardware.CrystalCore42EdgeLights:
					case StreamingLightHardware.CrystalCoreGills:
					case StreamingLightHardware.CrystalCurveGills:
					case StreamingLightHardware.StreamingDppLightRing:
					case StreamingLightHardware.GenericBacklitTopper:
					case StreamingLightHardware.VideoTopper:
					{
						// No backing lights!
						AddAllLightsFrame(color, displayTime);
						break;
					}

					case StreamingLightHardware.CrystalCore27EdgeLights:
					{
						var plColor = color.ToPeripheralLightsColor();
						var frameData = new List<Color>(LightCount);

						// top-front (right)
						for (var i = 0; i < 41 + 0; i++)
							frameData.Add(Color.LinkedColor);

						// top-back (right)
						for (var i = 41; i < 76; i++)
							frameData.Add(plColor);

						// top-front (left)
						for (var i = 76; i < 117; i++)
							frameData.Add(Color.LinkedColor);

						// top-back (left)
						for (var i = 117; i < 152; i++)
							frameData.Add(plColor);

						// bottom-front (right)
						for (var i = 152; i < 193; i++)
							frameData.Add(Color.LinkedColor);

						// bottom-back (right)
						for (var i = 193; i < 228; i++)
							frameData.Add(plColor);

						// bottom-front (left)
						for (var i = 228; i < 269; i++)
							frameData.Add(Color.LinkedColor);

						// bottom-back (left)
						for (var i = 269; i < 304; i++)
							frameData.Add(plColor);

						AddFrame(frameData, displayTime);
						break;
					}

					case StreamingLightHardware.CrystalCurveMonitorLights:
					{
						var frameData = new List<Color>(LightCount);

						for (var i = 0; i < 76; i++)
							frameData.Add(Color.LinkedColor);

						for (var i = 76; i < 176; i++)
							frameData.Add(color.ToPeripheralLightsColor());

						AddFrame(frameData, displayTime);
						break;
					}

					default:
						throw new NotSupportedException();
				}
			}

			private void AddFrame(IList<Color> frameData, TimeSpan displayTime)
			{
				Segment.AddFrame(new Frame(frameData) { DisplayTime = (ushort)displayTime.TotalMilliseconds });
			}
		}
	}
}