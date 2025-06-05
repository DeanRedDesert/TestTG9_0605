using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using IGT.Ascent.Communication.Platform.GameLib.Interfaces;
using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.TiltManagement;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration;
using IGT.Game.Core.Tilts;
using IGT.Game.SDKAssets.AscentBuildSettings;
using Midas.Ascent.Ugp;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.General;
using static Midas.Ascent.AscentFoundation;
using AscentGameMode = IGT.Ascent.Communication.Platform.Interfaces.GameMode;
using PlayerSessionParameterType = Midas.Core.PlayerSessionParameterType;

namespace Midas.Ascent
{
	internal sealed class FoundationShim : IFoundationShim
	{
		private Dictionary<(NvramScope Scope, string Path), byte[]> utilityModeData = new Dictionary<(NvramScope Scope, string Path), byte[]>();

		public FoundationShim()
		{
			GameMode = GameLib.GameContextMode switch
			{
				AscentGameMode.Play => FoundationGameMode.Play,
				AscentGameMode.History => FoundationGameMode.History,
				AscentGameMode.Utility => FoundationGameMode.Utility,
				_ => throw new Exception($"Invalid game mode {GameLib.GameContextMode}")
			};

			ShowMode = GameLibShow?.GetShowEnvironment() switch
			{
				ShowEnvironment.Development => FoundationShowMode.Development,
				ShowEnvironment.Show => FoundationShowMode.Show,
				_ => FoundationShowMode.None
			};

			WinCapStyle = AscentFoundation.UgpInterfaces.GetMachineConfigurationParameters().WinCapStyle switch
			{
				UgpMachineConfigurationWinCapStyle.None => WinCapStyle.None,
				UgpMachineConfigurationWinCapStyle.Clip => WinCapStyle.Clip,
				UgpMachineConfigurationWinCapStyle.ClipAndBreakout => WinCapStyle.ClipAndBreakout,
				_ => throw new Exception($"Invalid win cap style {AscentFoundation.UgpInterfaces.GetMachineConfigurationParameters().WinCapStyle}")
			};
		}

		public string GameMountPoint { get; } = GameLib.GameMountPoint;

		public bool IsInitialising => AscentGameEngine.IsInitialising;

		public FoundationGameMode GameMode { get; }
		public FoundationShowMode ShowMode { get; }

		public bool ShouldGameLogicExit => AscentGameEngine.ShouldGameEngineExit;
		public bool IsPaused => AscentGameEngine.IsPaused;
		public PaytableConfig PaytableConfiguration => AscentGameEngine.PaytableConfiguration;
		public GameIdentityType GameIdentity => AscentFoundation.UgpInterfaces.IsUgpFoundation ? GameIdentityType.Anz : GlobalGameIdentity;
		public FoundationType FoundationType => AscentFoundation.UgpInterfaces.IsUgpFoundation ? FoundationType.Ugp : FoundationType.Ascent;

		public void ProcessEvents(WaitHandle waitHandle) => AscentGameEngine.ProcessEvents(waitHandle);

		public IReadOnlyList<int> GetRandomNumbers(uint count, int rangeMin, int rangeMax)
		{
			return GameLib.GetRandomNumbers(new RandomValueRequest(count, rangeMin, rangeMax, count)).ToArray();
		}

		#region CriticalData

		private static readonly CriticalDataScope[] nvramScopeMap =
		{
			CriticalDataScope.Theme,
			CriticalDataScope.Payvar,
			CriticalDataScope.GameCycle,
			CriticalDataScope.History
		};

		private static readonly AscentGameMode[] gameModeMap =
		{
			AscentGameMode.Play,
			AscentGameMode.History,
			AscentGameMode.Utility
		};

