using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Ascent.Communication.Platform.GameLib.Interfaces;
using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Ascent.OutcomeList.Interfaces;
using IGT.Game.Core.Communication.Foundation;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.GameFunctionStatus;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve;
using IGT.Game.SDKAssets.AscentBuildSettings;
using Midas.Ascent.Ugp;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.General;
using static Midas.Ascent.AscentFoundation;
using ProgressiveAwardPayType = Midas.Core.ProgressiveAwardPayType;

namespace Midas.Ascent
{
	internal static partial class AscentGameEngine
	{
		private static IReadOnlyDictionary<int, string> progressiveLevelToId;
		private static IReadOnlyDictionary<string, int> progressiveIdToLevel;

		private static void RegisterGameServiceDataEvents()
		{
			if (GameLib.GameContextMode != GameMode.Play)
				return;

			GameLib.BankStatusChangedEvent += OnBankStatusChangedEvent;
			GameLib.MoneyEvent += OnMoneyEvent;
			GameLib.ProgressiveBroadcastEvent += OnProgressiveBroadcastEvent;
			GameLib.ThemeSelectionMenuOfferableStatusChangedEvent += OnGameLibThemeSelectionMenuOfferableStatusChangedEvent;
			GameLib.AutoPlayOnRequestEvent += OnAutoplayOnRequestEvent;
			GameLib.AutoPlayOffEvent += OnAutoplayOffEvent;
			GameLib.LanguageChangedEvent += OnLanguageChangedEvent;
			AscentFoundation.UgpInterfaces.MachineConfigurationChanged += OnMachineConfigurationChanged;
			AscentFoundation.UgpInterfaces.ReserveParametersChanged += OnReserveParametersChanged;
			AscentFoundation.UgpInterfaces.MessageAdded += OnMessageAdded;
			AscentFoundation.UgpInterfaces.MessageRemoved += OnMessageRemoved;
			AscentFoundation.UgpInterfaces.ExternalJackpotChanged += OnExternalJackpotChanged;
			AscentFoundation.UgpInterfaces.PidConfigurationChanged += OnPidConfigurationChanged;
			AscentFoundation.UgpInterfaces.IsServiceRequestedChanged += OnServiceRequestedChange;
			AscentFoundation.UgpInterfaces.DenominationMenuTimeoutSet += OnUgpInterfacesDenominationMenuTimeoutSet;
			AscentFoundation.UgpInterfaces.GameButtonBehaviorTypeChange += OnUgpInterfacesGameButtonBehaviorTypeChange;
			AscentFoundation.UgpInterfaces.DenominationPlayableStatusChange += OnUgpInterfacesDenominationPlayableStatusChange;
			AscentFoundation.PlayerSessionInterfaces.PlayerSessionStatusChanged += OnPlayerSessionPlayerSessionStatusChangedEvent;
			AscentFoundation.PlayerSessionInterfaces.CurrentResetParametersChanged += OnPlayerSessionParamsCurrentResetParametersChangedEvent;
			AscentFoundation.FlashingPlayerClockInterfaces.FlashingClockChanged += OnFlashingPlayerClockInterfacesFlashingClockChanged;
		}

