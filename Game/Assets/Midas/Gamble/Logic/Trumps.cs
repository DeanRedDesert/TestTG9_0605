using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.General;
using Midas.Core.Serialization;
using Midas.Gamble.LogicToPresentation;
using Midas.Logic;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;

namespace Midas.Gamble.Logic
{
	public sealed partial class Trumps : IGamble
	{
		private const string TrumpsStateCriticalData = "TrumpsState";

		private static readonly RationalNumber doubleUpColorMultiplier = new RationalNumber(2, 1);
		private static readonly RationalNumber doubleUpSuitMultiplier = new RationalNumber(4, 1);

		private readonly AncillaryConfig ancillaryConfig;
		private TrumpsService trumpsService;
		private IFoundationShim foundation;
		private TrumpsSelection? trumpsSelection;

		private TrumpsState trumpsState;
		private TrumpsSuit? dialUpSuit;

		public Trumps(AncillaryConfig ancillaryConfig)
		{
			this.ancillaryConfig = ancillaryConfig;
		}

		public void Init(IFoundationShim foundationShim, object historyData)
		{
			foundation = foundationShim;
			if (foundation.GameMode == FoundationGameMode.History)
			{
				// trumpsState will be null if no game was played.
				trumpsState = (TrumpsState)historyData;
			}
			else if (!foundation.TryReadNvram(NvramScope.Variation, TrumpsStateCriticalData, out trumpsState))
			{
				trumpsState = new TrumpsState
				{
					CycleData = new List<TrumpsCycleData>(),
					History = new List<TrumpsSuit>()
				};

				SaveTrumpsState();
			}

			trumpsService = TrumpsService.Instance;

			if (trumpsState != null)
			{
				trumpsService.HistoryService.SetValue(trumpsState.History.ToArray());
				trumpsService.ResultsService.SetValue(trumpsState.CycleData.ToArray());
				trumpsService.CurrentResultIndexService.SetValue(trumpsState.CycleData.Count - 1);
			}

			Communication.LogicDispatcher.AddHandler<TrumpsSelectionMessage>(OnTrumpsSelectionMessage);

			if (foundation.ShowMode == FoundationShowMode.Development)
				Communication.LogicDispatcher.AddHandler<TrumpsDialUpMessage>(OnTrumpsDialUpMessage);
		}

		private void OnTrumpsSelectionMessage(TrumpsSelectionMessage message)
		{
			if (trumpsSelection != null)
				Log.Instance.Warn($"Multiple {nameof(TrumpsSelectionMessage)} received before starting trumps.");

			trumpsSelection = message.Selection;
		}

		public void DeInit()
		{
			Communication.LogicDispatcher.RemoveHandler<TrumpsSelectionMessage>(OnTrumpsSelectionMessage);

			if (foundation.ShowMode == FoundationShowMode.Development)
				Communication.LogicDispatcher.RemoveHandler<TrumpsDialUpMessage>(OnTrumpsDialUpMessage);
		}

		private void OnTrumpsDialUpMessage(TrumpsDialUpMessage msg)
		{
			if (foundation.ShowMode == FoundationShowMode.Development)
				dialUpSuit = msg.Suit;
		}

		public IOutcome StartGamble(bool isFirstGambleCycle)
		{
			if (!trumpsSelection.HasValue)
			{
				var msg = $"Unable to start trumps because a {nameof(TrumpsSelectionMessage)} has not been sent by the presentation.";
				Log.Instance.Fatal(msg);
				throw new InvalidOperationException(msg);
			}

			Log.Instance.InfoFormat("{0} gamble with selection = {1}", isFirstGambleCycle ? "Starting" : "Continuing", trumpsSelection);

			if (trumpsSelection == TrumpsSelection.Decline)
				return GambleOutcome.CancelGambleOutcome;

			if (isFirstGambleCycle)
			{
				// Move the cycle data from the previous gamble set into the history and clear the state.

				trumpsState.History.AddRange(trumpsState.CycleData.Select(d => d.Suit));

				if (trumpsState.History.Count > 10)
					trumpsState.History.RemoveRange(0, trumpsState.History.Count - 10);

				trumpsState.CycleData.Clear();

				trumpsService.HistoryService.SetValue(trumpsState.History.ToArray());
			}

			var outcome = GenerateTrumpsResult();

			trumpsService.ResultsService.SetValue(trumpsState.CycleData.ToArray());
			trumpsService.CurrentResultIndexService.SetValue(trumpsState.CycleData.Count - 1);

			return outcome;
		}

