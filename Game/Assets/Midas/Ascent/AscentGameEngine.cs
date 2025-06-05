using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IGT.Ascent.Communication.Platform.GameLib.Interfaces;
using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.Debug;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using static Midas.Ascent.AscentFoundation;
using CriticalDataScope = IGT.Ascent.Communication.Platform.Interfaces.CriticalDataScope;
using GameCycleState = IGT.Ascent.Communication.Platform.Interfaces.GameCycleState;
using GameMode = IGT.Ascent.Communication.Platform.Interfaces.GameMode;

namespace Midas.Ascent
{
	internal static partial class AscentGameEngine
	{
		#region Critical Data Names

		private const string EngineStateCriticalData = "EngineState";
		private const string FoundationResponseCriticalData = "FoundationResponse";
		private const string TotalAwardCriticalData = "TotalAward";
		private const string HistoryStepCountCriticalData = "HistoryStepCount";
		private const string OfferGambleCriticalData = "OfferGamble";

		#endregion

		#region Fields

		private static Thread logicThread;
		private static readonly ManualResetEvent stopThreadSignal = new ManualResetEvent(false);
		private static readonly ManualResetEvent startGameSignal = new ManualResetEvent(false);
		private static readonly ManualResetEvent stopGameSignal = new ManualResetEvent(false);
		private static readonly ManualResetEvent foundationResponseSignal = new ManualResetEvent(false);
		private static bool isGameEngineActive;
		private static ShutdownReason shutdownReason;
		private static bool foundationResponseEventAdded;
		private static GameMode? pendingGameMode;
		private static Money bank;
		private static Money cycleAward;
		private static Money totalAward;
		private static Money wagerableAward;
		private static uint historyStepCount;
		private static Action utilityEndGame;

		#endregion

		#region Properties

		public static ThemeContext ThemeContext { get; private set; }
		public static PaytableConfig PaytableConfiguration { get; private set; }
		public static bool IsInitialising { get; private set; }
		public static bool ShouldGameEngineExit { get; private set; }
		public static bool IsPaused { get; private set; }

		#endregion

		#region Public Methods

		public static void Start()
		{
			Log.Instance.Info("Start");

			if (logicThread != null)
				return;

			LogicThreadException = null;

			logicThread = IsRunningInEditor
				? new Thread(RunLogic) { Name = "Logic" }
				: new Thread(RunLogicWithCatchall) { Name = "Logic" };

			logicThread.Start();

			void RunLogicWithCatchall()
			{
				try
				{
					RunLogic();
				}
				catch (Exception e)
				{
					LogicThreadException = e;
				}
			}
		}

		public static void Stop(ShutdownReason reason)
		{
			Log.Instance.Info("Stop");

			if (logicThread != null)
			{
				shutdownReason = reason;
				ShouldGameEngineExit = true;
				stopThreadSignal.Set();

				if (!logicThread.Join(TimeSpan.FromSeconds(5)))
				{
					Log.Instance.Warn("Failed to properly shut down logic thread");
					logicThread.Abort();
				}

				logicThread = null;
			}
		}

		public static void ChangeGameMode(GameMode nextMode)
		{
			if (GameLibDemo != null)
			{
				pendingGameMode = nextMode;
				GameLibDemo.ExitMode();
			}
		}

		public static bool ChangeGameDenom(Money denom)
		{
			var changeDenomRequested = false;
			var currentDenom = Money.FromMinorCurrency(GameLib.GameDenomination);
			if (denom != currentDenom)
			{
				Log.Instance.Info($"Requesting changing denom to '{denom}'");

				if (GameLib.RequestDenominationChange(denom.AsMinorCurrency))
				{
					changeDenomRequested = true;
					ShouldGameEngineExit = true;
					if (isGameEngineActive)
						stopGameSignal.Set();

					Log.Instance.Info($"Requesting changing denom to '{denom}' accepted");
				}
				else
				{
					Log.Instance.Warn($"Requesting changing denom to '{denom}' denied");
				}
			}

			return changeDenomRequested;
		}

