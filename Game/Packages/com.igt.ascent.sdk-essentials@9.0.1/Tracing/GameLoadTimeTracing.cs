// -----------------------------------------------------------------------
// <copyright file = "GameLoadTimeTracing.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing
{
    using System.Diagnostics;
    using EventDefinitions;

    /// <summary>
    /// This class provides APIs for tracing the game load time.
    /// </summary>
    public sealed class GameLoadTimeTracing
    {
        #region Singleton Implementation

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameLoadTimeTracing Log { get; } = new GameLoadTimeTracing();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GameLoadTimeTracing()
        {
        }

        #endregion

        #region Tracing Methods

        /// <summary>
        /// Tracing event indicating that the game application is loaded and run.
        /// </summary>
        /// <param name="delayMilliseconds">
        /// The time in milliseconds since the application started till this method is called.
        /// </param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameApplicationStarted(int delayMilliseconds)
        {
            GameLoadTimeTracingEventSource.Log.GameApplicationStarted(delayMilliseconds);
        }

        /// <summary>
        /// Tracing event indicating that the game is ready for play.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameReadyForPlay()
        {
            GameLoadTimeTracingEventSource.Log.GameReadyForPlay();
        }

        /// <summary>
        /// Tracing event indicating that one scene is loaded.
        /// </summary>
        /// <param name="sceneName">Scene name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void SceneLoaded(string sceneName)
        {
            GameLoadTimeTracingEventSource.Log.SceneLoaded(sceneName);
        }

        /// <summary>
        /// Tracing event indicating that game lib connection starts.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameLibConnectionStart()
        {
            GameLoadTimeTracingEventSource.Log.GameLibConnectionStart();
        }

        /// <summary>
        /// Tracing event indicating that game is trying to connect to foundation.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ConnectToFoundation()
        {
            GameLoadTimeTracingEventSource.Log.ConnectToFoundation();
        }

        /// <summary>
        /// Tracing event indicating that cabinet connection starts.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void CabinetConnectionStart()
        {
            GameLoadTimeTracingEventSource.Log.CabinetConnectionStart();
        }

        /// <summary>
        /// Tracing event indicating that game is creating the cabinet lib.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void CreateCabinetLib()
        {
            GameLoadTimeTracingEventSource.Log.CreateCabinetLib();
        }

        /// <summary>
        /// Tracing event indicating that game is creating the cabinet.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void CreateCabinet()
        {
            GameLoadTimeTracingEventSource.Log.CreateCabinet();
        }

        /// <summary>
        /// Tracing event indicating that cabinet async invocation starts.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void CabinetAsyncStart()
        {
            GameLoadTimeTracingEventSource.Log.CabinetAsyncStart();
        }

        /// <summary>
        /// Tracing event indicating that game logic thread starts.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void GameLogicStart()
        {
            GameLoadTimeTracingEventSource.Log.GameLogicStart();
        }

        /// <summary>
        /// Tracing event indicating that game logic is requesting to start the first presentation state (idle state).
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void LogicStartPresentation()
        {
            GameLoadTimeTracingEventSource.Log.LogicStartPresentation();
        }

        /// <summary>
        /// Tracing event indicating that refresh state handlers starts.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void RefreshStateHandlersStart()
        {
            GameLoadTimeTracingEventSource.Log.RefreshStateHandlersStart();
        }

        /// <summary>
        /// Tracing event indicating that refresh state handlers completes.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void RefreshStateHandlersComplete()
        {
            GameLoadTimeTracingEventSource.Log.RefreshStateHandlersComplete();
        }

        #endregion
    }
}