using System;
using System.Collections.Generic;
using System.IO;
using IGT.Ascent.Communication.Platform.GameLib.Interfaces;
using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid;
using IGT.Game.SDKAssets.AscentBuildSettings;
using IGT.Game.SDKAssets.AscentBuildSettings.Editor;
using Midas.Ascent.Ugp;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using IGT.Game.SDKAssets.SDKBuild;
using UnityEngine;
using static Midas.Ascent.Editor.Ugp.MachineConfigurationData;

namespace Midas.Ascent.Editor.Ugp
{
	public sealed class MachineConfigurationWindow : EditorWindow
	{
		#region Fields

		private Jurisdictions? lastJurisdiction;
		private const string DefaultClockFormat = "h:mm tt";
		private bool showGameFunctionSettings = true;
		private static bool showPidSettings = true;
		private static bool showMachineSettings = true;
		private static bool showReserveSettings = true;
		private static bool showAscentSettings = true;
		private static bool showExternalJackpotSettings = true;
		private static bool showProgressiveAward = true;
		private Vector2 currentScrollPos;

		#endregion

		#region Construction

		/// <summary>
		/// Initialises a new instance of the <see cref="MachineConfigurationWindow"/> class.
		/// </summary>
		private void OnEnable()
		{
			titleContent = new GUIContent("Machine Configuration Standalone Controller");

			UpdateSingleton(LastJurisdiction, true, true);
		}

		#endregion

		#region Editor Overrides

		private void OnInspectorUpdate()
		{
			if (lastJurisdiction != LastJurisdiction)
			{
				lastJurisdiction = LastJurisdiction;
				Repaint();
			}
		}

		private void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Expand All"))
				SetRegionStatus(true);

			if (GUILayout.Button("Collapse All"))
				SetRegionStatus(false);

			EditorGUILayout.EndHorizontal();

			currentScrollPos = EditorGUILayout.BeginScrollView(currentScrollPos);

			EditorGUILayout.BeginVertical();
			EditorGUIUtility.labelWidth = 220;

			LayoutAscentOverrides();

			EditorGUILayout.Separator();

			lastJurisdiction = LayoutMachine(LastJurisdiction);

			EditorGUILayout.Separator();

			LayoutReserve();

			EditorGUILayout.Separator();

			LayoutPid();

			EditorGUILayout.Separator();

			LayoutExternalJackpot();

			EditorGUILayout.Separator();

			LayoutProgressiveAward(out var autoAwardChanged, out var advanceProgressiveAwardChanged);

			EditorGUILayout.Separator();

			LayoutGameFunctionStatusControl();

			EditorGUILayout.EndVertical();
			EditorGUIUtility.labelWidth = 0;

			EditorGUILayout.EndScrollView();

			if (GUI.changed)
			{
				UpdateSingleton(lastJurisdiction.Value, autoAwardChanged, advanceProgressiveAwardChanged);
				StandaloneAustralianFoundationSettings.Save(AustralianFoundationSettings);
			}
		}

		private void LayoutGameFunctionStatusControl()
		{
			showGameFunctionSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGameFunctionSettings, "Game Function Status Settings");

			if (showGameFunctionSettings)
			{
				var gameFunctionStatusSettings = AustralianFoundationSettings.GameFunctionStatusSettings;

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("MCP No Card"))
				{
					foreach (var button in gameFunctionStatusSettings.GameButtonBehaviours)
						button.ButtonStatus = GameButtonStatus.SoftDisabled;

					gameFunctionStatusSettings.ConfiguredDenominationMenuTimeout = 30000;
					gameFunctionStatusSettings.ActiveTimeout = true;
				}

