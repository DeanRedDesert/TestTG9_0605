// Copyright (C) 2019, IGT Australia Pty. Ltd.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.Core.Communication.Cabinet.SymbolHighlights;
using IGT.Game.Core.Presentation.PeripheralLights;
using IGT.Game.Core.Presentation.PeripheralLights.Devices;
using IgtUnityEngine;
using Midas.Ascent.Cabinet.Lights.Visualiser.Devices;
using Midas.Ascent.Cabinet.Lights.Visualiser.Simulated;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.Lights;
using Midas.Tools;
using UnityEngine;

namespace Midas.Ascent.Cabinet.Lights.Visualiser
{
	public sealed class LightsVisualiserController : MonoBehaviour, IStreamingLights
	{
		#region Fields

		private const byte DefaultIntensity = 66;
		private static readonly IDictionary<Hardware, string> streamingLightFeatureNamesLookup = HardwareSpecs.GetStreamingLightFeatureNames();
		private readonly IDictionary<string, ICollection<SimulatedLightDevice>> lightDevicesByFeatureId = new Dictionary<string, ICollection<SimulatedLightDevice>>();
		private readonly List<(LightsHandle Handle, LightsBase)> allLoadedLights = new List<(LightsHandle Handle, LightsBase Lights)>();

		private Sprite sprite;
		private Vector2 size;
		private readonly List<Action> actionList = new List<Action>();
		private int currentCabinetIndex;
		private int currentSequenceIndex = 1;
		private GameObject devicesGo;
		private GameObject cameraGo;
		private IDictionary<StreamingLightHardware, GameObject> lightGameObjects;

		/// <summary>
		/// Supported StreamingLightHardware in the LightDeviceFactory:
		/// GamesmanButtonLightRing, AustraliaCrystalCoreTrim, CrystalCore30EdgeLights, CrystalCoreGills, StreamingDppLightRing, GenericBacklitTopper
		/// VideoTopper, AxxisVideoTopper, CrystalCurveGills, CrystalCurveMonitorLights, CrystalCore27EdgeLights
		/// </summary>
		public static readonly IReadOnlyList<CabinetData> CabinetData = new List<CabinetData>
		{
			new CabinetData(
				"Peak Dual",
				CabinetType.CrystalCoreDual, // Seq files with: CC23V
				StreamingLightHardware.AxxisVideoTopper, // Topper: Runtime Axxis Video Topper
				StreamingLightHardware.CrystalCore30EdgeLights, // Body: Runtime 30" Edge Lighting
				StreamingLightHardware.StreamingDppLightRing // DPP: Runtime DPP Light Ring (DPP Light Ring)
			),
			new CabinetData(
				"Crystal Dual 27 (USA)",
				CabinetType.CrystalCoreDual27, // Seq files with: CD27
				StreamingLightHardware.AxxisVideoTopper, // Topper: Runtime Axxis Video Topper
				StreamingLightHardware.CrystalCore27EdgeLights, // Body: Runtime Crystal Edge Lighting (Legacy support for Runtime 30" Edge Lighting)
				StreamingLightHardware.CrystalCoreGills, // Base: Runtime Gill Lighting
				StreamingLightHardware.StreamingDppLightRing // DPP: Runtime DPP Light Ring
			),
			new CabinetData(
				"Crystal Dual 27 (AUS)",
				CabinetType.CrystalCoreDual27, // Seq files with: CD27CCT
				StreamingLightHardware.VideoTopper, // Topper: Runtime Video Topper (Runtime Axxis Video Topper)
				StreamingLightHardware.CrystalCore27EdgeLights, // Body: Runtime Crystal Edge Lighting (Legacy support for Runtime 30" Edge Lighting)
				//StreamingLightHardware.CrystalCoreGills, // Base: Runtime Gill Lighting
				StreamingLightHardware.StreamingDppLightRing // DPP: Runtime DPP Light Ring
			),
			new CabinetData(
				"Crystal Curve (USA & AUS)",
				CabinetType.CrystalCurve, // Seq files with: CRC
				StreamingLightHardware.AxxisVideoTopper, // Topper: Runtime Axxis Video Topper
				StreamingLightHardware.CrystalCurveMonitorLights, // Body: Runtime Crystal Curve Monitor
				StreamingLightHardware.StreamingDppLightRing // DPP: Runtime DPP Light Ring (DPP Light Ring)
			)
		};

