using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Data.Services;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.General;

namespace Midas.Presentation.Data.StatusBlocks
{
	public sealed class ConfigurationStatus : StatusBlock
	{
		private StatusProperty<MachineConfig> machineConfig;
		private StatusProperty<CurrencyConfig> currencyConfig;
		private StatusProperty<DenomConfig> denomConfig;
		private StatusProperty<AncillaryConfig> ancillaryConfig;
		private StatusProperty<LanguageConfig> languageConfig;
		private StatusProperty<GameConfig> gameConfig;
		private StatusProperty<CustomConfig> customConfig;
		private StatusProperty<ReserveParameters> reserveParameters;
		private StatusProperty<ServiceConfig> serviceConfig;
		private StatusProperty<IReadOnlyDictionary<Money, IDenomBetData>> denomBetData;
		private StatusProperty<CreditAndMoneyFormatter> creditAndMoneyFormatter;
		private StatusProperty<IReadOnlyList<MonitorType>> configuredMonitors;
		private StatusProperty<GameIdentityType?> gameIdentity;
		private StatusProperty<(Money TokenVal, Credit TokenCredit)> tokenization;
		private StatusProperty<bool> isClockVisible;
		private StatusProperty<string> clockFormat;
		private StatusProperty<bool> isChooserAvailable;

		private StatusProperty<TimeSpan> baseGameCycleTime;
		private StatusProperty<TimeSpan> freeGameCycleTime;
		private StatusProperty<bool> isContinuousPlayAllowed;
		private StatusProperty<bool> isSlamSpinAllowedInFeatures;
		private StatusProperty<bool> isSlamSpinImmediate;
		private StatusProperty<bool> allowSlamSpinDppButton;
		private StatusProperty<bool> recordSlamSpinUsed;
		private StatusProperty<bool> isFeatureAutoStartEnabled;
		private StatusProperty<Credit> gameMaximumBet;
		private StatusProperty<bool> isSlamSpinAllowed;
		private StatusProperty<int> qcomJurisdiction;
		private StatusProperty<string> cabinetId;
		private StatusProperty<string> brainboxId;
		private StatusProperty<string> gpu;
		private StatusProperty<string> currentLanguage;
		private StatusProperty<string> currentFlag;
		private StatusProperty<FlashingPlayerClock> flashingPlayerClock;

		public MachineConfig MachineConfig => machineConfig.Value;
		public CurrencyConfig CurrencyConfig => currencyConfig.Value;
		public DenomConfig DenomConfig => denomConfig.Value;
		public AncillaryConfig AncillaryConfig => ancillaryConfig.Value;
		public LanguageConfig LanguageConfig => languageConfig.Value;
		public GameConfig GameConfig => gameConfig.Value;
		public CustomConfig CustomConfig => customConfig.Value;
		public ReserveParameters ReserveParameters => reserveParameters.Value;
		public ServiceConfig ServiceConfig => serviceConfig.Value;
		public IReadOnlyDictionary<Money, IDenomBetData> DenomBetData => denomBetData.Value;
		public CreditAndMoneyFormatter CreditAndMoneyFormatter => creditAndMoneyFormatter.Value;
		public (Money TokenVal, Credit TokenCredit) Tokenization => tokenization.Value;

		public bool IsClockVisible => isClockVisible.Value;
		public string ClockFormat => clockFormat.Value;
		public bool IsChooserAvailable => isChooserAvailable.Value;
		public TimeSpan BaseGameCycleTime => baseGameCycleTime.Value;
		public TimeSpan FreeGameCycleTime => freeGameCycleTime.Value;
		public bool IsContinuousPlayAllowed => isContinuousPlayAllowed.Value;
		public bool IsSlamSpinAllowedInFeatures => isSlamSpinAllowedInFeatures.Value;
		public bool IsSlamSpinImmediate => isSlamSpinImmediate.Value;
		public bool AllowSlamSpinDppButton => allowSlamSpinDppButton.Value;
		public bool RecordSlamSpinUsed => recordSlamSpinUsed.Value;
		public bool IsFeatureAutoStartEnabled => isFeatureAutoStartEnabled.Value;
		public Credit GameMaximumBet => gameMaximumBet.Value;
		public bool IsSlamSpinAllowed => isSlamSpinAllowed.Value;
		public int QcomJurisdiction => qcomJurisdiction.Value;
		public string CabinetId => cabinetId.Value;
		public string BrainboxId => brainboxId.Value;
		public string Gpu => gpu.Value;
		public string CurrentLanguage => currentLanguage.Value;
		public string CurrentFlag => currentFlag.Value;
		public FlashingPlayerClock FlashingPlayerClock => flashingPlayerClock.Value;