		public static void ChangeLanguage(string language)
		{
			// Don't do anything if we are shutting down

			if (ShouldGameEngineExit || GameLib.GameContextMode != GameMode.Play)
				return;

			if (string.IsNullOrEmpty(language))
				GameLib.SetDefaultLanguage();
			else if (GameLib.GameLanguage != language)
				GameLib.SetLanguage(language);
			else
				Log.Instance.Info($"Language {language} already set");
		}

		public static void ProcessEvents(WaitHandle waitHandle)
		{
			// Close transaction if one was open.

			var wasTransactionOpen = Transaction.IsGameTransactionOpen;
			if (wasTransactionOpen)
			{
				Transaction.CloseTransaction();
			}

			WaitHandle[] waitHandles = { stopThreadSignal, stopGameSignal, waitHandle };

			for (;;)
			{
				void ReopenTransaction()
				{
					if (wasTransactionOpen)
					{
						Transaction.OpenTransaction();
					}
				}

				var signaledHandle = waitHandles.WaitAny(0) ?? AscentFoundation.ProcessEvents(waitHandles);

				if (signaledHandle == stopThreadSignal || signaledHandle == stopGameSignal)
				{
					ReopenTransaction();
					Log.Instance.Info("DeInit forced during ProcessEvents");
					throw new StopForcedException("DeInit forced during ProcessEvents");
				}

				if (signaledHandle == waitHandle)
				{
					ReopenTransaction();
					break;
				}
			}
		}

		public static void UpdatePidSessionData() => RefreshPidSession();

		#endregion

		#region Private Methods

		private static void RunLogic()
		{
			Log.Instance.Info("Logic thread started");
			var eventHandles = new WaitHandle[] { stopThreadSignal, startGameSignal, stopGameSignal };

			GameLogicTimings.Reset();
			RegisterAscentEvents();

			while (shutdownReason == ShutdownReason.None)
			{
				shutdownReason = ShutdownReason.None;
				ShouldGameEngineExit = false;
				var handle = AscentFoundation.ProcessEvents(eventHandles);

				if (handle == startGameSignal || startGameSignal.WaitOne(0))
				{
					startGameSignal.Reset();
					RunGameEngine();
					isGameEngineActive = startGameSignal.WaitOne(0);
					stopGameSignal.Reset();
				}

				if (handle == stopThreadSignal || stopThreadSignal.WaitOne(0))
				{
					stopThreadSignal.Reset();
					break;
				}

				if (pendingGameMode.HasValue)
				{
					GameLibDemo?.EnterMode(pendingGameMode.Value);
					pendingGameMode = null;
				}

				if (shutdownReason == ShutdownReason.DenomChange)
					shutdownReason = ShutdownReason.None;
			}

			UnregisterAscentEvents();
		}

		private static void RunGameEngine()
		{
			Log.Instance.Info("Game started");

			var transaction = default(Transaction);

			try
			{
				Money.Denomination = Money.FromMinorCurrency(ThemeContext.Denomination);
				PaytableConfiguration = FindPaytables(ThemeContext.PaytableFileName);

				GameLib.DisplayControlEvent += OnGameLibDisplayControlEvent;
				UgpInterfaces.ProgressiveAwardVerified += OnProgressiveAwardVerified;
				UgpInterfaces.ProgressiveAwardPaid += OnProgressiveAwardPaid;

				// On startup, all pending transactional foundation events are sent when this transaction is opened.

				IsInitialising = true;
				transaction = Transaction.CreateTransient();

				var foundationShim = new FoundationShim();
				utilityEndGame = foundationShim.UtilityEndGame;
				GameLogic.Init(foundationShim);
				InitGameServiceData();
				GameLogic.Start();
				RegisterGameServiceDataEvents();
				var engineState = ReadEngineState();
				ReadHistoryStepCount();
				transaction.Close();
				IsInitialising = false;

				while (!ShouldGameEngineExit)
				{
					transaction = Transaction.CreateTransient();
					engineState = RunEngineState(engineState);
					SetEngineState(engineState);
					transaction.Close();
				}

				GameLogic.DeInit(shutdownReason);

				PaytableConfiguration = null;
			}
			catch (StopForcedException)
			{
				GameLogic.DeInit(shutdownReason);
			}
			finally
			{
				UnregisterGameServiceDataEvents();
				transaction?.Close();
				GameLib.DisplayControlEvent -= OnGameLibDisplayControlEvent;
				UgpInterfaces.ProgressiveAwardVerified -= OnProgressiveAwardVerified;
				UgpInterfaces.ProgressiveAwardPaid -= OnProgressiveAwardPaid;
			}
		}

