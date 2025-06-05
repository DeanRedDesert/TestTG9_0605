using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using IGT.Ascent.Communication.Platform.GameLib.Interfaces;
using IGT.Game.Core.Communication;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.RuntimeGameEvents;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.TiltManagement;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.GameMeter;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RandomNumber;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve;
using IGT.Game.Core.Communication.Foundation.Standard;
using IGT.Game.SDKAssets.AscentBuildSettings;
using Midas.Ascent.Ugp;
using Midas.Core;
using StandaloneGameLib = IGT.Game.Core.Communication.Foundation.Standalone.GameLib;

namespace Midas.Ascent
{
	public static class AscentFoundation
	{
		#region Constants

		private const string SafeStoreCommittedFile = "Com.safestorage";
		private const string SafeStoreModifiedFile = "Mod.safestorage";
		private const string FoundationSystemConfigFile = "SystemConfig.xml";
		private const string CommandLineArgumentAddressAndPort = "s";

		#endregion

		#region Static Properties

		internal static IGameLogic GameLogic { get; private set; }
		internal static IgtGameParameters GameParameters { get; private set; }
		internal static IGameLib GameLib { get; private set; }
		internal static IRawCriticalData RawCriticalData { get; private set; }
		internal static IGameLibRestricted GameLibRestricted { get; private set; }
		internal static IGameLibDemo GameLibDemo { get; private set; }
		internal static IGameLibShow GameLibShow { get; private set; }
		internal static UgpInterfaces UgpInterfaces { get; private set; }
		internal static PlayerSessionInterfaces PlayerSessionInterfaces { get; private set; }
		internal static FlashingPlayerClockInterfaces FlashingPlayerClockInterfaces { get; private set; }
		internal static GameIdentityType GlobalGameIdentity { get; private set; }
		internal static bool IsRunningInEditor { get; private set; }

		public static Exception LogicThreadException { get; internal set; }

		#endregion

		#region Public Methods

		public static void Init(IGameLogic gameLogic, GameIdentityType globalGi, bool isRunningInEditor)
		{
			Log.Instance.Info("Init");
			GlobalGameIdentity = globalGi;
			IsRunningInEditor = isRunningInEditor;

			GameLogic = gameLogic;
			LoadGameParameters(isRunningInEditor);

			var flagArgument = CommandLineArguments.Environment.GetValue(CommandLineArgumentAddressAndPort);
			if (flagArgument != null)
			{
				var fieldInfo = typeof(CommandLineArguments).GetField("flags", BindingFlags.Instance | BindingFlags.NonPublic);
				if (fieldInfo != null)
				{
					var flagsWithoutLocalhost = (Dictionary<string, string>)fieldInfo.GetValue(CommandLineArguments.Environment);
					flagsWithoutLocalhost[CommandLineArgumentAddressAndPort] = flagsWithoutLocalhost[CommandLineArgumentAddressAndPort].Replace("localhost", "127.0.0.1");
					fieldInfo.SetValue(CommandLineArguments.Environment, flagsWithoutLocalhost);
				}
			}

			if (CreateGameLib() && SetupGameLibRestricted())
			{
				SetupGameLibDemo();
				GameLibShow = GameLib as IGameLibShow;
				if (!Connect())
				{
					Log.Instance.Fatal("Unable to connect to the foundation");
					throw new Exception("Unable to connect to the foundation");
				}

				Log.Instance.Info("Connected to the foundation");

				UgpInterfaces = new UgpInterfaces();
				PlayerSessionInterfaces = new PlayerSessionInterfaces(UgpInterfaces.IsUgpFoundation);
				FlashingPlayerClockInterfaces = new FlashingPlayerClockInterfaces();
			}
			else
			{
				Log.Instance.Fatal("Unable to create game lib");
				throw new Exception("Unable to create game lib");
			}
		}

		public static void Start()
		{
			Log.Instance.Info("Start");
			AscentGameEngine.Start();
		}

		public static void Stop(ShutdownReason shutdownReason)
		{
			Log.Instance.Info("Stop");
			AscentGameEngine.Stop(shutdownReason);
		}

		public static void DeInit()
		{
			Log.Instance.Info("DeInit");
			UgpInterfaces.DeInit();
			UgpInterfaces = null;
			PlayerSessionInterfaces.DeInit();
			PlayerSessionInterfaces = null;
			FlashingPlayerClockInterfaces.DeInit();
			FlashingPlayerClockInterfaces = null;
		}

		#endregion

		#region Foundation Methods

		internal static bool IsInFoundationCallback => EventCallbackCounter != 0;

		private static int EventCallbackCounter { get; set; }

		internal static void ProcessEvents(int timeOut)
		{
			EventCallbackCounter++;
			GameLibRestricted.ProcessEvents(timeOut);
			EventCallbackCounter--;
		}

		internal static WaitHandle ProcessEvents(WaitHandle[] waitHandles)
		{
			EventCallbackCounter++;
			var handle = GameLibRestricted.ProcessEvents(waitHandles);
			EventCallbackCounter--;

			return handle;
		}

		#endregion

		#region Private Methods

