// -----------------------------------------------------------------------
// <copyright file = "WorkerThreadException.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System;

    /// <summary>
    /// This exception indicates that an exception has been thrown by a worker thread.
    /// </summary>
    public class WorkerThreadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="WorkerThreadException"/>
        /// with the thread name and an inner exception.
        /// </summary>
        /// <param name="threadName">The name of the thread that threw the exception.</param>
        /// <param name="innerException">The exception that has been thrown.</param>
        public WorkerThreadException(string threadName, Exception innerException)
            : base("An exception has been thrown by Worker Thread: " + threadName, innerException)
        {
        }
    }
}