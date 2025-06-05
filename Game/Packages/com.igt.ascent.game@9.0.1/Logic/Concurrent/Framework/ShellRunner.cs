// -----------------------------------------------------------------------
// <copyright file = "ShellRunner.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Communication.Platform;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Communication.CommunicationLib;
    using Game.Core.Communication.Logic.CommServices;
    using Game.Core.Communication.LogicPresentationBridge;
    using Game.Core.Threading;
    using IGT.Ascent.Restricted.EventManagement;
    using IGT.Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;
    using DisplayControlEventArgs = Communication.Platform.Interfaces.DisplayControlEventArgs;

    /// <summary>
    /// This runner is in charge of running the shell logic thread, and responsible for
    /// creating and destroying the shell state machine framework at appropriate times.
    /// </summary>
    internal sealed class ShellRunner : RunnerBase,
                                        IShellRunner,
                                        IThreadWorker,
                                        IShellFrameworkRunner,
                                        IShellHistoryQuery
    {
        #region Constants

        /// <summary>
        /// The id used to indicate an invalid coplayer.
        /// Valid coplayer id starts at 0.
        /// </summary>
        private const int InvalidCoplayerId = -1;

        /// <summary>
        /// The coplayer id assigned to any coplayer running in the History mode.
        /// There should be only one coplayer running in History.
        /// </summary>
        private const int HistoryCoplayerId = int.MaxValue;

        #endregion

        #region Private Fields

        /// <summary>
        /// The shell configurator provided by the game.
        /// </summary>
        private readonly IShellConfigurator shellConfigurator;

        /// <summary>
        /// The factory object used to create shell lib and coplayer lib etc.
        /// It could be different based on the Run Mode (standard vs. standalone).
        /// </summary>
        private readonly IPlatformFactory platformFactory;

        /// <summary>
        /// The object that monitors the health of a collection of worker threads.
        /// </summary>
        private readonly IThreadHealthChecker threadHealthChecker;

        /// <summary>
        /// Object for the shell to communicate with Foundation.
        /// </summary>
        private readonly IShellLibRestricted shellLibRestricted;

        private readonly IShellLib shellLib;

        /// <summary>
        /// The object managing the GL2P comm channels for shell and all coplayers.
        /// </summary>
        private readonly Gl2PCommManager gl2PCommManager;

        /// <summary>
        /// The object managing transactional operations submitted from multiple thread.
        /// </summary>
        private readonly TransactionalOperationManager transactionalOperationManager;

        /// <summary>
        /// The object in charge of providing coplayers with config data.
        /// One of the supporting roles in ShellCallBacks.
        /// </summary>
        private readonly ShellConfigQuery shellConfigQuery;

        /// <summary>
        /// The object in charge of sending parcel calls on behalf of coplayers.
        /// One of the supporting roles in ShellCallBacks.
        /// </summary>
        private readonly ShellParcelCallSender shellParcelCallSender;

        /// <summary>
        /// The object in charge of sending tilts on behalf of coplayers.
        /// One of the supporting roles in ShellCallBacks.
        /// </summary>
        private readonly ShellTiltSender shellTiltSender;

        /// <summary>
        /// The queue of events sent from coplayers. It can handle both blocking and non-blocking events.
        /// Since shell and coplayers cannot share transactions (because they are on different sessions),
        /// all the events in this queue will be non-transactional.
        /// </summary>
        private readonly DuplexEventQueue coplayerEventQueue;

        /// <summary>
        /// Event table for events received from Coplayers.
        /// </summary>
        private readonly Dictionary<Type, EventHandler> coplayerEventHandlers = new Dictionary<Type, EventHandler>();

        /// <summary>
        /// The wait handle used to indicate that the shell logic thread should be terminated.
        /// </summary>
        private WaitHandle exitHandle;

        /// <summary>
        /// The flag indicating whether the shell application is being parked.
        /// </summary>
        private bool isParked;

        /// <summary>
        /// The list of cothemes that are available to run in the shell.
        /// </summary>
        private IList<ShellThemeInfo> selectableThemes = new List<ShellThemeInfo>();

        /// <summary>
        /// The binders of running coplayers, each holding all the components related to a coplayer.
        /// </summary>
        private readonly List<CoplayerBinder> coplayerBinders = new List<CoplayerBinder>();

        /// <summary>
        /// The list of vacant coplayer IDs.  A vacant coplayer is a coplayer that
        /// has a valid coplayer ID (recognized by Foundation) but no cotheme running in it.
        /// </summary>
        private readonly List<int> vacantCoplayerIds = new List<int>();

        /// <summary>
        /// The running shell state machine.
        /// </summary>
        private ShellStateMachineFramework stateMachineFramework;

        // The wait handle used to indicate that the running shell state machine framework should be terminated.
        // It is signaled and waited on in the same thread in order to simplify the coordination work with
        // the state machine framework object.
        private readonly ManualResetEvent exitFrameworkEvent;

        /// <summary>
        /// The logic comm services used by the shell.
        /// </summary>
        private readonly ILogicCommServices logicCommServices;

        /// <summary>
        /// The service request data configured for the shell.
        /// </summary>
        private IDictionary<string, ServiceRequestData> serviceRequestDataMap;

        /// <summary>
        /// Previous game mode (optional).
        /// </summary>
        private GameMode? previousGameMode;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellRunner"/>.
        /// </summary>
        /// <param name="shellConfigurator">
        /// The shell configurator provided by the game.
        /// </param>
        /// <param name="platformFactory">
        /// The platform factory to use.
        /// </param>
        /// <param name="threadHealthChecker">
        /// The object monitoring the health of a collection of worker threads.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either of the arguments is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="shellConfigurator"/> returns no <see cref="IGameConfigurator"/> objects.
        /// </exception>
        public ShellRunner(IShellConfigurator shellConfigurator, IPlatformFactory platformFactory, IThreadHealthChecker threadHealthChecker)
        {
            this.shellConfigurator = shellConfigurator ?? throw new ArgumentNullException(nameof(shellConfigurator));

            var gameConfigurators = shellConfigurator.GetGameConfigurators();
            if(gameConfigurators?.Any() != true)
            {
                throw new ArgumentException("Shell configurator does not define any game configurator.",
                                            nameof(shellConfigurator));
            }

            this.platformFactory = platformFactory ?? throw new ArgumentNullException(nameof(platformFactory));
            this.threadHealthChecker = threadHealthChecker ?? throw new ArgumentNullException(nameof(threadHealthChecker));

            // Create Shell Lib.
            shellLibRestricted = this.platformFactory.CreateShellLib(shellConfigurator.GetInterfaceExtensionRequests(),
                                                                     shellConfigurator.GetStandaloneParcelCommMock());
            DisposableCollection.Add(shellLibRestricted);

            shellLib = shellLibRestricted as IShellLib;
            if(shellLib == null)
            {
                throw new FrameworkRunnerException("Shell lib instance does not implement IShellLib.");
            }

            // Create GL2P Comm Manager.
            gl2PCommManager = new Gl2PCommManager();
            DisposableCollection.Add(gl2PCommManager);

            logicCommServices = gl2PCommManager.CreateCommServices(CothemePresentationKey.ShellKey);

            // Create wait handle for exiting framework.
            exitFrameworkEvent = new ManualResetEvent(false);
            DisposableCollection.Add(exitFrameworkEvent);

            // Create transactional operation manager.
            transactionalOperationManager = new TransactionalOperationManager();
            DisposableCollection.Add(transactionalOperationManager);

            // Create other supporting roles in ShellCallBacks
            shellConfigQuery = new ShellConfigQuery(shellLib,
                                                    shellLibRestricted,
                                                    gameConfigurators,
                                                    gl2PCommManager);

            shellParcelCallSender = new ShellParcelCallSender(shellLib.GameParcelComm, transactionalOperationManager);

            shellTiltSender = new ShellTiltSender(shellLib.TiltController, transactionalOperationManager);

            // Wait handles that would interrupt event queue manager processing events.
            ReturnHandles = new List<WaitHandle>
                                {
                                    exitFrameworkEvent,
                                    transactionalOperationManager.TransactionalOperationRequested
                                };

            // Create Event Queue Manager.
            EventQueueManager = new EventQueueManager(shellLibRestricted.ExceptionMonitor);

            // Create the coplayer event queues.
            coplayerEventQueue = new DuplexEventQueue();
            DisposableCollection.Add(coplayerEventQueue);

            // Register event queues.  The order of registration matters.
            EventQueueManager.RegisterEventQueue(shellLibRestricted.TransactionalEventQueue);
            EventQueueManager.RegisterEventQueue(shellLibRestricted.NonTransactionalEventQueue);
            EventQueueManager.RegisterEventQueue(coplayerEventQueue);
            EventQueueManager.RegisterEventQueue(logicCommServices.PresentationEventQueue);

            // Keep a cached list for better performance.
            TransEventQueueList = new List<IEventQueue>
                                      {
                                          shellLibRestricted.TransactionalEventQueue,
                                      };

            // Monitor if an event has been processed in selected event queues.
            // This is mainly used for sending async service data over to the presentation side,
            // therefore no need to monitor the presentation queue.
            var eventQueuesToMonitor = new List<IEventQueue>
                                           {
                                               shellLibRestricted.TransactionalEventQueue,
                                               shellLibRestricted.NonTransactionalEventQueue,
                                               coplayerEventQueue
                                           };

            foreach(var eventQueue in eventQueuesToMonitor)
            {
                if(eventQueue is IEventDispatchMonitor dispatchMonitor)
                {
                    dispatchMonitor.EventDispatchEnded += HandlePostProcessingEvent;
                }
            }

            // Hook up event handling.
            shellLibRestricted.NewShellContextEvent += HandleNewShellContext;
            shellLibRestricted.ActivateShellContextEvent += HandleActivateShellContext;
            shellLibRestricted.InactivateShellContextEvent += HandleInactivateShellContext;
            shellLibRestricted.DisplayControlEvent += HandleDisplayControl;
            shellLibRestricted.ShutDownEvent += HandleShutDown;
            shellLibRestricted.ParkEvent += HandlePark;

            CreateCoplayerEventHandlerTable();
            coplayerEventQueue.EventDispatchedEvent += HandleCoplayerEvent;

            logicCommServices.PresentationEventDispatcher.EventDispatchedEvent += HandlePresentationEvent;
        }

        #endregion

        #region IShellRunner Implementation

        #region Properties

        /// <inheritdoc/>
        public string Token => shellLibRestricted.Token;

        /// <inheritdoc/>
        public string MountPoint => shellLibRestricted.MountPoint;

        /// <inheritdoc/>
        public IGl2PCommManager Gl2PCommManager => gl2PCommManager;

        #endregion

        #region Events

        /// <inheritdoc/>
        public event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Unpark;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Park;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public bool ConnectToFoundation()
        {
            return shellLibRestricted.ConnectToFoundation();
        }

        /// <inheritdoc/>
        public void Initialize(IServiceRequestDataManager requestDataManager)
        {
            if(requestDataManager == null)
            {
                throw new ArgumentNullException(nameof(requestDataManager));
            }

            shellConfigQuery.Initialize(requestDataManager);

            serviceRequestDataMap = requestDataManager.LoadServiceRequestData(CothemePresentationKey.ShellG2S);

            if(serviceRequestDataMap == null)
            {
                throw new FrameworkRunnerException("No Service Request Data was found for G2S Theme ID: " +
                                                   CothemePresentationKey.ShellG2S);
            }
        }

        #endregion

        #endregion

        #region IThreadWorker Implementation

        /// <inheritdoc/>
        public string ThreadName => "Shell Runner";

        /// <inheritdoc/>
        public void DoWork(WaitHandle waitHandle)
        {
            Game.Core.Logging.Log.Write("Shell thread started.");

            exitHandle = waitHandle ?? throw new ArgumentNullException(nameof(waitHandle));

            // Insert the exit handle BEFORE the exit framework handle.
            ReturnHandles.Insert(0, exitHandle);

            while(!exitHandle.WaitOne(0))
            {
                if(stateMachineFramework != null)
                {
                    // If the framework is told to stop while it is running.
                    if(exitFrameworkEvent.WaitOne(0))
                    {
                        DisposeStateMachineFramework();

                        exitFrameworkEvent.Reset();
                    }
                    else if(transactionalOperationManager.TransactionalOperationRequested.WaitOne(0))
                    {
                        transactionalOperationManager.ExecuteQueuedOperations(this);
                    }
                    else
                    {
                        stateMachineFramework.ExecuteState();
                    }
                }
                else
                {
                    EventQueueManager.ProcessEvents(ReturnHandles,
                                                    eventArgs => eventArgs is ActivateShellContextEventArgs,
                                                    TransEventQueueList);
                }
            }

            Stop();

            Game.Core.Logging.Log.Write("Shell thread ended.");
        }

        #endregion

        #region IShellHistoryQuery Implementation

        // This interface is to be used by coplayer runners.
        // Therefore, its implementation must be thread safe.

        /// <inheritdoc />
        public HistoryRecord GetHistoryRecord(int stepNumber, DataItems baseData = null)
        {
            // Should not happen.
            if(stateMachineFramework == null)
            {
                throw new FrameworkRunnerException("Shell state machine framework not running when is queried for history record.");
            }

            return stateMachineFramework.GetHistoryRecord(stepNumber, baseData);
        }

        #endregion

        #region IShellFrameworkRunner Implementation

        #region IFrameworkRunner Part

        // Part of the interface implementation is in the base class.

        /// <inheritdoc/>
        public bool DoTransaction(string transactionName, Action action)
        {
            void Handler(object sender, ActionResponseEventArgs eventArgs) => action();

            shellLibRestricted.ActionResponseEvent += Handler;

            shellLibRestricted.ActionRequest(ThreadName + " " + transactionName);

            var transactionOpened = ProcessEvents(eventArgs => eventArgs is ActionResponseEventArgs,
                                                  TransEventQueueList);

            shellLibRestricted.ActionResponseEvent -= Handler;

            return transactionOpened;
        }

        /// <inheritdoc/>
        public bool DoTransactionLite(string transactionName, Action action)
        {
            void Handler(object sender, ActionResponseLiteEventArgs eventArgs) => action();

            shellLibRestricted.ActionResponseLiteEvent += Handler;

            shellLibRestricted.ActionRequestLite(ThreadName + " " + transactionName);

            var transactionOpened = ProcessEvents(eventArgs => eventArgs is ActionResponseLiteEventArgs,
                                                  TransEventQueueList);

            shellLibRestricted.ActionResponseLiteEvent -= Handler;

            return transactionOpened;
        }

        /// <inheritdoc/>
        public bool OnNextTransaction(string transactionName, Action action)
        {
            shellLibRestricted.ActionRequest(ThreadName + " " + transactionName);

            var transactionOpened = ProcessEvents(eventArgs => LeverageHeavyweightEvent(eventArgs, action),
                                                  TransEventQueueList);

            return transactionOpened;
        }

        #endregion

        #region IShellFrameworkRunner Part

        #region Managing Coplayers and Cothemes

        /// <inheritdoc/>
        public IReadOnlyList<ShellThemeInfo> GetSelectableThemes()
        {
            // Return a copy of the list to protect the internal data.
            return selectableThemes.ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyList<CothemePresentationKey> GetRunningCothemes()
        {
            lock(coplayerBinders)
            {
                return GetPlayModeCoplayers().Select(binder => new CothemePresentationKey(binder.CoplayerLibRestricted.CoplayerId,
                                                                                          binder.ThemeInfo.G2SThemeId))
                                             .ToList();
            }
        }

        /// <inheritdoc/>
        public bool StartNewTheme(string g2SThemeId, long denomination, out int coplayerId)
        {
            var result = false;

            var themeInfo = GetThemeInfoByG2SThemeId(g2SThemeId);

            if(themeInfo == null)
            {
                throw new ArgumentException("No selectable theme is found whose G2SThemeId is " + g2SThemeId, nameof(g2SThemeId));
            }

            coplayerId = GetNextAvailableCoplayerId();

            if(coplayerId != InvalidCoplayerId)
            {
                // Set the theme and denomination in the coplayer.
                shellLibRestricted.SwitchCoplayerTheme(coplayerId, themeInfo.ThemeIdentifier, denomination);

                StartNewCoplayer(coplayerId, themeInfo);

                result = true;
            }

            return result;
        }

        /// <inheritdoc/>
        public bool SwitchCoplayerTheme(int coplayerId, string g2SThemeId, long denomination)
        {
            var result = false;

            var themeInfo = GetThemeInfoByG2SThemeId(g2SThemeId);

            if(themeInfo == null)
            {
                throw new ArgumentException("No selectable theme is found whose G2SThemeId is " + g2SThemeId, nameof(g2SThemeId));
            }

            lock(coplayerBinders)
            {
                var binder = GetCoplayerBinder(coplayerId);
                if(binder == null)
                {
                    throw new ArgumentException("No coplayer is found with coplayer id " + coplayerId, nameof(coplayerId));
                }

                // Only proceed with switching if the theme/denom pair is different from the existing one.
                if(g2SThemeId != binder.ThemeInfo.G2SThemeId ||
                   denomination != ((ICoplayerLib)binder.CoplayerLibRestricted).Context.Denomination)
                {
                    // Update the theme info in the binder.
                    binder.UpdateThemeInfo(themeInfo);

                    result = true;
                }
            }

            if(result)
            {
                // This does not return until the coplayer context switch is complete.
                shellLibRestricted.SwitchCoplayerTheme(coplayerId, themeInfo.ThemeIdentifier, denomination);
            }

            return result;
        }

        /// <inheritdoc/>
        public bool ShutDownCoplayer(int coplayerId)
        {
            var result = false;

            var sessionId = -1;

            lock(coplayerBinders)
            {
                var binder = GetCoplayerBinder(coplayerId);

                if(binder != null)
                {
                    sessionId = binder.SessionId;
                    result = true;
                }
            }

            if(result)
            {
                Game.Core.Logging.Log.Write($"Requesting Foundation to shut down Coplayer {coplayerId} on Session {sessionId}.");

                // DestroySession is only needed when the shut down is initiated by the player/shell.
                // This call does not return until a shut down event has been sent to the coplayer.
                shellLibRestricted.DestroySession(sessionId);

                // This must be done AFTER the session bound with the coplayer has been destroyed,
                // otherwise, RemoveCoplayerTheme will lead to Foundation sending out ActivateCoplayerContext
                // event with empty theme identifier, which will cause a big mess in shell and coplayer logic.
                // A session is destroyed when shell calls DestroySession or Foundation initiates a shut down.
                shellLibRestricted.RemoveCoplayerTheme(coplayerId);
            }

            return result;
        }

        #endregion

        #region Events to/from Coplayers

        /// <inheritdoc/>
        public event EventHandler<EventDispatchedEventArgs> CoplayerEventReceived;

        /// <inheritdoc/>
        public void PostEventToCoplayers(PlatformEventArgs platformEventArgs,
                                         bool isHeavyweightTransaction,
                                         IReadOnlyList<int> targetCoplayers = null)
        {
            // When routing to coplayer, the event must be non-transactional.
            if(platformEventArgs.IsTransactional)
            {
                throw new FrameworkRunnerException(
                    $"Cannot post transactional {platformEventArgs.GetType().Name} to coplayers.  The event must be downgraded first.");
            }

            if(isHeavyweightTransaction)
            {
                // If this is within a heavyweight transaction, we can use it to fire all pending transactional operations.
                // This is also necessary to free up any coplayers who is blocking on a pending transactional operation.
                transactionalOperationManager.FireQueuedOperations();

                lock(coplayerBinders)
                {
                    var targetBinders = GetTargetCoplayerBinders(targetCoplayers);

                    foreach(var binder in targetBinders)
                    {
                        // If the event is posted to a coplayer exactly at the same time that the coplayer submits
                        // a transactional operation, we could end up in a deadlock.  Therefore, the submission of
                        // a transactional operation should be treated as an interrupt and handled right away.
                        binder.CoplayerRunner.EventPoster.PostEvent(platformEventArgs,
                                                                    transactionalOperationManager.TransactionalOperationRequested,
                                                                    transactionalOperationManager.FireQueuedOperations);
                    }
                }
            }
            else
            {
                // If this not within a heavyweight transaction, enqueue the event instead of posting it,
                // because if the coplayer submits a transactional operation at the same time, we won't be
                // able to fire the queued operations right away and hence end up in a deadlock.
                EnqueueEventToCoplayers(platformEventArgs, targetCoplayers);
            }
        }

        /// <inheritdoc/>
        public void EnqueueEventToCoplayers(PlatformEventArgs platformEventArgs,
                                            IReadOnlyList<int> targetCoplayers = null)
        {
            // When routing to coplayer, the event must be non-transactional.
            if(platformEventArgs.IsTransactional)
            {
                throw new FrameworkRunnerException(
                    $"Cannot enqueue transactional {platformEventArgs.GetType().Name} to coplayers.  The event must be downgraded first.");
            }

            lock(coplayerBinders)
            {
                var targetBinders = GetTargetCoplayerBinders(targetCoplayers);

                foreach(var binder in targetBinders)
                {
                    binder.CoplayerRunner.EventQueuer.EnqueueEvent(platformEventArgs);
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        protected override void ClearEventHandlers()
        {
            base.ClearEventHandlers();
            CoplayerEventReceived = null;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if(IsDisposed)
            {
                return;
            }

            if(disposing)
            {
                Stop();

                DisposeStateMachineFramework();
            }

            // Call base method last as it modifies the IsDisposed flag.
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        #region Initialization and Clean up

        /// <summary>
        /// Performs work needed before terminating the thread.
        /// </summary>
        private void Stop()
        {
            transactionalOperationManager.CancelQueuedOperations();

            KillAllCoplayers();

            DisposeStateMachineFramework();
        }

        /// <summary>
        /// Disposes the state machine framework.
        /// </summary>
        private void DisposeStateMachineFramework()
        {
            // Stop GL2P.
            logicCommServices.LogicHostControl.Stop();

            stateMachineFramework?.Dispose();
            stateMachineFramework = null;

            ClearEventHandlers();
        }

        #endregion

        #region Handling Shell Lib Events

        /// <summary>
        /// Handles the shell lib new shell context event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleNewShellContext(object sender, NewShellContextEventArgs eventArgs)
        {
            // If a state machine framework is running, stop it first.
            if(stateMachineFramework != null)
            {
                exitFrameworkEvent.Set();
            }
        }

        /// <summary>
        /// Handles the shell lib activate shell context event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleActivateShellContext(object sender, ActivateShellContextEventArgs eventArgs)
        {
            Game.Core.Logging.Log.Write($"Activating ShellContext: {shellLib.Context}");

            // Start GL2P.
            logicCommServices.LogicHostControl.Start();

            if(isParked)
            {
                // It is possible that the game may have been previously parked.
                // This event will notify the shell to un-park if applicable.
                Unpark?.Invoke(logicCommServices.PresentationTransition, EventArgs.Empty);

                isParked = false;
            }

            // Create the framework and activate the context according to the game mode.
            var gameMode = shellLib.Context.GameMode;
            switch(gameMode)
            {
                case GameMode.Play:
                {
                    stateMachineFramework =
                        new ShellStateMachineFramework(this,
                                                       shellLib,
                                                       (IShellLibRestricted)shellLib,
                                                       shellConfigurator.CreateShellStateMachine(shellLib.Context),
                                                       shellConfigurator.GetParcelCallRouter(),
                                                       logicCommServices.PresentationClient,
                                                       serviceRequestDataMap);
                    ActivateInPlayMode();
                    break;
                }
                case GameMode.History:
                {
                    stateMachineFramework =
                        new ShellStateMachineFramework(this,
                                                       shellLib,
                                                       (IShellLibRestricted)shellLib,
                                                       new ShellHistoryStateMachine(),
                                                       shellConfigurator.GetParcelCallRouter(),
                                                       logicCommServices.PresentationClient,
                                                       serviceRequestDataMap);
                    ActivateInHistoryMode();
                    if(previousGameMode == GameMode.Play)
                    {
                        logicCommServices.PresentationTransition.EnterHistoryFromPlayContext();
                    }
                    break;
                }
                default:
                {
                    var message = $"Activation in game mode {gameMode} is not supported.";
                    throw new NotSupportedException(message);
                }
            }
        }

        /// <summary>
        /// Handles the shell lib inactivate shell context event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleInactivateShellContext(object sender, InactivateShellContextEventArgs eventArgs)
        {
            Game.Core.Logging.Log.Write($"Inactivating ShellContext: {eventArgs.LastActiveContext}");

            exitFrameworkEvent.Set();

            previousGameMode = eventArgs.LastActiveContext?.GameMode;

            // Notify the presentation about the context change
            switch(eventArgs.LastActiveContext?.GameMode)
            {
                case GameMode.Play:
                {
                    logicCommServices.PresentationTransition.ExitPlayContext();
                    break;
                }
                case GameMode.History:
                {
                    logicCommServices.PresentationTransition.ExitHistoryContext();
                    break;
                }
                case GameMode.Utility:
                {
                    logicCommServices.PresentationTransition.ExitUtilityContext();
                    break;
                }
            }

            if(eventArgs.LastActiveContext?.GameMode == GameMode.History)
            {
                var historyBinder = GetCoplayerBinder(HistoryCoplayerId);

                if(historyBinder != null)
                {
                    // Currently Foundation does not support reusing the history session.
                    // So we destroy and recreate the history session for every history game cycle.
                    // This call does not return until the coplayer shut down is complete.
                    shellLibRestricted.DestroySession(historyBinder.SessionId);
                }
            }
        }

        /// <summary>
        /// Handles the shell lib display control event.
        /// Forwards it to all coplayers.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDisplayControl(object sender, DisplayControlEventArgs eventArgs)
        {
            // In case of a warm boot up, Foundation returns an empty list of selectable themes
            // during the shell context activation. The list is not available until the first
            // DisplayControlEvent.
            if(selectableThemes.Count == 0)
            {
                selectableThemes = shellLibRestricted.GetSelectableThemes();
            }

            var transactionlessEvent = eventArgs.Downgrade(TransactionWeight.None);

            PostEventToCoplayers(transactionlessEvent, eventArgs.IsHeavyweight);
        }

        /// <summary>
        /// Handles the shell lib shut down event.
        /// Forwards it to the parent thread.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleShutDown(object sender, ShutDownEventArgs eventArgs)
        {
            Game.Core.Logging.Log.Write("Requested by Foundation to shut down Shell.");

            ShutDownEvent?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Handle the shell lib park event.
        /// Forwards it to the parent thread.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event of the arguments.</param>
        private void HandlePark(object sender, EventArgs eventArgs)
        {
            Game.Core.Logging.Log.Write("Parking Shell.");

            Park?.Invoke(logicCommServices.PresentationTransition, EventArgs.Empty);

            isParked = true;
        }

        #endregion

        #region Handling Coplayer Events

        /// <summary>
        /// Creates the event lookup table for events received from Shell.
        /// </summary>
        private void CreateCoplayerEventHandlerTable()
        {
            // Events should be listed in alphabetical order!

            coplayerEventHandlers[typeof(CoplayerShutDownEventArgs)] =
                (s, e) => HandleCoplayerShutDown((CoplayerShutDownEventArgs)e);
        }

        /// <summary>
        /// Handles the event sent by the coplayers.
        /// Forwards it to shell state machine framework as needed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event of the arguments.</param>
        private void HandleCoplayerEvent(object sender, EventDispatchedEventArgs eventArgs)
        {
            var eventType = eventArgs.DispatchedEventType;

            // If the runner has a handler configured for the event, let the runner handle it.
            if(coplayerEventHandlers.TryGetValue(eventType, out var handler))
            {
                handler?.Invoke(sender, eventArgs.DispatchedEvent);
                eventArgs.IsHandled = true;
            }

            // Else forward it to the state machine framework.
            else
            {
                CoplayerEventReceived?.Invoke(sender, eventArgs);
            }
        }

        /// <summary>
        /// Handles the shut down event sent from a coplayer.
        /// </summary>
        /// <param name="eventArgs">The event data.</param>
        private void HandleCoplayerShutDown(CoplayerShutDownEventArgs eventArgs)
        {
            KillCoplayer(eventArgs.CoplayerId);
        }

        #endregion

        #region Handling Context Activation

        /// <summary>
        /// Activates shell context in play mode.
        /// </summary>
        private void ActivateInPlayMode()
        {
            var allCoplayerInfos = shellLibRestricted.GetCoplayers();
            var runningCoplayers = GetRunningCoplayerIds();
            var vacantCoplayers = GetVacantCoplayerIds();

            // Get the selectable themes from Foundation.
            // In case of a warm boot up, the returned value would be an empty list.
            // In event of an empty list, we wait to query again
            // until a display control state message arrives.
            selectableThemes = shellLibRestricted.GetSelectableThemes();

            if(allCoplayerInfos.Count == 0)
            {
                // "No existing coplayers on Foundation side" indicates a cold boot.
                SyncColdBoot(runningCoplayers, vacantCoplayers);
            }
            else
            {
                // Warm boot or re-activation (e.g. due to game mode change)
                var runningCoplayerInfos = allCoplayerInfos.Where(info => info.ThemeIdentifier != null).OrderBy(info => info.CoplayerId).ToList();
                var vacantCoplayerInfos = allCoplayerInfos.Where(info => info.ThemeIdentifier == null).OrderBy(info => info.CoplayerId).ToList();

                var stoppedCoplayers = SyncRunningCoplayers(runningCoplayerInfos, runningCoplayers);
                var vacatedCoplayers = SyncVacantCoplayers(vacantCoplayerInfos, vacantCoplayers);

                SyncDisabledCoplayers(stoppedCoplayers, vacatedCoplayers);
            }
        }

        /// <summary>
        /// Activates shell context in history mode.
        /// </summary>
        private void ActivateInHistoryMode()
        {
            // Currently Foundation does not support reusing the history session (we are required to call LaunchSession
            // for each history game cycle, which starts the session from the connect category level negotiation),
            // so we destroy and recreate the history thread/runner/coplayer lib/session for every history game cycle.

            // The history coplayer for previous history game cycle should have been killed by far.
            var historyBinder = GetCoplayerBinder(HistoryCoplayerId);
            if(historyBinder != null)
            {
                // This should not happen.
                throw new FrameworkRunnerException("History binder already exists!");
            }

            var historyThemeInfo = shellLibRestricted.ShellHistoryControl.GetHistoryThemeInformation();

            StartNewHistoryCoplayer(historyThemeInfo);
        }

        /// <summary>
        /// Synchronizes ShellRunner with Foundation during a play mode cold boot.
        /// </summary>
        /// <param name="runningCoplayers">
        /// The running coplayer ids kept by the runner.
        /// </param>
        /// <param name="vacantCoplayers">
        /// The vacant coplayer ids kept by the runner.
        /// </param>
        private void SyncColdBoot(int[] runningCoplayers, int[] vacantCoplayers)
        {
            // Verify that if there is no coplayer on Foundation side, there is none in ShellRunner either.
            // This exception should never happen.
            if(runningCoplayers.Length + vacantCoplayers.Length != 0)
            {
                var coplayerIds = string.Join(", ", runningCoplayers.Concat(vacantCoplayers).Select(id => id.ToString()).ToArray());
                var message = $"The coplayers tracked by Foundation (empty list) are different from that by ShellRunner ({coplayerIds}).";

                throw new FrameworkRunnerException(message);
            }
        }

        /// <summary>
        /// Synchronizes running coplayers in ShellRunner with that in Foundation
        /// during a play mode warm boot or a context re-activation.
        /// </summary>
        /// <param name="runningCoplayerInfos">
        /// The Foundation-maintained coplayers that has a valid theme identifier.
        /// </param>
        /// <param name="runningCoplayers">
        /// The running coplayer ids kept by the runner.
        /// </param>
        /// <returns>
        /// List of coplayers that are running according to Shell, but not according to Foundation.
        /// </returns>
        private int[] SyncRunningCoplayers(IList<CoplayerInfo> runningCoplayerInfos, int[] runningCoplayers)
        {
            var result = new int[0];

            // If there are existing running coplayers on Foundation side, but none is found here,
            // we are recovering from a power hit.  Restart all coplayers.
            if(!runningCoplayers.Any())
            {
                foreach(var coplayerInfo in runningCoplayerInfos)
                {
                    var themeInfo = shellLibRestricted.GetThemeInformation(coplayerInfo.ThemeIdentifier);

                    StartNewCoplayer(coplayerInfo.CoplayerId, themeInfo);
                }
            }
            else
            {
                var foundationTracked = runningCoplayerInfos.Select(info => info.CoplayerId).ToArray();

                // Verify the running coplayers tracked by Foundation are also running on Shell side.
                // This exception should never happen.
                var missingFromShell = foundationTracked.Except(runningCoplayers).ToArray();
                if(missingFromShell.Any())
                {
                    var message = $"Some of the running coplayers tracked by Foundation ({string.Join(", ", missingFromShell.Select(id => id.ToString()).ToArray())}) are not found in ShellRunner.";

                    throw new FrameworkRunnerException(message);
                }

                // If a coplayer was running before, but not running now, it must have been
                // disabled by the Foundation when Shell was inactive.
                result = runningCoplayers.Except(foundationTracked).ToArray();
            }

            return result;
        }

        /// <summary>
        /// Synchronizes vacant coplayers in ShellRunner with that in Foundation
        /// during a play mode warm boot or context re-activation.
        /// </summary>
        /// <param name="vacantCoplayerInfos">
        /// The Foundation-maintained coplayers that has no theme identifier.
        /// </param>
        /// <param name="vacantCoplayers">
        /// The vacant coplayer ids kept by the runner.
        /// </param>
        /// <returns>
        /// List of coplayers that are vacant according to Foundation, but not according to Shell.
        /// </returns>
        private int[] SyncVacantCoplayers(IList<CoplayerInfo> vacantCoplayerInfos, int[] vacantCoplayers)
        {
            int[] result;

            var foundationTracked = vacantCoplayerInfos.Select(info => info.CoplayerId).ToArray();

            // If there are existing vacant coplayers on Foundation side, but none is found here,
            // we are recovering from a power hit, or a coplayer has been stopped by Foundation.
            // Restore or update the vacant coplayers list.
            if(!vacantCoplayers.Any())
            {
                lock(vacantCoplayerIds)
                {
                    vacantCoplayerIds.AddRange(foundationTracked);
                }

                result = foundationTracked;
            }

            // Verify the vacant coplayers tracked by Foundation is the same as the ones in cache.
            // This exception should never happen.
            else
            {
                // Verify the vacant coplayers tracked by Shell are also vacant on Foundation side.
                // This exception should never happen.
                var missingFromFoundation = vacantCoplayers.Except(foundationTracked).ToArray();
                if(missingFromFoundation.Any())
                {
                    var message = $"Some of the vacant coplayers tracked by Shell ({string.Join(", ", missingFromFoundation.Select(id => id.ToString()).ToArray())}) are not found in Foundation.";

                    throw new FrameworkRunnerException(message);
                }

                // If a coplayer was not vacant before, but is vacant now, it must have been
                // disabled by the Foundation when Shell was inactive.
                result = foundationTracked.Except(vacantCoplayers).ToArray();
            }

            return result;
        }

        /// <summary>
        /// Synchronize coplayers that have been disabled when Shell was inactive.
        /// </summary>
        /// <param name="stoppedCoplayers">
        /// The list of coplayers that have stopped running when Shell was inactive.
        /// </param>
        /// <param name="vacatedCoplayers">
        /// The list of coplayers that have become vacant when Shell was inactive.
        /// This could include all current vacant coplayers if after a power hit.
        /// </param>
        private void SyncDisabledCoplayers(int[] stoppedCoplayers, int[] vacatedCoplayers)
        {
            // Verify that the coplayers stopped running are among those became vacant.
            // This exception should never happen.
            if(stoppedCoplayers.Except(vacatedCoplayers).Any())
            {
                var message = $"Not all coplayers stopped running ({string.Join(", ", stoppedCoplayers.Select(id => id.ToString()).ToArray())}) appear in those became vacant ({string.Join(", ", vacatedCoplayers.Select(id => id.ToString()).ToArray())}).";

                throw new FrameworkRunnerException(message);
            }

            // We should disable the coplayers on the Shell side as well.
            foreach(var coplayerId in stoppedCoplayers)
            {
                ShutDownCoplayer(coplayerId);
            }
        }

        /// <summary>
        /// Gets a sorted array of running coplayer ids in play mode.
        /// </summary>
        /// <remarks>
        /// There could be some coplayers running in play mode and one coplayer in history mode at the same time.
        /// </remarks>
        /// <returns>
        /// The sorted array of running coplayer ids in play mode.
        /// </returns>
        private int[] GetRunningCoplayerIds()
        {
            int[] runningCoplayers;

            // Lock and get all the info we need at once.
            // Do not lock the collection longer than needed because the coplayer's category negotiation might
            // end up killing the coplayer, which would need the lock to remove an item from the collection.
            lock(coplayerBinders)
            {
                runningCoplayers = GetPlayModeCoplayers().Select(binder => binder.CoplayerLibRestricted.CoplayerId)
                                                         .OrderBy(id => id)
                                                         .ToArray();
            }

            return runningCoplayers;
        }

        /// <summary>
        /// Gets a sorted array of vacant coplayer ids.
        /// </summary>
        /// <returns>
        /// The sorted array of vacant coplayer ids.
        /// </returns>
        private int[] GetVacantCoplayerIds()
        {
            int[] vacantCoplayers;
            lock(vacantCoplayerIds)
            {
                vacantCoplayers = vacantCoplayerIds.OrderBy(id => id).ToArray();
            }

            return vacantCoplayers;
        }

        #endregion

        #region Managing Coplayers

        /// <summary>
        /// Searches in data list sent by Foundation, and looks for the info on a cotheme by its G2S theme id.
        /// </summary>
        /// <param name="g2SThemeId">The G2S theme id to search for.</param>
        /// <returns>
        /// The info on the cotheme if one is found; null otherwise.
        /// </returns>
        private ShellThemeInfo GetThemeInfoByG2SThemeId(string g2SThemeId)
        {
            var themeInfo = selectableThemes.FirstOrDefault(theme => theme.G2SThemeId == g2SThemeId);

            return themeInfo;
        }

        /// <summary>
        /// Creates and Starts a new coplayer session in play mode .
        /// </summary>
        /// <param name="coplayerId">The coplayer id.</param>
        /// <param name="themeInfo">The theme info for the coplayer.</param>
        private void StartNewCoplayer(int coplayerId, ShellThemeInfo themeInfo)
        {
            // First create a session.
            var sessionId = shellLibRestricted.CreateSession();

            // Create a new coplayer binder, and start the coplayer thread.
            StartNewCoplayerBinder(coplayerId, sessionId, themeInfo);

            // Bind the coplayer to the session.
            shellLibRestricted.BindCoplayerSession(coplayerId, sessionId);

            // Tell foundation to start the session.
            // This does not return until coplayer context is activated.
            shellLibRestricted.LaunchSession(sessionId);
        }

        /// <summary>
        /// Creates and starts a new coplayer session for history display.
        /// </summary>
        /// <param name="historyThemeInfo">The history theme info for the coplayer.</param>
        private void StartNewHistoryCoplayer(HistoryThemeInfo historyThemeInfo)
        {
            // Create a ShellThemeInfo based on history theme info.
            var themeInfo = new ShellThemeInfo(historyThemeInfo.ThemeIdentifier,
                                               historyThemeInfo.G2SThemeId,
                                               historyThemeInfo.ThemeTag,
                                               historyThemeInfo.ThemeTagDataFile,
                                               new List<ShellThemeDenomInfo> { new ShellThemeDenomInfo(historyThemeInfo.Denomination, false) },
                                               historyThemeInfo.Denomination);

            // First create a session.
            var sessionId = shellLibRestricted.CreateSession();

            // Create a new coplayer binder, and start the coplayer thread.
            StartNewCoplayerBinder(HistoryCoplayerId, sessionId, themeInfo);

            // Bind the history context to the session.
            shellLibRestricted.ShellHistoryControl.BindSessionToHistory(sessionId);

            // Tell foundation to start the session.
            // This does not return until coplayer context is activated.
            shellLibRestricted.LaunchSession(sessionId);
        }

        /// <summary>
        /// Creates the lib, runner and thread for a coplayer, puts them in a new binder,
        /// and then starts running the thread.
        /// </summary>
        /// <param name="coplayerId">The coplayer id.</param>
        /// <param name="sessionId">The session id used by the coplayer.</param>
        /// <param name="themeInfo">The theme info for the coplayer.</param>
        private void StartNewCoplayerBinder(int coplayerId, int sessionId, ShellThemeInfo themeInfo)
        {
            Game.Core.Logging.Log.Write($"Starting Coplayer {coplayerId} on Session {sessionId}.");

            // First create a coplayer lib.
            var gameConfigurator = shellConfigQuery.GetGameConfigurator(themeInfo.G2SThemeId);
            var coplayerLibRestricted = platformFactory.CreateCoplayerLib(coplayerId,
                                                                          sessionId,
                                                                          gameConfigurator.GetInterfaceExtensionRequests());

            // Check the coplayer lib implementation immediately.
            // Don't wait till when the game state machine framework is constructed.
            if(!(coplayerLibRestricted is ICoplayerLib))
            {
                throw new FrameworkRunnerException("Coplayer lib instance does not implement ICoplayerLib.");
            }

            // Then create a coplayer runner.
            // Attention! Do NOT send the shell state machine framework to coplayer runners,
            // as shell state machine framework has shorter lifespan than coplayer runners.
            // For example, when going into history mode and back, the shell state machine framework
            // would have been disposed, but the coplayer runners are still running.
            var coplayerRunner = new CoplayerRunner(coplayerLibRestricted,
                                                    new ShellCallbacks
                                                        {
                                                            ConfigQuery = shellConfigQuery,
                                                            ParcelCallSender = shellParcelCallSender,
                                                            EventPoster = coplayerEventQueue,
                                                            EventQueuer = coplayerEventQueue,
                                                            HistoryQuery = this,
                                                            ShellTiltSender = shellTiltSender
                                                        });

            // Lastly create the coplayer thread.
            var coplayerThread = new WorkerThread(coplayerRunner);

            // Keep all elements in a binder for easier maintenance and access.
            var coplayerBinder = new CoplayerBinder(sessionId,
                                                    themeInfo,
                                                    coplayerThread,
                                                    coplayerRunner,
                                                    coplayerLibRestricted);

            // Add to the collection.
            lock(coplayerBinders)
            {
                coplayerBinders.Add(coplayerBinder);
            }

            // Start the coplayer.
            coplayerThread.Start();

            // Add the coplayer thread to health checks after it has started.
            threadHealthChecker.AddCheck(coplayerThread);
        }

        /// <summary>
        /// Gets a coplayer binder by the coplayer id.
        /// </summary>
        /// <param name="coplayerId">The coplayer id.</param>
        /// <returns>
        /// The binder if one is found; null otherwise.
        /// </returns>
        private CoplayerBinder GetCoplayerBinder(int coplayerId)
        {
            lock(coplayerBinders)
            {
                return coplayerBinders.FirstOrDefault(binder => binder.CoplayerLibRestricted.CoplayerId == coplayerId);
            }
        }

        /// <summary>
        /// Gets all the coplayer binders that run in play mode.
        /// </summary>
        /// <returns>
        /// The list of coplayer binders that run in play mode.
        /// </returns>
        private IEnumerable<CoplayerBinder> GetPlayModeCoplayers()
        {
            lock(coplayerBinders)
            {
                return coplayerBinders.Where(binder => binder.CoplayerLibRestricted.CoplayerId != HistoryCoplayerId);
            }
        }

        /// <summary>
        /// Finds the next available coplayer id to start a new coplayer.
        /// If one is available in the vacant list, return that one;
        /// Otherwise get a new one from Foundation.
        /// </summary>
        /// <returns>
        /// The next available coplayer id.
        /// <see cref="InvalidCoplayerId"/> if the max number of coplayers has been reached.
        /// </returns>
        private int GetNextAvailableCoplayerId()
        {
            var result = InvalidCoplayerId;

            lock(vacantCoplayerIds)
            {
                // Reuse the vacant coplayer with the lowest coplayer id, if any.
                if(vacantCoplayerIds.Any())
                {
                    result = vacantCoplayerIds.Min();
                    vacantCoplayerIds.Remove(result);
                }
            }

            if(result == InvalidCoplayerId)
            {
                int coplayerCount;

                lock(coplayerBinders)
                {
                    coplayerCount = GetPlayModeCoplayers().Count();
                }

                // If we haven't reached the limit yet...
                if(coplayerCount < shellLib.MaxNumCoplayers)
                {
                    // Ask Foundation to create one more coplayer.
                    var createdCoplayers = shellLibRestricted.CreateCoplayers(1);
                    if(createdCoplayers.Count > 0)
                    {
                        result = createdCoplayers.First();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Adds the given coplayer id to the vacant list.  The vacant coplayer ids
        /// will later be reused for new coplayers.
        /// </summary>
        /// <param name="coplayerId">
        /// The identifier of the coplayer.
        /// </param>
        private void AddToVacantList(int coplayerId)
        {
            if(coplayerId == HistoryCoplayerId)
            {
                return;
            }

            lock(vacantCoplayerIds)
            {
                if(!vacantCoplayerIds.Contains(coplayerId))
                {
                    vacantCoplayerIds.Add(coplayerId);
                }
            }
        }

        /// <summary>
        /// Kills a coplayer binder.
        /// </summary>
        /// <remarks>
        /// This destroys the CoplayerBinder and everything inside the binder,
        /// including the thread, the thread runner and the lib.
        /// </remarks>
        /// <param name="coplayerId">
        /// The id of the coplayer to kill.
        /// </param>
        private void KillCoplayer(int coplayerId)
        {
            lock(coplayerBinders)
            {
                var binder = GetCoplayerBinder(coplayerId);

                if(binder != null)
                {
                    Game.Core.Logging.Log.Write($"Killing Coplayer {coplayerId} thread.");

                    // Remove the coplayer thread from the health checks before stopping it.
                    threadHealthChecker.RemoveCheck(binder.CoplayerThread);

                    // Stop the coplayer thread.
                    binder.CoplayerThread.Stop();

                    binder.CoplayerThread.Dispose();
                    binder.CoplayerRunner.Dispose();

                    if(binder.CoplayerLibRestricted is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    coplayerBinders.Remove(binder);

                    // Only make the coplayer id available for reuse after the old coplayer has been terminated completely.
                    // Note that there could be only one GL2P comm channel for a specific coplayer id.
                    AddToVacantList(coplayerId);
                }
            }
        }

        /// <summary>
        /// Kills all existing coplayer binders.
        /// </summary>
        private void KillAllCoplayers()
        {
            lock(coplayerBinders)
            {
                // Get the coplayer id list to enumerate.  Do not enumerate on coplayerBinders directly,
                // as KillCoplayer involves removing items from the coplayerBinders list.
                var coplayerIdList = coplayerBinders.Select(binder => binder.CoplayerLibRestricted.CoplayerId).ToList();

                foreach(var coplayerId in coplayerIdList)
                {
                    KillCoplayer(coplayerId);
                }
            }
        }

        #endregion

        #region Routing Events to Coplayers

        /// <summary>
        /// Returns the coplayer binders of the giving coplayer ids.
        /// </summary>
        /// <param name="targetCoplayers">
        /// The target coplayer ids.  Null or empty list means routing to all coplayers.
        /// </param>
        /// <remarks>
        /// Coplayer ids for which no binder is found are simply ignored.
        /// </remarks>
        /// <returns>
        /// The list of coplayer binders of the given coplayer ids.
        /// </returns>
        private IEnumerable<CoplayerBinder> GetTargetCoplayerBinders(IReadOnlyList<int> targetCoplayers)
        {
            IEnumerable<CoplayerBinder> targetBinders;

            lock(coplayerBinders)
            {
                if(targetCoplayers?.Any() == true)
                {
                    // Route to specific coplayers.
                    targetBinders = targetCoplayers.Select(coplayerId => GetCoplayerBinder(coplayerId)).Where(binder => binder != null);
                }
                else
                {
                    // Null or empty list means routing to all coplayers.
                    // If in history, forward the event to the history coplayer;
                    // Otherwise, forward to all running coplayers in play mode.
                    targetBinders = shellLib.Context.GameMode != GameMode.History
                                        ? GetPlayModeCoplayers().ToList()
                                        : new List<CoplayerBinder> { GetCoplayerBinder(HistoryCoplayerId) };
                }
            }

            return targetBinders;
        }

        #endregion

        #endregion
    }
}