		public void WriteNvram(NvramScope scope, string path, byte[] data)
		{
			Log.Instance.DebugFormat("WriteNvram({0}:{1})", scope, path);

			byte[] writeBuffer;
			if (data == null)
			{
				writeBuffer = new byte[1];
				writeBuffer[0] = 1;
			}
			else
			{
				writeBuffer = new byte[data.Length + 1];
				Buffer.BlockCopy(data, 0, writeBuffer, 1, data.Length);
			}

			if (GameMode == FoundationGameMode.Utility)
			{
				utilityModeData[(scope, path)] = writeBuffer;
				return;
			}

			RawCriticalData.WriteCriticalData(nvramScopeMap[(int)scope], path, writeBuffer);
		}

		public bool TryReadNvram(NvramScope scope, string path, out byte[] data)
		{
			Log.Instance.DebugFormat("TryReadNvram({0}:{1})", scope, path);

			byte[] rawData = null;
			if (GameMode == FoundationGameMode.Utility)
			{
				if (utilityModeData.TryGetValue((scope, path), out var umd))
					rawData = umd;
			}
			else
				rawData = RawCriticalData.ReadCriticalData(nvramScopeMap[(int)scope], path);

			if (rawData == null || rawData.Length == 0)
			{
				data = null;
				return false;
			}

			if (rawData[0] == 1)
			{
				data = null;
			}
			else
			{
				data = new byte[rawData.Length - 1];
				Buffer.BlockCopy(rawData, 1, data, 0, rawData.Length - 1);
			}

			return true;
		}

		public bool RemoveNvram(NvramScope scope, string path)
		{
			if (GameMode == FoundationGameMode.Utility)
			{
				if (!utilityModeData.ContainsKey((scope, path)))
					return false;

				utilityModeData.Remove((scope, path));
				return true;
			}

			Log.Instance.DebugFormat("RemoveNvram({0}:{1})", scope, path);
			return RawCriticalData.RemoveCriticalData(nvramScopeMap[(int)scope], path);
		}

		public void UtilityEndGame()
		{
			var newDict = new Dictionary<(NvramScope Scope, string Path), byte[]>();

			foreach (var kvp in utilityModeData)
			{
				if (kvp.Key.Scope == NvramScope.History || kvp.Key.Scope == NvramScope.GameCycle)
					continue;
				newDict.Add(kvp.Key, kvp.Value);
			}

			utilityModeData = newDict;
		}

		#endregion

		#region Configuration

		private static readonly CurrencySymbolPosition[] currencySymbolPositionMap =
		{
			CurrencySymbolPosition.Leading,
			CurrencySymbolPosition.Trailing,
			CurrencySymbolPosition.LeadingWithSpace,
			CurrencySymbolPosition.TrailingWithSpace
		};

		public ConfigData ReadConfiguration()
		{
			var machineConfig = ReadMachineConfig();
			var denomConfig = ReadDenomConfig();
			var currencyConfig = ReadCurrencyConfig();
			var ancillaryConfig = ReadAncillaryConfig();
			var languageConfig = ReadLanguageConfig();
			var gameConfig = ReadGameConfig();
			var customConfig = ReadCustomConfig();

			Log.Instance.Info($"MachineConfig='{machineConfig}'");
			Log.Instance.Info($"CurrencyConfig='{currencyConfig}'");
			Log.Instance.Info($"DenomConfig='{denomConfig}'");
			Log.Instance.Info($"AncillaryConfig='{ancillaryConfig}'");
			Log.Instance.Info($"LanguageConfig='{languageConfig}'");
			Log.Instance.Info($"GameConfig='{gameConfig}'");
			Log.Instance.Info($"CustomConfigItems='{customConfig}'");

			UpdateFoundationProgressivesForStandalonePlayer(customConfig, gameConfig, denomConfig);

			return new ConfigData(machineConfig, currencyConfig, denomConfig, ancillaryConfig,
				languageConfig, gameConfig, customConfig);
		}

		public bool ChangeGameDenom(Money denom)
		{
			return AscentGameEngine.ChangeGameDenom(denom);
		}

		public void ChangeLanguage(string language)
		{
			AscentGameEngine.ChangeLanguage(language);
		}

