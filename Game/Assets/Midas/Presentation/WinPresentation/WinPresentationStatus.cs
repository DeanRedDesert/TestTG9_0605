using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.General;
using Midas.Presentation.Data;

namespace Midas.Presentation.WinPresentation
{
	public sealed class WinPresentationStatus : StatusBlock
	{
		public static Money WinMeterInitialValue => StatusDatabase.WinPresentationStatus.CustomWinInitialValue ?? WinMeterFinalValue - StatusDatabase.BankStatus.CycleAward;
		public static Money WinMeterFinalValue => StatusDatabase.WinPresentationStatus.CustomWinFinalValue ?? StatusDatabase.BankStatus.TotalAward;
		public static Money CurrentWinValue => WinMeterFinalValue - WinMeterInitialValue;

		#region Public

		public WinPresentationStatus()
			: base(nameof(WinPresentationStatus))
		{
		}

		public bool WinPresentationComplete
		{
			get => winPresentationComplete.Value;
			set => winPresentationComplete.Value = value;
		}

		public IWinRanges WinRanges
		{
			get => winRanges.Value;
			set => winRanges.Value = value;
		}

		public IWinCountRanges WinCountRanges
		{
			get => winCountRanges.Value;
			set => winCountRanges.Value = value;
		}

		public IReadOnlyList<TimeSpan> DetailedWinPresCycleTimings
		{
			get => detailedWinPresCycleTimings.Value;
			set => detailedWinPresCycleTimings.Value = value.ToArray();
		}

		public IReadOnlyList<TimeSpan> DetailedWinPresDisplayTimings
		{
			get => detailedWinPresDisplayTimings.Value;
			set => detailedWinPresDisplayTimings.Value = value.ToArray();
		}

		public IReadOnlyList<TimeSpan> DetailedWinPresFlashTimings
		{
			get => detailedWinPresFlashTimings.Value;
			set => detailedWinPresFlashTimings.Value = value.ToArray();
		}

		public IReadOnlyList<TimeSpan> DetailedWinPresBetweenWinTimings
		{
			get => detailedWinPresBetweenWinTimings.Value;
			set => detailedWinPresBetweenWinTimings.Value = value.ToArray();
		}

		public TimeSpan CountingTime
		{
			get => countingTime.Value;
			set => countingTime.Value = value;
		}

		public int CountingIntensity
		{
			get => countingIntensity.Value;
			set => countingIntensity.Value = value;
		}

		public TimeSpan CountingDelayTime
		{
			get => countingDelayTime.Value;
			set => countingDelayTime.Value = value;
		}

		public TimeSpan WinMeterResetTimeout
		{
			get => winMeterResetTimeout.Value;
			set => winMeterResetTimeout.Value = value;
		}

		public bool WinPresActive
		{
			get => winPresActive.Value;
			set => winPresActive.Value = value;
		}

		public TimeSpan DetailedWinPresCycleTime => detailedWinPresCycleTimings.Value[(int)StatusDatabase.GameSpeedStatus.GameSpeed];

		public TimeSpan DetailedWinPresDisplayTime => detailedWinPresDisplayTimings.Value[currentWinLevel.Value == 0 ? 3 : (int)StatusDatabase.GameSpeedStatus.GameSpeed];

		public TimeSpan DetailedWinPresFlashTime => detailedWinPresFlashTimings.Value[currentWinLevel.Value == 0 ? 3 : (int)StatusDatabase.GameSpeedStatus.GameSpeed];

		public TimeSpan DetailedWinPresBetweenWinsDisplayTime => detailedWinPresBetweenWinTimings.Value[(int)StatusDatabase.GameSpeedStatus.GameSpeed];

		public int CurrentWinLevel
		{
			get => currentWinLevel.Value;
			set => currentWinLevel.Value = value;
		}

		public Money? CustomWinInitialValue
		{
			get => customWinInitialValue.Value;
			set => customWinInitialValue.Value = value;
		}

