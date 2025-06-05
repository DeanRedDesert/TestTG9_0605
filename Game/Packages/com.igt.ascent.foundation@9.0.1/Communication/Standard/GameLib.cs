//-----------------------------------------------------------------------
// <copyright file = "GameLib.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Communication;
    using CompactSerialization;
    using Core.Tracing;
    using F2LLink;
    using F2XLinks;
    using InterfaceExtensions.Interfaces;
    using Tracing;
    using BankStatus = Ascent.Communication.Platform.GameLib.Interfaces.BankStatus;
    using F2LInternal = F2L.Schemas.Internal;

    /// <summary>
    /// Game Lib that talks to the Foundation on behalf of the game.
    /// </summary>
    public sealed class GameLib : IGameLib, IGameLibRestricted, IGameLibCallback, IGameLibShow, IDisposable,
                                  ITransactionVerification, IConfigurationAccessContext,
                                  ITransactionWeightVerificationDependency, ILayeredContextActivationEventsDependency,
                                  ICriticalDataDependency, ICultureInfoDependency, IGameModeQuery
    {
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
        public event EventHandler<DenominationChangeCancelledEventArgs> DenominationChangeCancelledEvent;

        /// <inheritdoc />
        public event EventHandler<DisableAncillaryGameOfferEventArgs> DisableAncillaryGameOfferEvent;

        /// <inheritdoc />
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
                // We have to guarantee the same unconditionally.
                lock(progressiveBroadcastEventLocker)
                {
                    // If this is the first handler wired to the event...
                    if(progressiveBroadcastEvent == null)
                    {
                        // Enable the broadcasting.
                        progressiveBroadcastManager.EnableBroadcasting(true, nameof(ProgressiveBroadcastEvent));
                    }

                    progressiveBroadcastEvent += value;
                }
            }

            remove
            {
                lock(progressiveBroadcastEventLocker)
                {
                    progressiveBroadcastEvent -= value;

                    // If there is no more registered delegates...
                    if(progressiveBroadcastEvent == null)
                    {
                        // Disable the broadcasting.
                        progressiveBroadcastManager.EnableBroadcasting(false, nameof(ProgressiveBroadcastEvent));
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
        /// Create the event lookup table.
        /// </summary>
        private void CreateEventLookupTable()
        {
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

            eventTable[typeof(LanguageChangedEventArgs)] =
                (sender, eventArgs) => ExecuteHandler(sender, eventArgs, LanguageChangedEvent);
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
        }

        #endregion

        #region Fields

        /// <summary>
        /// Object used to communicate with the foundation.
        /// </summary>
        private readonly FoundationLink foundationLink;

        /// <summary>
        /// Object used to manage game initiated transactions and Foundation events.
        /// </summary>
        private readonly TransactionManager transactionManager;

        /// <summary>
        /// Object used to manage Foundation non transactional events.
        /// </summary>
        private readonly NonTransactionalEventManager nonTransactionalEventManager;

        /// <summary>
        /// Object used to coordinating event processing.
        /// </summary>
        private readonly EventCoordinator eventCoordinator;

        /// <summary>
        /// Dictionary of available Game Lib services keyed by their interface type.
        /// </summary>
        private readonly Dictionary<Type, object> builtinServices = new Dictionary<Type, object>();

        /// <summary>
        /// Environment attributes passed down from the Foundation.
        /// </summary>
        /// <devdoc>Initialized in case the attributes are not set.</devdoc>
        private ICollection<EnvironmentAttribute> environmentAttributes = new List<EnvironmentAttribute>();

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Flag indicating if there is an active theme context.
        /// </summary>
        private bool themeContextActive;

        /// <summary>
        /// The progressive broadcast manager controls the time interval to poll the
        /// progressive broadcast data from the Foundation.
        /// </summary>
        private readonly ProgressiveBroadcastManager progressiveBroadcastManager;

        /// <summary>
        /// The time interval between posting the progressive
        /// broadcast event.
        /// </summary>
        public const uint ProgressiveBroadcastInterval = 500;

        /// <summary>
        /// Object which handles caching of critical data.
        /// </summary>
        private readonly CriticalDataCache criticalDataCache = new CriticalDataCache();

        /// <summary>
        /// Flag which indicates that the current game cycle will end with the current transaction.
        /// </summary>
        private bool pendingEndGameCycle;

        /// <summary>
        /// An optional <see cref="IPrepickedValueProvider"/> which is used to set and return values to use
        /// in place of randomly generated values.
        /// </summary>
        private IPrepickedValueProvider prepickedValueProvider;

        /// <summary>
        /// Cache used to store the player meters.
        /// </summary>
        private PlayerMeters? cachedPlayerMeters;

        /// <summary>
        /// Enum used to cache the Game Show Environment.
        /// </summary>
        private ShowEnvironment gameShowEnvironment;

        /// <summary>
        /// Object used to request EGM config data.
        /// </summary>
        private EgmConfigData egmConfigData;

        /// <summary>
        /// The cached available game denominations.
        /// </summary>
        private ICollection<long> availableDenominations;

        /// <summary>
        /// The cached available progressive denominations.
        /// </summary>
        private ICollection<long> availableProgressiveDenominations;

        /// <summary>
        /// Cached value indicating if autoplay is on.
        /// </summary>
        private bool? cachedAutoplayOn;

        /// <summary>
        /// Object used to inquiry the game group control extension.
        /// </summary>
        private IGameGroupControlInquiry gameGroupControlInquiry;

        /// <summary>
        /// The object used to query the configuration items.
        /// </summary>
        private readonly GameConfigurationRead configurationRead;

        /// <summary>
        /// Cached committed bet.
        /// </summary>
        /// <remarks>
        /// The game controls commits to the bet. This means that the committed bet should be read a maximum of 1 time
        /// for a game load. In a load to idle the committed bet doesn't need to be read at all, because no bet
        /// could have been committed. The type is nullable to assist in validating the caching, and potentially other
        /// usage issues (access in non-play modes for instance).
        /// </remarks>
        private long? cachedCommittedBet;

        /// <summary>
        /// Cached game cycle state. This cache expires after the transaction is finalized.
        /// </summary>
        /// <remarks>
        /// All game cycle transitions are associated with either a function call into the foundation or with an event
        /// from the foundation. This allows for the value of the current game cycle state to be managed by the game.
        /// When the game calls a function which transitions the state, it checks for success and then updates its
        /// local state. When a message, indicating a state transition, is sent from the foundation, then it updates
        /// the state correspondingly. This technically means that the transitions are behind the foundation
        /// transitions, but given the state machine and transaction model this should not create any issues. The state
        /// needs to be updated before control is passed back to any game specific code.
        /// </remarks>
        private GameCycleState cachedGameCycleState = GameCycleState.Invalid;

        #endregion

        #region Constants and Constant Equivalents

        private const string GameLibCriticalDataPrefix = "GameLib/";
        private const string WagerCategoryInformationPath = "WagerCategoryInformation";

        // Internal for testing.
        internal static readonly Dictionary<InterfaceExtensionDataScope, CriticalDataScope> MapToCriticalDataScopes =
            new Dictionary<InterfaceExtensionDataScope, CriticalDataScope>
                {
                    { InterfaceExtensionDataScope.GameCycle, CriticalDataScope.GameCycle },
                    { InterfaceExtensionDataScope.Payvar, CriticalDataScope.Payvar },
                    { InterfaceExtensionDataScope.Theme, CriticalDataScope.Theme },
                };

        #endregion

        #region Public Properties

            /// <inheritdoc />
        public string GameMountPoint => foundationLink.GameMountPoint;

            /// <inheritdoc />
        public GameMode GameContextMode { get; private set; }

        /// <inheritdoc />
        public GameSubMode GameSubMode { get; private set; }

        /// <inheritdoc />
        public string PaytableName { get; private set; }

        /// <inheritdoc />
        public string PaytableFileName { get; private set; }

        /// <inheritdoc />
        public long GameDenomination { get; private set; }

        /// <inheritdoc />
        public long DefaultGameDenomination { get; private set; }

        /// <inheritdoc />
        public int MinimumBaseGameTime => PresentationBehaviorConfigs.MinimumBaseGameTime;

        /// <inheritdoc />
        public int MinimumFreeSpinTime => PresentationBehaviorConfigs.MinimumFreeSpinTime;

        /// <inheritdoc />
        public bool IsPlayerWagerOfferable
        {
            get
            {
                MustHaveOpenTransaction();
                return foundationLink.GameControl.IsPlayerWagerOfferable();
            }
        }

        /// <inheritdoc />
        public bool IsCashOutOfferable
        {
            get
            {
                MustHaveOpenTransaction();
                return foundationLink.GameControl.IsPlayerCashoutOfferable();
            }
        }

        /// <inheritdoc />
        public bool IsBankToWagerableOfferable
        {
            get
            {
                MustHaveOpenTransaction();
                return foundationLink.GameControl.IsPlayerBankToWagerableOfferable();
            }
        }

        /// <inheritdoc />
        public ICollection<string> AvailableLanguages { get; private set; }

        /// <inheritdoc />
        public string GameLanguage { get; private set; }

        /// <inheritdoc />
        public string Jurisdiction { get; private set; }

        /// <inheritdoc />
        public bool ShowMode => foundationLink.ShowMode;

        /// <inheritdoc />
        public bool IsThemeSelectionMenuOfferable { get; private set; }

        /// <inheritdoc />
        public ICollection<WagerCategoryOutcome> GameCycleWagerCategoryInfo
        {
            get =>
                ReadCriticalData<List<WagerCategoryOutcome>>(
                    CriticalDataScope.GameCycle,
                    GameLibCriticalDataPrefix + WagerCategoryInformationPath);
            set
            {
                MustHaveOpenTransaction();

                if(value == null)
                {
                    RemoveCriticalData(
                        CriticalDataScope.GameCycle,
                        GameLibCriticalDataPrefix + WagerCategoryInformationPath);
                    return;
                }

                WriteCriticalData(
                    CriticalDataScope.GameCycle,
                    GameLibCriticalDataPrefix + WagerCategoryInformationPath,
                    value.ToList());
            }
        }

        /// <inheritdoc />
        public IExtensionImportCollection ExtensionImportCollection => foundationLink.ExtensionImportCollection;

        /// <inheritdoc />
        public IGameConfigurationRead ConfigurationRead => configurationRead;

        /// <inheritdoc />
        public ILocalizationInformation LocalizationInformation { get; private set; }

        #region Foundation Owned Configuration Items

        private long gameMinBetAmount;
        /// <inheritdoc />
        public long GameMinBetAmount
        {
            get
            {
                Utility.ValidateConfigurationAccess(GameContextMode);
                return gameMinBetAmount;
            }

            private set => gameMinBetAmount = value;
        }

        private long gameMaxBet;
        /// <inheritdoc />
        public long GameMaxBet
        {
            get
            {
                Utility.ValidateConfigurationAccess(GameContextMode);
                return gameMaxBet;
            }

            private set => gameMaxBet = value;
        }

        private long gameMinBet;
        /// <inheritdoc />
        public long GameMinBet
        {
            get
            {
                Utility.ValidateConfigurationAccess(GameContextMode);
                return gameMinBet;
            }

            private set => gameMinBet = value;
        }

        private long buttonPanelMinBet;
        /// <inheritdoc />
        public long ButtonPanelMinBet
        {
            get
            {
                Utility.ValidateConfigurationAccess(GameContextMode);
                return buttonPanelMinBet;
            }

            private set => buttonPanelMinBet = value;
        }

        private uint maxHistorySteps;
        /// <inheritdoc />
        public uint MaxHistorySteps
        {
            get
            {
                Utility.ValidateConfigurationAccess(GameContextMode);
                return maxHistorySteps;
            }

            private set => maxHistorySteps = value;
        }

        /// <inheritdoc />
        public bool AncillaryEnabled { get; private set; }

        /// <inheritdoc />
        public long AncillaryCycleLimit { get; private set; }

        /// <inheritdoc />
        public long AncillaryMonetaryLimit { get; private set; }

        private MaxBetButtonBehavior maxBetButtonBehavior;
        /// <inheritdoc />
        public MaxBetButtonBehavior MaxBetButtonBehavior
        {
            get
            {
                Utility.ValidateConfigurationAccess(GameContextMode);
                return maxBetButtonBehavior;
            }

            private set => maxBetButtonBehavior = value;
        }

        /// <inheritdoc />
        public LineSelectionMode ConfiguredLineSelectionMode { get; private set; }

        /// <inheritdoc />
        public bool RoundWagerUpPlayoffEnabled { get; private set; }

        /// <inheritdoc />
        public bool BonusPlayEnabled { get; private set; }

        /// <inheritdoc/>
        public TopScreenGameAdvertisementType TopScreenGameAdvertisement => PresentationBehaviorConfigs.TopScreenGameAdvertisement;

        /// <inheritdoc/>
        public BetSelectionStyleInfo DefaultBetSelectionStyle => GamePlayBehaviorConfigs.DefaultBetSelectionStyle;

        /// <inheritdoc/>
        public WinCapInformation WinCapInformation { get; private set; }

        /// <inheritdoc />
        public IGamePlayBehaviorConfigs GamePlayBehaviorConfigs { get; private set; }

        /// <inheritdoc />
        public IPresentationBehaviorConfigs PresentationBehaviorConfigs { get; private set; }

        #endregion

        #endregion

        #region Methods

        #region IGameLib Implementation

        #region Persistent Storage, Configuration, Meter, RNG and Service Request Functions

        /// <inheritdoc />
        /// <remarks>
        /// This function uses <see cref="CompactSerializer"/> to deserialize the data if it
        /// is of a type supported by CompactSerializer.  For unsupported data types, the .NET
        /// binary serialization will be used.
        /// </remarks>
        /// <seealso cref="CompactSerializer"/>
        /// <seealso cref="ICompactSerializable"/>
        public T ReadCriticalData<T>(CriticalDataScope scope, CriticalDataName path)
        {
            var returnData = default(T);
            ReadCriticalData(ref returnData, scope, path);
            return returnData;
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
            var readData = ReadRawCriticalData(scope, path);
            var success = false;

            // Only attempt to deserialize it if readData is not null and is not empty.
            // GameControlCategory returns empty byte array if reply.ReadCriticalDataSuccess is false.
            if(readData != null && readData.Length != 0)
            {
                using(var memoryStream = new MemoryStream(readData))
                {
                    if(CompactSerializer.Supports(typeof(T)))
                    {
                        CompactSerializer.Deserialize(ref data, memoryStream);
                    }
                    else
                    {
                        var binaryFormatter = new BinaryFormatter();
                        data = (T)binaryFormatter.Deserialize(memoryStream);
                    }
                    success = true;
                }
            }

            return success;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This function uses <see cref="CompactSerializer"/> to serialize the data if it
        /// is of a type supported by CompactSerializer.  For unsupported data types, the .NET
        /// binary serialization will be used.
        /// </remarks>
        /// <seealso cref="CompactSerializer"/>
        /// <seealso cref="ICompactSerializable"/>
        public void WriteCriticalData(CriticalDataScope scope, CriticalDataName path, object data)
        {
            MustHaveOpenTransaction();

            if(data == null)
            {
                throw new ArgumentNullException(nameof(data), "Null data cannot be written to critical data.");
            }

            // Game state is not cached in Standard Game Lib.
            // Leave the state checking to the Foundation.
            Utility.ValidateCriticalDataAccess(GameContextMode, GameCycleState.Invalid, DataAccessing.Write, scope);

            byte[] serializedData;

            using(var memoryStream = new MemoryStream())
            {
                if(CompactSerializer.Supports(data.GetType()))
                {
                    CompactSerializer.Serialize(memoryStream, data);
                }
                else
                {
                    try
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(memoryStream, data);
                    }
                    catch(SerializationException serializationException)
                    {
                        throw new InvalidSafeStorageTypeException(
                            "Type must be serializable to be written to critical data.", serializationException);
                    }
                }

                serializedData = memoryStream.ToArray();
            }

            //The serialized data will not be null at this point. If there was a problem with serialization, then an
            //exception will have been thrown.
            if(criticalDataCache.IsCachedScope(scope))
            {
                criticalDataCache.WriteData(scope, path, serializedData);
            }
            else
            {
                foundationLink.GameControl.WriteCriticalData(scope.ToF2LCriticalDataScope(),
                                                             path,
                                                             serializedData);
            }
        }

        /// <inheritdoc />
        public bool RemoveCriticalData(CriticalDataScope scope, CriticalDataName path)
        {
            MustHaveOpenTransaction();

            // Game state is not cached in Standard Game Lib.
            // Leave the state checking to the Foundation.
            Utility.ValidateCriticalDataAccess(GameContextMode, GameCycleState.Invalid, DataAccessing.Remove, scope);

            criticalDataCache.RemoveCachedData(scope, path);
            return foundationLink.GameControl.RemoveCriticalData(scope.ToF2LCriticalDataScope(), path);
        }

        /// <inheritdoc />
        public ICollection<int> GetRandomNumbers(RandomValueRequest request)
        {
            if(request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if(prepickedValueProvider != null)
            {
                // If a pre-picked value provider is available, try getting the values from it first.
                var prepickResult = prepickedValueProvider.GetPrepickedValues(request);

                // If all of the values were provided, return them here.
                if(prepickResult.PrepickedValues != null)
                {
                    return prepickResult.PrepickedValues;
                }

                // Use the new request created by the provider.
                request = prepickResult.NewRandomValueRequest;
            }

            return GetRandomNumbersFromFoundation(request);
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method sends out different F2L messages based on the request list
        /// to optimize the number of F2L messages sent as well as the Foundation's
        /// performance of generating the random numbers.
        /// When there is no <see cref="IPrepickedValueProvider"/> configured for
        /// the game lib, the behavior of this method is described as following:
        /// <list type="bullet">
        /// <item>
        /// If <paramref name="requestList"/> has only one element,
        /// the method sends one F2L message asking for a single RNG request.
        /// </item>
        /// <item>
        /// If <paramref name="requestList"/> has multiple elements,
        /// each for a single random number in the same range,
        /// the method sends one F2L message asking for a single RNG request.
        /// </item>
        /// <item>
        /// If <paramref name="requestList"/> has multiple elements,
        /// each for a single random number in different ranges,
        /// the method sends one F2L message asking for multiple RGN requests,
        /// each for one element in <paramref name="requestList"/>.
        /// </item>
        /// <item>
        /// If <paramref name="requestList"/> has multiple elements,
        /// some of which request for multiple random numbers,
        /// the method sends multiple F2L messages,
        /// each for one element in <paramref name="requestList"/>.
        /// Each F2L message asks for a single RGN request.
        /// </item>
        /// </list>
        /// </remarks>
        public ICollection<int> GetRandomNumbers(ICollection<RandomValueRequest> requestList)
        {
            if(requestList == null)
            {
                throw new ArgumentNullException(nameof(requestList));
            }

            var randomNumbers = new List<int>();

            if(prepickedValueProvider != null)
            {
                // Get the pre-picked values for each request.  Each result contains either the values for the request or a new
                //  request that may be forwarded to the RNG.
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

            //Check the simple case and make an optimized request if it is true. There is room for further optimization,
            //but this should handle the most common cases.

            //TODO: Optimize for requests which are not singles, but do have shared ranges.

            //Do not optimize for singles if only 1 item is requested. The API for shared ranges is faster.
            var singles = requestList.Count != 1 && requestList.All(request => request.Count == 1);

            if(singles)
            {
                randomNumbers.AddRange(GetRandomNumbersMultipleSingleRanges(requestList));
            }
            else
            {
                foreach(var request in requestList)
                {
                    randomNumbers.AddRange(GetRandomNumbersFromFoundation(request));
                }
            }

            return randomNumbers;
        }

        /// <summary>
        /// Gets random numbers from the foundation using the parameters specified in the request.
        /// </summary>
        /// <param name="request">The <see cref="RandomValueRequest"/> object.</param>
        /// <returns>A collection of values from the foundation that satisfy the request.</returns>
        /// <remarks>This method adapts a <see cref="RandomValueRequest"/> to the foundation's interface.</remarks>
        private ICollection<int> GetRandomNumbersFromFoundation(RandomValueRequest request)
        {
            var prepicks = request.PrePickedNumbers != null ? request.PrePickedNumbers.ToList() : new List<int>();

            return foundationLink.GameControl.GetRandomNumbers(request.Count, request.RangeMin, request.RangeMax,
                                                               new ReadOnlyCollection<int>(prepicks),
                                                               request.MaxDuplicates, request.GeneratorName);
        }

        /// <summary>
        /// Handle requests for multiple numbers with each number having its own unique range.
        /// </summary>
        /// <param name="requestList">List of requests each requesting a single number.</param>
        /// <returns>List of random numbers with the requested ranges.</returns>
        private IEnumerable<int> GetRandomNumbersMultipleSingleRanges(ICollection<RandomValueRequest> requestList)
        {
            var count = (uint)requestList.Count;
            var name = requestList.ElementAt(0).GeneratorName;

            var firstHigh = requestList.ElementAt(0).RangeMax;
            var firstLow = requestList.ElementAt(0).RangeMin;
            var sharedRanges =
                requestList.All(request => request.RangeMax == firstHigh && request.RangeMin == firstLow);

            if(sharedRanges)
            {
                return foundationLink.GameControl.GetRandomNumbers(count, firstLow, firstHigh,
                                                                   new ReadOnlyCollection<int>(new List<int>()),
                                                                   count, name);
            }

            //The requests are singles, so the number of requests is the total.
            var highRanges = new List<int>();
            var lowRanges = new List<int>();

            foreach(var request in requestList)
            {
                highRanges.Add(request.RangeMax);
                lowRanges.Add(request.RangeMin);
            }

            return foundationLink.GameControl.GetRandomNumbers(count, new ReadOnlyCollection<int>(lowRanges),
                                                               new ReadOnlyCollection<int>(highRanges), name);
        }

        /// <inheritdoc />
        public bool RequestDenominationChange(long newDenomination)
        {
            MustHaveOpenTransaction();

            if(newDenomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newDenomination), "Denomination must be greater than 0");
            }

            return foundationLink.GameControl.RequestDenominationChange(newDenomination);
        }

        /// <inheritdoc />
        public bool RequestThemeSelectionMenu()
        {
            MustHaveOpenTransaction();

            var messageSent = false;
            if(GameContextMode == GameMode.Play &&
               QueryGameCycleState() == GameCycleState.Idle &&
               IsThemeSelectionMenuOfferable)
            {
                foundationLink.GameControl.RequestThemeSelectionMenu();
                messageSent = true;
            }
            return messageSent;
        }

        /// <inheritdoc />
        public void RequestServiceWindow()
        {
            MustHaveOpenTransaction();

            // Not defined yet.
        }

        /// <inheritdoc />
        public ICollection<long> GetAvailableDenominations()
        {
            MustHaveOpenTransaction();

            return availableDenominations ?? foundationLink.GameControl.GetAvailableDenominations();
        }

        /// <inheritdoc />
        public ICollection<long> GetAvailableProgressiveDenominations()
        {
            MustHaveOpenTransaction();

            return availableProgressiveDenominations ?? foundationLink.GameControl.GetAvailableProgressiveDenominations();
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

            var foundationProgressives = new Dictionary<int, ProgressiveBroadcastData>();
            var level = foundationLink.GameControl.GetProgressiveLevels(denom);

            if(level != null)
            {
                // Convert the progressive level data.
                foreach(var item in level)
                {
                    foundationProgressives.Add((int)item.GameLevel, new ProgressiveBroadcastData(item.Amount, item.PrizeString));
                }
            }

            return foundationProgressives;
        }

        /// <inheritdoc />
        public void SetLanguage(string newLanguage)
        {
            MustHaveOpenTransaction();

            if(!AvailableLanguages.Contains(newLanguage))
            {
                throw new InvalidOperationException(newLanguage + " is not enabled.");
            }

            if(GameLanguage != newLanguage)
            {
                foundationLink.GameControl.SetCulture(newLanguage);
                GameLanguage = newLanguage;
            }
        }

        /// <inheritdoc />
        public string SetDefaultLanguage()
        {
            MustHaveOpenTransaction();

            return foundationLink.GameControl.SetDefaultCulture();
        }

        /// <inheritdoc />
        public DateTime GetDateTime()
        {
            MustHaveOpenTransaction();

            // Not defined yet.

            return DateTime.Now;
        }

        /// <inheritdoc />
        public CreditMeterDisplayBehaviorMode GetCreditMeterBehavior()
        {
            return PresentationBehaviorConfigs.CreditMeterBehavior;
        }

        #endregion

        #region Game Cycle Functions

        /// <inheritdoc />
        public GameCycleState QueryGameCycleState()
        {
            MustHaveOpenTransaction();

            //If the game cycle state is its default value, then we need to read the state from the foundation.
            //In normal operation this should only occur on context activation after loading the game.
            if(cachedGameCycleState == GameCycleState.Invalid)
            {
                cachedGameCycleState = foundationLink.GameControl.QueryGameCycleState().ToPublic();
            }
            return cachedGameCycleState;
        }

        /// <inheritdoc />
        public bool CanCommitGameCycle()
        {
            MustHaveOpenTransaction();

            return foundationLink.GameControl.CanCommitGameCycle();
        }

        /// <inheritdoc />
        public bool CommitGameCycle()
        {
            MustHaveOpenTransaction();

            var result = foundationLink.GameControl.CommitGameCycle();

            if(result)
            {
                cachedGameCycleState = GameCycleState.Committed;
                GameLibTracing.Log.CommitGameCycleSuccess();
            }

            return result;
        }

        /// <inheritdoc />
        public void UncommitGameCycle()
        {
            MustHaveOpenTransaction();

            pendingEndGameCycle = true;

            foundationLink.GameControl.UncommitGameCycle();
            cachedGameCycleState = GameCycleState.Idle;
        }

        /// <inheritdoc />
        public void EnrollGameCycle(byte[] enrollmentData)
        {
            MustHaveOpenTransaction();

            foundationLink.GameControl.EnrollGameCycle(enrollmentData);
            cachedGameCycleState = GameCycleState.EnrollPending;
        }

        /// <inheritdoc />
        public void UnenrollGameCycle()
        {
            MustHaveOpenTransaction();

            pendingEndGameCycle = true;

            foundationLink.GameControl.UnenrollGameCycle();
            cachedGameCycleState = GameCycleState.Idle;
        }

        /// <inheritdoc />
        public bool CanStartPlaying()
        {
            MustHaveOpenTransaction();

            return foundationLink.GameControl.CanStartPlaying();
        }

        /// <inheritdoc />
        public bool StartPlaying()
        {
            MustHaveOpenTransaction();

            var playing = foundationLink.GameControl.StartPlaying();
            if(playing)
            {
                cachedCommittedBet = 0;
                cachedGameCycleState = GameCycleState.Playing;
            }
            return playing;
        }

        /// <inheritdoc />
        public void EndGameCycle(uint historySteps)
        {
            MustHaveOpenTransaction();

            pendingEndGameCycle = true;

            foundationLink.GameControl.EndGameCycle(historySteps);
            cachedGameCycleState = GameCycleState.Idle;

            GameLibTracing.Log.EndGameCycle();
        }

        /// <inheritdoc />
        public bool OfferAncillaryGame()
        {
            MustHaveOpenTransaction();

            return foundationLink.GameControl.OfferAncillaryGame();
        }

        /// <inheritdoc />
        public bool StartAncillaryPlaying()
        {
            MustHaveOpenTransaction();

            var ancillaryPlaying = foundationLink.GameControl.StartAncillaryPlaying();

            if(ancillaryPlaying)
            {
                cachedGameCycleState = GameCycleState.AncillaryPlaying;
            }
            return ancillaryPlaying;
        }

        /// <inheritdoc />
        public bool StartBonusPlaying()
        {
            MustHaveOpenTransaction();

            var bonusPlaying = foundationLink.GameControl.StartBonusPlaying();
            if(bonusPlaying)
            {
                cachedGameCycleState = GameCycleState.BonusPlaying;
            }
            return bonusPlaying;
        }

        /// <inheritdoc />
        public void AdjustOutcome(IOutcomeList outcome, bool isFinalOutcome)
        {
            if(outcome == null)
            {
                throw new ArgumentNullException(nameof(outcome), "Parameter may not be null.");
            }

            MustHaveOpenTransaction();

            var gameCycleState = QueryGameCycleState();

            if(isFinalOutcome)
            {
                switch(gameCycleState)
                {
                    case GameCycleState.Playing:
                        {
                            var wagerCategoryInformation = GameCycleWagerCategoryInfo;
                            if(wagerCategoryInformation == null || wagerCategoryInformation.Count == 0)
                            {
                                throw new InvalidOperationException(
                                    "There must be at least 1 wager category present before adjusting the final game outcome.");
                            }

                            AdjustFinalOutcome(outcome, wagerCategoryInformation);
                            break;
                        }
                    case GameCycleState.AncillaryPlaying:
                        {
                            foundationLink.GameControl.LastEvalAncillaryOutcomeRequest(outcome);
                            break;
                        }
                    case GameCycleState.BonusPlaying:
                        {
                            foundationLink.GameControl.LastEvalBonusOutcomeRequest(outcome);
                            break;
                        }
                    default:
                        {
                            //If any of the other adjust methods are called, then the Foundation can return an error code if
                            //the call is made in the wrong state. If we are not in the correct state for the final outcome,
                            //then the Foundation will not be called, so we must throw an exception.
                            throw new FunctionCallNotAllowedInModeOrStateException(
                                "AdjustOutcome is not allowed in the current state.", GameContextMode, gameCycleState);
                        }

                }
            }
            else
            {
                //Adjust a non-final outcome.
                foundationLink.GameControl.EvalOutcomeRequest(outcome);
            }

            //Transition to the correct cached game cycle state. This is done after any foundation calls which will
            //result in a transition as well as any error handling for calling in an invalid state or mode.
            switch(gameCycleState)
            {
                case GameCycleState.Playing:
                    {
                        cachedGameCycleState = GameCycleState.EvaluatePending;
                        break;
                    }
                case GameCycleState.AncillaryPlaying:
                    {
                        cachedGameCycleState = GameCycleState.AncillaryEvaluatePending;
                        break;
                    }
                case GameCycleState.BonusPlaying:
                    {
                        cachedGameCycleState = GameCycleState.BonusEvaluatePending;
                        break;
                    }
                default:
                    {
                        //The above game cycle states are the only ones in which this method call is valid. If this
                        //error is encountered, then it is either because the game cycle is being changed to allow
                        //adjustment from a new state, or that the foundation did not generate an error for an outcome
                        //call in an invalid state.
                        throw new InvalidOperationException(
                            "Cached game cycle state is not consistent. Outcome adjusted in an invalid game cycle state.");
                        //This is not using FunctionCallNotAllowedInModeOrStateException, because that typically denotes
                        //a situation in which a client is incorrectly using a game cycle state method. In this case
                        //the error is with the SDK or Foundation.
                    }
            }
        }

        /// <inheritdoc />
        public void FinalizeOutcome()
        {
            MustHaveOpenTransaction();

            foundationLink.GameControl.FinalizeAwardRequest();
            cachedGameCycleState = GameCycleState.FinalizeAwardPending;
        }

        #endregion

        #region Betting Functions

        /// <inheritdoc />
        public bool CanCommitBet(long bet, long denomination)
        {
            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            MustHaveOpenTransaction();

            return foundationLink.GameControl.CanCommitBet(Utility.ConvertToCents(bet, denomination));
        }

        /// <inheritdoc />
        public IEnumerable<bool> CanCommitBets(IEnumerable<long> bets, long denomination)
        {
            if(bets == null)
            {
                throw new ArgumentNullException(nameof(bets), "The list of bets should not be null.");
            }

            bets = bets.ToList();

            if(!bets.Any())
            {
                throw new ArgumentException("The list of bets should not be empty.", nameof(bets));
            }
            if(bets.Any(bet => bet < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bets), "Bet cannot be less than 0.");
            }
            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            MustHaveOpenTransaction();

            return foundationLink.GameControl.CanCommitBets(
                        bets.Select(betAmount => Utility.ConvertToCents(betAmount, denomination)));
        }

        /// <inheritdoc />
        public bool CommitBet(long bet, long denomination)
        {
            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            MustHaveOpenTransaction();

            var committed = foundationLink.GameControl.CommitBet(Utility.ConvertToCents(bet, denomination));
            if(committed)
            {
                cachedCommittedBet = Utility.ConvertToCents(bet, denomination);
            }
            return committed;
        }

        /// <inheritdoc />
        public void UncommitBet()
        {
            MustHaveOpenTransaction();

            foundationLink.GameControl.UncommitBet();
            cachedCommittedBet = 0;
        }

        /// <inheritdoc />
        public void GetCommittedBet(out long bet, out long denomination)
        {
            MustHaveOpenTransaction();

            denomination = GameDenomination;

            if(cachedCommittedBet != null)
            {
                bet = Utility.ConvertToCredits(cachedCommittedBet.Value, denomination);
            }
            else
            {
                // This should not happen as the committed bet is either set to 0, or read from the foundation, during
                // context activation. If it does happen, then it represents a defect in the caching logic for the
                // committed bet. This would most likely happen if this method was called in a mode other than play.
                // This does not do a foundation read, because that would mask such a defect.
                throw new InvalidOperationException("A cached committed bet value was not available.");
            }
        }

        /// <inheritdoc />
        public void PlaceStartingBet()
        {
            MustHaveOpenTransaction();

            var payvarMaxBet = GameMaxBet;

            // If Game Group is present, use payvar's max bet instead of overall group template's max bet.
            if(gameGroupControlInquiry?.IsPaytableGroupPresent() == true)
            {
                payvarMaxBet = gameGroupControlInquiry.GetCurrentRedefinedMaxBetCredits();
            }

            var isMax = cachedCommittedBet ==
                        Utility.ConvertToCents(payvarMaxBet, GameDenomination);
            foundationLink.GameControl.PlaceStartingBet(isMax);
        }

        /// <inheritdoc />
        public bool CanPlaceBet(long bet, long denomination)
        {
            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            MustHaveOpenTransaction();

            return CanPlaceBetAgainstPendingWins(bet, bet, 0, 0, denomination);
        }

        /// <inheritdoc />
        public bool CanPlaceBetAgainstPendingWins(long bet, long betFromCredits, long betFromPendingWins,
                                                  long availablePendingWins, long denomination)
        {
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

            if(bet != betFromCredits + betFromPendingWins)
            {
                throw new ArgumentException("Bet amounts don't add up.");
            }

            if(betFromPendingWins > availablePendingWins)
            {
                throw new ArgumentException("Bet from pending wins exceeds available pending wins.");
            }

            MustHaveOpenTransaction();

            return foundationLink.GameControl.CanPlaceBet(Utility.ConvertToCents(bet, denomination),
                                                          Utility.ConvertToCents(betFromCredits, denomination),
                                                          Utility.ConvertToCents(betFromPendingWins, denomination),
                                                          Utility.ConvertToCents(availablePendingWins, denomination));
        }

        /// <inheritdoc />
        public bool PlaceBet(long bet, long denomination)
        {
            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            MustHaveOpenTransaction();

            return PlaceBetAgainstPendingWins(bet, bet, 0, 0, denomination);
        }

        /// <inheritdoc />
        public bool PlaceBetAgainstPendingWins(long bet, long betFromCredits, long betFromPendingWins,
                                               long availablePendingWins, long denomination)
        {
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

            if(bet != betFromCredits + betFromPendingWins)
            {
                throw new ArgumentException("Bet amounts don't add up.");
            }

            if(betFromPendingWins > availablePendingWins)
            {
                throw new ArgumentException("Bet from pending wins exceeds available pending wins.");
            }

            MustHaveOpenTransaction();

            return foundationLink.GameControl.PlaceBet(Utility.ConvertToCents(bet, denomination),
                                                       Utility.ConvertToCents(betFromCredits, denomination),
                                                       Utility.ConvertToCents(betFromPendingWins, denomination),
                                                       Utility.ConvertToCents(availablePendingWins, denomination));
        }

        /// <inheritdoc />
        public IEnumerable<bool> CanBetNextGameCycle(IEnumerable<long> bets, long denomination)
        {
            if(bets == null)
            {
                throw new ArgumentNullException(nameof(bets), "The list of bets should not be null.");
            }

            bets = bets.ToList();

            if(!bets.Any())
            {
                throw new ArgumentException("The list of bets should not be empty.", nameof(bets));
            }
            if(bets.Any(bet => bet < 0))
            {
                throw new ArgumentOutOfRangeException(nameof(bets), "Bet cannot be less than 0.");
            }
            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            MustHaveOpenTransaction();
            return foundationLink.GameControl.CanBetNextGameCycle(
                        bets.Select(betAmount => Utility.ConvertToCents(betAmount, denomination)));
        }

        /// <inheritdoc />
        public bool CanBetNextGameCycle(long bet, long denomination)
        {
            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }
            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            MustHaveOpenTransaction();
            return foundationLink.GameControl.CanBetNextGameCycle(
                new List<long> { Utility.ConvertToCents(bet, denomination) }).ToList().First();
        }

        #endregion

        #region Money Functions

        /// <inheritdoc />
        public PlayerMeters GetPlayerMeters()
        {
            // We cache the PlayerMeters upon context activation, so most likely
            // cachedPlayerMeters is not null by the time this API is being called.
            if(cachedPlayerMeters == null)
            {
                MustHaveOpenTransaction();
                cachedPlayerMeters = foundationLink.GameControl.GetPlayerMeters().ToPublic();
            }

            return cachedPlayerMeters.Value;
        }

        /// <inheritdoc />
        public BankStatus QueryBankStatus()
        {
            MustHaveOpenTransaction();

            // F2L message keeps the name of BankLocked for sake of 
            // backward compatibility. It really should have been named IsPlayerWagerOfferable.
            var status = foundationLink.GameControl.QueryBankStatus();

            return new BankStatus(!status.BankLocked, status.PlayerCashoutOfferable, status.BankToWagerableOfferable);
        }

        /// <inheritdoc />
        public void RequestCashOut()
        {
            MustHaveOpenTransaction();

            if(foundationLink.GameControl.IsPlayerCashoutOfferable())
            {
                foundationLink.GameControl.PlayerCashoutRequest();
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
                if(IsBankToWagerableOfferable && IsPlayerWagerOfferable)
                {
                    foundationLink.GameControl.TransferBankToWagerableRequest();
                    result = true;
                }
            }
            // Prepare for cashout.
            else if(from == MoneyLocation.PlayerWagerableMeter && to == MoneyLocation.PlayerBankMeter)
            {
                result = foundationLink.GameControl.TransferWagerableToBankRequest();
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
        public bool IsEnvironmentTrue(EnvironmentAttribute attribute)
        {
            return environmentAttributes.Contains(attribute);
        }

        #endregion

        #region Auto Play

        /// <inheritdoc />
        public bool IsPlayerAutoPlayEnabled { get; private set; }

        /// <inheritdoc />
        public bool IsAutoPlayConfirmationRequired { get; private set; }

        /// <inheritdoc />
        public bool? IsAutoPlaySpeedIncreaseAllowed { get; private set; }

        /// <inheritdoc />
        public bool IsAutoPlayOn()
        {
            if(cachedAutoplayOn == null)
            {
                cachedAutoplayOn = foundationLink.AutoPlay.IsAutoPlayOn();
            }
            return cachedAutoplayOn.Value;
        }

        /// <inheritdoc />
        public bool SetAutoPlayOn()
        {
            cachedAutoplayOn = foundationLink.AutoPlay.SetAutoPlayOnRequest();
            return cachedAutoplayOn.Value;
        }

        /// <inheritdoc />
        public void SetAutoPlayOff()
        {
            foundationLink.AutoPlay.SetAutoPlayOff();
            cachedAutoplayOn = false;
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
            foundationLink.GameStopReport.ReportReelStops(physicalReelStops);
        }

        #endregion

        #region Extended Interface Handling

        /// <inheritdoc/>
        public TExtendedInterface GetInterface<TExtendedInterface>() where TExtendedInterface : class
        {
            return foundationLink.GetInterface<TExtendedInterface>();
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
        public bool ConnectToFoundation(IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaceConfigurations)
        {
            var result = false;

            if(foundationLink.Connect(additionalInterfaceConfigurations))
            {
                InitializeEgmConfigData();
                InitializeLocalizationInformation();
                InitializeConfigurationRead();
                // Initialize inquiry interfaces.
                gameGroupControlInquiry = additionalInterfaceConfigurations != null
                                              ? GetInterface<IGameGroupControlInquiry>()
                                              : null;
                result = true;
            }

            GameLifeCycleTracing.Log.GameLibConnected(result);

            return result;
        }

        /// <inheritdoc />
        public bool DisconnectFromFoundation()
        {
            foundationLink.Disconnect();
            //TODO: Can the disconnect really fail?
            return true;
        }

        /// <inheritdoc />
        public ErrorCode CreateTransaction()
        {
            return CreateTransaction(string.Empty);
        }

        /// <inheritdoc />
        public ErrorCode CreateTransaction(string name)
        {
            //If we are not in context, then a transaction may not be opened.
            if(!themeContextActive)
            {
                return ErrorCode.GeneralError;
            }

            return transactionManager.CreateTransaction(name);
        }

        /// <inheritdoc />
        public ErrorCode CloseTransaction()
        {
            return transactionManager.CloseTransaction();
        }

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

        /// <inheritdoc/>
        void IGameLibRestricted.SetPrepickedValueProvider(IPrepickedValueProvider providerToUse)
        {
            SetPrepickedValueProvider(providerToUse);
        }

        /// <inheritdoc cref="IGameLibRestricted"/>
        /// <remarks>This method does nothing in a release build.</remarks>
        [Conditional("DEBUG")]
        private void SetPrepickedValueProvider(IPrepickedValueProvider providerToUse)
        {
            prepickedValueProvider = providerToUse;
        }

        /// <inheritdoc/>
        public TServiceInterface GetServiceInterface<TServiceInterface>() where TServiceInterface : class
        {
            return builtinServices.ContainsKey(typeof(TServiceInterface))
                       ? builtinServices[typeof(TServiceInterface)] as TServiceInterface
                       : null;
        }

        /// <inheritdoc />
        public bool TransactionOpen => transactionManager.GameTransactionOpen;

        /// <inheritdoc />
        public string Token => foundationLink.Token;

        #endregion

        #region IGameLibCallback Implementation

        /// <inheritdoc />
        void IGameLibCallback.SetEnvironmentAttributes(ICollection<EnvironmentAttribute> attributeList)
        {
            environmentAttributes = attributeList ?? throw new ArgumentNullException(nameof(attributeList));
        }

        /// <inheritdoc />
        void IGameLibCallback.SetJurisdiction(string jurisdiction)
        {
            Jurisdiction = jurisdiction;
        }

        /// <inheritdoc />
        void IGameLibCallback.ShutDownProcess()
        {
            if(eventTable.ContainsKey(typeof(ShutDownEventArgs)))
            {
                var eventHandlerDelegate = eventTable[typeof(ShutDownEventArgs)];
                eventHandlerDelegate?.Invoke(this, new ShutDownEventArgs());
            }
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

            foundationLink.ShowControl.AddMoney(value, denomination);
        }


        /// <inheritdoc />
        public ShowEnvironment GetShowEnvironment()
        {
            return gameShowEnvironment;
        }

        #endregion

        #region ITransactionVerification Implementation

        /// <inheritdoc/>
        void ITransactionVerification.MustHaveOpenTransaction()
        {
            MustHaveOpenTransaction();
        }

        #endregion

        #region IConfigurationAccessContext Implementation

        /// <inheritdoc />
        public bool IsConfigurationScopeIdentifierRequired => false;

        /// <inheritdoc />
        public void ValidateConfigurationAccess(ConfigurationScope configurationScope)
        {
            Utility.ValidateConfigurationAccess(GameContextMode);
        }

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

        /// <inheritdoc />
        void ICriticalDataDependency.WriteCriticalData<TData>(InterfaceExtensionDataScope scope, string path, TData data)
        {
            WriteCriticalData(MapToCriticalDataScopes[scope], path, data);
        }

        /// <inheritdoc />
        TData ICriticalDataDependency.ReadCriticalData<TData>(InterfaceExtensionDataScope scope, string path)
        {
            return ReadCriticalData<TData>(MapToCriticalDataScopes[scope], path);
        }

        #endregion

        #region ICultureInfoDependency Implementation

        /// <inheritdoc />
        ICollection<string> ICultureInfoDependency.AvailableCultures => AvailableLanguages;

        #endregion

        #region IGameModeQuery Implementation

        /// <inheritdoc />
        GameMode IGameModeQuery.GameMode => GameContextMode;

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Implement IDisposable. Do not make this method
        /// virtual.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
                    // Disconnect from Foundation.
                    // Avoid throwing exceptions when disposing.
                    try
                    {
                        DisconnectFromFoundation();
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }

                    // Dispose internal managers.
                    eventCoordinator.UnregisterEventSource(nonTransactionalEventManager);
                    lock(progressiveBroadcastEventLocker)
                    {
                        eventCoordinator.UnregisterEventSource(progressiveBroadcastManager);
                    }
                    transactionManager.Dispose();
                    nonTransactionalEventManager.Dispose();
                    // ReSharper disable once InconsistentlySynchronizedField
                    progressiveBroadcastManager.Dispose();

                    // Dispose the Foundation Link.
                    if(foundationLink is IDisposable disposableLink)
                    {
                        disposableLink.Dispose();
                    }
                }

                disposed = true;
            }
        }

        #endregion

        #region Constructors and Finalizers

        /// <summary>
        /// Create an instance of <see cref="GameLib"/> for unit testing.
        /// </summary>
        /// <param name="foundationLink">
        /// The <see cref="FoundationLink"/> that provides a communication link with the Foundation.
        /// </param>
        /// <param name="transactionManager">
        /// The <see cref="TransactionManager"/> that manages transactions.
        /// </param>
        /// <DevDoc>
        /// This constructor should only be used for testing.
        /// </DevDoc>
        internal GameLib(FoundationLink foundationLink, TransactionManager transactionManager)
        {
            this.foundationLink = foundationLink;
            this.transactionManager = transactionManager;

            GameContextMode = GameMode.Play;

            transactionManager.EventDispatchedEvent += HandleEventDispatched;
            transactionManager.FinalizeTransactionEvent += HandleFinalizeTransaction;
        }

        /// <summary>
        /// Create a Game Lib based on the command line options
        /// passed down from the Foundation.
        /// </summary>
        /// <param name="foundationTarget">The foundation version to target.</param>
        public GameLib(FoundationTarget foundationTarget)
        {
            var transportType = "IPC";
            var foundationAddress = string.Empty;
            ushort foundationPort = 0;

            var flagArgument = CommandLineArguments.Environment.GetValue("s");
            if(flagArgument != null)
            {
                transportType = "SOCKET";

                var flagArgValues = flagArgument.Split(':');
                foundationAddress = flagArgValues[0];

                if(flagArgValues.Length > 1)
                {
                    foundationPort = ushort.TryParse(flagArgValues[1], out var k) ? k : (ushort)0;
                }
            }

            if(foundationTarget == FoundationTarget.UniversalController || foundationTarget == FoundationTarget.UniversalController2)
            {
                // The Universal Controller foundation doesn't provide the address and port as command line arguments
                // for the socket connection to the game, therefore these variables need to be set here.
                transportType = "SOCKET";
                foundationAddress = "localhost";
                foundationPort = 35200;
            }

            if(transportType != "SOCKET")
            {
                throw new InvalidOperationException(
                    $"{transportType} communication is not supported!  Only SOCKET type is.");
            }

            var baseExtensionDependencies = new InterfaceExtensionDependencies
            {
                TransactionWeightVerification = this,
                CriticalDataProvider = this,
                LayeredContextActivationEvents = this,
                CultureInfoProvider = this,
                GameModeQuery = this,
            };

            // Create the non transactional event manager.
            nonTransactionalEventManager = new NonTransactionalEventManager();
            baseExtensionDependencies.NonTransactionalEventDispatcher = nonTransactionalEventManager;

            // Create the foundation link.
            foundationLink = new FoundationLink(this, baseExtensionDependencies,
                                                foundationAddress, foundationPort, foundationTarget,
                                                nonTransactionalEventManager);

            CreateEventLookupTable();

            // Create the transaction manager.
            transactionManager = new TransactionManager(foundationLink, foundationLink.TransportExceptionMonitor);
            builtinServices[typeof(ITransactionAugmenter)] = transactionManager;
            baseExtensionDependencies.TransactionalEventDispatcher = transactionManager;

            transactionManager.EventDispatchedEvent += HandleEventDispatched;
            transactionManager.FinalizeTransactionEvent += HandleFinalizeTransaction;

            // Create the event coordinator.
            eventCoordinator = new EventCoordinator(transactionManager, foundationLink.TransportExceptionMonitor);
            builtinServices[typeof(IEventCoordinator)] = eventCoordinator;
            builtinServices[typeof(IEventProcessing)] = eventCoordinator;

            // Register other event sources with the event coordinator.
            eventCoordinator.RegisterEventSource(nonTransactionalEventManager);

            // Cache some foundation data upon entering a new theme context.
            ActivateThemeContextEvent += HandleActivateThemeContextEvent;
            OutcomeResponseEvent += HandleOutcomeResponseEvent;
            EnrollResponseEvent += HandleEnrollResponseEvent;
            FinalizeOutcomeEvent += HandleFinalizeOutcomeEvent;
            InactivateThemeContextEvent += HandleInactivateThemeContextEvent;
            SwitchThemeContextEvent += HandleSwitchThemeContextEvent;

            //We handle the autoplay on request and off event to update the cached value.
            AutoPlayOffEvent += HandleAutoPlayOffEvent;
            AutoPlayOnRequestEvent += HandleAutoPlayOnRequestEvent;

            //Cache the player meters on all money events.
            MoneyEvent += HandleMoneyEvent;

            //Cache if the theme selection menu offer-able.
            ThemeSelectionMenuOfferableStatusChangedEvent += HandleThemeSelectionMenuOfferableStatusChangedEvent;

            //Cache the current language on language changes.
            LanguageChangedEvent += HandleLanguageChangedEvent;

            // Create the progressive broadcast manager.
            progressiveBroadcastManager = new ProgressiveBroadcastManager(ProgressiveBroadcastInterval);
            progressiveBroadcastManager.ProgressiveBroadcastEvent += BroadcastProgressiveData;
            eventCoordinator.RegisterEventSource(progressiveBroadcastManager);

            // Create interface implementations.
            configurationRead = new GameConfigurationRead(this, this);
            GamePlayBehaviorConfigs = new GamePlayBehaviorConfigs { DefaultBetSelectionStyle = new BetSelectionStyleInfo() };
            PresentationBehaviorConfigs = new PresentationBehaviorConfigs();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Allow the foundation to adjust the outcome, causing the Foundation
        /// to transition from the Playing state to the Evaluate Pending state.
        /// When the foundation is finished adjusting the outcome,
        /// a OutcomeResponseEvent will be posted.
        /// </summary>
        /// <param name="outcome">The outcome to adjust.</param>
        /// <param name="wagerCategoryOutcomeList">
        /// Wager category outcomes associated with the current game cycle.
        /// </param>
        private void AdjustFinalOutcome(IOutcomeList outcome, IEnumerable<WagerCategoryOutcome> wagerCategoryOutcomeList)
        {
            var convertedCategories = from category in wagerCategoryOutcomeList
                                      select
                                          new F2LInternal.WagerCatOutcome
                                              {
                                                  WagerAmount = Utility.ConvertToCents(category.WagerAmount, category.Denomination),
                                                  WagerCatIndex = category.CategoryIndex
                                              };

            foundationLink.GameControl.LastEvalOutcomeRequest(outcome,
                                                              new ReadOnlyCollection<F2LInternal.WagerCatOutcome>(
                                                                  convertedCategories.ToList()));
        }

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
        /// Autoplay is no longer on, as this is not a request, so update the cached value.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="autoPlayOffEventArgs">The event data.</param>
        private void HandleAutoPlayOffEvent(object sender, AutoPlayOffEventArgs autoPlayOffEventArgs)
        {
            cachedAutoplayOn = false;
        }

        /// <summary>
        /// Autoplay is being asked to start by the Foundation, update cached value to null since we do not know if
        /// AutoPlay will be told to start at this point. Setting the flag to null will force update the flag the
        /// next time <see cref="IsAutoPlayOn"/> is called.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="autoPlayOnRequestEventArgs">The event data.</param>
        /// <remarks>
        /// The subscribed event handler determines if auto play should be started by modifying the
        /// AutoPlayOnRequestEventArgs.RequestAccepted to true. This will be performed after this
        /// method, since the external event handler is subscribed to after GameLib. 
        /// </remarks>
        private void HandleAutoPlayOnRequestEvent(object sender, AutoPlayOnRequestEventArgs autoPlayOnRequestEventArgs)
        {
            cachedAutoplayOn = null;
        }

        /// <summary>
        /// Actions performed when a Foundation event is dequeued
        /// and needs processing.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleEventDispatched(object sender, EventDispatchedEventArgs eventArgs)
        {
            if(eventTable.ContainsKey(eventArgs.DispatchedEventType))
            {
                var handler = eventTable[eventArgs.DispatchedEventType];
                if(handler != null)
                {
                    handler(this, eventArgs.DispatchedEvent);

                    eventArgs.IsHandled = true;
                }
            }
        }

        /// <summary>
        /// Actions performed before a transaction is closed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleFinalizeTransaction(object sender, EventArgs eventArgs)
        {
            // If the game cycle has ended, clear the game cycle critical data cache.
            // Clearing the cache will clear the pending writes for the scope as well.
            // Since the Foundation will clear the game cycle scope when the game cycle ends,
            // there is no need to persist changes in this transaction.
            // Besides, once the game cycle ends, the game cycle scope becomes inaccessible,
            // committing pending writes to the scope would lead to an exception.
            if (pendingEndGameCycle)
            {
                criticalDataCache.CycleElapsed();
                pendingEndGameCycle = false;
            }

            // Commit all pending writes to critical data, which will clear them.
            CommitPendingCriticalDataWrites();
            cachedGameCycleState = GameCycleState.Invalid;
        }

        /// <summary>
        /// Handle the finalize outcome event from the foundation. Used for managing game cycle state caching.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="finalizeOutcomeEventArgs">The data associated with the event.</param>
        private void HandleFinalizeOutcomeEvent(object sender, FinalizeOutcomeEventArgs finalizeOutcomeEventArgs)
        {
            cachedGameCycleState = GameCycleState.Finalized;
        }

        /// <summary>
        /// Handle the outcome response event from the foundation. Used for managing game cycle state caching.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="outcomeResponseEventArgs">
        /// Outcome response information. Indicates if this is the final outcome.
        /// </param>
        private void HandleOutcomeResponseEvent(object sender, OutcomeResponseEventArgs outcomeResponseEventArgs)
        {
            var gameCycleState = QueryGameCycleState();

            switch(gameCycleState)
            {
                //Several different state transitions may happen as the result of an outcome response. Regular play,
                //ancillary play, and bonus play all have different transition states.
                case GameCycleState.EvaluatePending:
                    cachedGameCycleState = outcomeResponseEventArgs.IsFinalOutcome
                        ? GameCycleState.MainPlayComplete
                        : GameCycleState.Playing;
                    break;
                case GameCycleState.AncillaryEvaluatePending:
                    cachedGameCycleState = outcomeResponseEventArgs.IsFinalOutcome
                        ? GameCycleState.AncillaryPlayComplete
                        : GameCycleState.AncillaryPlaying;
                    break;
                case GameCycleState.BonusEvaluatePending:
                    cachedGameCycleState = outcomeResponseEventArgs.IsFinalOutcome
                        ? GameCycleState.BonusPlayComplete
                        : GameCycleState.BonusPlaying;
                    break;
            }

            //In the case it was none of these states, then the game is likely recovering from a power hit event.
            //In this case the state requested from the foundation could be either the pre or post transition state.
        }

        /// <summary>
        /// Handle the enroll response event. Used for managing the game cycle state caching as well as recovery
        /// for the committed bet caching.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="enrollResponseEventArgs">The event data.</param>
        private void HandleEnrollResponseEvent(object sender, EnrollResponseEventArgs enrollResponseEventArgs)
        {
            cachedGameCycleState = GameCycleState.EnrollComplete;
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
        /// </devdoc>
        private void MustHaveOpenTransaction()
        {
            if(!transactionManager.TransactionOpen)
            {
                throw new InvalidTransactionException("No open transaction is available for this operation.");
            }
        }

        /// <summary>
        /// Method which allows a context to be activated for testing.
        /// </summary>
        /// <param name="themeContext">The theme context to activate.</param>
        /// <param name="isThemeSelectionMenuOfferable">If the theme selection menu should be offerable during testing.</param>
        /// <remarks>
        /// If the testing of standard game lib is substantially expanded, then things should be re-worked to have a
        /// fully activated context.
        /// </remarks>
        internal void ActivateTestContext(ThemeContext themeContext, bool isThemeSelectionMenuOfferable)
        {
            criticalDataCache.SetGameMode(themeContext.GameContextMode);
            GameDenomination = themeContext.Denomination;
            GameContextMode = themeContext.GameContextMode;
            GameSubMode = themeContext.GameSubMode;
            PaytableName = themeContext.PaytableName;
            PaytableFileName = themeContext.PaytableFileName;
            IsThemeSelectionMenuOfferable = isThemeSelectionMenuOfferable;
        }

        /// <summary>
        /// Event handler for ActivateThemeContextEvent.
        /// Cache the Foundation owned config items upon the
        /// activation of a new theme context.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleActivateThemeContextEvent(object sender, ActivateThemeContextEventArgs eventArgs)
        {
            themeContextActive = true;

            var themeContext = eventArgs.ThemeContext;
            GameDenomination = themeContext.Denomination;
            GameContextMode = themeContext.GameContextMode;
            GameSubMode = themeContext.GameSubMode;
            PaytableName = themeContext.PaytableName;
            PaytableFileName = themeContext.PaytableFileName;

            if(GameDenomination <= 0)
            {
                throw new InvalidOperationException("Invalid denomination in received Activate Theme Context event.");
            }

            criticalDataCache.SetGameMode(themeContext.GameContextMode);

            // Configuration data are only accessible in Play and Utility mode.
            if(themeContext.GameContextMode == GameMode.Play ||
               themeContext.GameContextMode == GameMode.Utility)
            {
                // Clear caches in context sensitive modules.
                configurationRead.ClearCache();
                egmConfigData?.NewContext();
                if(LocalizationInformation is IContextCache contextCache)
                {
                    contextCache.NewContext();
                }

                // Cache the Show Environment
                gameShowEnvironment = foundationLink.ShowControl?.GetShowEnvironment() ?? ShowEnvironment.Invalid;

                // Cache the Foundation owned config items.
                GameMaxBet = Utility.ConvertToCredits(foundationLink.GameControl.GetMaxBetAmount(), GameDenomination);
                ButtonPanelMinBet = Utility.ConvertToCredits(foundationLink.GameControl.GetButtonPanelMinBetAmount(),
                                                             GameDenomination,
                                                             true);
                // GameControl category returns MinBetAmount with configuration value of "MACHINE WIDE MIN BET PER GAME".
                GameMinBetAmount = foundationLink.GameControl.GetMinBetAmount();
                GameMinBet = Utility.ConvertToCredits(GameMinBetAmount, GameDenomination, true);

                // Cache the win cap information.
                if(egmConfigData != null)
                {
                    WinCapInformation = new WinCapInformation(egmConfigData.GetWinCapBehaviorInfo(),
                                                              egmConfigData.GetProgressiveWinCap(),
                                                              egmConfigData.GetTotalWinCap());
                }
                else
                {
                    WinCapInformation = new WinCapInformation(foundationLink.GameControl.GetWinCapBehavior(),
                                                              0,
                                                              0);
                }

                MaxHistorySteps = foundationLink.GameControl.GetHistoryPlaySteps();
                MaxBetButtonBehavior = (MaxBetButtonBehavior)foundationLink.GameControl.GetMaxBetButtonBehavior();

                // Cache game play behavior configs
                GamePlayBehaviorConfigs = new GamePlayBehaviorConfigs
                                          {
                                              DefaultBetSelectionStyle = egmConfigData?.GetDefaultBetSelectionStyle()
                                                                         ?? new BetSelectionStyleInfo(),
                                              RtpOrderedByBetRequired = egmConfigData?.GetRtpOrderedByBetRequired() ?? false
                                          };

                // Cache presentation behavior configs
                PresentationBehaviorConfigs = new PresentationBehaviorConfigs
                                              {
                                                  MinimumBaseGameTime = foundationLink.GameControl.GetMinimumBaseGameTimeInMs(),
                                                  MinimumFreeSpinTime = foundationLink.GameControl.GetMinimumFreeSpinTimeInMs(),
                                                  CreditMeterBehavior = foundationLink.GameControl.GetCreditMeterBehavior().ConvertFromF2LSchema(),
                                                  TopScreenGameAdvertisement = egmConfigData?.GetMarketingBehavior().TopScreenGameAdvertisement
                                                                               ?? TopScreenGameAdvertisementType.Invalid,
                                                  DisplayVideoReelsForStepper = egmConfigData?.GetDisplayVideoReelsForStepper() ?? false,
                                                  BonusSoaaSettings = egmConfigData?.GetBonusSoaaSettings()
                                              };

                // Cache the ancillary related config items.
                AncillaryEnabled = foundationLink.GameControl.GetAncillaryGameEnabled();
                AncillaryCycleLimit = foundationLink.GameControl.GetAncillaryCycleLimit();
                AncillaryMonetaryLimit = foundationLink.GameControl.GetAncillaryMonetaryLimit();

                // Cache the bonus extension play related config items.
                BonusPlayEnabled = foundationLink.GameControl.GetBonusPlayEnabled();

                // Initialize the culture information.
                AvailableLanguages = foundationLink.GameControl.GetAvailableCultures();
                GameLanguage = foundationLink.GameControl.GetCulture();

                // Cache the RoundWagerUpPlayoff related config item.
                RoundWagerUpPlayoffEnabled = foundationLink.GameControl.GetRoundWagerUpPlayoffEnabled();

                // Cache the player meters.
                cachedPlayerMeters = foundationLink.GameControl.GetPlayerMeters().ToPublic();
            }

            if(themeContext.GameContextMode == GameMode.Play)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                progressiveBroadcastManager.Activate(true);

                // LineSelection request is allowed only in play mode in Q1 foundation branches.
                ConfiguredLineSelectionMode = (LineSelectionMode)foundationLink.GameControl.GetConfiguredLineSelection();

                // QueryPlayerSelectableDenoms is allowed only in play mode.
                var denomInfo = foundationLink.GameControl.GetGameDenominationInfo();
                DefaultGameDenomination = denomInfo.DefaultGameDenomination;
                availableDenominations = denomInfo.AvailableDenominations;
                availableProgressiveDenominations = denomInfo.AvailableProgressiveDenominations;

                // The offerable status of the theme selection menu can only be accessed in play mode.
                IsThemeSelectionMenuOfferable = foundationLink.GameControl.GetThemeSelectionMenuOfferable();
                IsPlayerAutoPlayEnabled = foundationLink.AutoPlay.GetConfigDataPlayerAutoPlayEnabled();
                IsAutoPlayConfirmationRequired = foundationLink.AutoPlay.GetConfigDataPlayerAutoPlayConfirmationRequired();
                IsAutoPlaySpeedIncreaseAllowed = foundationLink.AutoPlay.GetConfigDataAutoPlaySpeedIncreaseAllowed();

                //If the game cycle state is idle, then we know that no bet can be committed.
                //If it is not, then ask the foundation. It doesn't validate the state the method is called in.
                //There are other states where it can be 0, but this reduces that need for maintaining a list of those
                //states at the cost of an extra call when recovering from a power-hit.
                cachedCommittedBet = QueryGameCycleState() == GameCycleState.Idle ? 0 : foundationLink.GameControl.GetCommittedBet();
            }
            else
            {
                ConfiguredLineSelectionMode = (LineSelectionMode)F2LInternal.LineSelectionType.Undefined;

                DefaultGameDenomination = 0;
                availableDenominations = new List<long>();
                availableProgressiveDenominations = new List<long>();

                IsThemeSelectionMenuOfferable = false;
                IsPlayerAutoPlayEnabled = false;
                IsAutoPlayConfirmationRequired = false;
                IsAutoPlaySpeedIncreaseAllowed = null;
            }
        }

        /// <summary>
        /// Event handler for InactivateThemeContextEvent.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleInactivateThemeContextEvent(object sender, InactivateThemeContextEventArgs eventArgs)
        {
            themeContextActive = false;
            GameContextMode = GameMode.Invalid;
            criticalDataCache.SetGameMode(GameContextMode);

            // ReSharper disable once InconsistentlySynchronizedField
            progressiveBroadcastManager.Activate(false);

            //Clear any pending game transaction. The foundation discards existing requests when inactivating a context.
            transactionManager.ClearPendingTransaction();
        }

        /// <summary>
        /// Event handler for SwitchThemeContextEvent.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleSwitchThemeContextEvent(object sender, SwitchThemeContextEventArgs eventArgs)
        {
            // While technically still in context, the game mustn't use the GI channel until re-activated.
            themeContextActive = false;

            // ReSharper disable once InconsistentlySynchronizedField
            progressiveBroadcastManager.Activate(false);

            //Clear any pending game transaction.
            transactionManager.ClearPendingTransaction();
        }

        /// <summary>
        /// Event handler for ThemeSelectionMenuOfferableStatusChangedEvent.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleThemeSelectionMenuOfferableStatusChangedEvent(object sender,
                                                                         ThemeSelectionMenuOfferableStatusChangedEventArgs
                                                                             eventArgs)
        {
            IsThemeSelectionMenuOfferable = eventArgs.Offerable;
        }

        /// <summary>
        /// Handle the money event and update the cached player meters.
        /// </summary>
        /// <param name="sender">Originator of the event.</param>
        /// <param name="moneyEvent">The event data.</param>
        private void HandleMoneyEvent(object sender, MoneyEventArgs moneyEvent)
        {
            cachedPlayerMeters = moneyEvent.Meters;
        }

        /// <summary>
        /// Handle the language changed event and update the current language.
        /// </summary>
        /// <param name="sender">Originator of the event.</param>
        /// <param name="eventArgs">Arguments containing the new language.</param>
        private void HandleLanguageChangedEvent(object sender, LanguageChangedEventArgs eventArgs)
        {
            GameLanguage = eventArgs.Language;
        }

        /// <summary>
        /// Handles the event triggered by broadcasting timer.
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
                var reply = foundationLink.GameControl.GetProgressiveLevels(GameDenomination);
                var lessData = reply.ToDictionary(entry => (int)entry.GameLevel,
                                                  entry => new ProgressiveBroadcastData(entry.Amount, entry.PrizeString));

                lessHandler(this, new ProgressiveBroadcastEventArgs(lessData));
            }
        }

        /// <summary>
        /// Retrieves the progressive broadcast data from Foundation for all available progressive denominations.
        /// </summary>
        /// <returns>
        /// The progressive broadcast data for all available progressive denominations.
        /// Each denomination has a dictionary of progressive data keyed by game levels.
        /// </returns>
        private IDictionary<long, IDictionary<int, ProgressiveBroadcastData>> RetrieveAvailableDenominationsWithProgressives()
        {
            if(availableProgressiveDenominations == null)
            {
                // Ensure there is a transaction open for the first time call.
                GetAvailableProgressiveDenominations();
            }

            var reply = foundationLink.GameControl.GetProgressiveLevels(availableProgressiveDenominations);
            var result = reply.Where(entry => entry.Value.Any())
                              .ToDictionary(entry => entry.Key,
                                            entry => entry.Value.ToDictionary(
                                                             data => (int)data.GameLevel,
                                                             data => new ProgressiveBroadcastData(data.Amount, data.PrizeString))
                                                         as IDictionary<int, ProgressiveBroadcastData>);

            return result;
        }

        /// <summary>
        /// Commit all pending critical data writes to critical data.
        /// </summary>
        /// <remarks>
        /// This should be called when a transaction closes, to commit all changes accumulated within that transaction.
        /// Only those changes which are different from cached data will be written.  E.g., if, within a single transaction,
        /// a game changes some critical data, then changes it back to its original state, no critical data change will be written.
        /// This also updates the critical data cache.
        /// </remarks>
        private void CommitPendingCriticalDataWrites()
        {
            var pendingCriticalDataWrites = criticalDataCache.GetPendingWrites();

            foreach(var scope in pendingCriticalDataWrites.Keys)
                foreach(var path in pendingCriticalDataWrites[scope].Keys)
                {
                    foundationLink.GameControl.WriteCriticalData(scope.ToF2LCriticalDataScope(),
                                                                 path,
                                                                 pendingCriticalDataWrites[scope][path]);
                }

            criticalDataCache.FlushPendingWrites();
        }

        /// <summary>
        /// Initialize EGM config data interface implementation 
        /// with installed EgmConfigData category handlers.
        /// </summary>
        private void InitializeEgmConfigData()
        {
            // EgmConfigData category probably won't be supported by UC.
            if(foundationLink.EgmConfigData != null)
            {
                egmConfigData = new EgmConfigData(this);
                egmConfigData.Initialize(foundationLink.EgmConfigData);
            }
        }

        /// <summary>
        /// Initialize localizationInformation interface implementation 
        /// with installed localization category handlers.
        /// </summary>
        private void InitializeLocalizationInformation()
        {
            // Create and initialize the appropriate localization object, and assign it to the localization interface.
            if(foundationLink.LocalizationControl != null)
            {
                var localizationInformationObject = new LocalizationInformation(this);
                localizationInformationObject.Initialize(foundationLink.LocalizationControl);
                LocalizationInformation = localizationInformationObject;
            }
            else
            {
                var legacyLocalizationInformationObject = new LegacyLocalizationInformation(this);
                legacyLocalizationInformationObject.Initialize(foundationLink.GameControl);
                LocalizationInformation = legacyLocalizationInformationObject;
            }
        }

        /// <summary>
        /// Initialize game configuration read object with installed categories.
        /// </summary>
        private void InitializeConfigurationRead()
        {
            configurationRead.Initialize(
                new GameConfigurationReadLink(foundationLink.GameControl, foundationLink.CustomConfigurationRead));
        }

        /// <summary>
        /// Read out the bytes at the specified path from the critical data.
        /// </summary>
        /// <param name="scope">The scope to read the data from.</param>
        /// <param name="path">The path to read the data from.</param>
        /// <returns>The binary array data read from safe storage.</returns>
        private byte[] ReadRawCriticalData(CriticalDataScope scope, string path)
        {
            MustHaveOpenTransaction();

            // Game state is not cached in Standard Game Lib.
            // Leave the state checking to the Foundation.
            Utility.ValidateCriticalDataAccess(GameContextMode, GameCycleState.Invalid, DataAccessing.Read, scope);

            if(!Utility.ValidateCriticalDataName(path))
            {
                throw new ArgumentException("Critical Data name contains illegal characters.", nameof(path));
            }

            byte[] readData;

            if(criticalDataCache.IsDataCached(scope, path))
            {
                readData = criticalDataCache.GetCachedData(scope, path);
            }
            else
            {
                //If the data was not cached, then cache it.
                readData = foundationLink.GameControl.ReadCriticalData(scope.ToF2LCriticalDataScope(), path);
                criticalDataCache.CacheData(scope, path, readData);
            }

            return readData;
        }
        #endregion

        #endregion
    }
}
