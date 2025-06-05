using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid;
using IGT.Game.Core.Communication.Standalone.Schemas;
using Midas.Ascent.Ugp;
using Midas.Core.General;
using UnityEditor;
using UnityEngine;

namespace Midas.Ascent.Editor.Ugp
{
	public sealed class JurisdictionData
	{
		public bool IsUgpFoundation = true;
		public long WinCapLimit = 1000000;
		public Money TokenValue = Money.FromMinorCurrency(100);
		public UgpMachineConfigurationWinCapStyle WinCapStyle = UgpMachineConfigurationWinCapStyle.None;
		public bool IsClockVisible = true;
		public string ClockFormat = "h:mm tt";
		public int GameCycleTime = 3000;
		public long CurrentMaximumBet = 0xFFFFFFFF;
		public QcomJurisdictions? QcomJurisdiction;

		public StandaloneAustralianReserveFoundationSettings ReserveSettings = StandaloneAustralianReserveFoundationSettings.CreateDisabled();
		public StandaloneAustralianPidFoundationSettings PidSettings = new StandaloneAustralianPidFoundationSettings();
		public StandaloneAustralianDefaultBetOptions DefaultBetOptions = StandaloneAustralianDefaultBetOptions.MinBet;

		public readonly StandaloneAustralianGameFunctionStatusSettings GameFunctionStatusSettings = new StandaloneAustralianGameFunctionStatusSettings
		{
			ConfiguredDenominationMenuTimeout = (uint)TimeSpan.Zero.TotalMilliseconds,
			ActiveTimeout = false,
			//The denoms can't be determined here and also vary by game, so they get set elsewhere
			DenominationPlayableStatuses = new List<DenominationPlayableStatus>(),
			GameButtonBehaviours = new List<GameButtonBehavior>(Enum.GetValues(typeof(GameButtonTypeEnum))
				.Cast<GameButtonTypeEnum>()
				.Select(o => new GameButtonBehavior(o, GameButtonStatus.Active)))
		};
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum QcomJurisdictions
	{
		QLD_Clubs,
		QLD_Casinos,
		NZ,
		VIC,
		SA,
		TAS
	}

	[InitializeOnLoad]
	internal static class MachineConfigurationData
	{
		private static readonly SystemConfigurations systemConfigurations;
		private static readonly Dictionary<Jurisdictions, JurisdictionData> jurisdictionData = new Dictionary<Jurisdictions, JurisdictionData>();

		public static StandaloneAustralianFoundationSettings AustralianFoundationSettings { get; private set; }
		public static Jurisdictions LastJurisdiction { get; private set; }