		private static MachineConfig ReadMachineConfig()
		{
			var showEnvironment = GameLibShow?.ShowMode == true ? GameLibShow.GetShowEnvironment() : ShowEnvironment.Invalid;
			var showMinimumCredits = Money.Zero;

			if (GameParameters.Type != IgtGameParameters.GameType.Standard)
				showMinimumCredits = Money.FromCredit(Credit.FromLong(StandaloneAustralianFoundationSettings.Load().AscentOverrideSettings.ShowMinimumCredits));

			return new MachineConfig(GameLib.Jurisdiction, showEnvironment == ShowEnvironment.Show, showEnvironment != ShowEnvironment.Invalid, showEnvironment == ShowEnvironment.Development, !GameParameters.Type.IsStandaloneBuild(), showMinimumCredits);
		}

		private static DenomConfig ReadDenomConfig()
		{
			var currentDenom = Money.FromMinorCurrency(GameLib.GameDenomination);
			var availableDenominations = GameLib.GameContextMode == AscentGameMode.Play
				? GameLib.GetAvailableDenominations().OrderBy(v => v).Select(Money.FromMinorCurrency).ToArray()
				: new[] { currentDenom };

			return new DenomConfig(currentDenom, availableDenominations, Money.FromMinorCurrency(AscentFoundation.UgpInterfaces.GetMachineConfigurationParameters().Tokenisation), AscentFoundation.UgpInterfaces.IsUgpFoundation);
		}

		private static CurrencyConfig ReadCurrencyConfig()
		{
			var creditFormatter = GameLib.LocalizationInformation.GetCreditFormatter();
			return new CurrencyConfig(
				creditFormatter.CurrencySymbol, currencySymbolPositionMap[(int)creditFormatter.SymbolPosition],
				creditFormatter.CurrencyCentSymbol, currencySymbolPositionMap[(int)creditFormatter.CentSymbolPosition],
				creditFormatter.DecimalSeparator, creditFormatter.DigitGroupSeparator,
				creditFormatter.UseCreditSeparator
			);
		}

		private static AncillaryConfig ReadAncillaryConfig()
		{
			return new AncillaryConfig(GameLib.AncillaryEnabled,
				Money.FromMinorCurrency(GameLib.AncillaryMonetaryLimit),
				GameLib.AncillaryCycleLimit);
		}

		private static LanguageConfig ReadLanguageConfig()
		{
			return new LanguageConfig(GameLib.AvailableLanguages);
		}

		private static GameConfig ReadGameConfig()
		{
			var gameContext = GameLib.GameContextMode;
			var minBet = gameContext != AscentGameMode.History ? GameLib.GameMinBet : 0;
			var maxBet = gameContext != AscentGameMode.History ? GameLib.GameMaxBet : 0;

			var gameConfig = new GameConfig(
				GameLib.IsPlayerAutoPlayEnabled,
				GameLib.IsAutoPlayConfirmationRequired,
				IsAutoPlaySpeedChangeAllowed(),
				GameLib.RoundWagerUpPlayoffEnabled,
				Credit.FromLong(minBet),
				Credit.FromLong(maxBet),
				ReadDefaultStakeSettings());

			return gameConfig;

			bool IsAutoPlaySpeedChangeAllowed()
			{
				var nonSpeedChangeJurisdictions = new[] { "USDM", "00NV" };
				return GameLib.IsAutoPlaySpeedIncreaseAllowed ?? !nonSpeedChangeJurisdictions.Contains(GameLib.Jurisdiction);
			}
		}

