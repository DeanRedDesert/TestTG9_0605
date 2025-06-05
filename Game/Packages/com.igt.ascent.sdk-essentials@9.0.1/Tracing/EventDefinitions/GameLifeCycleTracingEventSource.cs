// -----------------------------------------------------------------------
// <copyright file = "GameLifeCycleTracingEventSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing.EventDefinitions
{
    using System.Diagnostics.Tracing;

    [EventSource(Name = "IGT-Ascent-Core-Tracing-GameLifeCycleTracingEventSource")]
    internal sealed class GameLifeCycleTracingEventSource : EventSourceBase
    {
        #region Constants

        private const EventLevel DefaultLevel = EventLevel.Informational;

        #endregion

        #region Singleton

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameLifeCycleTracingEventSource Log { get; } = new GameLifeCycleTracingEventSource();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GameLifeCycleTracingEventSource()
        {
        }

        #endregion

        #region Event Definitions

        [Event(1, Level = DefaultLevel)]
        public void GameEntryAwake(int delayMilliseconds)
        {
            WriteEvent(1, delayMilliseconds);
        }

        [Event(2, Level = DefaultLevel)]
        public void GameLibConnected(bool success)
        {
            WriteEventBool(2, success);
        }

        [Event(3, Level = DefaultLevel)]
        public void CabinetLibConnected(bool success)
        {
            WriteEventBool(3, success);
        }

        [Event(4, Level = DefaultLevel)]
        public void ActivateThemeContext(string contextDescription)
        {
            WriteEvent(4, contextDescription);
        }

        [Event(5, Level = DefaultLevel)]
        public void StateMachineThreadStarted(string contextDescription)
        {
            WriteEvent(5, contextDescription);
        }

        /// <devdoc>
        /// Use Verbose level for easier enabling/disabling the Update tracing.
        /// </devdoc>
        [Event(6, Level = EventLevel.Verbose)]
        public void GameEntryUpdate()
        {
            WriteEvent(6);
        }

        [Event(7, Level = DefaultLevel)]
        public void FirstPresentationStateEntered(string stateName)
        {
            WriteEvent(7, stateName);
        }

        [Event(8, Level = DefaultLevel)]
        public void SendReadyState(bool isReady)
        {
            WriteEventBool(8, isReady);
        }

        [Event(9, Level = DefaultLevel)]
        public void WindowSizeRequest(int windowId, bool show)
        {
            WriteEventIntBool(9, windowId, show);
        }

        [Event(10, Level = DefaultLevel)]
        public void PresentationRenderingStarted()
        {
            WriteEvent(10);
        }

        [Event(11, Level = DefaultLevel)]
        public void WindowSizeResponse(int windowId)
        {
            WriteEvent(11, windowId);
        }

        [Event(12, Level = DefaultLevel)]
        public void PresentationDisplayControlState(string displayControlState)
        {
            WriteEvent(12, displayControlState);
        }

        [Event(13, Level = DefaultLevel)]
        public void InactivateThemeContext()
        {
            WriteEvent(13);
        }

        [Event(14, Level = DefaultLevel)]
        public void StateMachineEnded(string message)
        {
            WriteEvent(14, message);
        }

        [Event(15, Level = DefaultLevel)]
        public void ParkReceived()
        {
            WriteEvent(15);
        }

        [Event(16, Level = DefaultLevel)]
        public void ParkProcessed()
        {
            WriteEvent(16);
        }

        [Event(17, Level = DefaultLevel)]
        public void GameApplicationPark()
        {
            WriteEvent(17);
        }

        [Event(18, Level = DefaultLevel)]
        public void GameApplicationUnpark()
        {
            WriteEvent(18);
        }

        [Event(19, Level = DefaultLevel)]
        public void UnparkProcessed()
        {
            WriteEvent(19);
        }

        [Event(20, Level = DefaultLevel)]
        public void ShutDownReceived()
        {
            WriteEvent(20);
        }

        [Event(21, Level = DefaultLevel)]
        public void GameLogicThreadTerminated()
        {
            WriteEvent(21);
        }

        [Event(22, Level = DefaultLevel)]
        public void ApplicationQuit(int exitCode)
        {
            WriteEvent(22, exitCode);
        }

        [Event(23, Level = DefaultLevel)]
        public void AbortProcessed(string reason)
        {
            WriteEvent(23, reason);
        }

        [Event(24, Level = DefaultLevel)]
        public void ApplicationQuitComplete()
        {
            WriteEvent(24);
        }

        [Event(25, Level = DefaultLevel)]
        public void PresentationReadyForFirstOnEnter(int frameCounter)
        {
            WriteEvent(25, frameCounter);
        }

        [Event(26, Level = DefaultLevel)]
        public void LogicRequestsFirstOnEnter(int frameCounter)
        {
            WriteEvent(26, frameCounter);
        }

        #endregion
    }
}