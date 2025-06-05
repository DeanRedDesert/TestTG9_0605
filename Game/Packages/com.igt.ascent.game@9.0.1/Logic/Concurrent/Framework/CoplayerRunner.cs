// -----------------------------------------------------------------------
// <copyright file = "CoplayerRunner.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Game.Core.Communication.CommunicationLib;
    using Game.Core.Communication.Logic.CommServices;
    using Game.Core.Threading;
    using IGT.Ascent.Restricted.EventManagement;
    using IGT.Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// This runner is in charge of running a coplayer logic thread, and responsible for
    /// creating and destroying the coplayer state machine framework at appropriate times.
    /// </summary>
    internal sealed class CoplayerRunner : RunnerBase,
                                           IThreadWorker,
                                           IGameFrameworkRunner
    {
        #region Private Fields

        /// <summary>
        /// The game configurator provided by the game.
        /// </summary>
        private IGameConfigurator gameConfigurator;

        /// <summary>
        /// Object for the coplayer to communicate with Foundation.
        /// </summary>
        private readonly ICoplayerLibRestricted coplayerLibRestricted;

        /// <summary>
        /// The object for the coplayer to communicate back with shell.
        /// </summary>
        private readonly ShellCallbacks shellCallbacks;

        /// <summary>
        /// The queue of events from the shell.
        /// Since shell and coplayers cannot share transactions (because they are on different sessions),
        /// all the events in this queue will be non-transactional.
        /// It can handle both blocking and non-blocking events.
        /// </summary>
        private readonly DuplexEventQueue shellEventQueue;

        /// <summary>
        /// The wait handle used to indicate that the shell logic thread should be terminated.
        /// </summary>
        private WaitHandle exitHandle;

        /// <summary>
        /// The running shell state machine.
        /// </summary>
        private GameStateMachineFramework stateMachineFramework;

        // The wait handle used to indicate that the running shell state machine framework should be terminated.
        // It is signaled and waited on in the same thread in order to simplify the coordination work with
        // the state machine framework object.
        private readonly ManualResetEvent exitFrameworkEvent;

        /// <summary>
        /// The logic comm services used by the coplayer.
        /// </summary>
        private ILogicCommServices logicCommServices;

        /// <summary>
        /// The service request data configured for the coplayer.
        /// </summary>
        private IDictionary<string, ServiceRequestData> serviceRequestDataMap;

        /// <summary>
        /// The G2S Theme Id of current cotheme running in the coplayer.
        /// </summary>
        private string currentThemeId;

        #endregion

        #region Properties

        /// <summary>
        /// Gets event poster for the shell to post events to this coplayer.
        /// </summary>
        public IEventPoster EventPoster => shellEventQueue;

        /// <summary>
        /// Gets event queuer for the shell to enqueue events to this coplayer.
        /// </summary>
        public IEventQueuer EventQueuer => shellEventQueue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerRunner"/>.
        /// </summary>
        /// <param name="coplayerLibRestricted">
        /// The object for the coplayer to communicate with Foundation.
        /// </param>
        /// <param name="shellCallbacks">
        /// The object for the coplayer to communicate back with shell.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either of the arguments is null.
        /// </exception>
        public CoplayerRunner(ICoplayerLibRestricted coplayerLibRestricted,
                              ShellCallbacks shellCallbacks)
        {
            this.coplayerLibRestricted = coplayerLibRestricted ?? throw new ArgumentNullException(nameof(coplayerLibRestricted));
            this.shellCallbacks = shellCallbacks ?? throw new ArgumentNullException(nameof(shellCallbacks));

            // Create wait handle for exiting framework.
            exitFrameworkEvent = new ManualResetEvent(false);
            DisposableCollection.Add(exitFrameworkEvent);

            ReturnHandles = new List<WaitHandle> { exitFrameworkEvent };

            // Create Event Queue Manager.
            EventQueueManager = new EventQueueManager(coplayerLibRestricted.ExceptionMonitor);

            // Create the shell event queue.
            shellEventQueue = new DuplexEventQueue();
            DisposableCollection.Add(shellEventQueue);

            // Register event queues.  The order of registration matters.
            EventQueueManager.RegisterEventQueue(coplayerLibRestricted.TransactionalEventQueue);
            EventQueueManager.RegisterEventQueue(coplayerLibRestricted.NonTransactionalEventQueue);
            EventQueueManager.RegisterEventQueue(shellEventQueue);

            // Keep a cached list for better performance.
            TransEventQueueList = new List<IEventQueue>
                                      {
                                          coplayerLibRestricted.TransactionalEventQueue,
                                      };

            // Monitor if an event has been processed in selected event queues.
            // This is mainly used for sending async service data over to the presentation side,
            // therefore no need to monitor the presentation queue.
            var eventQueuesToMonitor = new List<IEventQueue>
                                           {
                                               coplayerLibRestricted.TransactionalEventQueue,
                                               coplayerLibRestricted.NonTransactionalEventQueue,
                                               shellEventQueue
                                           };

            foreach(var eventQueue in eventQueuesToMonitor)
            {
                if(eventQueue is IEventDispatchMonitor dispatchMonitor)
                {
                    dispatchMonitor.EventDispatchEnded += HandlePostProcessingEvent;
                }
            }

            // Hook up event handling.
            coplayerLibRestricted.NewCoplayerContextEvent += HandleNewCoplayerContext;
            coplayerLibRestricted.ActivateCoplayerContextEvent += HandleActivateCoplayerContext;
            coplayerLibRestricted.InactivateCoplayerContextEvent += HandleInactivateCoplayerContext;
            coplayerLibRestricted.ShutDownEvent += HandleShutDown;

            shellEventQueue.EventDispatchedEvent += HandleShellEvent;
        }

        #endregion

        #region IThreadWorker Members

        /// <inheritdoc/>
        public string ThreadName => "Coplayer Runner #" + coplayerLibRestricted.CoplayerId;

        /// <inheritdoc/>
        public void DoWork(WaitHandle waitHandle)
        {
            Game.Core.Logging.Log.Write($"Coplayer {coplayerLibRestricted.CoplayerId} thread started.");

            exitHandle = waitHandle ?? throw new ArgumentNullException(nameof(waitHandle));

            // Wait till both link level and coplayer level negotiation is complete.
            coplayerLibRestricted.ConnectToFoundation();

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
                    else
                    {
                        stateMachineFramework.ExecuteState();
                    }
                }
                else
                {
                    EventQueueManager.ProcessEvents(ReturnHandles,
                                                    eventArgs => eventArgs is ActivateCoplayerContextEventArgs,
                                                    TransEventQueueList);
                }
            }

            Game.Core.Logging.Log.Write($"Coplayer {coplayerLibRestricted.CoplayerId} thread ended.");
        }

        #endregion

        #region IGameFrameworkRunner Implementation

        #region IFrameworkRunner Part

        // Part of the interface implementation is in the base class.

        /// <inheritdoc/>
        public bool DoTransaction(string transactionName, Action action)
        {
            void Handler(object sender, ActionResponseEventArgs eventArgs) => action();

            coplayerLibRestricted.ActionResponseEvent += Handler;

            coplayerLibRestricted.ActionRequest(ThreadName + " " + transactionName);

            var transactionOpened = ProcessEvents(eventArgs => eventArgs is ActionResponseEventArgs,
                                                  TransEventQueueList);

            coplayerLibRestricted.ActionResponseEvent -= Handler;

            return transactionOpened;
        }

        /// <inheritdoc/>
        public bool DoTransactionLite(string transactionName, Action action)
        {
            void Handler(object sender, ActionResponseLiteEventArgs eventArgs) => action();

            coplayerLibRestricted.ActionResponseLiteEvent += Handler;

            coplayerLibRestricted.ActionRequestLite(ThreadName + " " + transactionName);

            var transactionOpened = ProcessEvents(eventArgs => eventArgs is ActionResponseLiteEventArgs,
                                                  TransEventQueueList);

            coplayerLibRestricted.ActionResponseLiteEvent -= Handler;

            return transactionOpened;
        }

        /// <inheritdoc/>
        public bool OnNextTransaction(string transactionName, Action action)
        {
            coplayerLibRestricted.ActionRequest(ThreadName + " " + transactionName);

            var transactionOpened = ProcessEvents(eventArgs => LeverageHeavyweightEvent(eventArgs, action),
                                                  TransEventQueueList);

            return transactionOpened;
        }

        #endregion

        #region IGameFrameworkRunner Part

        /// <inheritdoc/>
        public IShellHistoryQuery ShellHistoryQuery => shellCallbacks.HistoryQuery;

        /// <inheritdoc/>
        public IShellParcelCallSender ParcelCallSender => shellCallbacks.ParcelCallSender;

        /// <inheritdoc/>
        public IShellTiltSender ShellTiltSender => shellCallbacks.ShellTiltSender;

        /// <inheritdoc/>
        public bool SendEventToShell(CustomCoplayerEventArgs eventArgs)
        {
            shellCallbacks.EventQueuer.EnqueueEvent(eventArgs);

            return true;
        }

        /// <inheritdoc/>
        public event EventHandler<EventDispatchedEventArgs> ShellEventReceived;

        #endregion

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        protected override void ClearEventHandlers()
        {
            base.ClearEventHandlers();
            ShellEventReceived = null;
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
                DisposeStateMachineFramework();
            }

            // Call base method last as it modifies the IsDisposed flag.
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        #region Initialization and Clean up

        /// <summary>
        /// Disposes the state machine framework.
        /// </summary>
        private void DisposeStateMachineFramework()
        {
            // Stop logic comm services.
            logicCommServices?.LogicHostControl.Stop();

            stateMachineFramework?.Dispose();
            stateMachineFramework = null;

            // Clear all event handlers exposed to the state machine framework.
            ClearEventHandlers();
       }

        #endregion

        #region Handling Coplayer Lib Events

        /// <summary>
        /// Handles the coplayer lib new coplayer context event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleNewCoplayerContext(object sender, NewCoplayerContextEventArgs eventArgs)
        {
            // If a state machine framework is running, stop it first.
            if(stateMachineFramework != null)
            {
                exitFrameworkEvent.Set();
            }
        }

        /// <summary>
        /// Handles the coplayer lib activate coplayer context event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleActivateCoplayerContext(object sender, ActivateCoplayerContextEventArgs eventArgs)
        {
            // By this time CoplayerLib has been updated with all new context data.
            var newContext =((ICoplayerLib)coplayerLibRestricted).Context;

            Game.Core.Logging.Log.Write($"Activating CoplayerContext: {newContext}");

            var newThemeId = newContext.G2SThemeId;

            // If the theme has changed, update all theme specific information,
            // including game configurator, logic comm services, service request data etc.
            if(currentThemeId == null || currentThemeId != newThemeId)
            {
                ChangeTheme(newThemeId);
            }

            currentThemeId = newThemeId;

            // Start the GL2P.  It was stopped no matter whether the theme has changed.
            logicCommServices.LogicHostControl.Start();

            // Start the game state machine.
            IGameStateMachine stateMachine;

            switch(newContext.GameMode)
            {
                case GameMode.Play:
                {
                    stateMachine = gameConfigurator.CreateGameStateMachine(newContext);
                    break;
                }
                case GameMode.History:
                {
                    stateMachine = new CoplayerHistoryStateMachine();
                    break;
                }
                default:
                {
                    var message = $"Activating coplayer context in game mode {newContext.GameMode} is not supported.";
                    throw new NotSupportedException(message);
                }
            }

            stateMachineFramework = new GameStateMachineFramework(this,
                                                                  (ICoplayerLib)coplayerLibRestricted,
                                                                  coplayerLibRestricted,
                                                                  stateMachine,
                                                                  logicCommServices.PresentationClient,
                                                                  serviceRequestDataMap,
                                                                  shellCallbacks.ConfigQuery.GetCoplayerInitData(),
                                                                  $"#{newContext.CoplayerId}");
        }

        /// <summary>
        /// Handles the coplayer lib inactivate coplayer context event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleInactivateCoplayerContext(object sender, InactivateCoplayerContextEventArgs eventArgs)
        {
            Game.Core.Logging.Log.Write($"Inactivating CoplayerContext: {eventArgs.LastActiveContext}");

            // Clear the theme when it is inactivated. This will ensure that it is loaded if it is re-activated.
            currentThemeId = null;
            exitFrameworkEvent.Set();

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
        }

        /// <summary>
        /// Handles the coplayer lib shut down event.
        /// Notifies shell, which will then shuts down this coplayer.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleShutDown(object sender, ShutDownEventArgs eventArgs)
        {
            // Send an event to shell.
            shellCallbacks.EventQueuer.EnqueueEvent(new CoplayerShutDownEventArgs(coplayerLibRestricted.CoplayerId));
        }

        #endregion

        #region Handling Shell Events

        /// <summary>
        /// Handles the events sent from shell.
        /// Forwards it to shell state machine framework.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleShellEvent(object sender, EventDispatchedEventArgs eventArgs)
        {
            // Forward to state machine framework for handling.
            ShellEventReceived?.Invoke(this, eventArgs);
        }

        #endregion

        #region Handling Theme Change

        /// <summary>
        /// Changes the cotheme running in this coplayer.
        /// </summary>
        /// <param name="newG2SThemeId">
        /// The G2S Theme Id of the new theme.
        /// </param>
        private void ChangeTheme(string newG2SThemeId)
        {
            // First, we need to unregister the old presentation event queue.
            // Skip it if this is the first theme.
            if(logicCommServices != null)
            {
                logicCommServices.PresentationEventDispatcher.EventDispatchedEvent -= HandlePresentationEvent;
                EventQueueManager.UnregisterEventQueue(logicCommServices.PresentationEventQueue);
            }

            // Next, update theme specific fields.
            gameConfigurator = shellCallbacks.ConfigQuery.GetGameConfigurator(newG2SThemeId);
            logicCommServices = shellCallbacks.ConfigQuery.GetLogicCommServices(newG2SThemeId, coplayerLibRestricted.CoplayerId);
            serviceRequestDataMap = shellCallbacks.ConfigQuery.GetServiceRequestDataMap(newG2SThemeId);

            if(serviceRequestDataMap == null)
            {
                throw new FrameworkRunnerException("No Service Request Data was found for G2S Theme ID: " + newG2SThemeId);
            }

            // Register the new presentation event queue.
            EventQueueManager.RegisterEventQueue(logicCommServices.PresentationEventQueue);
            logicCommServices.PresentationEventDispatcher.EventDispatchedEvent += HandlePresentationEvent;
        }

        #endregion

        #endregion
    }
}