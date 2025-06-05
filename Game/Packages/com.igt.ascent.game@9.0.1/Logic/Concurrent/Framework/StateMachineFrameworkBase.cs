// -----------------------------------------------------------------------
// <copyright file = "StateMachineFrameworkBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Communication.Platform;
    using Communication.Platform.Interfaces;
    using Game.Core.Communication.CommunicationLib;
    using Game.Core.Communication.Logic.CommServices;
    using Game.Core.Logic.Services;
    using Interfaces;

    /// <summary>
    /// A base implementation of the state machine framework.
    /// The framework is responsible for executing a state machine.
    /// </summary>
    /// <typeparamref name="TState">
    /// The type of logic state contained in the state machine.
    /// </typeparamref>
    /// <typeparamref name="TFrameworkInitialization">
    /// The type of interface to access the functionalities available for
    /// a state machine and its states during the state machine initialization.
    /// </typeparamref>
    /// <typeparamref name="TFrameworkExecution">
    /// The type of interface to access the functionalities available for
    /// a state machine and its states during the state machine execution.
    /// </typeparamref>
    /// <typeparamref name="TLibInterface">
    /// The type of lib interface used to communicate with Foundation.
    /// </typeparamref>
    /// <remarks>
    /// This class does implement some interface APIs.  But as the interfaces implemented
    /// are runner specific, this base class does not declare to implement any interface.
    /// The interface declaration is done by the derived classes.
    /// </remarks>
    internal abstract class StateMachineFrameworkBase<TState, TFrameworkInitialization, TFrameworkExecution, TLibInterface> : IDisposable
        where TState : ILogicState<TFrameworkInitialization, TFrameworkExecution>
        where TFrameworkInitialization : IFrameworkInitialization
        where TFrameworkExecution : IFrameworkExecution
        where TLibInterface : IAppLib
    {
        #region Constants

        protected const string PathStateInfo = "StateInfo";

        #endregion

        #region Private Fields

        private readonly string frameworkIdentifier;
        private readonly IFrameworkRunner frameworkRunner;
        private readonly DataItems asyncServiceUpdateCache = new DataItems();
        private bool isInitialized;
        private bool isPresentationComplete;
        private bool isDisposed;
        private TState currentState;

        #endregion

        #region Protected Fields

        protected readonly GameMode GameMode;
        protected readonly TLibInterface LibInterface;
        protected readonly IStateMachine<TState, TFrameworkInitialization, TFrameworkExecution, TLibInterface> StateMachine;
        protected readonly ServiceController ServiceControllerInstance;
        protected readonly IPresentation PresentationClient;
        protected readonly ServiceRequestDataMaps ServiceRequestDataMaps;

        protected StateInfo StateInfo;
        protected TransactionWeight CurrentTransactionWeight;

        protected readonly DisposableCollection DisposableCollection = new DisposableCollection();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the fields in the base state machine framework.
        /// </summary>
        /// <param name="frameworkRunner">
        /// The interface to the extra functionalities provided to this state machine framework
        /// (that is not accessible by the state machine).
        /// </param>
        /// <param name="libInterface">
        /// The lib used to communicate with Foundation.
        /// </param>
        /// <param name="gameMode">
        /// The game mode the state machine runs in.
        /// </param>
        /// <param name="stateMachine">
        /// The state machine to run.
        /// </param>
        /// <param name="presentationClient">
        /// The interface to communicate with the presentation via GL2P.
        /// </param>
        /// <param name="serviceRequestDataMap">
        /// The service request data for the state machine.
        /// </param>
        /// <param name="frameworkIdentifier">
        /// The identifier of the framework.  Used for logging.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        /// <exception cref="ConcurrentLogicException">
        /// Thrown when the initial state name returned by <paramref name="stateMachine"/> is null.
        /// </exception>
        protected StateMachineFrameworkBase(IFrameworkRunner frameworkRunner,
                                            TLibInterface libInterface,
                                            GameMode gameMode,
                                            IStateMachine<TState, TFrameworkInitialization, TFrameworkExecution, TLibInterface> stateMachine,
                                            IPresentation presentationClient,
                                            IDictionary<string, ServiceRequestData> serviceRequestDataMap,
                                            string frameworkIdentifier = null)
        {
            if(libInterface == null)
            {
                throw new ArgumentNullException(nameof(libInterface));
            }

            if(stateMachine == null)
            {
                throw new ArgumentNullException(nameof(stateMachine));
            }

            if(stateMachine.InitialStateName == null)
            {
                throw new ConcurrentLogicException("The state machine does not specify an initial state.");
            }

            if(serviceRequestDataMap == null)
            {
                throw new ArgumentNullException(nameof(serviceRequestDataMap));
            }

            this.frameworkIdentifier = frameworkIdentifier;
            this.frameworkRunner = frameworkRunner ?? throw new ArgumentNullException(nameof(frameworkRunner));
            LibInterface = libInterface;
            GameMode = gameMode;
            StateMachine = stateMachine;
            PresentationClient = presentationClient ?? throw new ArgumentNullException(nameof(presentationClient));
            ServiceRequestDataMaps = new ServiceRequestDataMaps(serviceRequestDataMap);

            ServiceControllerInstance = new ServiceController();
            ServiceControllerInstance.StartFillAsynchronousRequest += HandleStartFillAsynchronousRequest;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the state machine by running the current state and also
        /// transitioning from one state to another as instructed by the states.
        /// </summary>
        public void ExecuteState()
        {
            if(!isInitialized)
            {
                StateMachine.Initialize(FrameworkInitialization);

                foreach(var logicState in StateMachine.GetAllStates())
                {
                    logicState.Initialize(FrameworkInitialization, StateMachine);
                }

                DoTransaction(TransactionWeight.Light,
                              "StateMachineFrameworkBase.ReadConfiguration",
                              ReadConfiguration);

                isInitialized = true;

                return;
            }

            FrameworkDebug.LogState(frameworkIdentifier, StateInfo.CurrentStateName, StateInfo.CurrentStep);

            var transactionName = StateInfo.ToString();

            switch(StateInfo.CurrentStep)
            {
                case StateStep.Processing:
                {
                    DoStateTransaction(transactionName, ExecuteProcessing);
                    break;
                }
                case StateStep.CommittedPreWait:
                {
                    DoStateTransaction(transactionName, ExecuteCommittedPreWait);
                    break;
                }
                case StateStep.CommittedWait:
                {
                    // Wait does not need a transaction.
                    ExecuteCommittedWait();
                    break;
                }
                case StateStep.CommittedPostWait:
                {
                    DoStateTransaction(transactionName, ExecuteCommittedPostWait);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region IFrameworkInitialization Implementation

        /// <remarks/>
        public IServiceController ServiceController => ServiceControllerInstance;

        /// <remarks/>
        public event EventHandler<GameLogicGenericMsg> PresentationEventReceived;

        #endregion

        #region IFrameworkExecution Implementation

        /// <remarks/>
        public void SetNextState(string nextStateName)
        {
            if(nextStateName == null)
            {
                throw new ArgumentNullException(nameof(nextStateName));
            }

            // This is to verify the pending state is valid.
            StateMachine.GetState(nextStateName);

            StateInfo.PendingStateName = nextStateName;
        }

        /// <remarks/>
        public void StartPresentationState()
        {
            CannotBeCalledFromStep(StateStep.Processing);

            var serviceRequestData = ServiceRequestDataMaps.GetPlayRequestData(StateInfo.CurrentStateName);
            var stateData = ServiceControllerInstance.FillRequest(serviceRequestData);

            RecordHistory(stateData, StateInfo.CurrentStateName, true);

            PresentationClient.StartState(StateInfo.CurrentStateName, stateData);
        }

        /// <remarks/>
        public bool WaitForCondition(Func<bool, bool> conditionChecker)
        {
            MustBeCalledFromStep(StateStep.CommittedWait);

            var result = false;

            // Before waiting, check if the condition is already met.
            DoTransaction(TransactionWeight.Light, StateInfo + " Initial Condition Check", () => result = conditionChecker(true));

            if(!result)
            {
                result = frameworkRunner.WaitForCondition(conditionChecker);
            }

            return result;
        }

        /// <remarks/>
        public bool WaitForLightweightTransactionalCondition(Func<bool> conditionChecker)
        {
            MustBeCalledFromStep(StateStep.CommittedWait);

            var result = false;

            // Before waiting, check if the condition is already met.
            DoTransaction(TransactionWeight.Light, StateInfo + " Initial LW Condition Check", () => result = conditionChecker());

            if(!result)
            {
                result = frameworkRunner.WaitForLightweightTransactionalCondition(conditionChecker);
            }

            return result;
        }

        /// <remarks/>
        public bool WaitForNonTransactionalCondition(Func<bool> conditionChecker)
        {
            MustBeCalledFromStep(StateStep.CommittedWait);

            // Before waiting, check if the condition is already met.
            var result = conditionChecker();

            if(!result)
            {
                result = frameworkRunner.WaitForNonTransactionalCondition(conditionChecker);
            }

            return result;
        }

        /// <remarks/>
        public bool WaitForPresentationStateComplete()
        {
            MustBeCalledFromStep(StateStep.CommittedWait);

            // Before waiting, check if the condition is already met.
            var result = isPresentationComplete;

            if(!result)
            {
                result = frameworkRunner.WaitForNonTransactionalCondition(() => isPresentationComplete);
            }

            return result;
        }

        /// <remarks/>
        public void RegisterEventQueue(IEventQueue eventQueue)
        {
            frameworkRunner.RegisterEventQueue(eventQueue);
        }

        /// <remarks/>
        public void UnregisterEventQueue(IEventQueue eventQueue)
        {
            frameworkRunner.UnregisterEventQueue(eventQueue);
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(isDisposed)
            {
                return;
            }

            if(disposing)
            {
                // Clean up the state machine and its states.
                StateMachine.CleanUp(FrameworkInitialization);
                foreach(var logicState in StateMachine.GetAllStates())
                {
                    logicState.CleanUp(FrameworkInitialization);
                }

                // State machine framework is responsible for disposing the state machine.
                // ReSharper disable once SuspiciousTypeConversion.Global
                if(StateMachine is IDisposable disposableStateMachine)
                {
                    disposableStateMachine.Dispose();
                }

                DisposableCollection.Dispose();
            }

            isDisposed = true;
        }

        #endregion

        #region Internal Methods For Testing Purpose

        /// <summary>
        /// Peeks at the current state info.
        /// This is for testing only.
        /// </summary>
        /// <returns>
        /// A copy of current state info.
        /// </returns>
        internal StateInfo PeekCurrentStateInfo()
        {
            return new StateInfo
                       {
                           CurrentStateName = StateInfo.CurrentStateName,
                           CurrentStep = StateInfo.CurrentStep
                       };
        }

        #endregion

        #region Abstract Members

        /// <summary>
        /// Gets the interface implementation for framework initialization.
        /// </summary>
        protected abstract TFrameworkInitialization FrameworkInitialization { get; }

        /// <summary>
        /// Gets the interface implementation for framework execution.
        /// </summary>
        protected abstract TFrameworkExecution FrameworkExecution { get; }

        /// <summary>
        /// Reads the state info from the critical data.
        /// Used for power hit recovery.
        /// </summary>
        /// <returns>
        /// The state info retrieved from the critical data.
        /// </returns>
        protected abstract StateInfo ReadStateInfo();

        /// <summary>
        /// Writes the state into to the critical data.
        /// Used for power hit tolerant.
        /// </summary>
        protected abstract void SaveStateInfo();

        /// <summary>
        /// Updates the tracked history data.
        /// </summary>
        /// <param name="newData">
        /// The new data to track.
        /// </param>
        /// <param name="stateName">
        /// The name of the state this data appeared in.
        /// </param>
        /// <param name="startingPresentationState">
        /// A flag indicating whether this method is called for starting a presentation state.
        /// </param>
        protected abstract void RecordHistory(DataItems newData, string stateName, bool startingPresentationState = false);

        /// <summary>
        /// Gets the async service data to send over GL2P during the History mode.
        /// </summary>
        /// <param name="historyStateName">
        /// Outputs the name of the state that is currently being displayed for history.
        /// This is usually different from the logic history state currently running,
        /// and is only known to the history state machine, not the state machine framework.
        /// </param>
        /// <param name="providerName">
        /// The name of the provider whose async service data is being updated.
        /// </param>
        /// <param name="serviceSignatures">
        /// The signatures of the async services being updated.
        /// </param>
        /// <returns>
        /// The async service data filled for the History mode.
        /// </returns>
        protected abstract DataItems GetHistoryAsyncData(out string historyStateName,
                                                         string providerName,
                                                         IList<ServiceSignature> serviceSignatures);

        #endregion

        #region Protected Methods

        /// <summary>
        /// Ensures that the method is not being called from <paramref name="step"/>.
        /// </summary>
        /// <param name="step">The step to check against.</param>
        /// <exception cref="ConcurrentLogicException">
        /// Thrown when current step is <paramref name="step"/>.
        /// </exception>
        protected void CannotBeCalledFromStep(StateStep step)
        {
            if(StateInfo.CurrentStep == step)
            {
                // Get the caller method name.
                var methodName = new StackFrame(1).GetMethod().Name;

                throw new ConcurrentLogicException(
                    $"The method {methodName} cannot be called from step {step}.  This occurred in state {StateInfo}.");
            }
        }

        /// <summary>
        /// Build a critical data path prefix based on the data owner and the game mode.
        /// </summary>
        /// <param name="owner">The critical data owner.</param>
        /// <param name="gameMode">The game mode.</param>
        /// <returns>
        ///  A path prefix, something like "GameStateMachineFramework/PlayMode/".
        /// </returns>
        protected static string BuildPathPrefix(string owner, GameMode gameMode)
        {
            return $"{owner}/{gameMode}Mode/";
        }

        /// <summary>
        /// Reads configuration data needed to run the state machine.
        /// This function needs a LW transaction.
        /// </summary>
        protected virtual void ReadConfiguration()
        {
            StateInfo = ReadStateInfo();

            // Cold boot.
            if(StateInfo == null)
            {
                StateInfo = new StateInfo();
                GotoState(StateMachine.InitialStateName);
            }
            // Warm boot.
            else
            {
                currentState = StateMachine.GetState(StateInfo.CurrentStateName);
            }

            StateMachine.ReadConfiguration(LibInterface);
        }

        /// <summary>
        /// Handles an event sent by the presentation thread.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        protected void HandlePresentationEventReceived(object sender, GameLogicGenericMsg eventArgs)
        {
            // We only handle the basic presentation state complete scenario for the state machine.
            // For more complicated tasks, the state machine should implement the presentation event
            // handler of its own.
            if(eventArgs is GameLogicPresentationStateCompleteMsg presentationCompleteMsg)
            {
                isPresentationComplete = StateInfo != null &&
                                         StateInfo.CurrentStateName == presentationCompleteMsg.StateName;
            }

            // Raise the presentation event.
            PresentationEventReceived?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Handles the event that occurs when an event has been processed from any of the registered event queue..
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        protected void HandleEventQueuePostProcessing(object sender, EventArgs eventArgs)
        {
            if(!isInitialized)
            {
                return;
            }

            SendCachedAsyncServiceUpdates();
        }

        /// <summary>
        /// Gets the async service data for async services that satisfy the following conditions:
        /// <list type="bullet">
        /// <item>The service is requested by the game in LDC (i.e. appears in <paramref name="serviceRequestData"/>, AND</item>
        /// <item>The service data is being updated as indicated by <paramref name="providerName"/> and <paramref name="serviceSignatures"/>.</item>
        /// </list>
        /// </summary>
        /// <param name="serviceRequestData">
        /// The service data requested by the game (for a given state) in LDC.
        /// </param>
        /// <param name="providerName">
        /// The name of the provider whose async service data is being updated.
        /// </param>
        /// <param name="serviceSignatures">
        /// The signatures of the async services being updated.
        /// </param>
        /// <returns>
        /// The async service data filled.
        /// </returns>
        protected DataItems GetAsyncServiceData(ServiceRequestData serviceRequestData,
                                                string providerName,
                                                IList<ServiceSignature> serviceSignatures)
        {
            DataItems result = null;

            // Update async game service data only if the state has data requests for the specified services
            // in the specified provider.
            if(serviceRequestData.TryGetValue(providerName, out var requestedServices))
            {
                // The name of services that have updated their data.
                var updatedServiceNames = serviceSignatures.Select(signature => signature.ServiceName);

                // The requested services in current state's provider being affected by the update.
                var affectedServices = requestedServices.Where(service => updatedServiceNames.Contains(service.Service)).ToList();

                if(affectedServices.Count > 0)
                {
                    // A selective service request that only requests the data being affected by the update.
                    var selectedServiceRequest = new ServiceRequestData
                                                     {
                                                         { providerName, affectedServices }
                                                     };

                    // Get the async service data requested by the update.
                    result = ServiceControllerInstance.FillAsynchronousRequest(selectedServiceRequest);
                }
            }

            return result;
        }

        /// <summary>
        /// This method is called when the state machine framework is transitioning from one state to another.
        /// </summary>
        protected virtual void OnStateTransition()
        {
        }

        #endregion

        #region Private Methods

        #region State Execution

        /// <summary>
        /// Ensures that the method is being called from <paramref name="step"/>.
        /// </summary>
        /// <param name="step">The step to check against.</param>
        /// <exception cref="ConcurrentLogicException">
        /// Thrown when current step is not <paramref name="step"/>.
        /// </exception>
        private void MustBeCalledFromStep(StateStep step)
        {
            if(StateInfo.CurrentStep != step)
            {
                // Get the caller method name.
                var methodName = new StackFrame(1).GetMethod().Name;

                throw new ConcurrentLogicException(
                    $"The method {methodName} cannot be called from steps other than {step}.  This occurred in state {StateInfo}.");
            }
        }

        private void DoTransaction(TransactionWeight transactionWeight, string transactionName, Action action)
        {
            // Cache the transaction weight for other functions to use.
            CurrentTransactionWeight = transactionWeight;

            switch(CurrentTransactionWeight)
            {
                case TransactionWeight.Heavy:
                {
                    frameworkRunner.DoTransaction(transactionName, action);
                    break;
                }
                case TransactionWeight.Light:
                {
                    frameworkRunner.DoTransactionLite(transactionName, action);
                    break;
                }
                default:
                {
                    // We allow a step not to require a transaction to provide
                    // the most flexibility for the state machine implementation.
                    action();
                    break;
                }
            }

            // Reset the current transaction weight.
            CurrentTransactionWeight = TransactionWeight.None;
        }

        private void DoStateTransaction(string transactionName, Action action)
        {
            DoTransaction(currentState.GetTransactionWeight(StateInfo.CurrentStep),
                          transactionName,
                          action);
        }

        private void GotoState(string nextStateName)
        {
            if(nextStateName == null)
            {
                throw new ConcurrentLogicException("The next state name is null.  Please check if SetNextState has been called.");
            }

            var callingState = StateInfo.ToString();

            currentState = StateMachine.GetState(nextStateName);

            StateInfo.CurrentStateName = nextStateName;
            StateInfo.PendingStateName = null;
            StateInfo.CurrentStep = currentState.InitialStep;

            // There are two scenarios where GotoState could be called outside a transaction:
            // 1. It is called in the CommittedWait step.
            // 2. It is called in a non-wait step whose TransactionWeight is None.
            if(CurrentTransactionWeight != TransactionWeight.None)
            {
                OnStateTransition();
                SaveStateInfo();
            }
            else
            {
                void DoAction()
                {
                    OnStateTransition();
                    SaveStateInfo();
                }

                DoTransaction(TransactionWeight.Light,
                              "StateMachineFrameworkBase.GotoState in " + callingState,
                              DoAction);
            }

            // Reset state specific data.
            isPresentationComplete = false;
        }

        private void ExecuteProcessing()
        {
            var stepControl = currentState.Processing(FrameworkExecution);

            switch(stepControl)
            {
                case ProcessingStepControl.GoNext:
                {
                    StateInfo.CurrentStep = StateStep.CommittedPreWait;
                    break;
                }
                case ProcessingStepControl.ExitState:
                {
                    GotoState(StateInfo.PendingStateName);
                    break;
                }
                case ProcessingStepControl.SkipToWait:
                {
                    StateInfo.CurrentStep = StateStep.CommittedWait;
                    break;
                }
                default:
                {
                    throw new ConcurrentLogicException(
                        $"ProcessingStepControl value {stepControl} is an invalid return value for a {StateInfo.CurrentStep} call.");
                }
            }

            // The transition from Processing to Committed should be saved.
            if(CurrentTransactionWeight != TransactionWeight.None)
            {
                SaveStateInfo();
            }
            else
            {
                DoTransaction(TransactionWeight.Light,
                              "StateMachineFrameworkBase.ExecuteProcessing in " + StateInfo.CurrentStateName,
                              SaveStateInfo);
            }
        }

        private void ExecuteCommittedPreWait()
        {
            var stepControl = currentState.CommittedPreWait(FrameworkExecution);

            // Send cached async service updates before updating StateInfo.
            SendCachedAsyncServiceUpdates();

            switch(stepControl)
            {
                case PreWaitStepControl.GoNext:
                {
                    StateInfo.CurrentStep = StateStep.CommittedWait;
                    break;
                }
                case PreWaitStepControl.ExitState:
                {
                    GotoState(StateInfo.PendingStateName);
                    break;
                }
                case PreWaitStepControl.RepeatPreWait:
                {
                    StateInfo.CurrentStep = StateStep.CommittedPreWait;
                    break;
                }
                default:
                {
                    throw new ConcurrentLogicException(
                        $"PreWaitStepControl value {stepControl} is an invalid return value for a {StateInfo.CurrentStep} call.");
                }
            }

            // Do not save StateInfo outside GotoState as Committed stage is not power hit tolerant.
        }

        private void ExecuteCommittedWait()
        {
            var stepControl = currentState.CommittedWait(FrameworkExecution);

            switch(stepControl)
            {
                case WaitStepControl.GoNext:
                {
                    StateInfo.CurrentStep = StateStep.CommittedPostWait;
                    break;
                }
                case WaitStepControl.ExitState:
                {
                    GotoState(StateInfo.PendingStateName);
                    break;
                }
                case WaitStepControl.BackToPreWait:
                {
                    StateInfo.CurrentStep = StateStep.CommittedPreWait;
                    break;
                }
                case WaitStepControl.RepeatWait:
                {
                    // Keep current step as Wait.
                    break;
                }
                default:
                {
                    throw new ConcurrentLogicException(
                        $"WaitStepControl value {stepControl} is an invalid return value for a {StateInfo.CurrentStep} call.");
                }
            }

            // Do not save StateInfo outside GotoState as Committed stage is not power hit tolerant.
        }

        private void ExecuteCommittedPostWait()
        {
            var stepControl = currentState.CommittedPostWait(FrameworkExecution);

            // Send cached async service updates before updating StateInfo.
            SendCachedAsyncServiceUpdates();

            switch(stepControl)
            {
                case PostWaitStepControl.ExitState:
                {
                    GotoState(StateInfo.PendingStateName);
                    break;
                }
                case PostWaitStepControl.BackToPreWait:
                {
                    StateInfo.CurrentStep = StateStep.CommittedPreWait;
                    break;
                }
                case PostWaitStepControl.BackToWait:
                {
                    StateInfo.CurrentStep = StateStep.CommittedWait;
                    break;
                }
                default:
                {
                    throw new ConcurrentLogicException(
                        $"PostWaitStepControl value {stepControl} is an invalid return value for a {StateInfo.CurrentStep} call.");
                }
            }

            // Do not save StateInfo outside GotoState as Committed stage is not power hit tolerant.
        }

        #endregion

        #region State Async Service Updates

        /// <summary>
        /// Handles the event raised by the service controller asking for async data.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleStartFillAsynchronousRequest(object sender, StartFillAsynchronousRequestEventArgs eventArgs)
        {
            if(!isInitialized)
            {
                // Do nothing if async update occurs before the state machine framework is initialized.
                return;
            }

            switch(GameMode)
            {
                case GameMode.Play:
                {
                    var asyncServiceData = GetPlayAsyncData(StateInfo.CurrentStateName,
                                                            eventArgs.ServiceProviderName,
                                                            eventArgs.ServiceSignatures);

                    if(asyncServiceData != null)
                    {
                        RecordHistory(asyncServiceData, StateInfo.CurrentStateName);

                        asyncServiceUpdateCache.Merge(asyncServiceData);
                    }

                    break;
                }
                case GameMode.History:
                {
                    var historyLiveData = GetHistoryAsyncData(out var historyStateName,
                                                              eventArgs.ServiceProviderName,
                                                              eventArgs.ServiceSignatures);

                    if(historyLiveData != null)
                    {
                        // Must set HistoryData, otherwise LDC won't work correctly
                        historyLiveData.HistoryData = HistoryMetaData.Create();

                        PresentationClient.UpdateAsynchData(historyStateName, historyLiveData);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Gets the async service data to send over GL2P during the Play mode.
        /// </summary>
        /// <param name="stateName">
        /// The name of the state to get the async service data for.
        /// </param>
        /// <param name="providerName">
        /// The name of the provider whose async service data is being updated.
        /// </param>
        /// <param name="serviceSignatures">
        /// The signatures of the async services being updated.
        /// </param>
        /// <returns>
        /// The async service data filled for the Play mode.
        /// </returns>
        private DataItems GetPlayAsyncData(string stateName,
                                           string providerName,
                                           IList<ServiceSignature> serviceSignatures)
        {
            var serviceRequestData = ServiceRequestDataMaps.GetPlayAsyncRequestData(stateName);

            return GetAsyncServiceData(serviceRequestData, providerName, serviceSignatures);
        }

        /// <summary>
        /// Sends cached async data over GL2P.
        /// This is to reduce GL2P traffic when one processed event causes multiple async updates.
        /// </summary>
        private void SendCachedAsyncServiceUpdates()
        {
            if(asyncServiceUpdateCache.Any())
            {
                // Send cached data over GL2P.
                // StateManager will check state name when updating async data for the state being displayed.
                // Async data for states that are not in display will be discarded.
                // Therefore we don't have to worry about this method being invoked at wrong times.
                PresentationClient.UpdateAsynchData(StateInfo.CurrentStateName, asyncServiceUpdateCache);

                // Clear the cache.
                asyncServiceUpdateCache.Clear();
            }
        }

        #endregion

        #endregion
    }
}