		#endregion

		#region Properties

		public CabinetData CurrentData => CabinetData == null || CabinetData.Count == 0 ? null : CabinetData[currentCabinetIndex];

		internal static ChoreographyBase CurrentLights => AscentCabinet.ChoreographyPlayer.CurrentChoreography;

		#endregion

		#region Unity Methods

		private void Update()
		{
			var acquiredLock = false;

			try
			{
				acquiredLock = Monitor.TryEnter(actionList, 500);

				if (acquiredLock)
				{
					foreach (var action in actionList)
						action();
					actionList.Clear();
				}
			}
			finally
			{
				if (acquiredLock)
					Monitor.Exit(actionList);
			}
		}

		#endregion

		#region IStreamingLights implementation

		public byte SupportedLightVersion => 2;

		public event EventHandler<StreamingLightsNotificationEventArgs> NotificationEvent;

		byte IStreamingLights.GetIntensity(string featureId)
		{
			if (!AssertFeatureId(featureId))
				throw new MissingSimulatedLightDeviceException(featureId);

			return lightDevicesByFeatureId[featureId] != null
				? (byte)Mathf.RoundToInt((float)lightDevicesByFeatureId[featureId].Average(x => (double)x.GetIntensity()))
				: DefaultIntensity;
		}

		IEnumerable<LightFeatureDescription> IStreamingLights.GetLightDevices()
		{
			return CabinetData.SelectMany(d => d.LightHardware).Distinct().Select(GetFeatureDescription);
		}

		void IStreamingLights.StartSequenceFile(string featureId, byte groupNumber, string filePath, StreamingLightsPlayMode playMode)
		{
			lock (actionList)
			{
				actionList.Add(() =>
				{
					if (File.Exists(filePath))
						return;

					var notificationEvent = NotificationEvent;

					if (notificationEvent == null)
						return;

					notificationEvent(this, new StreamingLightsNotificationEventArgs(featureId, groupNumber, StreamingLightNotificationCode.FileNotFound));

					if (AssertFeatureId(featureId))
					{
						foreach (var x in lightDevicesByFeatureId[featureId])
							x.StartSequenceFile(groupNumber, filePath, playMode);
					}
				});
			}
		}

		void IStreamingLights.StartSequenceFile(string featureId, byte groupNumber, string sequenceName, byte[] sequenceFile,
			StreamingLightsPlayMode playMode)
		{
			lock (actionList)
			{
				actionList.Add(() =>
				{
					if (AssertFeatureId(featureId))
					{
						foreach (var x in lightDevicesByFeatureId[featureId])
							x.StartSequenceFile(groupNumber, sequenceName, sequenceFile, playMode);
					}
				});
			}
		}

		void IStreamingLights.BreakLoop(string featureId, byte groupNumber)
		{
			lock (actionList)
			{
				actionList.Add(() =>
				{
					if (AssertFeatureId(featureId))
					{
						if (lightDevicesByFeatureId.TryGetValue(featureId, out var lightDevs) && lightDevs != null)
						{
							foreach (var dev in lightDevs)
								dev.BreakLoop(groupNumber);
						}
					}
				});
			}
		}

		void IStreamingLights.SetIntensity(string featureId, byte intensity)
		{
			lock (actionList)
			{
				actionList.Add(() =>
				{
					if (AssertFeatureId(featureId))
					{
						foreach (var x in lightDevicesByFeatureId[featureId])
							x.SetIntensity(intensity);
					}
				});
			}
		}

		void IStreamingLights.SendFrameChunk(string featureId, byte groupNumber, uint frameCount, byte[] frameData,
			StreamingLightsPlayMode playMode, byte identifier)
		{
			if (AssertFeatureId(featureId))
			{
				{
					foreach (var x in lightDevicesByFeatureId[featureId])
						x.SendFrameChunk(groupNumber, frameCount, frameData, playMode);
				}
			}
		}

		void IStreamingLights.EnableGivenSymbolHighlights(
			string featureId,
			byte groupNumber,
			IEnumerable<SymbolHighlightFeature> enabledFeatures)
		{
		}