		private static void UnregisterGameServiceDataEvents()
		{
			GameLib.BankStatusChangedEvent -= OnBankStatusChangedEvent;
			GameLib.MoneyEvent -= OnMoneyEvent;
			GameLib.ProgressiveBroadcastEvent -= OnProgressiveBroadcastEvent;
			GameLib.ThemeSelectionMenuOfferableStatusChangedEvent -= OnGameLibThemeSelectionMenuOfferableStatusChangedEvent;
			GameLib.AutoPlayOnRequestEvent -= OnAutoplayOnRequestEvent;
			GameLib.AutoPlayOffEvent -= OnAutoplayOffEvent;
			GameLib.LanguageChangedEvent -= OnLanguageChangedEvent;
			AscentFoundation.UgpInterfaces.MachineConfigurationChanged -= OnMachineConfigurationChanged;
			AscentFoundation.UgpInterfaces.ReserveParametersChanged -= OnReserveParametersChanged;
			AscentFoundation.UgpInterfaces.MessageAdded -= OnMessageAdded;
			AscentFoundation.UgpInterfaces.MessageRemoved -= OnMessageRemoved;
			AscentFoundation.UgpInterfaces.ExternalJackpotChanged -= OnExternalJackpotChanged;
			AscentFoundation.UgpInterfaces.PidConfigurationChanged -= OnPidConfigurationChanged;
			AscentFoundation.UgpInterfaces.IsServiceRequestedChanged -= OnServiceRequestedChange;
			AscentFoundation.UgpInterfaces.DenominationMenuTimeoutSet -= OnUgpInterfacesDenominationMenuTimeoutSet;
			AscentFoundation.UgpInterfaces.GameButtonBehaviorTypeChange -= OnUgpInterfacesGameButtonBehaviorTypeChange;
			AscentFoundation.UgpInterfaces.DenominationPlayableStatusChange -= OnUgpInterfacesDenominationPlayableStatusChange;
			AscentFoundation.PlayerSessionInterfaces.PlayerSessionStatusChanged -= OnPlayerSessionPlayerSessionStatusChangedEvent;
			AscentFoundation.PlayerSessionInterfaces.CurrentResetParametersChanged -= OnPlayerSessionParamsCurrentResetParametersChangedEvent;
			AscentFoundation.FlashingPlayerClockInterfaces.FlashingClockChanged -= OnFlashingPlayerClockInterfacesFlashingClockChanged;
		}

		private static void InitGameServiceData()
		{
			progressiveLevelToId = GameLogic.ProgressiveLevels.ToDictionary(p => (int)p.GameLevel, p => p.LevelId);
			progressiveIdToLevel = GameLogic.ProgressiveLevels.ToDictionary(p => p.LevelId, p => (int)p.GameLevel);

			if (GameLib.GameContextMode != GameMode.Play)
			{
				RefreshExternalJackpot(new ExternalJackpots());
				return;
			}

			UpdateMeters(MoneyEvent.MoneySet, GameLib.GetPlayerMeters());
			InitTotalAward();
			GameLogic.SetBankStatus(GameLib.IsPlayerWagerOfferable, GameLib.IsCashOutOfferable);
			InitIsChooserAvailable();
			RefreshMachineConfiguration(AscentFoundation.UgpInterfaces.GetMachineConfigurationParameters());
			RefreshReserveParameters(AscentFoundation.UgpInterfaces.GetReserveParameters());
			RefreshMessageStrip(AscentFoundation.UgpInterfaces.GetMessages().ToArray());
			RefreshProgressives(GameLib.GetAvailableProgressiveBroadcastData(GameLib.GameDenomination));
			RefreshProgressiveAward();
			RefreshExternalJackpot(AscentFoundation.UgpInterfaces.GetExternalJackpots());
			RefreshPidConfiguration();
			RefreshPidSession();
			RefreshGameFunctionStatus();
			RefreshProgressiveLevels();
			RefreshCurrentLanguage(GameLib.GameLanguage);
			RefreshPlayerSession();
			RefreshPlayerSessionParameters();
			RefreshFlashingPlayerClock();
		}

		private static void RefreshFlashingPlayerClock()
		{
			var config = Ascent.AscentFoundation.FlashingPlayerClockInterfaces.GetConfig();
			var props = Ascent.AscentFoundation.FlashingPlayerClockInterfaces.GetProperties();
			GameLogic.SetFlashingPlayerClock(new FlashingPlayerClock(props.PlayerClockSessionActive, config.FlashPlayerClockEnabled, config.NumberOfFlashesPerSequence,
				TimeSpan.FromMilliseconds(config.FlashSequenceLengthMilliseconds), TimeSpan.FromMinutes(config.MinutesBetweenSequences)));
		}

