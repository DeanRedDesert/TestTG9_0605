// -----------------------------------------------------------------------
// <copyright file = "ShellStateMachineFramework.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Platform;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Communication.CommunicationLib;
    using Game.Core.Logic.Services;
    using IGT.Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// The state machine framework for a shell state machine.
    /// </summary>
    internal class ShellStateMachineFramework
        : StateMachineFrameworkBase<IShellState, IShellFrameworkInitialization, IShellFrameworkExecution, IShellLib>,
          IShellFrameworkInitialization,
          IShellFrameworkExecution,
          IShellHistoryQuery,
          IShellHistoryPresentation
    {
        #region Private Fields

        /// <summary>
        /// The runner of the framework.
        /// </summary>
        private readonly IShellFrameworkRunner shellFrameworkRunner;

        /// <summary>
        /// The reference of shell lib, used for communication with Foundation.
        /// </summary>
        private readonly IShellLibRestricted shellLibRestricted;

        /// <summary>
        /// The custom object that determines to which coplayers a parcel call is to be routed.
        /// </summary>
        private readonly IParcelCallRouter parcelCallRouter;

        /// <summary>
        /// The object exposing observables.
        /// </summary>
        private readonly ShellObservableCollection shellObservableCollection;

        /// <summary>
        /// The critical data block for safe storing state info.
        /// </summary>
        private readonly SingleCriticalData<StateInfo> stateInfoBlock;

        /// <summary>
        /// The object responsible for tracking history data.
        /// </summary>
        private readonly ConcurrentHistoryRecorder historyRecorder;

        /// <summary>
        /// The object responsible for writing the last step of shell history data.
        /// </summary>
        private readonly ShellHistoryWriter historyWriter;

        /// <summary>
        /// The name of the last history state.
        /// </summary>
        private string lastHistoryStateName;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public ShellStateMachineFramework(IShellFrameworkRunner frameworkRunner,
                                          IShellLib shellLib,
                                          IShellLibRestricted shellLibRestricted,
                                          IShellStateMachine shellStateMachine,
                                          IParcelCallRouter parcelCallRouter,
                                          IPresentation presentationClient,
                                          IDictionary<string, ServiceRequestData> serviceRequestDataMap)
            : base(frameworkRunner,
                   shellLib,
                   shellLib?.Context.GameMode ?? GameMode.Invalid,
                   shellStateMachine,
                   presentationClient,
                   serviceRequestDataMap,
                   "#S")
        {
            shellFrameworkRunner = frameworkRunner;
            this.shellLibRestricted = shellLibRestricted ?? throw new ArgumentNullException(nameof(shellLibRestricted));
            this.parcelCallRouter = parcelCallRouter ?? throw new ArgumentNullException(nameof(parcelCallRouter));

            // Initialize observables
            shellObservableCollection = new ShellObservableCollection(shellLib, (IShellLibRestricted)shellLib);

            // Critical data paths
            var prefix = BuildPathPrefix(nameof(ShellStateMachineFramework), GameMode);
            stateInfoBlock = new SingleCriticalData<StateInfo>(prefix + PathStateInfo);

            // History handling
            historyRecorder = new ConcurrentHistoryRecorder();
            DisposableCollection.Add(historyRecorder);

            historyWriter = new ShellHistoryWriter(shellLibRestricted.ShellHistoryStore);

            // Event handling
            SubscribeEventHandlers();
        }

        #endregion

        #region IShellFramework*** Implementation

        /// <inheritdoc>
        ///     <cref>IShellFrameworkInitialization</cref>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public IShellLib ShellLib => LibInterface;

        /// <inheritdoc>
        ///     <cref>IShellFrameworkInitialization</cref>
        /// </inheritdoc>
        public IShellObservableCollection ObservableCollection => shellObservableCollection;

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public IShellHistoryPresentation HistoryPresentation => this;

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public IReadOnlyList<ShellThemeInfo> GetSelectableThemes()
        {
            return shellFrameworkRunner.GetSelectableThemes();
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public IReadOnlyList<CothemePresentationKey> GetRunningCothemes()
        {
            return shellFrameworkRunner.GetRunningCothemes();
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public bool StartNewTheme(string g2SThemeId, long denomination)
        {
            if(string.IsNullOrEmpty(g2SThemeId))
            {
                throw new ArgumentException("G2S Theme Id is null or empty.", nameof(g2SThemeId));
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0.");
            }

            CannotBeCalledFromStep(StateStep.CommittedWait);

            return shellFrameworkRunner.StartNewTheme(g2SThemeId, denomination, out _);
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public bool StartNewTheme(string g2SThemeId, long denomination, out int coplayerId)
        {
            if(string.IsNullOrEmpty(g2SThemeId))
            {
                throw new ArgumentException("G2S Theme Id is null or empty.", nameof(g2SThemeId));
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0.");
            }

            CannotBeCalledFromStep(StateStep.CommittedWait);

            return shellFrameworkRunner.StartNewTheme(g2SThemeId, denomination, out coplayerId);
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public bool SwitchCoplayerTheme(int coplayerId, string g2SThemeId, long denomination)
        {
            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(coplayerId), "Coplayer ID must be greater than or equal to 0.");
            }

            if(string.IsNullOrEmpty(g2SThemeId))
            {
                throw new ArgumentException("G2S Theme Id is null or empty.", nameof(g2SThemeId));
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0.");
            }

            CannotBeCalledFromStep(StateStep.CommittedWait);

            return shellFrameworkRunner.SwitchCoplayerTheme(coplayerId, g2SThemeId, denomination);
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public bool ShutDownCoplayer(int coplayerId)
        {
            CannotBeCalledFromStep(StateStep.CommittedWait);

            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(coplayerId), "Coplayer ID must be greater than or equal to 0.");
            }

            return shellFrameworkRunner.ShutDownCoplayer(coplayerId);
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public bool SendEventToCoplayer(CustomShellEventArgs eventArgs, int coplayerId)
        {
            if(coplayerId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(coplayerId), "Coplayer ID must be greater than or equal to 0.");
            }

            shellFrameworkRunner.EnqueueEventToCoplayers(eventArgs, new List<int> { coplayerId });

            return true;
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public bool SendEventToCoplayers(CustomShellEventArgs eventArgs, IReadOnlyList<int> targetCoplayers)
        {
            if(targetCoplayers == null || targetCoplayers.Count == 0 || targetCoplayers.Any(id => id < 0))
            {
                throw new ArgumentException("Target coplayer list is null or empty, or contains invalid data.", nameof(targetCoplayers));
            }

            shellFrameworkRunner.EnqueueEventToCoplayers(eventArgs, targetCoplayers);

            return true;
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public bool SendEventToAllCoplayers(CustomShellEventArgs eventArgs)
        {
            shellFrameworkRunner.EnqueueEventToCoplayers(eventArgs);

            return true;
        }

        /// <inheritdoc>
        ///     <cref>IShellFrameworkInitialization</cref>
        ///     <cref>IShellFrameworkExecution</cref>
        /// </inheritdoc>
        public event EventHandler<CustomCoplayerEventArgs> CoplayerEventReceived;

        #endregion

        #region IShellHistoryQuery Implementation

        /// <inheritdoc/>
        public HistoryRecord GetHistoryRecord(int stepNumber, DataItems baseData = null)
        {
            if(stepNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stepNumber), "Step number must be a positive integer.");
            }

            return historyRecorder.CreateHistoryRecord(stepNumber, baseData);
        }

        #endregion

        #region IShellHistoryPresentation Implementation

        /// <inheritdoc/>
        void IShellHistoryPresentation.StartHistoryState(string stateName, DataItems data)
        {
            CannotBeCalledFromStep(StateStep.Processing);

            var historyLiveRequest = ServiceRequestDataMaps.GetHistoryRequestData(stateName);
            var historyLiveData = ServiceControllerInstance.FillRequest(historyLiveRequest);

            data.Merge(historyLiveData);

            PresentationClient.StartState(stateName, data);

            lastHistoryStateName = stateName;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        protected override IShellFrameworkInitialization FrameworkInitialization => this;

        /// <inheritdoc/>
        protected override IShellFrameworkExecution FrameworkExecution => this;

        ///<inheritdoc/>
        protected override StateInfo ReadStateInfo()
        {
            StateInfo result = null;

            if(LibInterface.Context.GameMode == GameMode.Play)
            {
                var dataBlock = LibInterface.ShellStore.Read(stateInfoBlock.Name);

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

                LibInterface.ShellStore.Write(stateInfoBlock);
            }
        }

        /// <inheritdoc/>
        protected override void RecordHistory(DataItems newData, string stateName, bool startingPresentationState = false)
        {
            historyRecorder.Update(newData, startingPresentationState ? stateName : null);
        }

        /// <inheritdoc/>
        protected override DataItems GetHistoryAsyncData(out string historyStateName, string providerName, IList<ServiceSignature> serviceSignatures)
        {
            historyStateName = lastHistoryStateName ?? string.Empty;

            var historyLiveRequest = ServiceRequestDataMaps.GetHistoryAsyncRequestData(historyStateName);

            return GetAsyncServiceData(historyLiveRequest, providerName, serviceSignatures);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            UnsubscribeEventHandlers();

            shellObservableCollection.Dismiss();

            // Call base method last as it modifies the IsDisposed flag.
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Subscribe to any shell lib, or shell framework runner events
        /// that persists through the state machine framework's life spans.
        /// </summary>
        private void SubscribeEventHandlers()
        {
            LibInterface.GameParcelComm.NonTransactionalParcelCallReceivedEvent += HandleNonTransParcelCallReceived;
            LibInterface.GameParcelComm.TransactionalParcelCallReceivedEvent += HandleTransParcelCallReceived;

            LibInterface.BankPlay.BankPlayPropertiesUpdateEvent += HandleBankPlayPropertiesUpdate;
            LibInterface.BankPlay.MoneyChangedEvent += HandleMoneyChanged;

            LibInterface.GameCulture.CultureChangedEvent += HandleCultureChanged;

            LibInterface.TiltController.ShellTiltClearedByAttendantEvent += HandleShellTiltClearedByAttendant;

            shellLibRestricted.ShellHistoryStore.LogEndGameCycleEvent += HandleLogEndGameCycle;

            shellFrameworkRunner.PresentationEventReceived += HandlePresentationEventReceived;
            shellFrameworkRunner.CoplayerEventReceived += HandleCoplayerEventReceived;
            shellFrameworkRunner.EventQueuePostProcessing += HandleEventQueuePostProcessing;
        }

        /// <summary>
        /// Unsubscribe from any shell lib, or shell framework runner events
        /// that persists through the state machine framework's life spans.
        /// </summary>
        private void UnsubscribeEventHandlers()
        {
            LibInterface.GameParcelComm.NonTransactionalParcelCallReceivedEvent -= HandleNonTransParcelCallReceived;
            LibInterface.GameParcelComm.TransactionalParcelCallReceivedEvent -= HandleTransParcelCallReceived;

            LibInterface.BankPlay.BankPlayPropertiesUpdateEvent -= HandleBankPlayPropertiesUpdate;
            LibInterface.BankPlay.MoneyChangedEvent -= HandleMoneyChanged;

            LibInterface.GameCulture.CultureChangedEvent -= HandleCultureChanged;

            LibInterface.TiltController.ShellTiltClearedByAttendantEvent -= HandleShellTiltClearedByAttendant;

            shellLibRestricted.ShellHistoryStore.LogEndGameCycleEvent -= HandleLogEndGameCycle;

            shellFrameworkRunner.PresentationEventReceived -= HandlePresentationEventReceived;
            shellFrameworkRunner.CoplayerEventReceived -= HandleCoplayerEventReceived;
            shellFrameworkRunner.EventQueuePostProcessing -= HandleEventQueuePostProcessing;
        }

        /// <summary>
        /// Handles a non transactional parcel call received from Foundation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleNonTransParcelCallReceived(object sender, NonTransactionalParcelCallReceivedEventArgs eventArgs)
        {
            var targetCoplayers = parcelCallRouter?.GetTargetCoplayers(eventArgs, StateMachine);

            if(targetCoplayers == null)
            {
                // null means do not route to any coplayer.
                return;
            }

            shellFrameworkRunner.EnqueueEventToCoplayers(eventArgs, targetCoplayers);
        }

        /// <summary>
        /// Handles a transactional parcel call received from Foundation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleTransParcelCallReceived(object sender, TransactionalParcelCallReceivedEventArgs eventArgs)
        {
            var targetCoplayers = parcelCallRouter?.GetTargetCoplayers(eventArgs, StateMachine);

            if(targetCoplayers == null)
            {
                // null means do not route to any coplayer.
                return;
            }

            // When routing to coplayer, the event becomes non-transactional.
            var transactionlessEvent = eventArgs.Downgrade(TransactionWeight.None);

            shellFrameworkRunner.PostEventToCoplayers(transactionlessEvent, eventArgs.IsHeavyweight, targetCoplayers);
        }

        /// <summary>
        /// Handles a tilt cleared event from Foundation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleShellTiltClearedByAttendant(object sender, ShellTiltClearedByAttendantEventArgs eventArgs)
        {
            var transactionlessEvent = eventArgs.Downgrade(TransactionWeight.None);
            shellFrameworkRunner.PostEventToCoplayers(transactionlessEvent, eventArgs.IsHeavyweight);
        }

        /// <summary>
        /// Handles a bank play properties update event received from Foundation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleBankPlayPropertiesUpdate(object sender, BankPlayPropertiesUpdateEventArgs eventArgs)
        {
            if(eventArgs.CanBet != null)
            {
                var relayEvent = new PropertyRelayDataEventArgs<bool>(PropertyRelay.CanBetFlag,
                                                                      eventArgs.CanBet.Value);

                shellFrameworkRunner.EnqueueEventToCoplayers(relayEvent);
            }

            if(eventArgs.CanCommitGameCycle != null)
            {
                var relayEvent = new PropertyRelayDataEventArgs<bool>(PropertyRelay.CanCommitGameCycleFlag,
                                                                      eventArgs.CanCommitGameCycle.Value);

                shellFrameworkRunner.EnqueueEventToCoplayers(relayEvent);
            }
        }

        /// <summary>
        /// Handles a money changed event received from Foundation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleMoneyChanged(object sender, MoneyChangedEventArgs eventArgs)
        {
            var relayEvent = new PropertyRelayDataEventArgs<long>(PropertyRelay.PlayerBettableMeter,
                                                                  eventArgs.GamingMeters.Bettable);

            shellFrameworkRunner.EnqueueEventToCoplayers(relayEvent);
        }

        /// <summary>
        /// Handles a culture changed event received from Foundation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleCultureChanged(object sender, CultureChangedEventArgs eventArgs)
        {
            var relayEvent = new PropertyRelayDataEventArgs<string>(PropertyRelay.CultureString,
                                                                    eventArgs.Culture);

            shellFrameworkRunner.EnqueueEventToCoplayers(relayEvent);
        }

        /// <summary>
        /// Handles a log finalized game cycle event received from Foundation.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleLogEndGameCycle(object sender, LogEndGameCycleEventArgs eventArgs)
        {
            var lastStepHistoryRecord = GetHistoryRecord(eventArgs.NumberOfSteps);

            historyWriter.WriteLastStep(eventArgs.CoplayerId, lastStepHistoryRecord);
        }

        /// <summary>
        /// Handles an event received from a coplayer.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleCoplayerEventReceived(object sender, EventDispatchedEventArgs eventArgs)
        {
            // So far this is the only coplayer event gets forwarded here from the shell runner.
            // Convert to event table as needed in the future, following the example of
            // GameStateMachineFramework.HandleShellEventReceived
            if(eventArgs.DispatchedEvent is CustomCoplayerEventArgs customCoplayerEventArgs)
            {
                CoplayerEventReceived?.Invoke(this, customCoplayerEventArgs);
            }
        }

        #endregion
    }
}