		void IStreamingLights.DisableSymbolHighlights(string featureId, byte groupNumber)
		{
		}

		void IStreamingLights.SetSymbolHighlights(string featureId, byte groupNumber, SymbolTrackingData[] trackingData, SymbolHotPositionData[] hotPositionData)
		{
		}

		void IStreamingLights.ClearSymbolHighlights(
			string featureId,
			byte groupNumber,
			IEnumerable<SymbolHighlightFeature> featuresToClear)
		{
		}

		void IStreamingLights.ClearSymbolHighlightReel(
			string featureId,
			byte groupNumber,
			int reelIndex,
			IEnumerable<SymbolHighlightFeature> featuresToClear)
		{
		}

		#endregion

		#region Public Methods

		public void Configure(Sprite ledSprite, Vector2 ledSize)
		{
			sprite = ledSprite;
			size = ledSize;
		}

		public RenderTexture GetRenderTexture()
		{
			return cameraGo ? cameraGo.GetComponent<Camera>().targetTexture : null;
		}

		public void Enable(bool enable)
		{
			if (enable)
			{
				CreateCamera();
				CreateLights();
			}

			if (cameraGo != null)
				cameraGo.SetActive(enable);

			if (devicesGo != null)
				devicesGo.SetActive(enable);

			currentCabinetIndex = CabinetData.FindIndex(d => d.CabinetType == CabinetTypeIdentifier.GetCabinetType());

			if (currentCabinetIndex < 0)
				currentCabinetIndex = 0;

			if (enable)
			{
				SimulateCurrentCabinet();

				var lightOwners = SceneHelper.GetComponentsInAllLoadedScenes<ILightOwner>(true);
				allLoadedLights.Clear();
				allLoadedLights.AddRange(lightOwners.SelectMany(lightOwner => lightOwner.Lights.Select(l => (l.Register(), l))));
			}
			else
				AscentCabinet.ChoreographyPlayer.ClearCabinetOverride();
		}

		public void SimulatePrevCabinet()
		{
			currentCabinetIndex = (currentCabinetIndex - 1 + CabinetData.Count) % CabinetData.Count;
			SimulateCurrentCabinet();
		}

		public void SimulateNextCabinet()
		{
			currentCabinetIndex = (currentCabinetIndex + 1) % CabinetData.Count;
			SimulateCurrentCabinet();
		}

		public void SimulateNextSequence()
		{
			currentSequenceIndex++;
			currentSequenceIndex %= allLoadedLights.Count;

			SimulateCurrentSequence();
		}

		public void SimulatePrevSequence()
		{
			currentSequenceIndex--;

			if (currentSequenceIndex < 0)
				currentSequenceIndex = allLoadedLights.Count - 1;

			SimulateCurrentSequence();
		}

		public void StopSimulatingSequence()
		{
			currentSequenceIndex = 0;
			AscentCabinet.ChoreographyPlayer.ClearChoreographyOverride();
		}

		#endregion

		#region Private Methods

		private void SimulateCurrentCabinet()
		{
			AscentCabinet.ChoreographyPlayer.OverrideCabinet(CurrentData.CabinetType);
			EnableLights();
		}

		private void SimulateCurrentSequence()
		{
			var currentLights = allLoadedLights[currentSequenceIndex];

			lock (actionList)
				actionList.Add(() => { AscentCabinet.ChoreographyPlayer.OverrideChoreography(currentLights.Handle); });
		}

		private void CreateLights()
		{
			if (devicesGo != null)
				return;

			// Create the devices parent
			devicesGo = GetOrInstantiateGameObject(transform, "Devices");
			devicesGo.layer = LayerMask.NameToLayer("UI");
			devicesGo.transform.localPosition = new Vector3(-18f, 0f, 0f);
			devicesGo.transform.SetParent(transform, false);

			// Create the devices
			var lightsToCreate = CabinetData.SelectMany(d => d.LightHardware).Distinct().ToArray();
			lightGameObjects = LightDeviceFactory.InstantiateDevices(devicesGo.transform, sprite, size, lightsToCreate);
			PopulateLightDevicesByFeatureIdDictionary();
		}

