using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid;
using IGT.Ascent.Communication.Platform.GameLib.Interfaces;
using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Game.Core.Communication.Foundation;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus;

// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable InconsistentNaming - legacy xml serialized file, don't intend on fixing.

namespace Midas.Ascent.Ugp
{
	[Serializable]
	public class StandaloneAustralianFoundationSettings
	{
		public StandaloneAustralianMachineFoundationSettings MachineSettings = new StandaloneAustralianMachineFoundationSettings();
		public StandaloneAustralianReserveFoundationSettings ReserveSettings = new StandaloneAustralianReserveFoundationSettings();
		public StandaloneAustralianPidFoundationSettings PidSettings = new StandaloneAustralianPidFoundationSettings();
		public StandaloneAustralianExternalJackpotSettings ExternalJackpotSettings = new StandaloneAustralianExternalJackpotSettings();
		public StandaloneAustralianProgressiveAwardSettings ProgressiveAwardSettings = new StandaloneAustralianProgressiveAwardSettings();
		public StandaloneAustralianAscentOverrideSettings AscentOverrideSettings = new StandaloneAustralianAscentOverrideSettings();
		public StandaloneAustralianGameFunctionStatusSettings GameFunctionStatusSettings = new StandaloneAustralianGameFunctionStatusSettings();

		private const string SystemConfigFile = "StandaloneAustralianFoundationSettings.xml";

		public static bool Exists()
		{
			return File.Exists(SystemConfigFile);
		}

		public static StandaloneAustralianFoundationSettings Load()
		{
			if (!File.Exists(SystemConfigFile))
				return new StandaloneAustralianFoundationSettings();

			var ser = new XmlSerializer(typeof(StandaloneAustralianFoundationSettings));

			StandaloneAustralianFoundationSettings systemConfigurations;

			try
			{
				using var xmlReader = XmlReader.Create(SystemConfigFile);
				systemConfigurations = (StandaloneAustralianFoundationSettings)ser.Deserialize(xmlReader);
			}
			catch (Exception)
			{
				systemConfigurations = new StandaloneAustralianFoundationSettings();
			}

			return systemConfigurations;
		}

		public static void Save(StandaloneAustralianFoundationSettings systemConfigurations)
		{
			var ser = new XmlSerializer(typeof(StandaloneAustralianFoundationSettings));
			var settings = new XmlWriterSettings
			{
				Encoding = new UTF8Encoding(false),
				Indent = true
			};

			using var xmlWriter = XmlWriter.Create(SystemConfigFile, settings);
			ser.Serialize(xmlWriter, systemConfigurations);
			xmlWriter.Flush();
			xmlWriter.Close();
		}
	}

	[Serializable]
	public class StandaloneAustralianMachineFoundationSettings
	{
		public bool IsUGPFoundation = false;
		public bool IsClockVisible = true;
		public string ClockFormat = "h:mm tt";
		public int Tokenisation = 100;
		public int GameCycleTime = 3000;
		public bool IsContinuousPlayAllowed;
		public bool IsFeatureAutoStartEnabled;
		public bool IsSlamSpinAllowed;
		public bool IsMoreGamesEnabled;
		public long CurrentMaximumBet = 1000;
		public UgpMachineConfigurationWinCapStyle WinCapStyle = UgpMachineConfigurationWinCapStyle.None;
		public string CabinetId = "CC27AU";
		public string BrainBoxId = "AvpMe";
		public string Gpu = "Invalid";
		public int QcomJurisdiction;
	}

	[Serializable]
	public class StandaloneAustralianReserveFoundationSettings
	{
		public bool IsReserveAllowedWithCredits;
		public bool IsReserveAllowedWithoutCredits;
		public int ReserveTimeWithCreditsMilliseconds = 20000;
		public int ReserveTimeWithoutCreditsMilliseconds = 30000;

		public static StandaloneAustralianReserveFoundationSettings CreateDisabled()
		{
			return new StandaloneAustralianReserveFoundationSettings
			{
				IsReserveAllowedWithCredits = false,
				IsReserveAllowedWithoutCredits = false
			};
		}

		public static StandaloneAustralianReserveFoundationSettings CreateEnabled()
		{
			return new StandaloneAustralianReserveFoundationSettings
			{
				IsReserveAllowedWithCredits = true,
				IsReserveAllowedWithoutCredits = true,
				ReserveTimeWithCreditsMilliseconds = (int)TimeSpan.FromSeconds(180).TotalMilliseconds,
				ReserveTimeWithoutCreditsMilliseconds = (int)TimeSpan.FromSeconds(180).TotalMilliseconds
			};
		}
	}