		private static void InitTotalAward()
		{
			if (GameLib.GameContextMode != GameMode.Play)
			{
				cycleAward = Money.Zero;
				totalAward = Money.Zero;
				wagerableAward = Money.Zero;
			}
			else
			{
				var data = GameLib.ReadCriticalData<long[]>(CriticalDataScope.Payvar, TotalAwardCriticalData);
				cycleAward = data == null ? Money.Zero : Money.FromRationalNumber(data[0], data[1]);
				totalAward = data == null ? Money.Zero : Money.FromRationalNumber(data[2], data[3]);
				wagerableAward = data == null ? Money.Zero : Money.FromRationalNumber(data[4], data[5]);
			}

			GameLogic.SetAwardValues(cycleAward, totalAward);
			GameLogic.SetWagerableMeter(bank + wagerableAward);
		}

		private static void ClearWagerableAward()
		{
			wagerableAward = Money.Zero;
			SaveAwardMeters();
			GameLogic.SetWagerableMeter(bank);
		}

		private static void ClearTotalAward()
		{
			// This should only be done when returning to idle, so no need to remove the critical data since it's GameCycle scope.

			cycleAward = Money.Zero;
			totalAward = Money.Zero;
			wagerableAward = Money.Zero;
			SaveAwardMeters();
			GameLogic.SetAwardValues(cycleAward, totalAward);
			GameLogic.SetWagerableMeter(bank);
		}

		private static void IncrementTotalAward(Money amount)
		{
			cycleAward = amount;
			totalAward += amount;
			wagerableAward = totalAward;
			SaveAwardMeters();
			GameLogic.SetAwardValues(cycleAward, totalAward);
			GameLogic.SetWagerableMeter(bank + wagerableAward);
		}

		private static void SaveAwardMeters()
		{
			GameLib.WriteCriticalData(CriticalDataScope.Payvar, TotalAwardCriticalData, new[] { cycleAward.Value.Numerator, cycleAward.Value.Denominator, totalAward.Value.Numerator, totalAward.Value.Denominator, wagerableAward.Value.Numerator, wagerableAward.Value.Denominator });
		}

		#region Ascent Event Handlers

		private static void OnGameLibDisplayControlEvent(object sender, DisplayControlEventArgs e)
		{
			Log.Instance.Info(e);
			switch (e.DisplayControlState)
			{
				case DisplayControlState.DisplayAsHidden:
					DisplayAsHidden();
					break;
				case DisplayControlState.DisplayAsNormal:
					DisplayAsNormal();
					break;
				case DisplayControlState.DisplayAsSuspended:
					DisplayAsSuspended();
					break;
			}
		}

		private static void DisplayAsNormal()
		{
			if (isGameEngineActive)
			{
				Pause(false);
				GameLogic.SetDisplayState(DisplayState.Normal);
			}
			else
			{
				Log.Instance.Fatal("Game loop not active");
			}
		}

		private static void DisplayAsSuspended()
		{
			if (isGameEngineActive)
			{
				Pause(true);
				GameLogic.SetDisplayState(DisplayState.Suspended);
			}
			else
			{
				Log.Instance.Fatal("Game loop not active");
			}
		}

		private static void DisplayAsHidden()
		{
			if (isGameEngineActive)
			{
				Pause(true);
				GameLogic.SetDisplayState(DisplayState.Hidden);
			}
			else
			{
				Log.Instance.Fatal("Game loop not active");
			}
		}

		private static void OnMoneyEvent(object sender, MoneyEventArgs e)
		{
			Log.Instance.InfoFormat("Money Event {0}", e.Type);
			UpdateMeters(e.Type.ConvertToSdk(), e.Meters);
			GameLogic.SetWagerableMeter(bank + wagerableAward);

			var value = Money.FromMinorCurrency(e.Value * e.Denomination);
			switch (e.Type)
			{
				case MoneyEventType.MoneyIn:
					GameLogic.MoneyIn(value, e.MoneyInSource.ConvertToSdk());
					break;

				case MoneyEventType.MoneyOut:
					GameLogic.MoneyOut(value, e.MoneyOutSource.ConvertToSdk());
					break;
			}
		}

