using System;
using System.Collections.Generic;
using Midas.Core.Configuration;
using Midas.Core.General;

namespace Midas.Core
{
	public interface IGameLogic
	{
		IStakeCombination CurrentStakeCombination { get; }
		IReadOnlyList<(string LevelId, uint GameLevel)> ProgressiveLevels { get; }

		void Init(IFoundationShim foundationShim);
		void Start();
		void DeInit(ShutdownReason reason);

		void Park(bool pause);
		void Pause(bool pause);
		Money WaitForPlay(out IStakeCombination usedStakeCombionation);
		IOutcome StartGameCycle();
		void ShowResult();
		void EndGame();
		void Finalise();
		bool OfferGamble();
		IOutcome StartGamble(bool isFirstGambleCycle);
		void ShowGambleResult();
		void ShowHistory();

		void SetDisplayState(DisplayState displayState);
		void SetBankMeters(MoneyEvent moneyEvent, Money bank, Money paid);
		void SetWagerableMeter(Money wagerable);
		void MoneyIn(Money amount, MoneySource source);
		void MoneyOut(Money amount, MoneyTarget target);
		void SetAwardValues(Money cycleAward, Money totalAward);
		void SetBankStatus(bool isPlayerWagerOfferable, bool isCashOutOfferable);
		void SetIsChooserAvailable(bool isAvailable);
		void SetReserveConfig(bool isReserveAllowedWithCredits, bool isReserveAllowedWithoutCredits, TimeSpan timeoutWithCredits, TimeSpan timeoutWithoutCredits);
		void SetMessages(IReadOnlyList<string> messages);
		void SetSlamSpinConfig(bool allowed, bool allowedInFeature, bool isImmediate, bool allowDppButton, bool recordUsage);
		void SetClockConfig(bool isClockVisible, string clockFormat);
		void SetPlayConfig(TimeSpan baseGameTime, TimeSpan freeGameTime, bool isContinuousPlayAllowed, bool isFeatureAutoStartEnabled, Credit maxBet);
		void SetQcomConfig(int qcomJurisdiction);
		void SetHardwareId(string cabinetId, string brainboxId, string gpu);
		void SetProgressiveValues(IReadOnlyList<(string LevelId, Money Value)> broadcastData);
		void SetProgressiveHits(IReadOnlyList<ProgressiveHit> hits);
		void SetProgressiveVerified(int awardIndex, string levelId, ProgressiveAwardPayType payType, Money verifiedAmount);
		void SetProgressivePaid(int awardIndex, string levelId, Money paidAmount);
		void SetProgressiveLevels(IReadOnlyList<ProgressiveLevel> levels);
		void SetExternalJackpots(bool isVisible, int iconId, IReadOnlyList<ExternalJackpot> jackpots);
		ProgressiveAwardWaitState GetProgressiveAwardWaitState(out int awardIndex);
		void SetPidSession(PidSession pidSession);
		void SetPidConfiguration(PidConfiguration pidConfiguration);
		void SetIsServiceRequested(bool requested);
		void SetGameButtonBehaviours(IReadOnlyList<GameButtonBehaviour> behaviours);
		void SetDenomPlayableStatus(IReadOnlyList<DenominationPlayableStatus> denomPlayableStatus);
		void SetDenominationMenuTimeoutConfiguration(bool isActive, TimeSpan timeout);
		void ChangeDenom(Money newDenom, bool foundationInitiated);
		bool SetAutoplayOn();
		void SetAutoplayOff();
		void SetLanguage(string language, string flag);
		void SetPlayerSession(PlayerSession playerSession, bool isSessionTimerDisplayEnabled);
		void SetPlayerSessionParameters(PlayerSessionParameters playerSessionParameters);
		void SetFlashingPlayerClock(FlashingPlayerClock flashingPlayerClock);
		void SetIsGambleOfferable(bool isGambleOfferable);
	}
}