		private static DefaultStakeSettings ReadDefaultStakeSettings()
		{
			var defaultBetSelectionStyle = GetDefaultBetSelectionStyle();

			bool? useMaximumNumberOfLines = null;
			if (defaultBetSelectionStyle.NumberOfSubsets != BetSelectionBehavior.Undefined)
				useMaximumNumberOfLines = defaultBetSelectionStyle.NumberOfSubsets == BetSelectionBehavior.Maximum;

			bool? useMaximumBetMultiplier = null;
			if (defaultBetSelectionStyle.BetPerSubset != BetSelectionBehavior.Undefined)
				useMaximumBetMultiplier = defaultBetSelectionStyle.BetPerSubset == BetSelectionBehavior.Maximum;

			bool? includeSideBet = null;
			if (defaultBetSelectionStyle.SideBet != SideBetSelectionBehavior.Undefined)
				includeSideBet = defaultBetSelectionStyle.SideBet == SideBetSelectionBehavior.Include;

			return new DefaultStakeSettings(useMaximumNumberOfLines, useMaximumBetMultiplier, includeSideBet);

			BetSelectionStyleInfo GetDefaultBetSelectionStyle()
			{
				if (GameParameters.Type != IgtGameParameters.GameType.Standard)
				{
					// Read from the standalone configuration files when not a standard build.
					var dbo = StandaloneAustralianFoundationSettings.Load().AscentOverrideSettings.DefaultBetOptions;
					return new BetSelectionStyleInfo(dbo.NumberOfSubsets, dbo.BetPerSubset, dbo.SideBet);
				}

				// Use the game lib settings if not the UGP foundation.
				if (!AscentFoundation.UgpInterfaces.IsUgpFoundation)
					return GameLib.DefaultBetSelectionStyle;

				// Force to maximum bet if a show environment.
				return GameLibShow.GetShowEnvironment() == ShowEnvironment.Show
					? new BetSelectionStyleInfo(BetSelectionBehavior.Maximum, BetSelectionBehavior.Maximum, SideBetSelectionBehavior.Undefined)
					: new BetSelectionStyleInfo(BetSelectionBehavior.Minimum, BetSelectionBehavior.Minimum, SideBetSelectionBehavior.Undefined);
			}
		}

		private static CustomConfig ReadCustomConfig()
		{
			if (GameLib.GameContextMode != AscentGameMode.History)
			{
				var themeValues = ReadCustomConfigurationItems(GameConfigurationScopeKey.NewThemeScopeKey(), GameConfigurationKey.NewThemeKey);
				var payVarValues = ReadCustomConfigurationItems(GameConfigurationScopeKey.NewPayvarScopeKey(), GameConfigurationKey.NewPayvarKey);
				return new CustomConfig(themeValues, payVarValues);
			}

			return new CustomConfig(new Dictionary<string, object>(), new Dictionary<string, object>());
		}

		private static IReadOnlyDictionary<string, object> ReadCustomConfigurationItems(GameConfigurationScopeKey scopeKey, Func<string, GameConfigurationKey> keyFactory)
		{
			var configurationItems =
				GameLib.ConfigurationRead.QueryConfigurations(scopeKey)
					.Select(c => (keyFactory(c.Key), c.Value))
					.ToDictionary(v => v.Item1, v => v.Item2);
			return GameLib.ConfigurationRead.GetConfigurations(configurationItems)
				.Select(v => (v.Key.ConfigName, v.Value))
				.ToDictionary(v => v.ConfigName, v => v.Value);
		}

		private void UpdateFoundationProgressivesForStandalonePlayer(CustomConfig customConfig, GameConfig gameConfig, DenomConfig denomConfig)
		{
			if (AscentFoundation.UgpInterfaces.StandaloneProgressive == null)
				return;

			customConfig.PayVarConfigItems.TryGetValue("ProgressiveSetId", out var psi);
			customConfig.PayVarConfigItems.TryGetValue("Percentage", out var p);
			var levels = AusRegReader.GetAllProgressiveLevels(GameMountPoint, (string)psi, (string)p, gameConfig.MaxBetLimit, denomConfig.CurrentDenomination);
			AscentFoundation.UgpInterfaces.StandaloneProgressive.SetProgressiveLevels(levels);
		}

		#endregion

		#region Progressives

		public void StartProgressiveAward(int awardIndex, string levelId, Money amount)
		{
			AscentFoundation.UgpInterfaces.StartingProgressiveAward(awardIndex, levelId, amount.AsMinorCurrency);
		}

		public void FinishedProgressiveAwardDisplay(int awardIndex, string levelId, Money defaultPaidAmount)
		{
			AscentFoundation.UgpInterfaces.FinishedDisplay(awardIndex, levelId, defaultPaidAmount.AsMinorCurrency);
		}

		#endregion

		#region Dashboard

