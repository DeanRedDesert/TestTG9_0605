using System;
using Game.GameIdentity.Common;
using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;
using Midas.Presentation.WinPresentation;

namespace Game.GameIdentity.Anz
{
	/// <summary>
	/// Configures status database and any other game identity specific things that are required.
	/// </summary>
	public sealed class GiAnzConfig : GameIdentityConfig
	{
		/// <summary>
		/// This is based off the current ANZ big win ratios, with some higher levels removed due to only having
		/// three big win levels in global setup.
		/// </summary>
		private static readonly CreditOrBetRatioWinRanges normalWinRanges = new CreditOrBetRatioWinRanges(
			Money.FromMinorCurrency(1),
			new[] { 0L, 60, 100, 200, 499 }.ToCreditList(),
			new[] { 0f, 1.0f, 5.0f, 10.0f, 25.0f });

		private static readonly CreditOrBetRatioWinRanges lowDenomWinRanges = new CreditOrBetRatioWinRanges
		(
			Money.FromMinorCurrency(49),
			new[] { 0L, 21, 31, 41, 61, 81, 101, 151, 201, 301, 501, 1001, 1501, 2001, 3501, 5001, 5001, 5001, 5001, 5001 }.ToCreditList(),
			new[] { 0F, .15f, .25f, .35f, .5f, .65f, .8f, 1f, 1.5f, 2f, 3f, 5f, 7.5f, 10f, 15f, 25f, 35f, 50f, 70f, 100f }
		);

		private static readonly CreditOrBetRatioWinRanges highDenomWinRanges = new CreditOrBetRatioWinRanges
		(
			Money.FromMinorCurrency(49),
			new[] { 0L, 21, 31, 41, 81, 101, 151, 201, 301, 501, 1001, 1501, 2001, 5001, }.ToCreditList(),
			new[] { 0f, .5f, .75f, 1f, 2f, 3f, 4f, 5f, 7.5f, 10f, 15f, 25f, 35f, 50f, 70f, 100f }
		);

		private static readonly WinRangeBasedWinCountRanges lowDenomWinCountRanges = new WinRangeBasedWinCountRanges
		(
			lowDenomWinRanges,
			new[] { .8, 1, 1.5, 1.7, 2.2, 2.4, 2.9, 3.3, 3.8, 5.6, 7.5, 10.6, 14.6, 15.4, 22.4, 30.5, 30.9, 35.5, 44.2, 44.2 }.ToTimeSpanList(),
			new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.6, 1.6, 1.6, 1.6, 1.6, 1.6, 1.6 }.ToTimeSpanList());

