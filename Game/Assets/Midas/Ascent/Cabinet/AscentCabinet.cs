using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IGT.Game.Core.Communication.Cabinet;
using IGT.Game.Core.Communication.Cabinet.CSI.Schemas;
using IGT.Game.SDKAssets.AscentBuildSettings;
using Midas.Ascent.Cabinet.Lights;
using Midas.Core;
using Midas.Presentation.Cabinet;
using MonitorAspect = IGT.Game.Core.Communication.Cabinet.CSI.Schemas.MonitorAspect;
using MonitorRole = IGT.Game.Core.Communication.Cabinet.CSI.Schemas.MonitorRole;
using StandaloneCabinetLib = IGT.Game.Core.Communication.Cabinet.Standalone.CabinetLib;
using StandardCabinetLib = IGT.Game.Core.Communication.Cabinet.Standard.CabinetLib;

namespace Midas.Ascent.Cabinet
{
	public static class AscentCabinet
	{
		#region Nested Type: CabSubsystem

		private sealed class CabinetSubsystem : IGamingSubsystem
		{
			private readonly IReadOnlyDictionary<Type, object> standaloneCabinetInterfaces;

			public CabinetSubsystem(IReadOnlyDictionary<Type, object> standaloneCabinetInterfaces)
			{
				this.standaloneCabinetInterfaces = standaloneCabinetInterfaces;
			}

			public string Name => nameof(AscentCabinet);

			void IGamingSubsystem.Init()
			{
				Init(standaloneCabinetInterfaces);
			}

			void IGamingSubsystem.OnStart()
			{
			}

			void IGamingSubsystem.OnBeforeLoadGame()
			{
				OnBeforeLoadGame();
			}

			void IGamingSubsystem.OnAfterUnloadGame()
			{
				OnAfterUnloadGame();
			}

			void IGamingSubsystem.OnStop()
			{
			}

			void IGamingSubsystem.DeInit()
			{
				DeInit();
			}
		}

		#endregion

		private const string CsiConfigFile = "CsiConfig.xml";
		private static IReadOnlyList<ICabinetController> controllers;

		internal static ICabinetLib CabinetLib { get; private set; }

		internal static bool IsActive { get; private set; }

		internal static IReadOnlyList<Monitor> MonitorConfigurations { get; private set; }

		internal static VolumeController VolumeController { get; private set; }
		internal static ServiceController ServiceController { get; private set; }
		internal static ButtonPanelDevice ButtonPanel { get; private set; }

		internal static ChoreographyPlayer ChoreographyPlayer { get; private set; }
		internal static PeripheralLights PeripheralLights { get; private set; }

		/// <summary>
		/// Sends an update into the CabinetLib.
		/// </summary>
		public static void Update()
		{
			if (CabinetLib.IsConnected)
				CabinetLib.Update();
		}

		/// <summary>
		/// Creates the AscentCabinet subsystem.
		/// </summary>
		/// <param name="standaloneCabinetInterfaces">The standalone interfaces.</param>
		/// <returns>A cabinet shim for use by the rest of the system.</returns>
		public static ICabinetShim Create(IReadOnlyDictionary<Type, object> standaloneCabinetInterfaces)
		{
			return new AscentCabinetShim(new CabinetSubsystem(standaloneCabinetInterfaces));
		}

		private static void Init(IReadOnlyDictionary<Type, object> standaloneCabinetInterfaces)
		{
			if (CreateCabinetLib(standaloneCabinetInterfaces))
			{
				VolumeController = new VolumeController();
				ServiceController = new ServiceController();
				ButtonPanel = new ButtonPanelDevice();
				ChoreographyPlayer = new ChoreographyPlayer();
				PeripheralLights = new PeripheralLights();

				controllers = new ICabinetController[]
				{
					new CabinetServicesController(),
					VolumeController,
					ServiceController,
					ButtonPanel,
					ChoreographyPlayer,
					PeripheralLights,
					new WindowVisibilityController(),
					new ParkController()
				};

				foreach (var cabinetController in controllers)
					cabinetController.Init();

				Resume();
			}
		}

		private static void OnBeforeLoadGame()
		{
			foreach (var cabinetController in controllers)
				cabinetController.OnBeforeLoadGame();
		}

		internal static void Resume()
		{
			Log.Instance.Info("Resume ...");
			if (Connect())
			{
				InitReadiness();
				RequestMonitorConfigurations();
				RegisterForEvents();

				foreach (var cabinetController in controllers)
					cabinetController.Resume();

				Log.Instance.Info("Resume done.");
				IsActive = true;
				return;
			}

			Log.Instance.Fatal("Resume failed.");
		}

		internal static void Pause()
		{
			Log.Instance.Info("Pause ...");
			IsActive = false;

			if (CabinetLib is { IsConnected: true })
			{
				UnRegisterFromEvents();

				foreach (var cabinetController in controllers)
				{
					cabinetController.Pause();
				}

				Log.Instance.Info("CabinetLib.Disconnect");
				CabinetLib.Disconnect();
			}
			else
			{
				Log.Instance.Info("Not connected.");
			}

			Log.Instance.Info("Pause done.");
		}

		private static void OnAfterUnloadGame()
		{
			foreach (var cabinetController in controllers)
				cabinetController.OnAfterUnLoadGame();
		}

		private static void DeInit()
		{
			Log.Instance.Info("DeInit ...");
			Pause();

			foreach (var cabinetController in controllers)
				cabinetController.DeInit();

			controllers = null;
			VolumeController = null;

			(CabinetLib as IDisposable)?.Dispose();
			CabinetLib = null;
			Log.Instance.Info("DeInit done.");
		}

