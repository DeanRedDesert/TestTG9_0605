using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IGT.Game.Core.Presentation.CabinetServices;
using IGT.Game.Core.Presentation.PeripheralLights;
using IGT.Game.Core.Presentation.PeripheralLights.Devices;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming.Choreography;
using IGT.Game.SDKAssets.AscentBuildSettings;
using Midas.Core.Coroutine;
using Midas.Presentation;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Lights;

namespace Midas.Ascent.Cabinet.Lights
{
	internal sealed class ChoreographyPlayer : CabinetDevice
	{
		#region Fields

		private const float Epsilon = 0.0001f;
		private const string ChoreographyDisabledReason = "ChoreographyPlayer.Initialize must be called before playing.";

		private CabinetType? cabinetType;
		private CabinetType? cabinetTypeOverride;
		private IPeripheralLightService peripheralLightService;

		private readonly Dictionary<LightsHandle, ChoreographyBase> choreographyCache = new Dictionary<LightsHandle, ChoreographyBase>();
		private readonly Dictionary<string, LightSequencePacket> lightSequenceCache = new Dictionary<string, LightSequencePacket>();

		private Coroutine playCoroutine;
		private ChoreographyBase currentChoreography;
		private bool currentChoreographyLooping;
		private ChoreographyBase choreographyOverride;

		#endregion

		#region Properties

		internal ChoreographyBase CurrentChoreography => choreographyOverride ?? currentChoreography;
		private bool HasKnownCabinet => cabinetType.HasValue && cabinetType.Value != CabinetType.Unknown;

		#endregion

		#region Public Methods

		public ChoreographyPlayer()
			: base(DeviceType.StreamingLight)
		{
		}

		public void OverrideCabinet(CabinetType newCabinetType)
		{
			var foundationType = AscentFoundation.GameParameters.Type;
			if (foundationType == IgtGameParameters.GameType.Standard || foundationType == IgtGameParameters.GameType.UniversalController)
				return;

			cabinetTypeOverride = newCabinetType;
			if (CurrentChoreography != null)
				PlayCurrent(choreographyOverride != null || currentChoreographyLooping);
		}

		public void OverrideChoreography(LightsHandle lightsHandle)
		{
			if (!HasKnownCabinet)
				return;

			if (choreographyCache.TryGetValue(lightsHandle, out var lights))
			{
				choreographyOverride = lights;
				PlayCurrent(true);
			}
		}

		public void ClearCabinetOverride()
		{
			cabinetTypeOverride = null;
		}

		public void ClearChoreographyOverride()
		{
			if (choreographyOverride == null)
				return;

			choreographyOverride = null;
			playCoroutine?.Stop();

			if (!HasKnownCabinet)
				return;

			if (currentChoreography != null)
				PlayCurrent(currentChoreographyLooping);
		}

		public LightsHandle AddLights(IStreamingLights streamingLights)
		{
			foreach (var choreography in choreographyCache)
			{
				if (choreography.Value is StreamingChoreography sc && ReferenceEquals(sc.StreamingLights, streamingLights))
					return choreography.Key;
			}

			var lights = new StreamingChoreography(streamingLights);
			var handle = new LightsHandle();

			choreographyCache.Add(handle, lights);
			return handle;
		}

		public LightsHandle AddLights(IRuntimeLights runtimeLights)
		{
			foreach (var choreography in choreographyCache)
			{
				if (choreography.Value is RuntimeChoreography sc && ReferenceEquals(sc.RuntimeLights, runtimeLights))
					return choreography.Key;
			}

			var lights = new RuntimeChoreography(runtimeLights);
			var handle = new LightsHandle();

			choreographyCache.Add(handle, lights);
			return handle;
		}

		public TimeSpan Play(LightsHandle lightsHandle, bool loop)
		{
			if (!HasKnownCabinet)
				return TimeSpan.Zero;

			if (choreographyCache.TryGetValue(lightsHandle, out var lights))
			{
				currentChoreography = lights;
				currentChoreographyLooping = loop;
				return choreographyOverride == null
					? PlayCurrent(loop)
					: TimeSpan.FromSeconds(lights.GetChoreography(cabinetType!.Value).Duration);
			}

			Log.Instance.Error("Lights handle not registered");
			return TimeSpan.Zero;
		}

		public void Stop(bool clearLights)
		{
			Log.Instance.Info("Stop called");
			playCoroutine?.Stop();
			currentChoreography = null;
			currentChoreographyLooping = false;

			if (clearLights)
				AscentCabinet.PeripheralLights.SetColor(Color.Black);
		}

		public override void Init()
		{
			base.Init();
			peripheralLightService = CabinetServiceLocator.Instance.GetService<IPeripheralLightService>();
			InitSequenceCache(AscentFoundation.GameLib.GameMountPoint);
		}

