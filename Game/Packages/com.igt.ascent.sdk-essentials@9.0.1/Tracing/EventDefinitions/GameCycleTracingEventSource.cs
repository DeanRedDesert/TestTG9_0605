// -----------------------------------------------------------------------
// <copyright file = "GameCycleTracingEventSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing.EventDefinitions
{
    using System.Diagnostics.Tracing;

    [EventSource(Name = "IGT-Ascent-Core-Tracing-GameCycleTracingEventSource")]
    internal sealed class GameCycleTracingEventSource : EventSourceBase
    {
        #region Constants

        private const EventLevel DefaultLevel = EventLevel.Informational;

        #endregion

        #region Singleton

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameCycleTracingEventSource Log { get; } = new GameCycleTracingEventSource();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GameCycleTracingEventSource()
        {
        }

        #endregion

        #region Event Definitions

        [Event(1, Level = DefaultLevel)]
        public void BetButtonReady(CallSite callSite)
        {
            WriteEvent(1, (int)callSite);
        }

        [Event(2, Level = DefaultLevel)]
        public void BetButtonTriggered(CallSite callSite, long buttonTriggerTimeStamp)
        {
            WriteEventIntLong(2, (int)callSite, buttonTriggerTimeStamp);
        }

        [Event(3, Level = DefaultLevel)]
        public void ReelSpinStart(CallSite callSite)
        {
            WriteEvent(3, (int)callSite);
        }

        [Event(4, Level = DefaultLevel)]
        public void ReelSpinStop(CallSite callSite)
        {
            WriteEvent(4, (int)callSite);
        }

        [Event(5, Level = DefaultLevel)]
        public void SlamButtonTriggered(CallSite callSite, long buttonTriggerTimeStamp)
        {
            WriteEventIntLong(5, (int)callSite, buttonTriggerTimeStamp);
        }

        [Event(6, Level = DefaultLevel)]
        public void WinCelebrationStart(CallSite callSite)
        {
            WriteEvent(6, (int)callSite);
        }

        [Event(7, Level = DefaultLevel)]
        public void WinCelebrationStop(CallSite callSite)
        {
            WriteEvent(7, (int)callSite);
        }

        [Event(8, Level = DefaultLevel)]
        public void BonusTriggered(CallSite callSite)
        {
            WriteEvent(8, (int)callSite);
        }

        [Event(9, Level = DefaultLevel)]
        public void BonusComplete(CallSite callSite)
        {
            WriteEvent(9, (int)callSite);
        }

        #endregion
    }
}