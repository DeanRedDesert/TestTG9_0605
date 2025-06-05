//-----------------------------------------------------------------------
// <copyright file = "GameLib.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList;
    using Ascent.OutcomeList.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Communication.Standalone;
    using Communication.Standalone.Schemas;
    using CompactSerialization;
    using Foundation;
    using InterfaceExtensions.Interfaces;
    using Registries;
    using TopScreenGameAdvertisementType = Ascent.Communication.Platform.Interfaces.TopScreenGameAdvertisementType;
    using TournamentSessionType = InterfaceExtensions.Interfaces.TournamentSessionType;
    using Version = System.Version;

    /// <summary>
    /// This is a standalone game lib which emulates the
    /// functionality which would normally be provided by
    /// a foundation.
    /// </summary>
    public class GameLib : IGameLib, IGameLibRestricted, IGameLibDemo, IGameLibShow, IDisposable,
                           IAppLibRestrictedInfo, ITransactionVerification, IEventSource,
                           ITransactionAugmenter, IEventDispatcher,
                           ITransactionWeightVerificationDependency, ILayeredContextActivationEventsDependency,
                           ICriticalDataDependency, ICultureInfoDependency, IGameModeQuery,
                           IStandaloneGameInformationDependency, IStandaloneCriticalDataDependency,IGameCycleStateQuery,
                           IStandaloneEventPosterDependency, IStandaloneOutcomeAdjusterDependency,
                           IStandalonePlayStatusDependency
    {
        #region Delegates

        private readonly Dictionary<GameCycleState, Action> enterState = new Dictionary<GameCycleState, Action>();
        /// <summary>
        /// Create the lookup table for enter state handlers.
        /// </summary>
        private void CreateEnterStateTable()
        {
            enterState[GameCycleState.Idle] = EnterIdle;
            enterState[GameCycleState.Playing] = EnterPlaying;
            enterState[GameCycleState.AncillaryPlaying] = EnterAncillaryPlaying;
            enterState[GameCycleState.MainPlayComplete] = EnterMainPlayComplete;
        }

        private readonly Dictionary<GameCycleState, Action> exitState = new Dictionary<GameCycleState, Action>();
        /// <summary>
        /// Create the lookup table for exit state handlers.
        /// </summary>
        private void CreateExitStateTable()
        {
            exitState[GameCycleState.Idle] = ExitIdle;
            exitState[GameCycleState.Playing] = ExitPlaying;
            exitState[GameCycleState.AncillaryPlaying] = ExitAncillaryPlaying;
            exitState[GameCycleState.MainPlayComplete] = ExitMainPlayComplete;
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<ActivateThemeContextEventArgs> ActivateThemeContextEvent;

        /// <inheritdoc />
        public event EventHandler<AutoPlayOffEventArgs> AutoPlayOffEvent;

        /// <inheritdoc />
        public event EventHandler<AutoPlayOnRequestEventArgs> AutoPlayOnRequestEvent;

        /// <inheritdoc />
        public event EventHandler<BankStatusChangedEventArgs> BankStatusChangedEvent;

        /// <inheritdoc />
        /// <devdoc>
        /// Currently there is not a way to simulate this event as cancellations are controlled by external factors.
        /// </devdoc>
        public event EventHandler<DenominationChangeCancelledEventArgs> DenominationChangeCancelledEvent;

        /// <inheritdoc />
        public event EventHandler<DisableAncillaryGameOfferEventArgs> DisableAncillaryGameOfferEvent;

        /// <inheritdoc/>
        public event EventHandler<DisplayControlEventArgs> DisplayControlEvent;

        /// <inheritdoc />
        public event EventHandler<EnrollResponseEventArgs> EnrollResponseEvent;

        /// <inheritdoc />
        public event EventHandler<FinalizeOutcomeEventArgs> FinalizeOutcomeEvent;

        /// <inheritdoc />
        public event EventHandler<InactivateThemeContextEventArgs> InactivateThemeContextEvent;

        /// <inheritdoc />
        public event EventHandler<LanguageChangedEventArgs> LanguageChangedEvent;

        /// <inheritdoc />
        public event EventHandler<MoneyEventArgs> MoneyEvent;

        /// <inheritdoc />
        public event EventHandler<NewThemeContextEventArgs> NewThemeContextEvent;

        /// <inheritdoc />
        public event EventHandler<NewThemeSelectionEventArgs> NewThemeSelectionEvent;

        /// <inheritdoc />
        public event EventHandler<OutcomeResponseEventArgs> OutcomeResponseEvent;

        /// <inheritdoc />
        public event EventHandler<ParkEventArgs> ParkEvent;

        /// <inheritdoc />
        public event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <inheritdoc />
        public event EventHandler<SwitchThemeContextEventArgs> SwitchThemeContextEvent;

        /// <inheritdoc />
        public event EventHandler<ThemeSelectionMenuOfferableStatusChangedEventArgs>
            ThemeSelectionMenuOfferableStatusChangedEvent;

        /// <inheritdoc />
        public event EventHandler<VoucherPrintEventArgs> VoucherPrintNotificationEvent;

        /// <inheritdoc />
        /// <DevDoc>
        /// This event does not go into eventTable, since it is raised
        /// directly without going to the event queue.
        /// </DevDoc>
        public event EventHandler<ProgressiveBroadcastEventArgs> ProgressiveBroadcastEvent
        {
            add
            {
                // The field-like event's addition/subtraction actions are guaranteed thread-safe by the compiler.
                // We have to guarantee the same here.
                lock(progressiveBroadcastEventLocker)
                {
                    // If this is the first handler wired to the event...
                    if(progressiveBroadcastEvent == null)
                    {
                        // Enable the broadcast manager.
                        progressiveBroadcastManager.EnableBroadcasting(true);
                    }

                    progressiveBroadcastEvent += value;
                }
            }

            remove
            {
                lock(progressiveBroadcastEventLocker)
                {
                    progressiveBroadcastEvent -= value;

                    // If there is no more registered delegate...
                    if(progressiveBroadcastEvent == null)
                    {
                        // Disable the broadcast manager.
                        progressiveBroadcastManager.EnableBroadcasting(false);
                    }
                }
            }
        }

        /// <inheritdoc />
        /// <DevDoc>
        /// This event does not go into eventTable, since it is raised
        /// directly without going to the event queue.
        /// </DevDoc>
        public event EventHandler<DenominationsWithProgressivesBroadcastEventArgs> AvailableDenominationsWithProgressivesBroadcastEvent
        {
            add
            {
                // The field-like event's addition/subtraction actions are guaranteed thread-safe by the compiler.
                // We have to guarantee the same unconditionally.
                lock(availableDenominationsWithProgressivesBroadcastEventLocker)
                {
                    // If this is the first handler wired to the event...
                    if(availableDenominationsWithProgressivesBroadcastEvent == null)
                    {
                        // Enable the broadcasting.
                        progressiveBroadcastManager.EnableBroadcasting(true, nameof(AvailableDenominationsWithProgressivesBroadcastEvent));
                    }

                    availableDenominationsWithProgressivesBroadcastEvent += value;
                }
            }

            remove
            {
                lock(availableDenominationsWithProgressivesBroadcastEventLocker)
                {
                    availableDenominationsWithProgressivesBroadcastEvent -= value;

                    // If there is no more registered delegates...
                    if(availableDenominationsWithProgressivesBroadcastEvent == null)
                    {
                        // Disable the broadcasting.
                        progressiveBroadcastManager.EnableBroadcasting(false, nameof(AvailableDenominationsWithProgressivesBroadcastEvent));
                    }
                }
            }
        }

        #endregion

        #region Fields for Event Implementation

        // Back end fields for progressive broadcast events.
        // ReSharper disable InconsistentNaming
        private event EventHandler<ProgressiveBroadcastEventArgs> progressiveBroadcastEvent;
        private event EventHandler<DenominationsWithProgressivesBroadcastEventArgs> availableDenominationsWithProgressivesBroadcastEvent;
        // ReSharper restore InconsistentNaming

        // Locker objects guarding the back end fields of progressive broadcast events.
        private readonly object progressiveBroadcastEventLocker = new object();
        private readonly object availableDenominationsWithProgressivesBroadcastEventLocker = new object();

        // An instance of empty broadcast data to avoid repeated new allocations.
        private readonly IDictionary<int, ProgressiveBroadcastData> emptyBroadcastData = new Dictionary<int, ProgressiveBroadcastData>();

        /// <summary>
        /// Lookup table for the events that are posted to the event queue.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> eventTable = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// Lookup table for the events that are posted to the event queue that do not require transactions.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> nonTransactionalEventTable = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// Create the event lookup table.
        /// </summary>
        private void CreateEventLookupTable()
        {
            // Transactional Events.
            eventTable[typeof(ActivateThemeContextEventArgs)] =
                (sender, eventArgs) => ExecuteLayeredContextActivation(sender, eventArgs,
                    ActivateThemeContextEvent,
                    ActivateLayeredContextEventField, ContextLayer.LegacyTheme);

            eventTable[typeof(AutoPlayOffEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, AutoPlayOffEvent);
            eventTable[typeof(AutoPlayOnRequestEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, AutoPlayOnRequestEvent);
            eventTable[typeof(BankStatusChangedEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, BankStatusChangedEvent);
            eventTable[typeof(DenominationChangeCancelledEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, DenominationChangeCancelledEvent);
            eventTable[typeof(LanguageChangedEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, LanguageChangedEvent);
            eventTable[typeof(DisableAncillaryGameOfferEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, DisableAncillaryGameOfferEvent);
            eventTable[typeof(DisplayControlEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, DisplayControlEvent);
            eventTable[typeof(EnrollResponseEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, EnrollResponseEvent);
            eventTable[typeof(FinalizeOutcomeEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, FinalizeOutcomeEvent);

            eventTable[typeof(InactivateThemeContextEventArgs)] =
                (sender, eventArgs) => ExecuteLayeredContextActivation(sender, eventArgs,
                    InactivateThemeContextEvent,
                    InactivateLayeredContextEventField, ContextLayer.LegacyTheme);

            eventTable[typeof(MoneyEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, MoneyEvent);
            eventTable[typeof(NewThemeContextEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, NewThemeContextEvent);
            eventTable[typeof(NewThemeSelectionEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, NewThemeSelectionEvent);
            eventTable[typeof(OutcomeResponseEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, OutcomeResponseEvent);
            eventTable[typeof(ParkEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ParkEvent);
            eventTable[typeof(ShutDownEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ShutDownEvent);

            eventTable[typeof(SwitchThemeContextEventArgs)] =
                (sender, eventArgs) => ExecuteLayeredContextActivation(sender, eventArgs,
                    SwitchThemeContextEvent,
                    InactivateLayeredContextEventField, ContextLayer.LegacyTheme);

            eventTable[typeof(ThemeSelectionMenuOfferableStatusChangedEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, ThemeSelectionMenuOfferableStatusChangedEvent);
            eventTable[typeof(VoucherPrintEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, VoucherPrintNotificationEvent);

            //Non-Transactional Events
            nonTransactionalEventTable[typeof(FoundationStateChangedEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, FoundationStateChangedEvent);
            nonTransactionalEventTable[typeof(PlayerBankMeterChangedEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, PlayerBankMeterChangedEvent);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Standalone version of safe storage implementation.
        /// </summary>
        internal readonly IDiskStoreManager DiskStoreManager;

        /// <summary>
        /// Flag indicating if it is a cold start.
        /// Used to synchronize Game Lib and theme context initializations.
        /// </summary>
        private readonly bool isColdStart;

        /// <summary>
        /// Transaction id is used for debugging to indicate
        /// the number of transactions which have been performed.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private int transactionId;

        /// <summary>
        /// Locker object guarding the event related fields, including
        /// foundationTransactionOpen and foundationEvents.
        /// </summary>
        private readonly object eventLocker = new object();

        /// <summary>
        /// Event used for blocking process event calls.
        /// </summary>
        private readonly ManualResetEvent eventResetEvent = new ManualResetEvent(false);

        /// <summary>
        /// This flag indicates if a Foundation initiated transaction is currently open.
        /// The Standalone Game Lib simulates to open a Foundation initiated transaction
        /// when processing events.
        /// </summary>
        /// <remarks>Guarded by eventLocker.</remarks>
        private bool foundationTransactionOpen;

        /// <summary>
        /// Event queue to hold the incoming events posted by the Foundation.
        /// </summary>
        /// <remarks>Guarded by eventLocker.</remarks>
        /// <devdoc>
        /// There may not be a way for there to be multiple events
        /// but for now the lib will assume support for it.
        /// </devdoc>
        private readonly List<EventArgs> foundationEvents = new List<EventArgs>();

        /// <summary>
        /// Locker object guarding the non transactional event related fields, including
        /// non transactional foundation Events.
        /// </summary>
        private readonly object nonTransactionalEventLocker = new object();

        /// <summary>
        /// Event queue to hold the incoming non transactional events posted by the Foundation.
        /// </summary>
        /// <remarks>Guarded by nonTransactionalEventLocker.</remarks>
        private readonly List<EventArgs> nonTransactionalFoundationEvents = new List<EventArgs>();

        /// <summary>
        /// Flag indicating if the managed resources of this object have been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// This object manages the theme registry and payvar registries.
        /// </summary>
        private readonly GameRegistryManager gameRegistryManager;

        /// <summary>
        /// This object manages the paytable list and the associated denominations.
        /// </summary>
        private readonly PaytableListManager paytableListManager;

        /// <summary>
        /// This object translates a safe storage selector (scope + identifier) used in writing/reading methods
        /// to a designated disk store location (section + index).
        /// </summary>
        private readonly DiskStoreSectionIndexer diskStoreSectionIndexer;

        /// <summary>
        /// The object manages the history records and history browsing.
        /// </summary>
        private readonly HistoryManager historyManager;

        /// <summary>
        /// This object manages the progressive controllers and progressive links.
        /// </summary>
        private readonly ProgressiveManager progressiveManager;

        /// <summary>
        /// Object used to coordinating event processing.
        /// </summary>
        private readonly EventCoordinator eventCoordinator;

        /// <summary>
        /// Dictionary of available Game Lib services keyed by their interface type.
        /// </summary>
        private readonly Dictionary<Type, object> builtinServices = new Dictionary<Type, object>();

        /// <summary>
        /// The progressive broadcast manager controls the time interval to poll the
        /// progressive broadcast data from the Foundation.
        /// </summary>
        private readonly ProgressiveBroadcastManager progressiveBroadcastManager;

        /// <summary>
        /// The progressive simulator that will be enabled if specified in the standalone foundation
        /// configuration file.
        /// </summary>
        private readonly ProgressiveSimulator progressiveSimulator;

        /// <summary>
        /// The time interval between posting the progressive
        /// broadcast events.
        /// </summary>
        public const uint ProgressiveBroadcastInterval = 500;

        /// <summary>
        /// Random number generator.
        /// </summary>
        private readonly IRandomNumberGenerator random;

        /// <summary>
        /// An optional <see cref="IPrepickedValueProvider"/> object which can be used
        /// to substitute values in place of randomly generated values.
        /// </summary>
        private IPrepickedValueProvider prepickedValueProvider;

        /// <summary>
        /// List of environment attributes.
        /// </summary>
        private readonly List<EnvironmentAttribute> environmentAttributes;

        /// <summary>
        /// Flag indicating if current gaming environment
        /// supports banked credit functionality.
        /// </summary>
        private readonly bool isBankedCreditsEnvironment;

        /// <summary>
        /// Object used to request localization information.
        /// </summary>
        private readonly LocalizationInformation localizationInformation;

        /// <summary>
        /// Flag indicating if the auto play is currently on or off.
        /// </summary>
        private volatile bool isAutoPlayOn;

        /// <summary>
        /// The auto play system configuration.
        /// </summary>
        private readonly AutoPlayConfiguration autoPlaySysConfig;

        /// <summary>
        /// If auto play confirmation is required.
        /// </summary>
        private readonly bool autoPlayConfirmationRequired;

        /// <summary>
        /// If auto play speed increase is allowed.
        /// </summary>
        private readonly bool? autoPlaySpeedIncreaseAllowed;

        /// <summary>
        /// Foundation owned config setting if round wager up playoff is enabled.
        /// </summary>
        private readonly bool roundWagerUpPlayoffEnabledConfig;

        /// <summary>
        /// Whenever the current credit balance falls below creditThreshold this amount of credits will be added.
        /// </summary>
        private long creditsToAdd;

        /// <summary>
        /// Threshold at which to automatically add more credits.
        /// </summary>
        private long creditThreshold;

        /// <summary>
        /// The show environment.  Only valid when <see cref="ShowMode"/> is true.
        /// </summary>
        private ShowEnvironment showEnvironment;

        /// <summary>
        /// The internal game mode value to be used when posting theme events
        /// back to back.
        /// </summary>
        /// <remarks>
        /// Both theme context activation and inactivation have to check the game
        /// mode to decided whether to post the display control state event.  But
        /// the GameContextMode property is updated within the theme context event
        /// handlers.  It works well with Standard Game Lib, where an event must
        /// be processed before the next event is posted.  However, with Standalone
        /// Game Lib, where multiple theme context events can be posted to the event
        /// queue before any event is processed, GameContextMode is not a reliable
        /// information source for making decisions.  That is why this internal
        /// game mode value is helpful.
        /// </remarks>
        private GameMode processedGameMode;

        /// <summary>
        /// The foundation owned ancillary configurations.
        /// </summary>
        private readonly AncillaryConfiguration ancillarySysConfiguration;

        /// <summary>
        /// Events which were restored on a warm boot. Should only be used
        /// during initialization.
        /// </summary>
        private List<EventArgs> restoredFoundationEvents;

        /// <summary>
        /// These events should not be persisted to safe storage.
        /// </summary>
        private readonly List<Type> eventPersistenceFilter = new List<Type>
            {
                typeof(NewThemeContextEventArgs),
                typeof(NewThemeSelectionEventArgs),
                typeof(ActivateThemeContextEventArgs),
                typeof(InactivateThemeContextEventArgs),
                typeof(DisplayControlEventArgs),
                typeof(SwitchThemeContextEventArgs)
            };

        /// <summary>
        /// Dictionary of interface extensions installed in this game lib.
        /// </summary>
        private readonly Dictionary<Type, IInterfaceExtension> interfaceExtensions =
            new Dictionary<Type, IInterfaceExtension>();

        /// <summary>
        /// The tournament session configuration parser.
        /// </summary>
        private readonly TournamentSessionConfigParser tournamentSessionConfigParser;

        /// <summary>
        /// The stomp broker configuration parser.
        /// </summary>
        private readonly StompBrokerConfigParser stompBrokerConfigParser;

        /// <summary>
        /// The object used to query the configuration items.
        /// </summary>
        private readonly GameConfigurationRead configurationRead;

        /// <summary>
        /// That game play enforcements that are in effect.
        /// This field should be modified only through IGameLibDemo methods,
        /// and should be read only on main logic thread.
        /// </summary>
        private GamePlayEnforcements gamePlayEnforcements = GamePlayEnforcements.None;

        #endregion

        #region Constants

        #region Safe Storage Paths

        internal const string GameLibPrefix = "GameLib/";
        private const string PlayModeThemeNamePath = "PlayModeThemeName";
        private const string GameStatePath = "GameState";

        private const string PlayerWagerableMeterPath = "PlayerWagerableMeter";
        private const string PlayerBankMeterPath = "PlayerBankMeter";
        private const string PlayerPaidMeterPath = "PlayerPaidMeter";
        private const string IsPlayerWagerOfferablePath = "IsPlayerWagerOfferable";
        private const string IsCashOutOfferablePath = "IsCashOutOfferable";
        private const string IsBankToWagerableOfferablePath = "IsBankToWagerableOfferable";

        private const string CommittedBetPath = "CommittedBet";
        private const string StartingBetPath = "StartingBet";
        private const string MidGameBetPath = "MidGameBet";
        private const string FinalOutcomeListPath = "FinalOutcomeList";

        private const string PlayModeGameDenominationPath = "PlayModeGameDenomination";
        private const string GameLanguagePath = "GameLanguage";
        private const string AvailableLanguagesPath = "AvailableLanguages";
        private const string FoundationEventsPath = "FoundationEvents";
        private const string WagerCategoryInformationPath = "WagerCategoryInformation";
        private const string InterfaceExtensionPrefix = "InterfaceExtensionData/";

        #endregion

        #region Hard Coded Values

        /// <summary>
        /// The denomination to use if no paytable list is defined.
        /// </summary>
        public const long HardCodedDenomination = 100;

        /// <summary>
        /// The language to use if no language license is defined.
        /// </summary>
        public const string HardCodedLanguage = "en-US";

        /// <summary>
        /// The game maximum bet to use if no game registry is defined.
        /// </summary>
        public const long HardCodedGameMaxBet = 100;

        /// <summary>
        /// The button panel min bet to use if no game registry is defined.
        /// </summary>
        public const long HardCodedButtonPanelMinBet = 0;

        /// <summary>
        /// The max history steps to use if none is defined in foundation owned settings.
        /// </summary>
        public const uint HardCodedMaxHistorySteps = 50;

        #endregion

        /// <summary>
        /// The invalid bet amount indicates that appropriate betting functions have not been called yet.
        /// </summary>
        private const long InvalidBetAmount = -1;

        /// <summary>
        /// Number of events to be processed per each ProcessEvents call
        /// for the Standalone version.
        /// </summary>
        private const int EventProcessPace = 1;

        /// <summary>
        /// Number of non transactional events to be processed per each ProcessNonTransactionalEvents call
        /// for the Standalone version.
        /// </summary>
        private const int NonTransactionalEventProcessPace = 1;

        // Lookup table for building error messages for FunctionCallNotAllowedInModeOrStateException.
        private static readonly Dictionary<string, string> ValidStateForFunction = new Dictionary<string, string>
        {
            // Game Cycle Functions.
            {"CanCommitGameCycle",              "Idle"},
            {"CommitGameCycle",                 "Idle"},
            {"UncommitGameCycle",               "Committed"},
            {"EnrollGameCycle",                 "Committed"},
            {"UnenrollGameCycle",               "EnrollComplete"},
            {"CanStartPlaying",                 "EnrollComplete"},
            {"StartPlaying",                    "EnrollComplete"},
            {"EndGameCycle",                    "Finalized"},
            {"OfferAncillaryGame",              "MainPlayComplete"},
            {"StartAncillaryPlaying",           "MainPlayComplete"},
            {"StartBonusPlaying",               "MainPlayComplete or AncillaryPlayComplete"},
            {"AdjustOutcome",                   "Playing, AncillaryPlaying or BonusPlaying"},
            {"FinalizeOutcome",                 "MainPlayComplete, AncillaryPlayComplete or BonusPlayComplete"},
            // Betting Functions.
            {"CanCommitBet",                    "Idle or Committed"},
            {"CanCommitBets",                   "Idle or Committed"},
            {"CommitBet",                       "Committed"},
            {"UncommitBet",                     "Committed or EnrollComplete"},
            {"GetCommittedBet",                 "Committed or EnrollComplete"},
            {"PlaceStartingBet",                "EnrollComplete"},
            {"CanPlaceBet",                     "Playing"},
            {"CanPlaceBetAgainstPendingWins",   "Playing"},
            {"PlaceBet",                        "Playing"},
            {"PlaceBetAgainstPendingWins",      "Playing"},
            {"CanBetNextGameCycle",             "MainPlayComplete"},
            {"SetWagerCategoryInformation",     "Committed, EnrollPending, EnrollComplete, or Playing"},
            // Service Request Functions.
            {"RequestDenominationChange",       "Idle"},
        };


        /// <summary>
        /// States in which a call to SetWagerCategoryInformation is valid.
        /// </summary>
        private static readonly GameCycleState[] ValidStatesToSetWagerCategoryInformation = new[]
            {
                GameCycleState.Committed, GameCycleState.EnrollPending, GameCycleState.EnrollComplete,
                GameCycleState.Playing
            };

        #endregion

        #region Public Properties

        /// <inheritdoc />
        public string GameMountPoint => gameRegistryManager.GameMountPoint;

        /// <summary>
        /// Current game mode.
        /// </summary>
        private GameMode gameContextMode;

        /// <inheritdoc />
        /// <remarks>
        /// Game mode is not power hit tolerant.
        /// </remarks>
        public GameMode GameContextMode
        {
            get => gameContextMode;
            private set
            {
                // Update the internal field as well.
                processedGameMode = value;

                gameContextMode = value;
            }
        }

        /// <inheritdoc />
        public GameSubMode GameSubMode { get; }

        /// <inheritdoc />
        public string PaytableName { get; private set; }

        /// <inheritdoc />
        public string PaytableFileName { get; private set; }

        /// <inheritdoc />
        public int MinimumBaseGameTime => PresentationBehaviorConfigs.MinimumBaseGameTime;

        /// <inheritdoc />
        public int MinimumFreeSpinTime => PresentationBehaviorConfigs.MinimumFreeSpinTime;

        /// <inheritdoc cref="IGameLib" />
        public long GameDenomination { get; private set; }

        /// <inheritdoc />
        public long DefaultGameDenomination => 1;

        private bool isPlayerWagerOfferable = true;
        /// <inheritdoc />
        public bool IsPlayerWagerOfferable
        {
            get
            {
                MustHaveOpenTransaction();
                return isPlayerWagerOfferable;
            }
            // ReSharper disable once UnusedMember.Local
            private set
            {
                if(isPlayerWagerOfferable != value)
                {
                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + IsPlayerWagerOfferablePath,
                                        value);
                    isPlayerWagerOfferable = value;

                    if(GameContextMode == GameMode.Play)
                    {
                        PostEvent(new BankStatusChangedEventArgs(
                            new BankStatus(isPlayerWagerOfferable, isCashOutOfferable, isBankToWagerableOfferable)));
                    }
                }
            }
        }

        private bool isCashOutOfferable;
        /// <inheritdoc />
        public bool IsCashOutOfferable
        {
            get
            {
                MustHaveOpenTransaction();
                return isCashOutOfferable;
            }
            private set
            {
                if(isCashOutOfferable != value)
                {
                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + IsCashOutOfferablePath,
                                        value);
                    isCashOutOfferable = value;

                    if(GameContextMode == GameMode.Play)
                    {
                        PostEvent(new BankStatusChangedEventArgs(
                            new BankStatus(isPlayerWagerOfferable, isCashOutOfferable, isBankToWagerableOfferable)));
                    }
                }
            }
        }

        private bool isBankToWagerableOfferable;
        /// <inheritdoc />
        public bool IsBankToWagerableOfferable
        {
            get
            {
                MustHaveOpenTransaction();
                return isBankToWagerableOfferable;
            }
            private set
            {
                if(isBankToWagerableOfferable != value)
                {
                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + IsBankToWagerableOfferablePath,
                                        value);
                    isBankToWagerableOfferable = value;

                    if(GameContextMode == GameMode.Play)
                    {
                        PostEvent(new BankStatusChangedEventArgs(
                            new BankStatus(isPlayerWagerOfferable, isCashOutOfferable, isBankToWagerableOfferable)));
                    }
                }
            }
        }

        /// <summary>
        /// The configured languages in theme registry.
        /// </summary>
        private List<string> availableLanguages;

        /// <inheritdoc />
        public ICollection<string> AvailableLanguages
        {
            get => availableLanguages;
            private set
            {
                if(ReferenceEquals(availableLanguages, value))
                {
                    return;
                }

                if(availableLanguages != null && value != null)
                {
                    if(availableLanguages.OrderBy(lLanguage => lLanguage)
                                         .SequenceEqual(value.OrderBy(rLanguage => rLanguage)))
                    {
                        return;
                    }
                }

                // Only update the list when the two lists are not content equal.
                availableLanguages = value?.ToList();
                WriteFoundationData(FoundationDataScope.Theme,
                                    GameLibPrefix + AvailableLanguagesPath,
                                    availableLanguages);
            }
        }

        /// <summary>
        /// The current game language.
        /// </summary>
        private string gameLanguage;

        /// <inheritdoc />
        public string GameLanguage
        {
            get => gameLanguage;
            private set
            {
                if(gameLanguage != value)
                {
                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + GameLanguagePath,
                                        value);
                    gameLanguage = value;
                }
            }
        }

        /// <inheritdoc />
        public bool IsThemeSelectionMenuOfferable => false;

        /// <inheritdoc />
        public bool ShowMode { get; private set; }

        /// <inheritdoc />
        public string Jurisdiction { get; }

        /// <inheritdoc />
        public ICollection<WagerCategoryOutcome> GameCycleWagerCategoryInfo
        {
            get
            {
                MustHaveOpenTransaction();
                return ReadCriticalData<List<WagerCategoryOutcome>>(
                    CriticalDataScope.GameCycle,
                    GameLibPrefix + WagerCategoryInformationPath);
            }
            set
            {
                MustHaveOpenTransaction();

                if(GameContextMode != GameMode.Play || !ValidStatesToSetWagerCategoryInformation.Contains(GameState))
                {
                    var message = BuildMessageForWrongStateException();
                    throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
                }

                if(value == null)
                {
                    RemoveCriticalData(
                        CriticalDataScope.GameCycle,
                        GameLibPrefix + WagerCategoryInformationPath);
                    return;
                }

                WriteCriticalData(
                    CriticalDataScope.GameCycle,
                    GameLibPrefix + WagerCategoryInformationPath,
                    value.ToList());
            }
        }

        /// <inheritdoc/>
        public IExtensionImportCollection ExtensionImportCollection => gameRegistryManager.ExtensionImportCollection;

        /// <inheritdoc/>
        public IGameConfigurationRead ConfigurationRead => configurationRead;

        /// <inheritdoc/>
        public ILocalizationInformation LocalizationInformation => localizationInformation;

        #region Foundation Owned Configuration Items

        /// <inheritdoc />
        public long GameMinBetAmount { get; }

        /// <inheritdoc />
        public long GameMaxBet { get; private set; }

        /// <inheritdoc />
        public long GameMinBet { get; private set; }

        /// <inheritdoc />
        public long ButtonPanelMinBet { get; private set; }

        /// <inheritdoc />
        public uint MaxHistorySteps { get; }

        /// <inheritdoc />
        public bool AncillaryEnabled { get; private set; }

        /// <inheritdoc />
        public long AncillaryCycleLimit => ancillarySysConfiguration.AncillaryCycleLimit;

        /// <inheritdoc />
        public long AncillaryMonetaryLimit => ancillarySysConfiguration.AncillaryMonetaryLimit;

        /// <inheritdoc />
        public MaxBetButtonBehavior MaxBetButtonBehavior { get; private set; }

        /// <inheritdoc />
        public LineSelectionMode ConfiguredLineSelectionMode { get; private set; }

        /// <inheritdoc />
        public bool RoundWagerUpPlayoffEnabled => roundWagerUpPlayoffEnabledConfig &&
                                                  gameRegistryManager.GetRoundWagerUpPlayoffSupported(ThemeIdentifier);

        /// <inheritdoc />
        public bool BonusPlayEnabled => true;

        /// <inheritdoc />
        public TopScreenGameAdvertisementType TopScreenGameAdvertisement => marketingBehavior.TopScreenGameAdvertisement;

        /// <inheritdoc />
        public BetSelectionStyleInfo DefaultBetSelectionStyle => GamePlayBehaviorConfigs.DefaultBetSelectionStyle;

        /// <inheritdoc />
        public WinCapInformation WinCapInformation { get; }

        /// <inheritdoc />
        public IGamePlayBehaviorConfigs GamePlayBehaviorConfigs { get; }

        /// <inheritdoc />
        public IPresentationBehaviorConfigs PresentationBehaviorConfigs { get; }

        #endregion

        #endregion

        #region Private Properties

        /// <summary>
        /// Current play mode denomination.
        /// Current play mode denomination which is only updated for play mode.
        /// Play Mode GameDenomination is power hit tolerant.
        /// </summary>
        private long playModeGameDenomination;
        private long PlayModeGameDenomination
        {
            get => playModeGameDenomination;
            set
            {
                if(playModeGameDenomination != value)
                {
                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + PlayModeGameDenominationPath,
                                        value);
                    playModeGameDenomination = value;
                }
            }
        }

        /// <summary>
        /// Current play mode theme.
        /// Current play mode theme which is only updated for play mode.
        /// Play Mode Theme is power hit tolerant.
        /// </summary>
        private string playModeTheme;
        private string PlayModeTheme
        {
            get => playModeTheme;
            set
            {
                if(playModeTheme != value)
                {
                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + PlayModeThemeNamePath,
                                        value);
                    playModeTheme = value;
                }
            }
        }

        /// <summary>
        /// Current theme identifier.
        /// </summary>
        private string ThemeIdentifier { get; set; }

        /// <summary>
        /// The Game Cycle State maintained by the Foundation.
        /// </summary>
        private GameCycleState gameState;

        private GameCycleState GameState
        {
            get => gameState;
            set
            {
                if(gameState != value)
                {
                    // Check if game-in-progress status changed.
                    var toRaise = gameState == GameCycleState.Idle
                                  ? new GameInProgressStatusEventArgs(true)
                                  : value == GameCycleState.Idle
                                    ? new GameInProgressStatusEventArgs(false)
                                    : null;

                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + GameStatePath,
                                        value);

                    gameState = value;

                    // Notify listeners about the game-in-progress status change.
                    if(toRaise != null)
                    {
                        GameInProgressStatusEvent?.Invoke(this, toRaise);
                    }
                }
            }
        }

        /// <summary>
        /// Represents the amount of money available for player betting
        /// that is suitable for display to the player, in base units.
        /// This meter is typically labeled Credit in banked-credits environments.
        /// In non banked-credit environments this meter is always zero and is
        /// typically not shown.
        /// </summary>
        private long playerWagerableMeter;
        private long PlayerWagerableMeter
        {
            get => playerWagerableMeter;
            set
            {
                // Tournament uses a different set of meters.
                if(GameSubMode != GameSubMode.Tournament && playerWagerableMeter != value)
                {
                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + PlayerWagerableMeterPath,
                                        value);
                    playerWagerableMeter = value;
                }
            }
        }

        /// <summary>
        /// Represents the amount of money that may be eligible for cashout
        /// and/or transferable to the player wagerable meter, and is suitable
        /// for display to the player, in base units.
        /// This meter includes the wagerable amount in environments that
        /// do not use banked-credits.  This meter is typically labeled Bank
        /// in banked-credit environments and Credit in other environments.
        /// </summary>
        private long playerBankMeter;

        private long PlayerBankMeter
        {
            get => playerBankMeter;
            set
            {
                // Tournament uses a different set of meters.
                if(GameSubMode != GameSubMode.Tournament && playerBankMeter != value)
                {
                    // Check if money-on-machine status changed
                    var toRaise = playerBankMeter == 0
                                      ? new MoneyOnMachineStatusEventArgs(true)
                                      : value == 0
                                          ? new MoneyOnMachineStatusEventArgs(false)
                                          : null;

                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + PlayerBankMeterPath,
                                        value);

                    playerBankMeter = value;

                    // Post event that the player bank value has changed.
                    PostNonTransactionalEvent(new PlayerBankMeterChangedEventArgs(playerBankMeter));

                    // Notify listeners about the money-on-machine status change.
                    if(toRaise != null)
                    {
                        MoneyOnMachineStatusEvent?.Invoke(this, toRaise);
                    }
                }
            }
        }

        /// <summary>
        /// Represents the amount paid to the player during the last/current cashout
        /// and is suitable for display to the player, in base units.
        /// This meter will be reset to zero at the appropriate times
        /// (e.g. at game start or cash in).
        /// </summary>
        private long playerPaidMeter;
        private long PlayerPaidMeter
        {
            get => playerPaidMeter;
            set
            {
                // Tournament uses a different set of meters.
                if(GameSubMode != GameSubMode.Tournament && playerPaidMeter != value)
                {
                    WriteFoundationData(FoundationDataScope.Theme,
                                        GameLibPrefix + PlayerPaidMeterPath,
                                        value);
                    playerPaidMeter = value;
                }
            }
        }

        /// <summary>
        /// Bet amount that has been committed by the game, in base units.
        /// InvalidBetAmount means no bet has been committed.
        /// </summary>
        private long committedBet;
        private long CommittedBet
        {
            get => committedBet;
            set
            {
                if(committedBet != value)
                {
                    WriteFoundationData(FoundationDataScope.GameCycle,
                                        GameLibPrefix + CommittedBetPath,
                                        value);
                    committedBet = value;
                }
            }
        }

        /// <summary>
        /// Validated committed bet amount that can be used for credit
        /// calculation.
        /// </summary>
        private long ValidCommittedBet => CommittedBet == InvalidBetAmount ? 0 : CommittedBet;

        /// <summary>
        /// Bet amount that has been placed at the start of the game cycle,
        /// in base units.
        /// InvalidBetAmount means no bet has been placed for starting play.
        /// </summary>
        private long startingBet;
        private long StartingBet
        {
            get => startingBet;
            set
            {
                // Always write -1 to ensure it's re-written after a critical data clear.
                if(startingBet != value || value == InvalidBetAmount)
                {
                    WriteFoundationData(FoundationDataScope.GameCycle,
                                        GameLibPrefix + StartingBetPath,
                                        value);
                    startingBet = value;
                }
            }
        }

        /// <summary>
        /// The cumulative bet amount that has been placed during the game play,
        /// in base units.
        /// </summary>
        private long midGameBet;
        private long MidGameBet
        {
            get => midGameBet;
            set
            {
                if(midGameBet != value)
                {
                    WriteFoundationData(FoundationDataScope.GameCycle,
                                        GameLibPrefix + MidGameBetPath,
                                        value);
                    midGameBet = value;
                }
            }
        }

        /// <summary>
        /// The last final outcome list sent by the game, and adjusted by the Foundation.
        /// </summary>
        private OutcomeList FinalOutcomeList
        {
            get =>
                ReadFoundationData<OutcomeList>(FoundationDataScope.GameCycle,
                    GameLibPrefix + FinalOutcomeListPath);
            set =>
                WriteFoundationData(FoundationDataScope.GameCycle,
                    GameLibPrefix + FinalOutcomeListPath,
                    value);
        }

        /// <summary>
        /// The default amount to be transferred from bank meter to wagerable meter, in unit of cents.
        /// </summary>
        private long BankToWagerableDefaultAmount
        {
            get;
        }

        /// <summary>
        /// The EGM wide setting for the marketing behavior.
        /// </summary>
        private readonly MarketingBehavior marketingBehavior;

        #endregion

        #region Methods

        #region IGameLib Implementation

        #region Persistent Storage, Configuration, Meter, RNG and Service Request Functions

        /// <inheritdoc />
        public T ReadCriticalData<T>(CriticalDataScope scope, CriticalDataName path)
        {
            T obj = default;
            ReadCriticalData(ref obj, scope, path);
            return obj;
        }

        /// <inheritdoc />
        public void ReadCriticalData<T>(ref T data, CriticalDataScope scope, CriticalDataName path)
        {
            if(!TryReadCriticalData(ref data, scope, path))
            {
                data = default;
            }
        }

        /// <inheritdoc />
        public bool TryReadCriticalData<T>(ref T data, CriticalDataScope scope, CriticalDataName path)
        {
            MustHaveOpenTransaction();

            Utility.ValidateCriticalDataAccess(GameContextMode, GameState, DataAccessing.Read, scope);

            var success = false;

            var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(scope);
            if(DiskStoreManager.Contains(section, index, path))
            {
                DiskStoreManager.Read(ref data, section, index, path);
                success = true;
            }

            return success;
        }

        /// <inheritdoc />
        public void WriteCriticalData(CriticalDataScope scope, CriticalDataName path, object data)
        {
            MustHaveOpenTransaction();

            if(data == null)
            {
                throw new ArgumentNullException(nameof(data), "Null data cannot be written to critical data.");
            }

            Utility.ValidateCriticalDataAccess(GameContextMode, GameState, DataAccessing.Write, scope);

            // The reason this check is done here and not in the disk store manager is that the
            // standalone game lib writes other internal data to critical data that can't be
            // compact serialized at this time. Placing the check here prevents warnings the
            // user can't fix.
            if(!CompactSerializer.Supports(data.GetType()))
            {
                Logging.Log.WriteWarning(
                    $"The type {data.GetType().FullName} written to critical data path {path} does not support ICompactSerializable. This will cause inefficient critical data usage. It is recommended that the interface be added.");
            }

            var (section, index)= diskStoreSectionIndexer.GetCriticalDataLocation(scope);
            DiskStoreManager.Write(section, index, path, data);
        }

        /// <inheritdoc />
        public bool RemoveCriticalData(CriticalDataScope scope, CriticalDataName path)
        {
            MustHaveOpenTransaction();

            Utility.ValidateCriticalDataAccess(GameContextMode, GameState, DataAccessing.Remove, scope);

            var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(scope);
            return DiskStoreManager.Remove(section, index, path);
        }

        /// <inheritdoc />
        public ICollection<int> GetRandomNumbers(RandomValueRequest request)
        {
            MustHaveOpenTransaction();

            if(prepickedValueProvider != null)
            {
                // If a pre-picked value provider is available, try getting the values from it first.
                var prepickResult = prepickedValueProvider.GetPrepickedValues(request);

                // If all of the values were provided, return them here.
                if(prepickResult.PrepickedValues != null)
                {
                    return prepickResult.PrepickedValues;
                }

                // Use the newly created request
                request = prepickResult.NewRandomValueRequest;
            }

            return random.GetRandomNumbers(request);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="requestList"/> is null.
        /// </exception>
        public ICollection<int> GetRandomNumbers(ICollection<RandomValueRequest> requestList)
        {
            MustHaveOpenTransaction();

            if(requestList == null)
            {
                throw new ArgumentNullException(nameof(requestList));
            }

            var randomNumbers = new List<int>();

            if(prepickedValueProvider != null)
            {
                /* Get the pre-picked values for each request.  Each result contains either the values for the request or a new
                 * request that may be forwarded to the RNG. */
                var prepickResults = prepickedValueProvider.GetPrepickedValues(requestList);

                // Add the values for the filled requests.
                randomNumbers.AddRange(prepickResults.Where(result => result.PrepickedValues != null).SelectMany(result => result.PrepickedValues));

                if(prepickResults.All(result => result.PrepickedValues != null))
                {
                    return randomNumbers;
                }

                // Create a new request list using results that have new requests.
                requestList =
                    prepickResults.Where(result => result.NewRandomValueRequest != null).Select(
                        result => result.NewRandomValueRequest).ToList();
            }

            foreach(var request in requestList)
            {
                randomNumbers.AddRange(random.GetRandomNumbers(request));
            }

            return randomNumbers;
        }

        /// <inheritdoc />
        public bool RequestDenominationChange(long newDenomination)
        {
            MustHaveOpenTransaction();
            var requestAccepted = false;

            if(newDenomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newDenomination), "Denomination must be greater than 0");
            }

            if(GameContextMode == GameMode.Play && GameState == GameCycleState.Idle)
            {
                // It is an error to request a switch to a denomination that
                // is not available for the player to pick.
                if(!GetAvailableDenominations().Contains(newDenomination))
                {
                    throw new InvalidOperationException(
                        $"Denomination {newDenomination} is not enabled for current theme.");
                }

                // Set to the new denomination only when it is different from the current value.
                if(newDenomination != GameDenomination)
                {
                    if(IsEnforced(GamePlayEnforcements.DenominationChangeFail))
                    {
                        PostEvent(new DenominationChangeCancelledEventArgs());
                    }
                    else
                    {
                        PlayModeGameDenomination = newDenomination;

                        // Post theme context events.
                        ActivateThemeContext(GameContextMode,
                                             paytableListManager.GetPaytableVariant(ThemeIdentifier, newDenomination),
                                             newDenomination,
                                             true);
                    }

                    requestAccepted = true;
                }
            }
            else
            {
                var message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }

            return requestAccepted;
        }

        /// <inheritdoc />
        public bool RequestThemeSelectionMenu()
        {
            MustHaveOpenTransaction();

            // Nothing to do for Standalone version.

            return GameContextMode == GameMode.Play &&
                   QueryGameCycleState() == GameCycleState.Idle &&
                   IsThemeSelectionMenuOfferable;
        }

        /// <inheritdoc />
        public void RequestServiceWindow()
        {
            MustHaveOpenTransaction();

            // Nothing to do for Standalone version.
        }

        /// <inheritdoc />
        public ICollection<long> GetAvailableDenominations()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play)
            {
                return paytableListManager.GetAvailableDenominations(ThemeIdentifier) ?? new List<long> { HardCodedDenomination };
            }

            throw new FunctionCallNotAllowedInModeOrStateException(GameContextMode, GameState);
        }

        /// <inheritdoc />
        public ICollection<long> GetAvailableProgressiveDenominations()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play)
            {
                var availableDenominations = GetAvailableDenominations();
                var progressiveDenominations = new List<long>();

                progressiveDenominations.AddRange(
                    from denom in availableDenominations
                    let paytable = paytableListManager.GetPaytableVariant(ThemeIdentifier, denom)
                    let progressiveLevels = progressiveManager.GetProgressiveBroadcastDataForDenominationAndPaytable(denom, paytable)
                    where progressiveLevels.Count > 0 || progressiveManager.HasGameControlledProgressive(paytable)
                    select denom);

                return progressiveDenominations.OrderBy(denomination => denomination).ToList();
            }

            throw new FunctionCallNotAllowedInModeOrStateException(GameContextMode, GameState);
        }

        /// <inheritdoc />
        public IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> GetAvailableDenominationsWithProgressives()
        {
            MustHaveOpenTransaction();

            return RetrieveAvailableDenominationsWithProgressives();
        }

        /// <inheritdoc />
        public IDictionary<int, ProgressiveBroadcastData> GetAvailableProgressiveBroadcastData(long denom)
        {
            MustHaveOpenTransaction();

            var paytable = paytableListManager.GetPaytableVariant(ThemeIdentifier, denom);

            return progressiveManager.GetProgressiveBroadcastDataForDenominationAndPaytable(denom, paytable);
        }

        /// <inheritdoc />
        public void SetLanguage(string newLanguage)
        {
            MustHaveOpenTransaction();

            if(gameContextMode != GameMode.Play)
            {
                string message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, gameContextMode, gameState);
            }

            if(!AvailableLanguages.Contains(newLanguage))
            {
                throw new InvalidOperationException(newLanguage + " is not enabled.");
            }

            if(GameLanguage != newLanguage)
            {
                GameLanguage = newLanguage;
                PostEvent(new LanguageChangedEventArgs(newLanguage));
            }
        }

        /// <inheritdoc />
        public string SetDefaultLanguage()
        {
            // Let's use first available language to be the default language for standalone.
            var language = AvailableLanguages.First();

            SetLanguage(language);
            return language;
        }

        /// <inheritdoc />
        public DateTime GetDateTime()
        {
            MustHaveOpenTransaction();

            return DateTime.Now;
        }

        /// <inheritdoc />
        public CreditMeterDisplayBehaviorMode GetCreditMeterBehavior()
        {
            return PresentationBehaviorConfigs.CreditMeterBehavior;
        }

        #endregion

        #region Game Cycle Functions

        /// <inheritdoc cref="IGameLib"/>
        public GameCycleState QueryGameCycleState()
        {
            MustHaveOpenTransaction();

            return GameContextMode == GameMode.Play ? GameState : GameCycleState.Invalid;
        }

        /// <inheritdoc />
        public bool CanCommitGameCycle()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play &&
               GameState == GameCycleState.Idle &&
               IsPlayerWagerOfferable)
            {
                return !IsEnforced(GamePlayEnforcements.CommitGameCycleFail);
            }

            var message = BuildMessageForWrongStateException();
            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
        }

        /// <inheritdoc />
        public bool CommitGameCycle()
        {
            MustHaveOpenTransaction();

            var result = false;

            if(CanCommitGameCycle())
            {
                // Reseed the RNG with chaos before the game starts.
                random.ReseedWithChaos();

                GotoState(GameCycleState.Committed);
                result = true;
            }

            return result;
        }

        /// <inheritdoc />
        public void UncommitGameCycle()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play && GameState == GameCycleState.Committed)
            {
                if(CommittedBet != InvalidBetAmount)
                {
                    throw new InvalidOperationException(
                        "The committed bet must be uncommitted first before un-committing the game cycle.");
                }

                GotoState(GameCycleState.Idle);
            }
            else
            {
                string message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="CommitBet"/> has not been called
        /// prior to calling this function.
        /// </exception>
        public void EnrollGameCycle(byte[] enrollmentData)
        {
            MustHaveOpenTransaction();

            // Move to pending.
            if(GameContextMode == GameMode.Play && GameState == GameCycleState.Committed)
            {
                if(CommittedBet == InvalidBetAmount)
                {
                    throw new InvalidOperationException(
                        "A bet must be committed first before enrolling the game cycle.");
                }

                GotoState(GameCycleState.EnrollPending);
            }
            else
            {
                var message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }

            // Move to the next state and post the response.
            if(GameContextMode == GameMode.Play && GameState == GameCycleState.EnrollPending)
            {
                var enrollSuccess = !IsEnforced(GamePlayEnforcements.EnrollGameCycleFail);

                GotoState(GameCycleState.EnrollComplete);
                PostEvent(new EnrollResponseEventArgs(enrollSuccess, null));
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="UncommitBet"/> has not been called
        /// prior to calling this function.
        /// </exception>
        public void UnenrollGameCycle()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play && GameState == GameCycleState.EnrollComplete)
            {
                if(CommittedBet != InvalidBetAmount)
                {
                    throw new InvalidOperationException(
                        "The committed bet must be uncommitted first before un-enrolling the game cycle.");
                }

                GotoState(GameCycleState.Idle);
            }
            else
            {
                string message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }
        }

        /// <inheritdoc />
        public bool CanStartPlaying()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play && GameState == GameCycleState.EnrollComplete)
            {
                return StartingBet != InvalidBetAmount;
            }

            string message = BuildMessageForWrongStateException();
            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="PlaceStartingBet"/> has not been called
        /// prior to calling this function.
        /// </exception>
        public bool StartPlaying()
        {
            MustHaveOpenTransaction();

            bool result = false;

            if(CanStartPlaying())
            {
                // Contribute to system controlled progressives.
                // StartingBet is in base units.
                progressiveManager.ContributeToAllProgressives(StartingBet, 1, true);

                GotoState(GameCycleState.Playing);
                result = true;
            }
            else if(StartingBet == InvalidBetAmount)
            {
                throw new InvalidOperationException(
                    "A bet must be placed first before starting play.");
            }

            return result;
        }

        /// <inheritdoc />
        public void EndGameCycle(uint historySteps)
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play && GameState == GameCycleState.Finalized)
            {
                // Get the paytable variant information.
                var paytableVariant = paytableListManager.GetPaytableVariant(ThemeIdentifier, GameDenomination);

                // Archive the history record of this game cycle, and get ready for
                // the next game cycle.
                historyManager.ArchiveHistoryRecord(paytableVariant, GameDenomination);

                GotoState(GameCycleState.Idle);
            }
            else
            {
                var message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }
        }

        /// <inheritdoc />
        public bool OfferAncillaryGame()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play && GameState == GameCycleState.MainPlayComplete)
            {
                return true;
            }

            string message = BuildMessageForWrongStateException();
            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
        }

        /// <inheritdoc />
        public bool StartAncillaryPlaying()
        {
            MustHaveOpenTransaction();

            var result = false;

            if(OfferAncillaryGame())
            {
                GotoState(GameCycleState.AncillaryPlaying);
                result = true;
            }

            return result;
        }

        /// <inheritdoc />
        public bool StartBonusPlaying()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play &&
               (GameState == GameCycleState.MainPlayComplete || GameState == GameCycleState.AncillaryPlayComplete))
            {
                var result = false;

                if(BonusPlayEnabled)
                {
                    GotoState(GameCycleState.BonusPlaying);
                    result = true;
                }

                return result;
            }

            var message = BuildMessageForWrongStateException();
            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
        }


        /// <inheritdoc />
        public void AdjustOutcome(IOutcomeList outcome, bool isFinalOutcome)
        {
            MustHaveOpenTransaction();

            if(outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome), "Parameter may not be null.");
            }

            //For regular play ensure the wager categories have been set before adjusting the final outcome.
            if(GameState == GameCycleState.Playing && isFinalOutcome)
            {
                var wagerCategoryInformation = GameCycleWagerCategoryInfo;
                if(wagerCategoryInformation == null || wagerCategoryInformation.Count == 0)
                {
                    throw new InvalidOperationException(
                        "There must be at least 1 wager category present before adjusting the final game outcome.");
                }
            }

            if(GameContextMode == GameMode.Play)
            {
                switch(GameState)
                {
                    // Move to pending.
                    case GameCycleState.Playing:
                        {
                            GotoState(GameCycleState.EvaluatePending);
                            break;
                        }
                    case GameCycleState.AncillaryPlaying:
                        {
                            GotoState(GameCycleState.AncillaryEvaluatePending);
                            break;
                        }
                    case GameCycleState.BonusPlaying:
                        {
                            GotoState(GameCycleState.BonusEvaluatePending);
                            break;
                        }
                    default:
                        {
                            var message = BuildMessageForWrongStateException();
                            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
                        }
                }

                // Determine the next state.
                var nextState = GameCycleState.Invalid;
                switch(GameState)
                {
                    case GameCycleState.EvaluatePending:
                        {
                            nextState = isFinalOutcome
                                            ? GameCycleState.MainPlayComplete
                                            : GameCycleState.Playing;
                            break;
                        }
                    case GameCycleState.AncillaryEvaluatePending:
                        {
                            nextState = isFinalOutcome
                                            ? GameCycleState.AncillaryPlayComplete
                                            : GameCycleState.AncillaryPlaying;
                            break;
                        }
                    case GameCycleState.BonusEvaluatePending:
                        {
                            nextState = isFinalOutcome
                                            ? GameCycleState.BonusPlayComplete
                                            : GameCycleState.BonusPlaying;
                            break;
                        }
                }

                // Adjust the outcome.
                if(nextState != GameCycleState.Invalid)
                {
                    var returnList = new OutcomeList(outcome);
                    // Adjust the outcome.
                    AdjustOutcomeForProgressive(returnList);

                    // Merge the outcome list to the accumulated one which will get the total win amount so far.
                    var tempFinalOutcomeList = new OutcomeList(returnList);
                    if(FinalOutcomeList != null)
                    {
                        tempFinalOutcomeList.Merge(FinalOutcomeList);
                    }

                    // Apply any registered adjustments.
                    foreach(var adjustment in outcomeAdjustments)
                    {
                        adjustment(tempFinalOutcomeList, returnList, isFinalOutcome);
                    }

                    //The returnList might have an adjustment amount in it.  Add that into the FinalOutcomeList.
                    var tempReturnList = new OutcomeList(returnList);
                    if(FinalOutcomeList != null)
                    {
                        tempReturnList.Merge(FinalOutcomeList);
                    }

                    FinalOutcomeList = tempReturnList;

                    // Reset all progressive levels that have been hit.
                    progressiveManager.ResetProgressiveHits();

                    // Move to the next state.
                    GotoState(nextState);

                    // Post the response.
                    PostEvent(new OutcomeResponseEventArgs(returnList, isFinalOutcome));
                }
            }
            else
            {
                var message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }
        }

        /// <inheritdoc />
        public void FinalizeOutcome()
        {
            MustHaveOpenTransaction();

            // Move to pending.
            if(GameContextMode == GameMode.Play &&
                (GameState == GameCycleState.MainPlayComplete ||
                 GameState == GameCycleState.AncillaryPlayComplete ||
                 GameState == GameCycleState.BonusPlayComplete))
            {
                GotoState(GameCycleState.FinalizeAwardPending);
            }
            else
            {
                var message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }

            // Finalize the outcome.
            if(GameContextMode == GameMode.Play && GameState == GameCycleState.FinalizeAwardPending)
            {
                // Reflect the final outcome on the player meters.
                long totalWin = FinalOutcomeList.GetTotalDisplayableAmount();
                PlayerBankMeter += totalWin;

                // To be consistent with Foundation, send MoneyWon event even if the win amount is 0.
                PostEvent(new MoneyEventArgs(MoneyEventType.MoneyWon,
                                             Utility.ConvertToCredits(totalWin, GameDenomination),
                                             GameDenomination,
                                             new PlayerMeters(PlayerWagerableMeter,
                                                              PlayerBankMeter,
                                                              PlayerPaidMeter)));

                // Move to the next state and post the response.
                GotoState(GameCycleState.Finalized);
                PostEvent(new FinalizeOutcomeEventArgs());
            }
        }

        #endregion

        #region Betting Functions

        /// <inheritdoc />
        public bool CanCommitBet(long bet, long denomination)
        {
            MustHaveOpenTransaction();

            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            if(GameContextMode == GameMode.Play &&
                (GameState == GameCycleState.Idle ||
                 GameState == GameCycleState.Committed) &&
                IsPlayerWagerOfferable)
            {
                return CanCommitBet(bet, denomination, 0);
            }

            string message = BuildMessageForWrongStateException();
            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
        }

        /// <inheritdoc />
        public IEnumerable<bool> CanCommitBets(IEnumerable<long> bets, long denomination)
        {
            MustHaveOpenTransaction();

            if(bets == null)
            {
                throw new ArgumentNullException(nameof(bets), "The list of bets should not be null.");
            }

            var betList = bets.ToList();
            if(!betList.Any())
            {
                throw new ArgumentException("The list of bets should not be empty.", nameof(bets));
            }
            if(betList.Any(bet => bet < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bets), "Bet cannot be less than 0.");
            }
            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            if(GameContextMode == GameMode.Play &&
                (GameState == GameCycleState.Idle ||
                 GameState == GameCycleState.Committed) &&
                IsPlayerWagerOfferable)
            {
                return betList.Select(betAmount => CanCommitBet(betAmount, denomination, 0));
            }

            var message = BuildMessageForWrongStateException();
            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
        }

        /// <inheritdoc />
        public bool CommitBet(long bet, long denomination)
        {
            MustHaveOpenTransaction();

            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            checked
            {
                bool result = false;

                // Double check on the state since CanCommitBet
                // can be called from Idle as well.
                if(GameState == GameCycleState.Committed)
                {
                    if(CanCommitBet(bet, denomination))
                    {
                        var requestedBet = Utility.ConvertToCents(bet, denomination);

                        if(isBankedCreditsEnvironment)
                        {
                            PlayerWagerableMeter -= requestedBet - ValidCommittedBet;
                        }
                        else
                        {
                            PlayerBankMeter -= requestedBet - ValidCommittedBet;
                        }

                        CommittedBet = requestedBet;

                        PostEvent(new MoneyEventArgs(MoneyEventType.MoneyCommittedChanged,
                                                     bet,
                                                     denomination,
                                                     new PlayerMeters(PlayerWagerableMeter,
                                                                      PlayerBankMeter,
                                                                      PlayerPaidMeter)));

                        result = true;
                    }

                    return result;
                }

                string message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }
        }

        /// <inheritdoc />
        public void UncommitBet()
        {
            MustHaveOpenTransaction();

            checked
            {
                if(GameContextMode == GameMode.Play &&
                    (GameState == GameCycleState.Committed ||
                     GameState == GameCycleState.EnrollComplete))
                {
                    if(isBankedCreditsEnvironment)
                    {
                        PlayerWagerableMeter += CommittedBet;
                    }
                    else
                    {
                        PlayerBankMeter += CommittedBet;
                    }

                    CommittedBet = InvalidBetAmount;

                    // For money set event, value and denomination fields are not used.
                    PostEvent(new MoneyEventArgs(MoneyEventType.MoneySet,
                                                 0,
                                                 1,
                                                 new PlayerMeters(PlayerWagerableMeter,
                                                                  PlayerBankMeter,
                                                                  PlayerPaidMeter)));
                }
                else
                {
                    string message = BuildMessageForWrongStateException();
                    throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
                }
            }
        }

        /// <inheritdoc />
        public void GetCommittedBet(out long bet, out long denomination)
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play && (GameState == GameCycleState.Committed ||
                                                    GameState == GameCycleState.EnrollComplete))
            {
                bet = Utility.ConvertToCredits(ValidCommittedBet, GameDenomination);
                denomination = GameDenomination;
            }
            else
            {
                bet = 0;
                denomination = 1;
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when this function has been called more than once.
        /// </exception>
        public void PlaceStartingBet()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play &&
               GameState == GameCycleState.EnrollComplete)
            {
                if(StartingBet != InvalidBetAmount)
                {
                    throw new InvalidOperationException(
                        "Function PlaceStartingBet can only be called once per game cycle.");
                }

                StartingBet = CommittedBet;
                CommittedBet = InvalidBetAmount;
                MidGameBet = 0;
            }
            else
            {
                string message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }
        }

        /// <inheritdoc />
        public bool CanPlaceBet(long bet, long denomination)
        {
            MustHaveOpenTransaction();

            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            checked
            {
                if(GameContextMode == GameMode.Play &&
                   GameState == GameCycleState.Playing &&
                   IsPlayerWagerOfferable)
                {
                    long requestedBet = Utility.ConvertToCents(bet, denomination);

                    long availableCredits = isBankedCreditsEnvironment ?
                                            PlayerWagerableMeter : PlayerBankMeter;

                    long accumulatedBet = StartingBet + MidGameBet + requestedBet;

                    bool result = requestedBet <= availableCredits &&
                                  accumulatedBet <= Utility.ConvertToCents(GameMaxBet, GameDenomination) &&
                                  (accumulatedBet == 0 || accumulatedBet >= GameMinBetAmount);

                    return result;
                }

                string message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }
        }

        /// <inheritdoc />
        public bool CanPlaceBetAgainstPendingWins(long bet, long betFromCredits, long betFromPendingWins,
                                                  long availablePendingWins, long denomination)
        {
            MustHaveOpenTransaction();

            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(betFromCredits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(betFromCredits), "Bet cannot be less than 0.");
            }

            if(betFromPendingWins < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(betFromPendingWins), "Bet cannot be less than 0.");
            }

            if(availablePendingWins < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(availablePendingWins), "Win amount cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            checked
            {
                if(GameContextMode == GameMode.Play &&
                    GameState == GameCycleState.Playing &&
                    IsPlayerWagerOfferable)
                {
                    long requestedBet = Utility.ConvertToCents(bet, denomination);
                    long requestedBetFromCredits = Utility.ConvertToCents(betFromCredits, denomination);

                    long availableCredits = isBankedCreditsEnvironment ?
                                            PlayerWagerableMeter : PlayerBankMeter;

                    long accumulatedBet = StartingBet + MidGameBet + requestedBet;

                    bool result = bet == betFromCredits + betFromPendingWins &&
                                  requestedBetFromCredits <= availableCredits &&
                                  betFromPendingWins <= availablePendingWins &&
                                  accumulatedBet <= Utility.ConvertToCents(GameMaxBet, GameDenomination) &&
                                  (accumulatedBet == 0 || accumulatedBet >= GameMinBetAmount);

                    return result;
                }

                string message = BuildMessageForWrongStateException();
                throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
            }
        }

        /// <inheritdoc />
        public bool PlaceBet(long bet, long denomination)
        {
            MustHaveOpenTransaction();

            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            checked
            {
                bool result = false;

                if(CanPlaceBet(bet, denomination))
                {
                    long requestedBet = Utility.ConvertToCents(bet, denomination);

                    if(isBankedCreditsEnvironment)
                    {
                        PlayerWagerableMeter -= requestedBet;
                    }
                    else
                    {
                        PlayerBankMeter -= requestedBet;
                    }

                    MidGameBet += requestedBet;

                    PostEvent(new MoneyEventArgs(MoneyEventType.MoneyBet,
                                                 bet,
                                                 denomination,
                                                 new PlayerMeters(PlayerWagerableMeter,
                                                                  PlayerBankMeter,
                                                                  PlayerPaidMeter)));

                    result = true;
                }

                return result;
            }
        }

        /// <inheritdoc />
        public bool PlaceBetAgainstPendingWins(long bet, long betFromCredits, long betFromPendingWins,
                                               long availablePendingWins, long denomination)
        {
            MustHaveOpenTransaction();

            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(betFromCredits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(betFromCredits), "Bet cannot be less than 0.");
            }

            if(betFromPendingWins < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(betFromPendingWins), "Bet cannot be less than 0.");
            }

            if(availablePendingWins < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(availablePendingWins), "Win amount cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            checked
            {
                bool result = false;

                if(CanPlaceBetAgainstPendingWins(bet, betFromCredits, betFromPendingWins,
                                                  availablePendingWins, denomination))
                {
                    long requestedBet = Utility.ConvertToCents(bet, denomination);
                    long requestedBetFromCredits = Utility.ConvertToCents(betFromCredits, denomination);

                    if(isBankedCreditsEnvironment)
                    {
                        PlayerWagerableMeter -= requestedBetFromCredits;
                    }
                    else
                    {
                        PlayerBankMeter -= requestedBetFromCredits;
                    }

                    MidGameBet += requestedBet;

                    PostEvent(new MoneyEventArgs(MoneyEventType.MoneyBet,
                                                 bet,
                                                 denomination,
                                                 new PlayerMeters(PlayerWagerableMeter,
                                                                  PlayerBankMeter,
                                                                  PlayerPaidMeter)));

                    result = true;
                }

                return result;
            }
        }

        /// <inheritdoc />
        public IEnumerable<bool> CanBetNextGameCycle(IEnumerable<long> bets, long denomination)
        {
            MustHaveOpenTransaction();

            if(bets == null)
            {
                throw new ArgumentNullException(nameof(bets), "The list of bets should not be null.");
            }

            var betList = bets.ToList();

            if(!betList.Any())
            {
                throw new ArgumentException("The list of bets should not be empty.", nameof(bets));
            }
            if(betList.Any(bet => bet < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bets), "Bet cannot be less than 0.");
            }
            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            if(GameContextMode == GameMode.Play &&
                GameState == GameCycleState.MainPlayComplete &&
                IsPlayerWagerOfferable)
            {
                var pendingWin = FinalOutcomeList.GetTotalDisplayableAmount();
                return betList.Select(betAmount => CanCommitBet(betAmount, denomination, pendingWin));
            }

            string message = BuildMessageForWrongStateException();
            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
        }

        /// <inheritdoc />
        public bool CanBetNextGameCycle(long bet, long denomination)
        {
            MustHaveOpenTransaction();

            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }
            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            if(GameContextMode == GameMode.Play &&
                GameState == GameCycleState.MainPlayComplete &&
                IsPlayerWagerOfferable)
            {
                var pendingWin = FinalOutcomeList.GetTotalDisplayableAmount();
                return CanCommitBet(bet, denomination, pendingWin);
            }

            string message = BuildMessageForWrongStateException();
            throw new FunctionCallNotAllowedInModeOrStateException(message, GameContextMode, GameState);
        }

        #endregion

        #region Money Functions

        /// <inheritdoc />
        public PlayerMeters GetPlayerMeters()
        {
            return new PlayerMeters(PlayerWagerableMeter, PlayerBankMeter, PlayerPaidMeter);
        }

        /// <inheritdoc />
        public BankStatus QueryBankStatus()
        {
            MustHaveOpenTransaction();

            return new BankStatus(IsPlayerWagerOfferable, IsCashOutOfferable, IsBankToWagerableOfferable);
        }

        /// <inheritdoc />
        public void RequestCashOut()
        {
            MustHaveOpenTransaction();

            if(GameContextMode == GameMode.Play && IsCashOutOfferable && IsPlayerWagerOfferable)
            {
                long creditInBase = PlayerBankMeter;

                if(creditInBase != 0)
                {
                    PlayerBankMeter = 0;
                    PlayerPaidMeter = creditInBase;
                    IsCashOutOfferable = false;

                    long credit = Utility.ConvertToCredits(creditInBase, GameDenomination);
                    PostEvent(new MoneyEventArgs(MoneyOutSource.OtherSource,
                                                 credit,
                                                 GameDenomination,
                                                 new PlayerMeters(PlayerWagerableMeter,
                                                                  PlayerBankMeter,
                                                                  PlayerPaidMeter)));
                }
            }
        }

        /// <inheritdoc />
        public bool RequestMoneyMove(MoneyLocation from, MoneyLocation to)
        {
            MustHaveOpenTransaction();

            var result = false;

            // Make money wagerable.
            if(from == MoneyLocation.PlayerBankMeter && to == MoneyLocation.PlayerWagerableMeter)
            {
                if(GameContextMode == GameMode.Play &&
                   (GameState == GameCycleState.Idle ||
                    GameState == GameCycleState.Playing ||
                    GameState == GameCycleState.AncillaryPlaying))
                {
                    result = TransferFromBankToWagerable(BankToWagerableDefaultAmount);
                }
            }
            // Move money to bank, getting ready for cash out.
            else if(from == MoneyLocation.PlayerWagerableMeter && to == MoneyLocation.PlayerBankMeter)
            {
                if(GameContextMode == GameMode.Play && GameState == GameCycleState.Idle)
                {
                    if(IsCashOutOfferable && IsPlayerWagerOfferable)
                    {
                        // The default amount to move is all that is available on
                        // the Player Wagerable Meter.
                        var moveAmount = PlayerWagerableMeter;
                        PlayerBankMeter += moveAmount;
                        PlayerWagerableMeter = 0;

                        PostEvent(new MoneyEventArgs(MoneyEventType.MoneyWagerable,
                                                     Utility.ConvertToCredits(moveAmount, GameDenomination),
                                                     GameDenomination,
                                                     new PlayerMeters(PlayerWagerableMeter,
                                                                      PlayerBankMeter,
                                                                      PlayerPaidMeter),
                                                     from: from,
                                                     to: to));

                        IsCashOutOfferable = PlayerBankMeter > 0 &&
                                              IsPlayerWagerOfferable;

                        IsBankToWagerableOfferable = isBankedCreditsEnvironment &&
                                                     PlayerBankMeter > 0 &&
                                                      IsPlayerWagerOfferable;

                        result = true;
                    }
                }
            }
            // exception - meters passed are not valid for a transfer
            else
            {
                throw new ArgumentException($"Money is not allowed to be transferred from {from} to {to}.");
            }

            return result;
        }

        #endregion

        #region Environment Attribute Functions

        /// <inheritdoc />
        public bool IsEnvironmentTrue(EnvironmentAttribute environmentAttribute)
        {
            return environmentAttributes.Contains(environmentAttribute);
        }

        #endregion

        #region Auto Play

        /// <inheritdoc />
        public bool IsPlayerAutoPlayEnabled
        {
            get
            {
                bool gameSupportsAutoPlay = gameRegistryManager.GetAutoPlaySupported(ThemeIdentifier);

                // If auto play confirmation is required, auto play will be disabled for those games which do not
                // support this feature.
                return gameSupportsAutoPlay && autoPlaySysConfig == AutoPlayConfiguration.PlayerInitiatedAvailable &&
                       (!autoPlayConfirmationRequired ||
                        gameRegistryManager.GetAutoPlayConfirmationSupported(ThemeIdentifier));
            }
        }

        /// <inheritdoc />
        public bool IsAutoPlayConfirmationRequired => gameRegistryManager.GetAutoPlayConfirmationSupported(ThemeIdentifier) && autoPlayConfirmationRequired;

        /// <inheritdoc />
        public bool? IsAutoPlaySpeedIncreaseAllowed => gameRegistryManager.GetAutoPlaySupported(ThemeIdentifier) ? autoPlaySpeedIncreaseAllowed : false;

        /// <inheritdoc />
        bool IGameLib.IsAutoPlayOn()
        {
            return isAutoPlayOn;
        }

        /// <inheritdoc />
        public bool SetAutoPlayOn()
        {
            if(!IsPlayerAutoPlayEnabled)
            {
                return false;
            }
            MustHaveOpenTransaction();
            isAutoPlayOn = true;
            return isAutoPlayOn;
        }

        /// <inheritdoc />
        public void SetAutoPlayOff()
        {
            MustHaveOpenTransaction();
            isAutoPlayOn = false;
        }

        #endregion

        #region Game Stop Reporting

        /// <inheritdoc/>
        public void ReportReelStops(ICollection<uint> physicalReelStops)
        {
            if(physicalReelStops == null)
            {
                throw new ArgumentNullException(nameof(physicalReelStops), "Parameter may not be null.");
            }
            if(physicalReelStops.Count == 0)
            {
                throw new ArgumentException("Physical stop list cannot be empty when reporting stops.",
                                            nameof(physicalReelStops));
            }

            MustHaveOpenTransaction();
        }

        #endregion

        #region Extended Interface Handling

        /// <inheritdoc/>
        public TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class
        {
            if(interfaceExtensions.ContainsKey(typeof(TExtendedInterface)))
            {
                return interfaceExtensions[typeof(TExtendedInterface)] as TExtendedInterface;
            }
            return null;
        }

        #endregion

        #endregion

        #region IGameLibRestricted Implementation

        /// <inheritdoc />
        public bool ConnectToFoundation()
        {
            return ConnectToFoundation(null);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Initialize the fields that can not use their OS default values,
        /// such as Game Denomination, and Game Language.
        /// </remarks>
        public bool ConnectToFoundation(IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaceConfigurations)
        {
            if(additionalInterfaceConfigurations != null)
            {
                var baseDependencies = new InterfaceExtensionDependencies
                {
                    TransactionWeightVerification = this,
                    CriticalDataProvider = this,
                    LayeredContextActivationEvents = this,
                    CultureInfoProvider = this,
                    GameModeQuery = this,
                    TransactionalEventDispatcher = this,
                    NonTransactionalEventDispatcher = this
                };

                var dependencyObject = new StandaloneInterfaceExtensionDependencies(baseDependencies)
                                           {
                                               GameInformation = this,
                                               FoundationCriticalDataProvider = this,
                                               GameCycleStateQuery = this,
                                               EventPoster = this,
                                               OutcomeAdjuster = this,
                                               RegistryInformation = gameRegistryManager,
                                               ProgressiveManager = progressiveManager,
                                               PlayStatus = this,
                                           };

                foreach(var additionalInterfaceConfiguration in additionalInterfaceConfigurations)
                {
                    var extendedInterface = additionalInterfaceConfiguration.CreateInterfaceExtension(dependencyObject);
                    if(extendedInterface != null)
                    {
                        interfaceExtensions[additionalInterfaceConfiguration.InterfaceType] = extendedInterface;
                    }
                }
            }

            CreateFoundationTransaction();
            if(isColdStart)
            {
                // Initialize Theme Name.
                var defaultPaytable = paytableListManager.DefaultPaytableConfiguration;

                PlayModeTheme = defaultPaytable.PaytableVariant.ThemeIdentifier;

                // Initialize Game Denomination.
                // If the paytable list is available, use the default denomination defined;
                // Otherwise, use a hard coded value.
                var defaultDenomination = paytableListManager.DefaultDenomination;

                PlayModeGameDenomination = defaultDenomination > 0 ? defaultDenomination : HardCodedDenomination;

                // Initialize Game Language.
                // If the configuration of supported cultures is available, use the first one in the list;
                // Otherwise, use a hard coded value.
                var languageList = gameRegistryManager.GetSupportedCultures(PlayModeTheme);

                if(languageList?.Count >= 1)
                {
                    AvailableLanguages = languageList;
                    GameLanguage = languageList.First();
                }
                else
                {
                    AvailableLanguages = new List<string> { HardCodedLanguage };
                    GameLanguage = HardCodedLanguage;
                }
            }
            else
            {
                EnterState(GameState);
            }
            CloseFoundationTransaction();


            // Post theme selection event and theme context events.
            // In case of warm start, use the theme name and game denomination
            // restored from the safe store.
            // Game always starts in Play mode.
            StartThemeContext(true,
                              GameMode.Play,
                              paytableListManager.GetPaytableVariant(PlayModeTheme, PlayModeGameDenomination),
                              PlayModeGameDenomination);

            if(restoredFoundationEvents != null)
            {
                //Post previously stored events after the context has been activated.
                foundationEvents.AddRange(restoredFoundationEvents);
                restoredFoundationEvents = null;
            }

            return true;
        }

        /// <inheritdoc />
        public bool DisconnectFromFoundation()
        {
            return true;
        }

        /// <inheritdoc />
        public ErrorCode CreateTransaction()
        {
            ErrorCode result;
            bool eventWaiting;

            lock(eventLocker)
            {
                eventWaiting = foundationEvents.Count > 0;
            }

            if(eventWaiting)
            {
                result = ErrorCode.EventWaitingForProcess;
            }
            else if(TransactionOpen)
            {
                result = ErrorCode.OpenTransactionExisted;
            }
            else
            {
                TransactionOpen = true;
                transactionId++;

                result = ErrorCode.NoError;
            }

            return result;
        }

        /// <inheritdoc />
        public ErrorCode CreateTransaction(string name)
        {
            return CreateTransaction();
        }

        /// <inheritdoc />
        public ErrorCode CloseTransaction()
        {
            ErrorCode result;

            if(TransactionOpen)
            {
                DiskStoreManager.Commit();

                RaiseEvent(TransactionClosingEvent, EventArgs.Empty);

                TransactionOpen = false;

                result = ErrorCode.NoError;
            }
            else
            {
                result = ErrorCode.NoTransactionOpen;
            }

            return result;
        }

        /// <inheritdoc/>
        void IGameLibRestricted.SetPrepickedValueProvider(IPrepickedValueProvider providerToUse)
        {
            SetPrepickedValueProvider(providerToUse);
        }

        /// <summary>
        /// Sets the <see cref="IPrepickedValueProvider"/> to use.
        /// </summary>
        /// <param name="providerToUse">The provider to use.</param>
        private void SetPrepickedValueProvider(IPrepickedValueProvider providerToUse)
        {
            prepickedValueProvider = providerToUse;
        }

        /// <inheritdoc />
        public string Token { get; }

        /// <inheritdoc />
        public void ProcessEvents(int timeout)
        {
            ProcessEvents(timeout, null);
        }

        /// <inheritdoc />
        public void ProcessEvents()
        {
            ProcessEvents(Timeout.Infinite);
        }

        /// <inheritdoc />
        public WaitHandle ProcessEvents(WaitHandle[] waitHandles)
        {
            return ProcessEvents(Timeout.Infinite, waitHandles);
        }

        /// <inheritdoc />
        public WaitHandle ProcessEvents(int timeout, WaitHandle[] waitHandles)
        {
            return eventCoordinator.ProcessEvents(timeout, waitHandles);
        }

        /// <summary>
        /// This flag indicates if a game initiated transaction is currently open.
        /// Most operations can only be performed while a transaction is open,
        /// either initiated by the game or the Foundation.
        /// When a transaction is not open then an InvalidTransactionException
        /// exception should be thrown.
        /// </summary>
        public bool TransactionOpen { get; private set; }

        /// <inheritdoc/>
        public TServiceInterface GetServiceInterface<TServiceInterface>() where TServiceInterface : class
        {
            return builtinServices.ContainsKey(typeof(TServiceInterface))
                       ? builtinServices[typeof(TServiceInterface)] as TServiceInterface
                       : null;
        }

        #endregion

        #region IEventSource Members

        /// <inheritdoc />
        public WaitHandle EventPosted => eventResetEvent;

        /// <inheritdoc />
        /// <devdoc>
        /// Explicit interface implementation in order to distinguish this one
        /// from the one in IGameLibRestricted.
        /// </devdoc>
        void IEventSource.ProcessEvents()
        {
            // Process any non transactional events first
            ProcessNonTransactionalEvents();

            // Temp fix to go with event coordinator implementation.
            // TODO: Convert to use a separate Event Source to handle these events.
            bool nonTransactionalEventPending;
            lock(nonTransactionalEventLocker)
            {
                nonTransactionalEventPending = nonTransactionalFoundationEvents.Count > 0;
            }

            // If there is a game initiated transaction open, there would be
            // no event in the queue to be processed.
            if(TransactionOpen == false)
            {
                // Simulate a Foundation initiated event.
                CreateFoundationTransaction();

                // Process any event that is already in the queue.
                // There could be events left in the queue because of pace control,
                // so only reset the wait handle when the queue is empty.
                lock(eventLocker)
                {
                    ExecuteEvents(EventProcessPace);

                    if(foundationEvents.Count == 0 && !nonTransactionalEventPending)
                    {
                        eventResetEvent.Reset();
                    }

                    RaiseEvent(TransactionClosingEvent, EventArgs.Empty);
                }

                // Close the Foundation initiated event.
                CloseFoundationTransaction();

                // Cycle the RNG Sever a random number of times.
                random.Cycle();
            }
        }

        /// <summary>
        /// Standalone GameLib specific function for posting events.
        /// The function is thread safe and events may be posted from
        /// another thread. If the processEvents function is currently
        /// waiting for an event, then this function will notify it of
        /// a change in events.
        /// </summary>
        /// <param name="gameLibEvent">The event to post.</param>
        internal void PostEvent(EventArgs gameLibEvent)
        {
            lock(eventLocker)
            {
                foundationEvents.Add(gameLibEvent);
                UpdatePersistedEvents();
                eventResetEvent.Set();
            }
        }

        /// <summary>
        /// Distribute any pending events.
        /// </summary>
        /// <param name="pace">Number of events to be processed per function call.</param>
        private void ExecuteEvents(int pace)
        {
            lock(eventLocker)
            {
                if(foundationEvents.Count > 0 && pace > 0)
                {
                    // Get the desired number of events to process.
                    var eventList = foundationEvents.GetRange(0, pace);

                    foreach(var foundationEvent in eventList)
                    {
                        var dispatchedEventArgs = new EventDispatchedEventArgs(foundationEvent);

                        if(eventTable.ContainsKey(dispatchedEventArgs.DispatchedEventType))
                        {
                            var handler = eventTable[dispatchedEventArgs.DispatchedEventType];
                            if(handler != null)
                            {
                                handler(this, dispatchedEventArgs.DispatchedEvent);

                                dispatchedEventArgs.IsHandled = true;
                            }
                        }

                        var dispatchHandler = EventDispatchedEvent;
                        dispatchHandler?.Invoke(this, dispatchedEventArgs);
                    }

                    // Remove the processed events from the event queue.
                    foundationEvents.RemoveRange(0, pace);
                    UpdatePersistedEvents();
                }
            }
        }

        #endregion

        #region IGameLibDemo Implementation

        // IGameLibDemo is being used on presentation thread, so theoretically all implementation of
        // this interface should be thread safe. The requirement can be relaxed though, depending on
        // the usage, potential risk, and read/write implementation. APIs should be examined case by case.
        // One of the effective ways to separate logic thread from presentation thread is posting events.

        #region ISimulateGameModeControl Implementation

        #region Game Mode Changing

        /// <inheritdoc />
        public void EnterMode(GameMode nextMode)
        {
            if(nextMode == GameMode.Invalid)
            {
                ExitMode();
            }
            else if(nextMode != GameContextMode)
            {
                // Finish the current mode.
                CloseMode();

                // Check if the next mode can be entered.
                var canEnter = OpenMode(nextMode, out var nextPaytableVariant, out var nextDenomination);

                if(canEnter)
                {
                    var isNewTheme = nextPaytableVariant.ThemeIdentifier != ThemeIdentifier;
                    StartThemeContext(isNewTheme, nextMode, nextPaytableVariant, nextDenomination);
                }
                else
                {
                    InactivateCurrentThemeContext();
                }
            }
        }

        /// <summary>
        /// Check if a game mode can be entered, and do preparation work
        /// if the result is yes.
        /// </summary>
        /// <param name="nextMode">The game mode to enter.</param>
        /// <param name="nextPaytableVariant">Return the paytable variant to use for the new mode.</param>
        /// <param name="nextDenomination">Return the denomination to use for the new mode.</param>
        /// <returns>
        /// True if the new mode can be entered.  False otherwise.
        /// </returns>
        private bool OpenMode(GameMode nextMode, out PaytableVariant nextPaytableVariant, out long nextDenomination)
        {
            var result = false;

            nextPaytableVariant = new PaytableVariant();
            nextDenomination = 0;

            switch(nextMode)
            {
                case GameMode.History:
                    if(GameContextMode != GameMode.Play ||
                       GameContextMode == GameMode.Play && GameState == GameCycleState.Idle)
                    {
                        var historyRecord = historyManager.EnterHistory();

                        nextPaytableVariant = historyRecord.PaytableVariant;
                        nextDenomination = historyRecord.Denomination;

                        result = nextDenomination != 0;
                    }
                    break;

                case GameMode.Utility:
                    if((GameContextMode != GameMode.Play ||
                        GameContextMode == GameMode.Play && GameState == GameCycleState.Idle) &&
                       UtilitySelectionComplete)
                    {
                        nextPaytableVariant = new PaytableVariant(UtilityTheme,
                                                                  UtilityPaytable.Key,
                                                                  UtilityPaytable.Value);

                        nextDenomination = UtilityDenomination;

                        result = true;
                    }
                    break;

                case GameMode.Play:
                {
                    nextPaytableVariant = paytableListManager.GetPaytableVariant(PlayModeTheme, PlayModeGameDenomination);
                    nextDenomination = PlayModeGameDenomination;

                    result = true;
                }
                break;

                default:
                    throw new InvalidOperationException(
                        $"Game mode {nextMode} is not supported.");
            }

            return result;
        }

        /// <inheritdoc />
        public void ExitMode()
        {
            if(GameContextMode != GameMode.Invalid)
            {
                CloseMode();

                InactivateCurrentThemeContext();
            }
        }

        /// <summary>
        /// Do the ending work for the current game context mode,
        /// get ready for exit.
        /// </summary>
        private void CloseMode()
        {
            switch(GameContextMode)
            {
                case GameMode.History:
                    historyManager.ExitHistory();
                    break;

                case GameMode.Utility:
                    UtilitySelectionComplete = false;
                    break;
            }
        }

        /// <inheritdoc />
        public void ShutDown()
        {
            // Post shut down event.
            PostEvent(new ShutDownEventArgs());
        }

        #endregion

        #region History Mode Support

        /// <inheritdoc />
        public int GetHistoryRecordCount()
        {
            return historyManager.GetHistoryRecordCount();
        }

        /// <inheritdoc />
        public bool IsNextAvailable()
        {
            return GameContextMode == GameMode.History &&
                   historyManager.IsNextAvailable();
        }

        /// <inheritdoc />
        public bool IsPreviousAvailable()
        {
            return GameContextMode == GameMode.History &&
                   historyManager.IsPreviousAvailable();
        }

        /// <inheritdoc />
        public void NextHistoryRecord()
        {
            if(IsNextAvailable())
            {
                var nextRecord = historyManager.NextHistoryRecord(out var isNewTheme);

                DisplayHistoryRecord(isNewTheme, nextRecord);
            }
        }

        /// <inheritdoc />
        public void PreviousHistoryRecord()
        {
            if(IsPreviousAvailable())
            {
                var prevRecord = historyManager.PreviousHistoryRecord(out var isNewTheme);

                DisplayHistoryRecord(isNewTheme, prevRecord);
            }
        }

        /// <summary>
        /// Start the display of a game cycle in history by posting necessary theme events.
        /// </summary>
        /// <param name="isNewTheme">The flag indicating whether it is a new theme.</param>
        /// <param name="historyRecord">The information on the game cycle history to display.</param>
        private void DisplayHistoryRecord(bool isNewTheme, HistoryRecord historyRecord)
        {
            // Always inactivate the current theme context first.
            InactivateCurrentThemeContext();

            StartThemeContext(isNewTheme, GameMode.History, historyRecord.PaytableVariant, historyRecord.Denomination);
        }

        #endregion

        #region Utility Mode Support

        /// <inheritdoc />
        public bool IsUtilityModeEnabled => GetRegistrySupportedThemes().Count > 0;

        /// <inheritdoc />
        public IReadOnlyList<string> GetRegistrySupportedThemes()
        {
            return gameRegistryManager.GetThemeList()?.Where(theme => gameRegistryManager.GetUtilityModeSupported(theme)).ToList();
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<KeyValuePair<string, string>, IEnumerable<long>> GetRegistrySupportedDenominations(string theme)
        {
            Dictionary<KeyValuePair<string, string>, IEnumerable<long>> result = null;

            var supportedDenominations = gameRegistryManager.GetSupportedDenominations(theme);
            if(supportedDenominations != null)
            {
                result = supportedDenominations.ToDictionary(
                    x => new KeyValuePair<string, string>(x.Key.PaytableName, x.Key.PaytableFileName),
                    y => y.Value);
            }

            return result;
        }

        /// <inheritdoc />
        public bool UtilitySelectionComplete { private get; set; }

        /// <inheritdoc />
        public string UtilityTheme { private get; set; }

        /// <inheritdoc />
        public KeyValuePair<string, string> UtilityPaytable { private get; set; }

        /// <inheritdoc />
        public long UtilityDenomination { private get; set; }

        #endregion

        #endregion

        /// <inheritdoc />
        /// <devdoc>Explicitly implemented to avoid conflicting with IGameLibShow.</devdoc>
        void IGameLibDemo.InsertMoney(long value, long denomination)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            if(value > 0 &&
               GameContextMode == GameMode.Play &&
               (GameState == GameCycleState.Idle ||
                GameState == GameCycleState.Playing ||
                GameState == GameCycleState.AncillaryPlaying))
            {
                AddMoney(value, denomination);

                // Transfer default amount from Bank to Wagerable meter
                // for the convenience of game development.
                if(isBankedCreditsEnvironment)
                {
                    CreateFoundationTransaction();

                    RequestMoneyMove(MoneyLocation.PlayerBankMeter, MoneyLocation.PlayerWagerableMeter);

                    CloseFoundationTransaction();
                }
            }
        }

        /// <inheritdoc />
        public void SetShowMode(bool showMode, ShowEnvironment environment = ShowEnvironment.Development)
        {
            // Ignore the thread safety as the method is unlikely to be called outside logic thread.
            ShowMode = showMode;
            showEnvironment = showMode ? environment : ShowEnvironment.Invalid;
        }

        /// <inheritdoc />
        public void SetDisplayControlHidden()
        {
            PostEvent(new DisplayControlEventArgs(DisplayControlState.DisplayAsHidden));
        }

        /// <inheritdoc />
        public void SetDisplayControlNormal()
        {
            PostEvent(new DisplayControlEventArgs(DisplayControlState.DisplayAsNormal));
        }

        /// <inheritdoc />
        public void SetDisplayControlSuspended()
        {
            PostEvent(new DisplayControlEventArgs(DisplayControlState.DisplayAsSuspended));
        }

        /// <inheritdoc />
        public void SetAutoCredits(long credits, long threshold)
        {
            if(credits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(credits), "Parameter cannot be negative.");
            }
            if(threshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold), "Parameter cannot be negative.");
            }

            creditsToAdd = credits;
            creditThreshold = threshold;

            CreateFoundationTransaction();

            AutoAddCredits();

            CloseFoundationTransaction();
        }

        /// <inheritdoc />
        public IDictionary<string, IList<long>> GetAvailableThemes()
        {
            var result = new Dictionary<string, IList<long>>();

            var availableThemes = paytableListManager.GetAvailableThemes();

            foreach(var theme in availableThemes)
            {
                result[theme] = paytableListManager.GetAvailableDenominations(theme) ?? new List<long> { HardCodedDenomination };
            }

            return result;
        }

        /// <inheritdoc />
        public void RequestThemeChange(string newTheme, long newDenomination)
        {
            if(GameContextMode != GameMode.Invalid)
            {
                throw new InvalidOperationException(
                    "Theme change can only be made when theme context is inactive.");
            }

            if(!paytableListManager.IsThemeEnabled(newTheme))
            {
                throw new InvalidOperationException(
                    $"Theme \"{newTheme}\" is not enabled.");
            }

            if(!paytableListManager.IsDenominationEnabled(newTheme, newDenomination))
            {
                throw new InvalidOperationException(
                    $"Denomination {newDenomination} is not enabled for theme \"{newTheme}\".");
            }

            // Check if there is a theme change.
            var isNewTheme = newTheme != PlayModeTheme;

            // Update the theme name and denomination.  If there is no change, re-assignment doesn't hurt.
            PlayModeTheme = newTheme;
            PlayModeGameDenomination = newDenomination;

            var paytableVariant = paytableListManager.GetPaytableVariant(PlayModeTheme, PlayModeGameDenomination);

            if(isNewTheme)
            {
                ThemeIdentifier = PlayModeTheme;
                StartNewTheme(PlayModeTheme);
            }

            ActivateThemeContext(GameMode.Play, paytableVariant, PlayModeGameDenomination, true);
        }

        /// <inheritdoc />
        public void DisableAncillaryOffer()
        {
            if(GameContextMode == GameMode.Play && GameState == GameCycleState.MainPlayComplete)
            {
                PostEvent(new DisableAncillaryGameOfferEventArgs());
            }
        }

        /// <inheritdoc/>
        public void TriggerForceGameCompletion(DateTime warningTime, DateTime finishTime,
                                               Dictionary<string, string> messages)
        {
            PostEvent(new ForceGameCompletionChangedEventArgs(
                          new ForceGameCompletionStatus(true, warningTime, finishTime, messages)));
        }

        /// <inheritdoc/>
        public void ClearForceGameCompletion()
        {
            PostEvent(new ForceGameCompletionChangedEventArgs(new ForceGameCompletionStatus()));
        }

        /// <inheritdoc />
        public bool IsHostAutoPlayEnabled()
        {
            return  autoPlaySysConfig == AutoPlayConfiguration.HostInitiatedAvailable && GameSupportsAutoPlay();
        }

        /// <inheritdoc />
        bool IGameLibDemo.IsAutoPlayOn()
        {
            return isAutoPlayOn;
        }

        /// <inheritdoc/>
        public void RequestHostAutoPLayOn()
        {
            if(isAutoPlayOn || !IsHostAutoPlayEnabled())
            {
                return;
            }
            isAutoPlayOn = true; 
            PostEvent(new AutoPlayOnRequestEventArgs());
        }

        /// <inheritdoc/>
        public void RequestHostAutoPLayOff()
        {
            if(!isAutoPlayOn || !IsHostAutoPlayEnabled())
            {
                return;
            }
            isAutoPlayOn = false;
            PostEvent(new AutoPlayOffEventArgs());
        }

        /// <inheritdoc />
        public void EnforceDenominationChangeFail(bool enforce)
        {
            Enforce(GamePlayEnforcements.DenominationChangeFail, enforce);
        }

        /// <inheritdoc />
        public bool IsDenominationChangeFailEnforced()
        {
            return IsEnforced(GamePlayEnforcements.DenominationChangeFail);
        }

        /// <inheritdoc />
        public void EnforceCommitGameCycleFail(bool enforce)
        {
            Enforce(GamePlayEnforcements.CommitGameCycleFail, enforce);
        }

        /// <inheritdoc />
        public bool IsCommitGameCycleFailEnforced()
        {
            return IsEnforced(GamePlayEnforcements.CommitGameCycleFail);
        }

        /// <inheritdoc />
        public void EnforceEnrollGameCycleFail(bool enforce)
        {
            Enforce(GamePlayEnforcements.EnrollGameCycleFail, enforce);
        }

        /// <inheritdoc />
        public bool IsEnrollGameCycleFailEnforced()
        {
            return IsEnforced(GamePlayEnforcements.EnrollGameCycleFail);
        }

        #endregion

        #region IGameLibShow Implementation

        /// <inheritdoc />
        public void InsertMoney(long value, long denomination)
        {
            if(!ShowMode)
            {
                throw new ShowFunctionException(MethodBase.GetCurrentMethod()?.Name);
            }

            (this as IGameLibDemo).InsertMoney(value, denomination);
        }

        /// <inheritdoc />
        public ShowEnvironment GetShowEnvironment()
        {
            return showEnvironment;
        }

        #endregion

        #region IGameLibRestrictedInfo Implementation

        /// <inheritdoc />
        public event EventHandler<FoundationStateChangedEventArgs> FoundationStateChangedEvent;

        /// <inheritdoc />
        public event EventHandler<PlayerBankMeterChangedEventArgs> PlayerBankMeterChangedEvent;

        #endregion

        #region ITransactionVerification Implementation

        /// <inheritdoc/>
        void ITransactionVerification.MustHaveOpenTransaction()
        {
            MustHaveOpenTransaction();
        }

        #endregion

        #region IEventDispatcher Implementation

        /// <inheritdoc/>
        public event EventHandler<EventDispatchedEventArgs> EventDispatchedEvent;

        #endregion

        #region ITransactionWeightVerificationDependency Implementation

        /// <inheritdoc />
        void ITransactionWeightVerificationDependency.MustHaveOpenTransaction()
        {
            MustHaveOpenTransaction();
        }

        /// <inheritdoc />
        void ITransactionWeightVerificationDependency.MustHaveHeavyweightTransaction()
        {
            // So far we don't have lightweight transactions on the F2L connection yet.
            MustHaveOpenTransaction();
        }

        #endregion

        #region ILayeredContextActivationEventsDependency Implementation

        private static readonly object ActivateLayeredContextEventFieldLocker = new object();
        private event EventHandler<LayeredContextActivationEventArgs> ActivateLayeredContextEventField;

        /// <inheritdoc />
        event EventHandler<LayeredContextActivationEventArgs> ILayeredContextActivationEventsDependency.ActivateLayeredContextEvent
        {
            add
            {
                lock(ActivateLayeredContextEventFieldLocker)
                {
                    ActivateLayeredContextEventField += value;
                }
            }

            remove
            {
                lock(ActivateLayeredContextEventFieldLocker)
                {
                    ActivateLayeredContextEventField -= value;
                }
            }
        }

        private static readonly object InactivateLayeredContextEventFieldLocker = new object();
        private event EventHandler<LayeredContextActivationEventArgs> InactivateLayeredContextEventField;

        /// <inheritdoc />
        event EventHandler<LayeredContextActivationEventArgs> ILayeredContextActivationEventsDependency.InactivateLayeredContextEvent
        {
            add
            {
                lock(InactivateLayeredContextEventFieldLocker)
                {
                    InactivateLayeredContextEventField += value;
                }
            }

            remove
            {
                lock(InactivateLayeredContextEventFieldLocker)
                {
                    InactivateLayeredContextEventField -= value;
                }
            }
        }

        #endregion

        #region ICriticalDataDependency Implementation

        private static readonly Dictionary<InterfaceExtensionDataScope, CriticalDataScope> MapToCriticalDataScopes =
            new Dictionary<InterfaceExtensionDataScope, CriticalDataScope>
                {
                    { InterfaceExtensionDataScope.GameCycle, CriticalDataScope.GameCycle },
                    { InterfaceExtensionDataScope.Payvar, CriticalDataScope.Payvar },
                    { InterfaceExtensionDataScope.Theme, CriticalDataScope.Theme },
                };

        /// <inheritdoc/>
        void ICriticalDataDependency.WriteCriticalData<TData>(InterfaceExtensionDataScope scope, string path, TData data)
        {
            WriteCriticalData(MapToCriticalDataScopes[scope], path, data);
        }

        /// <inheritdoc/>
        TData ICriticalDataDependency.ReadCriticalData<TData>(InterfaceExtensionDataScope scope, string path)
        {
            return ReadCriticalData<TData>(MapToCriticalDataScopes[scope], path);
        }

        #endregion

        #region ICultureInfoDependency Implementation

        /// <inheritdoc/>
        ICollection<string> ICultureInfoDependency.AvailableCultures => AvailableLanguages;

        #endregion

        #region IGameModeQuery Implementation

        /// <inheritdoc />
        GameMode IGameModeQuery.GameMode => GameContextMode;

        #endregion

        #region IStandaloneGameInformationDependency Implementation

        /// <inheritdoc/>
        string IStandaloneGameInformationDependency.GetG2SThemeId()
        {
            return gameRegistryManager.GetG2SThemeId(PlayModeTheme);
        }

        /// <inheritdoc/>
        GameMode IStandaloneGameInformationDependency.GameContextMode => GameContextMode;

        /// <inheritdoc/>
        bool IStandaloneGameInformationDependency.IsTournamentMode => GameSubMode == GameSubMode.Tournament;

        /// <inheritdoc/>
        TournamentSessionType IStandaloneGameInformationDependency.TournamentSessionType => tournamentSessionConfigParser.SessionType;

        /// <inheritdoc/>
        long IStandaloneGameInformationDependency.MaxBet => GameMaxBet;

        /// <inheritdoc/>
        int IStandaloneGameInformationDependency.TournamentCountdownDuration => tournamentSessionConfigParser.CountdownDuration;

        /// <inheritdoc/>
        int IStandaloneGameInformationDependency.TournamentPlayDuration => tournamentSessionConfigParser.PlayDuration;

        /// <inheritdoc/>
        long IStandaloneGameInformationDependency.InitialCredits => tournamentSessionConfigParser.InitialCredits;

        /// <inheritdoc/>
        bool IStandaloneGameInformationDependency.IsBankCreditEnvironment => isBankedCreditsEnvironment;

        /// <inheritdoc/>
        string IStandaloneGameInformationDependency.StompBrokerHostname => stompBrokerConfigParser.Hostname;

        /// <inheritdoc/>
        int IStandaloneGameInformationDependency.StompBrokerPort => stompBrokerConfigParser.Port;

        /// <inheritdoc/>
        Version IStandaloneGameInformationDependency.StompVersion => stompBrokerConfigParser.StompVersion;

        /// <inheritdoc/>
        ReadOnlyCollection<PayvarInformation> IStandaloneGameInformationDependency.GetEnabledPayvars(IEnumerable<long> denominations)
        {
            var denominationList = denominations == null
                ? throw new ArgumentNullException(nameof(denominations))
                : denominations.ToList();
            
            var payvarInfoList = new List<PayvarInformation>();
            var enabledDenoms = paytableListManager.GetAvailableDenominations(ThemeIdentifier);
            foreach(var denom in enabledDenoms.Intersect(denominationList))
            {
                var payvar = paytableListManager.GetPaytableVariant(ThemeIdentifier, denom);
                var maxBetCredits = gameRegistryManager.GetMaxBet(payvar, denom);
                var maxBet = Utility.ConvertToCents(maxBetCredits, denom);
                var buttonPanelMinBetCredits = gameRegistryManager.GetButtonPanelMinBet(payvar, denom);
                var buttonPanelMinBet = Utility.ConvertToCents(buttonPanelMinBetCredits, denom);
                payvarInfoList.Add(new PayvarInformation(denom, payvar.PaytableFileName,
                    payvar.PaytableName, maxBet, GameMinBetAmount, buttonPanelMinBet));
            }

            return new ReadOnlyCollection<PayvarInformation>(payvarInfoList);
        }

        #endregion

        #region IStandaloneCriticalDataDependency Implementation

        /// <summary>
        /// Map for converting <see cref="InterfaceExtensionDataScope"/> to <see cref="FoundationDataScope"/>.
        /// This is to make sure that when certain scope of Foundation data is cleared,
        /// the interface extension data is correctly handled as well.
        /// </summary>
        private static readonly Dictionary<InterfaceExtensionDataScope, FoundationDataScope> MapToFoundationDataScopes =
            new Dictionary<InterfaceExtensionDataScope, FoundationDataScope>
                {
                    { InterfaceExtensionDataScope.GameCycle, FoundationDataScope.GameCycle },
                    { InterfaceExtensionDataScope.Payvar, FoundationDataScope.PayVar },
                    { InterfaceExtensionDataScope.Theme, FoundationDataScope.Theme },
                };

        /// <inheritdoc/>
        void IStandaloneCriticalDataDependency.WriteFoundationData(InterfaceExtensionDataScope scope, string path, object data)
        {
            var prefixedPath = InterfaceExtensionPrefix + path;
            Utility.ValidateCriticalDataName(prefixedPath);

            var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(MapToFoundationDataScopes[scope]);
            DiskStoreManager.Write(section, index, prefixedPath, data);
        }

        /// <inheritdoc/>
        TData IStandaloneCriticalDataDependency.ReadFoundationData<TData>(InterfaceExtensionDataScope scope, string path)
        {
            var prefixedPath = InterfaceExtensionPrefix + path;
            Utility.ValidateCriticalDataName(prefixedPath);

            var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(MapToFoundationDataScopes[scope]);
            return DiskStoreManager.Read<TData>(section, index, prefixedPath);
        }

        #endregion

        #region IStandaloneEventPosterDependency Implementation

        /// <inheritdoc />
        void IStandaloneEventPosterDependency.PostTransactionalEvent(EventArgs eventArgs)
        {
            PostEvent(eventArgs);
        }

        /// <inheritdoc />
        void IStandaloneEventPosterDependency.PostNonTransactionalEvent(PlatformEventArgs platformEventArgs)
        {
            PostNonTransactionalEvent(platformEventArgs);
        }

        #endregion

        #region IStandaloneOutcomeAdjusterDependency Implementation

        private readonly HashSet<Action<OutcomeList, OutcomeList, bool>> outcomeAdjustments = new HashSet<Action<OutcomeList, OutcomeList, bool>>();

        /// <inheritdoc/>
        public void RegisterAdjustment(Action<OutcomeList, OutcomeList, bool> outcomeAdjustment)
        {
            if(outcomeAdjustment == null)
            {
                throw new ArgumentNullException(nameof(outcomeAdjustment));
            }
            outcomeAdjustments.Add(outcomeAdjustment);
        }

        /// <inheritdoc/>
        public void UnregisterAdjustment(Action<OutcomeList, OutcomeList, bool> outcomeAdjustment)
        {
            if(outcomeAdjustment == null)
            {
                throw new ArgumentNullException(nameof(outcomeAdjustment));
            }
            outcomeAdjustments.Remove(outcomeAdjustment);
        }

        #endregion

        #region Implementation of IStandalonePlayStatusDependency

        /// <inheritdoc />
        public event EventHandler<GameInProgressStatusEventArgs> GameInProgressStatusEvent;

        /// <inheritdoc />
        public event EventHandler<MoneyOnMachineStatusEventArgs> MoneyOnMachineStatusEvent;

        /// <inheritdoc />
        public bool IsGameInProgress()
        {
            return GameState != GameCycleState.Idle;
        }

        /// <inheritdoc />
        public bool IsMoneyOnMachine()
        {
            return PlayerBankMeter != 0;
        }

        #endregion

        #region IGameCycleStateQuery Implementation

        GameCycleState IGameCycleStateQuery.GameCycleState => QueryGameCycleState();

        #endregion

        #region ITransactionAugmenter Members

        /// <inheritdoc />
        public event EventHandler TransactionClosingEvent;

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Implement IDisposable. Do not make this method
        /// virtual.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This function can execute by being called from the Dispose
        /// method of the IDisposable interface or from the finalizer.
        /// If called from Dispose, then managed and unmanaged resources
        /// may be released. If called from a finalizer, then only
        /// unmanaged resources may be released.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    var disposableEventResetEvent = eventResetEvent as IDisposable;
                    disposableEventResetEvent?.Dispose();

                    eventCoordinator.UnregisterEventSource(progressiveBroadcastManager);

                    // Dispose the progressive broadcast manager.
                    // ReSharper disable once InconsistentlySynchronizedField
                    progressiveBroadcastManager.Dispose();

                    // Dispose the progressive progressive simulator if it was created.
                    progressiveSimulator?.Dispose();

                    // Dispose the random number generator.
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if(random is IDisposable disposableRandom)
                    {
                        disposableRandom.Dispose();
                    }

                    // Clear any outcome adjustments that are still registered.
                    outcomeAdjustments.Clear();
                }

                disposed = true;
            }
        }

        #endregion

        #region Constructors and Finalizers

        /// <summary>
        /// Initialize a new instance of Game Lib that has a memory backed
        /// safe storage, plays a single hard coded denomination, uses no
        /// game registries, and has no simulated system configurations.
        /// </summary>
        /// <remarks>
        /// Game Lib with memory backed storage has no power hit tolerance.
        /// </remarks>
        public GameLib() : this(useCompressedSafeStorage: true)
        {
        }

        /// <summary>
        /// Initialize a new instance of Game Lib that has a memory backed
        /// safe storage, with choices of using game registries and/or a
        /// system configuration file.
        /// </summary>
        /// <remarks>
        /// Game Lib with memory backed storage has no power hit tolerance.
        /// </remarks>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for initialization.
        /// </param>
        /// <param name="systemConfigStream">
        /// A stream from which the system configurations to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        public GameLib(bool useGameRegistries, Stream systemConfigStream)
            : this(
                useGameRegistries,
                systemConfigStream,
                useCompressedSafeStorage: true)
        {
        }

        /// <summary>
        /// Initialize a new instance of Game Lib that has a memory backed
        /// safe storage, with choices of using game registries and/or a
        /// system configuration file, and the option to disable safe storage compression.
        /// </summary>
        /// <remarks>
        /// Game Lib with memory backed storage has no power hit tolerance.
        ///
        /// Disabling compression improves the latency of safe storage operations but increases the amount
        /// of space utilized.
        /// </remarks>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for initialization.
        /// </param>
        /// <param name="systemConfigStream">
        /// A stream from which the system configurations to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <param name="compressSafeStorage">
        /// A flag which disables safe storage compression if set to false.
        /// </param>
        public GameLib(bool useGameRegistries, Stream systemConfigStream, bool compressSafeStorage)
            : this(
                useGameRegistries,
                systemConfigStream,
                useCompressedSafeStorage: compressSafeStorage)
        {
        }

        /// <summary>
        /// Construct a GameLib with a custom mount point.
        /// </summary>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for initialization.
        /// </param>
        /// <param name="systemConfigStream">
        /// A stream from which the system configuration is to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <param name="mountPoint">Path indicating the game mount point.</param>
        public GameLib(bool useGameRegistries, Stream systemConfigStream, string mountPoint)
            : this(
                useGameRegistries,
                systemConfigStream,
                useCompressedSafeStorage: true,
                gameMountPoint: mountPoint)
        {
        }

        /// <summary>
        /// Initialize a new instance of Game Lib that uses the specified RNG, and has a memory backed
        /// safe storage, with choices of using game registries and/or a system configuration file.
        /// </summary>
        /// <remarks>
        /// Game Lib with memory backed storage has no power hit tolerance.
        /// </remarks>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for initialization.
        /// </param>
        /// <param name="systemConfigStream">
        /// A stream from which the system configurations to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <param name="randomNumberGenerator">The implementation of <see cref="IRandomNumberGenerator"/> to use.</param>
        public GameLib(bool useGameRegistries, Stream systemConfigStream, IRandomNumberGenerator randomNumberGenerator)
            : this(
                useGameRegistries,
                systemConfigStream,
                useCompressedSafeStorage: true,
                randomNumberGenerator: randomNumberGenerator)
        {
        }

        /// <summary>
        /// Initialize a new instance of Game Lib that uses the specified RNG, and has a memory backed
        /// safe storage, with choices of using game registries and/or a system configuration file, and
        /// the option to disable safe storage compression.
        /// </summary>
        /// <remarks>
        /// Game Lib with memory backed storage has no power hit tolerance.
        ///
        /// Disabling compression improves the latency of safe storage operations but increases the amount
        /// of space utilized.
        /// </remarks>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for initialization.
        /// </param>
        /// <param name="systemConfigStream">
        /// A stream from which the system configurations to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <param name="randomNumberGenerator">The implementation of <see cref="IRandomNumberGenerator"/> to use.</param>
        /// <param name="compressSafeStorage">
        /// A flag which disables safe storage compression if set to false.
        /// </param>
        public GameLib(
            bool useGameRegistries,
            Stream systemConfigStream,
            IRandomNumberGenerator randomNumberGenerator,
            bool compressSafeStorage)
            : this(
                useGameRegistries,
                systemConfigStream,
                useCompressedSafeStorage: compressSafeStorage,
                randomNumberGenerator: randomNumberGenerator)
        {
        }

        /// <summary>
        /// Initialize a new instance of Game Lib that has a file backed
        /// safe storage, plays a single hard coded denomination, uses no
        /// game registries, and has no simulated system configurations.
        /// </summary>
        /// <remarks>
        /// Game Lib with file backed storage provides basic safe storage.
        /// This storage does not guarantee data consistency.
        /// </remarks>
        /// <param name="modifierPath">The modifier path to use.</param>
        /// <param name="committedPath">The committed path to use.</param>
        /// <remarks>
        /// The working directory at the time of construction will be used for critical data access and as the mount
        /// point for the game.
        /// </remarks>
        public GameLib(string modifierPath, string committedPath)
            : this(
                useFileBackedSafeStorage: true,
                useCompressedSafeStorage: true,
                modifierPath: modifierPath,
                committedPath: committedPath)
        {
        }

        /// <summary>
        /// Initialize a new instance of Game Lib that has a file backed
        /// safe storage, with choices of using game registries and/or a
        /// system configuration file.
        /// </summary>
        /// <remarks>
        /// Game Lib with file backed storage provides basic safe storage.
        /// This storage does not guarantee data consistency.
        /// </remarks>
        /// <param name="modifierPath">The modifier path to use.</param>
        /// <param name="committedPath">The committed path to use.</param>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for initialization.
        /// </param>
        /// <param name="systemConfigStream">
        /// A stream from which the system configurations to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <param name="randomNumberGenerator">The implementation of <see cref="IRandomNumberGenerator"/> to use.</param>
        /// <remarks>
        /// The working directory at the time of construction will be used for critical data access and as the mount
        /// point for the game.
        /// </remarks>
        public GameLib(
            string modifierPath,
            string committedPath,
            bool useGameRegistries,
            Stream systemConfigStream,
            IRandomNumberGenerator randomNumberGenerator = null)
            : this(
                useGameRegistries,
                systemConfigStream,
                useFileBackedSafeStorage: true,
                useCompressedSafeStorage: true,
                modifierPath: modifierPath,
                committedPath: committedPath,
                randomNumberGenerator: randomNumberGenerator)
        {
        }

        /// <summary>
        /// Initialize a new instance of Game Lib that has a file backed
        /// safe storage, with choices of using game registries and/or a
        /// system configuration file, and a choice if the safe storage should be binary or XML file backed.
        /// </summary>
        /// <remarks>
        /// Game Lib with file backed storage provides basic safe storage.
        /// This storage does not guarantee data consistency.
        /// </remarks>
        /// <param name="modifierPath">The modifier path to use.</param>
        /// <param name="committedPath">The committed path to use.</param>
        /// <param name="useBinarySafeStorage">
        /// True indicates that binary safe storage should be used, false indicates XML backed safe storage.
        /// </param>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for initialization.
        /// </param>
        /// <param name="systemConfigStream">
        /// A stream from which the system configurations to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <remarks>
        /// The working directory at the time of construction will be used for critical data access and as the mount
        /// point for the game.
        /// </remarks>
        public GameLib(
            bool useGameRegistries,
            Stream systemConfigStream,
            string modifierPath,
            string committedPath,
            bool useBinarySafeStorage)
            : this(
                useGameRegistries,
                systemConfigStream,
                useFileBackedSafeStorage: true,
                useBinarySafeStorage: useBinarySafeStorage,
                useCompressedSafeStorage: true,
                modifierPath: modifierPath,
                committedPath: committedPath)

        {
        }

        /// <summary>
        /// Internal constructor used by the public constructors.
        /// </summary>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for initialization.
        /// </param>
        /// <param name="systemConfigStream">
        /// A stream from which the system configurations to be read.
        /// System configuration file must conform to the schema defined in "SystemConfigFile.xsd".
        /// </param>
        /// <param name="useFileBackedSafeStorage">
        /// Flag indicating if file backed safe storage should be used. If false safe storage will be memory backed.
        /// </param>
        /// <param name="useBinarySafeStorage">
        /// Flag indicating if binary file backed safe storage should be used.
        /// </param>
        /// <param name="useCompressedSafeStorage">
        /// Flag indicating if the safe storage implementation should compress its data.
        /// </param>
        /// <param name="modifierPath">
        /// Path to the Modifier file (for file backed safe storage).
        /// </param>
        /// <param name="committedPath">
        /// Path to the Committed file (for file backed safe storage).
        /// </param>
        /// <param name="randomNumberGenerator">The implementation of <see cref="IRandomNumberGenerator"/> to use.</param>
        /// <param name="gameMountPoint">The mount point for the game package.</param>
        private GameLib(
            bool useGameRegistries = false,
            Stream systemConfigStream = null,
            bool useFileBackedSafeStorage = false,
            bool useBinarySafeStorage = false,
            bool useCompressedSafeStorage = false,
            string modifierPath = null,
            string committedPath = null,
            IRandomNumberGenerator randomNumberGenerator = null,
            string gameMountPoint = null)
        {
            if(useFileBackedSafeStorage)
            {
                if(useBinarySafeStorage)
                {
                    DiskStoreManager = useCompressedSafeStorage
                        ? new CompressedBinaryDiskStoreManager(modifierPath, committedPath)
                        : new BinaryDiskStoreManager(modifierPath, committedPath);
                }
                else
                {
                    DiskStoreManager = useCompressedSafeStorage
                        ? new CompressedDiskStoreManager(modifierPath, committedPath)
                        : new DiskStoreManager(modifierPath, committedPath);
                }
            }
            else
            {
                DiskStoreManager = useCompressedSafeStorage
                    ? new CompressedDiskStoreManager()
                    : new DiskStoreManager();
            }

            CreateEventLookupTable();
            CreateEnterStateTable();
            CreateExitStateTable();

            // Make sure these event handlers are registered before any manager's.
            ActivateThemeContextEvent += HandleActivateThemeContextEvent;
            InactivateThemeContextEvent += HandleInactivateThemeContextEvent;

            // Use the default RNG if it isn't specified in the constructor parameters.
            if(randomNumberGenerator == null)
            {
                randomNumberGenerator = new DefaultRngAdapter();
            }

            random = randomNumberGenerator;

            // Use the current directory as a mount point if it is not specified by the constructor.
            if(gameMountPoint == null)
            {
                gameMountPoint = Directory.GetCurrentDirectory();
            }

            // Create the progressive broadcast manager.
            progressiveBroadcastManager = new ProgressiveBroadcastManager(ProgressiveBroadcastInterval);
            progressiveBroadcastManager.ProgressiveBroadcastEvent += BroadcastProgressiveData;

            // Initialize game registry manager, and discover the registries if requested.
            gameRegistryManager = new GameRegistryManager(useGameRegistries, gameMountPoint);

            // Initialize disk store section indexer.
            diskStoreSectionIndexer = new DiskStoreSectionIndexer(gameRegistryManager);

            // Initialize configuration read.  It reads configurations from the disk store.
            configurationRead = new GameConfigurationRead(DiskStoreManager, diskStoreSectionIndexer, this, this);

            // Load the root element of the system configurations.
            var systemConfigRoot = SystemConfigUtility.LoadSystemConfigFile(systemConfigStream);

            // Initialize paytable list manager.
            var sectionElement = systemConfigRoot?.Element("PaytableList");
            paytableListManager = new PaytableListManager(new PaytableListParser(sectionElement),
                                                          gameRegistryManager);

            // Initialize progressive manager.
            sectionElement = systemConfigRoot?.Element("SystemControlledProgressives");
            progressiveManager = new ProgressiveManager(this,
                                                        new ProgressiveParser(sectionElement),
                                                        gameRegistryManager);

            // Create the event coordinator.
            eventCoordinator = new EventCoordinator(this, null);
            builtinServices[typeof(IEventCoordinator)] = eventCoordinator;
            builtinServices[typeof(IEventProcessing)] = eventCoordinator;

            eventCoordinator.RegisterEventSource(progressiveBroadcastManager);

            // Add other service interfaces.
            builtinServices[typeof(ITransactionAugmenter)] = this;

            // Create the network progressive simulator in an idle state and add it to this game lib's built in services,
            // if it's enabled in the standalone system configuration file.
            sectionElement = systemConfigRoot?.Element("SystemProgressiveSimulator");
            if(sectionElement != null)
            {
                var progressiveSimulatorParser = new ProgressiveSimulatorParser(sectionElement);
                if(progressiveSimulatorParser.Enabled)
                {
                    progressiveSimulator = new ProgressiveSimulator(this, progressiveManager, progressiveSimulatorParser);
                    builtinServices[typeof(IProgressiveSimulator)] = progressiveSimulator;
                }
            }

            // Initialize foundation owned settings, including environment attributes and
            // foundation owned configurations etc.
            sectionElement = systemConfigRoot?.Element("FoundationOwnedSettings");
            var foundationOwnedSettingsParser = new FoundationOwnedSettingsParser(sectionElement);

            environmentAttributes = foundationOwnedSettingsParser.EnvironmentAttributes
                                        ?? new List<EnvironmentAttribute>();

            GameMinBetAmount = foundationOwnedSettingsParser.GameMinBet;

            var winCapBehaviorInfo = new WinCapBehaviorInfo(
                foundationOwnedSettingsParser.WinCapBehavior,
                foundationOwnedSettingsParser.WinCapLimit,
                foundationOwnedSettingsParser.WinCapMultiplier);

            WinCapInformation = new WinCapInformation(
                winCapBehaviorInfo,
                foundationOwnedSettingsParser.ProgressiveWinCapLimit,
                foundationOwnedSettingsParser.TotalWinCapLimit);

            BankToWagerableDefaultAmount = foundationOwnedSettingsParser.TransferBankToWagerable;

            MaxHistorySteps = foundationOwnedSettingsParser.MaxHistorySteps > 0
                                  ? foundationOwnedSettingsParser.MaxHistorySteps
                                  : HardCodedMaxHistorySteps;

            GamePlayBehaviorConfigs = new GamePlayBehaviorConfigs
                                      {
                                          DefaultBetSelectionStyle = new BetSelectionStyleInfo(),
                                          RtpOrderedByBetRequired = foundationOwnedSettingsParser.RtpOrderedByBetRequired
                                      };

            PresentationBehaviorConfigs = new PresentationBehaviorConfigs
                                          {
                                              MinimumBaseGameTime = foundationOwnedSettingsParser.MinimumBaseGamePresentationTime,
                                              MinimumFreeSpinTime = foundationOwnedSettingsParser.MinimumFreeSpinTime,
                                              CreditMeterBehavior = foundationOwnedSettingsParser.CreditMeterBehavior,
                                              DisplayVideoReelsForStepper = foundationOwnedSettingsParser.DisplayVideoReelsForStepper,
                                              BonusSoaaSettings = foundationOwnedSettingsParser.BonusSoaaSettings
                                          };

            ancillarySysConfiguration = foundationOwnedSettingsParser.AncillarySysConfig
                                        ?? new AncillaryConfiguration(false, 1, 20000);

            autoPlaySysConfig = foundationOwnedSettingsParser.AutoPlayConfiguration;

            autoPlayConfirmationRequired = foundationOwnedSettingsParser.AutoPlayConfirmationRequired;

            autoPlaySpeedIncreaseAllowed = foundationOwnedSettingsParser.AutoPlaySpeedIncreaseAllowed;

            Jurisdiction = foundationOwnedSettingsParser.Jurisdiction;

            roundWagerUpPlayoffEnabledConfig = foundationOwnedSettingsParser.RoundWagerUpPlayoffEnabled;

            marketingBehavior = new MarketingBehavior
            {
                TopScreenGameAdvertisement = foundationOwnedSettingsParser.MarketingBehavior.
                    TopScreenGameAdvertisement
            };

            localizationInformation = new LocalizationInformation();
            if(foundationOwnedSettingsParser.CreditFormatter != null)
            {
                localizationInformation.SetCreditFormatter(foundationOwnedSettingsParser.CreditFormatter);
            }

            // Save the result in a Boolean type to speed up the performance.
            isBankedCreditsEnvironment = IsEnvironmentTrue(EnvironmentAttribute.BankedCredits);

            // Check if it is a cold start.
            var isInitialized = ReadFoundationData<bool>(FoundationDataScope.Theme, "Game Lib Initialized");
            isColdStart = !isInitialized;

            // Initialize history manager.
            historyManager = new HistoryManager(this, isColdStart);

            // Initialize game sub-mode.
            sectionElement = systemConfigRoot?.Element("GameSubModeType");
            var gameSubModeParser = new GameSubModeParser(sectionElement);

            GameSubMode = gameSubModeParser.GameSubMode;

            // Initialize tournament session.
            sectionElement = systemConfigRoot?.Element("TournamentSessionConfiguration");
            tournamentSessionConfigParser = new TournamentSessionConfigParser(sectionElement);

            // Initialize STOMP broker configuration data.
            sectionElement = systemConfigRoot?.Element("StompBrokerConfiguration");
            stompBrokerConfigParser = new StompBrokerConfigParser(sectionElement);

            // Because the F2L message isBankLocked negatively mirrors isPlayerWagerOfferable we need to set to true
            // in the foundation data. This is because isPlayerWagerOfferable should default to true as opposed to false
            // which will happen if not present in the Foundation data.
            WriteFoundationData(FoundationDataScope.Theme, GameLibPrefix + IsPlayerWagerOfferablePath, true);

            // Load the configurations from game registry files only when it is cold start.
            if(isColdStart)
            {
                InitializeFoundationDataColdStart();
            }
            // Restore fields from the safe storage only when it is warm start.
            else
            {
                RestoreFoundationData();
            }

            // Post Player Bank and State Information.
            PostNonTransactionalEvent(GameState == GameCycleState.Idle ? new FoundationStateChangedEventArgs(true) :
                                                                         new FoundationStateChangedEventArgs(false));
            PostNonTransactionalEvent(new PlayerBankMeterChangedEventArgs(PlayerBankMeter));

            //Assign a randomly generated token.
            Token = new Random().Next().ToString();
        }

        /// <summary>
        /// Finalizer to dispose of any unmanaged resources.
        /// </summary>
        ~GameLib()
        {
            Dispose(false);
        }

        #endregion

        #region Private Methods

        #region State Handlers

        // All State Handlers should be of Action type,
        // i.e. void Foo().

        /// <summary>
        /// Perform operations needed when entering the Idle state.
        /// </summary>
        private void EnterIdle()
        {
            IsCashOutOfferable = PlayerBankMeter > 0 &&
                                  IsPlayerWagerOfferable;

            IsBankToWagerableOfferable = isBankedCreditsEnvironment &&
                                         PlayerBankMeter > 0 &&
                                          IsPlayerWagerOfferable;

            // Clear the critical data of the last game cycle.
            ClearCriticalDataScope(CriticalDataScope.GameCycle);
            ClearCriticalDataScope(CriticalDataScope.History);
            ClearFoundationDataScope(FoundationDataScope.GameCycle);

            // Reset the critical data of Game Cycle scope.
            // This must be done after the critical data
            // of Game Cycle scope has been cleared.
            CommittedBet = InvalidBetAmount;
            StartingBet = InvalidBetAmount;
            MidGameBet = 0;

            AutoAddCredits();

            PostNonTransactionalEvent(new FoundationStateChangedEventArgs(true));
        }

        /// <summary>
        /// Perform operations needed when entering the Playing state.
        /// </summary>
        private void EnterPlaying()
        {
            IsBankToWagerableOfferable = isBankedCreditsEnvironment &&
                                         PlayerBankMeter > 0 &&
                                         IsPlayerWagerOfferable;
        }

        /// <summary>
        /// Perform operations needed when entering the AncillaryPlaying state.
        /// </summary>
        private void EnterAncillaryPlaying()
        {
            IsBankToWagerableOfferable = isBankedCreditsEnvironment &&
                                         PlayerBankMeter > 0 &&
                                         IsPlayerWagerOfferable;
        }

        /// <summary>
        /// Perform operations needed when entering the MainPlayComplete state.
        /// </summary>
        private void EnterMainPlayComplete()
        {
            IsCashOutOfferable = AncillaryEnabled;
        }

        /// <summary>
        /// Perform operations needed when exiting the Idle state.
        /// </summary>
        private void ExitIdle()
        {
            IsCashOutOfferable = false;
            IsBankToWagerableOfferable = false;

            // Paid meter should be cleared upon exiting the Idle state.
            PlayerPaidMeter = 0;

            PostNonTransactionalEvent(new FoundationStateChangedEventArgs(false));
        }

        /// <summary>
        /// Perform operations needed when exiting the Playing state.
        /// </summary>
        private void ExitPlaying()
        {
            IsBankToWagerableOfferable = false;
        }

        /// <summary>
        /// Perform operations needed when exiting the AncillaryPlaying state.
        /// </summary>
        private void ExitAncillaryPlaying()
        {
            IsBankToWagerableOfferable = false;
        }

        /// <summary>
        /// Perform operations needed when exiting the MainPlayComplete state.
        /// </summary>
        private void ExitMainPlayComplete()
        {
            IsCashOutOfferable = false;
        }

        #endregion

        #region Miscellaneous Handling

        /// <summary>
        /// This function is a generic way to invoke an event handler.
        /// </summary>
        /// <param name="sender">The object posting the event.</param>
        /// <param name="eventArgs">The event payload.</param>
        /// <param name="eventHandler">The handler for the event.</param>
        private static void ExecuteHandler<TEventArgs>(object sender, EventArgs eventArgs,
                                                       EventHandler<TEventArgs> eventHandler)
                                                       where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, eventArgs as TEventArgs);
        }

        /// <summary>
        /// This function is a generic way to invoke a context activation event handler.
        /// </summary>
        /// <param name="sender">The object posting the event.</param>
        /// <param name="eventArgs">The event payload.</param>
        /// <param name="eventHandler">The handler for the event.</param>
        /// <param name="levelEventHandler">The level context activation event handler.</param>
        /// <param name="contextLayer">The level of the context.</param>
        private static void ExecuteLayeredContextActivation<TEventArgs>(object sender, EventArgs eventArgs,
                                                                        EventHandler<TEventArgs> eventHandler,
                                                                        EventHandler<LayeredContextActivationEventArgs> levelEventHandler,
                                                                        ContextLayer contextLayer)
            where TEventArgs : EventArgs
        {
            eventHandler?.Invoke(sender, eventArgs as TEventArgs);

            levelEventHandler?.Invoke(sender, new LayeredContextActivationEventArgs(contextLayer));
        }

        /// <summary>
        /// Event handler for ActivateThemeContextEvent.
        /// <list type="bullet">
        /// <item>Update the game context mode to the requested one.</item>
        /// <item>Prepare the theme scope of critical data and configurations
        /// for the new theme context.</item>
        /// <item>Prepare the payvar scope of critical data and configurations
        /// for the new theme context.</item>
        /// <item>Update Foundation owned configurations for the new them context.</item>
        /// </list>
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the game sub-mode is tournament but the paytable being activated does not support tournament.
        /// </exception>
        /// <DevDoc>
        /// The reason why the context mode is set in this
        /// event handler is to make game lib stay in Invalid
        /// mode until after receiving the first Activate Theme
        /// Context event from ConnectToFoundation.  This way,
        /// the state machine framework can run correctly with
        /// the InitialStateMachine for the Invalid mode after
        /// a power up.
        /// </DevDoc>
        private void HandleActivateThemeContextEvent(object sender, ActivateThemeContextEventArgs eventArgs)
        {
            var themeContext = eventArgs.ThemeContext;

            // There is no need to set the GameSubMode here since it has already been set
            // when the Standalone GameLib gets created if a system configuration file stream
            // is provided. Otherwise, it would default to GameSubMode.Standard.
            GameContextMode = themeContext.GameContextMode;
            PaytableName = themeContext.PaytableName;
            PaytableFileName = themeContext.PaytableFileName;
            GameDenomination = themeContext.Denomination;

            if(themeContext.GameContextMode == GameMode.Play || themeContext.GameContextMode == GameMode.Utility)
            {
                var paytableVariant = new PaytableVariant(ThemeIdentifier, PaytableName, PaytableFileName);

                // Update components that need to track current payvar.
                var paytableIdentifier = gameRegistryManager.UpdatePayvarSelection(paytableVariant);
                diskStoreSectionIndexer.UpdateThemeContext(ThemeIdentifier, paytableIdentifier);

                // Update the game min bet from machine wide min bet per game from System Config.
                GameMinBet = Utility.ConvertToCredits(GameMinBetAmount, GameDenomination, true);

                // Update max bet and button panel min bet for the new theme context from System Config.
                var paytableConfiguration = paytableListManager.GetPaytableConfiguration(paytableVariant, GameDenomination);
                GameMaxBet = paytableConfiguration.MaxBet == null ||
                             paytableConfiguration.MaxBet == GameRegistryManager.NoRegistryInUse
                                            ? HardCodedGameMaxBet
                                            : (long)paytableConfiguration.MaxBet;
                ButtonPanelMinBet = paytableConfiguration.ButtonPanelMinBet == null ||
                                    paytableConfiguration.ButtonPanelMinBet == GameRegistryManager.NoRegistryInUse
                                            ? HardCodedButtonPanelMinBet
                                            : (long)paytableConfiguration.ButtonPanelMinBet;

                // Update Foundation owned configurations for the new theme context.
                MaxBetButtonBehavior = gameRegistryManager.GetMaxBetButtonBehavior();
                var doubleUpSupported = gameRegistryManager.GetDoubleUpSupported(paytableVariant);
                AncillaryEnabled = ancillarySysConfiguration.AncillarySupported && doubleUpSupported;

                ConfiguredLineSelectionMode = gameRegistryManager.GetDefaultLineSelectionMode();

                // Throw an error if the current game sub-mode is tournament and the paytable registry
                // does not support tournament.
                if(GameSubMode == GameSubMode.Tournament &&
                   gameRegistryManager.GetPayvarType(paytableVariant) != PayvarType.Tournament)
                {
                    throw new InvalidOperationException(
                        $"The PayvarType in the paytable registry for paytable {paytableVariant.PaytableName} is not tournament.");
                }

                if(themeContext.GameContextMode == GameMode.Play)
                {
                    // ReSharper disable once InconsistentlySynchronizedField
                    progressiveBroadcastManager?.Activate(true);
                    progressiveSimulator?.Start();
                }
            }
        }

        /// <summary>
        /// Event handler for InactivateThemeContextEvent.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleInactivateThemeContextEvent(object sender, InactivateThemeContextEventArgs eventArgs)
        {
            GameContextMode = GameMode.Invalid;
            // ReSharper disable once InconsistentlySynchronizedField
            progressiveBroadcastManager?.Activate(false);
            progressiveSimulator?.Stop();
        }

        /// <summary>
        /// Check if an open transaction is available for the operation.
        /// Should be called by all IGameLib methods.
        /// </summary>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when there is no open transaction available.
        /// </exception>
        /// <devdoc>
        /// This file and IGameLib need clean up to check open transaction for all methods.
        /// </devdoc>>
        private void MustHaveOpenTransaction()
        {
            bool openTransactionAvailable;

            lock(eventLocker)
            {
                // There should be either a game initiated transaction or
                // a Foundation initiated one available.
                openTransactionAvailable = TransactionOpen || foundationTransactionOpen;
            }

            if(!openTransactionAvailable)
            {
                throw new InvalidTransactionException("No open transaction is available for this operation.");
            }
        }

        /// <summary>
        /// Transition to a new game cycle state.
        /// </summary>
        /// <param name="newState">The state to be entered.</param>
        private void GotoState(GameCycleState newState)
        {
            if(newState == GameCycleState.Invalid)
            {
                throw new ArgumentOutOfRangeException(nameof(newState), "Cannot go to an invalid state.");
            }

            if(GameState != newState)
            {
                ExitState(GameState);
                GameState = newState;
                EnterState(GameState);
            }
        }

        /// <summary>
        /// Exit a game cycle state.
        /// </summary>
        /// <param name="currentState">The state about to exit.</param>
        private void ExitState(GameCycleState currentState)
        {
            if(exitState.ContainsKey(currentState) && exitState[currentState] != null)
            {
                exitState[currentState]();
            }
        }

        /// <summary>
        /// Enter a new game cycle state.
        /// </summary>
        /// <param name="newState">The state to be entered.</param>
        private void EnterState(GameCycleState newState)
        {
            if(enterState.ContainsKey(newState) && enterState[newState] != null)
            {
                enterState[newState]();
            }
        }

        /// <summary>
        /// Build the error message for FunctionCallNotAllowedInModeOrStateException.
        /// </summary>
        /// <returns>The error message built.</returns>
        private static string BuildMessageForWrongStateException()
        {
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(1);
            var methodBase = stackFrame.GetMethod();
            var caller = methodBase.Name;

            var result = ValidStateForFunction.ContainsKey(caller)
                             ? $"Function {caller} can only be called from {ValidStateForFunction[caller]}"
                             : $"Function {caller} is not allowed in current game mode and state.";

            return result;
        }

        /// <summary>
        /// Read critical data which is specific to the foundation and which may not be
        /// accessed from the game.
        /// </summary>
        /// <typeparam name="T">The type of the data being read.</typeparam>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path the data is stored at.</param>
        /// <returns>Value read from the specified scope and path.</returns>
        internal T ReadFoundationData<T>(FoundationDataScope scope, string path)
        {
            Utility.ValidateCriticalDataName(path);

            var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(scope);
            return DiskStoreManager.Read<T>(section, index, path);
        }

        /// <summary>
        /// Initialize foundation data with cold start values.
        /// </summary>
        private void InitializeFoundationDataColdStart()
        {
            CreateFoundationTransaction();

            // Initialize fields whose initial value is different from the OS default.
            GameState = GameCycleState.Idle;

            CommittedBet = InvalidBetAmount;
            StartingBet = InvalidBetAmount;

            // Safe store the initial configuration values provided in the game registries.
            gameRegistryManager.WriteConfigurationItems(DiskStoreManager, Jurisdiction);

            WriteFoundationData(FoundationDataScope.Theme, "Game Lib Initialized", true);

            CloseFoundationTransaction();
        }

        /// <summary>
        /// Restore foundation data for initialization.
        /// </summary>
        /// <exception cref="RestoreFoundationException">
        /// Thrown if playModeTheme is null,
        /// either of playerWagerableMeter, playerBankMeter,
        /// playerPaidMeter, committedBet, startingBet, midGameBet is less than 0,
        /// availableLanguages, GameLanguage is empty,
        /// gameState is invalid or
        /// Thrown when the ReadFoundationData does not match the type expected.
        /// </exception>
        private void RestoreFoundationData()
        {
            try
            {
                playModeTheme = ReadFoundationData<string>(FoundationDataScope.Theme,
                                                           GameLibPrefix + PlayModeThemeNamePath);
                if(string.IsNullOrEmpty(playModeTheme))
                {
                    throw new RestoreFoundationException("The play mode theme cannot be empty or null.", "playModeTheme");
                }

                playModeGameDenomination = ReadFoundationData<long>(FoundationDataScope.Theme,
                                                                    GameLibPrefix + PlayModeGameDenominationPath);

                availableLanguages = ReadFoundationData<List<string>>(FoundationDataScope.Theme,
                                                                      GameLibPrefix + AvailableLanguagesPath);
                if(!availableLanguages.Any())
                {
                    throw new RestoreFoundationException("The list of available languages is empty.",
                                                         "availableLanguages");
                }

                gameLanguage = ReadFoundationData<string>(FoundationDataScope.Theme,
                                                          GameLibPrefix + GameLanguagePath);
                if(string.IsNullOrEmpty(gameLanguage))
                {
                    throw new RestoreFoundationException("The language string cannot be empty.", "gameLanguage");
                }

                gameState = ReadFoundationData<GameCycleState>(FoundationDataScope.Theme,
                                                               GameLibPrefix + GameStatePath);
                if(gameState == GameCycleState.Invalid)
                {
                    throw new RestoreFoundationException("Game state cannot be in invalid state.", "gameState");
                }

                isPlayerWagerOfferable = ReadFoundationData<bool>(FoundationDataScope.Theme,
                                                        GameLibPrefix + IsPlayerWagerOfferablePath);

                isCashOutOfferable = ReadFoundationData<bool>(FoundationDataScope.Theme,
                                                              GameLibPrefix + IsCashOutOfferablePath);

                isBankToWagerableOfferable = ReadFoundationData<bool>(FoundationDataScope.Theme,
                                                                      GameLibPrefix + IsBankToWagerableOfferablePath);

                playerWagerableMeter = ReadFoundationData<long>(FoundationDataScope.Theme,
                                                                GameLibPrefix + PlayerWagerableMeterPath);
                if(playerWagerableMeter < 0)
                {
                    throw new RestoreFoundationException("Money available for player betting can not be less than 0",
                                                         "playerWagerableMeter");
                }

                playerBankMeter = ReadFoundationData<long>(FoundationDataScope.Theme,
                                                           GameLibPrefix + PlayerBankMeterPath);
                if(playerBankMeter < 0)
                {
                    throw new RestoreFoundationException("Player bank meter can not be less than 0",
                                                         "playerBankMeter");
                }

                playerPaidMeter = ReadFoundationData<long>(FoundationDataScope.Theme,
                                                           GameLibPrefix + PlayerPaidMeterPath);
                if(playerPaidMeter < 0)
                {
                    throw new RestoreFoundationException("Player paid meter can not be less than 0",
                                                         "playerPaidMeter");
                }

                committedBet = ReadFoundationData<long>(FoundationDataScope.GameCycle,
                                                        GameLibPrefix + CommittedBetPath);
                if(committedBet < -1)
                {
                    throw new RestoreFoundationException("Committed bet can not be less than -1",
                                                         "committedBet");
                }

                startingBet = ReadFoundationData<long>(FoundationDataScope.GameCycle,
                                                       GameLibPrefix + StartingBetPath);
                if(startingBet < -1)
                {
                    throw new RestoreFoundationException("Starting bet can not be less than -1",
                                                         "startingBet");
                }

                midGameBet = ReadFoundationData<long>(FoundationDataScope.GameCycle,
                                                      GameLibPrefix + MidGameBetPath);
                if(midGameBet < 0)
                {
                    throw new RestoreFoundationException("Mid game bet can not be less than 0",
                                                         "midGameBet");
                }

                restoredFoundationEvents = ReadFoundationData<List<EventArgs>>(FoundationDataScope.Theme,
                                                                               FoundationEventsPath);
            }

            catch(Exception e)
            {
                throw new RestoreFoundationException(e.Message, e);
            }
        }

        /// <summary>
        /// Write critical data which is specific to the foundation and which may not be
        /// accessed from the game.
        /// </summary>
        /// <param name="scope">The scope to write the data to.</param>
        /// <param name="path">The path in the specified scope to write the data.</param>
        /// <param name="data">The data to write in the specified scope at the specified path.</param>
        internal void WriteFoundationData(FoundationDataScope scope, string path, object data)
        {
            Utility.ValidateCriticalDataName(path);

            var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(scope);
            DiskStoreManager.Write(section, index, path, data);
        }

        /// <summary>
        /// Clear the critical data scope which is specific to the foundation and which may not be
        /// accessed from the game.
        /// </summary>
        /// <param name="scope">The scope to clear.</param>
        /// <devDoc>
        /// This wrapper function is to ensure the correct scope to be used, rather than
        /// the type agnostic integer.
        /// </devDoc>
        internal void ClearFoundationDataScope(FoundationDataScope scope)
        {
            var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(scope);
            DiskStoreManager.ClearScope(section, index);
        }

        /// <summary>
        /// Clear the critical data scope.
        /// </summary>
        /// <param name="scope">The scope to clear.</param>
        /// <devDoc>
        /// This wrapper function is to ensure the correct scope to be used, rather than
        /// the type agnostic integer.
        /// </devDoc>
        internal void ClearCriticalDataScope(CriticalDataScope scope)
        {
            var (section, index) = diskStoreSectionIndexer.GetCriticalDataLocation(scope);
            DiskStoreManager.ClearScope(section, index);
        }

        /// <summary>
        /// Start a theme context that is not considered as newly selected for play
        /// by posting necessary theme selection and theme context events.
        /// </summary>
        /// <param name="isNewTheme">The flag indicating if it is a new theme.</param>
        /// <param name="gameMode">The game mode for the new theme context.</param>
        /// <param name="paytableVariant">The paytable variant of the new theme.</param>
        /// <param name="denomination">The denomination for the new theme context.</param>
        /// <remarks>
        /// If the theme context to start is considered as newly selected for play, please call the
        /// methods <see cref="StartNewTheme"/>, if necessary, and <see cref="ActivateThemeContext"/> directly.
        /// </remarks>
        private void StartThemeContext(bool isNewTheme, GameMode gameMode, PaytableVariant paytableVariant,
                                       long denomination)
        {
            if(isNewTheme)
            {
                ThemeIdentifier = paytableVariant.ThemeIdentifier;
                StartNewTheme(paytableVariant.ThemeIdentifier);
            }

            ActivateThemeContext(gameMode, paytableVariant, denomination, false);
        }

        /// <summary>
        /// Start a new theme selection by posting a NewThemeSelectionEvent.
        /// </summary>
        /// <param name="newThemeName">The new theme name.</param>
        private void StartNewTheme(string newThemeName)
        {
            // A theme change can only occur when the theme context is inactive.
            if(GameContextMode != GameMode.Invalid)
            {
                InactivateCurrentThemeContext();
            }

            // The Standalone Game Lib uses the theme name to synchronize the theme
            // selection internally, leaving the theme's tag data file and data tag
            // to be defined and interpreted by individual games.  Therefore, it
            // can not use NewThemeSelectionEvent, but has to update its manager
            // components manually here.
            gameRegistryManager.UpdateThemeSelection(newThemeName);
            progressiveManager.UpdateThemeSelection(newThemeName);

            // Post events.
            var themeResource = gameRegistryManager.GetThemeResource(newThemeName);
            PostEvent(new NewThemeSelectionEventArgs(themeResource));
        }

        /// <summary>
        /// Activate a new theme context by posting a NewThemeContextEvent followed by
        /// an ActivateThemeContextEvent.
        /// </summary>
        /// <param name="gameMode">The game mode for the new theme context.</param>
        /// <param name="paytableVariant">The paytable variant of the new theme.</param>
        /// <param name="denomination">The denomination for the new theme context.</param>
        /// <param name="newlySelectedForPlay">Flag indicating that the new (play mode) context is a context that
        /// should be considered new to the player.</param>
        private void ActivateThemeContext(GameMode gameMode, PaytableVariant paytableVariant,
                                          long denomination, bool newlySelectedForPlay)
        {
            // Get the absolute path of the paytable file.
            // The paytable file name in the registry and paytable list
            // should be relative to the game's mount point.
            var paytableFileFullPath = Path.Combine(GameMountPoint, paytableVariant.PaytableFileName);

            var themeContext = new ThemeContext(gameMode, denomination, paytableVariant.PaytableName,
                                                paytableFileFullPath, newlySelectedForPlay, GameSubMode);

            // Post theme context events.
            PostEvent(new NewThemeContextEventArgs(themeContext));

            PostEvent(new ActivateThemeContextEventArgs(themeContext));

            // Post display control event.
            SetDisplayControlNormal();

            // Update the internal game mode value before
            // the event is handled.
            processedGameMode = gameMode;
        }

        /// <summary>
        /// Inactivate the current theme context by posting a InactivateThemeContextEvent.
        /// </summary>
        private void InactivateCurrentThemeContext()
        {
            if(processedGameMode != GameMode.Invalid)
            {
                // Post display control event.
                SetDisplayControlHidden();
            }

            PostEvent(new InactivateThemeContextEventArgs());

            // Update the internal game mode value before
            // the event is handled.
            processedGameMode = GameMode.Invalid;
        }

        /// <summary>
        /// Open a simulated Foundation initiated transaction.
        /// </summary>
        private void CreateFoundationTransaction()
        {
            if(!TransactionOpen && !foundationTransactionOpen)
            {
                foundationTransactionOpen = true;
                transactionId++;
            }
        }

        /// <summary>
        /// Close a simulated Foundation initiated transaction.
        /// </summary>
        private void CloseFoundationTransaction()
        {
            if(foundationTransactionOpen)
            {
                foundationTransactionOpen = false;
                DiskStoreManager.Commit();
            }
        }

        /// <summary>
        /// The progressive broadcast event handler to retrieve the progressive broadcast data
        /// from the Foundation and post the broadcast event.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="eventArgs">The payload of this event.</param>
        private void BroadcastProgressiveData(object sender, EventArgs eventArgs)
        {
            // The data for "moreHandler" includes the data for "lessHandler".
            // So when applicable, use one F2X call to satisfy both event needs.
            var moreHandler = availableDenominationsWithProgressivesBroadcastEvent;
            var lessHandler = progressiveBroadcastEvent;

            if(moreHandler != null)
            {
                // Retrieve progressive info for all available denominations
                var moreData = RetrieveAvailableDenominationsWithProgressives();

                moreHandler(this, new DenominationsWithProgressivesBroadcastEventArgs(moreData));

                if(lessHandler != null)
                {
                    lessHandler(this, moreData.TryGetValue(GameDenomination, out var lessData)
                                          ? new ProgressiveBroadcastEventArgs(lessData)
                                          : new ProgressiveBroadcastEventArgs(emptyBroadcastData));
                }
            }
            else if(lessHandler != null)
            {
                // Retrieve progressive info for current denomination
                var lessData = progressiveManager.GetAllProgressiveBroadcastData();

                lessHandler(this, new ProgressiveBroadcastEventArgs(lessData));
            }
        }

        /// <summary>
        /// Retrieves the progressive broadcast data from Progressive Manager for all available progressive denominations.
        /// </summary>
        /// <returns>
        /// The progressive broadcast data for all available progressive denominations.
        /// Each denomination has a dictionary of progressive data keyed by game levels.
        /// </returns>
        private IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> RetrieveAvailableDenominationsWithProgressives()
        {
            var result = new Dictionary<long, IDictionary<int, ProgressiveBroadcastData>>();

            var availableDenominations = paytableListManager.GetAvailableDenominations(ThemeIdentifier) ?? new List<long> { HardCodedDenomination };

            foreach(var denomination in availableDenominations)
            {
                var paytable = paytableListManager.GetPaytableVariant(ThemeIdentifier, denomination);
                var broadcastDataList = progressiveManager.GetProgressiveBroadcastDataForDenominationAndPaytable(denomination, paytable);
                if(broadcastDataList.Count > 0)
                {
                    result.Add(denomination, broadcastDataList);
                }
            }

            return result;
        }

        /// <summary>
        /// Adjust the outcome list with progressive wins.
        /// </summary>
        /// <param name="outcomeList">The outcome list to be adjusted and returned.</param>
        // ReSharper disable once SuggestBaseTypeForParameter
        private void AdjustOutcomeForProgressive(OutcomeList outcomeList)
        {
            var featureEntries = outcomeList.GetFeatureEntries().Cast<FeatureEntry>().ToList();

            foreach(var featureEntry in featureEntries)
            {
                // Only adjust for feature awards.
                if(featureEntry.ContainsFeatureAwardOutcome)
                {
                    var awards = featureEntry.GetAwards<FeatureAward>().ToList();

                    foreach(var award in awards)
                    {
                        // Ensure the win level index is valid.
                        gameRegistryManager.ValidateWinLevel((int)award.WinLevelIndex, award.GetFeatureProgressiveAwards().Any());

                        var featureProgressiveAwards = award.GetFeatureProgressiveAwards().Cast<FeatureProgressiveAward>().ToList();

                        foreach(var featureProgressiveAward in featureProgressiveAwards)
                        {
                            // Validate the progressive hit.
                            if(featureProgressiveAward.HitState == ProgressiveAwardHitState.PotentialHit ||
                               featureProgressiveAward.HitState == ProgressiveAwardHitState.Hit)
                            {
                                progressiveManager.ValidateProgressiveHit(featureProgressiveAward);
                            }

                            // If the progressive award is not hit, apply the consolation amount to
                            // its parent feature award.
                            // Leave the feature award's amount specified and is displayable fields intact.
                            // The game must set them correctly in order to count them in the total
                            // displayable amount of the outcome list.
                            if(featureProgressiveAward.HitState == ProgressiveAwardHitState.NotHit)
                            {
                                award.UpdateAmountValue(award.AmountValue + featureProgressiveAward.ConsolationAmountValue);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if a bet is committable, regardless of the game state.
        /// </summary>
        /// <param name="bet">Total requested bet amount in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <param name="pendingWin">
        /// The win amount should be added to availableMoney, in base units.
        /// It should be 0 in most cases and has positive value
        /// when checking a bet committable for next game cycle.
        /// </param>
        /// <returns>True if requested bet amount may be committed.  False otherwise.</returns>
        private bool CanCommitBet(long bet, long denomination, long pendingWin)
        {
            // Can always commit in tournament.
            if(GameSubMode == GameSubMode.Tournament)
            {
                return true;
            }
            checked
            {
                var requestedBet = Utility.ConvertToCents(bet, denomination);
                // Banked credits support - the pending win is added to Bank and therefore not available to bet from wagerable.
                var availableMoney = isBankedCreditsEnvironment
                                         ? PlayerWagerableMeter
                                         : PlayerBankMeter + pendingWin;

                return requestedBet <= availableMoney + ValidCommittedBet &&
                       requestedBet <= Utility.ConvertToCents(GameMaxBet, GameDenomination) &&
                       (requestedBet == 0 || requestedBet >= GameMinBetAmount);
            }
        }

        /// <summary>
        /// Raise an event.
        /// </summary>
        /// <param name="handler">The event to raise.</param>
        /// <param name="eventArgs">The event data.</param>
        private void RaiseEvent(EventHandler handler, EventArgs eventArgs)
        {
            handler?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Checks if the theme supports AutoPlay.
        /// </summary>
        /// <returns>True is AutoPlay is supported. False otherwise</returns>
        private bool GameSupportsAutoPlay()
        {
            // If auto play confirmation is required, auto play will be disabled for those games which do not
            // support this feature.
            return gameRegistryManager.GetAutoPlaySupported(ThemeIdentifier) && 
                   (!autoPlayConfirmationRequired || gameRegistryManager.GetAutoPlayConfirmationSupported(ThemeIdentifier));
        }

        /// <summary>
        /// Adds or clears a game play enforcement.
        /// </summary>
        /// <param name="enforcement">The enforcement to add or clear.</param>
        /// <param name="enforce">True to enforce, false to clear.</param>
        private void Enforce(GamePlayEnforcements enforcement, bool enforce)
        {
            if(enforce)
            {
                gamePlayEnforcements |= enforcement;
            }
            else
            {
                gamePlayEnforcements &= ~enforcement;
            }
        }

        /// <summary>
        /// Checks whether a game play enforcement is in effect.
        /// </summary>
        /// <param name="enforcement">The enforcement to check.</param>
        private bool IsEnforced(GamePlayEnforcements enforcement)
        {
            return gamePlayEnforcements.HasFlag(enforcement);
        }

        #endregion

        #region Money Handling

        /// <summary>
        /// Add money amount to Player Bank Meter.
        /// </summary>
        /// <param name="value">Amount to add, in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination for the value to add.</param>
        private void AddMoney(long value, long denomination)
        {
            // Caller is responsible for validating the arguments.

            checked
            {
                PlayerBankMeter += Utility.ConvertToCents(value, denomination);

                // Paid meter should be cleared upon cash in.
                PlayerPaidMeter = 0;

                PostEvent(new MoneyEventArgs(MoneyInSource.OtherSource,
                                             value,
                                             denomination,
                                             new PlayerMeters(PlayerWagerableMeter,
                                                              PlayerBankMeter,
                                                              PlayerPaidMeter)));

                IsCashOutOfferable = GameState == GameCycleState.Idle &&
                                     isPlayerWagerOfferable;

                IsBankToWagerableOfferable = isBankedCreditsEnvironment &&
                                             isPlayerWagerOfferable;
            }
        }

        /// <summary>
        /// Move the specified amount from the Bank Meter to the Wagerable Meter.
        /// </summary>
        /// <param name="transferAmount">The amount to transfer, in base units.</param>
        /// <returns>True if the transfer is success; False otherwise.</returns>
        private bool TransferFromBankToWagerable(long transferAmount)
        {
            // Caller is responsible for validating the arguments.

            var result = false;

            if(IsBankToWagerableOfferable && IsPlayerWagerOfferable)
            {
                // If the balance of the Player Bank Meter is less than the transfer amount,
                // transfer all the amount that is available.
                if(PlayerBankMeter < transferAmount)
                {
                    transferAmount = PlayerBankMeter;
                }


                PlayerBankMeter -= transferAmount;
                PlayerWagerableMeter += transferAmount;

                PostEvent(new MoneyEventArgs(MoneyEventType.MoneyWagerable,
                                             Utility.ConvertToCredits(transferAmount, GameDenomination),
                                             GameDenomination,
                                             new PlayerMeters(PlayerWagerableMeter,
                                                              PlayerBankMeter,
                                                              PlayerPaidMeter),
                                             from: MoneyLocation.PlayerBankMeter,
                                             to: MoneyLocation.PlayerWagerableMeter));

                IsCashOutOfferable = GameState == GameCycleState.Idle &&
                                        PlayerBankMeter > 0 &&
                                        IsPlayerWagerOfferable;

                IsBankToWagerableOfferable = isBankedCreditsEnvironment &&
                                                PlayerBankMeter > 0 &&
                                                IsPlayerWagerOfferable;

                result = true;
            }

            return result;
        }

        /// <summary>
        /// Automatically add credits if the balance has fallen below the threshold.
        /// </summary>
        private void AutoAddCredits()
        {
            if(creditsToAdd > 0)
            {
                var wagerableCredits = Utility.ConvertToCredits(PlayerWagerableMeter, GameDenomination);
                var bankCredits = Utility.ConvertToCredits(PlayerBankMeter, GameDenomination);

                if(isBankedCreditsEnvironment && wagerableCredits <= creditThreshold)
                {
                    if(bankCredits < creditsToAdd)
                    {
                        AddMoney(creditsToAdd, GameDenomination);
                    }

                    // Transfer creditsToAdd from Bank meter to Wagerable meter.
                    TransferFromBankToWagerable(Utility.ConvertToCents(creditsToAdd, GameDenomination));
                }
                else if(!isBankedCreditsEnvironment && bankCredits <= creditThreshold)
                {
                    AddMoney(creditsToAdd, GameDenomination);
                }
            }
        }

        #endregion

        #region Non Transactional Event Processing

        /// <summary>
        /// Process any pending non transactional events
        /// </summary>
        private void ProcessNonTransactionalEvents()
        {
            lock(nonTransactionalEventLocker)
            {
                // Process any event that is already in the queue.
                if(nonTransactionalFoundationEvents.Count != 0)
                {
                    ExecuteNonTransactionalEvents(NonTransactionalEventProcessPace);
                }
            }
        }

        /// <summary>
        /// Standalone GameLib specific function for posting non transactional events.
        /// The function is thread safe and non transactional events may be posted from
        /// another thread.
        /// </summary>
        /// <param name="gameLibEvent">The event to post.</param>
        private void PostNonTransactionalEvent(EventArgs gameLibEvent)
        {
            lock(nonTransactionalEventLocker)
            {
                nonTransactionalFoundationEvents.Add(gameLibEvent);

                // Temp fix to go with event coordinator implementation.
                // TODO: Convert to use a separate Event Source to handle these events.
                eventResetEvent.Set();

                Monitor.Pulse(nonTransactionalEventLocker);
            }
        }

        /// <summary>
        /// Distribute any pending non transactional events.
        /// </summary>
        /// <param name="pace">Number of non transactional events to be processed per function call.</param>
        private void ExecuteNonTransactionalEvents(int pace)
        {
            if(nonTransactionalFoundationEvents.Count > 0 && pace > 0)
            {
                // Get the desired number of events to process.
                var nonTransactionalEventList = nonTransactionalFoundationEvents.GetRange(0, pace);

                // TODO: Convert to use a separate Event Source to handle these events.

                foreach(var nonTransactionalEvent in nonTransactionalEventList)
                {
                    var dispatchedEventArgs = new EventDispatchedEventArgs(nonTransactionalEvent);

                    if(nonTransactionalEventTable.ContainsKey(dispatchedEventArgs.DispatchedEventType))
                    {
                        var handler = nonTransactionalEventTable[dispatchedEventArgs.DispatchedEventType];
                        if(handler != null)
                        {
                            handler(this, dispatchedEventArgs.DispatchedEvent);

                            dispatchedEventArgs.IsHandled = true;
                        }
                    }

                    var dispatchHandler = EventDispatchedEvent;
                    dispatchHandler?.Invoke(this, dispatchedEventArgs);
                }

                // Remove the processed events from the event queue.
                nonTransactionalFoundationEvents.RemoveRange(0, pace);
            }
        }

        /// <summary>
        /// Update the list of persisted events.
        /// </summary>
        private void UpdatePersistedEvents()
        {
            //Filter the events as to not persist certain context events.
            var filteredEvents = (from foundationEvent in foundationEvents
                                  where !eventPersistenceFilter.Contains(foundationEvent.GetType())
                                  select foundationEvent).ToList();
            WriteFoundationData(FoundationDataScope.Theme, FoundationEventsPath, filteredEvents);
        }

        #endregion

        #endregion

        #endregion
    }
}