		private static AscentGameEngineState RunEngineState(AscentGameEngineState engineState)
		{
			Log.Instance.DebugFormat("RunEngineState({0})", engineState);
			switch (engineState)
			{
				case AscentGameEngineState.Idle:
				{
#if LOG_TIMINGS
					GameLogicTimings.LogTimings();
#endif
					var initialBet = GameLogic.WaitForPlay(out var sc);
					GameLogicTimings.BeginGame.Start();
					return TryEnrollGame(initialBet, sc.BetCategory) ? AscentGameEngineState.AwaitEnrollComplete : AscentGameEngineState.Idle;
				}

				case AscentGameEngineState.AwaitEnrollComplete:
				{
					var enrolComplete = AwaitEnrollComplete();
					GameLogicTimings.EnrollGame.Stop();
					GameLogicTimings.BeginGame.Stop();

					if (enrolComplete)
					{
						ClearTotalAward();
						return AscentGameEngineState.StartGameCycle;
					}

					return AscentGameEngineState.Idle;
				}

				case AscentGameEngineState.StartGameCycle:
				{
					GameLogicTimings.EvaluateCycle.Start();

					IOutcome results;

					using (new TimeSpanCollectorScope(GameLogicTimings.GameLogicStart))
						results = GameLogic.StartGameCycle();

					GameLogicTimings.AdjustOutcome.Start();
					IncrementTotalAward(GenerateOutcome(results));
					return AscentGameEngineState.AwaitOutcomeResponse;
				}

				case AscentGameEngineState.AwaitOutcomeResponse:
				{
					AwaitOutcomeResponse();
					GameLogicTimings.AdjustOutcome.Stop();
					GameLogicTimings.EvaluateCycle.Stop();
					return AscentGameEngineState.ShowResult;
				}

				case AscentGameEngineState.ShowResult:
				{
					var isFinished = GetIsFinishFromOutcomeResponse();
					if (isFinished)
						SetOfferGamble();

					GameLogic.ShowResult();
					IncHistoryStepCount();
					RemoveOutComeResponse();
					return isFinished ? AscentGameEngineState.LogicGameComplete : AscentGameEngineState.StartGameCycle;
				}

				case AscentGameEngineState.LogicGameComplete:
				{
					RefreshOfferGamble();
					GameLogic.EndGame();
					return AscentGameEngineState.OfferGamble;
				}

				case AscentGameEngineState.OfferGamble:
				{
					RefreshOfferGamble();
					if (GameLogic.OfferGamble() && GameLib.StartAncillaryPlaying())
						return AscentGameEngineState.StartGamble;

					GameLogicTimings.EndGame.Start();
					GameLogicTimings.FinalizeOutcome.Start();
					FinalizeOutcome();
					return AscentGameEngineState.AwaitFinalize;
				}

				case AscentGameEngineState.StartGamble:
				{
					var results = GameLogic.StartGamble(true);
					IncrementTotalAward(GenerateGambleOutcome(results, true, out var isAborted));
					return isAborted ? AscentGameEngineState.AwaitAbortGambleOutcomeResponse : AscentGameEngineState.AwaitGambleOutcomeResponse;
				}

				case AscentGameEngineState.ContinueGamble:
				{
					var results = GameLogic.StartGamble(false);
					IncrementTotalAward(GenerateGambleOutcome(results, false, out var isAborted));
					return isAborted ? AscentGameEngineState.AwaitAbortGambleOutcomeResponse : AscentGameEngineState.AwaitGambleOutcomeResponse;
				}

				case AscentGameEngineState.AwaitGambleOutcomeResponse:
				{
					AwaitOutcomeResponse();
					return AscentGameEngineState.ShowGambleResult;
				}

				case AscentGameEngineState.AwaitAbortGambleOutcomeResponse:
				{
					AwaitOutcomeResponse();

					GameLogicTimings.EndGame.Start();
					GameLogicTimings.FinalizeOutcome.Start();
					FinalizeOutcome();
					return AscentGameEngineState.AwaitFinalize;
				}

				case AscentGameEngineState.ShowGambleResult:
				{
					GameLogic.ShowGambleResult();
					IncHistoryStepCount();
					var isFinished = GetIsFinishFromOutcomeResponse();
					RemoveOutComeResponse();
					if (!isFinished)
						return AscentGameEngineState.ContinueGamble;

					GameLogicTimings.EndGame.Start();
					GameLogicTimings.FinalizeOutcome.Start();
					FinalizeOutcome();
					return AscentGameEngineState.AwaitFinalize;
				}

				case AscentGameEngineState.AwaitFinalize:
				{
					ClearOfferGamble();
					AwaitFinalize();
					GameLogicTimings.FinalizeOutcome.Stop();

					using (new TimeSpanCollectorScope(GameLogicTimings.GameLogicEndGame))
					{
						GameLogic.Finalise();
					}

					return AscentGameEngineState.Finalize;
				}

				case AscentGameEngineState.Finalize:
				{
					using (new TimeSpanCollectorScope(GameLogicTimings.EndGameCycle))
					{
						GameLib.EndGameCycle(historyStepCount);
					}

					ClearWagerableAward();
					ClearHistoryStepCount();
					GameLogicTimings.EndGame.Stop();
					return AscentGameEngineState.Idle;
				}

				case AscentGameEngineState.History:
				{
					GameLogic.ShowHistory();
					return AscentGameEngineState.History;
				}

				case AscentGameEngineState.Utility:
					RunUtilityEngine();
					return AscentGameEngineState.Utility;
			}

			throw new Exception($"Unhandled state {engineState}");
		}

