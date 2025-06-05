//-----------------------------------------------------------------------
// <copyright file = "StateMachineFramework.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Communication;
    using Communication.CommunicationLib;
    using Communication.Foundation;
    using Communication.Foundation.InterfaceExtensions.TiltManagement;
    using Communication.GL2PInterceptorLib;
    using Communication.Logic.CommServices;
    using CompactSerialization;
    using Core.Tracing;
    using RandomNumbers;
    using Services;
    using Timing;
    using Tracing;

    /// <summary>
    /// This class provides a framework for the execution of state machines.
    /// It utilizes the GameLib to provide a power hit tolerant state
    /// model.
    /// </summary>
    public class StateMachineFramework : IStateMachineFramework, ITransactionManager, IDisposable
    {
        #region Nested Classes

        /// <summary>
        /// Structure for storing current state information.
        /// </summary>
        [Serializable]
        internal struct StateStorage : ICompactSerializable
        {
            #region Fields

            /// <summary>
            /// The current state.
            /// </summary>
            public string CurrentState;

            /// <summary>
            /// The currently pending state.
            /// </summary>
            public string PendingState;

            /// <summary>
            /// The current state stage.
            /// </summary>
            public StateStage StateStage;

            #endregion

            #region ICompactSerializable Members

            /// <inheritdoc />
            public void Serialize(Stream stream)
            {
                CompactSerializer.Write(stream, CurrentState);
                CompactSerializer.Write(stream, PendingState);
                CompactSerializer.Write(stream, (int)StateStage);
            }

            /// <inheritdoc />
            public void Deserialize(Stream stream)
            {
                CurrentState = CompactSerializer.ReadString(stream);
                PendingState = CompactSerializer.ReadString(stream);
                StateStage = (StateStage)CompactSerializer.ReadInt(stream);
            }

            #endregion
        }

        #endregion

        #region Fields

        #region Constants

        /// <summary>
        /// Identifier used to represent an invalid state.
        /// </summary>
        public const string InvalidState = "INVALID";

        /// <summary>
        /// Critical data path to store the history record number.
        /// </summary>
        private const string HistoryRecordNumberPath = "historyStepNumber";

        /// <summary>
        /// Format string for the state information path.
        /// </summary>
        private const string StateInfoPathFormat = "{0}Mode/StateMachineFramework/StateInfo";

        /// <summary>
        /// Name to use for transactions initiated by the StateMachineFramework.
        /// </summary>
        private const string StateMachineTransactionName = "StateMachineFramework";

        /// <summary>
        /// Name to use for transactions opened after an interrupted transaction.
        /// </summary>
        private const string AfterInterruptedTransactionName = "AfterInterruptedTransaction";

        #endregion

        #region ReadOnly

        /// <summary>
        /// Lock object for queue.
        /// </summary>
        private readonly object transactionDataQueueLock = new object();

        /// <summary>
        /// Queue of delayed transactions.  Will be processed in order of queue.
        /// </summary>
        private readonly Queue<ITransactionalOperationSynchronization> transactionDataQueue =
            new Queue<ITransactionalOperationSynchronization>();

        ///<summary>
        /// Event that is triggered when a delayed transaction is added to <see cref="transactionDataQueue"/>.
        ///</summary>
        private readonly AutoResetEvent delayedTransactionEvent = new AutoResetEvent(false);

        /// <summary>
        /// Reference to the state machine being executed.
        /// </summary>
        private readonly IStateMachine stateMachine;

        /// <summary>
        /// This interface is used to consolidate the asynchronous service update.
        /// </summary>
        private readonly IConsolidateAsynchronousServiceUpdate consolidateAsynchronousServiceUpdate;

        /// <summary>
        /// Reference to the GameLib.
        /// </summary>
        private readonly IGameLib gameLib;

        /// <summary>
        /// Reference to the GameLib's restricted interface.
        /// </summary>
        private readonly IGameLibRestricted gameLibRestricted;

        /// <summary>
        /// The event processing is used to notify any event being processed, which could cause a service data update.
        /// </summary>
        private readonly IEventProcessing eventProcessing;

        /// <summary>
        /// Host for providing game logic services.
        /// </summary>
        private readonly IGameLogicServiceHost gameLogicServiceHost;

        /// <summary>
        /// The cached service request data for all the states.
        /// </summary>
        private readonly IEnumerable<KeyValuePair<string, ServiceRequestData>> serviceRequestDataCache;

        /// <summary>
        /// Reference to the notification interface of the presentation.
        /// </summary>
        private readonly IPresentationNotify presentationNotify;

        /// <summary>
        /// Critical data path used to store the current state information.
        /// </summary>
        private readonly string stateInfoPath;

        /// <summary>
        /// Event that is triggered when <see cref="running"/> changes.
        /// </summary>
        private readonly AutoResetEvent runningChangedEvent = new AutoResetEvent(false);

        /// <summary>
        /// This wait handle is used to hint an asynchronous service update.
        /// </summary>
        private readonly ManualResetEvent serviceUpdateWaitHandle = new ManualResetEvent(false);

        #endregion

        /// <summary>
        /// Field containing the current state information.
        /// </summary>
        private StateStorage stateInfo;

        /// <summary>
        /// Flag which indicates if the framework is currently running.
        /// </summary>
        private volatile bool running = true;

        /// <summary>
        /// Field for storing the exit reason.
        /// </summary>
        private volatile string exitReason = "Unknown reason for exit.";

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Flag indicating if the state machine is in a queued transaction.
        /// </summary>
        private bool inQueuedTransaction;

        /// <summary>
        /// Flag indicating that a power hit recovery has started.
        /// </summary>
        private bool recovering;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct an instance of the StateMachineFramework
        /// with the given StateMachine and GameLib.
        /// </summary>
        /// <param name="stateMachine">The StateMachine that this framework manage.</param>
        /// <param name="gameLib">The GameLib the framework will use.</param>
        /// <param name="presentation">
        /// Presentation client which the state machine will use for communication with the presentation.
        /// </param>
        /// <param name="gameLogicServiceHost">
        /// Host which allows states to receive messages from the presentation.
        /// </param>
        /// <param name="randomNumberProxyFactory">A factory object used to create random number proxies.</param>
        /// <param name="gameLogicAutomationService">The game logic automation service the framework will use.</param>
        /// <param name="serviceRequestDataCache">The cached service request data for all the states.</param>
        /// <param name="runMode">Indicates the mode the game is running in.</param>
        /// <exception cref="ArgumentNullException">
        /// If any of the parameters stateMachine, gameLib, presentation, or gameLogicServiceHost
        /// are null, then this exception may be thrown.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="presentation"/> does not support a conversion to
        /// <see cref="IPresentationNotify"/>.
        /// Thrown if <paramref name="gameLib"/> does not support a conversion to
        /// <see cref="IGameLibRestricted"/>.
        /// </exception>
        public StateMachineFramework(IStateMachine stateMachine,
                                     IGameLib gameLib,
                                     IPresentation presentation,
                                     IGameLogicServiceHost gameLogicServiceHost,
                                     IRandomNumberProxyFactory randomNumberProxyFactory,
                                     IGameLogicAutomationService gameLogicAutomationService = null,
                                     IEnumerable<KeyValuePair<string, ServiceRequestData>> serviceRequestDataCache = null,
                                     RunMode runMode = RunMode.Unspecified)
        {
            StateMachineFrameworkTracing.Log.FrameworkConstructionStart();

            // Assignments from arguments.
            this.stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            this.gameLib = gameLib ?? throw new ArgumentNullException(nameof(gameLib));
            Presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
            RandomNumberProxyFactory = randomNumberProxyFactory ?? throw new ArgumentNullException(nameof(randomNumberProxyFactory));
            this.gameLogicServiceHost = gameLogicServiceHost ?? throw new ArgumentNullException(nameof(gameLogicServiceHost));

            this.serviceRequestDataCache = serviceRequestDataCache;
            GameLogicAutomationService = gameLogicAutomationService;

            RunMode = runMode;

            // Other initialization work.
            stateInfo.PendingState = InvalidState;

            consolidateAsynchronousServiceUpdate = stateMachine as IConsolidateAsynchronousServiceUpdate;

            gameLibRestricted = gameLib as IGameLibRestricted;
            if(gameLibRestricted == null)
            {
                throw new ArgumentException("The passed in game lib could not be cast to IGameLibRestricted.", nameof(gameLib));
            }

            eventProcessing = gameLibRestricted.GetServiceInterface<IEventProcessing>();
            if(eventProcessing == null)
            {
                throw new ArgumentException(
                    "The passed in IGameLib interface must support an IEventProcessing Service Interface from its" +
                    " IGameLibRestricted interface.",
                    nameof(gameLib));
            }

            eventProcessing.EventProcessed += HandleEventProcessed;

            GameServiceController = new ServiceController();
            GameServiceController.StartFillAsynchronousRequest += HandleAsynchronousServiceUpdate;

            StateMachineFrameworkTracing.Log.StateMachineInitializationStart();
            InitializeStateMachine();
            StateMachineFrameworkTracing.Log.StateMachineInitializationStop();

            presentationNotify = presentation as IPresentationNotify;
            if(presentationNotify == null)
            {
                throw new ArgumentException("Parameter must have conversion to IPresentationNotify", nameof(presentation));
            }

            if(gameLib.GameContextMode == GameMode.Play)
            {
                presentationNotify.StartStateSent += HandleStartStateSent;
            }

            // Update the critical data paths according to the game mode.
            // The path reads something like "PlayMode/StateMachineFramework/StateInfo".
            stateInfoPath = string.Format(StateInfoPathFormat, gameLib.GameContextMode);

            StateMachineFrameworkTracing.Log.FrameworkConstructionStop();
        }

        #endregion

        #region ITransactionManager

        /// <summary>
        /// Event to be used for testing purposes to identify when transactions are requested.
        /// </summary>
        internal event EventHandler TransactionRequestSubmitted;

        /// <inheritdoc/>
        void ITransactionManager.SubmitTransactionRequest(ITransactionalOperationSynchronization transactionData)
        {
            lock(transactionDataQueueLock)
            {
                transactionDataQueue.Enqueue(transactionData);
            }
            delayedTransactionEvent.Set();

            TransactionRequestSubmitted?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void InterruptTransactionAndInvoke(Action function)
        {
            if(gameLibRestricted.TransactionOpen)
            {
                    //This function is not meant to be called from the processing transaction.
                    //We have a hard time detecting that situation, so instead here we look for the processing state(but
                    //not transaction) and explicitly allow the function to be called from a queued transaction.
                    //This situation is easier to detect but may require more maintenance if usage of this function
                    //is required in non-queued transactions where StateStage could potentially be Processing.
                    if(stateInfo.StateStage == StateStage.Processing && inQueuedTransaction != true)
                    {
                        var methodName = MethodBase.GetCurrentMethod()?.Name;
                        throw new InvalidStateStageException(methodName, stateInfo.StateStage, methodName +
                                                                                               " cannot be executed during the " +
                                                                                               stateInfo.StateStage +
                                                                                               " State Stage.");
                    }
                    gameLibRestricted.CloseTransaction();
                    function();
                    var transactionName = new StringBuilder(AfterInterruptedTransactionName);
                    transactionName.Append(function.Method.Name);
                    OpenTransaction(transactionName.ToString());
            }
            else
            {
                function();
            }
        }

        #endregion

        #region IStateMachineFramework Implementation

        /// <inheritdoc />
        [GameService]
        public string CurrentState => stateInfo.CurrentState;

        /// <inheritdoc />
        public RunMode RunMode { get; }

        /// <inheritdoc cref="IStateMachineFramework" />
        /// <inheritdoc cref="ITransactionManager" />
        public IGameLib GameLib => gameLib;

        /// <inheritdoc />
        public IPresentation Presentation { get; }

        /// <inheritdoc />
        public ServiceController GameServiceController { get; }

        /// <inheritdoc />
        public IGameLogicAutomationService GameLogicAutomationService { get; }

        /// <inheritdoc/>
        public IRandomNumberProxyFactory RandomNumberProxyFactory { get; }

        /// <inheritdoc />
        public void SetNextState(string nextState)
        {
            if(nextState == null)
            {
                throw new ArgumentNullException(nameof(nextState), "The next state parameter may not be null.");
            }

            if(!stateMachine.States.Contains(nextState))
            {
                throw new InvalidStateException(nextState, "The specified state does not exist in the state machine");
            }

            stateInfo.PendingState = nextState;
        }

        /// <inheritdoc />
        public void WaitForNonTransactionalEvents(Func<bool> processedCheck)
        {
            if(processedCheck == null)
            {
                throw new ArgumentNullException(nameof(processedCheck), "Argument may not be null.");
            }

            if(stateInfo.StateStage == StateStage.Processing)
            {
                var methodName = MethodBase.GetCurrentMethod()?.Name;
                throw new InvalidStateStageException(
                    methodName, stateInfo.StateStage,
                    methodName + " cannot be executed during the " + stateInfo.StateStage + " State Stage.");
            }

            using(var signal = new EventProcessingMonitor(eventProcessing, processedCheck))
            {
                if(!signal.CheckPredicate())
                {
                    //If there is a game transaction, then it needs to be closed before processing events.
                    gameLibRestricted.CloseTransaction();
                    do
                    {
                        var signaledHandle = gameLibRestricted.ProcessEvents(
                            new[]
                            {
                                runningChangedEvent,
                                delayedTransactionEvent,
                                serviceUpdateWaitHandle,
                                signal.EventProcessedWaitHandle
                            });

                        if(signaledHandle == delayedTransactionEvent)
                        {
                            ExecuteQueuedTransactions();
                        }
                        else if(signaledHandle == serviceUpdateWaitHandle)
                        {
                            // If serviceUpdateWaitHandler is signaled before any other event source,
                            // it hints that the service value is updated manually rather than by a event source.
                            SendCachedAsynchronousServiceData();
                        }
                        // signal.EventProcessedWaitHandle and serviceUpdateWaitHandle could be signaled at the same
                        // time, so don't use alternative check for them.
                    } while(!signal.CheckPredicate() && running);

                    if(running)
                    {
                        //This transaction will be used to finish the state handler.
                        OpenTransaction();
                    }
                }
                CheckRunningFlag(exitReason);
            }
        }

        /// <inheritdoc />
        public void ProcessEvents(Func<bool> processedCheck)
        {
            if(processedCheck == null)
            {
                throw new ArgumentNullException(nameof(processedCheck), "Argument may not be null.");
            }
            if(stateInfo.StateStage == StateStage.Processing)
            {
                var methodName = MethodBase.GetCurrentMethod()?.Name;
                throw new InvalidStateStageException(methodName, stateInfo.StateStage, methodName +
                                                                                       " cannot be executed during the " +
                                                                                       stateInfo.StateStage +
                                                                                       " State Stage.");
            }

            //If the event isn't already present close the current transaction and start processing events.
            if(!(gameLibRestricted.TransactionOpen && processedCheck()))
            {
                //If there is a game transaction, then it needs to be closed before processing events.
                gameLibRestricted.CloseTransaction();
                do
                {
                    var signaledHandle = gameLibRestricted.ProcessEvents(
                        new WaitHandle[]
                        {
                            runningChangedEvent,
                            delayedTransactionEvent,
                            serviceUpdateWaitHandle
                        });
                    if(signaledHandle == delayedTransactionEvent)
                    {
                        ExecuteQueuedTransactions();
                    }
                    else if(signaledHandle == serviceUpdateWaitHandle)
                    {
                        // If serviceUpdateWaitHandler is signaled before any other event source,
                        // it hints that the service value is updated manually rather than by a event source.
                        SendCachedAsynchronousServiceData();
                    }
                } while(!DoCheckProcess(processedCheck) && running);

                CheckRunningFlag(exitReason);

                //DoCheckProcess should leave a transaction open if the result was true.
                //This transaction can be used to finish the state handler.
            }
        }

        /// <inheritdoc />
        public TData WaitForCriticalData<TData>(string criticalDataPath) where TData : class
        {
            TData data = null;

            ProcessEvents(
                () =>
                {
                    data =
                        GameLib.ReadCriticalData<TData>(CriticalDataScope.GameCycle, criticalDataPath);

                    return data != null;
                });

            return data;
        }

        /// <inheritdoc />
        public GameLogicGenericMsg GetPresentationEvent(Type firstType, params Type[] otherTypes)
        {
            return GetPresentationEvent(Timeout.Infinite, firstType, otherTypes);
        }

        /// <inheritdoc />
        public GameLogicGenericMsg GetPresentationEvent(int timeout, Type firstType, params Type[] otherTypes)
        {
            //Close any open transaction before waiting for an event.
            gameLibRestricted.CloseTransaction();

            var isTimeout = timeout >= 0;
            var startTime = TimeSpanWatch.Now;

            var requestedEventReceived = false;
            GameLogicGenericMsg returnEvent = null;

            if(stateInfo.StateStage == StateStage.Processing)
            {
                var methodName = MethodBase.GetCurrentMethod()?.Name;
                throw new InvalidStateStageException(methodName, stateInfo.StateStage, methodName +
                                                                                       " cannot be executed during the " +
                                                                                       stateInfo.StateStage.ToString()
                                                                                       + " State Stage.");
            }

            if(otherTypes == null)
            {
                //This will not happen when called normally, but could
                //occur if invoked dynamically.
                throw new ArgumentNullException(nameof(otherTypes), "Argument may not be null.");
            }

            //Put all of the types in a single array for convenience.
            var eventTypes = GetEventTypes(firstType, otherTypes);

            while(!requestedEventReceived && running)
            {
                while(gameLogicServiceHost.IsMessagePending && running)
                {
                    var message = gameLogicServiceHost.GetNextMessage();

                    CheckForPresentationTilt(message);

                    if(eventTypes.Contains(message.GetType()))
                    {
                        requestedEventReceived = true;
                        returnEvent = message;
                    }
                }

                if(!requestedEventReceived)
                {
                    //Process any queued transactions.
                    ExecuteQueuedTransactions();

                    var actualTimeout = Timeout.Infinite;

                    if(isTimeout)
                    {
                        var elapsedTime = TimeSpanWatch.Now - startTime;
                        actualTimeout = Math.Max(timeout - (int)elapsedTime.TotalMilliseconds, 0);
                    }

                    //Process GameLib events prior to checking the presentation events
                    //again.
                    var signaledHandle =
                        gameLibRestricted.ProcessEvents(actualTimeout,
                                                        new[]
                                                        {
                                                            gameLogicServiceHost.MessageReceivedHandle,
                                                            runningChangedEvent,
                                                            delayedTransactionEvent,
                                                            serviceUpdateWaitHandle
                                                        });
                    if(signaledHandle == serviceUpdateWaitHandle)
                    {
                        // If serviceUpdateWaitHandler is signaled before any other event source,
                        // it hints that the service value is updated manually rather than by a event source.
                        SendCachedAsynchronousServiceData();
                    }
                    //Make sure there isn't any open transaction.
                    gameLibRestricted.CloseTransaction();
                }

                var runTime = TimeSpanWatch.Now - startTime;

                //Check to see if the timeout has elapsed.
                if(isTimeout && runTime.TotalMilliseconds > timeout)
                {
                    break;
                }
            }

            //Throw an exception if the end was triggered so that the state machine doesn't complete this state.
            CheckRunningFlag(exitReason);

            //Create a transaction to use for the remainder of the state.
            OpenTransaction();
            return returnEvent;
        }

        /// <inheritdoc />
        public TGameLogicGenericMsg GetPresentationEvent<TGameLogicGenericMsg>()
            where TGameLogicGenericMsg : GameLogicGenericMsg
        {
            var message = GetPresentationEvent(typeof(TGameLogicGenericMsg));

            //No need to check if the return is null, because the called GetPresentationEvent will not
            //return null.
            return message as TGameLogicGenericMsg;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation only supports history for transactional updates. If the developer needs to record
        /// non-transaction updates in history, then they will need a custom implementation.
        /// </remarks>
        public virtual void NotifyUpdateAsyncData(DataItems data, bool transactional)
        {
            if(data == null)
            {
                throw new ArgumentNullException(nameof(data), "Argument may not be null");
            }

            if(CurrentState == null)
            {
                throw new InvalidStateException("null", "Notification of asynchronous data without a valid state.");
            }

            if(transactional && stateMachine.IsAsynchronousHistoryState(CurrentState))
            {
                var recordNumber = GetRecordNumber();

                stateMachine.WriteUpdateHistory(CurrentState, data, recordNumber, this);
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        /// <param name="disposing">true if called from dispose function</param>
        protected virtual void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                // Unregister the presentation event handlers.
                // The Game Lib's game context mode has been changed at this point,
                // so just unregister the event handlers without checking it.

                presentationNotify.StartStateSent -= HandleStartStateSent;

                eventProcessing.EventProcessed -= HandleEventProcessed;

                // Unregister the Game Lib event handlers for all the service providers.
                var listeningProviders = GameServiceController.ServiceProviders.
                    Select(provider => provider.Value.ProviderObject).OfType<IGameLibEventListener>();

                foreach(var listeningProvider in listeningProviders)
                {
                    listeningProvider.UnregisterGameLibEvents(gameLib);
                }

                // If there are any pending queued transaction cancel them as the state machine framework
                // is shutting down.
                CancelQueuedTransactions();

                // Clean up anything state machine framework related within the state machine.
                stateMachine.CleanUp(this);

                //If the state machine supports IDisposable dispose the state machine to clean up anything non state machine
                //framework related.
                var listeningStateMachineDisposabe = stateMachine as IDisposable;
                listeningStateMachineDisposabe?.Dispose();

                //These events implement IDisposable, so there is no need to check if they converted.
                (delayedTransactionEvent as IDisposable).Dispose();
                (runningChangedEvent as IDisposable).Dispose();
                (serviceUpdateWaitHandle as IDisposable).Dispose();
                disposed = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Begin the execution of the state machine. This function
        /// will continue to execute until an exception is encountered
        /// or until an event from the GameLib indicates that the game
        /// is to stop executing.
        /// </summary>
        /// <exception cref="InvalidPendingStateException">
        /// If the next state is not set by the time execution of the committed
        /// callback is complete, then this exception will be thrown.
        /// </exception>
        public void Execute()
        {
            StateMachineFrameworkTracing.Log.FrameworkExecutionStart();

            try
            {
                gameLogicServiceHost.Start();

                StateMachineFrameworkTracing.Log.ReadStateConfigurationStart();
                ReadStateConfiguration();
                StateMachineFrameworkTracing.Log.ReadStateConfigurationStop();

                while(running)
                {
                    //Cache the state before executing the handler so the metrics can
                    //be recorded for the correct state.
                    // ReSharper disable once RedundantAssignment
                    var state = stateInfo.CurrentState;

                    switch(stateInfo.StateStage)
                    {
                        case StateStage.Processing:
                            StateMachineFrameworkTracing.Log.StateProcessingStageStart(state);
                            ExecuteProcessing();
                            StateMachineFrameworkTracing.Log.StateProcessingStageStop(state);
                            break;

                        case StateStage.Committed:
                            StateMachineFrameworkTracing.Log.StateCommittedStageStart(state);
                            ExecuteCommitted();
                            StateMachineFrameworkTracing.Log.StateCommittedStageStop(state);
                            break;
                    }
                    ExecuteQueuedTransactions();
                }
            }
            finally
            {
                gameLogicServiceHost.Stop();
                CancelQueuedTransactions();
            }

            StateMachineFrameworkTracing.Log.FrameworkExecutionStop();
        }

        /// <summary>
        /// Stop the execution of the state machine. This is intended to be used
        /// to stop the state machine when running in standalone mode.
        /// </summary>
        /// <param name="message">Message indicating the source of the exit request.</param>
        public void EndExecution(string message)
        {
            exitReason = message;
            running = false;
            runningChangedEvent.Set();

            GameLifeCycleTracing.Log.StateMachineEnded(message);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Tell if this StateMachineFramework can enter power hit recovery mode.
        /// </summary>
        /// <returns>True if this StateMachineFramework can enter power hit recovery mode, false otherwise.</returns>
        protected bool CanEnterPowerHitRecoveryMode()
        {
            // Return false if recovery is disabled in the StateMachine.
            if(!stateMachine.RecoveryEnabled)
            {
                return false;
            }

            // Return false if this is a cold start.
            if(string.IsNullOrEmpty(stateInfo.CurrentState))
            {
                return false;
            }

            // Return false if there is no game in progress.
            // This must be checked before GetRecordNumber() below, since the history
            // critical data scope cannot be accessed when the game is in idle state.
            if(stateInfo.CurrentState == stateMachine.InitialState)
            {
                return false;
            }

            // Return false if there is no transaction open.
            // This must be checked before QueryGameCycleState(), which
            // throws an exception if there is no transaction open.
            if(!gameLibRestricted.TransactionOpen)
            {
                return false;
            }

            // Return false if gameLib is in a state where history critical data cannot be read. This must be
            // checked before GetHistoryStepCount() since we are checking history scoped critical data there.
            if(!Utility.CanAccessHistoryDataInPlayMode(gameLib.QueryGameCycleState()))
            {
                return false;
            }

            // Return true only if the game history has recorded at least one state to replay in recovery mode.
            return StateMachineBase.GetHistoryStepCount(this) > 0;
        }

        /// <summary>
        /// Read state information from safe storage for Play mode,
        /// or initialize the state information to proper values for History and Utility mode.
        /// This function should be used when initially loading the state machine.
        /// </summary>
        /// <exception cref="InvalidStateException">
        /// If the initial state of the state machine is not configured,
        /// or the initial state is not present, then this exception will
        /// be thrown.
        /// </exception>
        protected void ReadStateConfiguration()
        {
            switch(gameLib.GameContextMode)
            {
                case GameMode.Play:
                case GameMode.Utility:
                    {
                        //Create a transaction for reading current state data.
                        OpenTransaction();

                        if(gameLib.GameContextMode == GameMode.Play)
                        {
                            stateInfo = gameLib.ReadCriticalData<StateStorage>(CriticalDataScope.Theme, stateInfoPath);

                            // Put the stateMachine in power hit recovery mode if the required conditions are met.
                            if(CanEnterPowerHitRecoveryMode())
                            {
                                stateInfo.CurrentState = stateMachine.RecoveryState;
                                stateInfo.StateStage = StateStage.Processing;
                                recovering = true;
                            }
                        }

                        stateMachine.ReadConfiguration(GameLib);

                        gameLibRestricted.CloseTransaction();
                    }
                    break;
            }

            //The theme is loading for the first time.
            if(stateInfo.CurrentState == null)
            {
                stateInfo.CurrentState = stateMachine.InitialState;
            }

            //The state machine is not configured correctly.
            if(stateInfo.CurrentState == null)
            {
                throw new InvalidStateException("state", "Initial state not configured.");
            }

            if(!stateMachine.States.Contains(stateInfo.CurrentState))
            {
                throw new InvalidStateException(stateInfo.CurrentState, "State not present in state machine.");
            }

            //Either cold start, or the pending state has not been set.
            if(stateInfo.PendingState == null)
            {
                stateInfo.PendingState = InvalidState;
            }

            if(stateInfo.PendingState != InvalidState && !stateMachine.States.Contains(stateInfo.PendingState))
            {
                //Could happen during development if state machine is changed without clearing safe storage.
                throw new InvalidStateException(stateInfo.PendingState, "Pending state not present in state machine.");
            }
        }

        /// <summary>
        /// Write state information to safe storage for Play mode.
        /// Critical data is read only in History mode, so no write is done.
        /// Utility Mode needs no critical data, so no write is done.
        /// This function assumes that an open transaction already exists.
        /// </summary>
        protected void WriteStateConfiguration()
        {
            if(gameLib.GameContextMode == GameMode.Play && !stateMachine.IsRecovering)
            {
                gameLib.WriteCriticalData(CriticalDataScope.Theme, stateInfoPath, stateInfo);
            }
        }

        /// <summary>
        /// Execute the processing state of a state.
        /// </summary>
        protected void ExecuteProcessing()
        {
            if(stateMachine.ContainsStateHandler(CurrentState, stateInfo.StateStage))
            {
                OpenTransaction();

                stateMachine.ExecuteStateStage(CurrentState, stateInfo.StateStage, this);

                // Move to the next stage of the state.
                stateInfo.StateStage = StateStage.Committed;

                // Write the state and state stage to critical data.
                WriteStateConfiguration();

                gameLibRestricted.CloseTransaction();
            }
            else
            {
                // Move to the next stage of the state.
                stateInfo.StateStage = StateStage.Committed;
            }
        }

        /// <summary>
        /// Handle StartStateSent events from the presentation client.
        /// </summary>
        /// <param name="sender">Origin of the event.</param>
        /// <param name="presentationNotification">Notification data for the event.</param>
        /// <exception cref="ArgumentNullException">Thrown when presentationNotification parameter is null.</exception>
        protected void HandleStartStateSent(object sender, PresentationNotificationEventArgs presentationNotification)
        {
            if(presentationNotification == null)
            {
                throw new ArgumentNullException(nameof(presentationNotification), "Argument may not be null");
            }

            //History should not be recorded while in idle.
            //If history must be recorded in idle, then the record number will have to be moved out of the GameCycle
            //critical data scope.
            if(GameLib.QueryGameCycleState() != GameCycleState.Idle && !stateMachine.IsRecovering)
            {
                var recordNumber = GetRecordNumber();

                ++recordNumber;

                stateMachine.WriteStartStateHistory(presentationNotification.StateName, presentationNotification.Data,
                                                    recordNumber, this);

                GameLib.WriteCriticalData(CriticalDataScope.GameCycle, HistoryRecordNumberPath, recordNumber);
            }
        }

        /// <summary>
        /// Execute the committed stage of a state.
        /// </summary>
        protected void ExecuteCommitted()
        {
            OpenTransaction();

            stateMachine.ExecuteStateStage(CurrentState, stateInfo.StateStage, this);

            //Game developer might update the asynchronous service data manually after the presentation completion.
            //In this case, we have to clear the asynchronous update explicitly.
            consolidateAsynchronousServiceUpdate?.ClearAsynchronousData();

            //If pending state is null then throw an exception.
            if(stateInfo.PendingState == InvalidState)
            {
                throw new InvalidPendingStateException(CurrentState,
                                                       "Next state not set during execution of current state.");
            }

            // When stateMachine.IsRecovering is changed to false, end power hit recovery.
            if(recovering && !stateMachine.IsRecovering)
            {
                // Resume normal game play by restoring the play state stored in critical data.
                stateInfo = gameLib.ReadCriticalData<StateStorage>(CriticalDataScope.Theme, stateInfoPath);
                recovering = false;
            }
            else
            {
                //Prepare the state stage for the next state.
                stateInfo.StateStage = StateStage.Processing;

                //A transaction should always be open by this point. Waiting for events
                //from either the presentation or foundation will cause the initial
                //transaction to be closed, but receiving an event will open a transaction.

                stateInfo.CurrentState = stateInfo.PendingState;
                stateInfo.PendingState = InvalidState;
            }

            WriteStateConfiguration();

            gameLibRestricted.CloseTransaction();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Execute the given delegate to determine if event processing has yielded the results desired by the
        /// developer. The delegate will be executed within a transaction.
        /// </summary>
        /// <param name="processedCheck">Delegate to execute.</param>
        /// <returns>
        /// True if the delegate execution resulted in true. If true, then a transaction will be open on the functions
        /// return.
        /// </returns>
        private bool DoCheckProcess(Func<bool> processedCheck)
        {
            OpenTransaction();
            var checkResult = processedCheck();

            //Only close this transaction if the result was false.
            //Otherwise keep it open for the return from process events.
            if(!checkResult)
            {
                gameLibRestricted.CloseTransaction();
            }
            return checkResult;
        }

        /// <summary>
        /// Combine a list of types with a single type and check that
        /// none of the passed types are null.
        /// </summary>
        /// <param name="firstType">A single type to add.</param>
        /// <param name="otherTypes">A list of types to add.</param>
        /// <returns>A list containing all passed types.</returns>
        /// <exception cref="ArgumentException">
        /// If any type in the list of types, or the single type, are null
        /// then this exception will be thrown.
        /// </exception>
        private static Type[] GetEventTypes(Type firstType, Type[] otherTypes)
        {
            var eventTypes = new Type[otherTypes.Length + 1];
            otherTypes.CopyTo(eventTypes, 0);
            eventTypes[eventTypes.Length - 1] = firstType;

            //Null check each type.
            if(eventTypes.Any(type => type == null))
            {
                throw new ArgumentException(
                    "One of the event types requested was null. Cannot request a null event.");
            }
            return eventTypes;
        }

        /// <summary>
        /// Get the history record number from critical data.
        /// </summary>
        /// <returns>The history record number.</returns>
        /// <remarks>Returns a new record number if there is not one in critical data.</remarks>
        private int GetRecordNumber()
        {
            return GameLib.ReadCriticalData<int>(CriticalDataScope.GameCycle, HistoryRecordNumberPath);
        }

        /// <summary>
        /// Open a transaction with the foundation. This function will return once the transaction has
        /// successfully been opened.
        /// </summary>
        /// <remarks>
        /// The foundation will not open a transaction until all events from the foundation have been processed.
        /// </remarks>
        private void OpenTransaction()
        {
            OpenTransaction(StateMachineTransactionName);
        }

        /// <summary>
        /// Open a transaction with the foundation. This function will return once the transaction has
        /// successfully been opened.
        /// </summary>
        /// <param name="name">Name of the Transaction.</param>
        /// <remarks>
        /// The foundation will not open a transaction until all events from the foundation have been processed.
        /// </remarks>
        private void OpenTransaction(string name)
        {
            var transactionOpen = false;
            while (!transactionOpen && running)
            {
                var result = gameLibRestricted.CreateTransaction(name);

                switch(result)
                {
                    case ErrorCode.EventWaitingForProcess:
                        gameLibRestricted.ProcessEvents(0);
                        break;
                    case ErrorCode.OpenTransactionExisted:
                        transactionOpen = true;
                        break;
                    case ErrorCode.NoError:
                        transactionOpen = true;
                        break;
                }
            }

            CheckRunningFlag("Exit while processing transactions: "+exitReason);
        }

        /// <summary>
        /// Execute any queued transactions.
        /// </summary>
        private void ExecuteQueuedTransactions()
        {
            int queueCount;
            lock(transactionDataQueueLock)
            {
                queueCount = transactionDataQueue.Count;
            }

            while(queueCount > 0 && !gameLibRestricted.TransactionOpen)
            {
                StateMachineFrameworkTracing.Log.ExecuteQueuedTransactionStart(stateInfo.CurrentState, queueCount);

                ITransactionalOperationSynchronization nextTransactionData;

                lock(transactionDataQueueLock)
                {
                    nextTransactionData = transactionDataQueue.Dequeue();
                    queueCount = transactionDataQueue.Count;
                }
                delayedTransactionEvent.Reset();

                OpenTransaction(nextTransactionData.Name);
                inQueuedTransaction = true;
                nextTransactionData.FireDelayedTransaction();
                inQueuedTransaction = false;
                gameLibRestricted.CloseTransaction();

                StateMachineFrameworkTracing.Log.ExecuteQueuedTransactionStop(stateInfo.CurrentState, queueCount);
            }
        }

        /// <summary>
        /// Cancel any pending queued transactions.
        /// </summary>
        private void CancelQueuedTransactions()
        {
            lock(transactionDataQueueLock)
            {
                while(transactionDataQueue.Any())
                {
                    var transactionData = transactionDataQueue.Dequeue();
                    transactionData.CancelTransaction();
                }
                delayedTransactionEvent.Reset();
            }
        }

        /// <summary>
        /// If the end has been triggered, throw the exception that will stop the
        /// state machine.
        /// </summary>
        /// <param name="exitInfo">The reason why the state machine was stopped.</param>
        private void CheckRunningFlag(string exitInfo)
        {
            if (!running)
            {
                throw new StateMachineForcedExitException(exitInfo);
            }
        }

        /// <summary>
        /// Check whether a message received from the presentation is for posting or clearing a tilt.
        /// If yes, forward the tilt request to the Foundation.
        /// </summary>
        /// <param name="message">The message received from the presentation to check.</param>
        private void CheckForPresentationTilt(GameLogicGenericMsg message)
        {
            //If the tilt manager is unavailable tilts are ignored.
            var tiltManager = gameLib.GetInterface<ITiltManagement>();
            if(tiltManager == null)
            {
                return;
            }

            // There is no easy way for the Presentation to know whether a tilt is present.
            // Therefore, we'll have to do the check for the Presentation.
            switch(message.MessageType)
            {
                case GameLogicMessageType.PostPresentationTilt:
                    {
                        var postTiltMessage = message as GameLogicPostPresentationTiltMsg;

                        // Post the tilt as requested.
                        // Check if the tilt is present before trying to post it.
                        // Re-posting a tilt will throw an exception.
                        if(postTiltMessage != null &&
                           !tiltManager.TiltPresent(postTiltMessage.TiltKey))
                        {
                            OpenTransaction();

                            tiltManager.PostTilt(postTiltMessage.PresentationTilt,
                                                 postTiltMessage.TiltKey,
                                                 postTiltMessage.TitleFormatArgs,
                                                 postTiltMessage.MessageFormatArgs);

                            gameLibRestricted.CloseTransaction();
                        }
                    }
                    break;

                case GameLogicMessageType.ClearPresentationTilt:
                    {
                        var clearTiltMessage = message as GameLogicClearPresentationTiltMsg;

                        // clear the tilt as requested.
                        // Check if the tilt is present before trying to clear it.
                        // Clearing a non-present tilt will throw an exception.
                        if(clearTiltMessage != null &&
                           tiltManager.TiltPresent(clearTiltMessage.TiltKey))
                        {
                            OpenTransaction();

                            tiltManager.ClearTilt(clearTiltMessage.TiltKey);

                            gameLibRestricted.CloseTransaction();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Initialize the state machine.
        /// </summary>
        /// <remarks>
        /// This is a wrapper used for recording metrics.
        /// </remarks>
        private void InitializeStateMachine()
        {
            stateMachine.Initialize(this);
            if(serviceRequestDataCache != null)
            {
                stateMachine.UpdateRequiredDataCache(serviceRequestDataCache);
            }
        }

        /// <summary>
        /// Event handler for EventProcessed from EventCoordinator.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleEventProcessed(object sender, EventArgs eventArgs)
        {
            SendCachedAsynchronousServiceData();
        }

        /// <summary>
        /// Handle the asynchronous service value updated event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The payload of the event.</param>
        private void HandleAsynchronousServiceUpdate(object sender, StartFillAsynchronousRequestEventArgs eventArgs)
        {
            serviceUpdateWaitHandle.Set();
        }

        /// <summary>
        /// Sends out the cached asynchronous service data.
        /// </summary>
        private void SendCachedAsynchronousServiceData()
        {
            consolidateAsynchronousServiceUpdate?.UpdateAsynchronousData(this);

            serviceUpdateWaitHandle.Reset();
        }

        #endregion
    }
}
