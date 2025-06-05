// -----------------------------------------------------------------------
// <copyright file = "GameLifeCycleTracing.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing
{
    using System.Diagnostics;
    using EventDefinitions;

    /// <summary>
    /// This class provides APIs for tracing a game life cycle.
    /// </summary>
    public sealed class GameLifeCycleTracing
    {
        #region Singleton Implementation

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameLifeCycleTracing Log { get; } = new GameLifeCycleTracing();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GameLifeCycleTracing()
        {
        }

        #endregion

        #region Tracing Methods

        /// <summary>
        /// Tracing event indicating that the Game Entry Script is awaken.
        /// </summary>
        /// <param name="delayMilliseconds">
        /// The time in milliseconds since the application started till this method is called.
        /// </param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameEntryAwake(int delayMilliseconds)
        {
            GameLifeCycleTracingEventSource.Log.GameEntryAwake(delayMilliseconds);
        }

        /// <summary>
        /// Tracing event indicating that game lib connection is established.
        /// </summary>
        /// <param name="success">Flag indicating if the connection is success.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameLibConnected(bool success)
        {
            GameLifeCycleTracingEventSource.Log.GameLibConnected(success);
        }

        /// <summary>
        /// Tracing event indicating that cabinet lib connection is established.
        /// </summary>
        /// <param name="success">Flag indicating if the connection is success.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void CabinetLibConnected(bool success)
        {
            GameLifeCycleTracingEventSource.Log.CabinetLibConnected(success);
        }

        /// <summary>
        /// Tracing event indicating that a theme context is activated.
        /// </summary>
        /// <param name="contextDescription">The description of the theme context.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ActivateThemeContext(string contextDescription)
        {
            GameLifeCycleTracingEventSource.Log.ActivateThemeContext(contextDescription);
        }

        /// <summary>
        /// Tracing event indicating that a a state machine thread is spawned and run.
        /// </summary>
        /// <param name="contextDescription">The description of the state machine context.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateMachineThreadStarted(string contextDescription)
        {
            GameLifeCycleTracingEventSource.Log.StateMachineThreadStarted(contextDescription);
        }

        /// <summary>
        /// Tracing event indicating a script update call in the Unity play loop.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameEntryUpdate()
        {
            GameLifeCycleTracingEventSource.Log.GameEntryUpdate();
        }

        /// <summary>
        /// Tracing event indicating that the first presentation state is entered,
        /// and its service consumer data has been set.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void FirstPresentationStateEntered(string stateName)
        {
            GameLifeCycleTracingEventSource.Log.FirstPresentationStateEntered(stateName);
        }

        /// <summary>
        /// Tracing event indicating that the presentation is ready for entering the first presentation state.
        /// </summary>
        /// <param name="frameCounter">The frame counter value when this event is emitted.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void PresentationReadyForFirstOnEnter(int frameCounter)
        {
            GameLifeCycleTracingEventSource.Log.PresentationReadyForFirstOnEnter(frameCounter);
        }

        /// <summary>
        /// Tracing event indicating that the logic has requested to enter the first presentation state.
        /// </summary>
        /// <param name="frameCounter">The frame counter value when this event is emitted.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void LogicRequestsFirstOnEnter(int frameCounter)
        {
            GameLifeCycleTracingEventSource.Log.LogicRequestsFirstOnEnter(frameCounter);
        }

        /// <summary>
        /// Tracing event indicating that the game sends ready state to CSI manager.
        /// </summary>
        /// <param name="isReady">Whether the game is ready for display.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void SendReadyState(bool isReady)
        {
            GameLifeCycleTracingEventSource.Log.SendReadyState(isReady);
        }

        /// <summary>
        /// Tracing event indicating a window Size request from the Foundation.
        /// </summary>
        /// <param name="windowId">The window id being resized.</param>
        /// <param name="show">Whether to show the window.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WindowSizeRequest(int windowId, bool show)
        {
            GameLifeCycleTracingEventSource.Log.WindowSizeRequest(windowId, show);
        }

        /// <summary>
        /// Tracing event indicating that the presentation rendering started.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void PresentationRenderingStarted()
        {
            GameLifeCycleTracingEventSource.Log.PresentationRenderingStarted();
        }

        /// <summary>
        /// Tracing event indicating a window Size response from the game.
        /// </summary>
        /// <param name="windowId">The window id being resized.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WindowSizeResponse(int windowId)
        {
            GameLifeCycleTracingEventSource.Log.WindowSizeResponse(windowId);
        }

        /// <summary>
        /// Tracing event indicating that the presentation's display control state is changed.
        /// </summary>
        /// <param name="displayControlState">The display control state.</param>
        ///<devdoc>
        /// Use string instead of enum to avoid the dependency on other components.
        /// </devdoc>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void PresentationDisplayControlState(string displayControlState)
        {
            GameLifeCycleTracingEventSource.Log.PresentationDisplayControlState(displayControlState);
        }

        /// <summary>
        /// Tracing event indicating that the theme context is inactivated.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void InactivateThemeContext()
        {
            GameLifeCycleTracingEventSource.Log.InactivateThemeContext();
        }

        /// <summary>
        /// Tracing event indicating that the state machine ends execution.
        /// </summary>
        /// <param name="message">The message describing why the execution ended.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateMachineEnded(string message)
        {
            GameLifeCycleTracingEventSource.Log.StateMachineEnded(message);
        }

        /// <summary>
        /// Tracing event indicating that Foundation tells the game to park.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ParkReceived()
        {
            GameLifeCycleTracingEventSource.Log.ParkReceived();
        }

        /// <summary>
        /// Tracing event indicating that the presentation processes game transition of park.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ParkProcessed()
        {
            GameLifeCycleTracingEventSource.Log.ParkProcessed();
        }

        /// <summary>
        /// Tracing event indicating that Unity engine responds to Park by suspending the Update loop etc.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameApplicationPark()
        {
            GameLifeCycleTracingEventSource.Log.GameApplicationPark();
        }

        /// <summary>
        /// Tracing event indicating that Unity engine responds to Unpark by resuming the Update loop etc.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameApplicationUnpark()
        {
            GameLifeCycleTracingEventSource.Log.GameApplicationUnpark();
        }

        /// <summary>
        /// Tracing event indicating that the presentation processes game transition of unpark.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void UnparkProcessed()
        {
            GameLifeCycleTracingEventSource.Log.UnparkProcessed();
        }

        /// <summary>
        /// Tracing event indicating that Foundation tells the game to show down.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ShutDownReceived()
        {
            GameLifeCycleTracingEventSource.Log.ShutDownReceived();
        }

        /// <summary>
        /// Tracing event indicating that the game logic thread is terminated.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameLogicThreadTerminated()
        {
            GameLifeCycleTracingEventSource.Log.GameLogicThreadTerminated();
        }

        /// <summary>
        /// Tracing event indicating that the game application is to quit.
        /// </summary>
        /// <param name="exitCode">The exit code. 0 means normal exit.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ApplicationQuit(int exitCode)
        {
            GameLifeCycleTracingEventSource.Log.ApplicationQuit(exitCode);
        }

        /// <summary>
        /// Tracing event indicating that the presentation processes game transition of abort.
        /// </summary>
        /// <param name="reason">The reason why the game aborts.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void AbortProcessed(string reason)
        {
            GameLifeCycleTracingEventSource.Log.AbortProcessed(reason);
        }

        /// <summary>
        /// Tracing event indicating that the application quit is complete.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ApplicationQuitComplete()
        {
            GameLifeCycleTracingEventSource.Log.ApplicationQuitComplete();
        }

        #endregion
    }
}