		private static void InitReadiness()
		{
			var readiness = CabinetLib.GetInterface<IReadiness>();
			if (readiness != null)
			{
				Log.Instance.Info("Init readiness with ReadyState.NotReadyForDisplay");
				readiness.SetReadyState(ReadyState.NotReadyForDisplay);
			}
			else
			{
				Log.Instance.Info("Readiness not available");
			}
		}

		private static bool Connect()
		{
			Log.Instance.Info("Connecting ...");

			if (!CabinetLib.IsConnected)
				CabinetLib.Connect();

			Log.Instance.Info($"Connecting result={CabinetLib.IsConnected} done.");
			return CabinetLib.IsConnected;
		}

		private static void RegisterForEvents()
		{
			CabinetLib.AttractAestheticConfigurationChangedEvent += OnAttractAestheticConfigurationChangedEvent;
			CabinetLib.DeviceAcquiredEvent += OnDeviceAcquiredEvent;
			CabinetLib.DeviceReleasedEvent += OnDeviceReleasedEvent;
			CabinetLib.DeviceConnectedEvent += OnDeviceConnectedEvent;
			CabinetLib.DeviceRemovedEvent += OnDeviceRemovedEvent;
		}

		private static void UnRegisterFromEvents()
		{
			CabinetLib.AttractAestheticConfigurationChangedEvent -= OnAttractAestheticConfigurationChangedEvent;
			CabinetLib.DeviceAcquiredEvent -= OnDeviceAcquiredEvent;
			CabinetLib.DeviceReleasedEvent -= OnDeviceReleasedEvent;
			CabinetLib.DeviceConnectedEvent -= OnDeviceConnectedEvent;
			CabinetLib.DeviceRemovedEvent -= OnDeviceRemovedEvent;
		}

		private static void OnDeviceRemovedEvent(object sender, DeviceRemovedEventArgs e)
		{
			Log.Instance.Info(e);
		}

		private static void OnDeviceConnectedEvent(object sender, DeviceConnectedEventArgs e)
		{
			Log.Instance.Info(e);
		}

		private static void OnDeviceReleasedEvent(object sender, DeviceReleasedEventArgs e)
		{
			Log.Instance.Info(e);
		}

		private static void OnDeviceAcquiredEvent(object sender, DeviceAcquiredEventArgs e)
		{
			Log.Instance.Info(e);
		}

		private static void OnAttractAestheticConfigurationChangedEvent(object sender, AttractAestheticConfigurationEventArgs e)
		{
			Log.Instance.Info(e);
		}

		private static bool CreateCabinetLib(IReadOnlyDictionary<Type, object> standaloneCabinetInterfaces)
		{
			Log.Instance.InfoFormat("Creating CabinetLib Type: {0}", AscentFoundation.GameParameters.Type);

			try
			{
				switch (AscentFoundation.GameParameters.Type)
				{
					case IgtGameParameters.GameType.Standard:
					{
						CabinetLib = new StandardCabinetLib("127.0.0.1", 9012, AscentFoundation.GameLibRestricted.Token, ClientType.Game, AscentFoundation.GameParameters.TargetedFoundation);
						break;
					}

					case IgtGameParameters.GameType.StandaloneNoSafeStorage:
					case IgtGameParameters.GameType.StandaloneFileBackedSafeStorage:
					case IgtGameParameters.GameType.StandaloneBinaryFileBackedSafeStorage:
					{
						using var systemConfigStream = File.OpenRead(CsiConfigFile);
						CabinetLib = new StandaloneCabinetLib(systemConfigStream, standaloneCabinetInterfaces?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
						break;
					}

					default:
						Log.Instance.ErrorFormat("The configured game type '{0}' is not supported.", AscentFoundation.GameParameters.Type);
						CabinetLib = null;
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Instance.Error("Failed creating CabinetLib", ex);
				CabinetLib = null;
			}

			return CabinetLib != null;
		}

		private static void RequestMonitorConfigurations()
		{
			// Set the static monitor configurations after monitor is configured.
			var monitorComposition = CabinetLib.RequestMonitorConfiguration();
			MonitorConfigurations = monitorComposition.Monitors.ToArray();
			CabinetManager.Cabinet.SetMonitorConfigs(MonitorConfigurations.Select(mc => new MonitorConfig(ConvertRole(mc.Role), ConvertAspect(mc.Aspect))).ToList());
			Log.Instance.Info($"Received {MonitorConfigurations.Count} monitor configurations");

			Midas.Presentation.Cabinet.MonitorRole ConvertRole(MonitorRole r)
			{
				switch (r)
				{
					case MonitorRole.Main: return Presentation.Cabinet.MonitorRole.Main;
					case MonitorRole.ButtonPanel: return Presentation.Cabinet.MonitorRole.ButtonPanel;
					case MonitorRole.Top: return Presentation.Cabinet.MonitorRole.Top;
					case MonitorRole.Topper: return Presentation.Cabinet.MonitorRole.Topper;
					case MonitorRole.BellyGlass: return Presentation.Cabinet.MonitorRole.BellyGlass;
					case MonitorRole.VideoWall: return Presentation.Cabinet.MonitorRole.VideoWall;
					default:
						return Presentation.Cabinet.MonitorRole.Main;
				}
			}

			Midas.Presentation.Cabinet.MonitorAspect ConvertAspect(MonitorAspect r)
			{
				switch (r)
				{
					case MonitorAspect.Standard: return Presentation.Cabinet.MonitorAspect.Standard;
					case MonitorAspect.Portrait: return Presentation.Cabinet.MonitorAspect.Portrait;
					case MonitorAspect.Ultrawide: return Presentation.Cabinet.MonitorAspect.Ultrawide;
					case MonitorAspect.Widescreen: return Presentation.Cabinet.MonitorAspect.Widescreen;
					default:
						return Presentation.Cabinet.MonitorAspect.Standard;
				}
			}
		}
	}
}