		private static void RunUtilityEngine()
		{
			while (!ShouldGameEngineExit)
			{
				GameLogic.WaitForPlay(out var _);
				GameLogic.SetAwardValues(Money.Zero, Money.Zero);

				IOutcome results = null;

				var tAward = Money.Zero;
				while (results?.IsFinalOutcome != true)
				{
					results = GameLogic.StartGameCycle();
					var cAward = GenerateUtilityOutcome(results);
					tAward += cAward;
					GameLogic.SetAwardValues(cAward, tAward);
					GameLogic.ShowResult();
				}

				GameLogic.OfferGamble();
				GameLogic.EndGame();
				GameLogic.Finalise();
				utilityEndGame();
			}
		}

		private static PaytableConfig FindPaytables(string paytableFileName)
		{
			return new PaytableConfig(new[] { paytableFileName }, new[] { 0 });
		}

		#endregion

		#region Ascent Event Handlers

		private static void RegisterAscentEvents()
		{
			GameLib.NewThemeContextEvent += OnNewThemeContext;
			GameLib.ActivateThemeContextEvent += OnActivateThemeContext;
			GameLib.InactivateThemeContextEvent += OnInactivateThemeContext;
			GameLib.ParkEvent += OnPark;
			GameLib.ShutDownEvent += OnShutdown;
		}

		private static void UnregisterAscentEvents()
		{
			GameLib.NewThemeContextEvent -= OnNewThemeContext;
			GameLib.ActivateThemeContextEvent -= OnActivateThemeContext;
			GameLib.InactivateThemeContextEvent -= OnInactivateThemeContext;
			GameLib.ParkEvent -= OnPark;
			GameLib.ShutDownEvent -= OnShutdown;
		}