		private static void UpdateMeters(MoneyEvent moneyEvent, PlayerMeters meters)
		{
			Log.Instance.InfoFormat("Meters Update (ME: {0} C:{1} P:{2} W:{3})", moneyEvent, meters.Bank, meters.Paid, meters.Wagerable);

			bank = Money.FromMinorCurrency(meters.Bank);
			GameLogic.SetBankMeters(moneyEvent, bank, Money.FromMinorCurrency(meters.Paid));
		}

		private static void OnBankStatusChangedEvent(object sender, BankStatusChangedEventArgs e)
		{
			GameLogic.SetBankStatus(e.Status.IsPlayerWagerOfferable, e.Status.IsCashOutOfferable);
		}

		#endregion

		#region Ugp Event Handlers

		private static void OnMachineConfigurationChanged(object sender, MachineConfigurationChangedEventArgs e)
		{
			RefreshMachineConfiguration(e.MachineConfigurationParameters);
		}

		private static void RefreshMachineConfiguration(MachineConfigurationParameters machineConfigParameters)
		{
			GameLogic.SetClockConfig(machineConfigParameters.IsClockVisible, machineConfigParameters.ClockFormat);
			GameLogic.SetPlayConfig(
				TimeSpan.FromMilliseconds(AscentFoundation.UgpInterfaces.IsUgpFoundation ? machineConfigParameters.GameCycleTime : GameLib.MinimumBaseGameTime),
				TimeSpan.FromMilliseconds(AscentFoundation.UgpInterfaces.IsUgpFoundation ? machineConfigParameters.GameCycleTime : GameLib.MinimumFreeSpinTime),
				machineConfigParameters.IsContinuousPlayAllowed,
				machineConfigParameters.IsFeatureAutoStartEnabled,
				Credit.FromLong(Math.Min(GameLib.GameMaxBet, machineConfigParameters.CurrentMaximumBet / GameLib.GameDenomination)));
			GameLogic.SetQcomConfig(machineConfigParameters.QcomJurisdiction);
			GameLogic.SetHardwareId(machineConfigParameters.CabinetId, machineConfigParameters.BrainboxId, machineConfigParameters.Gpu);

			if (AscentFoundation.UgpInterfaces.IsUgpFoundation)
				GameLogic.SetSlamSpinConfig(machineConfigParameters.IsSlamSpinAllowed, false, false, false, true);
			else
				GameLogic.SetSlamSpinConfig(GameLib.MinimumBaseGameTime == 0, GameLib.MinimumFreeSpinTime == 0, true, true, false);
		}

		private static void OnReserveParametersChanged(object sender, ReserveParametersChangedEventArgs e)
		{
			RefreshReserveParameters(e.ReserveParameters);
		}

		private static void RefreshReserveParameters(ReserveParameters reserveParameters)
		{
			GameLogic.SetReserveConfig(reserveParameters.IsReserveAllowedWithCredits,
				reserveParameters.IsReserveAllowedWithoutCredits,
				TimeSpan.FromMilliseconds(reserveParameters.ReserveTimeWithCreditsMilliseconds),
				TimeSpan.FromMilliseconds(reserveParameters.ReserveTimeWithoutCreditsMilliseconds));
		}

		private static void OnMessageAdded(object sender, MessageAddedEventArgs e)
		{
			RefreshMessageStrip(e.Messages);
		}

		private static void OnMessageRemoved(object sender, MessageRemovedEventArgs e)
		{
			RefreshMessageStrip(e.Messages);
		}

		private static void RefreshMessageStrip(IReadOnlyList<string> messages)
		{
			GameLogic.SetMessages(messages);
		}

		private static void OnProgressiveBroadcastEvent(object sender, ProgressiveBroadcastEventArgs e)
		{
			RefreshProgressives(e.BroadcastDataList);
		}

		private static void OnGameLibThemeSelectionMenuOfferableStatusChangedEvent(object sender, ThemeSelectionMenuOfferableStatusChangedEventArgs e)
		{
			GameLogic.SetIsChooserAvailable(e.Offerable);
		}

