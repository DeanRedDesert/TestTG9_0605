using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.General;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public struct ReserveParameters
	{
		public bool IsAvailable => AllowedWithCredits || AllowedWithoutCredits;
		public bool AllowedWithCredits { get; }
		public bool AllowedWithoutCredits { get; }
		public TimeSpan TimeoutWithCredits { get; }
		public TimeSpan TimeoutWithoutCredits { get; }

		public ReserveParameters(bool allowedWithCredits, bool allowedWithoutCredits, TimeSpan timeoutWithCredits, TimeSpan timeoutWithoutCredits)
		{
			AllowedWithCredits = allowedWithCredits;
			AllowedWithoutCredits = allowedWithoutCredits;
			TimeoutWithCredits = timeoutWithCredits;
			TimeoutWithoutCredits = timeoutWithoutCredits;
		}

		public override string ToString() => $"Avail: {IsAvailable}, WithCred: {AllowedWithCredits} (${TimeoutWithCredits}), NoCred: ${AllowedWithoutCredits} (${TimeoutWithoutCredits})";
	}

	public sealed class Configuration : CompositeGameService
	{
		internal readonly GameService<ConfigData> ConfigurationDataService = new GameService<ConfigData>(HistorySnapshotType.None);
		internal readonly GameService<ReserveParameters> ReserveParametersService = new GameService<ReserveParameters>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyDictionary<Money, IDenomBetData>> DenomBetDataService = new GameService<IReadOnlyDictionary<Money, IDenomBetData>>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsClockVisibleService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<string> ClockFormatService = new GameService<string>(HistorySnapshotType.GameStart);
		internal readonly GameService<TimeSpan> BaseGameCycleTimeService = new GameService<TimeSpan>(HistorySnapshotType.None);
		internal readonly GameService<TimeSpan> FreeGameCycleTimeService = new GameService<TimeSpan>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsContinuousPlayAllowedService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsFeatureAutoStartEnabledService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<Credit> GameMaximumBetService = new GameService<Credit>(HistorySnapshotType.GameStart);
		internal readonly GameService<bool> IsSlamSpinAllowedService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsSlamSpinAllowedInFeaturesService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsSlamSpinImmediateService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> AllowSlamSpinDPPButtonService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> RecordSlamSpinUsedService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<int> QcomJurisdictionService = new GameService<int>(HistorySnapshotType.GameStart);
		internal readonly GameService<string> CabinetIdService = new GameService<string>(HistorySnapshotType.GameStart);
		internal readonly GameService<string> BrainboxIdService = new GameService<string>(HistorySnapshotType.GameStart);
		internal readonly GameService<string> GpuService = new GameService<string>(HistorySnapshotType.GameStart);
		internal readonly GameService<string> CurrentLanguageService = new GameService<string>(HistorySnapshotType.None);
		internal readonly GameService<string> CurrentFlagService = new GameService<string>(HistorySnapshotType.None);
		internal readonly GameService<FlashingPlayerClock> FlashingPlayerClockService = new GameService<FlashingPlayerClock>(HistorySnapshotType.None);

		public IGameServiceConsumer<ConfigData> ConfigurationData => ConfigurationDataService.Variable;
		public IGameServiceConsumer<ReserveParameters> ReserveParameters => ReserveParametersService.Variable;
		public IGameServiceConsumer<IReadOnlyDictionary<Money, IDenomBetData>> DenomBetData => DenomBetDataService.Variable;
		public IGameServiceConsumer<bool> IsClockVisible => IsClockVisibleService.Variable;
		public IGameServiceConsumer<string> ClockFormat => ClockFormatService.Variable;
		public IGameServiceConsumer<TimeSpan> BaseGameCycleTime => BaseGameCycleTimeService.Variable;
		public IGameServiceConsumer<TimeSpan> FreeGameCycleTime => FreeGameCycleTimeService.Variable;
		public IGameServiceConsumer<bool> IsContinuousPlayAllowed => IsContinuousPlayAllowedService.Variable;
		public IGameServiceConsumer<bool> IsSlamSpinAllowedInFeatures => IsSlamSpinAllowedInFeaturesService.Variable;
		public IGameServiceConsumer<bool> IsSlamSpinImmediate => IsSlamSpinImmediateService.Variable;
		public IGameServiceConsumer<bool> AllowSlamSpinDPPButton => AllowSlamSpinDPPButtonService.Variable;
		public IGameServiceConsumer<bool> RecordSlamSpinUsed => RecordSlamSpinUsedService.Variable;
		public IGameServiceConsumer<bool> IsFeatureAutoStartEnabled => IsFeatureAutoStartEnabledService.Variable;
		public IGameServiceConsumer<Credit> GameMaximumBet => GameMaximumBetService.Variable;
		public IGameServiceConsumer<bool> IsSlamSpinAllowed => IsSlamSpinAllowedService.Variable;
		public IGameServiceConsumer<int> QcomJurisdiction => QcomJurisdictionService.Variable;
		public IGameServiceConsumer<string> CabinetId => CabinetIdService.Variable;
		public IGameServiceConsumer<string> BrainboxId => BrainboxIdService.Variable;
		public IGameServiceConsumer<string> Gpu => GpuService.Variable;
		public IGameServiceConsumer<string> CurrentLanguage => CurrentLanguageService.Variable;
		public IGameServiceConsumer<string> CurrentFlag => CurrentFlagService.Variable;
		public IGameServiceConsumer<FlashingPlayerClock> FlashingPlayerClock => FlashingPlayerClockService.Variable;

		protected override void CreateServices()
		{
			AddService(ConfigurationDataService, nameof(ConfigurationData));
			AddService(ReserveParametersService, nameof(ReserveParameters));
			AddService(DenomBetDataService, nameof(DenomBetData));
			AddService(IsClockVisibleService, nameof(IsClockVisible));
			AddService(ClockFormatService, nameof(ClockFormat));
			AddService(BaseGameCycleTimeService, nameof(BaseGameCycleTime));
			AddService(FreeGameCycleTimeService, nameof(FreeGameCycleTime));
			AddService(IsContinuousPlayAllowedService, nameof(IsContinuousPlayAllowed));
			AddService(IsFeatureAutoStartEnabledService, nameof(IsFeatureAutoStartEnabled));
			AddService(GameMaximumBetService, nameof(GameMaximumBet));
			AddService(IsSlamSpinAllowedService, nameof(IsSlamSpinAllowed));
			AddService(IsSlamSpinAllowedInFeaturesService, nameof(IsSlamSpinAllowedInFeatures));
			AddService(IsSlamSpinImmediateService, nameof(IsSlamSpinImmediate));
			AddService(AllowSlamSpinDPPButtonService, nameof(AllowSlamSpinDPPButton));
			AddService(RecordSlamSpinUsedService, nameof(RecordSlamSpinUsed));
			AddService(QcomJurisdictionService, nameof(QcomJurisdiction));
			AddService(CabinetIdService, nameof(CabinetId));
			AddService(BrainboxIdService, nameof(BrainboxId));
			AddService(GpuService, nameof(Gpu));
			AddService(CurrentLanguageService, nameof(CurrentLanguage));
			AddService(CurrentFlagService, nameof(CurrentFlag));
			AddService(FlashingPlayerClockService, nameof(FlashingPlayerClock));
		}
	}
}