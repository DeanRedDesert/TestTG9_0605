// -----------------------------------------------------------------------
// <copyright file = "GameStateMachineFramework.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using Communication.Platform;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Communication.CommunicationLib;
    using Game.Core.Logic.Services;
    using Game.Core.Tilts;
    using IGT.Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// The state machine framework for a game state machine running in a coplayer.
    /// </summary>
    internal sealed class GameStateMachineFramework
        : StateMachineFrameworkBase<IGameState, IGameFrameworkInitialization, IGameFrameworkExecution, ICoplayerLib>,
          IGameFrameworkInitialization,
          IGameFrameworkExecution,
          IGameParcelComm,
          ICoplayerTiltController,
          IHistoryPresentationControl
    {
        #region Private Fields

        /// <summary>
        /// The runner of the framework.
        /// </summary>
        private readonly IGameFrameworkRunner gameFrameworkRunner;

        /// <summary>
        /// The reference of coplayer lib, used for communication with Foundation.
        /// </summary>
        private readonly ICoplayerLibRestricted coplayerLibRestricted;

        /// <summary>
        /// The object exposing observables.
        /// </summary>
        private readonly CoplayerObservableCollection coplayerObservableCollection;

        /// <summary>
        /// The object exposing shell exposition properties.
        /// </summary>
        private readonly ShellExposition shellExposition;

        /// <summary>
        /// The critical data block for safe storing state info.
        /// </summary>
        private readonly SingleCriticalData<StateInfo> stateInfoBlock;

        /// <summary>
        /// Event table for events received from Shell.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> shellEventHandlers = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// The object tracking and writing history data.
        /// </summary>
        private readonly HistoryManager historyManager;

        /// <summary>
        /// The name of the last history state.
        /// </summary>
        private string lastHistoryStateName;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public GameStateMachineFramework(IGameFrameworkRunner frameworkRunner,
                                         ICoplayerLib coplayerLib,
                                         ICoplayerLibRestricted coplayerLibRestricted,
                                         IGameStateMachine gameStateMachine,
                                         IPresentation presentationClient,
                                         IDictionary<string, ServiceRequestData> serviceRequestDataMap,
                                         CoplayerInitData initData,
                                         string frameworkIdentifier = null)
            : base(frameworkRunner,
                   coplayerLib,
                   coplayerLib?.Context.GameMode ?? GameMode.Invalid,
                   gameStateMachine,
                   presentationClient,
                   serviceRequestDataMap,
                   frameworkIdentifier)
        {
            this.coplayerLibRestricted = coplayerLibRestricted ?? throw new ArgumentNullException(nameof(coplayerLibRestricted));
            if(initData == null)
            {
                throw new ArgumentNullException(nameof(initData));
            }

            gameFrameworkRunner = frameworkRunner;

            // Initialize observables
            coplayerObservableCollection = new CoplayerObservableCollection(initData);

            // Initialize shell exposition
            shellExposition = new ShellExposition(initData);

            // Critical data paths
            var prefix = BuildPathPrefix(nameof(GameStateMachineFramework), GameMode);
            stateInfoBlock = new SingleCriticalData<StateInfo>(prefix + PathStateInfo);

            // History handling
            historyManager = new HistoryManager(gameFrameworkRunner.ShellHistoryQuery,
                                                LibInterface.HistoryStore,
                                                LibInterface.GamePlayStore);

            // Event handling
            CreateShellEventHandlerTable();
            SubscribeEventHandlers();
        }

        #endregion

        #region IGameFramework*** Implementation

        /// <inheritdoc>
        ///     <cref>IGameFrameworkInitialization</cref>
        ///     <cref>IGameFrameworkExecution</cref>
        /// </inheritdoc>
        public ICoplayerLib CoplayerLib => LibInterface;

        /// <inheritdoc>
        ///     <cref>IGameFrameworkInitialization</cref>
        ///     <cref>IGameFrameworkExecution</cref>
        /// </inheritdoc>
        public IShellExposition ShellExposition => shellExposition;

        /// <inheritdoc>
        ///     <cref>IGameFrameworkInitialization</cref>
        /// </inheritdoc>
        public ICoplayerObservableCollection ObservableCollection => coplayerObservableCollection;

        /// <inheritdoc>
        ///     <cref>IGameFrameworkInitialization</cref>
        ///     <cref>IGameFrameworkExecution</cref>
        /// </inheritdoc>

        // Use "this" at the moment.  See if we need a separate module in the future.
        public IGameParcelComm GameParcelComm => this;

        /// <inheritdoc>
        ///     <cref>IGameFrameworkExecution</cref>
        /// </inheritdoc>
        public ICoplayerTiltController TiltController => this;

        /// <inheritdoc>
        ///     <cref>IGameFrameworkExecution</cref>
        /// </inheritdoc>
        public IHistoryPresentationControl HistoryControl => this;

        /// <inheritdoc>
        ///     <cref>IGameFrameworkExecution</cref>
        /// </inheritdoc>
        public int HistoryStepCount => historyManager.StepCount;

        /// <inheritdoc>
        ///     <cref>IGameFrameworkExecution</cref>
        /// </inheritdoc>
        public bool SendEventToShell(CustomCoplayerEventArgs eventArgs)
        {
            return gameFrameworkRunner.SendEventToShell(eventArgs);
        }

        /// <inheritdoc>
        ///     <cref>IGameFrameworkInitialization</cref>
        ///     <cref>IGameFrameworkExecution</cref>
        /// </inheritdoc>
        public event EventHandler<CustomShellEventArgs> ShellEventReceived;

        #endregion

        #region IGameParcelComm Implementation

        /// <inheritdoc/>
        public event EventHandler<NonTransactionalParcelCallReceivedEventArgs> NonTransactionalParcelCallReceivedEvent;

        /// <inheritdoc/>
        public event EventHandler<TransactionalParcelCallReceivedEventArgs> TransactionalParcelCallReceivedEvent;

        /// <inheritdoc/>
        public ParcelCallStatus SendNonTransactionalParcelCall(ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            return gameFrameworkRunner.ParcelCallSender.SendNonTransactionalParcelCall(targetEndpoint, payload);
        }

        /// <inheritdoc/>
        public ParcelCallResult SendTransactionalParcelCall(ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            if(CurrentTransactionWeight == TransactionWeight.Heavy)
            {
                throw new ConcurrentLogicException("Transactional parcel call cannot be sent within a heavyweight transaction." +
                                                   "This occurred in " + StateInfo);
            }

            return gameFrameworkRunner.ParcelCallSender.SendTransactionalParcelCall(targetEndpoint, payload);
        }

        #endregion

        #region ICoplayerTiltController Implementation

        /// <inheritdoc/>
        public event EventHandler<ShellTiltClearedByAttendantEventArgs> TiltClearedByAttendantEvent;

        /// <inheritdoc/>
        public bool PostTilt(ITilt tilt, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat)
        {
            if(CurrentTransactionWeight == TransactionWeight.Heavy)
            {
                throw new ConcurrentLogicException("Post tilt cannot be sent within a heavyweight transaction." +
                                                   "This occurred in " + StateInfo);
            }

            return gameFrameworkRunner.ShellTiltSender.PostTilt(tilt, key, titleFormat, messageFormat, LibInterface.Context.CoplayerId);
        }

        /// <inheritdoc/>
        public bool ClearTilt(string key)
        {
            if(CurrentTransactionWeight == TransactionWeight.Heavy)
            {
                throw new ConcurrentLogicException("Clear tilt call cannot be sent within a heavyweight transaction." +
                                                   "This occurred in " + StateInfo);
            }

            return gameFrameworkRunner.ShellTiltSender.ClearTilt(key, LibInterface.Context.CoplayerId);
        }

        /// <inheritdoc/>
        public bool ClearAllTilts()
        {
            if(CurrentTransactionWeight == TransactionWeight.Heavy)
            {
                throw new ConcurrentLogicException("Clear all tilts cannot be sent within a heavyweight transaction." +
                                                   "This occurred in " + StateInfo);
            }

            return gameFrameworkRunner.ShellTiltSender.ClearAllTilts(LibInterface.Context.CoplayerId);
        }

        /// <inheritdoc/>
        public bool IsTilted()
        {
            return gameFrameworkRunner.ShellTiltSender.IsTilted(LibInterface.Context.CoplayerId);
        }

        /// <inheritdoc/>
        public bool TiltPresent(string key)
        {
            return gameFrameworkRunner.ShellTiltSender.TiltPresent(key, LibInterface.Context.CoplayerId);
        }

        #endregion

        #region IHistoryPresentationControl Implementation

        /// <inheritdoc/>
        public void SetCoplayerHistoryData(string stateName, DataItems data)
        {
            var historyLiveRequest = ServiceRequestDataMaps.GetHistoryRequestData(stateName);
            var historyLiveData = ServiceControllerInstance.FillRequest(historyLiveRequest);

            data.Merge(historyLiveData);

            PresentationClient.StartState(stateName, data);

            lastHistoryStateName = stateName;
        }

        /// <inheritdoc/>
        public void SetShellHistoryData(CothemePresentationKey coplayerPresentationKey, int stepNumber, string stateName, DataItems data)
        {
            SendEventToShell(new ShellHistoryUpdateEventArgs(coplayerPresentationKey,
                                                             new HistoryRecord(stepNumber, stateName, data)));
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        protected override IGameFrameworkInitialization FrameworkInitialization => this;

        /// <inheritdoc/>
        protected override IGameFrameworkExecution FrameworkExecution => this;

        /// <inheritdoc/>
        protected override void ReadConfiguration()
        {
            base.ReadConfiguration();
            if(CoplayerLib.Context.GameMode == GameMode.Play && CoplayerLib.GameCyclePlay.GameCycleState != GameCycleState.Idle)
            {
                historyManager.ReadConfiguration();
            }
        }

        ///<inheritdoc/>
        protected override StateInfo ReadStateInfo()
        {
            StateInfo result = null;

            if(LibInterface.Context.GameMode == GameMode.Play)
            {
                var dataBlock = LibInterface.ThemeStore.Read(stateInfoBlock.Name);

                result = dataBlock.GetCriticalData<StateInfo>(stateInfoBlock.Name);
            }

            return result;
        }

        ///<inheritdoc/>
        protected override void SaveStateInfo()
        {
            if(LibInterface.Context.GameMode == GameMode.Play)
            {
                stateInfoBlock.Data = StateInfo;

                LibInterface.ThemeStore.Write(stateInfoBlock);
            }
        }

        /// <inheritdoc/>
        protected override void RecordHistory(DataItems newData, string stateName, bool startingPresentationState = false)
        {
            // All history manager calls need a transaction.
            if(CurrentTransactionWeight != TransactionWeight.None)
            {
                if(historyManager.HistoryWriteEnabled && startingPresentationState)
                {
                    historyManager.BeginHistoryStep(stateName);
                }

                historyManager.TrackData(newData);
            }
        }

        /// <inheritdoc/>
        protected override DataItems GetHistoryAsyncData(out string historyStateName, string providerName, IList<ServiceSignature> serviceSignatures)
        {
            historyStateName = lastHistoryStateName ?? string.Empty;

            var historyLiveRequest = ServiceRequestDataMaps.GetHistoryAsyncRequestData(historyStateName);

            return GetAsyncServiceData(historyLiveRequest, providerName, serviceSignatures);
        }

        /// <inheritdoc/>
        protected override void OnStateTransition()
        {
            base.OnStateTransition();
            historyManager.EndHistoryStep();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            UnsubscribeEventHandlers();

            coplayerObservableCollection.Dismiss();

            // Call base method last as it modifies the IsDisposed flag.
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Subscribes to any coplayer lib, or game framework runner events
        /// that persists through the state machine framework's life spans.
        /// </summary>
        private void SubscribeEventHandlers()
        {
            coplayerLibRestricted.GameCyclePlayRestricted.GameCycleStateTransitioned += HandleGameCycleStateTransitioned;

            gameFrameworkRunner.PresentationEventReceived += HandlePresentationEventReceived;
            gameFrameworkRunner.ShellEventReceived += HandleShellEventReceived;
            gameFrameworkRunner.EventQueuePostProcessing += HandleEventQueuePostProcessing;
        }

        /// <summary>
        /// Unsubscribes from any coplayer lib, or game framework runner events
        /// that persists through the state machine framework's life spans.
        /// </summary>
        private void UnsubscribeEventHandlers()
        {
            coplayerLibRestricted.GameCyclePlayRestricted.GameCycleStateTransitioned -= HandleGameCycleStateTransitioned;

            gameFrameworkRunner.PresentationEventReceived -= HandlePresentationEventReceived;
            gameFrameworkRunner.ShellEventReceived -= HandleShellEventReceived;
            gameFrameworkRunner.EventQueuePostProcessing -= HandleEventQueuePostProcessing;
        }

        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <typeparam name="TEventArgs">Type of event to raise.</typeparam>
        /// <param name="handler">The event handler that raises the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void RaiseEvent<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs eventArgs) where TEventArgs : EventArgs
        {
            handler?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Creates the event lookup table for events received from Shell.
        /// </summary>
        private void CreateShellEventHandlerTable()
        {
            // Events should be listed in alphabetical order!

            shellEventHandlers[typeof(DisplayControlEventArgs)] =
                (s, e) => HandleDisplayControlState((DisplayControlEventArgs)e);

            shellEventHandlers[typeof(PropertyRelayEventArgs)] =
                (s, e) => HandlePropertyRelay((PropertyRelayEventArgs)e);

            shellEventHandlers[typeof(NonTransactionalParcelCallReceivedEventArgs)] =
                (s, e) => RaiseEvent(NonTransactionalParcelCallReceivedEvent, (NonTransactionalParcelCallReceivedEventArgs)e);

            shellEventHandlers[typeof(TransactionalParcelCallReceivedEventArgs)] =
                (s, e) => RaiseEvent(TransactionalParcelCallReceivedEvent, (TransactionalParcelCallReceivedEventArgs)e);

            shellEventHandlers[typeof(ShellTiltClearedByAttendantEventArgs)] =
                (s, e) => RaiseEvent(TiltClearedByAttendantEvent, (ShellTiltClearedByAttendantEventArgs)e);

            shellEventHandlers[typeof(CustomShellEventArgs)] =
                (s, e) => RaiseEvent(ShellEventReceived, (CustomShellEventArgs)e);
        }

        /// <summary>
        /// Handles an event received from Shell.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleShellEventReceived(object sender, EventDispatchedEventArgs eventArgs)
        {
            var isHandled = false;
            var eventType = eventArgs.DispatchedEventType;

            if(eventArgs.DispatchedEventType.IsSubclassOf(typeof(CustomShellEventArgs)))
            {
                eventType = typeof(CustomShellEventArgs);
            }
            else if(eventArgs.DispatchedEventType.IsSubclassOf(typeof(PropertyRelayEventArgs)))
            {
                eventType = typeof(PropertyRelayEventArgs);
            }

            if(shellEventHandlers.ContainsKey(eventType))
            {
                var handler = shellEventHandlers[eventType];
                if(handler != null)
                {
                    handler(sender, eventArgs.DispatchedEvent);

                    isHandled = true;
                }
            }

            eventArgs.IsHandled = isHandled;
        }

        /// <summary>
        /// Handles a display control event sent by Shell.
        /// </summary>
        /// <param name="eventArgs">The event data.</param>
        private void HandleDisplayControlState(DisplayControlEventArgs eventArgs)
        {
            coplayerObservableCollection.HandleDisplayControlState(eventArgs);
        }

        /// <summary>
        /// Handles a property relay event sent by Shell.
        /// </summary>
        /// <param name="eventArgs">The event data.</param>
        private void HandlePropertyRelay(PropertyRelayEventArgs eventArgs)
        {
            // Update observables.
            coplayerObservableCollection.HandlePropertyRelay(eventArgs);

            // Update properties.
            switch(eventArgs.PropertyId)
            {
                case PropertyRelay.CanBetFlag:
                {
                    var canBet = ((PropertyRelayDataEventArgs<bool>)eventArgs).Data;
                    shellExposition.CanBet = canBet;
                    break;
                }
                case PropertyRelay.CanCommitGameCycleFlag:
                {
                    var canCommitGameCycle = ((PropertyRelayDataEventArgs<bool>)eventArgs).Data;
                    shellExposition.CanCommitGameCycle = canCommitGameCycle;
                    break;
                }
                case PropertyRelay.PlayerBettableMeter:
                {
                    var playerBettableMeter = ((PropertyRelayDataEventArgs<long>)eventArgs).Data;
                    shellExposition.PlayerBettableMeter = playerBettableMeter;
                    break;
                }
            }
        }

        /// <summary>
        /// Handles a game cycle state transitioned event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleGameCycleStateTransitioned(object sender, GameCycleStateTransitionedEventArgs eventArgs)
        {
            switch(eventArgs.ToState)
            {
                case GameCycleState.Playing:
                {
                    if(eventArgs.FromState == GameCycleState.EnrollComplete)
                    {
                        // History cycle starts after StartPlaying call.
                        historyManager.EnableHistoryWrite();
                        historyManager.ClearTrackedData();
                    }
                    break;
                }
                case GameCycleState.Idle:
                {
                    // History cycle ends after EndGameCycle call.
                    historyManager.DisableHistoryWrite();
                    break;
                }
            }
        }

        #endregion
    }
}