		private static readonly WinRangeBasedWinCountRanges highDenomWinCountRanges = new WinRangeBasedWinCountRanges
		(
			highDenomWinRanges,
			new[] { .8, 1, 1.5, 1.7, 2.4, 2.9, 3.3, 5.6, 10.6, 14.6, 15.4, 22.4, 30.9, 35.5, 44.2, 44.2 }.ToTimeSpanList(),
			new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1.6, 1.6, 1.6, 1.6, 1.6, 1.6, 1.6, 1.6, 1.6, 1.6, 1.6 }.ToTimeSpanList());

		private static readonly BetRatioWinRanges featureLowDenomWinRanges = new BetRatioWinRanges
		(
			new[] { 0F, .5f, 1f, 2f, 3f, 5f, 10f, 25f, 100f }
		);

		private static readonly BetRatioWinRanges featureHighDenomWinRanges = new BetRatioWinRanges
		(
			new[] { 0f, 1f, 5f, 10f, 25f, 50f, 100f, 150f, 200f }
		);

		private static readonly WinRangeBasedWinCountRanges featureLowDenomWinCountRanges = new WinRangeBasedWinCountRanges
		(
			featureLowDenomWinRanges,
			new[] { 1.824, 3.2, 3.2, 4.5, 7.5, 17.5, 31.5, 41.5, 72.5 }.ToTimeSpanList(),
			new[] { 0, 0, 0, 0, 0, 0, 0, 1.6, 1.6 }.ToTimeSpanList());

		private static readonly WinRangeBasedWinCountRanges featureHighDenomWinCountRanges = new WinRangeBasedWinCountRanges
		(
			featureHighDenomWinRanges,
			new[] { 1.824, 3.2, 3.2, 4.5, 7.5, 17.5, 31.5, 41.5, 72.5 }.ToTimeSpanList(),
			new[] { 0, 0, 0, 0, 1.6, 1.6, 1.6, 1.6, 1.6 }.ToTimeSpanList());

		private static readonly FeatureWinRangeBasedWinCountRanges fldwcr = new FeatureWinRangeBasedWinCountRanges(lowDenomWinCountRanges, featureLowDenomWinCountRanges, 4);
		private static readonly FeatureWinRangeBasedWinCountRanges fhdwcr = new FeatureWinRangeBasedWinCountRanges(highDenomWinCountRanges, featureHighDenomWinCountRanges, 4);

		public GiAnzConfig(GameIdentityType gameIdentityType) : base(gameIdentityType) { }

		protected override void GiInit()
		{
			StatusDatabase.DashboardStatus.VolumePopupEnabled = false;
			StatusDatabase.WinPresentationStatus.WinMeterResetTimeout = TimeSpan.FromSeconds(StatusDatabase.ConfigurationStatus.GameIdentity!.Value.IsGlobalGi() ? 0.2 : 0.05);

			var winSequences = GameBase.GameInstance.GetPresentationController<CommonSequences>();
			for (var winLevel = (int)WinLevel.LNoCredit; winLevel < (int)WinLevel.EnumSize; ++winLevel)
			{
				var coinFlightIntensity = winLevel < (int)WinLevel.L4 ? 0 : 1;
				winSequences.MainWinPres.WinPresEventTable.SetCustomIntensity((int)SequenceEvent.CoinFlight, winLevel, coinFlightIntensity);
				winSequences.MainWinPres.WinPresEventTable.SetCustomIntensity((int)DefaultWinPresSequence.AdditionalSequenceEvents.TopAwardSound, winLevel, coinFlightIntensity);
			}

			winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.WinIncrement, (int)WinLevel.L0] = 1;
			winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.WinIncrementSound, (int)WinLevel.L0] = 1;
			winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.FeatureWinIncrementSound, (int)WinLevel.L0] = 1;
			winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.WinSequenceComplete, (int)WinLevel.L0] = 1;

			for (var winLevel = (int)WinLevel.L0; winLevel < (int)WinLevel.EnumSize; ++winLevel)
			{
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.BellSound, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.FrameLightAnimation, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.ReelScreenZoom, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.ReelScreenShake, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.ReelScreenShakeSound, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.BackgroundLoopSound, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.WinMeterZoom, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.WinMeterEffect, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.WinDumpAndSpaghettiLines, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.WinDumpSound, winLevel] = 0;
				winSequences.MainWinPres.WinPresEventTable.Entries[(int)SequenceEvent.WinMessageAndAnim, winLevel] = 0;
			}
		}

		protected override void RegisterForEvents(AutoUnregisterHelper autoUnregister)
		{
			base.RegisterForEvents(autoUnregister);

			autoUnregister.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.DenomConfig), OnDenomConfigChanged);
			autoUnregister.RegisterPropertyChangedHandler(StatusDatabase.StageStatus, nameof(StageStatus.CurrentStage), OnDenomConfigChanged);
		}

		private static void OnDenomConfigChanged(StatusBlock sender, string propertyname)
		{
			var cs = StatusDatabase.ConfigurationStatus;
			var denomLevel = cs.DenomBetData[cs.DenomConfig.CurrentDenomination].DenomLevel;

			if (StatusDatabase.StageStatus.CurrentStage != GameBase.GameInstance.BaseGameStage)
			{
				switch (denomLevel)
				{
					case DenomLevel.Low:
					case DenomLevel.Mid:
						StatusDatabase.WinPresentationStatus.WinRanges = normalWinRanges;
						StatusDatabase.WinPresentationStatus.WinCountRanges = fldwcr;
						break;

					case DenomLevel.High:
						StatusDatabase.WinPresentationStatus.WinRanges = normalWinRanges;
						StatusDatabase.WinPresentationStatus.WinCountRanges = fhdwcr;
						break;
				}

				return;
			}

			switch (denomLevel)
			{
				case DenomLevel.Low:
				case DenomLevel.Mid:
					StatusDatabase.WinPresentationStatus.WinRanges = normalWinRanges;
					StatusDatabase.WinPresentationStatus.WinCountRanges = lowDenomWinCountRanges;
					break;

				case DenomLevel.High:
					StatusDatabase.WinPresentationStatus.WinRanges = normalWinRanges;
					StatusDatabase.WinPresentationStatus.WinCountRanges = highDenomWinCountRanges;
					break;
			}
		}
	}
}