		static MachineConfigurationData()
		{
			systemConfigurations = SystemConfigurationHelper.Load();
			LastJurisdiction = GetCurrentJurisdiction();
			AustralianFoundationSettings = StandaloneAustralianFoundationSettings.Load();

			var usdm = new JurisdictionData { IsUgpFoundation = false, IsClockVisible = false, ClockFormat = string.Empty, DefaultBetOptions = StandaloneAustralianDefaultBetOptions.MaxBet };
			usdm.PidSettings.IsMainEntryEnabled = false;
			jurisdictionData.Add(Jurisdictions.USDM, usdm);
			jurisdictionData.Add(Jurisdictions.SING, usdm);
			jurisdictionData.Add(Jurisdictions.INTL, usdm);
			jurisdictionData.Add(Jurisdictions.MCAU, usdm);

			jurisdictionData.Add(Jurisdictions.NSW, new JurisdictionData { ReserveSettings = StandaloneAustralianReserveFoundationSettings.CreateEnabled() });
			jurisdictionData.Add(Jurisdictions.SCC, new JurisdictionData { ReserveSettings = StandaloneAustralianReserveFoundationSettings.CreateEnabled() });

			var nzp = new JurisdictionData
			{
				TokenValue = Money.FromMinorCurrency(200),
				CurrentMaximumBet = 250,
				WinCapLimit = 50000,
				WinCapStyle = UgpMachineConfigurationWinCapStyle.ClipAndBreakout,
				PidSettings = StandaloneAustralianPidFoundationSettings.CreateEnabled(),
				QcomJurisdiction = QcomJurisdictions.NZ
			};
			nzp.PidSettings.SessionTrackingOption = SessionTrackingOption.Viewable;
			nzp.PidSettings.IsRequestServiceEnabled = false;
			jurisdictionData.Add(Jurisdictions.NZP, nzp);

			var nzc = new JurisdictionData
			{
				TokenValue = Money.FromMinorCurrency(200),
				PidSettings = StandaloneAustralianPidFoundationSettings.CreateEnabled(),
				QcomJurisdiction = QcomJurisdictions.NZ
			};
			nzc.PidSettings.SessionTrackingOption = SessionTrackingOption.Viewable;
			nzc.PidSettings.IsRequestServiceEnabled = true;
			jurisdictionData.Add(Jurisdictions.NZC, nzc);

			var crn = new JurisdictionData
			{
				TokenValue = Money.FromMinorCurrency(100),
				GameCycleTime = 2700,
				CurrentMaximumBet = 1000,
				PidSettings = StandaloneAustralianPidFoundationSettings.CreateEnabled()
			};
			crn.PidSettings.IsRequestServiceEnabled = true;
			jurisdictionData.Add(Jurisdictions.CRN, crn);

			var vsi = new JurisdictionData
			{
				TokenValue = Money.FromMinorCurrency(100),
				PidSettings = StandaloneAustralianPidFoundationSettings.CreateEnabled(),
				QcomJurisdiction = QcomJurisdictions.VIC
			};
			jurisdictionData.Add(Jurisdictions.VSI, vsi);

			var tsc = new JurisdictionData
			{
				TokenValue = Money.FromMinorCurrency(100),
				PidSettings = StandaloneAustralianPidFoundationSettings.CreateEnabled(),
				QcomJurisdiction = QcomJurisdictions.TAS
			};
			jurisdictionData.Add(Jurisdictions.TSC, tsc);
			jurisdictionData.Add(Jurisdictions.TSS, tsc);

			var sasadc = new JurisdictionData
			{
				QcomJurisdiction = QcomJurisdictions.SA
			};
			jurisdictionData.Add(Jurisdictions.SAS, sasadc);
			jurisdictionData.Add(Jurisdictions.ADC, sasadc);

			var qld = new JurisdictionData
			{
				QcomJurisdiction = QcomJurisdictions.QLD_Clubs
			};
			jurisdictionData.Add(Jurisdictions.QLD, qld);

			var defaultAus = new JurisdictionData
			{
				TokenValue = Money.FromMinorCurrency(100),
				PidSettings = StandaloneAustralianPidFoundationSettings.CreateEnabled(),
				ReserveSettings = StandaloneAustralianReserveFoundationSettings.CreateEnabled()
			};
			defaultAus.PidSettings.IsRequestServiceEnabled = true;
			jurisdictionData.Add(Jurisdictions.DefaultAus, defaultAus);
		}

