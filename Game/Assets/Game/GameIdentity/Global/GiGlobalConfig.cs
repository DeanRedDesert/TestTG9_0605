using System;
using Game.GameIdentity.Common;
using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.WinPresentation;

namespace Game.GameIdentity.Global
{
	/// <summary>
	/// Configures status database and any other game identity specific things that are required.
	/// </summary>
	public sealed class GiGlobalConfig : GameIdentityConfig
	{
		private static readonly BetRatioWinRanges normalWinRanges = new BetRatioWinRanges(new[] { 3.0f, 5.0f, 10.0f, 25.0f, 50.0f, 100.0f }, IsLevelZeroWinAllowed);
		private static readonly BetRatioWinRanges highDenomWinRanges = new BetRatioWinRanges(new[] { 5.0f, 10.0f, 25.0f, 50.0f, 100.0f }, IsLevelZeroWinAllowed);
		private static readonly TimeSpan[] maxCycleTimings = { TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(0.7), TimeSpan.FromSeconds(0.3) };
		private static readonly TimeSpan[] maxBetweenWinTimings = { TimeSpan.FromSeconds(0.03), TimeSpan.FromSeconds(0.03), TimeSpan.FromSeconds(0.03) };
		private static readonly TimeSpan[] flashTimings = { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero };

		public GiGlobalConfig() : base(GameIdentityType.Global) { }

		protected override void GiInit()
		{
			StatusDatabase.DashboardStatus.VolumePopupEnabled = true;
			StatusDatabase.WinPresentationStatus.WinCountRanges = new GiGlobalWinCountRanges();
			StatusDatabase.WinPresentationStatus.DetailedWinPresCycleTimings = maxCycleTimings;
			StatusDatabase.WinPresentationStatus.DetailedWinPresFlashTimings = flashTimings;
			StatusDatabase.WinPresentationStatus.DetailedWinPresBetweenWinTimings = maxBetweenWinTimings;
			StatusDatabase.WinPresentationStatus.WinMeterResetTimeout = TimeSpan.FromSeconds(0.2);
		}

		protected override void RegisterForEvents(AutoUnregisterHelper autoUnregister)
		{
			base.RegisterForEvents(autoUnregister);

			autoUnregister.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.DenomConfig), OnDenomConfigChanged);
		}

		private static void OnDenomConfigChanged(StatusBlock sender, string propertyname)
		{
			var cs = StatusDatabase.ConfigurationStatus;
			var denomLevel = cs.DenomBetData[cs.DenomConfig.CurrentDenomination].DenomLevel;

			switch (denomLevel)
			{
				case DenomLevel.Low:
				case DenomLevel.Mid:
					StatusDatabase.WinPresentationStatus.WinRanges = normalWinRanges;
					break;

				case DenomLevel.High:
					StatusDatabase.WinPresentationStatus.WinRanges = highDenomWinRanges;
					break;
			}
		}

		private static bool IsLevelZeroWinAllowed()
		{
			return StatusDatabase.AutoPlayStatus.State == AutoPlayState.Active &&
				StatusDatabase.GameSpeedStatus.GameSpeed == GameSpeed.SuperFast;
		}
	}
}