		private static void OnNewThemeContext(object sender, NewThemeContextEventArgs e)
		{
			Log.Instance.InfoFormat("NewThemeContext {0}", e.ThemeContext.ToString());

			if (ThemeContext != ThemeContext.Invalid && ThemeContext.Denomination != e.ThemeContext.Denomination)
			{
				// Denom change without shutting down the theme.
			}

			ThemeContext = e.ThemeContext;
		}

		private static void OnActivateThemeContext(object sender, ActivateThemeContextEventArgs e)
		{
			Log.Instance.InfoFormat("ActivateThemeContext {0}", e.ThemeContext.ToString());

			GameLogic.Park(false);

			ThemeContext = e.ThemeContext;

			if (isGameEngineActive)
				stopGameSignal.Set();
			else
				isGameEngineActive = true;

			startGameSignal.Set();
		}

		private static void OnInactivateThemeContext(object sender, InactivateThemeContextEventArgs e)
		{
			Log.Instance.Info("OnInactivateThemeContext");

			ShouldGameEngineExit = true;
			ThemeContext = ThemeContext.Invalid;
			if (isGameEngineActive)
				stopGameSignal.Set();
		}

		private static void Pause(bool pause)
		{
			if (!ShouldGameEngineExit)
			{
				IsPaused = pause;

				Log.Instance.Info($"Sending GameLogicPauseMessage({pause}) to Game Loop to wake up any message loop");
				GameLogic.Pause(pause);
			}
		}

		private static void OnPark(object sender, ParkEventArgs e)
		{
			Log.Instance.Info("Parking Game");
			if (isGameEngineActive)
				ShouldGameEngineExit = true;

			GameLogic.Park(true);
		}

		private static void OnShutdown(object sender, ShutDownEventArgs e)
		{
			Log.Instance.Info("OnShutdown");

			shutdownReason = ShutdownReason.FoundationRequest;
			stopThreadSignal.Set();
			if (isGameEngineActive)
				ShouldGameEngineExit = true;
		}

		#endregion

		#region State Machine Actions

		private static AscentGameEngineState ReadEngineState()
		{
			switch (GameLib.GameContextMode)
			{
				case GameMode.History:
					return AscentGameEngineState.History;
				case GameMode.Utility:
					return AscentGameEngineState.Utility;
				default:
				{
					var state = AscentGameEngineState.Idle;
					return GameLib.TryReadCriticalData(ref state, CriticalDataScope.Payvar, EngineStateCriticalData) ? state : AscentGameEngineState.Idle;
				}
			}
		}

		private static void SetEngineState(AscentGameEngineState nextState)
		{
			if (GameLib.GameContextMode == GameMode.Play)
				GameLib.WriteCriticalData(CriticalDataScope.Payvar, EngineStateCriticalData, nextState);
		}

		private static void ReadHistoryStepCount()
		{
			historyStepCount = 0;

			if (GameLib.GameContextMode == GameMode.History)
				return;

			GameLib.TryReadCriticalData(ref historyStepCount, CriticalDataScope.Payvar, HistoryStepCountCriticalData);
		}

		private static void ClearHistoryStepCount()
		{
			if (GameLib.GameContextMode == GameMode.Play)
			{
				historyStepCount = 0;
				GameLib.WriteCriticalData(CriticalDataScope.Payvar, HistoryStepCountCriticalData, historyStepCount);
			}
		}

		private static void IncHistoryStepCount()
		{
			if (GameLib.GameContextMode == GameMode.Play)
			{
				historyStepCount++;
				GameLib.WriteCriticalData(CriticalDataScope.Payvar, HistoryStepCountCriticalData, historyStepCount);
			}
		}