		public IReadOnlyList<MonitorType> ConfiguredMonitors
		{
			get => configuredMonitors.Value;
			set => configuredMonitors.Value = value;
		}

		public GameIdentityType? GameIdentity
		{
			get => gameIdentity.Value;
			set
			{
				if (gameIdentity.Value.HasValue)
					Log.Instance.Fatal("GameIdentity may only be set once");

				gameIdentity.Value = value;
			}
		}

		public event Action MoneyAndCreditFormatChanged;

		public ConfigurationStatus() : base(nameof(ConfigurationStatus))
		{
		}

		/// <summary>
		/// TODO: Implement game speed properly
		/// TODO: adjust this value to account for framework overhead
		/// </summary>
		public TimeSpan GetGameTime()
		{
			return StatusDatabase.QueryStatusBlock<GameResultStatus>().IsBaseGameCycle() ? BaseGameCycleTime : FreeGameCycleTime;
		}

		[Expression("Configuration")]
		public static IDenomBetData CurrentDenomBetData
		{
			get
			{
				var currentDenom = StatusDatabase.ConfigurationStatus.DenomConfig?.CurrentDenomination;
				if (currentDenom == null)
					return null;

				IDenomBetData result = null;

				StatusDatabase.ConfigurationStatus.DenomBetData?.TryGetValue(currentDenom.Value, out result);
				return result;
			}
		}