		public override void Pause()
		{
			Log.Instance.Info("Pause");
			playCoroutine?.Stop();
			cabinetType = null;
			base.Pause();
		}

		public override void Resume()
		{
			Log.Instance.Info("Resume");
			base.Resume();
			cabinetType = CabinetTypeIdentifier.GetCabinetType();
		}

		public override void DeInit()
		{
			Stop(false);
			cabinetType = null;

			lightSequenceCache.Clear();
			choreographyCache.Clear();
			peripheralLightService = null;

			base.DeInit();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Plays a choreography file
		/// </summary>
		/// <param name="loop">If the choreography file should loop or not.</param>
		/// <returns>The total duration of the specified choreography in seconds.</returns>
		private TimeSpan PlayCurrent(bool loop)
		{
			playCoroutine?.Stop();

			var lights = CurrentChoreography;

			AscentCabinet.PeripheralLights.SetColor(lights.FallbackColor);

			var ct = cabinetType;
			var foundationType = AscentFoundation.GameParameters.Type;
			if (foundationType != IgtGameParameters.GameType.Standard && foundationType != IgtGameParameters.GameType.UniversalController && cabinetTypeOverride != null)
				ct = cabinetTypeOverride;

			Debug.Assert(ct != null, nameof(ct) + " != null");
			var choreography = lights.GetChoreography(ct.Value);

			if (Log.Instance.IsInfoEnabled)
			{
				Log.Instance.Info($"Playing id={choreography.Name}, looping={loop}");
			}

			var choreographyDuration = TimeSpan.Zero;

			if (HasKnownCabinet)
			{
				if (choreography.Steps.Any())
				{
					choreographyDuration = TimeSpan.FromSeconds(choreography.Duration);
					playCoroutine = FrameUpdateService.Update.StartCoroutine(PlaySteps(choreography, loop));
				}
				else
				{
					Log.Instance.Warn($"Choreography file {choreography.Name} contains no steps.");
				}
			}
			else
			{
				Log.Instance.Warn("The Streaming Choreography feature is disabled: " + ChoreographyDisabledReason);
			}

			Log.Instance.InfoFormat("Playing id={0}, looping={1} done, choreographyDuration={2}", choreography.Name, loop, choreographyDuration);

			return choreographyDuration;
		}

		/// <summary>
		/// The coroutine that plays a choreography file by iterating over the <see cref="Step" />s and their <see cref="StreamingAction" />s.
		/// </summary>
		/// <param name="choreography">The choreography that will be played.</param>
		/// <param name="loop">If the choreography file should loop or not.</param>
		/// <returns>A Task which can be awaited/>.</returns>
		private IEnumerator<CoroutineInstruction> PlaySteps(Choreography choreography, bool loop)
		{
			var filteredActions = new List<(StreamingLightHardware Hardware, StreamingAction Action)>();

			do
			{
				var letLightSequenceLoop = loop && choreography.Steps.Count == 1;
				foreach (var step in choreography.Steps)
				{
					if (step.Time < Epsilon)
					{
						Log.Instance.Warn($"Choreography file {choreography.Name} contains invalid data. One of the steps has a runtime of 0 seconds.");
						loop = false;
						break;
					}

					filteredActions.Clear();
					var looping = PlayStep(step, letLightSequenceLoop, filteredActions);
					yield return new CoroutineDelay(looping ? TimeSpan.FromSeconds(step.Time * 100) : TimeSpan.FromSeconds(step.Time));
				}
			} while (loop);
		}

		private bool PlayStep(Step step, bool letLightSequenceLoop, List<(StreamingLightHardware Hardware, StreamingAction Action)> filteredActions)
		{
			// Search one Action action per Hardware, because duplicate actions on same hardware would be useless

			foreach (var action in step.Actions)
			{
				var hardwareAndLightSequence = GetLightSequence(action);
				if (filteredActions.FindIndex(p => p.Hardware == hardwareAndLightSequence.Hardware) == -1)
					filteredActions.Add((hardwareAndLightSequence.Hardware, action));
			}

			// Ensure that CrystalCore27EdgeLights(94) goes before CrystalCore30EdgeLights(26)

			filteredActions.Sort((a, b) => b.Hardware - a.Hardware);

			// If CrystalCore27EdgeLights = 94, action could be played, then no CrystalCore30EdgeLights = 26, should be played

			var looping = false;
			var crystalCore27EdgeLightsFound = false;
			var atLeastOneFound = false;
			foreach (var (hardware, action) in filteredActions)
			{
				if (hardware == StreamingLightHardware.CrystalCore30EdgeLights && crystalCore27EdgeLightsFound)
					continue;

				var result = PlayAction(action, letLightSequenceLoop);
				if (result.found)
				{
					atLeastOneFound = true;
					looping |= result.looping;
					if (hardware == StreamingLightHardware.CrystalCore27EdgeLights)
					{
						crystalCore27EdgeLightsFound = true;
					}
				}
			}

			if (!atLeastOneFound)
			{
				Log.Instance.Info("No FrameLightDevice found for playing step");
			}

			return looping;
		}

		private (bool looping, bool found) PlayAction(StreamingAction action, bool letLightSequenceLoop)
		{
			var looping = false;
			var found = false;

			if (action.Action == ActionType.Start)
			{
				try
				{
					var (hardware, lightSequence) = GetLightSequence(action);
					if (letLightSequenceLoop)
					{
						lightSequence.Loop = true;
					}

					if (TryGetDevice(hardware, out var lightDevice))
					{
						looping = lightSequence.Loop;
						lightDevice.PlayLightSequence((byte)action.GroupId, lightSequence);
						found = true;
					}
				}
				catch (Exception e)
				{
					Log.Instance.Warn($"PlayLightSequence received exception of type='{e.GetType().Name}' Exception={e}", e);
					if (e is CoupledLightException)
					{
						Log.Instance.Warn("PlayLightSequence throws CoupledLightException");
					}
				}
			}

			return (looping, found);
		}

		private (StreamingLightHardware Hardware, LightSequence) GetLightSequence(StreamingAction action)
		{
			StreamingLightHardware hardware;
			LightSequence lightSequence;
			if (!lightSequenceCache.TryGetValue(action.Sequence, out var packet))
			{
				lightSequence = CurrentChoreography.GetLightSequence(action.Sequence);
				if (lightSequence == null)
					throw new Exception("Unable to find sequence with unique ID " + action.Sequence);
				hardware = lightSequence.LightDevice;
				packet = new LightSequencePacket(hardware, lightSequence, LightSequenceLoopingBehaviour.Unknown);
				lightSequenceCache.Add(lightSequence.UniqueId, packet);
			}
			else if (packet.IsSequenceFileName)
			{
				using var fileStream = new FileStream(packet.SequenceFileName, FileMode.Open, FileAccess.Read);
				lightSequence = new LightSequence(fileStream);
				hardware = lightSequence.LightDevice;
				var originalLightSequenceLoop = lightSequence.Loop ? LightSequenceLoopingBehaviour.Looping : LightSequenceLoopingBehaviour.NonLooping;
				lightSequenceCache[action.Sequence] = new LightSequencePacket(hardware, lightSequence, originalLightSequenceLoop);
			}
			else
			{
				hardware = packet.Hardware;
				lightSequence = packet.Sequence;
				//restore LightSequence.Loop which may have been modified by previous playActions
				lightSequence.Loop = packet.OriginalLightSequenceLoopingBehaviour == LightSequenceLoopingBehaviour.Looping;
			}

			return (hardware, lightSequence);
		}

		/// <summary>
		/// Tries to get the streaming light device specified.
		/// </summary>
		/// <param name="deviceType">The device to access.</param>
		/// <param name="device">The instance to the object representing the device.</param>
		/// <returns>True if the device was obtained. (This doesn't mean it was acquired.)</returns>
		private bool TryGetDevice(StreamingLightHardware deviceType, out UsbStreamingLight device)
		{
			device = null;
			if (deviceType == StreamingLightHardware.Unknown)
			{
				return false;
			}

			if (!AscentCabinet.CabinetLib.IsConnected)
			{
				Log.Instance.Info("CabinetLib.IsConnected==FALSE");
				return false;
			}

			try
			{
				device = peripheralLightService.GetPeripheralLight(deviceType, true);
				return device?.FeatureDescription != null;
			}
			catch (CabinetDoesNotSupportLightsInterfaceException e)
			{
				Log.Instance.Info($"DeviceType='{deviceType}' not supported Exception={e}", e);
			}
			catch (CoupledLightException e)
			{
				Log.Instance.Warn($"CoupledLightException for device='{deviceType}' Exception={e}", e);
			}
			catch (Exception e)
			{
				Log.Instance.Info($"GetPeripheralLight for device='{deviceType}' throws Exception={e}", e);
			}

			return false;
		}

		private void InitSequenceCache(string gameDirectory)
		{
			var sequenceFiles = LightSequenceIndex.LoadIndex(AscentFoundation.GameLib.GameMountPoint);
			lightSequenceCache.Clear();

			foreach (var sequencePair in sequenceFiles)
			{
				if (!lightSequenceCache.ContainsKey(sequencePair.Key))
				{
					var sequenceFileName = sequencePair.Value;
					if (!Path.IsPathRooted(sequenceFileName))
					{
						sequenceFileName = Path.Combine(gameDirectory, sequenceFileName);
					}

					lightSequenceCache.Add(sequencePair.Key, new LightSequencePacket(sequenceFileName));
				}
			}
		}

		#endregion
	}
}