		private void CreateCamera()
		{
			if (cameraGo != null)
				return;

			var renderTexture = new RenderTexture(256, 512, 0, RenderTextureFormat.ARGB32);
			renderTexture.Create();

			// Create camera
			var uiLayer = LayerMask.NameToLayer("UI");
			cameraGo = GetOrInstantiateGameObject(transform, "Lights Camera");
			cameraGo.layer = uiLayer;
			cameraGo.transform.SetParent(transform, false);
			cameraGo.transform.localPosition = new Vector3(0f, 26.5f, -0.5f);
			var cam = GetOrInstantiateComponent<Camera>(cameraGo);
			cam.clearFlags = CameraClearFlags.Color;
			cam.allowHDR = false;
			cam.allowMSAA = false;
			cam.cullingMask = 1 << uiLayer;
			cam.orthographic = true;
			cam.orthographicSize = 4.5f;
			cam.aspect = 0.5f;
			cam.nearClipPlane = 0.3f;
			cam.farClipPlane = 1f;
			cam.targetDisplay = (int)MonitorRole.Main;
			cam.depth = 45f; // Puts it behind the meters (depth 50f)
			cam.eventMask = 0;
			cam.targetTexture = renderTexture;
		}

		private void EnableLights()
		{
			foreach (var kvp in lightGameObjects)
				kvp.Value.SetActive(CurrentData.LightHardware.Contains(kvp.Key));
		}

		private bool AssertFeatureId(string featureId)
		{
			if (!lightDevicesByFeatureId.ContainsKey(featureId))
			{
				PopulateLightDevicesByFeatureIdDictionary();
			}

			return lightDevicesByFeatureId.ContainsKey(featureId);
		}

		private static GameObject GetOrInstantiateGameObject(Component parent, string name)
		{
			var child = parent.transform.Find(name);
			if (child != null && child.gameObject != null)
				return child.gameObject;

			return new GameObject(name);
		}

		private static T GetOrInstantiateComponent<T>(GameObject gameObject) where T : Component
		{
			var component = gameObject.GetComponent<T>();
			return component != null
				? component
				: gameObject.AddComponent<T>();
		}

		private void PopulateLightDevicesByFeatureIdDictionary()
		{
			lightDevicesByFeatureId.Clear();

			var simulatedLightDevices = GetComponentsInChildren<SimulatedLightDevice>();
			foreach (var simulatedLightDevice in simulatedLightDevices)
			{
				var key = streamingLightFeatureNamesLookup[(Hardware)simulatedLightDevice.GetStreamingLightHardware()];
				var value = simulatedLightDevice;

				if (!lightDevicesByFeatureId.ContainsKey(key))
					lightDevicesByFeatureId.Add(key, new List<SimulatedLightDevice>());

				lightDevicesByFeatureId[key].Add(value);
			}
		}

		private static LightFeatureDescription GetFeatureDescription(StreamingLightHardware streamingLightHardware)
		{
			return new LightFeatureDescription(
				streamingLightFeatureNamesLookup[(Hardware)streamingLightHardware],
				LightSubFeature.AccentLights,
				new List<ILightGroup> { new StreamingLightGroup(0, 1, true, true) });
		}

		#endregion

		#region Sub Classes

		private sealed class MissingSimulatedLightDeviceException : Exception
		{
			public MissingSimulatedLightDeviceException(string featureId)
				: base($"Cannot find simulated light device {featureId}. Please make sure you instantiate the lights visualiser first!")
			{
			}
		}

		#endregion
	}

	public sealed class CabinetData
	{
		private readonly string title;
		public CabinetType CabinetType { get; }
		public StreamingLightHardware[] LightHardware { get; }

		public CabinetData(string title, CabinetType cabinetType, params StreamingLightHardware[] lightHardware)
		{
			this.title = title;
			CabinetType = cabinetType;
			LightHardware = lightHardware;
		}

		public string GetDescription()
		{
			var seed = string.Format("Name: {1}{0}Folder: {2}{0}Lights:{0}", Environment.NewLine, title, CabinetType);
			return LightHardware.Aggregate(seed, (s, h) => s + h + Environment.NewLine);
		}
	}
}