		private static bool Connect()
		{
			var additionalInterfaceConfigurations = new List<IInterfaceExtensionConfiguration>();

			if (!StandaloneAustralianFoundationSettings.Exists() || StandaloneAustralianFoundationSettings.Load().MachineSettings.IsUGPFoundation)
			{
				additionalInterfaceConfigurations.AddRange(new IInterfaceExtensionConfiguration[]
				{
					new UgpExternalJackpotsInterfaceConfiguration(false),
					new UgpGameMeterInterfaceConfiguration(false),
					new UgpMachineConfigurationInterfaceConfiguration(false),
					new UgpMessageStripInterfaceConfiguration(false),
					new UgpPidInterfaceConfiguration(false),
					new UgpProgressiveAwardInterfaceConfiguration(false),
					new UgpProgressiveInterfaceConfiguration(false),
					new UgpRandomNumberInterfaceConfiguration(false),
					new UgpReserveInterfaceConfiguration(false),
					new RuntimeGameEventsInterfaceConfiguration(false),
					new GameFunctionStatusInterfaceConfiguration(false),
				});
			}

			additionalInterfaceConfigurations.Add(new PlayerSessionParametersInterfaceConfiguration(false));
			additionalInterfaceConfigurations.Add(new F2LPlayerSessionInterfaceConfiguration(false));
			additionalInterfaceConfigurations.Add(new FlashPlayerClockInterfaceConfiguration(false));
			additionalInterfaceConfigurations.Add(new TiltManagementInterfaceConfiguration(false));

			return GameLibRestricted.ConnectToFoundation(additionalInterfaceConfigurations);
		}

		private static void LoadGameParameters(bool isRunningInEditor)
		{
			var fileLocation = CommandLineArguments.Environment.GetValue("igtGameParametersFile");

			if (!string.IsNullOrEmpty(fileLocation))
			{
				Log.Instance.InfoFormat("Loading GameParameters from {0}", fileLocation);
				GameParameters = new IgtGameParameters();
				GameParameters.Load(fileLocation);
			}
			else
			{
				try
				{
					Log.Instance.Info("Loading GameParameters from the default location");
					GameParameters = IgtGameParameters.Load();
				}
				catch (FileNotFoundException e)
				{
					if (isRunningInEditor)
					{
						Log.Instance.Info("Creating default GameParameters");
						GameParameters = new IgtGameParameters();
					}
					else
					{
						Log.Instance.Fatal("Unable to load game parameters", e);
						throw;
					}
				}
			}
		}

		private static bool CreateGameLib()
		{
			Log.Instance.InfoFormat("Creating GameLib ({0})", GameParameters.Type);

			switch (GameParameters.Type)
			{
				case IgtGameParameters.GameType.Standard:
					var standard = new GameLib(GameParameters.TargetedFoundation);
					GameLib = standard;
					RawCriticalData = new StandardRawCriticalData(standard);
					break;

				case IgtGameParameters.GameType.StandaloneNoSafeStorage:
					CreateStandalone(CreateMemoryBackedGameLib());
					break;

				case IgtGameParameters.GameType.StandaloneFileBackedSafeStorage:
					CreateStandalone(CreateFileBackedGameLib(false));
					break;

				case IgtGameParameters.GameType.StandaloneBinaryFileBackedSafeStorage:
					CreateStandalone(CreateFileBackedGameLib(true));
					break;

				default:
					return false;
			}

			return true;

			void CreateStandalone(StandaloneGameLib standalone)
			{
				GameLib = standalone;
				RawCriticalData = new StandaloneRawCriticalData(standalone);
			}
		}

		private static bool SetupGameLibRestricted()
		{
			// Make sure Game Lib supports IGameLibRestricted.
			GameLibRestricted = GameLib as IGameLibRestricted;
			return GameLibRestricted != null;
		}

		private static void SetupGameLibDemo()
		{
			GameLibDemo = GameLib as IGameLibDemo;
			if (GameLibDemo != null)
			{
				switch (GameParameters.Type)
				{
					case IgtGameParameters.GameType.StandaloneNoSafeStorage:
					case IgtGameParameters.GameType.StandaloneFileBackedSafeStorage:
					case IgtGameParameters.GameType.StandaloneBinaryFileBackedSafeStorage:
						var ascentOverrideSettings = StandaloneAustralianFoundationSettings.Load().AscentOverrideSettings;
						GameLibDemo.SetShowMode(ascentOverrideSettings.ShowMode, ascentOverrideSettings.ShowEnvironment);
						break;
				}
			}
		}

		private static StandaloneGameLib CreateMemoryBackedGameLib()
		{
			using var systemConfigStream = GetFoundationConfigStream();
			return new StandaloneGameLib(true, systemConfigStream, false);
		}

		private static StandaloneGameLib CreateFileBackedGameLib(bool useBinarySafeStorage)
		{
			using var systemConfigStream = GetFoundationConfigStream();
			return new StandaloneGameLib(true, systemConfigStream, SafeStoreModifiedFile, SafeStoreCommittedFile, useBinarySafeStorage);
		}

		private static Stream GetFoundationConfigStream()
		{
			return new StreamReader(FoundationSystemConfigFile).BaseStream;
		}

		#endregion
	}
}