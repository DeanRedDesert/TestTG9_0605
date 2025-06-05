// -----------------------------------------------------------------------
// <copyright file = "DiagnosticTracingEventSource.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing.EventDefinitions
{
    using System.Diagnostics.Tracing;

    /// <summary>
    /// An etw event provider to emit events with general string payloads.
    /// This provider is designed to be used in the Debug build and the Release build for testing.
    /// </summary>
    [EventSource(Name = "IGT-Ascent-Core-Tracing-DiagnosticTracingEventSource")]
    internal sealed class DiagnosticTracingEventSource : EventSourceBase
    {
        #region Constants

        private const EventLevel DefaultLevel = EventLevel.Informational;

        #endregion

        #region Singleton

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DiagnosticTracingEventSource Log { get; } = new DiagnosticTracingEventSource();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private DiagnosticTracingEventSource()
        {
        }

        #endregion
        
        #region Event Definitions

        /// <summary>
        /// Emits a named event.
        /// </summary>
        /// <param name="callSite">The priority level of this emitting call.</param>
        /// <param name="eventName">The name of the event.</param>
        [Event(1, Level = DefaultLevel)]
        public void Event(CallSite callSite, string eventName)
        {
            WriteEvent(1, (int)callSite, eventName);
        }

        /// <summary>
        /// Emits a named timer start event.
        /// </summary>
        /// <param name="callSite">The priority level of this emitting call.</param>
        /// <param name="timerName">The name of the timer.</param>
        [Event(2, Level = DefaultLevel)]
        public void TimerStart(CallSite callSite, string timerName)
        {
            WriteEvent(2, (int)callSite, timerName);
        }

        /// <summary>
        /// Emits a named timer stop event.
        /// </summary>
        /// <param name="callSite">The priority level of this emitting call.</param>
        /// <param name="timerName">The name of the timer.</param>
        [Event(3, Level = DefaultLevel)]
        public void TimerStop(CallSite callSite, string timerName)
        {
            WriteEvent(3, (int)callSite, timerName);
        }

        #endregion
    }
}