		public static void UpdateJurisdictionData(Jurisdictions jurisdiction,
			StandaloneAustralianAscentOverrideSettings ascentOverrideSettings,
			StandaloneAustralianMachineFoundationSettings machineSettings,
			StandaloneAustralianReserveFoundationSettings reserveSettings, StandaloneAustralianPidFoundationSettings pidSettings,
			StandaloneAustralianGameFunctionStatusSettings gameFunctionStatusSettings)
		{
			if (LastJurisdiction != jurisdiction)
			{
				LastJurisdiction = jurisdiction;

				if (!jurisdictionData.TryGetValue(jurisdiction, out var jd))
					jd = new JurisdictionData();

				systemConfigurations.FoundationOwnedSettings.Jurisdiction = LastJurisdiction.ToString();
				systemConfigurations.FoundationOwnedSettings.WinCapLimit = jd.WinCapLimit;
				systemConfigurations.FoundationOwnedSettings.AncillarySetting.MonetaryLimit = jd.WinCapLimit;
				SystemConfigurationHelper.Save(systemConfigurations);

				ascentOverrideSettings.DefaultBetOptions = jd.DefaultBetOptions;

				machineSettings.IsUGPFoundation = jd.IsUgpFoundation;
				machineSettings.Tokenisation = (int)jd.TokenValue.AsMinorCurrency;
				machineSettings.WinCapStyle = jd.WinCapStyle;
				machineSettings.IsClockVisible = jd.IsClockVisible;
				machineSettings.ClockFormat = jd.ClockFormat;
				machineSettings.GameCycleTime = jd.GameCycleTime;
				machineSettings.CurrentMaximumBet = jd.CurrentMaximumBet;

				if (jd.QcomJurisdiction.HasValue)
					machineSettings.QcomJurisdiction = (int)jd.QcomJurisdiction.Value;

				reserveSettings.IsReserveAllowedWithCredits = jd.ReserveSettings.IsReserveAllowedWithCredits;
				reserveSettings.IsReserveAllowedWithoutCredits = jd.ReserveSettings.IsReserveAllowedWithoutCredits;
				reserveSettings.ReserveTimeWithCreditsMilliseconds = jd.ReserveSettings.ReserveTimeWithCreditsMilliseconds;
				reserveSettings.ReserveTimeWithoutCreditsMilliseconds = jd.ReserveSettings.ReserveTimeWithoutCreditsMilliseconds;

				pidSettings.IsMainEntryEnabled = jd.PidSettings.IsMainEntryEnabled;
				pidSettings.IsGameRulesEnabled = jd.PidSettings.IsGameRulesEnabled;
				pidSettings.IsRequestServiceEnabled = jd.PidSettings.IsRequestServiceEnabled;
				pidSettings.SessionTimeoutStartOnZeroCredits = jd.PidSettings.SessionTimeoutStartOnZeroCredits;
				pidSettings.ShowLinkJackpotCount = jd.PidSettings.ShowLinkJackpotCount;
				pidSettings.InformationMenuTimeout = jd.PidSettings.InformationMenuTimeout;
				pidSettings.JackpotRtp = jd.PidSettings.JackpotRtp;
				pidSettings.LinkRtpForGameRtp = jd.PidSettings.LinkRtpForGameRtp;
				pidSettings.SessionStartMessageTimeout = jd.PidSettings.SessionStartMessageTimeout;
				pidSettings.SessionTimeoutInterval = jd.PidSettings.SessionTimeoutInterval;
				pidSettings.SessionTrackingOption = jd.PidSettings.SessionTrackingOption;
				pidSettings.Style = jd.PidSettings.Style;
				pidSettings.TotalNumberLinkEnrolments = jd.PidSettings.TotalNumberLinkEnrolments;
				pidSettings.ViewGameInformationTimeout = jd.PidSettings.ViewGameInformationTimeout;
				pidSettings.ViewGameRulesTimeout = jd.PidSettings.ViewGameRulesTimeout;
				pidSettings.ViewPayTableTimeout = jd.PidSettings.ViewPayTableTimeout;
				pidSettings.ViewSessionScreenTimeout = jd.PidSettings.ViewSessionScreenTimeout;

				gameFunctionStatusSettings.GameButtonBehaviours = jd.GameFunctionStatusSettings.GameButtonBehaviours.ToList();
				gameFunctionStatusSettings.DenominationPlayableStatuses = jd.GameFunctionStatusSettings.DenominationPlayableStatuses.ToList();
				gameFunctionStatusSettings.ActiveTimeout = jd.GameFunctionStatusSettings.ActiveTimeout;
				gameFunctionStatusSettings.ConfiguredDenominationMenuTimeout = jd.GameFunctionStatusSettings.ConfiguredDenominationMenuTimeout;
			}

			if (gameFunctionStatusSettings.DenominationPlayableStatuses == null || gameFunctionStatusSettings.DenominationPlayableStatuses.Count == 0)
				gameFunctionStatusSettings.DenominationPlayableStatuses = new List<DenominationPlayableStatus>(systemConfigurations.PaytableList.Select(o => new DenominationPlayableStatus(o.Denomination, GameButtonStatus.Active)));

			// Ensure continuous Update calls.
			if (!Application.isPlaying)
			{
				EditorApplication.QueuePlayerLoopUpdate();
				SceneView.RepaintAll();
			}
		}

		private static Jurisdictions GetCurrentJurisdiction()
		{
			var jurisdiction = Jurisdictions.USDM;

			try
			{
				jurisdiction = (Jurisdictions)Enum.Parse(typeof(Jurisdictions), systemConfigurations.FoundationOwnedSettings.Jurisdiction);
			}
			catch (Exception)
			{
				// Suppress errors when parsing jurisdictions
			}

			return jurisdiction;
		}
	}
}