				if (GUILayout.Button("MCP Card Inserted"))
				{
					foreach (var button in gameFunctionStatusSettings.GameButtonBehaviours)
						button.ButtonStatus = GameButtonStatus.Active;

					gameFunctionStatusSettings.ConfiguredDenominationMenuTimeout = 0;
					gameFunctionStatusSettings.ActiveTimeout = false;
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Separator();

				EditorGUILayout.LabelField("Denomination Menu Settings", EditorStyles.boldLabel);

				checked
				{
					gameFunctionStatusSettings.ConfiguredDenominationMenuTimeout = (uint)EditorGUILayout.IntField("Denom menu timeout", (int)gameFunctionStatusSettings.ConfiguredDenominationMenuTimeout);
				}

				gameFunctionStatusSettings.ActiveTimeout = EditorGUILayout.Toggle("Active Timeout", gameFunctionStatusSettings.ActiveTimeout);

				EditorGUILayout.Separator();

				EditorGUILayout.LabelField("Button States", EditorStyles.boldLabel);

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("All Active"))
					SetAllBehaviors(gameFunctionStatusSettings.GameButtonBehaviours, GameButtonStatus.Active);
				if (GUILayout.Button("All Soft Disable"))
					SetAllBehaviors(gameFunctionStatusSettings.GameButtonBehaviours, GameButtonStatus.SoftDisabled);
				if (GUILayout.Button("All Hidden"))
					SetAllBehaviors(gameFunctionStatusSettings.GameButtonBehaviours, GameButtonStatus.Hidden);
				EditorGUILayout.EndHorizontal();

				foreach (var button in gameFunctionStatusSettings.GameButtonBehaviours)
					button.ButtonStatus = (GameButtonStatus)EditorGUILayout.EnumPopup(button.ButtonType.ToString(), button.ButtonStatus);

				EditorGUILayout.Separator();