		protected override void RegisterForEvents(AutoUnregisterHelper autoUnregisterHelper)
		{
			base.RegisterForEvents(autoUnregisterHelper);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.ConfigurationData, OnConfigurationChanged);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.ReserveParameters, p => reserveParameters.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.DenomBetData, p => denomBetData.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.IsClockVisible, p => isClockVisible.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.ClockFormat, p => clockFormat.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.BaseGameCycleTime, p => baseGameCycleTime.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.FreeGameCycleTime, p => freeGameCycleTime.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.IsContinuousPlayAllowed, p => isContinuousPlayAllowed.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.IsSlamSpinAllowedInFeatures, p => isSlamSpinAllowedInFeatures.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.IsSlamSpinImmediate, p => isSlamSpinImmediate.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.AllowSlamSpinDPPButton, p => allowSlamSpinDppButton.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.RecordSlamSpinUsed, p => recordSlamSpinUsed.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.IsFeatureAutoStartEnabled, p => isFeatureAutoStartEnabled.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.GameMaximumBet, p => gameMaximumBet.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.IsSlamSpinAllowed, p => isSlamSpinAllowed.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.QcomJurisdiction, p => qcomJurisdiction.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.CabinetId, p => cabinetId.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.BrainboxId, p => brainboxId.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.Gpu, p => gpu.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.IsChooserAvailable, p => isChooserAvailable.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.CurrentLanguage, p => currentLanguage.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.CurrentFlag, p => currentFlag.Value = p);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.ConfigurationService.FlashingPlayerClock, p => flashingPlayerClock.Value = p);
		}

		public void UpdateServiceConfig(ServiceConfig config) => serviceConfig.Value = config;

		private void OnConfigurationChanged(ConfigData configData)
		{
			machineConfig.Value = configData?.MachineConfig;
			currencyConfig.Value = configData?.CurrencyConfig;
			denomConfig.Value = configData?.DenomConfig;
			ancillaryConfig.Value = configData?.AncillaryConfig;
			languageConfig.Value = configData?.LanguageConfig;
			gameConfig.Value = configData?.GameConfig;
			customConfig.Value = configData?.CustomConfig;
			creditAndMoneyFormatter.Value = configData == null ? null : new CreditAndMoneyFormatter(configData.CurrencyConfig);
			tokenization.Value = GenerateTokenValue();
			MoneyAndCreditFormatChanged?.Invoke();
		}

		private (Money TokenVal, Credit TokenCredit) GenerateTokenValue()
		{
			return DenomConfig.CurrentDenomination >= DenomConfig.TokenValue
				? (DenomConfig.CurrentDenomination, Credit.FromLong(1))
				: (DenomConfig.TokenValue, DenomConfig.TokenValue.ToCredit());
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			machineConfig = AddProperty<MachineConfig>(nameof(MachineConfig), null);
			currencyConfig = AddProperty<CurrencyConfig>(nameof(CurrencyConfig), null);
			denomConfig = AddProperty<DenomConfig>(nameof(DenomConfig), null);
			ancillaryConfig = AddProperty<AncillaryConfig>(nameof(AncillaryConfig), null);
			languageConfig = AddProperty<LanguageConfig>(nameof(LanguageConfig), null);
			gameConfig = AddProperty<GameConfig>(nameof(GameConfig), null);
			customConfig = AddProperty<CustomConfig>(nameof(CustomConfig), null);
			reserveParameters = AddProperty<ReserveParameters>(nameof(ReserveParameters), default);
			serviceConfig = AddProperty<ServiceConfig>(nameof(ServiceConfig), default);
			denomBetData = AddProperty<IReadOnlyDictionary<Money, IDenomBetData>>(nameof(DenomBetData), default);
			creditAndMoneyFormatter = AddProperty<CreditAndMoneyFormatter>(nameof(CreditAndMoneyFormatter), null);
			configuredMonitors = AddProperty<IReadOnlyList<MonitorType>>(nameof(ConfiguredMonitors), null);
			tokenization = AddProperty(nameof(Tokenization), (Money.Zero, Credit.Zero));
			gameIdentity = AddProperty<GameIdentityType?>(nameof(GameIdentity), null);
			isChooserAvailable = AddProperty(nameof(IsChooserAvailable), false);

			isClockVisible = AddProperty(nameof(IsClockVisible), false);
			clockFormat = AddProperty<string>(nameof(ClockFormat), null);
			baseGameCycleTime = AddProperty(nameof(BaseGameCycleTime), TimeSpan.Zero);
			freeGameCycleTime = AddProperty(nameof(FreeGameCycleTime), TimeSpan.Zero);
			isContinuousPlayAllowed = AddProperty(nameof(IsContinuousPlayAllowed), false);
			isSlamSpinAllowedInFeatures = AddProperty(nameof(IsSlamSpinAllowedInFeatures), false);
			isSlamSpinImmediate = AddProperty(nameof(IsSlamSpinImmediate), false);
			allowSlamSpinDppButton = AddProperty(nameof(AllowSlamSpinDppButton), false);
			recordSlamSpinUsed = AddProperty(nameof(RecordSlamSpinUsed), false);
			isFeatureAutoStartEnabled = AddProperty(nameof(IsFeatureAutoStartEnabled), false);
			gameMaximumBet = AddProperty(nameof(GameMaximumBet), Credit.Zero);
			isSlamSpinAllowed = AddProperty(nameof(IsSlamSpinAllowed), false);
			qcomJurisdiction = AddProperty(nameof(QcomJurisdiction), 0);
			cabinetId = AddProperty<string>(nameof(CabinetId), null);
			brainboxId = AddProperty<string>(nameof(BrainboxId), null);
			gpu = AddProperty<string>(nameof(Gpu), null);
			currentLanguage = AddProperty<string>(nameof(CurrentLanguage), null);
			currentFlag = AddProperty<string>(nameof(CurrentFlag), null);
			flashingPlayerClock = AddProperty<FlashingPlayerClock>(nameof(Core.Configuration.FlashingPlayerClock), null);
		}
	}
}