		private static bool TryEnrollGame(Money bet, BetCategory betCategory)
		{
			using (new TimeSpanCollectorScope(GameLogicTimings.CommitGame))
			{
				if (!GameLib.CanCommitGameCycle() || !GameLib.CommitGameCycle())
					return false;
			}

			long betToCommit;
			long denominationToCommit;
			var betInCredits = Credit.FromMoney(bet);

			if (betInCredits.HasSubCredits)
			{
				betToCommit = bet.AsMinorCurrency;
				denominationToCommit = 1;
			}
			else
			{
				betToCommit = betInCredits.Credits;
				denominationToCommit = Money.Denomination.AsMinorCurrency;
			}

			using (new TimeSpanCollectorScope(GameLogicTimings.CommitBet))
			{
				if (!GameLib.CanCommitBet(betToCommit, denominationToCommit) || !GameLib.CommitBet(betToCommit, denominationToCommit))
				{
					// If we get here we failed to commit the bet, so bail out on the game.

					GameLib.UncommitGameCycle();
					return false;
				}
			}

			GameLogicTimings.EnrollGame.Start();
			GameLib.GameCycleWagerCategoryInfo = new[] { new WagerCategoryOutcome((ushort)betCategory, betToCommit, denominationToCommit) };

			foundationResponseSignal.Reset();
			GameLib.EnrollResponseEvent += OnEnrollResponse;
			foundationResponseEventAdded = true;
			GameLib.EnrollGameCycle(Array.Empty<byte>());
			return true;
		}

		private static void OnEnrollResponse(object sender, EnrollResponseEventArgs e)
		{
			GameLib.WriteCriticalData(CriticalDataScope.GameCycle, FoundationResponseCriticalData, e);
			foundationResponseSignal.Set();
		}

		private static bool AwaitEnrollComplete()
		{
			var gameCycleState = GameLib.QueryGameCycleState();
			try
			{
				if (gameCycleState == GameCycleState.EnrollPending)
				{
					if (!foundationResponseEventAdded)
					{
						foundationResponseSignal.Reset();
						GameLib.EnrollResponseEvent += OnEnrollResponse;
					}

					ProcessEvents(foundationResponseSignal);
				}
			}
			finally
			{
				GameLib.EnrollResponseEvent -= OnEnrollResponse;
				foundationResponseEventAdded = false;
			}

			// Check if we have an enroll response.
			// Should not happen in a standard game but there is a small window in standalone where this can be null.

			var enrollResponse = GameLib.ReadCriticalData<EnrollResponseEventArgs>(CriticalDataScope.GameCycle, FoundationResponseCriticalData);
			if (enrollResponse != null)
			{
				GameLib.RemoveCriticalData(CriticalDataScope.GameCycle, FoundationResponseCriticalData);

				if (enrollResponse.EnrollSuccess)
				{
					GameLib.PlaceStartingBet();

					if (GameLib.CanStartPlaying() && GameLib.StartPlaying())
					{
						UgpInterfaces.SendBetInformation(GameLogic.CurrentStakeCombination);
						return true;
					}
				}
			}

			GameLib.UncommitBet();
			GameLib.UnenrollGameCycle();
			return false;
		}

		private static Money GenerateUtilityOutcome(IOutcome results)
		{
			var gameCycleAward = Money.Zero;

			foreach (var prize in results.Prizes)
			{
				if (prize.RiskAmount.IsZero)
					gameCycleAward += prize.Amount;
			}

			return gameCycleAward;
		}