		private IOutcome GenerateTrumpsResult()
		{
			if (!trumpsSelection.HasValue)
			{
				var msg = $"Unable to generate trumps result because a {nameof(TrumpsSelectionMessage)} has not been sent by the presentation.";
				Log.Instance.Fatal(msg);
				throw new InvalidOperationException(msg);
			}

			TrumpsSuit suit;

			if (foundation.ShowMode == FoundationShowMode.Development && dialUpSuit != null)
			{
				suit = dialUpSuit.Value;
				dialUpSuit = null;
			}
			else
			{
				suit = (TrumpsSuit)foundation.GetRandomNumbers(1, 0, 3)[0];
			}

			Log.Instance.InfoFormat("Trumps result {0}", suit);

			TrumpsResult result;
			var riskAmount = GameServices.MetersService.TotalAwardService.Value;
			Money winAmount;

			void EvalTrumps(bool isWin, RationalNumber multiplier)
			{
				if (isWin)
				{
					result = TrumpsResult.Win;
					winAmount = riskAmount * multiplier;
				}
				else
				{
					result = TrumpsResult.Loss;
					winAmount = Money.Zero;
				}
			}

			switch (trumpsSelection.Value)
			{
				case TrumpsSelection.Red:
					EvalTrumps(suit == TrumpsSuit.Heart || suit == TrumpsSuit.Diamond, doubleUpColorMultiplier);
					break;
				case TrumpsSelection.Black:
					EvalTrumps(suit == TrumpsSuit.Spade || suit == TrumpsSuit.Club, doubleUpColorMultiplier);
					break;
				case TrumpsSelection.Heart:
					EvalTrumps(suit == TrumpsSuit.Heart, doubleUpSuitMultiplier);
					break;
				case TrumpsSelection.Diamond:
					EvalTrumps(suit == TrumpsSuit.Diamond, doubleUpSuitMultiplier);
					break;
				case TrumpsSelection.Club:
					EvalTrumps(suit == TrumpsSuit.Club, doubleUpSuitMultiplier);
					break;
				case TrumpsSelection.Spade:
					EvalTrumps(suit == TrumpsSuit.Spade, doubleUpSuitMultiplier);
					break;
				default:
					var msg = $"Unknown trumps selection {trumpsSelection.Value}.";
					Log.Instance.Fatal(msg);
					throw new InvalidOperationException(msg);
			}

			var finalGameReason = CheckForFinalGame(winAmount, trumpsState.CycleData.Count + 1);
			trumpsState.CycleData.Add(new TrumpsCycleData(trumpsSelection.Value, suit, result, winAmount, finalGameReason));
			SaveTrumpsState();
			trumpsSelection = null;

			return result == TrumpsResult.Loss
				? GambleOutcome.CreateLossOutcome(riskAmount)
				: GambleOutcome.CreateWinOutcome(riskAmount, winAmount, finalGameReason != GambleCompleteReason.None);
		}

		private bool IsWinLimitReached(Money winAmount, Money totalProgressiveAwardedValue)
		{
			if (ancillaryConfig.MoneyLimit == Money.Zero)
				return false;

			if (totalProgressiveAwardedValue >= ancillaryConfig.MoneyLimit)
				return true;

			var remainingAncillaryMonetaryLimit = ancillaryConfig.MoneyLimit - totalProgressiveAwardedValue;
			return winAmount * doubleUpSuitMultiplier >= remainingAncillaryMonetaryLimit;
		}

		private GambleCompleteReason CheckForFinalGame(Money winAmount, int cyclesComplete)
		{
			var reason = GambleCompleteReason.None;

			if (winAmount == Money.Zero)
				reason |= GambleCompleteReason.Loss;
			else if (IsWinLimitReached(winAmount, GameServices.ProgressiveService.TotalProgressiveAwardInGameService.Value))
				reason |= GambleCompleteReason.MoneyLimit;
			if (cyclesComplete >= ancillaryConfig.CycleLimit)
				reason |= GambleCompleteReason.CycleLimit;

			Log.Instance.InfoFormat("CheckForFinalGame returning {0}", reason);

			return reason;
		}

		public object GetHistoryState()
		{
			if (trumpsState.CycleData[trumpsState.CycleData.Count - 1].GambleCompleteReason == GambleCompleteReason.None)
				return null;

			// Clone so it can safely change after the fact.

			return new TrumpsState
			{
				History = new List<TrumpsSuit>(trumpsState.History),
				CycleData = new List<TrumpsCycleData>(trumpsState.CycleData)
			};
		}

		public void ShowHistory(object historyData)
		{
			trumpsService.CurrentResultIndexService.SetValue((int)historyData);
		}

		public bool IsPlayPossible(Money riskAmount)
		{
			return !IsWinLimitReached(riskAmount, GameServices.ProgressiveService.TotalProgressiveAwardInGameService.Value);
		}

		public object GetGameCycleHistoryData()
		{
			return trumpsState.CycleData.Count - 1;
		}

		private void SaveTrumpsState()
		{
			foundation.WriteNvram(NvramScope.Variation, TrumpsStateCriticalData, trumpsState);
		}
	}
}