	[Serializable]
	public class StandaloneAustralianPidFoundationSettings
	{
		public bool IsMainEntryEnabled;
		public bool IsRequestServiceEnabled;
		public GameInformationDisplayStyle Style = GameInformationDisplayStyle.Victoria;
		public SessionTrackingOption SessionTrackingOption = SessionTrackingOption.Disabled;
		public bool IsGameRulesEnabled = true;
		public int InformationMenuTimeout = 60;
		public int SessionStartMessageTimeout = 60;
		public int ViewSessionScreenTimeout = 60;
		public int ViewGameInformationTimeout = 60;
		public int ViewGameRulesTimeout = 60;
		public int ViewPayTableTimeout = 60;
		public int SessionTimeoutInterval = 60;
		public bool SessionTimeoutStartOnZeroCredits;
		public int TotalNumberLinkEnrolments;
		public float JackpotRtp;
		public bool ShowLinkJackpotCount;
		public float LinkRtpForGameRtp;

		public static StandaloneAustralianPidFoundationSettings CreateEnabled()
		{
			return new StandaloneAustralianPidFoundationSettings
			{
				IsMainEntryEnabled = true,
				IsRequestServiceEnabled = false,
				Style = GameInformationDisplayStyle.Victoria,
				SessionTrackingOption = SessionTrackingOption.PlayerControlled,
				SessionTimeoutStartOnZeroCredits = true
			};
		}

		public PidConfiguration ToPidConfiguration()
		{
			return new PidConfiguration
			{
				IsMainEntryEnabled = IsMainEntryEnabled,
				IsRequestServiceEnabled = IsRequestServiceEnabled,
				GameInformationDisplayStyle = Style,
				SessionTrackingOption = SessionTrackingOption,
				IsGameRulesEnabled = IsGameRulesEnabled,
				InformationMenuTimeout = TimeSpan.FromSeconds(InformationMenuTimeout),
				SessionStartMessageTimeout = TimeSpan.FromSeconds(SessionStartMessageTimeout),
				ViewSessionScreenTimeout = TimeSpan.FromSeconds(ViewSessionScreenTimeout),
				ViewGameInformationTimeout = TimeSpan.FromSeconds(ViewGameInformationTimeout),
				ViewGameRulesTimeout = TimeSpan.FromSeconds(ViewGameRulesTimeout),
				ViewPayTableTimeout = TimeSpan.FromSeconds(ViewPayTableTimeout),
				SessionTimeoutInterval = TimeSpan.FromSeconds(SessionTimeoutInterval),
				SessionTimeoutStartOnZeroCredits = SessionTimeoutStartOnZeroCredits,
				TotalNumberLinkEnrolments = (ushort)TotalNumberLinkEnrolments,
				TotalLinkPercentageContributions = JackpotRtp.ToString("F2"),
				ShowLinkJackpotCount = ShowLinkJackpotCount,
				LinkRtpForGameRtp = LinkRtpForGameRtp
			};
		}
	}

	[Serializable]
	public class StandaloneAustralianGameFunctionStatusSettings
	{
		public uint ConfiguredDenominationMenuTimeout;
		public bool ActiveTimeout;

		public List<DenominationPlayableStatus> DenominationPlayableStatuses;
		public List<GameButtonBehavior> GameButtonBehaviours;

		public DenominationMenuTimeoutConfiguration ToDenominationMenuTimeoutConfiguration() => new DenominationMenuTimeoutConfiguration(ConfiguredDenominationMenuTimeout, ActiveTimeout);
	}

	[Serializable]
	public class StandaloneAustralianExternalJackpotSettings
	{
		public bool IsVisible;
		public bool IsIconVisible;
		public int NumberOfJackpots = 2;

		public List<ExternalJackpot> ToExternalJackpots()
		{
			return Enumerable.Range(0, NumberOfJackpots).Select(i => new ExternalJackpot { IconId = 0, IsVisible = true, Name = "ABCDEFGHIJKLMN" + i, Value = "$34545.44" }).ToList();
		}
	}

	[Serializable]
	public class StandaloneAustralianProgressiveAwardSettings
	{
		public bool AutoAward = true;
		public int AwardAmount = 100000;
	}

	[Serializable]
	public class StandaloneAustralianAscentOverrideSettings
	{
		public bool ShowMode = true;
		public ShowEnvironment ShowEnvironment = ShowEnvironment.Development;
		public long ShowMinimumCredits;
		public StandaloneAustralianDefaultBetOptions DefaultBetOptions = StandaloneAustralianDefaultBetOptions.MinBet;
		public float DefaultVolume = 0.54f;
	}

	[Serializable]
	public class StandaloneAustralianDefaultBetOptions
	{
		public BetSelectionBehavior NumberOfSubsets = BetSelectionBehavior.Minimum;
		public BetSelectionBehavior BetPerSubset = BetSelectionBehavior.Minimum;
		public SideBetSelectionBehavior SideBet = SideBetSelectionBehavior.Undefined;

		public static StandaloneAustralianDefaultBetOptions MinBet = new StandaloneAustralianDefaultBetOptions { NumberOfSubsets = BetSelectionBehavior.Minimum, BetPerSubset = BetSelectionBehavior.Minimum };
		public static StandaloneAustralianDefaultBetOptions MaxBet = new StandaloneAustralianDefaultBetOptions { NumberOfSubsets = BetSelectionBehavior.Maximum, BetPerSubset = BetSelectionBehavior.Maximum };
	}
}