		public void RequestCashout()
		{
			GameLib?.RequestCashOut();
		}

		public bool RequestThemeSelectionMenu()
		{
			if (GameLib == null)
				return false;

			return GameParameters.Type == IgtGameParameters.GameType.Standard ? GameLib.RequestThemeSelectionMenu() : SimulateGameExitToChooser();
		}

		private static bool SimulateGameExitToChooser()
		{
			GameLib.RequestThemeSelectionMenu();

			var activateThemeMethod = GameLib.GetType().GetMethod("ActivateThemeContext", BindingFlags.Instance | BindingFlags.NonPublic);
			var playModeDenomProperty = GameLib.GetType().GetProperty("PlayModeGameDenomination", BindingFlags.Instance | BindingFlags.NonPublic);
			var payTableManagerField = GameLib.GetType().GetField("paytableListManager", BindingFlags.Instance | BindingFlags.NonPublic);

			if (activateThemeMethod == null || playModeDenomProperty == null || payTableManagerField == null)
				return false;

			var payTableManager = payTableManagerField.GetValue(GameLib);
			var playModeDenom = playModeDenomProperty.GetValue(GameLib);

			var getVariant = payTableManager.GetType().GetMethod("GetPaytableVariant", BindingFlags.Instance | BindingFlags.Public);
			var themeIdentifier = GameLib.GetType().GetProperty("ThemeIdentifier", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(GameLib);

			if (getVariant == null || themeIdentifier == null)
				return false;

			var gv = getVariant.Invoke(payTableManager, new[] { themeIdentifier, playModeDenom });

			if (gv == null)
				return false;

			activateThemeMethod.Invoke(GameLib, new[] { GameLib.GameContextMode, gv, playModeDenom, true });
			return true;
		}

		#endregion

		#region PID

		public void PidActivated(bool status)
		{
			AscentFoundation.UgpInterfaces.ActivationStatusChanged(status);
		}

		public void PidGameInfoEntered()
		{
			AscentFoundation.UgpInterfaces.GameInformationScreenEntered();
		}

		public void PidSessionInfoEntered()
		{
			AscentFoundation.UgpInterfaces.SessionInformationScreenEntered();
		}

		public void StartPidSessionTracking()
		{
			AscentFoundation.UgpInterfaces.StartTracking();
			AscentGameEngine.UpdatePidSessionData();
		}

		public void StopPidSessionTracking()
		{
			AscentFoundation.UgpInterfaces.StopTracking();
			AscentGameEngine.UpdatePidSessionData();
		}

		public void ToggleServiceRequested()
		{
			AscentFoundation.UgpInterfaces.AttendantServiceRequested();
		}

		#endregion

		#region AutoPlay

		public bool IsAutoPlayOn()
		{
			return GameLib.IsAutoPlayOn();
		}

		public bool SetAutoPlayOn()
		{
			return GameLib.SetAutoPlayOn();
		}

		public void SetAutoPlayOff()
		{
			GameLib.SetAutoPlayOff();
		}

		#endregion

		#region Demo

		public void DemoSetDisplayState(DisplayState displayState)
		{
			Log.Instance.DebugFormat("DemoSetDisplayState({0})", displayState);
			if (GameLibDemo != null)
			{
				switch (displayState)
				{
					case DisplayState.Normal:
						GameLibDemo.SetDisplayControlNormal();
						break;
					case DisplayState.Suspended:
						GameLibDemo.SetDisplayControlSuspended();
						break;
					case DisplayState.Hidden:
						GameLibDemo.SetDisplayControlHidden();
						break;
				}
			}
		}

		public void DemoEnterGameMode(FoundationGameMode mode)
		{
			Log.Instance.DebugFormat("DemoEnterGameMode({0})", mode);

			if (mode == FoundationGameMode.History && DemoGetHistoryRecordCount() == 0)
				return;

			AscentGameEngine.ChangeGameMode(gameModeMap[(int)mode]);
		}

		public void DemoEnterUtilityMode(string theme, KeyValuePair<string, string> paytable, Money denomination)
		{
			if (GameLibDemo == null)
			{
				Log.Instance.DebugFormat("DemoEnterGameMode(Utility) {0} {1} {2} failed due to no GameLibDemo object", theme, paytable, denomination);
				return;
			}

			Log.Instance.DebugFormat("DemoEnterGameMode(Utility) {0} {1} {2}", theme, paytable, denomination);
			AscentGameEngine.ChangeGameMode(gameModeMap[(int)FoundationGameMode.Utility]);

			// Set these after calling change mode to ensure they are set.
			GameLibDemo.UtilityTheme = theme;
			GameLibDemo.UtilityPaytable = paytable;
			GameLibDemo.UtilityDenomination = denomination.AsMinorCurrency;
			GameLibDemo.UtilitySelectionComplete = true;
		}

		public int DemoGetHistoryRecordCount() => GameLibDemo?.GetHistoryRecordCount() ?? 0;
		public bool DemoIsNextHistoryRecordAvailable() => GameLibDemo?.IsNextAvailable() ?? false;
		public bool DemoIsPreviousHistoryRecordAvailable() => GameLibDemo?.IsPreviousAvailable() ?? false;

		public void DemoNextHistoryRecord()
		{
			Log.Instance.Debug("DemoNextHistoryRecord()");
			GameLibDemo?.NextHistoryRecord();
		}

		public void DemoPreviousHistoryRecord()
		{
			Log.Instance.Debug("DemoPreviousHistoryRecord()");
			GameLibDemo?.PreviousHistoryRecord();
		}

		public IReadOnlyList<string> DemoGetRegistrySupportedThemes()
		{
			return GameLibDemo == null ? Array.Empty<string>() : GameLibDemo.GetRegistrySupportedThemes();
		}

		public IReadOnlyDictionary<KeyValuePair<string, string>, IReadOnlyList<long>> DemoGetRegistrySupportedDenominations(string theme)
		{
			if (GameLibDemo == null)
				return new Dictionary<KeyValuePair<string, string>, IReadOnlyList<long>>();

			// Only report the currently configured paytables due to limitations in the GameRegistryManager.

			var payTableManagerField = GameLib.GetType().GetField("paytableListManager", BindingFlags.Instance | BindingFlags.NonPublic);

			if (payTableManagerField == null)
				return new Dictionary<KeyValuePair<string, string>, IReadOnlyList<long>>();

			var payTableManager = payTableManagerField.GetValue(GameLib);
			var themeIdentifier = GameLib.GetType().GetProperty("ThemeIdentifier", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(GameLib);
			var getPaytableVariant = payTableManager.GetType().GetMethod("GetPaytableVariant", BindingFlags.Instance | BindingFlags.Public);
			var getAvailableDenoms = payTableManager.GetType().GetMethod("GetAvailableDenominations", BindingFlags.Instance | BindingFlags.Public);
			var availableDenoms = (IReadOnlyList<long>)getAvailableDenoms?.Invoke(payTableManager, new[] { themeIdentifier });

			if (availableDenoms == null || getPaytableVariant == null)
				return new Dictionary<KeyValuePair<string, string>, IReadOnlyList<long>>();

			var d = new Dictionary<KeyValuePair<string, string>, List<long>>();
			foreach (var denom in availableDenoms)
			{
				var res = getPaytableVariant.Invoke(payTableManager, new[] { themeIdentifier, denom });
				var n = (string)res.GetType().GetProperty("PaytableName", BindingFlags.Instance | BindingFlags.Public)?.GetValue(res);
				var fn = (string)res.GetType().GetProperty("PaytableFileName", BindingFlags.Instance | BindingFlags.Public)?.GetValue(res);

				var key = new KeyValuePair<string, string>(n, fn);
				if (d.ContainsKey(key))
					d[key].Add(denom);
				else
					d.Add(key, new List<long> { denom });
			}

			return d.ToDictionary(p => p.Key, p => (IReadOnlyList<long>)p.Value.ToList());
		}

		#endregion

		#region Show

		public void ShowAddMoney(Money amount)
		{
			Log.Instance.DebugFormat("ShowAddMoney({0})", amount.ToString());
			if (GameLibDemo != null)
				GameLibDemo.InsertMoney(amount.AsMinorCurrency, 1);
			else if (GameLibShow != null && GameLibShow.GetShowEnvironment() != ShowEnvironment.Invalid)
				GameLibShow.InsertMoney(amount.AsMinorCurrency, 1);
		}

		#endregion

		#region Runtime Events

		public void SendWaitingForInput(bool status) => AscentFoundation.UgpInterfaces.WaitingForGenericInput(status);

		public void SendDenomSelectionActive(bool active)
		{
			Log.Instance.DebugFormat("DenomSelectionActive({0})", active.ToString());
			AscentFoundation.UgpInterfaces.DenomSelectionActive(active);
		}

		#endregion

		#region Debug Info

		public string TimingsString => GameLogicTimings.TimingsString;
		public void ResetTimings() => GameLogicTimings.Reset();

		#endregion

		#region Player Session

		public void ReportParametersBeingReset(IReadOnlyList<PlayerSessionParameterType> parametersBeingReset)
		{
			AscentFoundation.PlayerSessionInterfaces.ReportParametersBeingReset(parametersBeingReset.Select(p => (IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams.PlayerSessionParameterType)(int)p).ToArray());
		}

		public void InitiatePlayerSessionReset(IReadOnlyList<PlayerSessionParameterType> parametersToReset)
		{
			AscentFoundation.PlayerSessionInterfaces.InitiatePlayerSessionReset(parametersToReset.Select(p => (IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams.PlayerSessionParameterType)(int)p).ToArray());
		}

		public void PlayerSessionActive(bool isActive)
		{
			AscentFoundation.PlayerSessionInterfaces.ActivatePlayerSession(isActive);
		}

		public void PostTilt(string tiltKey, GameTiltPriority priority, string title, string message, bool isBlocking, bool discardOnGameShutdown, bool userInterventionRequired)
		{
			var tiltManagement = GameLib.GetInterface<ITiltManagement>();
			if (tiltManagement == null)
				return;

			if (tiltManagement.TiltPresent(tiltKey))
				return;

			var titleLocalizations = new List<Localization> { new Localization { Culture = "en-US", Content = title } };
			var messageLocalizations = new List<Localization> { new Localization { Culture = "en-US", Content = message } };

			var result = new GameTilt(new GameTiltDefinition
			{
				Priority = ConvertPriority(priority),
				GamePlayBehavior = isBlocking ? GameTiltDefinitionGamePlayBehavior.Blocking : GameTiltDefinitionGamePlayBehavior.NonBlocking,
				DiscardBehavior = discardOnGameShutdown ? GameTiltDefinitionDiscardBehavior.OnGameTermination : GameTiltDefinitionDiscardBehavior.Never,
				UserInterventionRequired = userInterventionRequired,
				GameControlledProgressiveLinkDown = false,
				TitleLocalizations = titleLocalizations,
				MessageLocalizations = messageLocalizations
			});

			tiltManagement.PostTilt(result, tiltKey, null, null);

			GameTiltDefinitionPriority ConvertPriority(GameTiltPriority p)
			{
				switch (p)
				{
					case GameTiltPriority.Low: return GameTiltDefinitionPriority.Low;
					case GameTiltPriority.Med: return GameTiltDefinitionPriority.Medium;
					default:
					case GameTiltPriority.High: return GameTiltDefinitionPriority.High;
				}
			}
		}

		public void ClearTilt(string tiltKey)
		{
			var tiltManagement = GameLib.GetInterface<ITiltManagement>();
			if (tiltManagement == null)
				return;

			if (!tiltManagement.TiltPresent(tiltKey))
				return;

			tiltManagement.ClearTilt(tiltKey);
		}

		#endregion

		#region Win Capping

		public WinCapStyle WinCapStyle { get; private set; }
		public Money WinCapLimit => Money.FromMinorCurrency(GameLib.WinCapInformation.TotalWinCapLimit);

		#endregion
	}
}