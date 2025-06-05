// -----------------------------------------------------------------------
// <copyright file = "DiagnosticTracing.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing
{
    using System.Diagnostics;
    using EventDefinitions;

    /// <summary>
    /// A general event trace used in the Debug build and the Release build for testing.
    /// </summary>
    public sealed class DiagnosticTracing
    {
        #region Singleton Implementation

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DiagnosticTracing Log { get; } = new DiagnosticTracing();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private DiagnosticTracing()
        {
        }

        #endregion
        
        #region Tracing Methods

        /// <summary>
        /// Tracing event indicating the occurrence of a generic named event.
        /// </summary>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        /// <param name="eventName">Name of the event.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void Event(CallSite callSite, string eventName)
        {
            DiagnosticTracingEventSource.Log.Event(callSite, eventName);
        }

        /// <summary>
        /// Tracing event indicating the start of a generic named timer.
        /// </summary>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        /// <param name="timerName">Name of the timer.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void TimerStart(CallSite callSite, string timerName)
        {
            DiagnosticTracingEventSource.Log.TimerStart(callSite, timerName);
        }

        /// <summary>
        /// Tracing event indicating the stop of a generic named timer.
        /// </summary>
        /// <param name="callSite">The call site emitting the tracing event.</param>
        /// <param name="timerName">Name of the timer.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void TimerStop(CallSite callSite, string timerName)
        {
            DiagnosticTracingEventSource.Log.TimerStop(callSite, timerName);
        }

        #endregion
    }
}