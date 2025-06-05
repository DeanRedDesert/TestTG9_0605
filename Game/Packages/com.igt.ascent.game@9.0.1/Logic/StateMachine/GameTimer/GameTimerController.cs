//-----------------------------------------------------------------------
// <copyright file = "GameTimerController.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Communication.Foundation;
    using Timers = System.Timers;

    /// <summary>
    /// A class that manages timers that can be persisted and have their events raised on the game logic thread.
    /// </summary>
    internal class GameTimerController : IGameTimerController, IGameTimerControllerInternal, IGameLibEventListener, IEventSource, IDisposable
    {
        #region Private

        // The system timer used to drive all game timer and persistence actions.
        private Timers.Timer heartBeat;

        // Locking object that locks the timer event queue.
        private readonly object timerTickEventLocker = new object();

        // A dictionary of game timer names vs. the game timer object.
        private readonly Dictionary<string, GameTimer> timerDictionary = new Dictionary<string, GameTimer>();

        // The critical data root to append timer names to when creating the final full path for game timer storage.
        private const string CriticalDataRoot = "PersistentGameTimer/";

        // The hard coded string indicating the name of a game timer related transaction.
        private const string TransactionName = "GameTimerControllerAction";

        // The default duration in ms between the system timer tick events. A value of 100 specifies that the timer will fire every 100 ms.
        private const uint HeartBeatTickInterval = 100;

        // The scaling factor used to determine when 1 second has passed. This value times the 'HeartBeatTickInterval' should equal 1000.
        // If a faster resolution is needed (as an example, for multiple tests) a smaller value can be used.
        private const uint TimerTickScaleFactorDefault = 10;

        // Flag indicating the game controller is currently disposing, or has been disposed.
        private volatile bool isDisposed;

        // Flag indicating that at least one game timer needs to have its event(s) dequeued and raised, and/or its critical data state updated.
        private volatile bool timerEventsNeedProcessing;

        // Flag indicating that at least one game timer needs to have its state saved or removed from safe storage.
        private volatile bool timerStatesNeedPersistenceAction;

        // Flag indicating that a queued transaction callback is pending and there is no need to create another.
        private volatile bool queuedTransactionExists;

        // The backing event for the wait handle used to signal the event coordinator.
        private ManualResetEvent eventPosted;

        // These are the various interfaces need by this controller. They are cast from the single IStateMachineFramework instance passed in.
        private readonly IGameLibRestricted gameLibRestricted;
        private readonly IGameLib iGameLib;
        private readonly IEventCoordinator eventCoordindator;
        private readonly ITransactionAugmenter transactionAugmenter;
        private readonly ITransactionManager transactionManager;

        #endregion

        /// <summary>
        /// The number of 100 ms interval ticks from the heartbeat that will indicate one timer event to be raised.
        /// By default this value is set to 10, meaning that timer events be raised at 1 second intervals.
        /// </summary>
        private uint TimerTickScaleFactor
        {
            get; set;
        }

        #region Constructors

        /// <summary>
        /// Construct an instance of <see cref="GameTimerController"/> with the supplied <see cref="IStateMachineFramework"/>
        /// instance.
        /// </summary>
        /// <param name="framework">The <see cref="IStateMachineFramework"/> interface.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <see paramref="framework"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required interfaces that <see paramref="framework"/> should
        ///  contain are not supported.</exception>
        internal GameTimerController(IStateMachineFramework framework)
        {
            if(framework == null)
            {
                throw new ArgumentNullException(nameof(framework));
            }

            iGameLib = framework.GameLib;
            gameLibRestricted = iGameLib as IGameLibRestricted;

            if(gameLibRestricted == null)
            {
                throw new ArgumentException("The passed in IStateMachineFramework.IGameLib interface must support" +
                                            "IGameLibRestricted.");
            }

            transactionAugmenter = gameLibRestricted.GetServiceInterface<ITransactionAugmenter>();

            if(transactionAugmenter == null)
            {
                throw new ArgumentException("The passed in IStateMachineFramework interface must support an" +
                                                " ITransactionAugmenter Service Interface from its IGameLibRestricted interface.");
            }

            eventCoordindator = gameLibRestricted.GetServiceInterface<IEventCoordinator>();

            if(eventCoordindator == null)
            {
                throw new ArgumentException("The passed in IStateMachineFramework interface must support an" +
                                    " IEventCoordinator Service Interface from its IGameLibRestricted interface.");
            }

            transactionManager = framework as ITransactionManager;
            TimerTickScaleFactor = TimerTickScaleFactorDefault;
        }

        #endregion

        #region IGameTimerControllerInternal implementation

        /// <inheritdoc />
        void IGameTimerControllerInternal.SetTimerOverrides(ushort timerTickScaleFactor)
        {
            TimerTickScaleFactor = timerTickScaleFactor;
        }

        #endregion

        #region IGameTimerController implementation

        /// <inheritdoc />
        public void Initialize()
        {
            if(!Initialized && !isDisposed)
            {
                eventPosted = new ManualResetEvent(false);
                eventCoordindator.RegisterEventSource(this);
                transactionAugmenter.TransactionClosingEvent += OnTransactionClosing;

                heartBeat = new Timers.Timer(HeartBeatTickInterval);
                heartBeat.Elapsed += OnHeartBeat;
                Initialized = true;
                TimerTickScaleFactor = TimerTickScaleFactorDefault;
            }
        }
        /// <inheritdoc />
        public void PauseAll()
        {
            Paused = true;

            if(Initialized && !isDisposed && heartBeat != null)
            {
               heartBeat.Enabled = false;
            }
        }

        /// <inheritdoc />
        public void ResumeAll()
        {
            Paused = false;

            if(Initialized && !isDisposed && heartBeat != null)
            {
                heartBeat.Enabled = true;
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown if the game timer controller has not been previously initialized.
        /// </exception>
        public IGameTimer CreateTimer(string timerName, uint baseValue, uint targetValue, uint tickInterval, uint persistenceInterval)
        {
            if(string.IsNullOrEmpty(timerName))
            {
                throw new ArgumentNullException(nameof(timerName));
            }

            GameTimer timer = null;

            Initialize();

            if(Initialized)
            {
                var criticalDataPath = CriticalDataRoot + timerName;
                timer = new GameTimer(timerName, baseValue, targetValue, tickInterval * TimerTickScaleFactor,
                                      persistenceInterval * TimerTickScaleFactor, criticalDataPath);

                timer.GameTimerStatusChangedEvent += HandleGameTimerStatusChangedEvent;
                AddOrReplaceTimer(timer);
            }

            return timer;
        }

        /// <inheritdoc />
        public void LoadTimerStates()
        {
            Initialize();

            if(Initialized)
            {
                lock(timerTickEventLocker)
                {
                    foreach(var timer in timerDictionary)
                    {
                        if(timer.Value?.PersistenceInterval > 0)
                        {
                            var criticalDataPath = CriticalDataRoot + timer.Key;
                            var timerState = LoadTimerState(iGameLib, criticalDataPath);

                            // Restore the timer from its critical data state.
                            if(timerState != null)
                            {
                                timerEventsNeedProcessing = true;
                                timerDictionary[timer.Key].RestoreValues(timerState);
                            }
                        }
                    }
                }

                // Signal the event coordinator if any 'Restore' events need to be raised.
                if(timerEventsNeedProcessing)
                {
                    eventPosted.Set();
                }
            }
        }

        /// <inheritdoc />
        public IGameTimer GetTimer(string name)
        {
            GameTimer timer = null;

            // Check if the timer exists in memory.
            lock(timerTickEventLocker)
            {
                if(timerDictionary.ContainsKey(name))
                {
                    timer = timerDictionary[name];
                }
            }

            return timer;
        }

        /// <inheritdoc />
        public bool RemoveTimer(string name)
        {
            return RemoveTimer(name, true);
        }

        /// <inheritdoc />
        public bool Initialized
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool Paused
        {
            get;
            private set;
        }

        #endregion

        #region IEventSource

        /// <inheritdoc />
        public WaitHandle EventPosted => eventPosted;

        /// <inheritdoc />
        public void ProcessEvents()
        {
            if(isDisposed || !Initialized)
            {
                return;
            }

            if(!Paused && timerEventsNeedProcessing)
            {
                // Process all queued timers. This happens when (if) the wait handle is signaled in the
                // system timer callback processing.
                lock(timerTickEventLocker)
                {
                    foreach(var timer in timerDictionary.Values)
                    {
                        if(timer.AnyEventsPending)
                        {
                            // Raise any and all queued events.
                            timer.RaiseAllQueuedEvents();
                        }
                    }
                }

                timerEventsNeedProcessing = false;
            }

            // Tell the event manager that we're done.
            eventPosted.Reset();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the internal heartbeat (system timer) tick event.
        /// </summary>
        /// <param name="sender">The <see cref="Timers.Timer"/>sender of this event.</param>
        /// <param name="eventArgs">Event args of type <see cref="Timers.ElapsedEventArgs"/>.</param>
        private void OnHeartBeat(object sender, Timers.ElapsedEventArgs eventArgs)
        {
            // Check if we are in the process of disposing; if so, then don't queue any more system timer
            // ticks that may still be incoming from a timer thread.
            if(isDisposed)
            {
                return;
            }

            lock(timerTickEventLocker)
            {
                foreach(var timer in timerDictionary.Values)
                {
                    if(timer != null)
                    {
                        timer.OnHeartBeat();

                        // Check and set a flag if any events need to be raised.
                        if(timer.AnyEventsPending)
                        {
                            timerEventsNeedProcessing = true;
                        }

                        // Flag and store any critical data actions that need to be taken.
                        if(timer.PersistenceAction != GameTimer.PersistenceActionType.DoNothing)
                        {
                            timerStatesNeedPersistenceAction = true;
                        }
                    }
                }
            }

            // Queue up a transaction callback.
            if(timerStatesNeedPersistenceAction)
            {
                SubmitQueuedTransaction();
            }

            // Signal the event coordinator if events need to be raised.
            if(timerEventsNeedProcessing)
            {
                eventPosted.Set();
            }

            // See if we still need to have the heartbeat enabled.
            DetermineHeartBeatEnabledStatus();
        }

        /// <summary>
        /// Determines if the heartbeat should be enabled or disabled; used to optimize performance.
        /// </summary>
        private void DetermineHeartBeatEnabledStatus()
        {
            lock(timerTickEventLocker)
            {
                heartBeat.Enabled = timerDictionary.Values.Any(timer => timer.IsActive);
            }
        }

        /// <summary>
        /// Handles a game timer's status changing event. Currently, we use this only to enable the heartbeat if it was previously
        /// stopped due to no active timers ticking.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="timerStatusChangedEvent">The event arguments of type <see cref="GameTimerEventArgs"/>.</param>
        private void HandleGameTimerStatusChangedEvent(object sender, GameTimerEventArgs timerStatusChangedEvent)
        {
            if(!heartBeat.Enabled && timerStatusChangedEvent.TimerStatus == GameTimerStatus.Started)
            {
                lock(timerTickEventLocker)
                {
                    if(heartBeat != null)
                    {
                        heartBeat.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Queue up a transaction callback - let's the transaction manager know safe storage needs to be managed.
        /// </summary>
        private void SubmitQueuedTransaction()
        {
            // Check if we already have a transaction queued up.
            if(!queuedTransactionExists)
            {
                queuedTransactionExists = true;
                var queuedOperation = new QueuedOperation(transactionManager, TransactionName);
                queuedOperation.Submit(QueuedTransactionCallback);
            }
        }

        /// <summary>
        /// Add, update or remove timer states in safe storage.
        /// </summary>
        /// <param name="writeAll">A flag indicating if all timers should have their states persisted regardless
        /// of when the last persistence action was taken.</param>
        private void ManageTimerStatesInCriticalData(bool writeAll)
        {
            if(isDisposed || Paused)
            {
                return;
            }

            lock(timerTickEventLocker)
            {
                var removalList = new List<string>();

                foreach(var timerKvp in timerDictionary)
                {
                    var timerName = timerKvp.Key;
                    var timer = timerKvp.Value;

                    if(timer == null)
                    {
                        continue;
                    }

                    if(timer.PersistenceInterval > 0)
                    {
                        bool write = false, remove = false;
                        var writeable = !timer.TargetReached && timer.IsActive;

                        switch(timer.PersistenceAction)
                        {
                            case GameTimer.PersistenceActionType.DoNothing:
                                write = writeable && writeAll;
                                break;
                            case GameTimer.PersistenceActionType.Save:
                                write = writeable;
                                break;
                            case GameTimer.PersistenceActionType.Remove:
                                remove = true;
                                break;
                        }

                        if(write)
                        {
                            var timerState = new GameTimerStorage(timer);
                            iGameLib.WriteCriticalData(CriticalDataScope.Theme, timer.CriticalDataPath, timerState);
                            timer.ResetPersistenceStatus();
                        }
                        else if(remove)
                        {
                            iGameLib.RemoveCriticalData(CriticalDataScope.Theme, timer.CriticalDataPath);
                            timer.ResetPersistenceStatus();
                        }
                    }

                    // If the timer is being completely removed, mark it for removal from the controller dictionary.
                    if(timer.MarkedForDelayedDelete)
                    {
                        removalList.Add(timerName);
                    }
                }

                // Remove the timer(s) from the timer collection if flagged for removal.
                foreach(var timerName in removalList)
                {
                    timerDictionary.Remove(timerName);
                }

                // Reset the timer collection persistence needed flag.
                timerStatesNeedPersistenceAction = false;
            }
        }

        /// <summary>
        /// Removes a timer from the timer collection and optionally removes its persisted state from
        /// safe storage as well.
        /// </summary>
        /// <param name="name">The timer name.</param>
        /// <param name="flagForSafeStorageRemoval">Flag indicating if the persisted timer state should be
        /// removed from safe storage.</param>
        /// <returns>Flag indicating if the timer was present in the timer collection.</returns>
        private bool RemoveTimer(string name, bool flagForSafeStorageRemoval)
        {
            var result = false;

            lock(timerTickEventLocker)
            {
                if(timerDictionary.ContainsKey(name))
                {
                    var timer = timerDictionary[name];
                    timer.Stop();

                    if(flagForSafeStorageRemoval)
                    {
                        timer.MarkedForDelayedDelete = true;
                        SubmitQueuedTransaction();
                    }

                    timer.GameTimerStatusChangedEvent -= HandleGameTimerStatusChangedEvent;
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Adds or replaces a game timer instance to the collection of timers.
        /// </summary>
        /// <param name="timer">The <see cref="GameTimer"/> instance to add or overwrite an existing timer with.</param>
        private void AddOrReplaceTimer(GameTimer timer)
        {
            // Add timer to the dictionary. Check to see if it exists first and overwrite if so.
            if(timer != null)
            {
                lock(timerTickEventLocker)
                {
                    var timerName = timer.Name;

                    // Check if the timer is already in use.
                    if(timerDictionary.ContainsKey(timerName))
                    {
                        timerDictionary[timerName].Stop();
                        timerDictionary[timerName].Dispose();
                        timerDictionary[timerName] = timer;
                    }
                    else
                    {
                        timerDictionary.Add(timerName, timer);
                    }
                }
            }
        }

        /// <summary>
        /// Load a safe stored timer state.
        /// </summary>
        /// <param name="callerGameLib">The <see cref="IGameLib"/> that supports critical data reading and writing.</param>
        /// <param name="criticalDataPath">The full critical path of the timer to retrieve from safe storage.</param>
        /// <returns>A <see cref="GameTimerStorage"/> object containing a found persisted timer state; null if not.</returns>
        private GameTimerStorage LoadTimerState(IGameLib callerGameLib, string criticalDataPath)
        {
            GameTimerStorage timerState = null;

            if(callerGameLib != null)
            {
                var results = gameLibRestricted.CreateTransaction("GameTimerStates");

                if(results == ErrorCode.NoError || results == ErrorCode.OpenTransactionExisted)
                {
                    timerState = callerGameLib.ReadCriticalData<GameTimerStorage>(CriticalDataScope.Theme, criticalDataPath);
                }
            }

            return timerState;
        }

        /// <summary>
        /// The function to be called from the previously submitted transaction manager's queued operations processing.
        /// </summary>
        /// <param name="gameLibQueued">The <see cref="IGameLib"/> interface that supports critical data access.</param>
        private void QueuedTransactionCallback(IGameLib gameLibQueued)
        {
            ManageTimerStatesInCriticalData(false);
            queuedTransactionExists = false;
        }

        /// <summary>
        /// Event handler for the transaction manager's TransactionClosingEvent. This method is called to take advantage of an open
        /// transaction and to write any timer states that need persisting.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Event args of type <see cref="EventArgs"/>.</param>
        private void OnTransactionClosing(object sender, EventArgs args)
        {
            ManageTimerStatesInCriticalData(true);
        }

        /// <summary>
        /// Stops all timers and removes them from the internal collection, but does
        /// not remove them from persistent data.
        /// </summary>
        private void RemoveAll()
        {
            lock(timerTickEventLocker)
            {
                var timerList = new List<GameTimer>(timerDictionary.Values.ToList());
                foreach(var timer in timerList)
                {
                    RemoveTimer(timer.Name, false);
                }
            }
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            isDisposed = true;
            Initialized = false;

            RemoveAll();
            (eventPosted as IDisposable).Dispose();

            if(heartBeat != null)
            {
                heartBeat.Enabled = false;
                heartBeat.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region IGameLibEventListener Implementation

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib gameLib)
        {
            eventCoordindator.UnregisterEventSource(this);
            transactionAugmenter.TransactionClosingEvent -= OnTransactionClosing;
        }

        #endregion
    }
}
