// -----------------------------------------------------------------------
// <copyright file = "DebugTracingEventSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing.EventDefinitions
{
    using System.Diagnostics.Tracing;

    [EventSource(Name = "IGT-Ascent-Core-Tracing-DebugTracingEventSource")]
    internal sealed class DebugTracingEventSource : EventSourceBase
    {
        #region Constants

        private const EventLevel DefaultLevel = EventLevel.Informational;

        #endregion

        #region Singleton

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DebugTracingEventSource Log { get; } = new DebugTracingEventSource();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private DebugTracingEventSource()
        {
        }

        #endregion

        #region Event Definitions

        [Event(1, Level = DefaultLevel)]
        public void Event(CallSite callSite, string eventName)
        {
            WriteEvent(1, (int)callSite, eventName);
        }

        [Event(2, Level = DefaultLevel)]
        public void TimerStart(CallSite callSite, string timerName)
        {
            WriteEvent(2, (int)callSite, timerName);
        }

        [Event(3, Level = DefaultLevel)]
        public void TimerStop(CallSite callSite, string timerName)
        {
            WriteEvent(3, (int)callSite, timerName);
        }

        #endregion
    }
}