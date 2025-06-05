//-----------------------------------------------------------------------
// <copyright file = "GameTimer.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class that defines a single timer object. This class encapsulates methods and actions that are manipulated
    /// by a single underlying OS timer to generate tick events and persistence actions.
    /// </summary>
    internal class GameTimer : IGameTimer, IDisposable
    {
        #region nested region

        /// <summary>
        /// Enums defining whether to count up or down.
        /// </summary>
        private enum GameTimerCountType
        {
            /// <summary>
            /// Descending count.
            /// </summary>
            CountDown,

            /// <summary>
            /// Ascending count.
            /// </summary>
            CountUp
        }

        /// <summary>
        /// Type of persistence action to take when needed.
        /// </summary>
        internal enum PersistenceActionType
        {
            /// <summary>
            /// Default - no critical data access needed.
            /// </summary>
            DoNothing,

            /// <summary>
            /// Write/overwrite a timer state.
            /// </summary>
            Save,

            /// <summary>
            /// Remove a timer state.
            /// </summary>
            Remove,
        }

        #endregion

        #region Private members

        /// <summary>
        /// Private event handler backing store for <see cref="GameTimerConsumerEvent"/>.
        /// </summary>
        private EventHandler<GameTimerEventArgs> gameTimerConsumerEventField;

        /// <summary>
        /// The event queue of tick events to be raised.
        /// </summary>
        private Queue<GameTimerEventArgs> timerTickEventQueue = new Queue<GameTimerEventArgs>();

        /// <summary>
        /// The current internal incrementing timer tick value.
        /// </summary>
        private uint currentTimerTick;

        /// <summary>
        /// The current persistence value.
        /// </summary>
        private uint currentPersistenceTick;

        /// <summary>
        /// The <see cref="GameTimerStatus"/> that describes the current status of this timer.
        /// </summary>
        private volatile GameTimerStatus gameTimerStatus;

        /// <summary>
        /// The <see cref="GameTimerAction"/> that describes the pending action to take on this timer.
        /// </summary>
        private volatile GameTimerAction gameTimerAction;

        /// <summary>
        /// The <see cref="GameTimerCountType"/> that sets the timer counting type.
        /// </summary>
        private GameTimerCountType countType;

        /// <summary>
        /// A flag indicating if this timer should be persisted in critical data.
        /// </summary>
        private readonly bool persist;

        /// <summary>
        /// Flag indicating the game timer is currently disposing, or has been disposed.
        /// </summary>
        private volatile bool isDisposed;

        /// <summary>
        /// Object to lock<see cref="GameTimerStatus"/> set access.
        /// </summary>
        private readonly object gameTimerStatusLocker = new object();

        /// <summary>
        /// Object to lock<see cref="GameTimerAction"/> set access.
        /// </summary>
        private readonly object gameTimerActionLocker = new object();

        /// <summary>
        /// Backing field for MarkedForDelayedDelete property.
        /// </summary>
        private bool markedForDelayedDelete;

        #endregion

        #region IGameTimer implementation

        /// <inheritdoc />
        public event EventHandler<GameTimerEventArgs> GameTimerConsumerEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, gameTimerConsumerEventField))
                {
                    gameTimerConsumerEventField += value;
                }
            }
            // ReSharper disable once DelegateSubtraction
            remove => gameTimerConsumerEventField -= value;
        }

        /// <inheritdoc />
        public string Name
        {
            get;
        }

        /// <inheritdoc />
        public uint TimerTickInterval
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public uint CurrentValue
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public GameTimerStatus GameTimerStatus
        {
            get => gameTimerStatus;
            private set
            {
                lock(gameTimerStatusLocker)
                {
                    gameTimerStatus = value;
                }
            }
        }

        /// <inheritdoc />
        public bool IsActive => gameTimerStatus == GameTimerStatus.Ticking;

        /// <inheritdoc />
        public void Start()
        {
            GameTimerAction = GameTimerAction.Start;
            EnqueueGameTimerStatusEvent(GameTimerStatus.Started);

            // Notify the controller and any other subscribers that we have started.
            GameTimerStatusChangedEvent?.Invoke(this, new GameTimerEventArgs(Name, CurrentValue, GameTimerStatus.Started));
        }

        /// <inheritdoc />
        public void Reset()
        {
            CurrentValue = BaseValue;
            GameTimerAction = GameTimerAction.Reset;
        }

        /// <inheritdoc />
        public void Stop()
        {
            GameTimerAction = GameTimerAction.Stop;
        }

        /// <inheritdoc />
        public void SetOrResetValues(uint baseValue, uint targetValue)
        {
            // Will throw if there are invalid values.
            ValidateTimerValues(baseValue, targetValue, TimerTickInterval);

            BaseValue = baseValue;
            TargetValue = targetValue;
            CurrentValue = baseValue;
            GameTimerAction = GameTimerAction.Stop;
            SetCountType();
        }

        #endregion

        #region Internal and Private Properties and Events

        /// <summary>
        /// The start value of tick counts, used to count down to or up to.
        /// </summary>
        internal uint BaseValue
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of tick counts that this timer will count up/down to.
        /// </summary>
        internal uint TargetValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/private sets the persistence interval.
        /// </summary>
        internal uint PersistenceInterval
        {
            get;
            private set;
        }

        /// <summary>
        /// The <see cref="PersistenceActionType"/> that this timer needs to have performed on it.
        /// </summary>
        internal PersistenceActionType PersistenceAction
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a flag indicating that this timer has any events that need to be dequeued and processed.
        /// </summary>
        internal bool AnyEventsPending => timerTickEventQueue.Count > 0;

        /// <summary>
        /// The specific path in critical data to read/write this timer to/from.
        /// </summary>
        internal string CriticalDataPath
        {
            get;
        }

        /// <summary>
        /// Gets/sets whether this timer should be marked for safe storage and controlling collection deletion at the appropriate time.
        /// </summary>
        internal bool MarkedForDelayedDelete
        {
            get => markedForDelayedDelete;
            set
            {
                lock(gameTimerStatusLocker)
                {
                    markedForDelayedDelete = value;
                    if(markedForDelayedDelete)
                    {
                        FlagForSafeStorageRemoval();
                    }
                }
            }
        }

        /// <summary>
        /// Gets/sets the <see cref="GameTimerAction"/> to be performed on the timer.
        /// </summary>
        private GameTimerAction GameTimerAction
        {
            get => gameTimerAction;
            set
            {
                lock(gameTimerActionLocker)
                {
                    gameTimerAction = value;
                }
            }
        }

        /// <summary>
        /// Internal event raised to notify subscribers that a timer status event has occurred.
        /// </summary>
        internal event EventHandler<GameTimerEventArgs> GameTimerStatusChangedEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor used to create a game timer object with the supplied parameters.
        /// </summary>
        /// <param name="name">The timer name.</param>
        /// <param name="baseValue">The starting value.</param>
        /// <param name="targetValue">The ending value.</param>
        /// <param name="tickInterval">The interval in heart beat ticks that this timer should process its internal state at.</param>
        /// <param name="persistenceInterval">The interval in heart beat ticks that this timer's state should be persisted to critical data at.</param>
        /// <param name="criticalDataPath">The timer state's critical data path.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <see paramref="name"/> or <see paramref="criticalDataPath"/> parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the numeric timer count or tick values are invalid. See <seealso cref="ValidateTimerValues"/>.</exception>
        public GameTimer(string name, uint baseValue, uint targetValue, uint tickInterval, uint persistenceInterval, string criticalDataPath)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if(string.IsNullOrEmpty(criticalDataPath))
            {
                throw new ArgumentNullException(nameof(criticalDataPath));
            }

            // Will throw if there are invalid values.
            ValidateTimerValues(baseValue, targetValue, tickInterval);

            BaseValue = baseValue;
            CriticalDataPath = criticalDataPath;
            currentTimerTick = 0;
            CurrentValue = baseValue;
            GameTimerStatus = GameTimerStatus.Idling;
            GameTimerAction = GameTimerAction.DoNothing;
            Name = name;
            persist = persistenceInterval > 0;
            PersistenceInterval = persistenceInterval;
            PersistenceAction = PersistenceActionType.DoNothing;
            TargetValue = targetValue;
            TimerTickInterval = tickInterval;
            SetCountType();
        }

        #endregion

        /// <summary>
        /// Restore values from persistent data.
        /// </summary>
        /// <param name="gameTimerStorage">The <see cref="GameTimerStorage"/> object read from critical data.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <see paramref="gameTimerStorage"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the numeric timer count or tick values are invalid. See <seealso cref="ValidateTimerValues"/>.</exception>
        internal void RestoreValues(GameTimerStorage gameTimerStorage)
        {
            if(gameTimerStorage == null)
            {
                throw new ArgumentNullException(nameof(gameTimerStorage));
            }

            // Will throw if there are invalid values.
            ValidateTimerValues(gameTimerStorage.BaseValue,  gameTimerStorage.TargetValue, gameTimerStorage.TimerTickInterval);

            BaseValue = gameTimerStorage.BaseValue;
            CurrentValue = gameTimerStorage.CurrentValue;
            PersistenceInterval = gameTimerStorage.PersistenceInterval;
            TargetValue = gameTimerStorage.TargetValue;
            TimerTickInterval = gameTimerStorage.TimerTickInterval;
            SetCountType();
        }

        /// <summary>
        /// Sets the <see cref="GameTimerCountType"/> that this timer will behave as.
        /// </summary>
        private void SetCountType()
        {
            countType = TargetValue <= BaseValue ? GameTimerCountType.CountDown
                                                 : GameTimerCountType.CountUp;
        }

        /// <summary>
        /// Enqueue a game timer status event.
        /// </summary>
        /// <param name="status">The <see cref="GameTimerStatus"/> event to enqueue.</param>
        private void EnqueueGameTimerStatusEvent(GameTimerStatus status)
        {
            lock(gameTimerStatusLocker)
            {
                timerTickEventQueue.Enqueue(new GameTimerEventArgs(Name, CurrentValue, status));
            }
        }

        /// <summary>
        /// Raises any and all queued tick events to any external subscribers to the <see cref="GameTimerConsumerEvent"/>.
        /// </summary>
        internal void RaiseAllQueuedEvents()
        {
            if(gameTimerConsumerEventField != null && !isDisposed)
            {
                Queue<GameTimerEventArgs> timerTickEventQueueCopy;

                lock(gameTimerStatusLocker)
                {
                    timerTickEventQueueCopy = timerTickEventQueue;
                    timerTickEventQueue = new Queue<GameTimerEventArgs>();
                }

                while(timerTickEventQueueCopy.Count > 0)
                {
                    var timerEventToRaise = timerTickEventQueueCopy.Dequeue();
                    if(timerEventToRaise != null)
                    {
                        gameTimerConsumerEventField(this, timerEventToRaise);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if an event handler delegate is already in an event handler's list.
        /// </summary>
        /// <param name="prospectiveHandler">The <see cref="Delegate"/> that is going to be added.</param>
        /// <param name="handler">The <see cref="EventHandler"/> where T is based on <see cref="EventArgs"/></param>.
        /// <returns>Flag indicating if this event is already handled by the prospective handler.</returns>
        private static bool IsEventHandlerAlreadySubscribed<TEventArgs>(Delegate prospectiveHandler, EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            if(handler != null)
            {
                var invocationList = handler.GetInvocationList();

                if(invocationList.Contains(prospectiveHandler))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if any persistence action should be taken on this timer.
        /// </summary>
        internal void ResetPersistenceStatus()
        {
            currentPersistenceTick = 0;
            PersistenceAction = PersistenceActionType.DoNothing;
        }

        /// <summary>
        /// Flag this timer to be removed from safe storage.
        /// </summary>
        private void FlagForSafeStorageRemoval()
        {
            PersistenceAction = PersistenceActionType.Remove;
        }

        /// <summary>
        /// Gets whether the timer has finished counting up/down.
        /// </summary>
        internal bool TargetReached => CurrentValue == TargetValue;

        /// <summary>
        /// Determines the persistence action to take with this timer state.
        /// </summary>
        private void UpdatePersistenceStatus()
        {
            switch(PersistenceAction)
            {
                case PersistenceActionType.Save:
                case PersistenceActionType.Remove:
                    break;
                case PersistenceActionType.DoNothing:
                    PersistenceAction = ++currentPersistenceTick >= PersistenceInterval ?
                        PersistenceActionType.Save:
                        PersistenceAction;
                    break;
            }
        }

        /// <summary>
        /// Process a single timer. Update applicable values, set next status/state, and check if a tick event
        /// needs to be raised and/or if the timer's state needs to be persisted.
        /// </summary>
        internal void OnHeartBeat()
        {
            lock(gameTimerStatusLocker)
            {
                switch(GameTimerAction)
                {
                    case GameTimerAction.Reset:
                    case GameTimerAction.Stop:
                        GameTimerStatus = GameTimerStatus.Idling;
                        GameTimerAction = GameTimerAction.DoNothing;
                        break;
                    case GameTimerAction.Start:
                        GameTimerStatus = GameTimerStatus.Ticking;
                        GameTimerAction = GameTimerAction.DoNothing;
                        break;
                }

                // Update as needed if timer is ticking.
                if(GameTimerStatus == GameTimerStatus.Ticking)
                {
                    // Determine whether to raise ticks.
                    if(++currentTimerTick == TimerTickInterval)
                    {
                        currentTimerTick = 0;

                        switch(countType)
                        {
                            case GameTimerCountType.CountDown:
                                if(CurrentValue > uint.MinValue)
                                {
                                    CurrentValue--;
                                }
                                break;

                            case GameTimerCountType.CountUp:
                                if(CurrentValue < uint.MaxValue)
                                {
                                    CurrentValue++;
                                }
                                break;
                        }

                        // Create and queue a tick event.
                        EnqueueGameTimerStatusEvent(GameTimerStatus.Ticking);

                        // If the timer has completed, stop it and remove from safe storage.
                        if(TargetReached)
                        {
                            Stop();
                            EnqueueGameTimerStatusEvent(GameTimerStatus.Stopped);
                            FlagForSafeStorageRemoval();
                        }
                    }

                    // Update safe storage status.
                    if(persist)
                    {
                        UpdatePersistenceStatus();
                    }
                }
            }
        }

        /// <summary>
        /// Validates certain timer values to ensure they make sense.
        /// </summary>
        /// <param name="baseValue">See <seealso cref="GameTimer"/> parameter definitions.</param>
        /// <param name="targetValue">See <seealso cref="GameTimer"/> parameter definitions.</param>
        /// <param name="tickInterval">See <seealso cref="GameTimer"/> parameter definitions.</param>
        /// <exception cref="ArgumentException">Thrown if any of the passed in parameter values are invalid.</exception>
        private void ValidateTimerValues(uint baseValue, uint targetValue, uint tickInterval)
        {
            if(baseValue == targetValue || tickInterval == 0)
            {
                throw new ArgumentException($@"Ensure that 'baseValue' ({baseValue}) and 'targetValue' ({targetValue}) are not identical,
                                                     and that 'tickinterval' ({tickInterval}) is greater than 0.");
            }
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            if(isDisposed)
            {
                return;
            }

            isDisposed = true;

            lock(gameTimerStatusLocker)
            {
                gameTimerConsumerEventField = null;

                timerTickEventQueue?.Clear();
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
