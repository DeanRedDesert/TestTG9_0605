//-----------------------------------------------------------------------
// <copyright file = "WorkerThread.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System;
    using System.Threading;

    /// <inheritdoc/>
    /// <summary>
    /// This class wraps a thread and controls the starting and stopping of the thread.
    /// </summary>
    public class WorkerThread : IDisposable
    {
        #region Constants

        /// <summary>
        /// The number of milliseconds to wait for the worker thread to terminate.
        /// </summary>
        private const int WorkerThreadTimeout = 250;

        #endregion

        #region Private Members

        /// <summary>
        /// The object that will do the work on the thread.
        /// </summary>
        private readonly IThreadWorker threadWorker;

        /// <summary>
        /// The exception monitor to report the exception thrown from the worker thread.
        /// </summary>
        private readonly GenericExceptionMonitor exceptionMonitor = new GenericExceptionMonitor();

        /// <summary>
        /// The thread instance running the worker function.
        /// </summary>
        private readonly Thread workerThread;

        /// <summary>
        /// The event set to let the worker know the thread is stopping.
        /// </summary>
        private readonly ManualResetEvent stoppingEvent = new ManualResetEvent(false);

        /// <summary>
        /// The flag indicating whether this instance has been disposed.
        /// </summary>
        private bool disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Create the thread.
        /// </summary>
        /// <param name="threadWorker">
        /// The worker that will do the work on the thread.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="threadWorker"/> is null.
        /// </exception>
        public WorkerThread(IThreadWorker threadWorker)
        {
            this.threadWorker = threadWorker ?? throw new ArgumentNullException(nameof(threadWorker));

            workerThread = new Thread(Run) { Name = "Worker Thread: " + threadWorker.ThreadName };
        }

        #endregion

        #region Thread Run 

        /// <summary>
        /// The run function for the thread.  It calls the worker to do the work
        /// and will run until the worker function returns.
        /// </summary>
        private void Run()
        {
            try
            {
                threadWorker.DoWork(stoppingEvent);
            }
            catch(ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch(Exception exception)
            {
                exceptionMonitor.ThrowException(new WorkerThreadException(threadWorker.ThreadName, exception));
            }
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Gets the exception monitor which reports the exception thrown on the worker thread.
        /// </summary>
        /// <remarks>
        /// The caller can wait on <see cref="IExceptionMonitor.ExceptionSignalHandle"/> to be notified when an exception is thrown,
        /// and call <see cref="IExceptionMonitor.CheckException"/> to retrieve the exception.
        /// </remarks>
        public IExceptionMonitor ExceptionMonitor => exceptionMonitor;

        /// <summary>
        /// Gets the flag indicating whether the worker thread is alive.
        /// </summary>
        public bool IsAlive => workerThread.IsAlive;

        /// <summary>
        /// Gets the name of the worker thread.
        /// </summary>
        public string Name => workerThread.Name;

        /// <summary>
        /// Starts the thread.
        /// </summary>
        /// <remarks>
        /// It is an error to start a thread that has terminated.
        /// </remarks>
        /// <returns>
        /// True if the thread is started successfully; False if the thread is already running.
        /// </returns>
        public bool Start()
        {
            var result = false;

            if(!workerThread.IsAlive)
            {
                stoppingEvent.Reset();
                workerThread.Start();

                result = true;
            }

            return result;
        }

        /// <summary>
        /// Signals the thread to stop, forces it to stop if it doesn't.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait before forcibly terminating the worker thread.
        /// This parameter is optional.  If not specified, it is default to <see cref="WorkerThreadTimeout"/>.
        /// </param>
        /// <returns>
        /// True if the thread stopped as told; False if the thread was forcibly terminated, or the thread was not running.
        /// </returns>
        public bool Stop(int millisecondsTimeout = WorkerThreadTimeout)
        {
            var result = false;

            // The thread should be monitoring this wait handle so setting it should cause the thread to exit.
            stoppingEvent.Set();

            // Wait for it to close on its own.
            if(workerThread.IsAlive)
            {
                result = workerThread.Join(millisecondsTimeout);

                if(!result)
                {
                    // Try and abort it.
                    workerThread.Abort();
                }
            }

            return result;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        /// <param name="disposing">True if called from dispose function.</param>
        private void Dispose(bool disposing)
        {
            if(disposed)
            {
                return;
            }

            if(disposing)
            {
                Stop();

                (stoppingEvent as IDisposable).Dispose();
                exceptionMonitor.Dispose();
            }

            disposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}