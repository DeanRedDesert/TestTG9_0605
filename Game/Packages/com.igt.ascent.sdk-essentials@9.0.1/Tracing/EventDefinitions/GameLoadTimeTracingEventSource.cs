// -----------------------------------------------------------------------
// <copyright file = "GameLoadTimeTracingEventSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing.EventDefinitions
{
    using System.Diagnostics.Tracing;

    [EventSource(Name = "IGT-Ascent-Core-Tracing-GameLoadTimeTracingEventSource")]
    internal sealed class GameLoadTimeTracingEventSource : EventSourceBase
    {
        #region Constants

        private const EventLevel DefaultLevel = EventLevel.Informational;

        #endregion

        #region Singleton

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameLoadTimeTracingEventSource Log { get; } = new GameLoadTimeTracingEventSource();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GameLoadTimeTracingEventSource()
        {
        }

        #endregion

        #region Event Definitions

        [Event(1, Level = DefaultLevel)]
        public void GameApplicationStarted(int delayMilliseconds)
        {
            WriteEvent(1, delayMilliseconds);
        }

        [Event(2, Level = DefaultLevel)]
        public void GameReadyForPlay()
        {
            WriteEvent(2);
        }

        [Event(3, Level = DefaultLevel)]
        public void SceneLoaded(string sceneName)
        {
            WriteEvent(3, sceneName);
        }

        [Event(4, Level = DefaultLevel)]
        public void GameLibConnectionStart()
        {
            WriteEvent(4);
        }

        [Event(5, Level = DefaultLevel)]
        public void ConnectToFoundation()
        {
            WriteEvent(5);
        }

        [Event(6, Level = DefaultLevel)]
        public void CabinetConnectionStart()
        {
            WriteEvent(6);
        }

        [Event(7, Level = DefaultLevel)]
        public void CreateCabinetLib()
        {
            WriteEvent(7);
        }

        [Event(8, Level = DefaultLevel)]
        public void CreateCabinet()
        {
            WriteEvent(8);
        }

        [Event(9, Level = DefaultLevel)]
        public void CabinetAsyncStart()
        {
            WriteEvent(9);
        }

        [Event(10, Level = DefaultLevel)]
        public void GameLogicStart()
        {
            WriteEvent(10);
        }

        [Event(11, Level = DefaultLevel)]
        public void LogicStartPresentation()
        {
            WriteEvent(11);
        }

        [Event(12, Level = DefaultLevel)]
        public void RefreshStateHandlersStart()
        {
            WriteEvent(12);
        }

        [Event(13, Level = DefaultLevel)]
        public void RefreshStateHandlersComplete()
        {
            WriteEvent(13);
        }

        #endregion
    }
}