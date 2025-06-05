// -----------------------------------------------------------------------
// <copyright file = "IThreadHealthChecker.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System;

    /// <summary>
    /// An object that can check the health of a collection of worker threads.
    /// </summary>
    public interface IThreadHealthChecker
    {
        /// <summary>
        /// Add a worker thread for health checks.
        /// </summary>
        /// <remarks>
        /// Since <see cref="CheckHealth"/> checks if the thread is alive, it is recommended to
        /// add only live threads for checking, i.e. adding the thread after having started it.
        /// </remarks>
        /// <param name="workerThread">The worker thread to check health for.</param>
        void AddCheck(WorkerThread workerThread);

        /// <summary>
        /// Removes a worker thread from the health checks.
        /// </summary>
        /// <remarks>
        /// Since <see cref="CheckHealth"/> checks if the thread is alive, it is recommended to
        /// first remove the thread from the heath checks before stopping the thread.
        /// </remarks>
        /// <param name="workerThread">The worker thread to remove health check for.</param>
        void RemoveCheck(WorkerThread workerThread);

        /// <summary>
        /// Checks if all threads being monitored are still alive.
        /// Returns an exception if any thread has stopped running.
        /// </summary>
        /// <returns>
        /// Null if all threads being checked are still alive.
        /// Otherwise, an exception which may contain multiple inner exceptions thrown by the threads.
        /// </returns>
        AggregateException CheckHealth();
    }
}