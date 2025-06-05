// -----------------------------------------------------------------------
// <copyright file = "DebugTracing.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing
{
    using System.Diagnostics;
    using EventDefinitions;

    /// <summary>
    /// Generic development tracing.
    /// </summary>
    public sealed class DebugTracing
    {
        #region Singleton Implementation

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DebugTracing Log { get; } = new DebugTracing();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private DebugTracing()
        {
        }

        #endregion

        #region Tracing Methods

        /// <summary>
        /// Tracing event indicating the occurrence of a generic debug event.
        /// </summary>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        /// <param name="eventName">Name of the event.</param>
        [Conditional("DEBUG_TRACING")]
        public void Event(CallSite callSite, string eventName)
        {
            DebugTracingEventSource.Log.Event(callSite, eventName);
        }

        /// <summary>
        /// Tracing event indicating the start of a generic timer.
        /// </summary>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        /// <param name="timerName">Name of the timer.</param>
        [Conditional("DEBUG_TRACING")]
        public void TimerStart(CallSite callSite, string timerName)
        {
            DebugTracingEventSource.Log.TimerStart(callSite, timerName);
        }

        /// <summary>
        /// Tracing event indicating the stop of a generic timer.
        /// </summary>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        /// <param name="timerName">Name of the timer.</param>
        [Conditional("DEBUG_TRACING")]
        public void TimerStop(CallSite callSite, string timerName)
        {
            DebugTracingEventSource.Log.TimerStop(callSite, timerName);
        }

        #endregion
    }
}