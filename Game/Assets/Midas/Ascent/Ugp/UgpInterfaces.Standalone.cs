using IGT.Ascent.Communication.Platform.GameLib.Interfaces;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve;
using Midas.Core;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IStandaloneEventPosterDependency eventPoster;

		private void InitStandalone()
		{
			eventPoster = GameLib as IStandaloneEventPosterDependency;
			UpdateStandaloneConfig(StandaloneAustralianFoundationSettings.Load());
		}

		internal void UpdateStandaloneConfig(StandaloneAustralianFoundationSettings australianFoundationSettings, bool autoAwardChanged = true, bool advanceProgressiveAward = false)
		{
			eventPoster?.PostTransactionalEvent(new ThemeSelectionMenuOfferableStatusChangedEventArgs(australianFoundationSettings.MachineSettings.IsMoreGamesEnabled));

			if (!(machineConfiguration is IStandaloneHelperUgpMachineConfiguration standaloneMachineConfig))
				return;

			if (!(reserve is IStandaloneHelperUgpReserve standaloneReserve))
				return;

			if (!(ugpPid is IStandaloneHelperUgpPid standalonePid))
				return;

			if (!(externalJackpots is IStandaloneHelperUgpExternalJackpots standaloneExternalJackpots))
				return;

			if (!(progressiveAward is IStandaloneHelperUgpProgressiveAward standaloneProgAward))
				return;

			if (!(gameFunctionStatus is IStandaloneGameFunctionStatusHelper standaloneGameFunctionStatus))
				return;

			standaloneMachineConfig.SetMachineConfiguration(
				australianFoundationSettings.MachineSettings.IsClockVisible,
				australianFoundationSettings.MachineSettings.ClockFormat,
				australianFoundationSettings.MachineSettings.Tokenisation,
				australianFoundationSettings.MachineSettings.GameCycleTime,
				australianFoundationSettings.MachineSettings.IsContinuousPlayAllowed,
				australianFoundationSettings.MachineSettings.IsFeatureAutoStartEnabled,
				australianFoundationSettings.MachineSettings.CurrentMaximumBet,
				australianFoundationSettings.MachineSettings.WinCapStyle,
				australianFoundationSettings.MachineSettings.IsSlamSpinAllowed,
				australianFoundationSettings.MachineSettings.QcomJurisdiction,
				australianFoundationSettings.MachineSettings.CabinetId,
				australianFoundationSettings.MachineSettings.BrainBoxId,
				australianFoundationSettings.MachineSettings.Gpu);
			standaloneReserve.SetReserveConfiguration(
				australianFoundationSettings.ReserveSettings.IsReserveAllowedWithCredits,
				australianFoundationSettings.ReserveSettings.IsReserveAllowedWithoutCredits,
				australianFoundationSettings.ReserveSettings.ReserveTimeWithCreditsMilliseconds,
				australianFoundationSettings.ReserveSettings.ReserveTimeWithoutCreditsMilliseconds);
			standalonePid.SetPidConfiguration(australianFoundationSettings.PidSettings.ToPidConfiguration());
			standaloneExternalJackpots.SetExternalJackpots(
				australianFoundationSettings.ExternalJackpotSettings.IsVisible,
				australianFoundationSettings.ExternalJackpotSettings.IsIconVisible ? 1 : 0,
				australianFoundationSettings.ExternalJackpotSettings.ToExternalJackpots());
			standaloneGameFunctionStatus.SetGameFunctionStatusConfiguration(australianFoundationSettings.GameFunctionStatusSettings.ToDenominationMenuTimeoutConfiguration(),
				australianFoundationSettings.GameFunctionStatusSettings.DenominationPlayableStatuses,
				australianFoundationSettings.GameFunctionStatusSettings.GameButtonBehaviours);

#if UNITY_EDITOR
			if (autoAwardChanged)
				standaloneProgAward.SetManualControl(!australianFoundationSettings.ProgressiveAwardSettings.AutoAward);

			if (advanceProgressiveAward)
			{
				switch (GameLogic.GetProgressiveAwardWaitState(out var currentIndex))
				{
					case ProgressiveAwardWaitState.Verification: standaloneProgAward.SendVerified(currentIndex, australianFoundationSettings.ProgressiveAwardSettings.AwardAmount); break;
					case ProgressiveAwardWaitState.Payment: standaloneProgAward.SendPaid(currentIndex, australianFoundationSettings.ProgressiveAwardSettings.AwardAmount); break;
				}
			}
#else
			standaloneProgAward.SetManualControl(false);
#endif
		}
	}
}