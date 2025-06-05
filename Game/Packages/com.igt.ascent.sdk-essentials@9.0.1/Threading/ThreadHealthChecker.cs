// -----------------------------------------------------------------------
// <copyright file = "ThreadHealthChecker.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    /// <summary>
    /// A simple implementation of <see cref="IThreadHealthChecker"/> interface.
    /// </summary>
    public sealed class ThreadHealthChecker : IThreadHealthChecker
    {
        #region Private Fields

        private readonly ConcurrentDictionary<string, WorkerThread> workerThreads = new ConcurrentDictionary<string, WorkerThread>();

        #endregion

        #region Implementation of IThreadHealthChecker

        /// <inheritdoc />
        public void AddCheck(WorkerThread workerThread)
        {
            if(workerThread != null && !workerThreads.TryAdd(workerThread.Name, workerThread))
            {
                throw new InvalidOperationException($"A thread with the name of \"{workerThread.Name}\" is already in the list.");
            }
        }

        /// <inheritdoc />
        public void RemoveCheck(WorkerThread workerThread)
        {
            if(workerThread != null)
            {
                workerThreads.TryRemove(workerThread.Name, out _);
            }
        }

        /// <inheritdoc />
        public AggregateException CheckHealth()
        {
            AggregateException result = null;

            if(workerThreads.Any(entry => !entry.Value.IsAlive))
            {
                var exceptions = workerThreads.Values
                                              .Select(workerThread => workerThread.ExceptionMonitor.CheckException())
                                              .Where(ex => ex != null);

                result = new AggregateException("One or more threads have stopped running.", exceptions);
            }

            return result;
        }

        #endregion
    }
}