		private static Money GenerateOutcome(IOutcome results)
		{
			var gameCycleAward = Money.Zero;
			var outcomeList = new OutcomeList();
			var featureEntry = new OutcomeListFeatureEntry();

			// Validate results

			if (results.Prizes.All(p => p.RiskAmount.IsZero))
			{
				// This is a normal game cycle, so set the feature index.
				featureEntry.feature_index = (uint)results.FeatureIndex;
			}
			else if (results.Prizes.Count > 1)
			{
				Log.Instance.Error("Risk awards (credit playoff) should have exactly one prize");
				throw new InvalidOperationException("Risk awards (credit playoff) should have exactly one prize");
			}

			foreach (var prize in results.Prizes)
			{
				if (prize.RiskAmount.IsZero)
				{
					gameCycleAward += prize.Amount;

					var award = new FeatureAward
					{
						amount = prize.Amount.AsMinorCurrency,
						origin = OutcomeOrigin.Bin,
						tag = prize.PrizeName,
						is_displayable = true
					};

					if (prize.ProgressiveHit != null)
					{
						var progressiveAward = new FeatureProgressiveAward
						{
							game_levelSpecified = true,
							game_level = (uint)progressiveIdToLevel[prize.ProgressiveHit.LevelId],
							is_displayable = true,
							tag = prize.ProgressiveHit.LevelId,
							hit_state = ProgressiveAwardHit_state.PotentialHit,
							source = prize.ProgressiveHit.SourceName,
							source_detail = prize.ProgressiveHit.SourceDetails
						};

						award.FeatureProgressiveAward.Add(progressiveAward);
					}

					featureEntry.AddAward(award);
				}
				else
				{
					var riskAward = new RiskAward
					{
						amount = prize.Amount.AsMinorCurrency,
						risk_amount = prize.RiskAmount.AsMinorCurrency,
						award_type = RiskAwardAward_type.RoundWagerUpPlayoff
					};

					featureEntry.AddAward(riskAward);
				}
			}

			outcomeList.FeatureEntry.Add(featureEntry);
			outcomeList.GameCycleEntry.Add(new OutcomeListGameCycleEntry { Award = new List<Award>() });

			foundationResponseSignal.Reset();
			GameLib.OutcomeResponseEvent += OnOutcomeResponse;
			foundationResponseEventAdded = true;
			GameLib.AdjustOutcome(outcomeList, results.IsFinalOutcome);
			return gameCycleAward;
		}

		private static Money GenerateGambleOutcome(IOutcome results, bool firstGambleGame, out bool isAborted)
		{
			var gameCycleAward = Money.Zero;
			var outcomeList = new OutcomeList();

			isAborted = results.Prizes.Count == 0;
			if (isAborted)
			{
				if (firstGambleGame)
				{
					var ancillaryAward = new AncillaryAward
					{
						amount = 0,
						is_displayable = true,
						risk_amount = 0,
						win_type = AncillaryAwardWin_type.Cancel
					};

					var featureEntry = new OutcomeListFeatureEntry();
					featureEntry.AddAward(ancillaryAward);

					outcomeList.FeatureEntry.Add(featureEntry);
				}
			}
			else
			{
				var featureEntry = new OutcomeListFeatureEntry();
				foreach (var prize in results.Prizes)
				{
					gameCycleAward += prize.Amount - prize.RiskAmount;

					var winType = AncillaryAwardWin_type.Loss;
					if (prize.Amount == prize.RiskAmount)
						winType = AncillaryAwardWin_type.Tie;
					else if (prize.Amount > prize.RiskAmount)
						winType = AncillaryAwardWin_type.Win;

					var ancillaryAward = new AncillaryAward
					{
						amount = prize.Amount.AsMinorCurrency,
						is_displayable = true,
						risk_amount = prize.RiskAmount.AsMinorCurrency,
						win_type = winType
					};

					featureEntry.AddAward(ancillaryAward);
				}

				outcomeList.FeatureEntry.Add(featureEntry);
			}

			foundationResponseSignal.Reset();
			GameLib.OutcomeResponseEvent += OnOutcomeResponse;
			foundationResponseEventAdded = true;
			GameLib.AdjustOutcome(outcomeList, results.IsFinalOutcome);
			return gameCycleAward;
		}

		private static void OnOutcomeResponse(object sender, OutcomeResponseEventArgs e)
		{
			GameLib.WriteCriticalData(CriticalDataScope.GameCycle, FoundationResponseCriticalData, e);
			foundationResponseSignal.Set();
		}