		private static void OnAutoplayOnRequestEvent(object sender, AutoPlayOnRequestEventArgs e)
		{
			e.RequestAccepted = GameLogic.SetAutoplayOn();
		}

		private static void OnAutoplayOffEvent(object sender, AutoPlayOffEventArgs e)
		{
			GameLogic.SetAutoplayOff();
		}

		private static void OnLanguageChangedEvent(object sender, LanguageChangedEventArgs e)
		{
			RefreshCurrentLanguage(e.Language);
		}

		private static void InitIsChooserAvailable()
		{
			var isChooserAvailable = GameLib.IsThemeSelectionMenuOfferable;
			if (GameParameters.Type != IgtGameParameters.GameType.Standard)
			{
				var loadSettings = StandaloneAustralianFoundationSettings.Load();
				isChooserAvailable = loadSettings.MachineSettings.IsMoreGamesEnabled;
			}

			GameLogic.SetIsChooserAvailable(isChooserAvailable);
		}

		private static void RefreshProgressives(IDictionary<int, ProgressiveBroadcastData> broadcastData)
		{
			var values = new List<(string, Money)>(broadcastData.Count);

			foreach (var kvp in broadcastData)
			{
				if (progressiveLevelToId.TryGetValue(kvp.Key, out var levelId))
					values.Add((levelId, Money.FromMinorCurrency(kvp.Value.Amount)));
			}

			GameLogic.SetProgressiveValues(values);
		}

		private static void CheckOutcomeForProgressives(IOutcomeList outcomeList)
		{
			var newHits = new List<ProgressiveHit>();
			foreach (var featureEntry in outcomeList.GetFeatureEntries())
			{
				foreach (var featureAward in featureEntry.GetAwards<IFeatureAward>())
				{
					foreach (var progAward in featureAward.GetFeatureProgressiveAwards())
					{
						if (progAward.HitState == ProgressiveAwardHitState.NotHit)
							continue;

						newHits.Add(new ProgressiveHit(progAward.Tag, Money.FromMinorCurrency(progAward.AmountValue), progAward.Source, progAward.SourceDetail));
					}
				}
			}

			foreach (var gameCycleEntry in outcomeList.GetGameCycleEntries())
			{
				foreach (var progAward in gameCycleEntry.GetProgressiveAwards())
				{
					if (progAward.HitState == ProgressiveAwardHitState.NotHit)
						continue;

					newHits.Add(new ProgressiveHit(progAward.Tag, Money.FromMinorCurrency(progAward.AmountValue), progAward.Source, progAward.SourceDetail));
				}
			}

			GameLogic.SetProgressiveHits(newHits);
		}

		private static void OnProgressiveAwardVerified(object sender, ProgressiveAwardVerifiedEventArgs e)
		{
			var amount = Money.FromMinorCurrency(e.VerifiedAmount);
			var payType = (ProgressiveAwardPayType)e.PayType;

			if (IsInitialising)
			{
				var progAwardEvent = new ProgressiveAwardEvent(ProgressiveAwardEventType.Verified, e.ProgressiveAwardIndex, e.ProgressiveLevelId, payType, amount);
				GameLib.WriteCriticalData(CriticalDataScope.Payvar, nameof(ProgressiveAwardEvent), progAwardEvent);
				return;
			}

			GameLogic.SetProgressiveVerified(e.ProgressiveAwardIndex, e.ProgressiveLevelId, payType, amount);
		}

		private static void OnProgressiveAwardPaid(object sender, ProgressiveAwardPaidEventArgs e)
		{
			var amount = Money.FromMinorCurrency(e.PaidAmount);

			if (IsInitialising)
			{
				var progAwardEvent = new ProgressiveAwardEvent(ProgressiveAwardEventType.Paid, e.ProgressiveAwardIndex, e.ProgressiveLevelId, null, amount);
				GameLib.WriteCriticalData(CriticalDataScope.Payvar, nameof(ProgressiveAwardEvent), progAwardEvent);
				return;
			}

			GameLogic.SetProgressivePaid(e.ProgressiveAwardIndex, e.ProgressiveLevelId, amount);
		}

