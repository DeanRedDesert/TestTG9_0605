//-----------------------------------------------------------------------
// <copyright file = "GameTimerEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    using System;
    using System.Text;

    /// <summary>
    /// Event arguments for a game timer tick event.
    /// </summary>
    [Serializable]
    public class GameTimerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets/private Sets the unique timer name.
        /// </summary>
        public string TimerName
        {
            get; 
            private set;
        }

        /// <summary>
        /// The <see cref="GameTimerStatus"/> describing the action this timer event is tied to (start, tick, stop, etc.)
        /// </summary>
        public GameTimerStatus TimerStatus
        {
            get;
            private set;
        }

        /// <summary>
        /// The current value of the timer; increments or decrements.
        /// </summary>
        public uint CurrentValue
        {
            get;
            set;
        }

        /// <summary>
        /// Return a HH:MM:SS formatted string of the current value.
        /// </summary>
        public string CurrentValueAsTimeString
        {
            get 
            { 
                var tickSpan = TimeSpan.FromSeconds(CurrentValue);
                return tickSpan.ToString(); 
            }
        }

        /// <summary>
        /// Return an MM:SS formatted string of the current value's minutes and seconds. Note: this
        /// method will truncate the hours value.
        /// </summary>
        public string CurrentValueAsMinutesSecondsTimeString
        {
            get
            {
                var tickSpanString = TimeSpan.FromSeconds(CurrentValue).ToString();
                return tickSpanString.Substring(tickSpanString.Length > 5 ? 
                    tickSpanString.Length - 5:
                    tickSpanString.Length);
            }
        }

        /// <summary>
        /// Constructs a <see cref="GameTimerEventArgs"/> with default values.
        /// </summary>
        public GameTimerEventArgs() : this("<UnknownTimer>", 0, GameTimerStatus.Idling)
        {
        }

        /// <summary>
        /// Copy constructer.
        /// </summary>
        /// <param name="existingArgs">An existing <see cref="GameTimerEventArgs"/> object to be used during
        /// construction.</param>
        /// <exception cref="ArgumentNullException ">Thrown if <see paramref="existingArgs"/> is null.</exception>
        internal GameTimerEventArgs(GameTimerEventArgs existingArgs) : this()
        {
            if(existingArgs == null)
            {
                throw new ArgumentNullException("existingArgs");
            }

            TimerName = existingArgs.TimerName;
            CurrentValue = existingArgs.CurrentValue;
            TimerStatus = existingArgs.TimerStatus;
        }

        /// <summary>
        /// Constructs a <see cref="GameTimerEventArgs"/> with the specified values.
        /// </summary>
        /// <param name="timerName">Unique timer name.</param>
        /// <param name="currentValue">Current value.</param>
        /// <param name="timerStatus">The <see cref="GameTimerStatus"/> describing the current action 
        /// (start, tick, stop, etc.)</param>
        internal GameTimerEventArgs(string timerName, uint currentValue, GameTimerStatus timerStatus)
        {
            TimerName = timerName;
            CurrentValue = currentValue;
            TimerStatus = timerStatus;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("Timer Name: ");
            builder.Append(TimerName);
            builder.Append(" Current Value: ");
            builder.Append(CurrentValue);
            builder.Append(" Timer Action: ");
            builder.Append(TimerStatus);

            return builder.ToString();
        }
    }
}