		private static void AwaitOutcomeResponse()
		{
			var gameCycleState = GameLib.QueryGameCycleState();
			try
			{
				if (gameCycleState == GameCycleState.EvaluatePending || gameCycleState == GameCycleState.AncillaryEvaluatePending)
				{
					if (!foundationResponseEventAdded)
					{
						foundationResponseSignal.Reset();
						GameLib.OutcomeResponseEvent += OnOutcomeResponse;
					}

					ProcessEvents(foundationResponseSignal);
				}
			}
			finally
			{
				GameLib.OutcomeResponseEvent -= OnOutcomeResponse;
				foundationResponseEventAdded = false;
			}

			// Check if we have an outcome response.
			// Should not happen in a standard game but there is a small window in standalone where this can be null.

			var outcomeResponse = GameLib.ReadCriticalData<OutcomeResponseEventArgs>(CriticalDataScope.GameCycle, FoundationResponseCriticalData);
			if (outcomeResponse == null)
			{
				Log.Instance.Fatal("Foundation failed to return an outcome response.");
				throw new Exception("Foundation failed to return an outcome response.");
			}

			CheckOutcomeForProgressives(outcomeResponse.AdjustedOutcome);
		}

		private static void RemoveOutComeResponse()
		{
			GameLib.RemoveCriticalData(CriticalDataScope.GameCycle, FoundationResponseCriticalData);
		}

		private static bool GetIsFinishFromOutcomeResponse()
		{
			var outcomeResponse = GameLib.ReadCriticalData<OutcomeResponseEventArgs>(CriticalDataScope.GameCycle, FoundationResponseCriticalData);
			return outcomeResponse.IsFinalOutcome;
		}

		private static void FinalizeOutcome()
		{
			foundationResponseEventAdded = true;
			foundationResponseSignal.Reset();
			GameLib.FinalizeOutcomeEvent += OnFinalizeOutcomeResponse;
			GameLib.FinalizeOutcome();
		}

		private static void OnFinalizeOutcomeResponse(object sender, FinalizeOutcomeEventArgs e)
		{
			GameLib.WriteCriticalData(CriticalDataScope.GameCycle, FoundationResponseCriticalData, e);
			foundationResponseSignal.Set();
		}

		private static void AwaitFinalize()
		{
			var gameCycleState = GameLib.QueryGameCycleState();
			try
			{
				if (gameCycleState == GameCycleState.FinalizeAwardPending)
				{
					if (!foundationResponseEventAdded)
					{
						foundationResponseSignal.Reset();
						GameLib.FinalizeOutcomeEvent += OnFinalizeOutcomeResponse;
					}

					ProcessEvents(foundationResponseSignal);
				}
			}
			finally
			{
				GameLib.FinalizeOutcomeEvent -= OnFinalizeOutcomeResponse;
				foundationResponseEventAdded = false;
			}

			// Check if we have an finalize response.
			// Should not happen in a standard game but there is a small window in standalone where this can be null.

			var finalizeResponse = GameLib.ReadCriticalData<FinalizeOutcomeEventArgs>(CriticalDataScope.GameCycle, FoundationResponseCriticalData);
			if (finalizeResponse == null)
				throw new Exception("Foundation failed to return a finalize response.");

			GameLib.RemoveCriticalData(CriticalDataScope.GameCycle, FoundationResponseCriticalData);
		}

		private static void SetOfferGamble()
		{
			var offerGamble = false;
			if (!GameLib.TryReadCriticalData(ref offerGamble, CriticalDataScope.GameCycle, OfferGambleCriticalData))
			{
				offerGamble = GameLib.OfferAncillaryGame();
				GameLib.WriteCriticalData(CriticalDataScope.GameCycle, OfferGambleCriticalData, offerGamble);
			}

			GameLogic.SetIsGambleOfferable(offerGamble);
		}

		private static void RefreshOfferGamble()
		{
			var offerGamble = false;
			GameLib.TryReadCriticalData(ref offerGamble, CriticalDataScope.GameCycle, OfferGambleCriticalData);
			GameLogic.SetIsGambleOfferable(offerGamble);
		}

		private static void ClearOfferGamble()
		{
			GameLogic.SetIsGambleOfferable(false);
		}

		#endregion
	}
}