		private static void RefreshProgressiveAward()
		{
			var e = new ProgressiveAwardEvent();
			if (GameLib.TryReadCriticalData(ref e, CriticalDataScope.Payvar, nameof(ProgressiveAwardEvent)))
			{
				GameLib.RemoveCriticalData(CriticalDataScope.Payvar, nameof(ProgressiveAwardEvent));
				switch (e.EventType)
				{
					case ProgressiveAwardEventType.Verified:
						GameLogic.SetProgressiveVerified(e.ProgressiveAwardIndex, e.ProgressiveLevelId, e.PayType!.Value, e.Amount);
						break;

					case ProgressiveAwardEventType.Paid:
						GameLogic.SetProgressivePaid(e.ProgressiveAwardIndex, e.ProgressiveLevelId, e.Amount);
						break;
				}
			}
		}

		private static void OnExternalJackpotChanged(object sender, ExternalJackpotChangedEventArgs e)
		{
			RefreshExternalJackpot(e.ExternalJackpots);
		}

		private static void RefreshExternalJackpot(ExternalJackpots externalJackpots)
		{
			GameLogic.SetExternalJackpots(externalJackpots.IsVisible, externalJackpots.IconId, externalJackpots.Jackpots.Select(j => new Core.ExternalJackpot(j.Name, j.Value, j.IsVisible, j.IconId)).ToArray());
		}

		private static void OnPidConfigurationChanged(object sender, PidConfigurationChangedEventArgs e)
		{
			RefreshPidConfiguration();
			RefreshPidSession();
		}

		private static void OnServiceRequestedChange(object sender, PidServiceRequestedChangedEventArgs e)
		{
			GameLogic.SetIsServiceRequested(e.IsServiceRequested);
		}

		private static void OnUgpInterfacesDenominationPlayableStatusChange(object sender, DenominationPlayableStatusChangeEventArgs e)
		{
			var dps = e.DenominationPlayableStatusTypes.Select(t => new Core.DenominationPlayableStatus(t.Denomination, (Core.GameButtonStatus)(int)t.ButtonStatus)).ToList();
			GameLogic.SetDenomPlayableStatus(dps);
		}

		private static void OnUgpInterfacesGameButtonBehaviorTypeChange(object sender, GameButtonBehaviorTypeChangeEventArgs e)
		{
			var gbb = e.GameButtonBehaviorTypes.Select(t => new GameButtonBehaviour((GameButton)(int)t.ButtonType, (Core.GameButtonStatus)(int)t.ButtonStatus)).ToList();
			GameLogic.SetGameButtonBehaviours(gbb);
		}

		private static void OnUgpInterfacesDenominationMenuTimeoutSet(object sender, DenominationMenuControlSetTimeoutEventArgs e)
		{
			GameLogic.SetDenominationMenuTimeoutConfiguration(e.TimeoutActive, TimeSpan.FromMilliseconds(e.DenominationTimeout));
		}

		private static void OnPlayerSessionPlayerSessionStatusChangedEvent(object sender, PlayerSessionStatusChangedEventArgs e)
		{
			RefreshPlayerSession();
		}

		private static void OnPlayerSessionParamsCurrentResetParametersChangedEvent(object sender, CurrentResetParametersChangedEventArgs e)
		{
			RefreshPlayerSessionParameters();
		}

		private static void OnFlashingPlayerClockInterfacesFlashingClockChanged(object sender, bool e)
		{
			RefreshFlashingPlayerClock();
		}

