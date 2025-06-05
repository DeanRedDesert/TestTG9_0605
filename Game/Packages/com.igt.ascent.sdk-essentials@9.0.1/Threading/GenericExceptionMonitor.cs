//-----------------------------------------------------------------------
// <copyright file = "GenericExceptionMonitor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// This class provides a basic implementation of <see cref="IExceptionMonitor"/>.
    /// and the capability of waking up the blocking thread with a specific exception.
    /// </summary>
    public class GenericExceptionMonitor : IExceptionMonitor, IDisposable
    {
        /// <summary>
        /// The wait handle to be signaled when an exception is thrown.
        /// </summary>
        private readonly ManualResetEvent exceptionSignalHandle = new ManualResetEvent(false);

        /// <summary>
        /// The exception thrown.
        /// </summary>
        private Exception pendingException;

        /// <summary>
        /// The flag indicating whether this instance has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Unblock the waiting thread that uses this exception monitor,
        /// and provide it with an exception to throw after waking up.
        /// </summary>
        /// <param name="exception">The exception to throw after the thread wakes up.</param>
        public void ThrowException(Exception exception)
        {
            pendingException = exception;
            exceptionSignalHandle.Set();
        }

        #region IExceptionMonitor Members

        /// <inheritdoc />
        public WaitHandle ExceptionSignalHandle => exceptionSignalHandle;

        /// <inheritdoc />
        public Exception CheckException()
        {
            return pendingException;
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
                (exceptionSignalHandle as IDisposable).Dispose();
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
