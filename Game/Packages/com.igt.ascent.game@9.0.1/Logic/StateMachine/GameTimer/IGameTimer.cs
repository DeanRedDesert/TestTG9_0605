//-----------------------------------------------------------------------
// <copyright file = "IGameTimer.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    using System;

    /// <summary>
    /// An interface to a game timer, which provides methods to manage the actions of the timer, properties describing the timer,
    /// and the game timer status and event handler which a game can subscribe to.
    /// </summary>
    public interface IGameTimer
    {
        #region events

        /// <summary>
        /// The event handler that the game subscribes to. Events raised through this handler will be raised on the logic thread.
        /// </summary>
        event EventHandler<GameTimerEventArgs> GameTimerConsumerEvent;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the timer.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The timer tick interval in seconds.
        /// </summary>
        uint TimerTickInterval { get; }

        /// <summary>
        /// The current value, which should be a discrete increasing or decreasing value ranging from the initial start value 
        /// to the destination target value.
        /// </summary>
        uint CurrentValue { get; }

        /// <summary>
        /// Flag indicating the current status of this timer.
        /// </summary>
        GameTimerStatus GameTimerStatus
        {
            get;
        }

        /// <summary>
        /// Flag indicating if the timer is in an active state, which indicates it is being started, stopped, or is ticking.
        /// Timers that are stopped revert to 'Idling' on the next tick.
        /// </summary>
        bool IsActive
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts or restarts a timer if paused.
        /// </summary>
        void Start();

        /// <summary>
        /// Resets the timer to its original values; does not restart the timer.
        /// </summary>
        void Reset();

        /// <summary>
        /// Stops a timer - timer stays in critical data if persistence is specified.
        /// </summary>
        void Stop();

        /// <summary>
        /// Sets or resets the base and target values of a timer. If a timer is currently ticking then it will be
        /// stopped first and will need to be restarted.
        /// </summary>
        /// <param name="baseValue">The number of tick counts that this timer will count up/down from.</param>
        /// <param name="targetValue">The number of tick counts that this timer will count up/down to.</param>
        void SetOrResetValues(uint baseValue, uint targetValue);

        #endregion
    }
}