		private static void RefreshPidConfiguration()
		{
			var pc = AscentFoundation.UgpInterfaces.GetPidConfiguration();
			var pc2 = new Core.PidConfiguration(pc.IsMainEntryEnabled, pc.IsRequestServiceEnabled, pc.IsRequestServiceActivated, (Core.GameInformationDisplayStyle)pc.GameInformationDisplayStyle, (Core.SessionTrackingOption)pc.SessionTrackingOption, pc.IsGameRulesEnabled,
				pc.InformationMenuTimeout, pc.SessionStartMessageTimeout, pc.ViewSessionScreenTimeout, pc.ViewGameInformationTimeout, pc.ViewGameRulesTimeout, pc.ViewPayTableTimeout, pc.SessionTimeoutInterval,
				pc.SessionTimeoutStartOnZeroCredits, pc.TotalNumberLinkEnrolments, pc.TotalLinkPercentageContributions, pc.ShowLinkJackpotCount, pc.LinkRtpForGameRtp);

			GameLogic.SetPidConfiguration(pc2);
			GameLogic.SetIsServiceRequested(pc2.IsRequestServiceActivated);
		}

		private static void RefreshPidSession()
		{
			var sd = AscentFoundation.UgpInterfaces.GetSessionData();
			var sd2 = new PidSession(sd.IsSessionTrackingActive, Money.FromMinorCurrency(sd.CreditMeterAtStart), Money.FromMinorCurrency(sd.AvailableCredits), Money.FromMinorCurrency(sd.CashIn), Money.FromMinorCurrency(sd.CashOut), Money.FromMinorCurrency(sd.CreditsPlayed),
				Money.FromMinorCurrency(sd.CreditsWon), Money.FromMinorCurrency(sd.SessionWinOrLoss), sd.IsWinningSession, sd.IsCrown, sd.SessionStarted, sd.SessionDuration);
			GameLogic.SetPidSession(sd2);
		}

		private static void RefreshGameFunctionStatus()
		{
			var dps = AscentFoundation.UgpInterfaces.GetDenominationPlayableStatus().Select(t => new Core.DenominationPlayableStatus(t.Denomination, (Core.GameButtonStatus)(int)t.ButtonStatus)).ToList();
			GameLogic.SetDenomPlayableStatus(dps);

			var gbb = AscentFoundation.UgpInterfaces.GetGameButtonStatus().Select(t => new GameButtonBehaviour((GameButton)(int)t.ButtonType, (Core.GameButtonStatus)(int)t.ButtonStatus)).ToList();
			GameLogic.SetGameButtonBehaviours(gbb);

			var d = AscentFoundation.UgpInterfaces.GetDenominationMenuTimeoutConfiguration();
			GameLogic.SetDenominationMenuTimeoutConfiguration(d.TimeoutActive, TimeSpan.FromMilliseconds(d.DenominationTimeout));
		}

		private static void RefreshProgressiveLevels()
		{
			var p = AscentFoundation.UgpInterfaces.GetAllProgressives();
			GameLogic.SetProgressiveLevels(p.Select(pl => new ProgressiveLevel(pl.Id, pl.Name, pl.IncrementPercentage, pl.HiddenIncrementPercentage, Money.FromMinorCurrency(pl.Startup), Money.FromMinorCurrency(pl.Ceiling),
				pl.IsStandalone, pl.IsTriggered, pl.Rtp, pl.TriggerProbability)).ToArray());
		}

		private static void RefreshCurrentLanguage(string newLanguage)
		{
			string flag = null;
			GameLib.LocalizationInformation?.GetLanguageIconInformation()?.TryGetValue(newLanguage, out flag);
			GameLogic.SetLanguage(newLanguage, flag);
		}

		private static void RefreshPlayerSession()
		{
			var ps = AscentFoundation.PlayerSessionInterfaces.GetPlayerSessionStatus();
			var p = new PlayerSession(ps.SessionActive, ps.SessionStartTime);
			GameLogic.SetPlayerSession(p, AscentFoundation.PlayerSessionInterfaces.GetSessionTimerDisplayEnabled());
		}

		private static void RefreshPlayerSessionParameters()
		{
			var pp = AscentFoundation.PlayerSessionInterfaces.PendingParametersToReset();
			var p = new PlayerSessionParameters(AscentFoundation.PlayerSessionInterfaces.IsPlayerSessionParameterResetEnabled(), pp.Select(pr => (Core.PlayerSessionParameterType)(int)pr).ToArray());
			GameLogic.SetPlayerSessionParameters(p);
		}

		#endregion
	}
}