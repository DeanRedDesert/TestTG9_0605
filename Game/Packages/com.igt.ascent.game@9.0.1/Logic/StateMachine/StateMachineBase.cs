//-----------------------------------------------------------------------
// <copyright file = "StateMachineBase.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable UnusedParameter.Local
namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Communication.CommunicationLib;
    using Communication.Foundation;
    using Evaluator.Schemas;
    using Foundation.ServiceProviders;
    using GameTimer;
    using Services;
    using Tracing;

    /// <summary>
    /// Function call when transaction is open with access to GameLib.
    /// </summary>
    /// <param name="gameLib">GameLib instance.</param>
    public delegate void TransactionalFunction(IGameLib gameLib);

    /// <summary>
    /// Base class to use for creating game state machines.
    /// This class provides basic functionality which allows state machines to operate within the State Machine
    /// Framework. This class should be used as the base for the games main state machine, but state machines for
    /// individual providers should be based on StateMachineSegment.
    /// </summary>
    public abstract class StateMachineBase : IStateMachine,
                                             IGameLibEventListener,
                                             IConsolidateAsynchronousServiceUpdate,
                                             IDisposable
    {
        #region Constants

        /// <summary>
        /// Critical data path of history step number list.
        /// </summary>
        public const string HistoryListPath = "HistoryList";

        /// <summary>
        /// Critical data path of history priority list.
        /// </summary>
        public const string HistoryPriorityListPath = "HistoryPriorityList";

        #endregion Constants

        #region Nested Class

        /// <summary>
        /// A Dictionary of provider and service values
        /// Key is provider name.
        /// Value is a Dictionary of service and values.
        /// Key is service identifier.
        /// Value is service value.
        /// </summary>
        protected internal class SerializedDataItems : Dictionary<string, Dictionary<int, byte[]>>
        {}

        /// <summary>
        /// Type which encapsulates information about a state.
        /// </summary>
        protected class StateInformation
        {
            /// <summary>
            /// Create a StateInformation object.
            /// </summary>
            /// <param name="processing">A StateStageDelegate for processing.</param>
            /// <param name="committed">A StateStageDelegate for committed.</param>
            public StateInformation(StateStageHandler processing, StateStageHandler committed)
            {
                Processing = processing;
                Committed = committed;
            }

            /// <summary>
            /// Create a StateInformation object.
            /// </summary>
            /// <param name="processing">A StateStageDelegate for processing.</param>
            /// <param name="committed">A StateStageDelegate for committed.</param>
            /// <param name="historySettings">History related settings.</param>
            public StateInformation(StateStageHandler processing, StateStageHandler committed,
                                    HistorySettings historySettings)
            {
                Processing = processing;
                Committed = committed;
                HistorySettings = historySettings;
            }

            /// <summary>
            /// Get the processing callback.
            /// </summary>
            public StateStageHandler Processing { get; private set; }

            /// <summary>
            /// Get the committed callback.
            /// </summary>
            public StateStageHandler Committed { get; private set; }

            /// <summary>
            /// Get if this state is to be saved for history.
            /// </summary>
            public bool IsHistory 
            {
                get
                {
                    return HistorySettings != null;
                }
            }

            /// <summary>
            /// Checks if current state is a history state that will save asynchronous 
            /// update data to history.
            /// </summary>
            /// <returns>
            /// True if the current state is a history state that will save asynchronous 
            /// update data to history, and false otherwise.
            /// </returns>
            public bool IsAsynchronousHistory
            {
                get
                {
                    return IsHistory && HistorySettings.IsAsynchronousHistory;
                }
            }

            /// <summary>
            /// History related settings.
            /// </summary>
            public HistorySettings HistorySettings { get; private set; }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Current paytable for this state machine.
        /// </summary>
        public virtual Paytable Paytable { set; get; }

        /// <summary>
        /// Gets the empty <see cref="ServiceRequestData"/> object and ensures that it is empty by clearing it.
        /// </summary>
        private static ServiceRequestData EmptyRequestData
        {
            get
            {
                emptyRequestData.Clear();
                return emptyRequestData;
            }
        }

        /// <summary>
        /// The constant for an empty state stage.
        /// </summary>
        public static readonly StateStageHandler EmptyStateStage = framework => { };

        /// <summary>
        /// A cache of service provider updates that are sent asynchronously.
        /// </summary>
        private readonly DataItems asyncServiceUpdateCache = new DataItems();

        /// <summary>
        /// Provides APIs for tracing game logic events occurred at the State Machine level.
        /// </summary>
        private static readonly StateMachineFrameworkTracing Tracing = StateMachineFrameworkTracing.Log;

        /// <summary>
        /// A flag to indicate that the current state being executed should be excluded from
        /// history. This should only be set externally through ExcludeCurrentHistoryStep().
        /// </summary>
        private bool excludeCurrentHistoryStep;

        /// <summary>
        /// A flag to indicate when calling ExcludeCurrentHistoryStep() is allowed.
        /// </summary>
        private bool excludeCurrentHistoryStepSettable;

        /// <summary>
        /// An empty <see cref="ServiceRequestData"/> which will be used for any states that do not have
        /// required data.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private static readonly ServiceRequestData emptyRequestData = new ServiceRequestData();

        /// <summary>
        /// Cache of all history related data that categorized by steps.
        /// Key is the history step.
        /// Value is history data for particular step.
        /// </summary>
        private readonly Dictionary<int, HistoryStepRecord> cachedHistoryData = new Dictionary<int, HistoryStepRecord>();

        /// <summary>
        /// Cache used to store non-history requests for each state which has negotiated data.
        /// </summary>
        private readonly Dictionary<string, ServiceRequestData> cachedNonHistoryRequests =
            new Dictionary<string, ServiceRequestData>();

        /// <summary>
        /// Cache of asynchronous non-history requests for each state which has negotiated data.
        /// </summary>
        private readonly Dictionary<string, ServiceRequestData> cachedAsyncNonHistoryRequests =
            new Dictionary<string, ServiceRequestData>();

        /// <summary>
        /// Cache of history mode/volatile requests for each state which has negotiated data.
        /// </summary>
        private readonly Dictionary<string, ServiceRequestData> cachedHistoryRequests =
            new Dictionary<string, ServiceRequestData>();

        /// <summary>
        /// List of state handlers for the state machine.
        /// </summary>
        private readonly Dictionary<string, StateInformation> stateHandlers = new Dictionary<string, StateInformation>();

        /// <summary>
        /// Flag indicating if the instance has been disposed.
        /// </summary>
        protected bool Disposed;

        /// <summary>
        /// The initial state of the state machine. Should only be accessed through the InitialState property.
        /// </summary>
        private string initialState;

        /// <summary>
        /// The Game Lib event listeners held by the State Machine.
        /// </summary>
        private readonly List<IGameLibEventListener> gameLibEventListeners = new List<IGameLibEventListener>();

        /// <summary>
        /// The game timer controller that manages game timers.
        /// </summary>
        private GameTimerController gameTimerController;

        /// <summary>
        /// The <see cref="StateMachineSegment"/> that implements power hit recovery and history.
        /// </summary>
        protected RecoverySegment RecoverySegment;

        #endregion

        #region IStateMachine Implementation

        /// <inheritdoc />
        public virtual IStateMachineContext Context { get; private set; }

        /// <inheritdoc />
        public virtual ICollection<string> States
        {
            get { return stateHandlers.Keys; }
        }

        /// <inheritdoc />
        public string InitialState
        {
            set
            {
                if(States.Contains(value))
                {
                    initialState = value;
                }
                else
                {
                    throw new InvalidStateException(value, "Initial state must be a configured state.");
                }
            }

            get { return initialState; }
        }

        /// <inheritdoc />
        public bool RecoveryEnabled { get; private set; }

        /// <inheritdoc />
        public string RecoveryState
        {
            get { return RecoveryEnabled ? RecoverySegment.InitialState : string.Empty; }
        }

        /// <inheritdoc />
        public bool IsRecovering
        {
            get { return RecoverySegment != null && RecoverySegment.IsRecovering; }
        }

        /// <inheritdoc />
        public virtual void ExecuteStateStage(string state, StateStage stage, IStateMachineFramework framework)
        {
            if(state == null)
            {
                throw new ArgumentNullException("stage", "Argument may not be null");
            }

            if(framework == null)
            {
                throw new ArgumentNullException("framework", "Argument may not be null");
            }

            if(!States.Contains(state))
            {
                throw new InvalidStateException(state, "Attempted to execute a state which is not configured.");
            }

            switch (stage)
            {
                case StateStage.Processing:
                    Tracing.StateHandlerProcessingStageStart(state);
                    stateHandlers[state].Processing(framework);
                    Tracing.StateHandlerProcessingStageStop(state);
                    break;

                case StateStage.Committed:
                    excludeCurrentHistoryStepSettable = true;

                    Tracing.StateHandlerCommittedStageStart(state);
                    stateHandlers[state].Committed(framework);
                    Tracing.StateHandlerCommittedStageStop(state);

                    Tracing.WriteCachedHistoryStart(state);
                    WriteCachedHistory(framework);
                    Tracing.WriteCachedHistoryStop(state);

                    excludeCurrentHistoryStepSettable = false;
                    excludeCurrentHistoryStep = false;
                    break;
            }
        }

        /// <inheritdoc />
        public virtual void WriteStartStateHistory(
            string state,
            DataItems data,
            int historyStep,
            IStateMachineFramework framework)
        {
            CheckHistoryParameters(state, framework, data);

            if(stateHandlers[state].IsHistory)
            {
                var historySettings = stateHandlers[state].HistorySettings;
                var cacheHistoryBlock = historySettings.StartStateHistoryHandler(state, data, framework);
                if(cacheHistoryBlock != null)
                {
                    cachedHistoryData[historyStep] = new HistoryStepRecord
                    {
                        Priority = historySettings.HistoryStepPriority,
                        StartStateData = cacheHistoryBlock.ToBytes()
                    };
                }
            }
        }

        /// <inheritdoc />
        public virtual bool WriteUpdateHistory(
            string state,
            DataItems data,
            int historyStep,
            IStateMachineFramework framework)
        {
            CheckHistoryParameters(state, framework, data);

            var writePerformed = false;
            if(stateHandlers[state].IsHistory)
            {
                var historySettings = stateHandlers[state].HistorySettings;
                if(historySettings.UpdateDataHistoryHandler != null)
                {
                    var cacheHistoryBlock = historySettings.UpdateDataHistoryHandler(
                        state,
                        data,
                        framework);

                    if(cacheHistoryBlock != null)
                    {
                        // Only update the cached history data if data for the history step has already been cached
                        // after WriteStartStateHistory has been called.
                        if(cachedHistoryData.ContainsKey(historyStep))
                        {
                            writePerformed = true;
                            cachedHistoryData[historyStep].StartStateData = cacheHistoryBlock.ToBytes();
                            cachedHistoryData[historyStep].Priority = historySettings.HistoryStepPriority;
                        }
                    }
                }
                else if(historySettings.HasAsyncServicesToUpdate)
                {
                    // For each provider appeared in the async data.
                    foreach(var dataItem in data)
                    {
                        var providerName = dataItem.Key;
                        // Get the list of async service identifiers the game wants to save for history.
                        // Pass in a delegate for history settings to retrieve identifiers.
                        var servicesToUpdate = historySettings.GetAsyncServicesToUpdate(providerName,
                            serviceName => GetIdentifiersForService(state, providerName, serviceName));
                        // Check if required service identifier is present in async data.
                        var availableUpdates = dataItem.Value.Where(entry => servicesToUpdate.Contains(entry.Key));
                        // Serialize each service data and add to the cache.
                        foreach(var serviceData in availableUpdates)
                        {
                            UpdateAsyncHistoryCache(historyStep, providerName, serviceData.Key, SerializeDataObject(serviceData.Value));
                            writePerformed = true;
                        }
                    }
                }
                //If the WriteStartStateAsynchUpdateHistory handler is null or AsyncServicesToSaveToHistory is null 
                //or empty, then no history will be written for updates.
            }
            return writePerformed;
        }

        /// <inheritdoc />
        public bool IsHistoryState(string state)
        {
            if(state == null)
            {
                throw new ArgumentNullException("state", "Parameter may not be null.");
            }

            if(stateHandlers.ContainsKey(state))
            {
                return stateHandlers[state].IsHistory;
            }
            throw new InvalidStateException(state, "The specified state is not configured.");
        }

        /// <inheritdoc />
        /// <devdoc>Overrides should call this base.</devdoc>
        public virtual void Initialize(IStateMachineFramework framework)
        {
            var meterSource = new GameLibMeterSource(framework.GameLib);
            var metersProvider = new MetersProvider(meterSource);
            framework.GameServiceController.AddProvider(metersProvider, "MetersProvider");
            framework.GameServiceController.AddProvider(
                new ConfigurationProvider(framework.GameLib),
                "ConfigurationProvider");
            framework.GameServiceController.AddProvider(new ShowProvider(framework.GameLib), "ShowProvider");
            framework.GameServiceController.AddProvider(
                new GameContextModeProvider(framework.GameLib),
                "GameContextModeProvider");
            framework.GameServiceController.AddProvider(new ThemeContextSessionProvider(framework.GameLib),
                "ThemeContextSessionProvider");
            framework.GameServiceController.AddProvider(
                new DisplayControlStateProvider(framework.GameLib),
                "DisplayControlStateProvider");
            framework.GameServiceController.AddProvider(
                new LanguageConfigurationProvider(framework.GameLib),
                "LanguageConfigurationProvider");
            framework.GameServiceController.AddProvider(
                new BankStatusProvider(framework.GameLib),
                "BankStatusProvider");
            framework.GameServiceController.AddProvider(
                new VoucherPrintProvider(framework.GameLib),
                "VoucherPrintProvider");

            //This provider is useful for debugging and testing.
            framework.GameServiceController.AddProvider(framework, "StateFramework");

            framework.GameServiceController.StartFillAsynchronousRequest +=
                (s, e) => UpdateAsyncData(framework, e);

            if(RecoverySegment != null)
            {
                RecoverySegment.Initialize(framework);
            }
        }

        /// <inheritdoc/>
        public void UpdateRequiredDataCache(IEnumerable<KeyValuePair<string, ServiceRequestData>> requiredDataCache)
        {
            cachedAsyncNonHistoryRequests.Clear();
            cachedHistoryRequests.Clear();
            cachedNonHistoryRequests.Clear();
            foreach (var item in requiredDataCache)
            {
                CacheNonHistoryRequest(item.Key, item.Value);
                CacheAsyncRequest(item.Key, item.Value);
                CacheHistoryRequest(item.Key, item.Value);
            }
        }

        /// <inheritdoc/>
        public bool IsAsynchronousHistoryState(string state)
        {
            if(state == null)
            {
                throw new ArgumentNullException("state", "Parameter may not be null.");
            }

            if(!stateHandlers.ContainsKey(state))
            {
                throw new InvalidStateException(state, "The specified state is not configured.");
            }

            return stateHandlers[state].IsAsynchronousHistory;
        }

        /// <inheritdoc/>
        public virtual void ReadConfiguration(IGameLib gameLib)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// Only the processing stage is allowed to be empty.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="state"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="state"/> is not the name of a state.
        /// </exception>
        public bool ContainsStateHandler(string state, StateStage stage)
        {
            if(state == null)
            {
                throw new ArgumentNullException("state", "The state name cannot be null.");
            }

            if(!stateHandlers.ContainsKey(state))
            {
                throw new ArgumentException(string.Format("The state named {0} could not be found.", state), "state");
            }

            switch(stage)
            {
                case StateStage.Processing:
                    return stateHandlers[state].Processing != EmptyStateStage;
                default:
                    // Only the processing stage is allowed to be skipped for now because
                    // it is always going to be a single transaction. In addition very rarely
                    // should a logic state have a processing but not a committed.
                    return true;
            }
        }

        /// <inheritdoc />
        public virtual void CleanUp(IStateMachineFramework framework)
        {
            UnregisterGameLibEvents(framework.GameLib);

            foreach(var listener in gameLibEventListeners)
            {
                listener.UnregisterGameLibEvents(framework.GameLib);
            }

            if(gameTimerController != null)
            {
                gameTimerController.UnregisterGameLibEvents(framework.GameLib);
            }
        }

        #endregion

        #region IGameLibEventListener Implementation

        /// <inheritdoc />
        /// <remarks>
        /// This function is made abstract to force the state machine implementations to pause
        /// and review if all the game lib event handlers are cleaned up. This is important to
        /// memory leak prevention. The cleanup should cover the game lib event subscriptions 
        /// of the state machine as well as objects held by the state machine except the following:
        /// 1. Providers that are added to the framework's service controller, which are cleaned up by
        /// the state machine framework.
        /// 2. Objects that are added to the state machine's listener list by calling 
        /// <see cref="AddGameLibEventListener"/>. These are cleaned up in the base implementation 
        /// of <see cref="CleanUp"/> method which is to be called by the framework.
        /// </remarks>
        /// <DevDoc>
        /// IMPORTANT!!  Please keep in mind that this State Machine Base should avoid
        /// registering Game Lib events itself.  If it does, we will lose the abstractness.
        /// </DevDoc>
        public abstract void UnregisterGameLibEvents(IGameLib gameLib);

        #endregion

        #region IConsolidateAsynchronousServiceUpdate Implementation

        /// <inheritdoc />
        public void UpdateAsynchronousData(IStateMachineFramework framework)
        {
            if(asyncServiceUpdateCache.Any())
            {
                framework.Presentation.UpdateAsynchData(framework.CurrentState, asyncServiceUpdateCache);
                asyncServiceUpdateCache.Clear();
            }
        }

        /// <inheritdoc />
        public void ClearAsynchronousData()
        {
            asyncServiceUpdateCache.Clear();
        }

        #endregion

        /// <summary>
        /// The default constructor of the state machine.
        /// </summary>
        /// <param name="recoveryEnabled">
        /// Indicates whether power hit recovery is enabled.
        /// This parameter is optional.  If not specified, its default is true.
        /// </param>
        protected StateMachineBase(bool recoveryEnabled = true)
        {
            RecoveryEnabled = recoveryEnabled; // Derived classes can set this to false to disable recovery mode.
            if(RecoveryEnabled)
            {
                RecoverySegment = new RecoverySegment(this, RecoverySegment.RecoverySegmentFunction.Recovery);
            }
        }

        /// <summary>
        /// Construct the state machine with the specified context information.
        /// </summary>
        /// <param name="context">The context information used to create the state machine.</param>
        /// <param name="recoveryEnabled">
        /// Indicates whether power hit recovery is enabled.
        /// This parameter is optional.  If not specified, its default is true.
        /// </param>
        protected StateMachineBase(IStateMachineContext context, bool recoveryEnabled = true)
            : this(recoveryEnabled)
        {
            if(context == null)
            {
                throw new ArgumentNullException("context", "Parameter may not be null.");
            }

            Context = context;
        }

        /// <summary>
        /// The createState function allows a game to create a power hit tolerant state
        /// that will be managed by the state machine framework.
        /// </summary>
        /// <param name="stateName">
        /// The name to give the state, this name will be used for all references
        /// to the state.
        /// </param>
        /// <param name="processing">
        /// The processing callback for the state. This callback is for performing
        /// processing and saving the results of that processing.
        /// </param>
        /// <param name="committed">
        /// The committed callback for the state.  The committed callback is for
        /// presenting information which was generated during processing, and for
        /// saving the values of user input. The committed callback will not be
        /// entered until the processing callback has completed and any initiated
        /// transactions have completed.
        /// </param>
        /// <param name="saveHistory">
        /// Flag which indicates that this state is to be saved for history.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// None of the parameters to this function may be null. If any are then this
        /// exception will be thrown.
        /// </exception>
        /// <exception cref="DuplicateStateNameException">
        /// If an attempt is made to add a state with the same name as an existing state,
        /// then this exception will be thrown.
        /// </exception>
        public void CreateState(
            string stateName,
            StateStageHandler processing,
            StateStageHandler committed,
            bool saveHistory)
        {
            CheckCreateStateParameters(stateName, processing, committed);

            if(saveHistory)
            {
                stateHandlers[stateName] = new StateInformation(processing, committed, new HistorySettings());
            }
            else
            {
                stateHandlers[stateName] = new StateInformation(processing, committed);
            }
        }

        /// <summary>
        /// The createState function allows a game to create a power hit tolerant state
        /// that will be managed by the state machine framework.
        /// </summary>
        /// <param name="stateName">
        /// The name to give the state, this name will be used for all references
        /// to the state.
        /// </param>
        /// <param name="processing">
        /// The processing callback for the state. This callback is for performing
        /// processing and saving the results of that processing.
        /// </param>
        /// <param name="committed">
        /// The committed callback for the state.  The committed callback is for
        /// presenting information which was generated during processing, and for
        /// saving the values of user input. The committed callback will not be
        /// entered until the processing callback has completed and any initiated
        /// transactions have completed.
        /// </param>
        /// <param name="writeStartStateHistoryHandler">
        /// A delegate which handles writing of start state message data for history.
        /// If the argument is null, then standard serialization will be used for StartState messages.
        /// </param>
        /// <param name="writeUpdateDataHistoryHandler">
        /// A delegate which handles writing of asynchronous update messages for history.
        /// If the argument is null, then no history will be written for updates.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// None of the parameters to this function may be null. If any are then this
        /// exception will be thrown.
        /// </exception>
        /// <exception cref="DuplicateStateNameException">
        /// If an attempt is made to add a state with the same name as an existing state,
        /// then this exception will be thrown.
        /// </exception>
        public void CreateState(
            string stateName,
            StateStageHandler processing,
            StateStageHandler committed,
            WriteStartStateHistoryHandler writeStartStateHistoryHandler,
            WriteUpdateDataHistoryHandler writeUpdateDataHistoryHandler)
        {
            CheckCreateStateParameters(stateName, processing, committed);

            stateHandlers[stateName] = new StateInformation(
                processing,
                committed,
                new HistorySettings(writeStartStateHistoryHandler, writeUpdateDataHistoryHandler));
        }

        /// <summary>
        /// The createState function allows a game to create a power hit tolerant state
        /// that will be managed by the state machine framework.
        /// </summary>
        /// <param name="stateName">
        /// The name to give the state, this name will be used for all references
        /// to the state.
        /// </param>
        /// <param name="processing">
        /// The processing callback for the state. This callback is for performing
        /// processing and saving the results of that processing.
        /// </param>
        /// <param name="committed">
        /// The committed callback for the state.  The committed callback is for
        /// presenting information which was generated during processing, and for
        /// saving the values of user input. The committed callback will not be
        /// entered until the processing callback has completed and any initiated
        /// transactions have completed.
        /// </param>
        /// <param name="historySettings">
        /// History related settings.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// None of the parameters to this function may be null. If any are then this
        /// exception will be thrown.
        /// </exception>
        /// <exception cref="DuplicateStateNameException">
        /// If an attempt is made to add a state with the same name as an existing state,
        /// then this exception will be thrown.
        /// </exception>
        public void CreateState(
            string stateName,
            StateStageHandler processing,
            StateStageHandler committed,
            HistorySettings historySettings
            )
        {
            CheckCreateStateParameters(stateName, processing, committed);

            if(historySettings == null)
            {
                throw new ArgumentNullException("historySettings", "Argument may not be null.");
            }

            stateHandlers[stateName] = new StateInformation(processing, committed, historySettings);
        }

        /// <summary>
        /// Check if the parameters are valid when create a state.
        /// </summary>
        /// <param name="stateName">
        /// The name to give the state, this name will be used for all references to the state.
        /// </param>
        /// <param name="processing">
        /// The processing callback for the state. This callback is for performing
        /// processing and saving the results of that processing.
        /// </param>
        /// <param name="committed">
        /// The committed callback for the state. The committed callback is for
        /// presenting information which was generated during processing, and for
        /// saving the values of user input. The committed callback will not be
        /// entered until the processing callback has completed and any initiated
        /// transactions have completed.
        /// </param>
        private void CheckCreateStateParameters(string stateName, StateStageHandler processing, StateStageHandler committed)
        {
            if(stateName == null)
            {
                throw new ArgumentNullException("stateName", "Argument may not be null.");
            }

            if(processing == null)
            {
                throw new ArgumentNullException("processing", "Argument may not be null.");
            }

            if(committed == null)
            {
                throw new ArgumentNullException("committed", "Argument may not be null.");
            }

            if(States.Contains(stateName))
            {
                throw new DuplicateStateNameException(stateName, "Cannot create multiple states with the same name.");
            }
        }

        /// <summary>
        /// Get the data required for a presentation state.
        /// </summary>
        /// <param name="framework">A state machine framework for negotiating data.</param>
        /// <returns>The data required for the specified presentation state.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="framework"/> is null.</exception>
        /// <remarks>
        /// This method uses a cache containing the required data for each state that actually needs data.  The
        /// cache is provided by the presentation before the state machine begins execution by calling 
        /// <see cref="UpdateRequiredDataCache"/>.  Any states that do not have an entry in the cache will be assumed to 
        /// require no data, and an empty request data will be sent.
        /// </remarks>
        public virtual DataItems GetStateData(IStateMachineFramework framework)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework", "Framework parameter may not be null.");
            }

            var requestData = cachedNonHistoryRequests.ContainsKey(framework.CurrentState)
                ? cachedNonHistoryRequests[framework.CurrentState]
                : EmptyRequestData;

            return framework.GameServiceController.FillRequest(requestData);
        }

        /// <summary>
        /// Get the volatile data required for a presentation state.
        /// </summary>
        /// <param name="framework">A state machine framework for negotiating data.</param>
        /// <param name="stateName">State to get volatile data for the presentation.</param>
        /// <returns>The data required for the specified presentation state.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="framework"/> is null.</exception>
        public virtual DataItems GetHistoryStateData(IStateMachineFramework framework, string stateName)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework", "Framework parameter may not be null.");
            }

            var requestData = cachedHistoryRequests.ContainsKey(stateName) && cachedHistoryRequests[stateName] != null
                ? cachedHistoryRequests[stateName]
                : EmptyRequestData;

            return framework.GameServiceController.FillRequest(requestData);
        }

        /// <summary>
        /// For a set of negotiated data build a cache of the services which are not history.
        /// </summary>
        /// <param name="currentState">State to cache the non-history request for.</param>
        /// <param name="requiredData">The required data containing non-history items.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="currentState"/> 
        /// or <paramref name="requiredData"/> is null.</exception>
        protected void CacheNonHistoryRequest(string currentState, ServiceRequestData requiredData)
        {
            CacheRequest(currentState, requiredData, cachedNonHistoryRequests,
                         accessor => !accessor.NotificationType.IsHistory());
        }

        /// <summary>
        /// For a set of negotiated data build a cache of the services which are to be accessed asynchronously.
        /// </summary>
        /// <param name="currentState">State to cache the asynchronous request for.</param>
        /// <param name="requiredData">The required data containing synchronous and asynchronous items.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="currentState"/> 
        /// or <paramref name="requiredData"/> is null.</exception>
        protected void CacheAsyncRequest(string currentState, ServiceRequestData requiredData)
        {
            CacheRequest(currentState, requiredData, cachedAsyncNonHistoryRequests,
                accessor => accessor.NotificationType.IsAsynchronousNonHistory());
        }

        /// <summary>
        /// For a set of negotiated data build a cache of the services which are to be accessed volatile.
        /// </summary>
        /// <param name="currentState">State to cache the volatile request for.</param>
        /// <param name="requiredData">The required data containing volatile items.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="currentState"/> 
        /// or <paramref name="requiredData"/> is null.</exception>
        protected void CacheHistoryRequest(string currentState, ServiceRequestData requiredData)
        {
            CacheRequest(currentState, requiredData, cachedHistoryRequests,
                accessor => accessor.NotificationType.IsHistory());
        }

        /// <summary>
        /// For a set of negotiated data build a cache of the services.
        /// </summary>
        /// <param name="currentState">State to cache the asynchronous request for.</param>
        /// <param name="requiredData">The required data containing synchronous and asynchronous items.</param>
        /// <param name="cache">The cache to add requests to.</param>
        /// <param name="predicate">The predicate to select the <seealso cref="ServiceAccessor"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="currentState"/> 
        /// or <paramref name="requiredData"/> is null.</exception>
        protected void CacheRequest(string currentState, ServiceRequestData requiredData,
            Dictionary<string, ServiceRequestData> cache, Func<ServiceAccessor, bool> predicate)
        {
            if(currentState == null)
            {
                throw new ArgumentNullException("currentState", "Argument may not be null.");
            }
            if(requiredData == null)
            {
                throw new ArgumentNullException("requiredData", "Argument may not be null.");
            }

            var serviceRequestData = new ServiceRequestData();

            foreach(var provider in requiredData)
            {
                foreach(var item in provider.Value.Where(predicate))
                {
                    if(!serviceRequestData.ContainsKey(provider.Key))
                    {
                        serviceRequestData[provider.Key] = new List<ServiceAccessor>();
                    }
                    serviceRequestData[provider.Key].Add(item);
                }
            }

            if(serviceRequestData.Count != 0)
            {
                cache[currentState] = serviceRequestData;
            }
            else
            {
                cache[currentState] = null;
            }
        }

        /// <summary>
        /// Get the asynchronous data required for the state and given asynchronous request event.
        /// </summary>
        /// <param name="framework">The framework sending the update.</param>
        /// <param name="currentState">The state to get the asynchronous data for.</param>
        /// <param name="asyncEventArgs">Event containing the asynchronous update.</param>
        /// <returns>
        /// A list of data to update. If the state had not been negotiated, or there isn't any asynchronous data for
        /// the state, then null will be returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="framework"/>,  
        /// <paramref name="currentState"/>, or <paramref name="asyncEventArgs"/> are null.</exception>
        public virtual DataItems GetAsynchStateData(
            IStateMachineFramework framework,
            string currentState,
            StartFillAsynchronousRequestEventArgs asyncEventArgs)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework", "Framework parameter may not be null.");
            }
            if(asyncEventArgs == null)
            {
                throw new ArgumentNullException("asyncEventArgs", "Argument may not be null.");
            }
            if(currentState == null)
            {
                throw new ArgumentNullException("currentState", "Argument may not be null.");
            }

            DataItems request = null;

            if(cachedAsyncNonHistoryRequests.ContainsKey(currentState) &&
               cachedAsyncNonHistoryRequests[currentState] != null &&
               cachedAsyncNonHistoryRequests[currentState].ContainsKey(asyncEventArgs.ServiceProviderName))
            {
                var updatedValues = new ServiceRequestData();

                var cachedItems = cachedAsyncNonHistoryRequests[currentState][asyncEventArgs.ServiceProviderName];

                var serviceAccessors =
                    cachedItems.Where(item =>
                        asyncEventArgs.ServiceSignatures.Any(signature => signature.ServiceName == item.Service)).ToList();

                if(serviceAccessors.Count > 0)
                {
                    updatedValues[asyncEventArgs.ServiceProviderName] = serviceAccessors;
                }

                //Only return a result if there was a service to update in the provider.
                if(updatedValues.Count != 0)
                {
                    request = framework.GameServiceController.FillAsynchronousRequest(updatedValues);
                }
            }

            return request;
        }

        /// <summary>
        /// Get the asynchronous history data required for the state and given asynchronous request event.
        /// </summary>
        /// <param name="framework">The framework sending the update.</param>
        /// <param name="currentState">The state to get the asynchronous history data for.</param>
        /// <param name="asyncEventArgs">Event containing the asynchronous update.</param>
        /// <returns>
        /// A list of data to update. If the state had not been negotiated, or there isn't any asynchronous 
        /// history data for the state, then null will be returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="framework"/>,  
        /// <paramref name="currentState"/>, or <paramref name="asyncEventArgs"/> are null.</exception>
        public virtual DataItems GetAsyncHistoryStateData(
            IStateMachineFramework framework,
            string currentState,
            StartFillAsynchronousRequestEventArgs asyncEventArgs)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework", "Framework parameter may not be null.");
            }
            if(asyncEventArgs == null)
            {
                throw new ArgumentNullException("asyncEventArgs", "Argument may not be null.");
            }
            if(currentState == null)
            {
                throw new ArgumentNullException("currentState", "Argument may not be null.");
            }

            DataItems request = null;

            if(cachedHistoryRequests.ContainsKey(currentState) &&
               cachedHistoryRequests[currentState] != null &&
               cachedHistoryRequests[currentState].ContainsKey(asyncEventArgs.ServiceProviderName))
            {
                var updatedValues = new ServiceRequestData();

                var cachedItems = cachedHistoryRequests[currentState][asyncEventArgs.ServiceProviderName];

                var serviceAccessors =
                    cachedItems.Where(item =>
                        asyncEventArgs.ServiceSignatures.Any(signature => signature.ServiceName == item.Service) &&
                        item.NotificationType == NotificationType.AsynchronousHistory).ToList();

                if(serviceAccessors.Count > 0)
                {
                    updatedValues[asyncEventArgs.ServiceProviderName] = serviceAccessors;
                }

                //Only return a result if there was a service to update in the provider.
                if(updatedValues.Any())
                {
                    request = framework.GameServiceController.FillAsynchronousRequest(updatedValues);
                }
            }

            return request;
        }

        /// <summary>
        /// Convenience function for starting a presentation state. Sends a start state message for
        /// the currently executing state.
        /// </summary>
        /// <param name="framework">The state machine framework executing the state machine.</param>
        public virtual void StartPresentationState(IStateMachineFramework framework)
        {
            var stateData = GetStateData(framework);
            framework.Presentation.StartState(framework.CurrentState, stateData);
        }

        /// <summary>
        /// Serialize the object to binary format.
        /// </summary>
        /// <param name="dataObject">The object to be serialized.</param>
        /// <returns>Binary format of the object.</returns>
        private static byte[] SerializeDataObject(object dataObject)
        {
            if(dataObject == null)
            {
                return null;
            }
            using(var memStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memStream, dataObject);
                return memStream.ToArray();
            }
        }

        /// <summary>
        /// Exclude the current history step from the history.
        /// This can only be called within StateStage.Committed.
        /// 
        /// This will typically be used when the state machine exits a state and then
        /// re-enters that same state due to a non-gameflow change, e.g. a language change.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when this method is called outside of StateStage.Committed.</exception>
        protected internal void ExcludeCurrentHistoryStep()
        {
            if(excludeCurrentHistoryStepSettable)
            {
                excludeCurrentHistoryStep = true;
            }
            else
            {
                throw new InvalidOperationException("ExcludeCurrentHistoryStep() can only be invoked while committing a state.");
            }
        }

        /// <summary>
        /// Update the serialized data to async history cache.
        /// </summary>
        /// <param name="historyStep">The history to update.</param>
        /// <param name="providerName">The provider name to update.</param>
        /// <param name="serviceIdentifier">The service identifier to update.</param>
        /// <param name="updateData">The serialized data to be updated to the cache.</param>
        private void UpdateAsyncHistoryCache(int historyStep, string providerName, int serviceIdentifier, byte[] updateData)
        {
            if(!cachedHistoryData.ContainsKey(historyStep))
            {
                // We are unable to update a step that does not exist.
                return;
            }

            if(cachedHistoryData[historyStep].AsynchronousData == null)
            {
                cachedHistoryData[historyStep].AsynchronousData = new SerializedDataItems();
            }
            if(!cachedHistoryData[historyStep].AsynchronousData.ContainsKey(providerName))
            {
                cachedHistoryData[historyStep].AsynchronousData[providerName] = new Dictionary<int, byte[]>();
            }
            cachedHistoryData[historyStep].AsynchronousData[providerName][serviceIdentifier] = updateData;
        }

        /// <summary>
        /// Common parameter check used by WriteStartStateStartStateHistory and WriteUpdateHistory.
        /// </summary>
        /// <param name="state">
        /// A state which is checked against null and checked against the list of configured states.
        /// </param>
        /// <param name="framework">A framework to check for null.</param>
        /// <param name="data">Data to check for null.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown if the passed <paramref name="state"/>, <paramref name="framework"/>, 
        /// or <paramref name="data"/> is null.
        /// </exception>
        /// <exception cref="InvalidStateException">
        /// This exception is thrown if the specified <paramref name="state"/> is not configured for the state machine.
        /// </exception>
        /// <devdoc>
        /// Normally exception documentation isn't propagated up functions, but this function simply
        /// checks parameters and it not part of an API itself. As such these exceptions are propagated in the
        /// documentation. Additionally these are documented in the interfaces, so all implementations should
        /// throw these exceptions under the same circumstances.
        /// </devdoc>
        private void CheckHistoryParameters(
            string state,
            IStateMachineFramework framework,
            DataItems data)
        {
            if(state == null)
            {
                throw new ArgumentNullException("state", "Argument may not be null");
            }

            if(framework == null)
            {
                throw new ArgumentNullException("framework", "Argument may not be null");
            }

            if(data == null)
            {
                throw new ArgumentNullException("data", "Argument may not be null");
            }

            if(!States.Contains(state))
            {
                var stateList = States.Aggregate(string.Empty, (current, stateName) => current + stateName + "; ");

                throw new InvalidStateException(
                    state,
                    "Attempted to execute a state which is not configured. Available states are: " +
                    stateList);
            }
        }

        /// <summary>
        /// Write cached history data.
        /// </summary>
        /// <param name="framework">A state machine framework for writing cached history data.</param>
        /// <exception cref="ArgumentNullException">This exception is thrown if the passed 
        /// <paramref name="framework"/> is null.</exception>
        private void WriteCachedHistory(IStateMachineFramework framework)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework", "Parameter may not be null");
            }

            // If the flag indicates so, exclude the current state from the history.
            if(!excludeCurrentHistoryStep)
            {
                foreach(var historyData in cachedHistoryData)
                {
                    if(historyData.Value.AsynchronousData != null)
                    {
                        Tracing.WriteCachedHistoryCreateCommonHistoryBlockStart(framework.CurrentState, historyData.Key);
                        var startDataBlock = new CommonHistoryBlock(historyData.Value.StartStateData);
                        Tracing.WriteCachedHistoryCreateCommonHistoryBlockStop(framework.CurrentState, historyData.Key);

                        //Merge async data to start state data.
                        Tracing.WriteCachedHistoryMergeSerializedStart(framework.CurrentState, historyData.Key);
                        startDataBlock.Data.MergeSerialized(historyData.Value.AsynchronousData);
                        Tracing.WriteCachedHistoryMergeSerializedStop(framework.CurrentState, historyData.Key);

                        Tracing.WriteCachedHistoryWriteCriticalDataAsyncStart(framework.CurrentState, historyData.Key);
                        framework.GameLib.WriteCriticalData(CriticalDataScope.History,
                            Utility.GetHistoryStepPath(historyData.Key),
                            startDataBlock.ToBytes());
                        Tracing.WriteCachedHistoryWriteCriticalDataAsyncStop(framework.CurrentState, historyData.Key);
                    }
                    else
                    {
                        Tracing.WriteCachedHistoryWriteCriticalDataNoAsyncStart(framework.CurrentState, historyData.Key);
                        framework.GameLib.WriteCriticalData(CriticalDataScope.History,
                            Utility.GetHistoryStepPath(historyData.Key),
                            historyData.Value.StartStateData);
                        Tracing.WriteCachedHistoryWriteCriticalDataNoAsyncStop(framework.CurrentState, historyData.Key);
                    }

                    Tracing.WriteCachedHistoryUpdateHistoryListStart(framework.CurrentState, historyData.Key);
                    UpdateHistoryList(historyData.Key, framework);
                    Tracing.WriteCachedHistoryUpdateHistoryListStop(framework.CurrentState, historyData.Key);
                }
            }

            cachedHistoryData.Clear();
        }

        /// <summary>
        /// Updates and filters History List.
        /// </summary>
        /// <devdoc>
        /// Limitation of the history steps with the MaxHistorySteps is not used any more in the SDK state machine.
        /// Removal of any history step could lead to the situation that history becomes unusable.
        /// There is also no guarantee that any value of MaxHistorySteps would preserve enough space on safe storage
        /// to keep necessary history data. Therefore, the game must take care not to override the available
        /// safe storage size with the created history data.
        /// </devdoc>
        /// <param name="historyStepNumber">The record in history to store the data in.</param>
        /// <param name="framework">Framework to use for critical data access.</param>
        protected virtual void UpdateHistoryList(int historyStepNumber, IStateMachineFramework framework)
        {
            if(framework == null)
            {
                throw new ArgumentNullException("framework", "Parameter may not be null");
            }

            Tracing.UpdateHistoryListReadHistoryListStart(framework.CurrentState, historyStepNumber);
            var historyList = ReadHistoryList(framework) ?? new List<KeyValuePair<int, uint>>();
            Tracing.UpdateHistoryListReadHistoryListStop(framework.CurrentState, historyStepNumber);

            Tracing.UpdateHistoryListProcessHistoryListStart(framework.CurrentState, historyStepNumber);
            var priority = cachedHistoryData[historyStepNumber].Priority;
            historyList.Add(new KeyValuePair<int, uint>(historyStepNumber, priority));
            Tracing.UpdateHistoryListProcessHistoryListStop(framework.CurrentState, historyStepNumber);
            
            Tracing.UpdateHistoryListWriteHistoryListStart(framework.CurrentState, historyStepNumber);
            WriteHistoryList(framework, historyList);
            Tracing.UpdateHistoryListWriteHistoryListStop(framework.CurrentState, historyStepNumber);
        }

        /// <summary>
        /// Return the number of history steps recorded this game cycle.
        /// </summary>
        /// <param name="framework">Framework to access safe storage.</param>
        /// <returns>Number of history steps.</returns>
        public static uint GetHistoryStepCount(IStateMachineFramework framework)
        {
            var stepNumberList = framework.GameLib.ReadCriticalData<List<int>>
                                 (CriticalDataScope.History, HistoryListPath);
            return stepNumberList == null ? 0 : (uint)stepNumberList.Count;
        }

        /// <summary>
        /// Read HistoryList from critical data.
        /// Because we are unable to serialize/deserialize a list of KeyValuePair to critical data,
        /// we serialize/deserialize them into/from two lists instead.
        /// </summary>
        /// <param name="framework">Framework to use for critical data access.</param>
        /// <returns>Returns the History List.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="framework"/> is null.</exception>
        /// <exception cref="InvalidHistoryRecordException">Step number list and priority list
        /// are unequal in length.</exception>
        public static List<KeyValuePair<int, uint>> ReadHistoryList(IStateMachineFramework framework)
        {
            if(null == framework)
            {
                throw new ArgumentNullException("framework", "Parameter may not be null");
            }

            var stepNumberList = framework.GameLib.ReadCriticalData<List<int>>
                                 (CriticalDataScope.History, HistoryListPath);
            var priorityList = framework.GameLib.ReadCriticalData<List<uint>>
                               (CriticalDataScope.History, HistoryPriorityListPath);

            List<KeyValuePair<int, uint>> historyList = null;
            if(stepNumberList != null && priorityList != null)
            {
                if(stepNumberList.Count == priorityList.Count)
                {
                    historyList = stepNumberList.
                        Select((step, index) => new KeyValuePair<int, uint>(step, priorityList[index])).ToList();
                }
                else
                {
                    throw new InvalidHistoryRecordException("Invalid History List. stepNumberList.Count = " + stepNumberList.Count +
                                                            ", priorityList.Count = " + priorityList.Count);
                }
            }

            return historyList;
        }

        /// <summary>
        /// Write HistoryList to critical data.
        /// Because we are unable to serialize/deserialize a list of KeyValuePair to critical data,
        /// we serialize/deserialize them into/from two lists instead.
        /// </summary>
        /// <param name="framework">Framework to use for critical data access.</param>
        /// <param name="historyList">The historyList to write to critical data.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="framework"/> 
        /// or <paramref name="historyList"/> is null.</exception>
        protected static void WriteHistoryList(IStateMachineFramework framework, List<KeyValuePair<int, uint>> historyList)
        {
            if(null == framework)
            {
                throw new ArgumentNullException("framework", "Parameter may not be null");
            }

            if(null == historyList)
            {
                throw new ArgumentNullException("historyList", "Parameter may not be null");
            }

            List<int> stepNumberList = new List<int>();
            List<uint> priorityList = new List<uint>();

            historyList.ForEach(step =>
            {
                stepNumberList.Add(step.Key);
                priorityList.Add(step.Value);
            });

            framework.GameLib.WriteCriticalData(CriticalDataScope.History, HistoryListPath, stepNumberList);
            framework.GameLib.WriteCriticalData(CriticalDataScope.History, HistoryPriorityListPath, priorityList);
        }

        /// <summary>
        /// Returns the base critical data path with the current game mode prefixed.
        /// </summary>
        /// <param name="currentGameMode">The current <see cref="GameMode"/>.</param>
        /// <param name="basePath">The base path to prefix.</param>
        /// <returns>The prefixed path.</returns>
        public static string PrefixCriticalDataPath(GameMode currentGameMode, string basePath)
        {
            const string pathFormat = "{0}Mode/{1}";
            return string.Format(pathFormat, currentGameMode, basePath);
        }

        /// <summary>
        /// Perform an asynchronous update of the service specified in the asyncEventArgs. 
        /// </summary>
        /// <param name="framework">Framework to use for performing the update.</param>
        /// <param name="asyncEventArgs">Event containing the service which was updated.</param>
        protected virtual void UpdateAsyncData(
            IStateMachineFramework framework,
            StartFillAsynchronousRequestEventArgs asyncEventArgs)
        {
            var currentState = framework.CurrentState;
            if(!string.IsNullOrEmpty(currentState))
            {
                if(IsRecovering)
                {
                    // Use the data for the presentation state currently being executed by the recovery segment.
                    var asyncData = GetAsyncHistoryStateData(framework, RecoverySegment.PresentationStateName, asyncEventArgs);
                    if(asyncData != null)
                    {
                        RecoverySegment.UpdateAsyncData(RecoverySegment.PresentationStateName, framework, asyncData);
                    }
                }
                else
                {
                    var asyncData = GetAsynchStateData(framework, currentState, asyncEventArgs);
                    if (asyncData != null)
                    {
                        asyncServiceUpdateCache.Merge(asyncData);
                        framework.NotifyUpdateAsyncData(asyncData, asyncEventArgs.Transactional);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the unique identifier for a specific service accessor.
        /// </summary>
        /// <param name="stateName">Name of the state.</param>
        /// <param name="serviceProvider">The name of the service provider providing data for the state.</param>
        /// <param name="serviceAttribute">The name of the service accessor to search.</param>
        /// <returns>The unique identifier for a specific service accessor if found, otherwise null.</returns>
        public int? GetIdentifierForServiceAccessor(string stateName, string serviceProvider, string serviceAttribute)
        {
            int? identifier = null;

            if(cachedNonHistoryRequests.ContainsKey(stateName))
            {
                var requestData = cachedNonHistoryRequests[stateName];
                var matchedAccessor = (from data in requestData.Where(provider => provider.Key == serviceProvider)
                                       from accessor in data.Value
                                       where accessor.Service == serviceAttribute
                                       select accessor).FirstOrDefault();
                if(matchedAccessor != null)
                {
                    identifier = matchedAccessor.Identifier;
                }
            }
            return identifier;
        }

        /// <summary>
        /// Return the game timer controller.
        /// </summary>
        /// <param name="framework">The <see cref="IStateMachineFramework"/> framework to use.</param>
        /// <returns>An instance of <see cref="IGameTimerController"/></returns>
        public IGameTimerController GetGameTimerController(IStateMachineFramework framework)
        {
            if(gameTimerController == null)
            {
                gameTimerController = new GameTimerController(framework);
            }

            gameTimerController.Initialize();
            return gameTimerController;
        }

        /// <summary>
        /// Get identifiers for a specific service.
        /// </summary>
        /// <param name="stateName">Name of the state.</param>
        /// <param name="serviceProvider">The name of the service provider providing data for the state.</param>
        /// <param name="serviceAttribute">The name of the service accessor to search.</param>
        /// <returns>Identifiers array for a specific service.</returns>
        /// <remarks>
        /// When the specified service has parameters, the identifier is not unique. It depends on the parameter 
        /// value. So this method returns a identifier array to handle this case.
        /// </remarks>
        private int[] GetIdentifiersForService(string stateName, string serviceProvider, string serviceAttribute)
        {
            int[] identifiers = null;

            if(cachedNonHistoryRequests.ContainsKey(stateName))
            {
                var requestData = cachedNonHistoryRequests[stateName];
                identifiers = requestData
                    .Where(provider => provider.Key == serviceProvider)
                    .SelectMany(data => data.Value)
                    .Where(accessor => accessor.Service == serviceAttribute)
                    .Select(accessor => accessor.Identifier).ToArray();
            }
            return identifiers;
        }

        #region Recovery and History Implementation

        /// <summary>
        /// Read a <see cref="CommonHistoryBlock"/> from critical data.
        /// </summary>
        /// <param name="framework">A state machine framework for negotiating data.</param>
        /// <param name="stepNumber">The history step to ready</param>
        /// <returns></returns>
        public virtual CommonHistoryBlock ReadCommonHistoryBlock(IStateMachineFramework framework, int stepNumber)
        {
            var historyRawData = framework.GameLib.ReadCriticalData<byte[]>(
                                    CriticalDataScope.History,
                                    Utility.GetHistoryStepPath(stepNumber));
            if(historyRawData == null)
            {
                throw new InvalidHistoryRecordException("History Record " + stepNumber + " is missing.");
            }

            // Convert the serialized raw data to a history block.
            var historyBlock = new CommonHistoryBlock(historyRawData);

            return historyBlock;
        }

        #endregion

        #region IDisposable Members

        /// <inheritDoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// This function can execute by being called by the IDisposable interface or by the finalizer.
        /// If called by the IDisposable interface, then managed and unmanaged resources may be released.
        /// If called from a finalizer, then only unmanaged resources may be released.
        /// </summary>
        /// <param name="disposing">True if called by the IDisposable interface.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(!Disposed)
            {
                if(disposing)
                {
                    if(gameTimerController != null)
                    {
                        gameTimerController.Dispose();
                    }
                }

                Disposed = true;
            }
        }

        /// <summary>
        /// Add a game lib event listener to the game lib event listeners list.
        /// </summary>
        /// <param name="eventListener">The game lib event listener.</param>
        public void AddGameLibEventListener(object eventListener)
        {
            var listener = eventListener as IGameLibEventListener;
            if(listener != null)
            {
                gameLibEventListeners.Add(listener);
            }
        }

        /// <summary>
        /// Handles saving the response from a Foundation event to critical data in the game cycle scope.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="response">The event response to save.</param>
        /// <param name="path">The path to save the response to in critical data.</param>
        /// <exception cref="System.InvalidCastException">
        /// Thrown if <paramref name="sender"/> is not convertible to IGameLib.
        /// </exception>
        protected static void SaveEventResponse(object sender, EventArgs response, string path)
        {
            var gameLib = sender as IGameLib;
            if(gameLib == null)
            {
                throw new InvalidCastException("Sender was not convertible to IGameLib.");
            }
            gameLib.WriteCriticalData(CriticalDataScope.GameCycle, path, response);
        }
    }
}