				EditorGUILayout.LabelField("Denom buttons", EditorStyles.boldLabel);

				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button("All Active"))
					SetAllDenoms(gameFunctionStatusSettings.DenominationPlayableStatuses, GameButtonStatus.Active);
				if (GUILayout.Button("All Soft Disable"))
					SetAllDenoms(gameFunctionStatusSettings.DenominationPlayableStatuses, GameButtonStatus.SoftDisabled);
				if (GUILayout.Button("All Hidden"))
					SetAllDenoms(gameFunctionStatusSettings.DenominationPlayableStatuses, GameButtonStatus.Hidden);

				EditorGUILayout.EndHorizontal();
				foreach (var denom in gameFunctionStatusSettings.DenominationPlayableStatuses)
					denom.ButtonStatus = (GameButtonStatus)EditorGUILayout.EnumPopup(denom.Denomination.ToString(), denom.ButtonStatus);
			}

			EditorGUILayout.EndFoldoutHeaderGroup();

			void SetAllDenoms(List<DenominationPlayableStatus> list, GameButtonStatus status) => list.ForEach(l => l.ButtonStatus = status);

			void SetAllBehaviors(List<GameButtonBehavior> list, GameButtonStatus status) => list.ForEach(l => l.ButtonStatus = status);
		}

		[MenuItem("Midas/Configuration/Standalone Machine Configuration")]
		private static void OpenWindow()
		{
			GetWindow<MachineConfigurationWindow>();
		}

		#endregion

		#region Private Methods

		private static void UpdateSingleton(Jurisdictions newJurisdiction, bool autoAwardChanged, bool advanceProgressiveAwardChanged)
		{
			UpdateJurisdictionData(newJurisdiction, AustralianFoundationSettings.AscentOverrideSettings, AustralianFoundationSettings.MachineSettings, AustralianFoundationSettings.ReserveSettings,
				AustralianFoundationSettings.PidSettings, AustralianFoundationSettings.GameFunctionStatusSettings);
			if (Application.isPlaying)
				AscentFoundation.UgpInterfaces.UpdateStandaloneConfig(AustralianFoundationSettings, autoAwardChanged, advanceProgressiveAwardChanged);
		}

		private static Jurisdictions LayoutMachine(Jurisdictions jurisdiction)
		{
			showMachineSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showMachineSettings, "Machine Settings");
			if (showMachineSettings)
			{
				var machineSettings = AustralianFoundationSettings.MachineSettings;
				if (Application.isPlaying)
				{
					EditorGUILayout.LabelField("Is UGP Foundation ", machineSettings.IsUGPFoundation.ToString());
					EditorGUILayout.LabelField("Jurisdiction ", jurisdiction.ToString());
					EditorGUILayout.LabelField("Qcom Jurisdiction ", ((QcomJurisdictions)machineSettings.QcomJurisdiction).ToString());
					EditorGUILayout.LabelField("Tokenisation ", machineSettings.Tokenisation.ToString());
					EditorGUILayout.LabelField("Win Cap Style ", machineSettings.WinCapStyle.ToString());
				}
				else
				{
					machineSettings.IsUGPFoundation = EditorGUILayout.Toggle("Is UGP Foundation ", machineSettings.IsUGPFoundation);
					jurisdiction = (Jurisdictions)EditorGUILayout.EnumPopup("Jurisdiction", jurisdiction);
					var qcomJurisdiction = (QcomJurisdictions)EditorGUILayout.EnumPopup("QcomJurisdiction", (QcomJurisdictions)machineSettings.QcomJurisdiction);
					machineSettings.QcomJurisdiction = (int)qcomJurisdiction;
					machineSettings.Tokenisation = EditorGUILayout.IntField("Tokenisation", machineSettings.Tokenisation);
					machineSettings.WinCapStyle = (UgpMachineConfigurationWinCapStyle)EditorGUILayout.EnumPopup("Win Cap Style", machineSettings.WinCapStyle);
				}

				// Clock
				machineSettings.IsClockVisible = EditorGUILayout.Toggle("Is Clock Visible", machineSettings.IsClockVisible);
				machineSettings.ClockFormat = EditorGUILayout.TextField("Clock Format", machineSettings.ClockFormat);
				machineSettings.ClockFormat = CheckClockFormat(machineSettings.ClockFormat);

				// Game Cycle Time
				machineSettings.GameCycleTime = EditorGUILayout.IntField("Game Cycle Time", machineSettings.GameCycleTime);
				machineSettings.IsContinuousPlayAllowed = EditorGUILayout.Toggle("Is Continuous Play Allowed", machineSettings.IsContinuousPlayAllowed);
				machineSettings.IsFeatureAutoStartEnabled = EditorGUILayout.Toggle("Is Feature Auto Start Enabled", machineSettings.IsFeatureAutoStartEnabled);
				machineSettings.IsSlamSpinAllowed = EditorGUILayout.Toggle("Is Slam Spin Allowed", machineSettings.IsSlamSpinAllowed);
				machineSettings.IsMoreGamesEnabled = EditorGUILayout.Toggle("Is More Games Button Enabled", machineSettings.IsMoreGamesEnabled);

				// Current Maximum Bet.
				var currentMaxBetValue = machineSettings.CurrentMaximumBet.ToString();
				currentMaxBetValue = EditorGUILayout.TextField("Current Maximum Bet", currentMaxBetValue);
				if (long.TryParse(currentMaxBetValue, out var currentMaxBet))
					machineSettings.CurrentMaximumBet = currentMaxBet;
			}

			EditorGUILayout.EndFoldoutHeaderGroup();

			return jurisdiction;
		}

		private static void LayoutPid()
		{
			showPidSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showPidSettings, "PID Settings");
			if (showPidSettings)
			{
				var pid = AustralianFoundationSettings.PidSettings;

				pid.IsMainEntryEnabled = EditorGUILayout.Toggle("Is PID Enabled", pid.IsMainEntryEnabled);
				pid.IsRequestServiceEnabled = EditorGUILayout.Toggle("Allow Request Service", pid.IsRequestServiceEnabled);
				pid.IsGameRulesEnabled = EditorGUILayout.Toggle("Allow Game Rules", pid.IsGameRulesEnabled);

				pid.Style = (GameInformationDisplayStyle)EditorGUILayout.EnumPopup("Display Style", pid.Style);
				pid.SessionTrackingOption = (SessionTrackingOption)EditorGUILayout.EnumPopup("Session Tracking Option", pid.SessionTrackingOption);

				pid.InformationMenuTimeout = EditorGUILayout.IntField("Information Menu Timeout", pid.InformationMenuTimeout);
				pid.ViewGameRulesTimeout = EditorGUILayout.IntField("View Game Rules Timeout", pid.ViewGameRulesTimeout);
				pid.ViewPayTableTimeout = EditorGUILayout.IntField("View Pay Table Timeout", pid.ViewPayTableTimeout);
				pid.ViewSessionScreenTimeout = EditorGUILayout.IntField("View Session Screen Timeout", pid.ViewSessionScreenTimeout);
				pid.ViewGameInformationTimeout = EditorGUILayout.IntField("View Game Information Timeout", pid.ViewGameInformationTimeout);
				pid.SessionStartMessageTimeout = EditorGUILayout.IntField("Session Start Message Timeout", pid.SessionStartMessageTimeout);

				pid.SessionTimeoutInterval = EditorGUILayout.IntField("Session Timeout Interval", pid.SessionTimeoutInterval);
				pid.SessionTimeoutStartOnZeroCredits = EditorGUILayout.Toggle("Session Timeout Start On Zero Credits", pid.SessionTimeoutStartOnZeroCredits);

				pid.JackpotRtp = EditorGUILayout.FloatField("Jackpot Rtp", pid.JackpotRtp);
				pid.TotalNumberLinkEnrolments = EditorGUILayout.IntField("Total Number Link Enrolments", pid.TotalNumberLinkEnrolments);
				pid.ShowLinkJackpotCount = EditorGUILayout.Toggle("Show Link Jackpot Count", pid.ShowLinkJackpotCount);
				pid.LinkRtpForGameRtp = EditorGUILayout.FloatField("Link RTP For Game RTP", pid.LinkRtpForGameRtp);
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static void LayoutExternalJackpot()
		{
			showExternalJackpotSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showExternalJackpotSettings, "External Jackpots Settings");
			if (showExternalJackpotSettings)
			{
				var externalJackpotSettings = AustralianFoundationSettings.ExternalJackpotSettings;

				externalJackpotSettings.IsVisible = EditorGUILayout.Toggle("Is Visible", externalJackpotSettings.IsVisible);
				externalJackpotSettings.NumberOfJackpots = EditorGUILayout.IntField("Number Of Jackpots", externalJackpotSettings.NumberOfJackpots);
				externalJackpotSettings.IsIconVisible = EditorGUILayout.Toggle("Is Icon Visible", externalJackpotSettings.IsIconVisible);
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static void LayoutProgressiveAward(out bool autoAwardChanged, out bool advanceProgressiveAward)
		{
			var externalJackpotSettings = AustralianFoundationSettings.ProgressiveAwardSettings;

			autoAwardChanged = externalJackpotSettings.AutoAward;
			advanceProgressiveAward = false;
			showProgressiveAward = EditorGUILayout.BeginFoldoutHeaderGroup(showProgressiveAward, "Progressive Award  Settings");
			if (showProgressiveAward)
			{
				var newAutoAward = EditorGUILayout.Toggle("Auto Award", externalJackpotSettings.AutoAward);

				autoAwardChanged = newAutoAward != externalJackpotSettings.AutoAward;
				externalJackpotSettings.AutoAward = newAutoAward;

				if (!newAutoAward)
				{
					externalJackpotSettings.AwardAmount = EditorGUILayout.IntField("Award Amount", externalJackpotSettings.AwardAmount);
					advanceProgressiveAward = GUILayout.Button("Advance Progressive Award");
				}
				else
					advanceProgressiveAward = false;
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static void LayoutReserve()
		{
			showReserveSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showReserveSettings, "Reserve Settings");
			if (showReserveSettings)
			{
				var reserve = AustralianFoundationSettings.ReserveSettings;

				EditorGUILayout.BeginHorizontal();
				reserve.IsReserveAllowedWithCredits = EditorGUILayout.Toggle("Reserve With Credits", reserve.IsReserveAllowedWithCredits);
				EditorGUILayout.LabelField("Timeout", GUILayout.MaxWidth(50));
				reserve.ReserveTimeWithCreditsMilliseconds = EditorGUILayout.IntField(reserve.ReserveTimeWithCreditsMilliseconds);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				reserve.IsReserveAllowedWithoutCredits = EditorGUILayout.Toggle("Reserve Without Credits", reserve.IsReserveAllowedWithoutCredits);
				EditorGUILayout.LabelField("Timeout", GUILayout.MaxWidth(50));
				reserve.ReserveTimeWithoutCreditsMilliseconds = EditorGUILayout.IntField(reserve.ReserveTimeWithoutCreditsMilliseconds);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static void LayoutAscentOverrides()
		{
			showAscentSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAscentSettings, "Ascent Settings");
			if (showAscentSettings)
			{
				var ascentOverrides = AustralianFoundationSettings.AscentOverrideSettings;

				if (Application.isPlaying)
				{
					EditorGUILayout.LabelField("Show Mode ", ascentOverrides.ShowMode.ToString());
					EditorGUILayout.LabelField("Show Environment ", ascentOverrides.ShowEnvironment.ToString());
					EditorGUILayout.LabelField("Show Min Credits ", ascentOverrides.ShowMinimumCredits.ToString());
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("Default Volume ", $"{ascentOverrides.DefaultVolume}");
					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("Default Bet Options", EditorStyles.boldLabel);
					EditorGUILayout.LabelField("Default Stake ", $"{ascentOverrides.DefaultBetOptions.NumberOfSubsets}");
					EditorGUILayout.LabelField("Default Bet Multiplier ", $"{ascentOverrides.DefaultBetOptions.BetPerSubset}");
					EditorGUILayout.LabelField("Default Side Bet ", $"{ascentOverrides.DefaultBetOptions.SideBet}");
				}
				else
				{
					ascentOverrides.ShowMode = EditorGUILayout.Toggle("Show Mode", ascentOverrides.ShowMode);
					var wasEnabled = GUI.enabled;
					GUI.enabled = ascentOverrides.ShowMode;
					ascentOverrides.ShowEnvironment = (ShowEnvironment)EditorGUILayout.EnumPopup("Show Environment", ascentOverrides.ShowEnvironment);
					ascentOverrides.ShowMinimumCredits = EditorGUILayout.LongField("Show Min Credits", ascentOverrides.ShowMinimumCredits);
					GUI.enabled = wasEnabled;

					EditorGUILayout.Separator();
					ascentOverrides.DefaultVolume = EditorGUILayout.FloatField("Default Volume", ascentOverrides.DefaultVolume);

					EditorGUILayout.Separator();
					EditorGUILayout.LabelField("Default Bet Options", EditorStyles.boldLabel);
					ascentOverrides.DefaultBetOptions.NumberOfSubsets = (BetSelectionBehavior)EditorGUILayout.EnumPopup("Default Stake", ascentOverrides.DefaultBetOptions.NumberOfSubsets);
					ascentOverrides.DefaultBetOptions.BetPerSubset = (BetSelectionBehavior)EditorGUILayout.EnumPopup("Default Bet Multiplier", ascentOverrides.DefaultBetOptions.BetPerSubset);
					ascentOverrides.DefaultBetOptions.SideBet = (SideBetSelectionBehavior)EditorGUILayout.EnumPopup("Default Side Bet", ascentOverrides.DefaultBetOptions.SideBet);
				}
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static string CheckClockFormat(string clockFormat)
		{
			var formatString = clockFormat;
			try
			{
				var _ = DateTime.Now.ToString(formatString);
			}
			catch (Exception)
			{
				formatString = DefaultClockFormat;
				Debug.Log("Invalid clock format. Using Default. Entered format: " + clockFormat);
			}

			if (string.IsNullOrEmpty(clockFormat))
			{
				formatString = DefaultClockFormat;
				Debug.Log("Empty clock format. Using Default.");
			}

			return formatString;
		}

		private void SetRegionStatus(bool value)
		{
			showGameFunctionSettings = value;
			showPidSettings = value;
			showMachineSettings = value;
			showReserveSettings = value;
			showAscentSettings = value;
			showExternalJackpotSettings = value;
			showProgressiveAward = value;
		}

		public sealed class SettingsCopier : IPreprocessBuildWithReport
		{
			public int callbackOrder { get; }

			/// <summary>
			/// Callback used to copy the StandaloneAustralianFoundationSettings.xml to the build folder for standalone builds
			/// </summary>
			public void OnPreprocessBuild(BuildReport report)
			{
				var pathToBuiltProject = report.summary.outputPath;
				if (string.IsNullOrEmpty(pathToBuiltProject))
					return;

				if (IGTEGMSettings.IgtEgmBuildSettings.BuildType != BuildType.Game || IGTEGMSettings.IgtEgmBuildSettings.GameParameters.Type == IgtGameParameters.GameType.Standard
					|| IGTEGMSettings.IgtEgmBuildSettings.GameParameters.Type == IgtGameParameters.GameType.UniversalController
					|| IGTEGMSettings.IgtEgmBuildSettings.GameParameters.Type == IgtGameParameters.GameType.WebAndMobile
					|| IGTEGMSettings.IgtEgmBuildSettings.GameParameters.Type == IgtGameParameters.GameType.FastPlay)
				{
					return;
				}

				var buildFolder = Path.GetDirectoryName(pathToBuiltProject);
				var projectFolder = Directory.GetCurrentDirectory();

				BuildUtilities.CopyBuildFile(projectFolder, buildFolder, "StandaloneAustralianFoundationSettings.xml");
			}
		}

		#endregion
	}
}