		public Money? CustomWinFinalValue
		{
			get => customWinFinalValue.Value;
			set => customWinFinalValue.Value = value;
		}

		#endregion

		#region Protected

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			winPresentationComplete = AddProperty(nameof(WinPresentationComplete), false);
			winPresActive = AddProperty(nameof(WinPresActive), false);
			winRanges = AddProperty(nameof(WinRanges), default(IWinRanges));
			winCountRanges = AddProperty(nameof(WinCountRanges), default(IWinCountRanges));
			detailedWinPresCycleTimings = AddProperty(nameof(DetailedWinPresCycleTimings), defaultDetailedWinPresCycleTime);
			detailedWinPresDisplayTimings = AddProperty(nameof(DetailedWinPresDisplayTimings), defaultDetailedWinPresDisplayTime);
			detailedWinPresFlashTimings = AddProperty(nameof(DetailedWinPresFlashTimings), defaultDetailedWinPresFlashTime);
			detailedWinPresBetweenWinTimings = AddProperty(nameof(DetailedWinPresBetweenWinTimings), defaultDetailedWinPresBetweenWinTime);
			currentWinLevel = AddProperty(nameof(CurrentWinLevel), -1);
			countingTime = AddProperty(nameof(CountingTime), TimeSpan.Zero);
			countingIntensity = AddProperty(nameof(CountingIntensity), -1);
			countingDelayTime = AddProperty(nameof(CountingDelayTime), TimeSpan.Zero);
			winMeterResetTimeout = AddProperty(nameof(WinMeterResetTimeout), TimeSpan.Zero);
			customWinInitialValue = AddProperty(nameof(CustomWinInitialValue), default(Money?));
			customWinFinalValue = AddProperty(nameof(CustomWinFinalValue), default(Money?));
		}

		#endregion

		#region Private

		private StatusProperty<bool> winPresentationComplete;
		private StatusProperty<IWinRanges> winRanges;
		private StatusProperty<IWinCountRanges> winCountRanges;
		private StatusProperty<TimeSpan[]> detailedWinPresDisplayTimings;
		private StatusProperty<TimeSpan[]> detailedWinPresFlashTimings;
		private StatusProperty<TimeSpan[]> detailedWinPresCycleTimings;
		private StatusProperty<TimeSpan[]> detailedWinPresBetweenWinTimings;
		private StatusProperty<bool> winPresActive;
		private StatusProperty<TimeSpan> countingTime;
		private StatusProperty<TimeSpan> countingDelayTime;
		private StatusProperty<TimeSpan> winMeterResetTimeout;
		private StatusProperty<int> countingIntensity;
		private StatusProperty<int> currentWinLevel;
		private StatusProperty<Money?> customWinInitialValue;
		private StatusProperty<Money?> customWinFinalValue;

		private static readonly TimeSpan[] defaultDetailedWinPresDisplayTime =
		{
			TimeSpan.FromSeconds(0.8),
			TimeSpan.FromSeconds(0.5),
			TimeSpan.FromSeconds(0.3),
			TimeSpan.FromSeconds(1.0),
		};

		private static readonly TimeSpan[] defaultDetailedWinPresFlashTime =
		{
			TimeSpan.FromSeconds(0.4),
			TimeSpan.FromSeconds(0.0),
			TimeSpan.FromSeconds(0.0),
			TimeSpan.FromSeconds(0.0),
		};

		private static readonly TimeSpan[] defaultDetailedWinPresCycleTime =
		{
			TimeSpan.FromSeconds(0.0),
			TimeSpan.FromSeconds(0.0),
			TimeSpan.FromSeconds(0.0),
		};

		private static readonly TimeSpan[] defaultDetailedWinPresBetweenWinTime =
		{
			TimeSpan.FromSeconds(0.4),
			TimeSpan.FromSeconds(0.0),
			TimeSpan.FromSeconds(0.0),
		};

		#endregion
	}
}